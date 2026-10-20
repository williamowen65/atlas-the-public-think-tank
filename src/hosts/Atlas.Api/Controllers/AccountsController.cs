using System.ComponentModel.DataAnnotations;
using Atlas.Identity;
using Atlas.Contracts.Observability;
using Atlas.Persistence.Identity;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Api.Controllers;

/// <summary>HTTP account/session adapter over Identity; cookies and confirmation delivery belong to this host.</summary>
// Explicit controller policy is visible to CodeQL and reviewers. GET/HEAD/OPTIONS
// remain readable; writes require the cookie/header token pair.
[AutoValidateAntiforgeryToken]
[ApiController, Route("api/auth"), Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("auth")]
public sealed class AccountsController(
    AtlasAccounts accounts,
    UserManager<AtlasIdentityUser> users,
    SignInManager<AtlasIdentityUser> signIn,
    IAntiforgery antiforgery,
    DevelopmentAccountMail mail
) : ControllerBase
{
    /// <summary>Get a CSRF token before writes, and get a fresh one after login/logout.</summary>
    [HttpGet("csrf")]
    public object Csrf() => new
    {
        token = antiforgery.GetAndStoreTokens(HttpContext).RequestToken
    };

    /// <summary>Validate credentials through Identity and issue its session cookie on success.</summary>
    // lockoutOnFailure is true; AtlasSignInManager records bounded sign-in/lockout outcomes.
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        // Let Identity validate credentials, lockout and Atlas profile eligibility.
        var result = await signIn.PasswordSignInAsync(
            request.Email,
            request.Password,
            false,
            true
        );
        // Do not disclose whether an email exists, is unconfirmed, disabled or locked.
        if (!result.Succeeded)
            return Problem(
                statusCode: 401,
                title: "Sign-in was not completed."
            );
        return NoContent();
    }

    /// <summary>Clear the session cookie, then record sign-out in the active request telemetry scope.</summary>
    [Authorize, HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await signIn.SignOutAsync();
        // Record a bounded outcome, not a cookie, password or serialized account.
        Telemetry.Record(Telemetry.Create(EventKind.SignOut));
        return NoContent();
    }

    /// <summary>Return the current active participant identity without exposing account/security fields.</summary>
    [Authorize, HttpGet("session")]
    public async Task<object> Session() => new
    {
        participantId = (await accounts.ResolveParticipantAsync(User))?.Value
    };

    /// <summary>Create the account/profile together, then write its private confirmation payload.</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterRequest request,
        CancellationToken cancellationToken
    )
    {
        // No production mail provider has been selected. Fail before creating an
        // unusable account; never expose confirmation/reset tokens in JSON or logs.
        if (!mail.IsAvailable)
            return Problem(
                statusCode: 503,
                title: "Account email delivery is not configured."
            );

        // Commit the linked account, Participant profile and Member role together.
        var registration = await accounts.RegisterAsync(
            request.Email,
            request.Password,
            request.DisplayName,
            cancellationToken
        );
        if (!registration.Result.Succeeded)
            return Problem(
                statusCode: 400,
                title: "Registration could not be completed."
            );

        // Generate confirmation evidence only after registration succeeds.
        var user = (await users.FindByIdAsync(registration.ParticipantId!.Value.ToString()))!;
        var token = await users.GenerateEmailConfirmationTokenAsync(user);
        await mail.WriteAsync(
            "confirm-email",
            user.Id,
            token,
            cancellationToken
        );

        // Delivery stays private; the response contains no confirmation or reset token.
        return Accepted();
    }

    /// <summary>Close this account, retain contributions under a deleted-user placeholder, and clear this session.</summary>
    [Authorize, HttpPost("delete-account")]
    public async Task<IActionResult> DeleteAccount(
        DeleteAccountRequest request,
        CancellationToken cancellationToken
    )
    {
        // Resolve this session’s active participant; do not accept an account ID from JSON.
        var participant = await accounts.ResolveParticipantAsync(User, cancellationToken);
        if (participant is null)
            return Unauthorized();

        // Verify the password and close the account while retaining its contributions.
        var result = await accounts.DeleteAccountAsync(
            participant.Value,
            request.Password,
            cancellationToken
        );
        if (!result.Succeeded)
            return Problem(
                statusCode: result.Errors.Any(e => e.Code == "CommunityOwnership") ? 409 : 400,
                title: result.Errors.Any(e => e.Code == "CommunityOwnership")
                    ? "Transfer community ownership before closing the account."
                    : "Account closure could not be completed."
            );

        // Clear this browser session only after closure commits.
        await signIn.SignOutAsync();
        return NoContent();
    }

    /// <summary>Generic response avoids disclosing whether a confirmed active account exists.</summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        EmailRequest request,
        CancellationToken cancellationToken
    )
    {
        if (!mail.IsAvailable)
            return Problem(
                statusCode: 503,
                title: "Account email delivery is not configured."
            );

        // Look up eligibility without revealing account existence in the HTTP response.
        var user = await users.FindByEmailAsync(request.Email);
        if (user is not null && await users.IsEmailConfirmedAsync(user)
            && await accounts.HasActiveProfileAsync(user.Id, cancellationToken))
            await mail.WriteAsync(
                "reset-password",
                user.Id,
                await users.GeneratePasswordResetTokenAsync(user),
                cancellationToken
            );

        // Delivery stays private; the response contains no confirmation or reset token.
        return Accepted();
    }

    /// <summary>Identity verifies purpose, expiry and password policy, and updates the security stamp.</summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordRequest request,
        CancellationToken cancellationToken
    )
    {

        // Look up eligibility without revealing account existence in the HTTP response.
        var user = await users.FindByEmailAsync(request.Email);
        if (user is null || !await users.IsEmailConfirmedAsync(user)
            || !await accounts.HasActiveProfileAsync(user.Id, cancellationToken)
            || !(await users.ResetPasswordAsync(user, request.Token, request.NewPassword)).Succeeded)
            return Problem(
                statusCode: 400,
                title: "Password reset could not be completed."
            );
        // Do not automatically sign in or clear lockouts; existing session revalidation remains Identity-owned.
        return NoContent();
    }

    /// <summary>Deliver confirmation only for an active unconfirmed account; respond generically.</summary>
    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmation(
        EmailRequest request,
        CancellationToken cancellationToken
    )
    {
        if (!mail.IsAvailable)
            return Problem(
                statusCode: 503,
                title: "Account email delivery is not configured."
            );

        // Look up eligibility without revealing account existence in the HTTP response.
        var user = await users.FindByEmailAsync(request.Email);
        if (user is not null && !await users.IsEmailConfirmedAsync(user)
            && await accounts.HasActiveProfileAsync(user.Id, cancellationToken))
            await mail.WriteAsync(
                "confirm-email",
                user.Id,
                await users.GenerateEmailConfirmationTokenAsync(user),
                cancellationToken
            );

        // Delivery stays private; the response contains no confirmation or reset token.
        return Accepted();
    }

    /// <summary>Validate the account’s confirmation token through Identity without signing in.</summary>
    [HttpPost("confirm-email")]
    public async Task<IActionResult> Confirm(ConfirmationRequest request)
    {
        var user = await users.FindByIdAsync(request.UserId.ToString());
        if (user is null || !(await users.ConfirmEmailAsync(user, request.Token)).Succeeded)
            return Problem(
                statusCode: 400,
                title: "Confirmation could not be completed."
            );
        return NoContent();
    }
}

public sealed record LoginRequest(
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MaxLength(1024)] string Password
);

public sealed record RegisterRequest(
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MaxLength(1024)] string Password,
    [Required, MaxLength(100)] string DisplayName
);

public sealed record ConfirmationRequest(
    Guid UserId,
    [Required, MaxLength(4096)] string Token
);


public sealed record EmailRequest([Required, EmailAddress, MaxLength(256)] string Email);

public sealed record ResetPasswordRequest(
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MaxLength(4096)] string Token,
    [Required, MaxLength(1024)] string NewPassword
);


public sealed record DeleteAccountRequest([Required, MaxLength(1024)] string Password);

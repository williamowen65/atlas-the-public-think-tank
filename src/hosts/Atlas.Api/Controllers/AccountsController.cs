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
public sealed class AccountsController(AtlasAccounts accounts, UserManager<AtlasIdentityUser> users,
    SignInManager<AtlasIdentityUser> signIn, IAntiforgery antiforgery) : ControllerBase
{
    /// <summary>Get a CSRF token before writes, and get a fresh one after login/logout.</summary>
    [HttpGet("csrf")]
    public object Csrf() => new { token = antiforgery.GetAndStoreTokens(HttpContext).RequestToken };

    /// <summary>Validate credentials through Identity and issue its session cookie on success.</summary>
    // lockoutOnFailure is true; AtlasSignInManager records bounded sign-in/lockout outcomes.
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await signIn.PasswordSignInAsync(request.Email, request.Password, false, true);
        // Do not disclose whether an email exists, is unconfirmed, disabled or locked.
        if (!result.Succeeded) return Problem(statusCode: 401, title: "Sign-in was not completed.");
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
    public async Task<object> Session() => new { participantId = (await accounts.ResolveParticipantAsync(User))?.Value };

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, [FromServices] IWebHostEnvironment environment,
        [FromServices] IConfiguration configuration, CancellationToken cancellationToken)
    {
        // No production mail provider has been selected. Fail before creating an
        // unusable account; never expose confirmation/reset tokens in JSON or logs.
        if (!environment.IsDevelopment())
            return Problem(statusCode: 503, title: "Account email delivery is not configured.");
        // Resolve against the host content root, not the shell's working directory.
        // No user secret is needed for local development; an override remains optional.
        var configuredDirectory = configuration["Atlas:DevelopmentMailDirectory"];
        var directory = Path.GetFullPath(string.IsNullOrWhiteSpace(configuredDirectory)
            ? "development-mail" : configuredDirectory, environment.ContentRootPath);
        var registration = await accounts.RegisterAsync(request.Email, request.Password, request.DisplayName, cancellationToken);
        if (!registration.Result.Succeeded) return Problem(statusCode: 400, title: "Registration could not be completed.");
        var user = (await users.FindByIdAsync(registration.ParticipantId!.Value.ToString()))!;
        var token = await users.GenerateEmailConfirmationTokenAsync(user);
        Directory.CreateDirectory(directory);
        await System.IO.File.WriteAllTextAsync(Path.Combine(directory, Guid.NewGuid()+".json"),
            System.Text.Json.JsonSerializer.Serialize(new { userId = user.Id, token }), cancellationToken);
        return Accepted();
    }
    [HttpPost("confirm-email")]
    public async Task<IActionResult> Confirm(ConfirmationRequest request)
    {
        var user = await users.FindByIdAsync(request.UserId.ToString());
        if (user is null || !(await users.ConfirmEmailAsync(user, request.Token)).Succeeded)
            return Problem(statusCode: 400, title: "Confirmation could not be completed.");
        return NoContent();
    }
}
public sealed record LoginRequest([Required, EmailAddress, MaxLength(256)] string Email, [Required, MaxLength(1024)] string Password);
public sealed record RegisterRequest([Required, EmailAddress, MaxLength(256)] string Email, [Required, MaxLength(1024)] string Password,
    [Required, MaxLength(100)] string DisplayName);
public sealed record ConfirmationRequest(Guid UserId, [Required, MaxLength(4096)] string Token);

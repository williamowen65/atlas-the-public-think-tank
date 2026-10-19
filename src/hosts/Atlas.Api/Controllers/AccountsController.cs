using System.ComponentModel.DataAnnotations;
using Atlas.Identity;
using Atlas.Persistence.Identity;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Api.Controllers;

[ApiController, Route("api/auth"), Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("auth")]
public sealed class AccountsController(AtlasAccounts accounts, UserManager<AtlasIdentityUser> users,
    SignInManager<AtlasIdentityUser> signIn, IAntiforgery antiforgery) : ControllerBase
{
    /// <summary>Get a CSRF token before writes, and get a fresh one after login/logout.</summary>
    [HttpGet("csrf")]
    public object Csrf() => new { token = antiforgery.GetAndStoreTokens(HttpContext).RequestToken };

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await signIn.PasswordSignInAsync(request.Email, request.Password, false, true);
        // Do not disclose whether an email exists, is unconfirmed, disabled or locked.
        if (!result.Succeeded) return Problem(statusCode: 401, title: "Sign-in was not completed.");
        return NoContent();
    }
    [Authorize, HttpPost("logout")]
    public async Task<IActionResult> Logout() { await signIn.SignOutAsync(); return NoContent(); }

    [Authorize, HttpGet("session")]
    public async Task<object> Session() => new { participantId = (await accounts.ResolveParticipantAsync(User))?.Value };

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, [FromServices] IWebHostEnvironment environment,
        [FromServices] IConfiguration configuration, CancellationToken cancellationToken)
    {
        // No production mail provider has been selected. Fail before creating an
        // unusable account; never expose confirmation/reset tokens in JSON or logs.
        var directory = configuration["Atlas:DevelopmentMailDirectory"];
        if (!environment.IsDevelopment() || string.IsNullOrWhiteSpace(directory))
            return Problem(statusCode: 503, title: "Account email delivery is not configured.");
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

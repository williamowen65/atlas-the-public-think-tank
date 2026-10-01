using System.Security.Claims;
using Atlas.Identity;
using Atlas.Moderation;
using Atlas.Participants.Participants;
using Atlas.Persistence;
using Atlas.Persistence.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.ConsoleApp.Identity;

/// <summary>
/// Acts as the console application's session manager and security guard.
/// A web application normally keeps an authenticated user signed in with an authentication cookie. The console
/// has no browser or cookie, so this class keeps the current ClaimsPrincipal, security stamp, and Participant
/// in memory and repeatedly checks SQL/Identity to make sure that session is still allowed to continue.
/// </summary>
public sealed class ConsoleIdentitySession(IServiceScopeFactory scopes) : IModeratorAuthorization
{
    private ClaimsPrincipal? _principal;
    private string? _securityStamp;
    public Participant? Participant { get; private set; }

    /// <summary>
    /// Attempts to start a console session from an email and password.
    /// Identity verifies the account/password and applies lockout rules. The console deliberately rejects accounts
    /// requiring 2FA because it has no second-factor UI; 2FA belongs to the future REST API/web login flow.
    /// A successful password check is not enough by itself: the account must also satisfy the Atlas Member policy.
    /// </summary>
    public async Task<SignInResult> SignInAsync(string email, string password)
    {
        // Always discard any previous in-memory session before attempting a new login.
        SignOut();
        using var scope = scopes.CreateScope();
        var services = scope.ServiceProvider;
        var users = services.GetRequiredService<UserManager<AtlasIdentityUser>>();
        var user = await users.FindByEmailAsync(email.Trim());
        if (user is null) return SignInResult.Failed;
        // Check before the framework password flow: its 2FA remembered-client check requires an HTTP context.
        // The console has no second-factor flow and never grants these accounts a session.
        if (await users.GetTwoFactorEnabledAsync(user)) return SignInResult.TwoFactorRequired;
        var signIn = services.GetRequiredService<SignInManager<AtlasIdentityUser>>();
        var result = await signIn.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        if (!result.Succeeded) return result;
        // Build the ClaimsPrincipal that represents this authenticated Identity user. It contains the claims/roles
        // that Atlas authorization policies inspect, similar to the User principal a web request would normally have.
        var principal = await services.GetRequiredService<IUserClaimsPrincipalFactory<AtlasIdentityUser>>()
            .CreateAsync(user);
        if (!(await services.GetRequiredService<IAuthorizationService>()
                .AuthorizeAsync(principal, null, AtlasPolicies.Member)).Succeeded) return SignInResult.NotAllowed;
        _principal = principal;
        // Remember Identity's current security stamp. Identity changes this value when security-sensitive account
        // state changes (for example, a password/security update), allowing RefreshAsync to invalidate this session.
        _securityStamp = await users.GetSecurityStampAsync(user);
        return await RefreshAsync() ? SignInResult.Success : SignInResult.NotAllowed;
    }

    /// <summary>
    /// Revalidates the in-memory console session against current database state.
    /// This is the console equivalent of rechecking an authentication session: it makes sure the Identity user still
    /// exists, the security stamp has not changed, and the user still satisfies the Member policy. If any check fails,
    /// the session is immediately cleared. It then reloads the linked Participant from SQL so the console uses current
    /// Atlas profile data rather than a stale copy kept from login.
    /// </summary>
    public async Task<bool> RefreshAsync()
    {
        if (_principal is null) return false;
        using var scope = scopes.CreateScope();
        var services = scope.ServiceProvider;
        var users = services.GetRequiredService<UserManager<AtlasIdentityUser>>();
        var user = await users.GetUserAsync(_principal);
        if (user is null || _securityStamp != await users.GetSecurityStampAsync(user) ||
            !(await services.GetRequiredService<IAuthorizationService>()
                .AuthorizeAsync(_principal, null, AtlasPolicies.Member)).Succeeded)
        {
            SignOut();
            return false;
        }
        // Identity answers "is this account still authenticated/authorized?"; Participant supplies the current
        // Atlas-domain profile (display name, bio, active state, etc.). The shared GUID links the two records.
        var row = await services.GetRequiredService<AtlasDataContext>().ParticipantRows.AsNoTracking()
            .SingleAsync(row => row.Id == user.Id);
        Participant = Atlas.Participants.Participants.Participant.Reconstitute(new ParticipantId(row.Id),
            row.DisplayName, row.Bio, row.IsActive, row.CreatedAt, row.UpdatedAt);
        return true;
    }

    /// <summary>
    /// Ends the console session by forgetting all locally held authentication/profile state.
    /// There is no browser authentication cookie to delete; clearing these values resets the console to signed out.
    /// </summary>
    public void SignOut()
    {
        _principal = null;
        _securityStamp = null;
        Participant = null;
    }

    /// <summary>
    /// Checks whether the requested Participant is the currently signed-in actor and has Atlas-wide moderation access.
    /// The session is refreshed first so a stale login cannot keep moderator privileges after access changes.
    /// The final decision is delegated to the Identity-backed moderator authorization service, which checks the
    /// persisted global role. This is global Atlas moderation, not a future community-specific moderator role.
    /// </summary>
    public bool IsAtlasModerator(Guid participantId)
    {
        if (Participant?.Id.Value != participantId || !RefreshAsync().GetAwaiter().GetResult()) return false;
        using var scope = scopes.CreateScope();
        return scope.ServiceProvider.GetRequiredService<IModeratorAuthorization>().IsAtlasModerator(participantId);
    }
}

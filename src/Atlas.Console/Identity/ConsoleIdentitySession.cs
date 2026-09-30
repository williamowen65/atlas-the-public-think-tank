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

/// <summary>Holds an authenticated console actor; each check uses fresh SQL state without browser cookies.</summary>
public sealed class ConsoleIdentitySession(IServiceScopeFactory scopes) : IModeratorAuthorization
{
    private ClaimsPrincipal? _principal;
    private string? _securityStamp;
    public Participant? Participant { get; private set; }

    public async Task<SignInResult> SignInAsync(string email, string password)
    {
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
        var principal = await services.GetRequiredService<IUserClaimsPrincipalFactory<AtlasIdentityUser>>()
            .CreateAsync(user);
        if (!(await services.GetRequiredService<IAuthorizationService>()
                .AuthorizeAsync(principal, null, AtlasPolicies.Member)).Succeeded) return SignInResult.NotAllowed;
        _principal = principal;
        _securityStamp = await users.GetSecurityStampAsync(user);
        return await RefreshAsync() ? SignInResult.Success : SignInResult.NotAllowed;
    }

    /// <summary>Rejects revoked credentials, lockout, inactive profiles, and removed access before each main menu.</summary>
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
        var row = await services.GetRequiredService<AtlasDataContext>().ParticipantRows.AsNoTracking()
            .SingleAsync(row => row.Id == user.Id);
        Participant = Atlas.Participants.Participants.Participant.Reconstitute(new ParticipantId(row.Id),
            row.DisplayName, row.Bio, row.IsActive, row.CreatedAt, row.UpdatedAt);
        return true;
    }

    public void SignOut()
    {
        _principal = null;
        _securityStamp = null;
        Participant = null;
    }

    public bool IsAtlasModerator(Guid participantId)
    {
        if (Participant?.Id.Value != participantId || !RefreshAsync().GetAwaiter().GetResult()) return false;
        using var scope = scopes.CreateScope();
        return scope.ServiceProvider.GetRequiredService<IModeratorAuthorization>().IsAtlasModerator(participantId);
    }
}

using Atlas.Persistence;
using Atlas.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Identity;

public sealed record GlobalModeratorGrantResult(bool Succeeded, string Message);

/// <summary>Trusted operator-only provisioning. Never expose through public account endpoints.</summary>
public static class GlobalModeratorGrant
{
    public static async Task<GlobalModeratorGrantResult> GrantAsync(IServiceScopeFactory scopes, string email)
    {
        using var scope = scopes.CreateScope();
        return await GrantAsync(scope.ServiceProvider, email);
    }

    // The cloud host supplies a scope with its already-open SQL connection. Keep
    // all reads and writes on that connection rather than probing a different one.
    public static async Task<GlobalModeratorGrantResult> GrantAsync(
        IServiceProvider services, string email, Action<string>? progress = null)
    {
        if (string.IsNullOrWhiteSpace(email)) return new(false, "An account email is required.");
        progress?.Invoke("resolving account services");
        var users = services.GetRequiredService<UserManager<AtlasIdentityUser>>();
        var database = services.GetRequiredService<AtlasDataContext>();
        progress?.Invoke("looking up the existing account");
        var user = await users.FindByEmailAsync(email.Trim());
        if (user is null) return new(false, "Account not found.");
        progress?.Invoke("checking email confirmation, lockout, and active profile");
        if (!await users.IsEmailConfirmedAsync(user) || await users.IsLockedOutAsync(user) ||
            !await database.ParticipantRows.AsNoTracking().AnyAsync(p => p.Id == user.Id && p.IsActive))
            return new(false, "Account must have a confirmed email, be unlocked, and have an active participant profile.");
        progress?.Invoke("checking existing moderator role");
        if (await users.IsInRoleAsync(user, AtlasRoles.GlobalModerator))
            return new(true, "Account already has the GlobalModerator role; no changes made.");
        progress?.Invoke("starting role transaction");
        await using var transaction = await database.Database.BeginTransactionAsync();
        progress?.Invoke("assigning global moderator role");
        var result = await users.AddToRoleAsync(user, AtlasRoles.GlobalModerator);
        if (!result.Succeeded)
            return new(false, "Role assignment failed: " + string.Join("; ", result.Errors.Select(x => x.Description)));
        // Force existing cookies/sessions to revalidate rather than retaining stale authentication state.
        progress?.Invoke("updating security stamp");
        var stamp = await users.UpdateSecurityStampAsync(user);
        if (!stamp.Succeeded)
            throw new InvalidOperationException("Security stamp update failed; role assignment rolled back.");
        progress?.Invoke("committing role transaction");
        await transaction.CommitAsync();
        return new(true, "GlobalModerator role granted. Sign out and sign in again to verify moderator access.");
    }

}

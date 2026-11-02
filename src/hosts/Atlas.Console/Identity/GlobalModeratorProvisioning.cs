using Atlas.Persistence;
using Atlas.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.ConsoleApp.Identity;

/// <summary>
/// Trusted terminal-only operator workflow. Never expose role assignment to registration or HTTP callers.
/// </summary>
public static class GlobalModeratorProvisioning
{
    public static async Task<string> GrantAsync(IServiceScopeFactory scopes, string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return "An account email is required.";
        using var scope = scopes.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AtlasIdentityUser>>();
        var database = scope.ServiceProvider.GetRequiredService<AtlasDataContext>();
        var user = await users.FindByEmailAsync(email.Trim());
        if (user is null) return "Account not found.";
        if (!await users.IsEmailConfirmedAsync(user) || await users.IsLockedOutAsync(user) ||
            !await database.ParticipantRows.AsNoTracking().AnyAsync(p => p.Id == user.Id && p.IsActive))
            return "Account must have a confirmed email, be unlocked, and have an active participant profile.";
        if (await users.IsInRoleAsync(user, AtlasRoles.GlobalModerator))
            return "Account already has the GlobalModerator role; no changes made.";
        var result = await users.AddToRoleAsync(user, AtlasRoles.GlobalModerator);
        if (!result.Succeeded)
            return "Role assignment failed: " + string.Join("; ", result.Errors.Select(x => x.Description));
        // Force existing cookies/sessions to revalidate rather than retaining stale authentication state.
        var stamp = await users.UpdateSecurityStampAsync(user);
        if (!stamp.Succeeded)
            throw new InvalidOperationException("Role assigned, but security stamp update failed; operator must investigate.");
        return "GlobalModerator role granted. Sign out and sign in again to verify moderator access.";
    }

    public static async Task RunAsync(IServiceScopeFactory scopes)
    {
        if (Console.IsInputRedirected)
        {
            Console.Error.WriteLine("Operator role provisioning requires an interactive terminal.");
            Environment.ExitCode = 1;
            return;
        }
        Console.WriteLine("TRUSTED OPERATOR — GRANT ATLAS GLOBAL MODERATOR");
        Console.WriteLine("This grants Atlas-wide moderation privileges. Use only on a trusted machine.");
        Console.Write("Existing account email: ");
        var email = Console.ReadLine()?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(email)) { Console.WriteLine("Cancelled."); return; }
        Console.Write("Type GRANT GLOBAL MODERATOR to confirm: ");
        if (Console.ReadLine() != "GRANT GLOBAL MODERATOR") { Console.WriteLine("Cancelled."); return; }
        Console.WriteLine(await GrantAsync(scopes, email));
    }
}

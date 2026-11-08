using Microsoft.Extensions.DependencyInjection;

namespace Atlas.ConsoleApp.Identity;

/// <summary>
/// Trusted terminal-only operator workflow. Never expose role assignment to registration or HTTP callers.
/// </summary>
public static class GlobalModeratorProvisioning
{
    public static async Task<string> GrantAsync(IServiceScopeFactory scopes, string email) =>
        (await Atlas.Identity.GlobalModeratorGrant.GrantAsync(scopes, email)).Message;

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

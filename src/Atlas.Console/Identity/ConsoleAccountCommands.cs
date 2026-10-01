using Atlas.Identity;
using Atlas.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.ConsoleApp.Identity;

/// <summary>Interactive account entry and local operator confirmation; passwords are never echoed.</summary>
internal static class ConsoleAccountCommands
{
    public static async Task<bool> SignInOrRegisterAsync(IServiceScopeFactory scopes, ConsoleIdentitySession session)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("ATLAS — ACCOUNT");
            Console.WriteLine("1. Sign in");
            Console.WriteLine("2. Register");
            Console.WriteLine("0. Exit");
            Console.Write("Selection: ");
            var selection = Console.ReadLine();
            if (selection is null or "0") return false;
            if (selection is not ("1" or "2")) continue;
            Console.Write("Email: ");
            var email = Console.ReadLine() ?? "";
            string displayName = "";
            if (selection == "2")
            {
                Console.Write("Display name: ");
                displayName = Console.ReadLine() ?? "";
                Console.WriteLine("Password: at least 12 characters, including uppercase, lowercase, digit, and symbol.");
            }
            var password = ReadPassword("Password: ");
            if (selection == "2")
            {
                if (password != ReadPassword("Confirm password: "))
                {
                    ConsoleUi.Pause("Passwords did not match.");
                    continue;
                }
                using var scope = scopes.CreateScope();
                var result = await scope.ServiceProvider.GetRequiredService<AtlasAccounts>()
                    .RegisterAsync(email, password, displayName);
                ConsoleUi.Pause(result.Result.Succeeded
                    ? "Account created. Email confirmation is required before sign-in. For local development, run the console with --confirm-email in another terminal."
                    : string.Join(Environment.NewLine, result.Result.Errors.Select(error => error.Description)));
            }
            else
            {
                var result = await session.SignInAsync(email, password);
                if (result.Succeeded) return true;
                ConsoleUi.Pause(result.IsLockedOut ? "Account is temporarily locked. Try again later."
                    : result.IsNotAllowed ? "Sign-in is unavailable. Confirm the email and check that the account/profile has access."
                    : result.RequiresTwoFactor ? "This account requires two-factor authentication, which the console does not support yet."
                    : "Unable to sign in with those credentials.");
            }
        }
    }

    /// <summary>Trusted local operator utility, not email ownership verification or a public confirmation endpoint.</summary>
    public static async Task ConfirmEmailAsync(IServiceScopeFactory scopes)
    {
        Console.WriteLine("LOCAL DEVELOPMENT — OPERATOR EMAIL CONFIRMATION");
        Console.Write("Account email: ");
        var email = Console.ReadLine() ?? "";
        using var scope = scopes.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AtlasIdentityUser>>();
        var user = await users.FindByEmailAsync(email.Trim());
        if (user is null) { Console.WriteLine("Account not found."); return; }
        if (user.EmailConfirmed) { Console.WriteLine("Email is already confirmed."); return; }
        Console.Write("Confirm this account as the local operator? Type CONFIRM: ");
        if (Console.ReadLine() != "CONFIRM") { Console.WriteLine("Cancelled."); return; }
        // LOCAL-DEVELOPMENT SHORTCUT: there is no email provider or in-memory confirmation list here.
        // ASP.NET Core Identity generates a real email-confirmation token for this persisted user, and
        // ConfirmEmailAsync validates that token and persists EmailConfirmed = true in the Identity user row.
        //
        // A future web/email integration should keep this Identity token + ConfirmEmailAsync mechanism.
        // The difference is that the application will generate the token, put it in a confirmation link,
        // and an email provider will deliver that link to the user's email address. The public confirmation
        // endpoint will then receive the token from the clicked link and pass it to ConfirmEmailAsync.
        // This console command deliberately skips the delivery/ownership-verification step and is therefore
        // only a trusted local operator utility.
        var token = await users.GenerateEmailConfirmationTokenAsync(user);
        var result = await users.ConfirmEmailAsync(user, token);
        Console.WriteLine(result.Succeeded ? "Email confirmed. You can now sign in."
            : string.Join(Environment.NewLine, result.Errors.Select(error => error.Description)));
    }

    private static string ReadPassword(string prompt)
    {
        if (Console.IsInputRedirected)
            throw new InvalidOperationException("Password entry requires an interactive terminal.");
        Console.Write(prompt);
        var password = new System.Text.StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter) { Console.WriteLine(); return password.ToString(); }
            if (key.Key == ConsoleKey.Backspace) { if (password.Length > 0) password.Length--; }
            else if (!char.IsControl(key.KeyChar)) password.Append(key.KeyChar);
        }
    }
}

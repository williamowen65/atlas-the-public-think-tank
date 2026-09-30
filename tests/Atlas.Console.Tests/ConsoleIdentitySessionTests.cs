using Atlas.ConsoleApp.Identity;
using Atlas.Identity;
using Atlas.Persistence;
using Atlas.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Console.Tests;

[TestClass]
public sealed class ConsoleIdentitySessionTests
{
    [TestMethod]
    public async Task Signed_out_session_has_no_actor_or_moderator_access()
    {
        using var services = new ServiceCollection().AddAtlasIdentity("Server=localhost;Database=Unused;Integrated Security=True")
            .BuildServiceProvider();
        var session = new ConsoleIdentitySession(services.GetRequiredService<IServiceScopeFactory>());
        Assert.IsFalse(await session.RefreshAsync());
        Assert.IsNull(session.Participant);
        Assert.IsFalse(session.IsAtlasModerator(Guid.NewGuid()));
    }

    [TestMethod]
    public async Task Sql_console_session_uses_confirmed_account_checks_roles_and_revokes_changed_credentials()
    {
        var template = Environment.GetEnvironmentVariable("ATLAS_SQL_TEST_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(template)) Assert.Inconclusive("Set ATLAS_SQL_TEST_CONNECTION_STRING for console sign-in tests.");
        var connection = new SqlConnectionStringBuilder(template) { InitialCatalog = $"AtlasConsoleIdentity_{Guid.NewGuid():N}" };
        using var services = new ServiceCollection().AddAtlasIdentity(connection.ConnectionString).BuildServiceProvider();
        var scopes = services.GetRequiredService<IServiceScopeFactory>();
        using var setup = scopes.CreateScope();
        var database = setup.ServiceProvider.GetRequiredService<AtlasDataContext>();
        try
        {
            await database.Database.MigrateAsync();
            const string password = "Console_94!TestPassword";
            var accounts = setup.ServiceProvider.GetRequiredService<AtlasAccounts>();
            var created = await accounts.RegisterAsync("console@example.test", password, "Console Member");
            Assert.IsTrue(created.Result.Succeeded);
            var users = setup.ServiceProvider.GetRequiredService<UserManager<AtlasIdentityUser>>();
            var user = (await users.FindByEmailAsync("console@example.test"))!;
            var session = new ConsoleIdentitySession(scopes);
            Assert.IsTrue((await session.SignInAsync(user.Email!, password)).IsNotAllowed);
            Assert.IsNull(session.Participant);
            var token = await users.GenerateEmailConfirmationTokenAsync(user);
            Assert.IsTrue((await users.ConfirmEmailAsync(user, token)).Succeeded);
            Assert.IsFalse((await session.SignInAsync(user.Email!, "wrong")).Succeeded);
            Assert.IsTrue((await session.SignInAsync(user.Email!, password)).Succeeded);
            Assert.AreEqual(created.ParticipantId, session.Participant!.Id);
            Assert.IsFalse(session.IsAtlasModerator(user.Id));
            database.ChangeTracker.Clear();
            user = (await users.FindByEmailAsync("console@example.test"))!;
            Assert.IsTrue((await users.AddToRoleAsync(user, AtlasRoles.Moderator)).Succeeded);
            Assert.IsTrue(session.IsAtlasModerator(user.Id));
            Assert.IsFalse(session.IsAtlasModerator(Guid.NewGuid()));
            Assert.IsTrue((await users.RemoveFromRoleAsync(user, AtlasRoles.Moderator)).Succeeded);
            Assert.IsFalse(session.IsAtlasModerator(user.Id));
            Assert.IsTrue((await users.ChangePasswordAsync(user, password, password + "New")).Succeeded);
            Assert.IsFalse(await session.RefreshAsync());
            Assert.IsNull(session.Participant);
            Assert.IsTrue((await session.SignInAsync(user.Email!, password + "New")).Succeeded);
            session.SignOut();
            Assert.IsNull(session.Participant);
            Assert.IsFalse(await session.RefreshAsync());
            Assert.IsTrue((await users.SetTwoFactorEnabledAsync(user, true)).Succeeded);
            Assert.IsTrue((await session.SignInAsync(user.Email!, password + "New")).RequiresTwoFactor);
            Assert.IsNull(session.Participant);
        }
        finally { await database.Database.EnsureDeletedAsync(); }
    }
}

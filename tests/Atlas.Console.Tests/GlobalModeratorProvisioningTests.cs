using Atlas.ConsoleApp.Identity;
using Atlas.Identity;
using Atlas.Persistence;
using Atlas.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Console.Tests;

[TestClass]
public sealed class GlobalModeratorProvisioningTests
{
    [TestMethod]
    public async Task Sql_operator_grant_requires_confirmed_active_account_and_is_idempotent()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<GlobalModeratorProvisioningTests>(optional: true)
            .AddEnvironmentVariables().Build();
        var template = configuration["ATLAS_SQL_TEST_CONNECTION_STRING"];
        if (string.IsNullOrWhiteSpace(template))
            Assert.Inconclusive("Set ATLAS_SQL_TEST_CONNECTION_STRING to run SQL provisioning tests.");
        var connection = new SqlConnectionStringBuilder(template) {
            InitialCatalog = $"AtlasModeratorProvisioning_{Guid.NewGuid():N}"
        };
        using var services = new ServiceCollection().AddAtlasIdentity(connection.ConnectionString)
            .BuildServiceProvider();
        var scopes = services.GetRequiredService<IServiceScopeFactory>();
        using var setup = scopes.CreateScope();
        var database = setup.ServiceProvider.GetRequiredService<AtlasDataContext>();
        try
        {
            await database.Database.MigrateAsync();
            var accounts = setup.ServiceProvider.GetRequiredService<AtlasAccounts>();
            var registration = await accounts.RegisterAsync(
                "moderator@example.test", "Moderator_159!TestPassword", "Provisioning Test");
            Assert.IsTrue(registration.Result.Succeeded);
            Assert.AreEqual("Account not found.",
                await GlobalModeratorProvisioning.GrantAsync(scopes, "missing@example.test"));
            StringAssert.Contains(
                await GlobalModeratorProvisioning.GrantAsync(scopes, "moderator@example.test"),
                "confirmed email");
            var users = setup.ServiceProvider.GetRequiredService<UserManager<AtlasIdentityUser>>();
            var user = (await users.FindByEmailAsync("moderator@example.test"))!;
            Assert.IsFalse(await users.IsInRoleAsync(user, AtlasRoles.GlobalModerator));
            var token = await users.GenerateEmailConfirmationTokenAsync(user);
            Assert.IsTrue((await users.ConfirmEmailAsync(user, token)).Succeeded);
            StringAssert.Contains(
                await GlobalModeratorProvisioning.GrantAsync(scopes, "moderator@example.test"),
                "granted");
            database.ChangeTracker.Clear();
            user = (await users.FindByEmailAsync("moderator@example.test"))!;
            Assert.IsTrue(await users.IsInRoleAsync(user, AtlasRoles.GlobalModerator));
            StringAssert.Contains(
                await GlobalModeratorProvisioning.GrantAsync(scopes, "moderator@example.test"),
                "already");
            Assert.IsTrue(await users.IsInRoleAsync(user, AtlasRoles.Member));
        }
        finally { await database.Database.EnsureDeletedAsync(); }
    }
}

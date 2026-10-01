using System.Security.Claims;
using Atlas.Moderation;
using Atlas.Persistence;
using Atlas.Persistence.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Identity.Tests;

[TestClass]
public sealed class SqlIdentityTests
{
    private const string Password = "Atlas_Test_Only_94!Password";

    [TestMethod]
    public async Task Registration_persists_shared_id_password_and_member_only_across_scopes()
    {
        using var fixture = Database.Create();
        Guid id;
        using (var scope = fixture.Services.CreateScope())
        {
            var accounts = scope.ServiceProvider.GetRequiredService<AtlasAccounts>();
            var registration = await accounts.RegisterAsync("member@example.test", Password, "New Member");
            Assert.IsTrue(registration.Result.Succeeded);
            id = registration.ParticipantId!.Value;
        }
        using var reloaded = fixture.Services.CreateScope();
        var users = reloaded.ServiceProvider.GetRequiredService<UserManager<AtlasIdentityUser>>();
        var user = (await users.FindByEmailAsync("MEMBER@example.test"))!;
        Assert.AreEqual(id, user.Id);
        Assert.IsTrue(await users.CheckPasswordAsync(user, Password));
        Assert.AreNotEqual(Password, user.PasswordHash);
        CollectionAssert.AreEquivalent(new[] { AtlasRoles.Member }, (await users.GetRolesAsync(user)).ToArray());
        var context = reloaded.ServiceProvider.GetRequiredService<AtlasDataContext>();
        Assert.AreEqual("New Member", (await context.ParticipantRows.FindAsync(id))!.DisplayName);
        Assert.IsFalse(context.Database.HasPendingModelChanges());
    }

    [TestMethod]
    public async Task Invalid_password_duplicate_email_and_missing_role_leave_no_orphan_profile()
    {
        using var fixture = Database.Create();
        using var scope = fixture.Services.CreateScope();
        var services = scope.ServiceProvider;
        var accounts = services.GetRequiredService<AtlasAccounts>();
        Assert.IsFalse((await accounts.RegisterAsync("weak@example.test", "weak", "Weak")).Result.Succeeded);
        Assert.IsTrue((await accounts.RegisterAsync("valid@example.test", Password, "Valid")).Result.Succeeded);
        Assert.IsFalse((await accounts.RegisterAsync("VALID@example.test", Password, "Duplicate")).Result.Succeeded);
        var context = services.GetRequiredService<AtlasDataContext>();
        Assert.AreEqual(1, await context.ParticipantRows.CountAsync());
        Assert.AreEqual(1, await context.Users.CountAsync());
        var roles = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        // Remove the role only after removing its sole membership to simulate misconfigured infrastructure.
        context.UserRoles.RemoveRange(context.UserRoles);
        await context.SaveChangesAsync();
        Assert.IsTrue((await roles.DeleteAsync((await roles.FindByNameAsync(AtlasRoles.Member))!)).Succeeded);
        await Assert.ThrowsAsync<InvalidOperationException>(() => accounts.RegisterAsync("no-role@example.test", Password, "No Role"));
        Assert.AreEqual(1, await context.ParticipantRows.CountAsync());
        Assert.AreEqual(1, await context.Users.CountAsync());
        Assert.AreEqual(0, context.ChangeTracker.Entries().Count());
    }

    [TestMethod]
    public async Task Login_requires_confirmation_enforces_lockout_and_rejects_inactive_profile()
    {
        using var fixture = Database.Create();
        using var scope = fixture.Services.CreateScope();
        var services = scope.ServiceProvider;
        var result = await services.GetRequiredService<AtlasAccounts>().RegisterAsync("login@example.test", Password, "Login");
        Assert.IsTrue(result.Result.Succeeded);
        var users = services.GetRequiredService<UserManager<AtlasIdentityUser>>();
        var user = (await users.FindByEmailAsync("login@example.test"))!;
        var signIn = services.GetRequiredService<SignInManager<AtlasIdentityUser>>();
        Assert.IsTrue((await signIn.CheckPasswordSignInAsync(user, Password, true)).IsNotAllowed);
        await Confirm(users, user);
        Assert.IsTrue((await signIn.CheckPasswordSignInAsync(user, Password, true)).Succeeded);
        for (var attempt = 0; attempt < 5; attempt++)
            await signIn.CheckPasswordSignInAsync(user, "wrong", true);
        Assert.IsTrue((await signIn.CheckPasswordSignInAsync(user, Password, true)).IsLockedOut);
        Assert.IsTrue((await users.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(-1))).Succeeded);
        var context = services.GetRequiredService<AtlasDataContext>();
        var profile = (await context.ParticipantRows.FindAsync(user.Id))!;
        profile.IsActive = false;
        await context.SaveChangesAsync();
        Assert.IsTrue((await signIn.CheckPasswordSignInAsync(user, Password, true)).IsNotAllowed);
    }

    [TestMethod]
    public async Task Policies_use_persisted_roles_and_active_authenticated_participant()
    {
        using var fixture = Database.Create();
        using var scope = fixture.Services.CreateScope();
        var services = scope.ServiceProvider;
        var accounts = services.GetRequiredService<AtlasAccounts>();
        await accounts.RegisterAsync("policy@example.test", Password, "Policy");
        var users = services.GetRequiredService<UserManager<AtlasIdentityUser>>();
        var user = (await users.FindByEmailAsync("policy@example.test"))!;
        await Confirm(users, user);
        var principal = await services.GetRequiredService<IUserClaimsPrincipalFactory<AtlasIdentityUser>>().CreateAsync(user);
        var authorization = services.GetRequiredService<IAuthorizationService>();
        Assert.AreEqual(user.Id, (await accounts.ResolveParticipantAsync(principal))!.Value);
        Assert.IsTrue((await authorization.AuthorizeAsync(principal, null, AtlasPolicies.Member)).Succeeded);
        Assert.IsFalse((await authorization.AuthorizeAsync(principal, null, AtlasPolicies.GlobalModerator)).Succeeded);
        // An invented role claim does not grant a database role.
        ((ClaimsIdentity)principal.Identity!).AddClaim(new Claim(ClaimTypes.Role, AtlasRoles.Administrator));
        Assert.IsFalse((await authorization.AuthorizeAsync(principal, null, AtlasPolicies.Administrator)).Succeeded);
        Assert.IsTrue((await users.AddToRoleAsync(user, AtlasRoles.GlobalModerator)).Succeeded);
        Assert.IsTrue((await authorization.AuthorizeAsync(principal, null, AtlasPolicies.GlobalModerator)).Succeeded);
        var moderation = services.GetRequiredService<IModeratorAuthorization>();
        Assert.IsTrue(moderation.IsAtlasModerator(user.Id));
        Assert.IsFalse((await authorization.AuthorizeAsync(principal, null, AtlasPolicies.Administrator)).Succeeded);
        Assert.IsTrue((await users.RemoveFromRoleAsync(user, AtlasRoles.GlobalModerator)).Succeeded);
        Assert.IsFalse((await authorization.AuthorizeAsync(principal, null, AtlasPolicies.GlobalModerator)).Succeeded);
        Assert.IsFalse(moderation.IsAtlasModerator(user.Id));
        Assert.IsTrue((await users.AddToRoleAsync(user, AtlasRoles.Administrator)).Succeeded);
        Assert.IsTrue((await authorization.AuthorizeAsync(principal, null, AtlasPolicies.Administrator)).Succeeded);
        Assert.IsTrue((await authorization.AuthorizeAsync(principal, null, AtlasPolicies.GlobalModerator)).Succeeded);
        Assert.IsTrue(moderation.IsAtlasModerator(user.Id));
        Assert.IsFalse((await authorization.AuthorizeAsync(new ClaimsPrincipal(new ClaimsIdentity(principal.Claims)),
            null, AtlasPolicies.Member)).Succeeded);
        var unknown = new ClaimsPrincipal(new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, AtlasRoles.Administrator) }, "test"));
        Assert.IsNull(await accounts.ResolveParticipantAsync(unknown));
        Assert.IsFalse((await authorization.AuthorizeAsync(unknown, null, AtlasPolicies.Administrator)).Succeeded);
        var context = services.GetRequiredService<AtlasDataContext>();
        (await context.ParticipantRows.FindAsync(user.Id))!.IsActive = false;
        await context.SaveChangesAsync();
        Assert.IsNull(await accounts.ResolveParticipantAsync(principal));
        Assert.IsFalse((await authorization.AuthorizeAsync(principal, null, AtlasPolicies.Member)).Succeeded);
        Assert.IsFalse(moderation.IsAtlasModerator(user.Id));
    }

    [TestMethod]
    public async Task Framework_password_change_and_reset_invalidate_old_credentials_and_tokens()
    {
        using var fixture = Database.Create();
        using var scope = fixture.Services.CreateScope();
        var services = scope.ServiceProvider;
        await services.GetRequiredService<AtlasAccounts>().RegisterAsync("reset@example.test", Password, "Reset");
        var users = services.GetRequiredService<UserManager<AtlasIdentityUser>>();
        var user = (await users.FindByEmailAsync("reset@example.test"))!;
        var oldStamp = user.SecurityStamp;
        const string changed = "Changed_Only_94!Password";
        Assert.IsTrue((await users.ChangePasswordAsync(user, Password, changed)).Succeeded);
        Assert.AreNotEqual(oldStamp, user.SecurityStamp);
        Assert.IsFalse(await users.CheckPasswordAsync(user, Password));
        var token = await users.GeneratePasswordResetTokenAsync(user);
        Assert.IsFalse((await users.ResetPasswordAsync(user, "invalid-token", Password)).Succeeded);
        Assert.IsTrue((await users.ResetPasswordAsync(user, token, Password)).Succeeded);
        Assert.IsFalse(await users.CheckPasswordAsync(user, changed));
        Assert.IsFalse((await users.ResetPasswordAsync(user, token, changed)).Succeeded);
    }

    [TestMethod]
    public async Task Identity_migration_preserves_existing_profile_and_sql_restricts_missing_profile_and_duplicate_email()
    {
        using var fixture = Database.Create(migrate: false);
        var id = Guid.NewGuid();
        using (var scope = fixture.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AtlasDataContext>();
            await context.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrator>()
                .MigrateAsync("20260929202249_AddReferentialIntegrity");
            context.ParticipantRows.Add(new ParticipantRow { Id = id, DisplayName = "Existing demo", Bio = "Keep",
                IsActive = true, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow });
            await context.SaveChangesAsync();
            await context.Database.MigrateAsync();
            Assert.AreEqual("Keep", (await context.ParticipantRows.AsNoTracking().SingleAsync(row => row.Id == id)).Bio);
            Assert.AreEqual(0, await context.Users.CountAsync());
            context.Users.Add(new AtlasIdentityUser { Id = Guid.NewGuid(), UserName = "missing-profile" });
            await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        }
        using var validScope = fixture.Services.CreateScope();
        var services = validScope.ServiceProvider;
        await services.GetRequiredService<AtlasAccounts>().RegisterAsync("unique@example.test", Password, "Unique");
        var database = services.GetRequiredService<AtlasDataContext>();
        database.Users.Add(new AtlasIdentityUser { Id = id, UserName = "other", NormalizedUserName = "OTHER",
            Email = "unique@example.test", NormalizedEmail = "UNIQUE@EXAMPLE.TEST" });
        await Assert.ThrowsAsync<DbUpdateException>(() => database.SaveChangesAsync());
    }

    private static async Task Confirm(UserManager<AtlasIdentityUser> users, AtlasIdentityUser user)
    {
        var token = await users.GenerateEmailConfirmationTokenAsync(user);
        Assert.IsTrue((await users.ConfirmEmailAsync(user, token)).Succeeded);
    }

    private sealed class Database : IDisposable
    {
        public ServiceProvider Services { get; }
        private Database(string connection, bool migrate)
        {
            var services = new ServiceCollection();
            services.AddAtlasIdentity(connection);
            Services = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
            if (migrate)
            {
                using var scope = Services.CreateScope();
                scope.ServiceProvider.GetRequiredService<AtlasDataContext>().Database.Migrate();
            }
        }
        public static Database Create(bool migrate = true)
        {
            var template = Environment.GetEnvironmentVariable("ATLAS_SQL_TEST_CONNECTION_STRING");
            if (string.IsNullOrWhiteSpace(template)) Assert.Inconclusive("Set ATLAS_SQL_TEST_CONNECTION_STRING for SQL Server Identity tests.");
            var connection = new SqlConnectionStringBuilder(template) { InitialCatalog = $"AtlasIdentityTests_{Guid.NewGuid():N}" };
            return new Database(connection.ConnectionString, migrate);
        }
        public void Dispose()
        {
            using (var scope = Services.CreateScope())
                scope.ServiceProvider.GetRequiredService<AtlasDataContext>().Database.EnsureDeleted();
            Services.Dispose();
        }
    }
}

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Atlas.Application.Storage;
using Atlas.Graph.Nodes.NodeTypes;
using Atlas.Identity;
using Atlas.Persistence;
using Atlas.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Atlas.Api.Tests;

[TestClass]
public sealed class SqlWorkflowTests
{
    [TestMethod]
    public async Task Cookie_login_csrf_node_vote_community_comment_profile_and_logout_use_real_workflows()
    {
        var config = new ConfigurationBuilder().AddUserSecrets<SqlWorkflowTests>(true).AddEnvironmentVariables().Build();
        var template = config["ATLAS_SQL_TEST_CONNECTION_STRING"];
        if (string.IsNullOrWhiteSpace(template)) Assert.Inconclusive("Configure ATLAS_SQL_TEST_CONNECTION_STRING for HTTP/SQL parity tests.");
        var connection = new SqlConnectionStringBuilder(template) { InitialCatalog = "AtlasApiTests_" + Guid.NewGuid().ToString("N") }.ConnectionString;
        using var factory = new ApiFactory(connection);
        var options = new DbContextOptionsBuilder<AtlasDataContext>().UseSqlServer(connection).Options;
        await using var database = new AtlasDataContext(options);
        await database.Database.EnsureCreatedAsync();
        try
        {
            Guid typeId;
            using (var scope = factory.Services.CreateScope())
            {
                var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
                Assert.IsTrue(await roles.RoleExistsAsync(AtlasRoles.Member));
                var accounts = scope.ServiceProvider.GetRequiredService<AtlasAccounts>();
                var registration = await accounts.RegisterAsync("api@example.test", "Atlas_Test_Only_125!Password", "API participant");
                Assert.IsTrue(registration.Result.Succeeded);
                var users = scope.ServiceProvider.GetRequiredService<UserManager<AtlasIdentityUser>>();
                var user = (await users.FindByEmailAsync("api@example.test"))!;
                Assert.IsTrue((await users.ConfirmEmailAsync(user, await users.GenerateEmailConfirmationTokenAsync(user))).Succeeded);
                var type = NodeTypeDefinition.CreateSystemDefined("Idea", "Test idea", DateTimeOffset.UtcNow);
                new SqlNodeTypeRepository(() => new AtlasDataContext(options)).Save(type); typeId = type.Id.Value;
            }
            using var client = factory.Browser();
            await Token(client);
            var login = await client.PostAsJsonAsync("/api/auth/login", new { email="api@example.test", password="Atlas_Test_Only_125!Password" });
            Assert.AreEqual(HttpStatusCode.NoContent, login.StatusCode, await login.Content.ReadAsStringAsync());
            var cookies = string.Join(";", login.Headers.GetValues("Set-Cookie"));
            StringAssert.Contains(cookies, "__Host-Atlas.Session"); StringAssert.Contains(cookies, "httponly"); StringAssert.Contains(cookies, "secure");
            await Token(client); // Antiforgery tokens are bound to the current principal.
            var created = await client.PostAsJsonAsync("/api/nodes", new { title="HTTP idea", markdown="Description", typeId, requestedTypeIds=Array.Empty<Guid>(), actorParticipantId=Guid.NewGuid(), roles=new[]{"Administrator"} });
            Assert.AreEqual(HttpStatusCode.Created, created.StatusCode, await created.Content.ReadAsStringAsync());
            var node = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
            // A real authenticated cookie is insufficient for writes: both missing
            // and forged tokens must fail before creation/archive/catalog mutations.
            foreach (var path in new[] { "/api/nodes", $"/api/nodes/{node}/archive", "/api/catalog/reactions" })
            {
                client.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
                Assert.AreEqual(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync(path, new { })).StatusCode, path);
                client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", "forged-token");
                var rejected = await client.PostAsJsonAsync(path, new { });
                Assert.AreEqual(HttpStatusCode.BadRequest, rejected.StatusCode, path);
                StringAssert.Contains(await rejected.Content.ReadAsStringAsync(), "A valid CSRF token is required.");
            }
            await Token(client); // Restore a valid pair for the successful workflow below.
            // A second real account must not be able to rename the first author's node.
            using (var scope = factory.Services.CreateScope())
            {
                var accounts = scope.ServiceProvider.GetRequiredService<AtlasAccounts>();
                Assert.IsTrue((await accounts.RegisterAsync("other@example.test", "Atlas_Test_Only_125!Password", "Other participant")).Result.Succeeded);
                var users = scope.ServiceProvider.GetRequiredService<UserManager<AtlasIdentityUser>>();
                var other = (await users.FindByEmailAsync("other@example.test"))!;
                Assert.IsTrue((await users.ConfirmEmailAsync(other, await users.GenerateEmailConfirmationTokenAsync(other))).Succeeded);
            }
            using (var otherClient = factory.Browser())
            {
                await Token(otherClient);
                await Success(await otherClient.PostAsJsonAsync("/api/auth/login", new { email="other@example.test", password="Atlas_Test_Only_125!Password" }));
                await Token(otherClient);
                Assert.AreEqual(HttpStatusCode.Forbidden, (await otherClient.PutAsJsonAsync($"/api/nodes/{node}/title", new { text="Forged title" })).StatusCode);
            }
            var unchanged = await client.GetFromJsonAsync<JsonElement>($"/api/nodes/{node}");
            Assert.AreEqual("HTTP idea", unchanged.GetProperty("title").GetString());
            await Success(await client.PutAsJsonAsync($"/api/nodes/{node}/vote", new { value=7 }));
            await Success(await client.DeleteAsync($"/api/nodes/{node}/vote"));
            await Success(await client.PostAsJsonAsync($"/api/nodes/{node}/comments", new { body="Comment from HTTP" }));
            var page = await client.GetFromJsonAsync<JsonElement>($"/api/nodes/{node}/comments?size=1");
            Assert.AreEqual(1, page.GetProperty("items").GetArrayLength());
            await Success(await client.PostAsJsonAsync("/api/communities", new { name="API community",description="Test community" }));
            var quota = await client.PostAsJsonAsync("/api/communities", new { name="Second community",description="Test community" });
            Assert.AreEqual(HttpStatusCode.TooManyRequests, quota.StatusCode);
            StringAssert.Contains(await quota.Content.ReadAsStringAsync(), "quota");
            await Success(await client.PutAsJsonAsync("/api/participants/me", new { displayName="Updated API participant", bio="Updated over HTTP" }));
            Assert.AreEqual(HttpStatusCode.Forbidden, (await client.GetAsync("/api/moderation/queue")).StatusCode);
            await Success(await client.GetAsync("/api/nodes?page=1&searchText=HTTP"));
            await Success(await client.PostAsJsonAsync($"/api/nodes/{node}/description/blocks", new { kind="markdown", text="Another block" }));
            await Success(await client.GetAsync($"/api/nodes/{node}/description"));
            await Success(await client.GetAsync("/api/notifications?size=20"));
            await Success(await client.PostAsJsonAsync("/api/catalog/node-types", new { name="HTTP custom type", description="Created over HTTP", autoPluralize=true }));
            await Success(await client.PostAsJsonAsync($"/api/moderation/nodes/{node}/reports", new { reason="Spam", explanation="Review this test node" }));
            using (var scope = factory.Services.CreateScope())
            {
                var users = scope.ServiceProvider.GetRequiredService<UserManager<AtlasIdentityUser>>();
                var user = (await users.FindByEmailAsync("api@example.test"))!;
                Assert.IsTrue((await users.AddToRoleAsync(user, AtlasRoles.GlobalModerator)).Succeeded);
            }
            // Current stored roles are consulted even though the cookie predates this grant.
            await Success(await client.GetAsync("/api/moderation/queue"));
            await Success(await client.PostAsJsonAsync($"/api/moderation/nodes/{node}/decision", new { decision=1, rationale="Test moderation", reason=1 }));
            using (var anonymous = factory.Browser())
            {
                var hidden = await anonymous.GetFromJsonAsync<JsonElement>($"/api/nodes/{node}");
                Assert.IsTrue(hidden.GetProperty("hidden").GetBoolean());
                Assert.IsFalse(hidden.TryGetProperty("title", out _));
                Assert.AreEqual(HttpStatusCode.NotFound, (await anonymous.GetAsync($"/api/nodes/{node}/description")).StatusCode);
                Assert.AreEqual(HttpStatusCode.NotFound, (await anonymous.GetAsync($"/api/nodes/{node}/comments")).StatusCode);
            }
            // Restoration follows the existing author-edit -> review-request workflow.
            await Success(await client.PutAsJsonAsync($"/api/nodes/{node}/title", new { text="Revised after moderation" }));
            await Success(await client.PostAsync($"/api/moderation/nodes/{node}/review", null));
            await Success(await client.PostAsJsonAsync($"/api/moderation/nodes/{node}/restore", new { text="Restore after test" }));
            await Success(await client.PostAsync("/api/auth/logout", null));
            Assert.AreEqual(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/auth/session")).StatusCode);
        }
        finally { await database.Database.EnsureDeletedAsync(); }
    }
    [TestMethod]
    public async Task Recovery_uses_private_delivery_and_Identity_token_validation()
    {
        var config = new ConfigurationBuilder().AddUserSecrets<SqlWorkflowTests>(true).AddEnvironmentVariables().Build();
        var template = config["ATLAS_SQL_TEST_CONNECTION_STRING"];
        if (string.IsNullOrWhiteSpace(template)) Assert.Inconclusive("Configure ATLAS_SQL_TEST_CONNECTION_STRING.");
        var connection = new SqlConnectionStringBuilder(template) { InitialCatalog = "AtlasRecovery_" + Guid.NewGuid().ToString("N") }.ConnectionString;
        var mail = Path.Combine(Path.GetTempPath(), "atlas-mail-" + Guid.NewGuid().ToString("N"));
        using var root = new ApiFactory(connection);
        using var factory = root.WithWebHostBuilder(b => b.UseSetting("Atlas:DevelopmentMailDirectory", mail));
        var options = new DbContextOptionsBuilder<AtlasDataContext>().UseSqlServer(connection).Options;
        await using var database = new AtlasDataContext(options);
        await database.Database.EnsureCreatedAsync();
        try
        {
            using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
                { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
            await Token(client);
            const string email = "recover@example.test", oldPassword = "Atlas_Old_125!Password", newPassword = "Atlas_New_125!Password";
            Assert.AreEqual(HttpStatusCode.Accepted, (await client.PostAsJsonAsync("/api/auth/register",
                new { email, password = oldPassword, displayName = "Recovery test" })).StatusCode);
            var confirm = JsonDocument.Parse(await File.ReadAllTextAsync(Directory.GetFiles(mail).Single())).RootElement.Clone();
            File.Delete(Directory.GetFiles(mail).Single());
            // Unknown and unconfirmed accounts both receive the same accepted response and no reset delivery.
            foreach (var address in new[] { "unknown@example.test", email })
                Assert.AreEqual(HttpStatusCode.Accepted, (await client.PostAsJsonAsync("/api/auth/forgot-password", new { email = address })).StatusCode);
            Assert.AreEqual(0, Directory.GetFiles(mail).Length);
            Assert.AreEqual(HttpStatusCode.Accepted, (await client.PostAsJsonAsync("/api/auth/resend-confirmation", new { email })).StatusCode);
            Assert.AreEqual(1, Directory.GetFiles(mail).Length);
            File.Delete(Directory.GetFiles(mail).Single());
            Assert.AreEqual(HttpStatusCode.NoContent, (await client.PostAsJsonAsync("/api/auth/confirm-email",
                new { userId = confirm.GetProperty("userId").GetGuid(), token = confirm.GetProperty("token").GetString() })).StatusCode);
            Assert.AreEqual(HttpStatusCode.Accepted, (await client.PostAsJsonAsync("/api/auth/forgot-password", new { email })).StatusCode);
            using var resetFile = JsonDocument.Parse(await File.ReadAllTextAsync(Directory.GetFiles(mail).Single()));
            var token = resetFile.RootElement.GetProperty("token").GetString();
            Assert.AreEqual("reset-password", resetFile.RootElement.GetProperty("purpose").GetString());
            Assert.AreEqual(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/auth/reset-password", new { email, token = "invalid", newPassword })).StatusCode);
            Assert.AreEqual(HttpStatusCode.NoContent, (await client.PostAsJsonAsync("/api/auth/reset-password", new { email, token, newPassword })).StatusCode);
            Assert.AreEqual(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/auth/reset-password", new { email, token, newPassword })).StatusCode);
            Assert.AreEqual(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/auth/login", new { email, password = oldPassword })).StatusCode);
            Assert.AreEqual(HttpStatusCode.NoContent, (await client.PostAsJsonAsync("/api/auth/login", new { email, password = newPassword })).StatusCode);
            await Token(client);
            var session = await client.GetFromJsonAsync<JsonElement>("/api/auth/session");
            var participantId = session.GetProperty("participantId").GetGuid();
            var type = NodeTypeDefinition.CreateSystemDefined("Closure idea", "Retained content", DateTimeOffset.UtcNow);
            new SqlNodeTypeRepository(() => new AtlasDataContext(options)).Save(type);
            var created = await client.PostAsJsonAsync("/api/nodes", new
                { title = "Retained post", markdown = "Retained body", typeId = type.Id.Value, requestedTypeIds = Array.Empty<Guid>() });
            Assert.AreEqual(HttpStatusCode.Created, created.StatusCode);
            var node = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
            await Success(await client.PostAsJsonAsync($"/api/nodes/{node}/comments", new { body = "Retained comment" }));
            Assert.AreEqual(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/auth/delete-account", new { password = "wrong" })).StatusCode);
            Assert.AreEqual(HttpStatusCode.NoContent, (await client.PostAsJsonAsync("/api/auth/delete-account", new { password = newPassword })).StatusCode);
            Assert.AreEqual(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/auth/session")).StatusCode);
            var tombstone = await client.GetFromJsonAsync<JsonElement>($"/api/participants/{participantId}");
            Assert.AreEqual("Deleted user", tombstone.GetProperty("displayName").GetString());
            Assert.IsTrue(tombstone.GetProperty("deleted").GetBoolean());
            Assert.IsTrue((await client.GetAsync($"/api/nodes/{node}")).IsSuccessStatusCode);
            var comments = await client.GetAsync($"/api/nodes/{node}/comments");
            Assert.IsTrue(comments.IsSuccessStatusCode);
            StringAssert.Contains(await comments.Content.ReadAsStringAsync(), "Retained comment");
            Assert.IsFalse(await database.Users.AnyAsync(u => u.Id == participantId));
            var profile = await database.ParticipantRows.AsNoTracking().SingleAsync(p => p.Id == participantId);
            Assert.IsFalse(profile.IsActive);
            Assert.AreEqual("Deleted user", profile.DisplayName);
            Assert.AreEqual(string.Empty, profile.Bio);
        }
        finally
        {
            await database.Database.EnsureDeletedAsync();
            if (Directory.Exists(mail)) Directory.Delete(mail, true);
        }
    }

    private static async Task Token(HttpClient client)
    {
        var json = await client.GetFromJsonAsync<JsonElement>("/api/auth/csrf");
        client.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", json.GetProperty("token").GetString());
    }
    private static async Task Success(HttpResponseMessage response) => Assert.IsTrue(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
}

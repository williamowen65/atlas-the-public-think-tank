using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Atlas.Application.Storage;
using Atlas.Testing;
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
    /// <summary>
    /// Given an isolated SQL database and real Identity accounts, follow one signed-in content workflow.
    /// Expect CSRF/ownership/role denials where appropriate, successful permitted operations, and logout
    /// rejection.
    /// </summary>
    [TestMethod]
    public async Task Cookie_login_csrf_node_vote_community_comment_profile_and_logout_use_real_workflows()
    {
        // A unique database name isolates this scenario; finally removes only that temporary database.
        // User secrets/environment provide the test SQL connection, never a production database name.
        var config = new ConfigurationBuilder().AddUserSecrets<SqlWorkflowTests>(true).AddEnvironmentVariables().Build();
        var template = config["ATLAS_SQL_TEST_CONNECTION_STRING"];
        if (string.IsNullOrWhiteSpace(template))
            Assert.Inconclusive("Configure ATLAS_SQL_TEST_CONNECTION_STRING for HTTP/SQL parity tests.");
        var connection = new SqlConnectionStringBuilder(template)
        {
            InitialCatalog = "AtlasApiTests_" + Guid.NewGuid().ToString("N")
        }.ConnectionString;
        using var factory = new ApiFactory(connection);
        var options = new DbContextOptionsBuilder<AtlasDataContext>().UseSqlServer(connection).Options;
        await using var database = new AtlasDataContext(options);
        // EnsureCreated builds this disposable database from the EF model; it does not apply production migrations.
        await database.Database.EnsureCreatedAsync();
        SqlFullTextSupport.CreateIndexes(database);
        try
        {
            // This client retains its cookie jar across all phases of the same scenario.
            using var client = factory.Browser();
            var typeId = await SeedConfirmedMemberAndNodeType(factory, options);
            var node = await SignInAndCreateNode(client, typeId);

            await RejectAuthenticatedWritesWithoutMatchingCsrf(client, node);
            await RejectOtherMemberEditsAndModeratorReads(factory, client, node);
            await ExerciseVotesCommentsAndCommunityQuota(client, node);
            await ExerciseProfileContentCatalogAndReporting(client, node);

            await GrantGlobalModeratorRole(factory);
            await HideVerifyAndRestoreNode(factory, client, node);
            await SignOutAndRejectSession(client);
        }
        finally
        {
            await database.Database.EnsureDeletedAsync();
        }
    }

    /// <summary>
    /// Given a fresh SQL database and private mail folder, register, confirm, recover and close an account.
    /// Expect single-use reset validation and removal of credentials while posts/comments survive
    /// anonymously.
    /// </summary>
    [TestMethod]
    public async Task Recovery_uses_private_delivery_and_Identity_token_validation()
    {
        // A unique database name isolates this scenario; finally removes only that temporary database.
        // User secrets/environment provide the test SQL connection, never a production database name.
        var config = new ConfigurationBuilder().AddUserSecrets<SqlWorkflowTests>(true).AddEnvironmentVariables().Build();
        var template = config["ATLAS_SQL_TEST_CONNECTION_STRING"];
        if (string.IsNullOrWhiteSpace(template))
            Assert.Inconclusive("Configure ATLAS_SQL_TEST_CONNECTION_STRING.");
        var connection = new SqlConnectionStringBuilder(template)
        {
            InitialCatalog = "AtlasRecovery_" + Guid.NewGuid().ToString("N")
        }.ConnectionString;
        var mail = Path.Combine(Path.GetTempPath(), "atlas-mail-" + Guid.NewGuid().ToString("N"));
        using var root = new ApiFactory(connection);
        using var factory = root.WithWebHostBuilder(b => b.UseSetting("Atlas:DevelopmentMailDirectory", mail));
        var options = new DbContextOptionsBuilder<AtlasDataContext>().UseSqlServer(connection).Options;
        await using var database = new AtlasDataContext(options);
        // EnsureCreated builds this disposable database from the EF model; it does not apply production migrations.
        await database.Database.EnsureCreatedAsync();
        SqlFullTextSupport.CreateIndexes(database);
        try
        {
            using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),
                AllowAutoRedirect = false
            });
            await RefreshCsrfToken(client);
            const string email = "recover@example.test";
            const string oldPassword = "Atlas_Old_125!Password";
            const string newPassword = "Atlas_New_125!Password";
            var confirm = await RegisterUnconfirmedAccount(client, mail, email, oldPassword);
            await CheckGenericRecoveryAndConfirm(client, mail, email, confirm);
            await ResetPasswordAndCheckTokens(client, mail, email, newPassword);
            await VerifyOldAndNewPasswordLogin(client, email, oldPassword, newPassword);

            var contribution = await CreateContributionsBeforeClosure(client, options);
            await CloseAccountAndVerifyRetainedContributions(
                client,
                database,
                contribution.ParticipantId,
                contribution.NodeId,
                newPassword
            );
        }
        finally
        {
            await database.Database.EnsureDeletedAsync();
            if (Directory.Exists(mail))
                Directory.Delete(mail, true);
        }
    }

    /// <summary>
    /// Given the fresh database, create a confirmed Member and a node type; expect all seed operations
    /// to succeed.
    /// </summary>
    private static async Task<Guid> SeedConfirmedMemberAndNodeType(ApiFactory factory, DbContextOptions<AtlasDataContext> options)
    {
        Guid typeId;
        // A DI scope supplies correctly scoped Identity/DbContext services for arranging test state.
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
            new SqlNodeTypeRepository(() => new AtlasDataContext(options)).Save(type);
            typeId = type.Id.Value;
        }
        return typeId;
    }

    /// <summary>
    /// Given the confirmed Member, expect a secure session cookie and a 201 node response, despite
    /// forged actor/role fields.
    /// </summary>
    private static async Task<Guid> SignInAndCreateNode(HttpClient client, Guid typeId)
    {
        await RefreshCsrfToken(client);
        var login = await client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                email = "api@example.test",
                password = "Atlas_Test_Only_125!Password"
            }
        );
        Assert.AreEqual(HttpStatusCode.NoContent, login.StatusCode, await login.Content.ReadAsStringAsync());
        var cookies = string.Join(";", login.Headers.GetValues("Set-Cookie"));
        StringAssert.Contains(cookies, "__Host-Atlas.Session");
        StringAssert.Contains(cookies, "httponly");
        StringAssert.Contains(cookies, "secure");
        await RefreshCsrfToken(client);
        // Antiforgery tokens are bound to the current principal.
        var created = await client.PostAsJsonAsync(
            "/api/nodes",
            new
            {
                title = "HTTP idea",
                markdown = "Description",
                shortDescription = "A concise HTTP idea introduction",
                typeId,
                requestedTypeIds = Array.Empty<Guid>(),
                actorParticipantId = Guid.NewGuid(),
                roles = new[]
                {
                    "Administrator"
                }
            }
        );
        Assert.AreEqual(
            HttpStatusCode.Created,
            created.StatusCode,
            await created.Content.ReadAsStringAsync()
        );
        var node = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        return node;
    }

    /// <summary>
    /// Given a signed-in cookie, missing and forged CSRF headers must still return 400; restore a valid
    /// pair afterward.
    /// </summary>
    private static async Task RejectAuthenticatedWritesWithoutMatchingCsrf(HttpClient client, Guid node)
    {
        // A real authenticated cookie is insufficient for writes: both missing
        // and forged tokens must fail before creation/archive/catalog mutations.
        foreach (var path in new[]
        {
            "/api/nodes",
            $"/api/nodes/{node}/archive",
            "/api/catalog/reactions"
        })
        {
            client.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
            Assert.AreEqual(
                HttpStatusCode.BadRequest,
                (await client.PostAsJsonAsync(path, new { })).StatusCode,
                path
            );
            client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", "forged-token");
            var rejected = await client.PostAsJsonAsync(path, new { });
            Assert.AreEqual(HttpStatusCode.BadRequest, rejected.StatusCode, path);
            StringAssert.Contains(await rejected.Content.ReadAsStringAsync(), "A valid CSRF token is required.");
        }
        await RefreshCsrfToken(client);
        // Restore a valid pair for the successful workflow below.
    }

    /// <summary>
    /// Given a second Member, expect 403 for editing another author’s node or reading the moderator
    /// queue; the title stays unchanged.
    /// </summary>
    private static async Task RejectOtherMemberEditsAndModeratorReads(ApiFactory factory, HttpClient client, Guid node)
    {
        // A second real account must not be able to rename the first author's node.
        // A DI scope supplies correctly scoped Identity/DbContext services for arranging test state.
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
            await RefreshCsrfToken(otherClient);
            await Success(await otherClient.PostAsJsonAsync(
                "/api/auth/login",
                new
                {
                    email = "other@example.test",
                    password = "Atlas_Test_Only_125!Password"
                }
            ));
            await RefreshCsrfToken(otherClient);
            Assert.AreEqual(
                HttpStatusCode.Forbidden,
                (await otherClient.PutAsJsonAsync($"/api/nodes/{node}/title", new
                {
                    text = "Forged title"
                })).StatusCode
            );
            // An authenticated Member is not a global moderator and cannot inspect the queue.
            Assert.AreEqual(
                HttpStatusCode.Forbidden,
                (await otherClient.GetAsync("/api/moderation/queue")).StatusCode
            );
        }
        var unchanged = await client.GetFromJsonAsync<JsonElement>($"/api/nodes/{node}");
        Assert.AreEqual("HTTP idea", unchanged.GetProperty("title").GetString());
    }

    /// <summary>
    /// Given the original author’s session, exercise writes/reads and expect the second community
    /// creation to hit its domain quota.
    /// </summary>
    private static async Task ExerciseVotesCommentsAndCommunityQuota(HttpClient client, Guid node)
    {
        await Success(await client.PutAsJsonAsync($"/api/nodes/{node}/short-description", new { text = "Updated short introduction" }));
        var updatedIntroduction = await client.GetFromJsonAsync<JsonElement>($"/api/nodes/{node}");
        Assert.AreEqual("Updated short introduction", updatedIntroduction.GetProperty("shortDescription").GetString());
        Assert.AreEqual(HttpStatusCode.BadRequest, (await client.PutAsJsonAsync($"/api/nodes/{node}/short-description", new { text = new string('x', 281) })).StatusCode);
        await Success(await client.PutAsJsonAsync($"/api/nodes/{node}/vote", new
        {
            value = 7
        }));
        await Success(await client.DeleteAsync($"/api/nodes/{node}/vote"));
        await Success(await client.PostAsJsonAsync($"/api/nodes/{node}/comments", new
        {
            body = "Comment from HTTP"
        }));
        var page = await client.GetFromJsonAsync<JsonElement>($"/api/nodes/{node}/comments?size=1");
        Assert.AreEqual(1, page.GetProperty("items").GetArrayLength());
        var commentId = page.GetProperty("items")[0].GetProperty("id").GetGuid();
        Assert.AreEqual(HttpStatusCode.BadRequest, (await client.PutAsJsonAsync($"/api/comments/{commentId}/vote", new { value = 0 })).StatusCode);
        foreach (var value in new[] { 1, 1, -1 })
        {
            await Success(await client.PutAsJsonAsync($"/api/comments/{commentId}/vote", new { value }));
            var totals = await client.GetFromJsonAsync<JsonElement>($"/api/comments/{commentId}/votes");
            Assert.AreEqual(value, totals.GetProperty("score").GetInt32());
            Assert.AreEqual(1, totals.GetProperty("totalVotes").GetInt32());
            Assert.AreEqual(value, totals.GetProperty("currentParticipantVote").GetInt32());
        }
        await Success(await client.DeleteAsync($"/api/comments/{commentId}/vote"));
        var cleared = await client.GetFromJsonAsync<JsonElement>($"/api/comments/{commentId}/votes");
        Assert.AreEqual(0, cleared.GetProperty("score").GetInt32());
        Assert.AreEqual(JsonValueKind.Null, cleared.GetProperty("currentParticipantVote").ValueKind);

        await Success(await client.PostAsJsonAsync("/api/communities", new
        {
            name = "API community",
            description = "Test community"
        }));
        var quota = await client.PostAsJsonAsync("/api/communities", new
        {
            name = "Second community",
            description = "Test community"
        });
        Assert.AreEqual(HttpStatusCode.TooManyRequests, quota.StatusCode);
        StringAssert.Contains(await quota.Content.ReadAsStringAsync(), "quota");
    }

    /// <summary>
    /// Given an ordinary Member session, expect profile/content/catalog operations to succeed, but
    /// moderator reads to fail.
    /// Submit a report that the following moderator scenario can review.
    /// </summary>
    private static async Task ExerciseProfileContentCatalogAndReporting(HttpClient client, Guid node)
    {
        await Success(await client.PutAsJsonAsync(
            "/api/participants/me",
            new
            {
                displayName = "Updated API participant",
                bio = "Updated over HTTP"
            }
        ));
        Assert.AreEqual(
            HttpStatusCode.Forbidden,
            (await client.GetAsync("/api/moderation/queue")).StatusCode
        );
        await Success(await client.GetAsync("/api/nodes?page=1&searchText=HTTP"));
        await Success(await client.PostAsJsonAsync(
            $"/api/nodes/{node}/description/blocks",
            new
            {
                kind = "markdown",
                text = "Another block"
            }
        ));
        await Success(await client.GetAsync($"/api/nodes/{node}/description"));
        await Success(await client.GetAsync("/api/notifications?size=20"));
        await Success(await client.PostAsJsonAsync(
            "/api/catalog/node-types",
            new
            {
                name = "HTTP custom type",
                description = "Created over HTTP",
                autoPluralize = true
            }
        ));
        await Success(await client.PostAsJsonAsync(
            "/api/catalog/node-types",
            new
            {
                name = "HTTP evidence",
                description = "Uncountable contribution type",
                autoPluralize = false
            }
        ));
        using (var catalog = JsonDocument.Parse(await client.GetStringAsync("/api/catalog/node-types")))
        {
            var customType = catalog.RootElement.EnumerateArray()
                .Single(type => type.GetProperty("name").GetString() == "HTTP custom type");
            Assert.AreEqual("HTTP custom types", customType.GetProperty("pluralName").GetString());
            var evidenceType = catalog.RootElement.EnumerateArray()
                .Single(type => type.GetProperty("name").GetString() == "HTTP evidence");
            Assert.AreEqual("HTTP evidence", evidenceType.GetProperty("pluralName").GetString());
        }
        await Success(await client.PostAsJsonAsync(
            $"/api/moderation/nodes/{node}/reports",
            new
            {
                reason = "Spam",
                explanation = "Review this test node"
            }
        ));
    }

    /// <summary>
    /// Grant a stored moderator role to the existing account; the next phase checks it without logging
    /// in again.
    /// </summary>
    private static async Task GrantGlobalModeratorRole(ApiFactory factory)
    {
        // A DI scope supplies correctly scoped Identity/DbContext services for arranging test state.
        using (var scope = factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<AtlasIdentityUser>>();
            var user = (await users.FindByEmailAsync("api@example.test"))!;
            Assert.IsTrue((await users.AddToRoleAsync(user, AtlasRoles.GlobalModerator)).Succeeded);
        }
    }

    /// <summary>
    /// Given the new moderator role, hide the node, expect anonymous placeholders/404s, then follow
    /// edit-review-restore.
    /// </summary>
    private static async Task HideVerifyAndRestoreNode(ApiFactory factory, HttpClient client, Guid node)
    {
        // Current stored roles are consulted even though the cookie predates this grant.
        await Success(await client.GetAsync("/api/moderation/queue"));
        await Success(await client.PostAsJsonAsync(
            $"/api/moderation/nodes/{node}/decision",
            new
            {
                decision = 1,
                rationale = "Test moderation",
                reason = 1
            }
        ));
        using (var anonymous = factory.Browser())
        {
            var hidden = await anonymous.GetFromJsonAsync<JsonElement>($"/api/nodes/{node}");
            Assert.IsTrue(hidden.GetProperty("hidden").GetBoolean());
            Assert.IsFalse(hidden.TryGetProperty("title", out _));
            Assert.AreEqual(
                HttpStatusCode.NotFound,
                (await anonymous.GetAsync($"/api/nodes/{node}/description")).StatusCode
            );
            Assert.AreEqual(
                HttpStatusCode.NotFound,
                (await anonymous.GetAsync($"/api/nodes/{node}/comments")).StatusCode
            );
        }
        // Restoration follows the existing author-edit -> review-request workflow.
        await Success(await client.PutAsJsonAsync($"/api/nodes/{node}/title", new
        {
            text = "Revised after moderation"
        }));
        await Success(await client.PostAsync($"/api/moderation/nodes/{node}/review", null));
        await Success(await client.PostAsJsonAsync($"/api/moderation/nodes/{node}/restore", new
        {
            text = "Restore after test"
        }));
    }

    /// <summary>
    /// Given a signed-in session, logout must succeed and the following session read must return 401.
    /// </summary>
    private static async Task SignOutAndRejectSession(HttpClient client)
    {
        await Success(await client.PostAsync("/api/auth/logout", null));
        Assert.AreEqual(
            HttpStatusCode.Unauthorized,
            (await client.GetAsync("/api/auth/session")).StatusCode
        );
    }

    /// <summary>
    /// Given a fresh email, register through HTTP and read its private confirmation payload; expect 202
    /// and one payload.
    /// </summary>
    private static async Task<JsonElement> RegisterUnconfirmedAccount(HttpClient client, string mail, string email, string oldPassword)
    {
        Assert.AreEqual(
            HttpStatusCode.Accepted,
            (await client.PostAsJsonAsync(
                "/api/auth/register",
                new
                {
                    email,
                    password = oldPassword,
                    displayName = "Recovery test"
                }
            )).StatusCode
        );
        // Read the private server-side mail payload, not a token returned to an HTTP caller.
        // Clone detaches the JSON value from its document so it can be returned to the next phase.
        var confirm = JsonDocument.Parse(await File.ReadAllTextAsync(Directory.GetFiles(mail).Single())).RootElement.Clone();
        // Remove consumed test evidence so later file-count checks observe only the next delivery.
        File.Delete(Directory.GetFiles(mail).Single());
        return confirm;
    }

    /// <summary>
    /// Unknown/unconfirmed reset requests return 202 without delivery; resend creates one confirmation
    /// file, then confirmation returns 204.
    /// </summary>
    private static async Task CheckGenericRecoveryAndConfirm(HttpClient client, string mail, string email, JsonElement confirm)
    {
        // Unknown and unconfirmed accounts both receive the same accepted response and no reset delivery.
        foreach (var address in new[]
        {
            "unknown@example.test",
            email
        })
            Assert.AreEqual(
                HttpStatusCode.Accepted,
                (await client.PostAsJsonAsync("/api/auth/forgot-password", new
                {
                    email = address
                })).StatusCode
            );
        Assert.AreEqual(0, Directory.GetFiles(mail).Length);
        Assert.AreEqual(
            HttpStatusCode.Accepted,
            (await client.PostAsJsonAsync("/api/auth/resend-confirmation", new
            {
                email
            })).StatusCode
        );
        Assert.AreEqual(1, Directory.GetFiles(mail).Length);
        // Remove consumed test evidence so later file-count checks observe only the next delivery.
        File.Delete(Directory.GetFiles(mail).Single());
        Assert.AreEqual(
            HttpStatusCode.NoContent,
            (await client.PostAsJsonAsync(
                "/api/auth/confirm-email",
                new
                {
                    userId = confirm.GetProperty("userId").GetGuid(),
                    token = confirm.GetProperty("token").GetString()
                }
            )).StatusCode
        );
    }

    /// <summary>
    /// Given a confirmed account, expect private reset delivery, rejection of invalid/reused tokens and
    /// one successful reset.
    /// </summary>
    private static async Task ResetPasswordAndCheckTokens(
        HttpClient client,
        string mail,
        string email,
        string newPassword
    )
    {
        Assert.AreEqual(
            HttpStatusCode.Accepted,
            (await client.PostAsJsonAsync("/api/auth/forgot-password", new
            {
                email
            })).StatusCode
        );
        using var resetFile = JsonDocument.Parse(await File.ReadAllTextAsync(Directory.GetFiles(mail).Single()));
        // The reset token is distinct from CSRF: it proves authority to change this account's password.
        var token = resetFile.RootElement.GetProperty("token").GetString();
        Assert.AreEqual("reset-password", resetFile.RootElement.GetProperty("purpose").GetString());
        Assert.AreEqual(
            HttpStatusCode.BadRequest,
            (await client.PostAsJsonAsync("/api/auth/reset-password", new
            {
                email,
                token = "invalid",
                newPassword
            })).StatusCode
        );
        Assert.AreEqual(
            HttpStatusCode.NoContent,
            (await client.PostAsJsonAsync("/api/auth/reset-password", new
            {
                email,
                token,
                newPassword
            })).StatusCode
        );
        Assert.AreEqual(
            HttpStatusCode.BadRequest,
            (await client.PostAsJsonAsync("/api/auth/reset-password", new
            {
                email,
                token,
                newPassword
            })).StatusCode
        );
    }

    /// <summary>
    /// Given the completed password reset, the old password must fail with 401 and the new password must
    /// succeed.
    /// Refresh the authenticated CSRF pair before the later contribution and closure writes.
    /// </summary>
    private static async Task VerifyOldAndNewPasswordLogin(
        HttpClient client, string email, string oldPassword, string newPassword)
    {
        Assert.AreEqual(
            HttpStatusCode.Unauthorized,
            (await client.PostAsJsonAsync("/api/auth/login", new
            {
                email,
                password = oldPassword
            })).StatusCode
        );
        Assert.AreEqual(
            HttpStatusCode.NoContent,
            (await client.PostAsJsonAsync("/api/auth/login", new
            {
                email,
                password = newPassword
            })).StatusCode
        );
        await RefreshCsrfToken(client);
    }

    /// <summary>
    /// Given the recovered session, create a post and comment so closure must preserve real linked
    /// contributions.
    /// </summary>
    private static async Task<(Guid ParticipantId, Guid NodeId)> CreateContributionsBeforeClosure(HttpClient client, DbContextOptions<AtlasDataContext> options)
    {
        var session = await client.GetFromJsonAsync<JsonElement>("/api/auth/session");
        var participantId = session.GetProperty("participantId").GetGuid();
        var type = NodeTypeDefinition.CreateSystemDefined("Closure idea", "Retained content", DateTimeOffset.UtcNow);
        new SqlNodeTypeRepository(() => new AtlasDataContext(options)).Save(type);
        var created = await client.PostAsJsonAsync(
            "/api/nodes",
            new
            {
                title = "Retained post",
                markdown = "Retained body",
                shortDescription = "A retained introduction",
                typeId = type.Id.Value,
                requestedTypeIds = Array.Empty<Guid>()
            }
        );
        Assert.AreEqual(HttpStatusCode.Created, created.StatusCode);
        var node = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        await Success(await client.PostAsJsonAsync($"/api/nodes/{node}/comments", new
        {
            body = "Retained comment"
        }));
        return (participantId, node);
    }

    /// <summary>
    /// Wrong-password closure returns 400; correct closure returns 204, removes credentials, and retains
    /// content under Deleted user.
    /// </summary>
    private static async Task CloseAccountAndVerifyRetainedContributions(
        HttpClient client,
        AtlasDataContext database,
        Guid participantId,
        Guid node,
        string newPassword
    )
    {
        Assert.AreEqual(
            HttpStatusCode.BadRequest,
            (await client.PostAsJsonAsync("/api/auth/delete-account", new
            {
                password = "wrong"
            })).StatusCode
        );
        Assert.AreEqual(
            HttpStatusCode.NoContent,
            (await client.PostAsJsonAsync("/api/auth/delete-account", new
            {
                password = newPassword
            })).StatusCode
        );
        Assert.AreEqual(
            HttpStatusCode.Unauthorized,
            (await client.GetAsync("/api/auth/session")).StatusCode
        );
        var tombstone = await client.GetFromJsonAsync<JsonElement>($"/api/participants/{participantId}");
        Assert.AreEqual("Deleted user", tombstone.GetProperty("displayName").GetString());
        Assert.IsTrue(tombstone.GetProperty("deleted").GetBoolean());
        Assert.IsTrue((await client.GetAsync($"/api/nodes/{node}")).IsSuccessStatusCode);
        var comments = await client.GetAsync($"/api/nodes/{node}/comments");
        Assert.IsTrue(comments.IsSuccessStatusCode);
        StringAssert.Contains(await comments.Content.ReadAsStringAsync(), "Retained comment");
        // Verify persisted state too: HTTP success alone does not prove credentials were removed.
        Assert.IsFalse(await database.Users.AnyAsync(u => u.Id == participantId));
        var profile = await database.ParticipantRows.AsNoTracking().SingleAsync(p => p.Id == participantId);
        Assert.IsFalse(profile.IsActive);
        Assert.AreEqual("Deleted user", profile.DisplayName);
        Assert.AreEqual(string.Empty, profile.Bio);
    }

    /// <summary>Fetch a token for the client’s current principal and pair it with the cookie retained by that client.</summary>
    private static async Task RefreshCsrfToken(HttpClient client)
    {
        var json = await client.GetFromJsonAsync<JsonElement>("/api/auth/csrf");
        client.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", json.GetProperty("token").GetString());
    }

    /// <summary>Require a successful HTTP status and include the response body if the assertion fails.</summary>
    private static async Task Success(HttpResponseMessage response) => Assert.IsTrue(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
}

using Atlas.Api;
using Atlas.Contracts.Observability;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Api.Tests;

/// <summary>Start the real Program pipeline inside an in-process test server, with test configuration.</summary>
/// <remarks>
/// WebApplicationFactory does not copy the API or contact the developer's running server.
/// Each factory owns a host/service provider; its clients exercise routing, middleware and controllers.
/// The primary-constructor parameter below is a C# constructor argument available to this class.
/// </remarks>
internal sealed class ApiFactory(string? connection = null) : WebApplicationFactory<Program>
{
    // Keep production registrations, overriding only environment, output and connection settings.
    // Non-SQL tests use an unreachable placeholder: accidental database access should fail.
    protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseEnvironment("Development")
        .UseSetting("Atlas:Telemetry:Enabled", "false")
        .UseSetting("ATLAS_SQL_CONNECTION_STRING", connection ?? "Server=localhost;Database=Atlas_Not_Used;Integrated Security=true;TrustServerCertificate=true");
    // The HTTPS address is logical in TestServer: no localhost port is opened.
    // Factory clients retain cookies, but redirects stay visible so tests can assert 401/403.
    public HttpClient Browser() => CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
}

[TestClass]
public sealed class BoundaryTests
{
    private sealed class HttpEventSink : ITelemetryObserver
    {
        public List<TelemetryEvent> Events { get; } = [];
        public void Record(TelemetryEvent value) => Events.Add(value);
    }

    [TestMethod]
    public async Task Early_rejection_has_API_origin_and_no_actor_or_target()
    {
        using var factory = new ApiFactory();
        var sink = new HttpEventSink();
        using var observer = Telemetry.UseObserver(sink);
        var context = new DefaultHttpContext();
        var middleware = new HttpTelemetryMiddleware(c =>
        {
            c.Response.StatusCode = 429;
            return Task.CompletedTask;
        });
        await middleware.InvokeAsync(context, factory.Services.GetRequiredService<ApiTelemetry>());
        var value = sink.Events.Single();
        Assert.AreEqual(EventKind.HttpRejected, value.Kind);
        Assert.AreEqual(TelemetryHost.Api, value.Host);
        Assert.AreEqual(EventReason.QuotaExceeded, value.ReasonCode);
        Assert.IsNull(value.ActorId);
        Assert.IsNull(value.TargetId);
        Assert.AreNotEqual(Guid.Empty, value.ObservabilityOperationId);
    }

    [TestMethod]
    public async Task Account_routes_reject_request_21_in_the_same_fixed_window_without_SQL()
    {
        using var factory = new ApiFactory(); using var client = factory.Browser();
        for (var i = 0; i < 20; i++)
            Assert.AreEqual(HttpStatusCode.OK, (await client.GetAsync("/api/auth/csrf")).StatusCode);
        Assert.AreEqual(HttpStatusCode.TooManyRequests, (await client.GetAsync("/api/auth/csrf")).StatusCode);
    }

    [TestMethod]
    public async Task Production_registration_is_unavailable_even_with_a_mail_override()
    {
        using var factory = new ApiFactory();
        using var production = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Production")
            .UseSetting("Atlas:DevelopmentMailDirectory", "unused-test-mail"));
        using var client = production.CreateClient(new WebApplicationFactoryClientOptions
            { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
        var csrf = await client.GetFromJsonAsync<System.Text.Json.JsonElement>("/api/auth/csrf");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", csrf.GetProperty("token").GetString());
        var response = await client.PostAsJsonAsync("/api/auth/register",
            new { email = "test@example.test", password = "Atlas_Test_Only!Password", displayName = "Test" });
        Assert.AreEqual(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [TestMethod]
    public async Task Anonymous_mutation_is_401_without_redirect_or_database_access()
    {
        using var factory = new ApiFactory(); using var client = factory.Browser();
        var response = await client.PostAsJsonAsync("/api/nodes", new { title="X" });
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.IsNull(response.Headers.Location);
    }
    [TestMethod]
    public async Task Login_requires_csrf_and_does_not_query_database_without_it()
    {
        using var factory = new ApiFactory(); using var client = factory.Browser();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email="test@example.test",password="not-a-real-password" });
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }
    [TestMethod]
    [DataRow("/api/auth/register")]
    [DataRow("/api/auth/confirm-email")]
    [DataRow("/api/auth/forgot-password")]
    [DataRow("/api/auth/reset-password")]
    [DataRow("/api/auth/resend-confirmation")]
    public async Task Anonymous_account_writes_reject_missing_and_invalid_csrf(string path)
    {
        using var factory = new ApiFactory(); using var client = factory.Browser();
        // These routes allow anonymous callers, but never bypass CSRF validation.
        var missing = await client.PostAsJsonAsync(path, new { });
        Assert.AreEqual(HttpStatusCode.BadRequest, missing.StatusCode);
        var problem = await missing.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        Assert.AreEqual("A valid CSRF token is required.", problem.GetProperty("title").GetString());
        await client.GetAsync("/api/auth/csrf");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", "invalid");
        var invalid = await client.PostAsJsonAsync(path, new { });
        Assert.AreEqual(HttpStatusCode.BadRequest, invalid.StatusCode);
        StringAssert.Contains(await invalid.Content.ReadAsStringAsync(), "A valid CSRF token is required.");
    }
    [TestMethod]
    public async Task Csrf_cookie_is_secure_and_http_only_and_api_does_not_serve_frontend()
    {
        using var factory = new ApiFactory(); using var client = factory.Browser();
        var response = await client.GetAsync("/api/auth/csrf"); response.EnsureSuccessStatusCode();
        var cookies = string.Join(";", response.Headers.GetValues("Set-Cookie"));
        StringAssert.Contains(cookies, "secure"); StringAssert.Contains(cookies, "httponly");
        StringAssert.Contains(await response.Content.ReadAsStringAsync(), "token");
        Assert.AreEqual(HttpStatusCode.NotFound, (await client.GetAsync("/")).StatusCode);
        Assert.IsTrue(response.Headers.CacheControl?.NoStore);
        Assert.IsTrue(response.Headers.CacheControl?.NoCache);
        CollectionAssert.Contains(response.Headers.Pragma.Select(x => x.Name).ToArray(), "no-cache");
    }
    [TestMethod]
    public async Task Malformed_json_and_invalid_csrf_are_rejected_before_identity_queries()
    {
        using var factory = new ApiFactory(); using var client = factory.Browser();
        var token = await client.GetFromJsonAsync<System.Text.Json.JsonElement>("/api/auth/csrf");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", token.GetProperty("token").GetString());
        var malformed = await client.PostAsync("/api/auth/login", new StringContent("{", System.Text.Encoding.UTF8, "application/json"));
        Assert.AreEqual(HttpStatusCode.BadRequest, malformed.StatusCode);
        client.DefaultRequestHeaders.Remove("X-CSRF-TOKEN"); client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", "invalid");
        Assert.AreEqual(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/auth/login", new { email="a@example.test",password="not-real" })).StatusCode);
    }
    [TestMethod]
    public async Task Account_request_bursts_are_throttled_even_when_inputs_are_rejected()
    {
        using var factory = new ApiFactory(); using var client = factory.Browser();
        var denied = false;
        for (var i = 0; i < 25; i++)
        {
            var response = await client.PostAsJsonAsync("/api/auth/login", new { });
            if (response.StatusCode == HttpStatusCode.TooManyRequests) { denied = true; break; }
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }
        Assert.IsTrue(denied, "The transport budget must include rejected attempts.");
    }
    [TestMethod]
    public void All_write_actions_have_authorization_except_explicit_account_entry_points()
    {
        using var factory = new ApiFactory();
        var actions = factory.Services.GetRequiredService<IActionDescriptorCollectionProvider>().ActionDescriptors.Items.OfType<ControllerActionDescriptor>();
        var writes = actions.Where(a => a.ActionConstraints?.OfType<Microsoft.AspNetCore.Mvc.ActionConstraints.HttpMethodActionConstraint>()
            .Any(c => c.HttpMethods.Any(m => m != "GET" && m != "HEAD")) == true);
        foreach (var action in writes)
        {
            Assert.IsTrue(action.ControllerTypeInfo.IsDefined(typeof(Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute), inherit: true),
                $"{action.DisplayName} must expose its antiforgery policy to reviewers and CodeQL.");
            var hasAuth = action.EndpointMetadata.OfType<Microsoft.AspNetCore.Authorization.IAuthorizeData>().Any();
            var anonymousAccountEntryPoints = new[]
            {
                "Register", "Login", "Confirm", "ForgotPassword", "ResetPassword", "ResendConfirmation"
            };
            var intentionalAnonymousWrite = action.ControllerName == "Accounts"
                && anonymousAccountEntryPoints.Contains(action.ActionName);
            Assert.IsTrue(hasAuth || intentionalAnonymousWrite, action.DisplayName);
        }
    }
}

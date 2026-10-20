using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Api.Tests;

/// <summary>Short, bounded adversarial checks; never direct these probes at a public server.</summary>
[TestClass]
public sealed class HttpAbuseTests
{
    [TestMethod]
    [DataRow("POST", "/api/nodes")]
    [DataRow("POST", "/api/communities")]
    [DataRow("PUT", "/api/participants/me")]
    [DataRow("PUT", "/api/nodes/11111111-1111-4111-8111-111111111111/vote")]
    [DataRow("POST", "/api/nodes/11111111-1111-4111-8111-111111111111/comments")]
    [DataRow("GET", "/api/moderation/queue")]
    [DataRow("POST", "/api/auth/delete-account")]
    public async Task Anonymous_protected_routes_reject_before_database_work(string method, string path)
    {
        using var factory = new ApiFactory();
        using var client = factory.Browser();
        using var request = new HttpRequestMessage(new HttpMethod(method), path);
        using var response = await client.SendAsync(request);
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.IsNull(response.Headers.Location);
    }

    [TestMethod]
    public async Task Csrf_token_from_another_cookie_jar_is_rejected()
    {
        using var factory = new ApiFactory();
        using var first = factory.Browser();
        using var second = factory.Browser();
        var stolen = await first.GetFromJsonAsync<JsonElement>("/api/auth/csrf");
        await second.GetAsync("/api/auth/csrf");
        second.DefaultRequestHeaders.Add("X-CSRF-TOKEN", stolen.GetProperty("token").GetString());
        using var rejected = await second.PostAsJsonAsync("/api/auth/login", new { email = "test@example.test", password = "not-used" });
        Assert.AreEqual(HttpStatusCode.BadRequest, rejected.StatusCode);
        StringAssert.Contains(await rejected.Content.ReadAsStringAsync(), "A valid CSRF token is required.");
    }

    [TestMethod]
    public async Task Foreign_origin_gets_no_CORS_grant_and_responses_keep_security_headers()
    {
        using var factory = new ApiFactory();
        using var client = factory.Browser();
        client.DefaultRequestHeaders.Add("Origin", "https://untrusted.example");
        using var response = await client.GetAsync("/api/auth/csrf");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsFalse(response.Headers.Contains("Access-Control-Allow-Origin"));
        Assert.IsFalse(response.Headers.Contains("Access-Control-Allow-Credentials"));
        Assert.AreEqual("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.AreEqual("no-referrer", response.Headers.GetValues("Referrer-Policy").Single());
        Assert.IsTrue(response.Headers.CacheControl?.NoCache == true);
        Assert.IsTrue(response.Headers.CacheControl?.NoStore == true);
        using var preflight = new HttpRequestMessage(HttpMethod.Options, "/api/auth/login");
        preflight.Headers.Add("Origin", "https://untrusted.example");
        preflight.Headers.Add("Access-Control-Request-Method", "POST");
        preflight.Headers.Add("Access-Control-Request-Headers", "X-CSRF-TOKEN, Content-Type");
        using var denial = await client.SendAsync(preflight);
        Assert.IsFalse(denial.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [TestMethod]
    public async Task HTTP_redirects_to_configured_HTTPS_and_production_sets_HSTS()
    {
        using var root = new ApiFactory();
        using var factory = root.WithWebHostBuilder(builder => builder.UseEnvironment("Production")
            .ConfigureServices(services => services.Configure<Microsoft.AspNetCore.HttpsPolicy.HttpsRedirectionOptions>(o => o.HttpsPort = 7240)));
        using var http = factory.CreateClient(new WebApplicationFactoryClientOptions
            { BaseAddress = new Uri("http://atlas.test"), AllowAutoRedirect = false });
        using var redirect = await http.GetAsync("/api/auth/csrf");
        Assert.AreEqual(HttpStatusCode.TemporaryRedirect, redirect.StatusCode);
        Assert.AreEqual("https://atlas.test:7240/api/auth/csrf", redirect.Headers.Location?.ToString());
        using var https = factory.CreateClient(new WebApplicationFactoryClientOptions
            { BaseAddress = new Uri("https://atlas.test"), AllowAutoRedirect = false });
        using var secure = await https.GetAsync("/api/auth/csrf");
        Assert.IsTrue(secure.Headers.Contains("Strict-Transport-Security"));
    }

    [TestMethod]
    public async Task Concurrency_limit_rejects_request_65_and_recovers_after_release()
    {
        using var root = new ApiFactory();
        using var factory = Probes(root);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var gate = factory.Services.GetRequiredService<AbuseGate>();
        var held = Enumerable.Range(0, 64).Select(_ => client.GetAsync("/__tests/abuse/hold")).ToArray();
        try
        {
            // Wait for real admitted handlers, rather than relying on timing or sleeps.
            await gate.Full.Task.WaitAsync(TimeSpan.FromSeconds(30));
            using var rejected = await client.GetAsync("/api/auth/csrf");
            Assert.AreEqual(HttpStatusCode.TooManyRequests, rejected.StatusCode);
        }
        finally
        {
            gate.Release.TrySetResult(true);
            var responses = await Task.WhenAll(held).WaitAsync(TimeSpan.FromSeconds(30));
            foreach (var response in responses) response.Dispose();
        }
        using var recovered = await client.GetAsync("/api/auth/csrf");
        Assert.AreEqual(HttpStatusCode.OK, recovered.StatusCode);
    }

    [TestMethod]
    public async Task Kestrel_rejects_known_length_and_chunked_oversized_bodies_and_accepts_the_boundary()
    {
        using var key = RSA.Create(2048);
        var certificateRequest = new CertificateRequest("CN=localhost", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var certificate = certificateRequest.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddMinutes(10));
        using var root = new ApiFactory();
        using var factory = Probes(root);
        factory.UseKestrel(options => options.Listen(IPAddress.Loopback, 0, endpoint => endpoint.UseHttps(certificate)));
        // Resolve Services to start the real socket server; TestServer cannot test Kestrel's body ceiling.
        var addresses = factory.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!;
        using var handler = new HttpClientHandler
        {
            // Trust only this test's ephemeral certificate, not arbitrary invalid server certificates.
            ServerCertificateCustomValidationCallback = (_, supplied, _, _) => supplied?.GetCertHashString() == certificate.GetCertHashString()
        };
        using var client = new HttpClient(handler) { BaseAddress = new Uri(addresses.Addresses.Single()), Timeout = TimeSpan.FromSeconds(30) };
        var csrf = await client.GetFromJsonAsync<JsonElement>("/api/auth/csrf");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", csrf.GetProperty("token").GetString());
        foreach (var chunked in new[] { false, true })
        {
            using HttpContent body = chunked ? new UnknownLengthBody(new byte[65_537]) : new ByteArrayContent(new byte[65_537]);
            using var rejected = await client.PostAsync("/__tests/abuse/body", body);
            Assert.AreEqual(HttpStatusCode.RequestEntityTooLarge, rejected.StatusCode);
            var problem = await rejected.Content.ReadAsStringAsync();
            StringAssert.Contains(problem, "Request body too large.");
            Assert.IsFalse(problem.Contains("stackTrace", StringComparison.OrdinalIgnoreCase));
        }
        using var allowed = await client.PostAsync("/__tests/abuse/body", new ByteArrayContent(new byte[65_536]));
        Assert.AreEqual(HttpStatusCode.NoContent, allowed.StatusCode);
    }

    private static WebApplicationFactory<Program> Probes(ApiFactory root) => root.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
    {
        services.AddSingleton<AbuseGate>();
        services.AddControllers().AddApplicationPart(typeof(AbuseProbeController).Assembly);
    }));

    private sealed class UnknownLengthBody(byte[] bytes) : HttpContent
    {
        protected override bool TryComputeLength(out long length) { length = 0; return false; }
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) => stream.WriteAsync(bytes).AsTask();
    }
}

public sealed class AbuseGate
{
    private int _entered;
    public TaskCompletionSource<bool> Full { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource<bool> Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public async Task Enter(CancellationToken cancellationToken)
    {
        if (Interlocked.Increment(ref _entered) == 64) Full.TrySetResult(true);
        await Release.Task.WaitAsync(cancellationToken);
    }
}

/// <summary>Test-assembly endpoints are added only to probe factories, never to the production API.</summary>
[ApiController, AutoValidateAntiforgeryToken, Route("__tests/abuse")]
public sealed class AbuseProbeController(AbuseGate gate) : ControllerBase
{
    [HttpGet("hold")]
    public async Task<IActionResult> Hold(CancellationToken cancellationToken)
    {
        await gate.Enter(cancellationToken);
        return NoContent();
    }

    [HttpPost("body")]
    public async Task<IActionResult> Body(CancellationToken cancellationToken)
    {
        await Request.Body.CopyToAsync(Stream.Null, cancellationToken);
        return NoContent();
    }
}

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
    /// <summary>
    /// Given an unsigned-in client requesting each protected route below, expect 401 with no login redirect.
    /// The factory uses an unreachable SQL placeholder, so reaching database work would fail this check.
    /// </summary>
    [TestMethod]
    [DataRow("POST", "/api/nodes")]
    [DataRow("PUT", "/api/nodes/11111111-1111-4111-8111-111111111111/short-description")]
    [DataRow("POST", "/api/communities")]
    [DataRow("PUT", "/api/participants/me")]
    [DataRow("PUT", "/api/nodes/11111111-1111-4111-8111-111111111111/vote")]
    [DataRow("PUT", "/api/comments/11111111-1111-4111-8111-111111111111/vote")]
    [DataRow("DELETE", "/api/comments/11111111-1111-4111-8111-111111111111/vote")]
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

    /// <summary>
    /// Given two independent clients, send the first client's request token with the second client's cookie.
    /// Expect 400: each request token must match its accompanying antiforgery cookie, even before login.
    /// </summary>
    [TestMethod]
    public async Task Csrf_token_from_another_cookie_jar_is_rejected()
    {
        // ApiFactory starts Atlas.Api's existing Program and middleware in a test host.
        // Browser() creates an HTTP client with its own retained cookies; it is not a browser UI.
        using var factory = new ApiFactory();
        using var first = factory.Browser();
        using var second = factory.Browser();
        // A cookie jar is the client's cookie storage: it retains Set-Cookie values and sends
        // matching cookies automatically. These clients have separate jars, like separate browser profiles.
        // The JSON "token" is the explicit request-token half; it is different from the HttpOnly cookie.
        var stolen = await first.GetFromJsonAsync<JsonElement>("/api/auth/csrf");
        await second.GetAsync("/api/auth/csrf");
        // Deliberately cross the pairs: second keeps its own cookie but sends first's JSON token.
        // "stolen" names this simulated misuse; the test does not actually steal a browser's credentials.
        second.DefaultRequestHeaders.Add("X-CSRF-TOKEN", stolen.GetProperty("token").GetString());
        // Rejection should happen at CSRF validation, before Identity evaluates the dummy credentials.
        using var rejected = await second.PostAsJsonAsync("/api/auth/login", new { email = "test@example.test", password = "not-used" });
        Assert.AreEqual(HttpStatusCode.BadRequest, rejected.StatusCode);
        StringAssert.Contains(await rejected.Content.ReadAsStringAsync(), "A valid CSRF token is required.");
    }

    /// <summary>
    /// Given requests declaring an untrusted origin, expect no cross-origin permission headers.
    /// The ordinary GET still returns 200 with security headers; CORS governs browser access, not all traffic.
    /// </summary>
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
        // Simulate the OPTIONS permission inquiry a browser sends before certain cross-origin writes.
        // HttpClient does not enforce browser CORS rules, so we inspect the permission headers ourselves.
        using var preflight = new HttpRequestMessage(HttpMethod.Options, "/api/auth/login");
        preflight.Headers.Add("Origin", "https://untrusted.example");
        preflight.Headers.Add("Access-Control-Request-Method", "POST");
        preflight.Headers.Add("Access-Control-Request-Headers", "X-CSRF-TOKEN, Content-Type");
        using var denial = await client.SendAsync(preflight);
        Assert.IsFalse(denial.Headers.Contains("Access-Control-Allow-Origin"));
    }

    /// <summary>
    /// Given Production settings and a known HTTPS port, expect HTTP to return a 307 HTTPS redirect.
    /// A logical HTTPS request should include HSTS. TestServer checks middleware behavior, not a TLS handshake.
    /// </summary>
    [TestMethod]
    public async Task HTTP_redirects_to_configured_HTTPS_and_production_sets_HSTS()
    {
        // Derive a test factory with different host configuration; production application code stays unchanged.
        // atlas.test avoids HSTS's default localhost exclusion. No DNS/socket connection is made by TestServer.
        using var root = new ApiFactory();
        using var factory = root.WithWebHostBuilder(builder => builder.UseEnvironment("Production")
            .ConfigureServices(services => services.Configure<Microsoft.AspNetCore.HttpsPolicy.HttpsRedirectionOptions>(o => o.HttpsPort = 7240)));
        // Disable automatic redirects so the assertion can see the initial 307 instead of following it.
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

    /// <summary>
    /// Given 64 requests held inside admitted handlers, expect the next request to receive 429.
    /// Release the held requests, then expect 200: permits must become available again after requests finish.
    /// </summary>
    [TestMethod]
    public async Task Concurrency_limit_rejects_request_65_and_recovers_after_release()
    {
        using var root = new ApiFactory();
        using var factory = Probes(root);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        // The test and its probe controller resolve the same singleton gate from this host.
        var gate = factory.Services.GetRequiredService<AbuseGate>();
        // Creating the tasks starts 64 overlapping HTTP requests; ToArray forces their enumeration.
        // Each handler asynchronously waits at the gate, consuming a request permit without busy-waiting.
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
            // Always unblock the handlers, even if an assertion fails, so the test can clean up.
            // Completing this shared signal lets all 64 awaiting handlers finish.
            gate.Release.TrySetResult(true);
            var responses = await Task.WhenAll(held).WaitAsync(TimeSpan.FromSeconds(30));
            foreach (var response in responses) response.Dispose();
        }
        // This final request distinguishes temporary saturation from a leaked concurrency permit.
        using var recovered = await client.GetAsync("/api/auth/csrf");
        Assert.AreEqual(HttpStatusCode.OK, recovered.StatusCode);
    }

    /// <summary>
    /// Given a real local Kestrel HTTPS server, send bodies one byte above its 64 KiB ceiling.
    /// Expect 413 for both known-length and chunked requests, but 204 for exactly 65,536 bytes.
    /// This tests the web server's byte limit independently of JSON parsing, domain validation or SQL.
    /// </summary>
    [TestMethod]
    public async Task Kestrel_rejects_known_length_and_chunked_oversized_bodies_and_accepts_the_boundary()
    {
        // Build temporary server TLS credentials entirely inside the test; no user supplies them.
        // RSA (Rivest-Shamir-Adleman) is an asymmetric algorithm. This creates a fresh 2048-bit
        // public/private key pair, not an "encrypted key". Here the private key signs the certificate.
        using var key = RSA.Create(2048);
        // CertificateRequest is a .NET object describing the certificate to create, not an incoming
        // HTTP request or a user's certificate upload. CN is the subject's common-name label.
        // SHA256 selects the signing digest; Pkcs1 selects RSA signature padding.
        var certificateRequest = new CertificateRequest("CN=localhost", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        // Self-signed means this test signs its own certificate instead of asking a trusted issuer.
        // The short validity window covers the test; the slightly earlier start tolerates clock skew.
        using var certificate = certificateRequest.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddMinutes(10));
        using var root = new ApiFactory();
        using var factory = Probes(root);
        // Kestrel is the web-server software hosting the API, not the host computer.
        // TestServer normally runs the middleware in memory and cannot enforce Kestrel-specific limits.
        // UseKestrel switches this factory to a real listener: loopback stays on this machine,
        // port 0 asks the OS for a free port, and UseHttps gives the listener our temporary certificate.
        factory.UseKestrel(options => options.Listen(IPAddress.Loopback, 0, endpoint => endpoint.UseHttps(certificate)));
        // Resolve Services to start the real socket server; TestServer cannot test Kestrel's body ceiling.
        var addresses = factory.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!;
        // HttpClientHandler owns transport concerns such as TLS certificate checks and cookie storage.
        // A newly self-signed test certificate is not normally trusted by the OS.
        using var handler = new HttpClientHandler
        {
            // During the TLS handshake .NET calls this function with request, certificate, chain and errors.
            // "_" parameters are intentionally unused; "supplied" is the certificate presented by Kestrel.
            // Return true only when its certificate fingerprint equals the certificate we just created.
            // This is a narrowly scoped test trust rule, not normal public CA/hostname/expiry validation.
            // Do not copy it into a production HTTP client or replace it with an always-true callback.
            ServerCertificateCustomValidationCallback = (_, supplied, _, _) => supplied?.GetCertHashString() == certificate.GetCertHashString()
        };
        // Use the actual address chosen by Kestrel. This client makes real HTTPS requests over loopback.
        // Its handler retains the Secure antiforgery cookie from the next response.
        using var client = new HttpClient(handler) { BaseAddress = new Uri(addresses.Addresses.Single()), Timeout = TimeSpan.FromSeconds(30) };
        // Supply valid CSRF first so a missing-token rejection does not mask the body-limit check.
        var csrf = await client.GetFromJsonAsync<JsonElement>("/api/auth/csrf");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", csrf.GetProperty("token").GetString());
        // ByteArrayContent advertises Content-Length. UnknownLengthBody withholds the length so
        // HTTP/1.1 streams chunks; the server must count bytes as it reads, not only inspect a header.
        foreach (var chunked in new[] { false, true })
        {
            using HttpContent body = chunked ? new UnknownLengthBody(new byte[65_537]) : new ByteArrayContent(new byte[65_537]);
            using var rejected = await client.PostAsync("/__tests/abuse/body", body);
            Assert.AreEqual(HttpStatusCode.RequestEntityTooLarge, rejected.StatusCode);
            var problem = await rejected.Content.ReadAsStringAsync();
            StringAssert.Contains(problem, "Request body too large.");
            Assert.IsFalse(problem.Contains("stackTrace", StringComparison.OrdinalIgnoreCase));
        }
        // A limit must reject excess data without rejecting a body exactly at the allowed boundary.
        using var allowed = await client.PostAsync("/__tests/abuse/body", new ByteArrayContent(new byte[65_536]));
        Assert.AreEqual(HttpStatusCode.NoContent, allowed.StatusCode);
    }

    /// <summary>
    /// Given the normal API factory, return a derived factory that also discovers test-only probe routes.
    /// The real middleware still applies; the added handlers provide controlled work without SQL access.
    /// </summary>
    // WithWebHostBuilder changes this test host's setup. ConfigureServices changes its DI registrations.
    // AddSingleton shares one gate between test and controller. AddApplicationPart tells MVC where
    // to discover AbuseProbeController; it does not write a controller into the production API project.
    private static WebApplicationFactory<Program> Probes(ApiFactory root) => root.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
    {
        services.AddSingleton<AbuseGate>();
        services.AddControllers().AddApplicationPart(typeof(AbuseProbeController).Assembly);
    }));

    /// <summary>Send bytes without a precomputed length so the body limit is tested while streaming.</summary>
    private sealed class UnknownLengthBody(byte[] bytes) : HttpContent
    {
        // false means the length is unknown; assigning zero only initializes the required out argument.
        protected override bool TryComputeLength(out long length) { length = 0; return false; }
        // Write the supplied bytes to the outgoing request stream when HttpClient sends the body.
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) => stream.WriteAsync(bytes).AsTask();
    }
}

/// <summary>
/// A test-owned rendezvous: signal when 64 handlers have entered, then hold them until the test releases them.
/// This coordinates requests to exercise the real limiter; it is not the limiter or a production security gate.
/// </summary>
public sealed class AbuseGate
{
    // Multiple requests arrive concurrently, so Interlocked updates this counter atomically.
    private int _entered;
    // TaskCompletionSource is a manually completed signal. The test awaits Full; handlers await Release.
    // RunContinuationsAsynchronously avoids running waiting code inline inside the signal setter.
    public TaskCompletionSource<bool> Full { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource<bool> Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    /// <summary>Count this admitted handler, signal capacity at 64, and await release or request cancellation.</summary>
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
    /// <summary>Given an admitted GET, keep it active at the gate; return 204 only after the test releases it.</summary>
    [HttpGet("hold")]
    public async Task<IActionResult> Hold(CancellationToken cancellationToken)
    {
        await gate.Enter(cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Given a CSRF-valid POST, read and discard its raw bytes so Kestrel must enforce its read-time limit.
    /// Return 204 if reading succeeds; oversized reads throw and the real API error handler returns 413.
    /// </summary>
    [HttpPost("body")]
    public async Task<IActionResult> Body(CancellationToken cancellationToken)
    {
        // Stream.Null discards data; no payload is stored, parsed as JSON, or written to the database.
        await Request.Body.CopyToAsync(Stream.Null, cancellationToken);
        return NoContent();
    }
}

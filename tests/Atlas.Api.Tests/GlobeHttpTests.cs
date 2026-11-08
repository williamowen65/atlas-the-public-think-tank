using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Atlas.Persistence.Globe;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Atlas.Api.Tests;

[TestClass]
public sealed class GlobeHttpTests
{
    [TestMethod]
    [DataRow("Development")]
    [DataRow("Production")]
    public async Task Real_event_stream_synchronizes_two_browsers_and_deduplicates_tabs(string environment)
    {
        using var factory = new GlobeApiFactory(environment);
        using var first = factory.CreateClient(new() { BaseAddress = new("https://localhost") });
        using var second = factory.CreateClient(new() { BaseAddress = new("https://localhost") });
        async Task Visit(HttpClient client)
        {
            var csrf = await client.GetFromJsonAsync<JsonElement>("/atlas-session-api/auth/csrf");
            client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", csrf.GetProperty("token").GetString());
            (await client.PostAsync("/atlas-session-api/globe/visit", null)).EnsureSuccessStatusCode();
        }
        await Visit(first);
        using var cancelled = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        using var response = await first.GetAsync("/atlas-session-api/globe/events", HttpCompletionOption.ResponseHeadersRead, cancelled.Token);
        using var stream = new StreamReader(await response.Content.ReadAsStreamAsync(cancelled.Token));
        Assert.AreEqual("text/event-stream", response.Content.Headers.ContentType?.MediaType);
        async Task<JsonElement> Until(int live)
        {
            while (true)
            {
                var line = await stream.ReadLineAsync(cancelled.Token);
                if (line is null) throw new InvalidOperationException("Stream ended early.");
                if (!line.StartsWith("data: ")) continue;
                var data = JsonSerializer.Deserialize<JsonElement>(line[6..]);
                if (data.GetProperty("live").GetProperty("visitors").GetInt32() == live) return data;
            }
        }
        await Until(1);
        using var tab = await first.GetAsync("/atlas-session-api/globe/events", HttpCompletionOption.ResponseHeadersRead, cancelled.Token);
        await Until(1);
        await Visit(second);
        using var other = await second.GetAsync("/atlas-session-api/globe/events", HttpCompletionOption.ResponseHeadersRead, cancelled.Token);
        var both = await Until(2);
        Assert.AreEqual(2, both.GetProperty("historical").GetProperty("visitors").GetInt32());
        (await second.PutAsJsonAsync("/atlas-session-api/globe/location", new { longitude = -122.432, latitude = 47.654 })).EnsureSuccessStatusCode();
        JsonElement shared;
        do { shared = await Until(2); } while (shared.GetProperty("live").GetProperty("pins").GetArrayLength() == 0);
        Assert.AreEqual(-122, shared.GetProperty("live").GetProperty("pins")[0].GetProperty("longitude").GetInt32());
        Assert.IsFalse(shared.ToString().Contains("VisitorId"));
        (await second.PutAsJsonAsync("/atlas-session-api/globe/location", new { longitude = (double?)null, latitude = (double?)null })).EnsureSuccessStatusCode();
        JsonElement removed;
        do { removed = await Until(2); } while (removed.GetProperty("historical").GetProperty("pins").GetArrayLength() != 0);
        cancelled.Cancel();
    }

    [TestMethod]
    public async Task Capacity_returns_429_with_retry_after_instead_of_an_unhandled_error()
    {
        using var factory = new GlobeApiFactory("Production");
        using var client = factory.CreateClient(new() { BaseAddress = new("https://localhost") });
        var csrf = await client.GetFromJsonAsync<JsonElement>("/atlas-session-api/auth/csrf");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", csrf.GetProperty("token").GetString());
        (await client.PostAsync("/atlas-session-api/globe/visit", null)).EnsureSuccessStatusCode();
        using var cancelled = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var streams = new List<HttpResponseMessage>();
        try
        {
            for (var i = 0; i < 8; i++)
                streams.Add(await client.GetAsync("/atlas-session-api/globe/events", HttpCompletionOption.ResponseHeadersRead, cancelled.Token));
            var rejected = await client.GetAsync("/atlas-session-api/globe/events");
            Assert.AreEqual(HttpStatusCode.TooManyRequests, rejected.StatusCode);
            Assert.AreEqual(TimeSpan.FromSeconds(5), rejected.Headers.RetryAfter?.Delta);
            var body = await rejected.Content.ReadFromJsonAsync<JsonElement>();
            Assert.AreEqual("globe_connection_limit", body.GetProperty("code").GetString());
        }
        finally
        {
            cancelled.Cancel();
            foreach (var response in streams) response.Dispose();
        }
    }

    [TestMethod]
    [DataRow("Development")]
    [DataRow("Production")]
    public async Task Visit_and_location_require_csrf_in_every_environment(string environment)
    {
        using var factory = new GlobeApiFactory(environment);
        using var client = factory.CreateClient(new() { BaseAddress = new("https://localhost") });
        Assert.AreEqual(HttpStatusCode.BadRequest, (await client.PostAsync("/atlas-session-api/globe/visit", null)).StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest, (await client.PutAsJsonAsync("/atlas-session-api/globe/location", new { longitude = 0, latitude = 0 })).StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest, (await client.GetAsync("/atlas-session-api/globe/events")).StatusCode);
        var csrf = await client.GetFromJsonAsync<JsonElement>("/atlas-session-api/auth/csrf");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", csrf.GetProperty("token").GetString());
        Assert.AreEqual(HttpStatusCode.OK, (await client.PostAsync("/atlas-session-api/globe/visit", null)).StatusCode);
        Assert.AreEqual(HttpStatusCode.NoContent, (await client.PutAsJsonAsync("/atlas-session-api/globe/location", new { longitude = 0, latitude = 0 })).StatusCode);
    }
}


// Boot the actual host in each environment without cloud exporters or database access.
internal sealed class GlobeApiFactory(string environment) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseEnvironment(environment)
        .UseSetting("Atlas:Telemetry:Enabled", "false")
        .UseSetting("ATLAS_SQL_CONNECTION_STRING", "Server=localhost;Database=Atlas_Not_Used;Integrated Security=true;TrustServerCertificate=true")
        .ConfigureServices(services =>
        {
            services.RemoveAll<IGlobeHistory>();
            services.AddSingleton<IGlobeHistory, MemoryGlobeHistory>();
        });
}

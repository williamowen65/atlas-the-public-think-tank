using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Atlas.Persistence.Globe;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Atlas.Api.Tests;

[TestClass]
public sealed class GlobeHttpTests
{
    [TestMethod]
    public async Task Real_event_stream_synchronizes_two_browsers_and_deduplicates_tabs()
    {
        using var baseFactory = new ApiFactory();
        using var factory = baseFactory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
        {
            services.RemoveAll<IGlobeHistory>();
            services.AddSingleton<IGlobeHistory, MemoryGlobeHistory>();
        }));
        using var first = factory.CreateClient(new() { BaseAddress = new("https://localhost") });
        using var second = factory.CreateClient(new() { BaseAddress = new("https://localhost") });
        async Task Visit(HttpClient client)
        {
            var csrf = await client.GetFromJsonAsync<JsonElement>("/api/auth/csrf");
            client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", csrf.GetProperty("token").GetString());
            (await client.PostAsync("/api/globe/visit", null)).EnsureSuccessStatusCode();
        }
        await Visit(first);
        using var cancelled = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        using var response = await first.GetAsync("/api/globe/events", HttpCompletionOption.ResponseHeadersRead, cancelled.Token);
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
        using var tab = await first.GetAsync("/api/globe/events", HttpCompletionOption.ResponseHeadersRead, cancelled.Token);
        await Until(1);
        await Visit(second);
        using var other = await second.GetAsync("/api/globe/events", HttpCompletionOption.ResponseHeadersRead, cancelled.Token);
        var both = await Until(2);
        Assert.AreEqual(2, both.GetProperty("historical").GetProperty("visitors").GetInt32());
        (await second.PutAsJsonAsync("/api/globe/location", new { longitude = -122.432, latitude = 47.654 })).EnsureSuccessStatusCode();
        JsonElement shared;
        do { shared = await Until(2); } while (shared.GetProperty("live").GetProperty("pins").GetArrayLength() == 0);
        Assert.AreEqual(-122, shared.GetProperty("live").GetProperty("pins")[0].GetProperty("longitude").GetInt32());
        Assert.IsFalse(shared.ToString().Contains("VisitorId"));
        (await second.PutAsJsonAsync("/api/globe/location", new { longitude = (double?)null, latitude = (double?)null })).EnsureSuccessStatusCode();
        JsonElement removed;
        do { removed = await Until(2); } while (removed.GetProperty("historical").GetProperty("pins").GetArrayLength() != 0);
        cancelled.Cancel();
    }

    [TestMethod]
    public async Task Visit_and_location_require_csrf_and_production_routes_are_disabled()
    {
        using var factory = new ApiFactory();
        using var client = factory.Browser();
        Assert.AreEqual(HttpStatusCode.BadRequest, (await client.PostAsync("/api/globe/visit", null)).StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest, (await client.PutAsJsonAsync("/api/globe/location", new { longitude = 0, latitude = 0 })).StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest, (await client.GetAsync("/api/globe/events")).StatusCode);
        var csrf = await client.GetFromJsonAsync<JsonElement>("/api/auth/csrf");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", csrf.GetProperty("token").GetString());
        factory.Services.GetRequiredService<IWebHostEnvironment>().EnvironmentName = "Production";
        Assert.AreEqual(HttpStatusCode.NotFound, (await client.PostAsync("/api/globe/visit", null)).StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, (await client.GetAsync("/api/globe/events")).StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, (await client.PutAsJsonAsync("/api/globe/location", new { longitude = 0, latitude = 0 })).StatusCode);
    }
}

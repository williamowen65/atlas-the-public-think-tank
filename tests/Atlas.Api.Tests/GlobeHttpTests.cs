using System.Net;
using System.Net.Http.Json;
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
    public async Task Anonymous_snapshot_is_finite_and_never_registers_a_visitor(string environment)
    {
        using var factory = new GlobeApiFactory(environment);
        using var client = factory.CreateClient(new() { BaseAddress = new("https://localhost") });
        var response = await client.GetAsync("/atlas-session-api/globe/snapshot");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual("application/json", response.Content.Headers.ContentType?.MediaType);
        var snapshot = await response.Content.ReadFromJsonAsync<GlobeView>();
        Assert.IsFalse(snapshot!.CanShare);
        Assert.AreEqual(0, snapshot.Historical.Visitors);
        Assert.IsFalse(response.Headers.TryGetValues("Set-Cookie", out _));
        Assert.AreEqual(0, factory.Services.GetRequiredService<MemoryGlobeHistory>().Rows.Count);
        Assert.AreEqual(HttpStatusCode.NotFound, (await client.GetAsync("/atlas-session-api/globe/events")).StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, (await client.PostAsync("/atlas-session-api/globe/visit", null)).StatusCode);
    }

    [TestMethod]
    public async Task Anonymous_pin_and_activity_writes_are_denied_even_with_a_csrf_token()
    {
        using var factory = new GlobeApiFactory("Production");
        using var client = factory.CreateClient(new() { BaseAddress = new("https://localhost") });
        var csrf = await client.GetFromJsonAsync<System.Text.Json.JsonElement>("/atlas-session-api/auth/csrf");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", csrf.GetProperty("token").GetString());
        Assert.AreEqual(HttpStatusCode.Unauthorized, (await client.PostAsync("/atlas-session-api/globe/activity", null)).StatusCode);
        Assert.AreEqual(HttpStatusCode.Unauthorized, (await client.PutAsJsonAsync("/atlas-session-api/globe/location", new { longitude = 10, latitude = 20 })).StatusCode);
        Assert.AreEqual(0, factory.Services.GetRequiredService<MemoryGlobeHistory>().Rows.Count);
    }
}

internal sealed class GlobeApiFactory(string environment) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseEnvironment(environment)
        .UseSetting("Atlas:Telemetry:Enabled", "false")
        .UseSetting("ATLAS_SQL_CONNECTION_STRING", "Server=localhost;Database=Atlas_Not_Used;Integrated Security=true;TrustServerCertificate=true")
        .ConfigureServices(services =>
        {
            services.RemoveAll<IGlobeHistory>();
            services.AddSingleton<MemoryGlobeHistory>();
            services.AddSingleton<IGlobeHistory>(provider => provider.GetRequiredService<MemoryGlobeHistory>());
        });
}

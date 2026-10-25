using System.Net;
using System.Text.Json;
using Atlas.Discovery;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Api.Tests;

[TestClass]
public sealed class SearchUnavailableTests
{
    [TestMethod]
    public async Task Search_dependency_failure_returns_safe_503_problem_instead_of_empty_results()
    {
        using var root = new ApiFactory();
        using var factory = root.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.AddSingleton<IDiscoveryReadQuery>(new UnavailableSearch())));
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
        var response = await client.GetAsync("/api/nodes?SearchText=salmon");
        Assert.AreEqual(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        using var problem = JsonDocument.Parse(body);
        Assert.AreEqual("search_unavailable", problem.RootElement.GetProperty("code").GetString());
        StringAssert.Contains(problem.RootElement.GetProperty("title").GetString()!, "Text search is temporarily unavailable");
        Assert.IsFalse(body.Contains("private SQL diagnostic", StringComparison.Ordinal));
    }

    private sealed class UnavailableSearch : IDiscoveryReadQuery
    {
        private static SearchUnavailableException Failure() => new(new InvalidOperationException("private SQL diagnostic"));
        public DiscoveryPage DiscoverPage(DiscoveryQuery query) => throw Failure();
        public DiscoveryPage DiscoverAuthoredPage(DiscoveryQuery query, IReadOnlyCollection<Guid> authoredNodeIds) => throw Failure();
        public IReadOnlyList<RankedDiscoveryItem> Discover(DiscoveryQuery query) => throw Failure();
        public IReadOnlyList<RankedDiscoveryItem> DiscoverAuthored(DiscoveryQuery query, IReadOnlyCollection<Guid> authoredNodeIds) => throw Failure();
    }
}

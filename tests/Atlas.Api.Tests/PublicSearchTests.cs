using System.Net;
using System.Text.Json;
using Atlas.Discovery;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Api.Tests;

[TestClass]
public sealed class PublicSearchTests
{
    [TestMethod]
    public async Task PublicSearchForwardsScopeAndPageAndCommentPathRedactsRemovedAncestors()
    {
        var port = new SearchPort();
        using var root = new ApiFactory();
        using var factory = root.WithWebHostBuilder(b => b.ConfigureServices(s => s.AddSingleton<IPublicSearch>(port)));
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        using var response = await client.GetAsync("/api/search?SearchText=salmon&scope=comments&Page=2");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual("comments", port.Scope); Assert.AreEqual("salmon", port.Query?.SearchText); Assert.AreEqual(2, port.Query?.Page);
        var node = Guid.NewGuid(); var comment = Guid.NewGuid();
        var body = await client.GetStringAsync($"/api/nodes/{node}/comments/{comment}/path");
        using var path = JsonDocument.Parse(body);
        Assert.AreEqual(JsonValueKind.Null, path.RootElement.GetProperty("items")[0].GetProperty("body").ValueKind);
        Assert.IsFalse(body.Contains("removed secret", StringComparison.Ordinal));
        port.Unavailable = true;
        using var failure = await client.GetAsync("/api/search?SearchText=salmon");
        Assert.AreEqual(HttpStatusCode.ServiceUnavailable, failure.StatusCode);
        StringAssert.Contains(await failure.Content.ReadAsStringAsync(), "search_unavailable");
    }

    private sealed class SearchPort : IPublicSearch
    {
        public DiscoveryQuery? Query; public string? Scope; public bool Unavailable;
        public PublicSearchPage Search(DiscoveryQuery query, string scope)
        {
            if (Unavailable) throw new SearchUnavailableException(new InvalidOperationException("private diagnostic"));
            Query = query; Scope = scope;
            return new([], 0, query.Page, 3);
        }
        public IReadOnlyList<CommentPathItem> CommentPath(Guid nodeId, Guid commentId) =>
            [new(commentId, "Node", nodeId, null, Guid.NewGuid(), "removed secret", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, true)];
    }
}

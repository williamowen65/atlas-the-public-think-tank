using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Atlas.Api.Tests;

[TestClass]
public sealed class PublicPageSeoTests
{
    internal static TempWebRoot CreateWebRoot() => new();

    [TestMethod]
    public async Task Demo_shell_has_public_metadata_but_no_indexing_or_sitemap()
    {
        using var web = CreateWebRoot();
        using var root = new ApiFactory();
        using var factory = root.WithWebHostBuilder(builder => builder.UseWebRoot(web.Path)
            .UseSetting("ATLAS_DEPLOYMENT_VARIANT", "demo")
            .UseSetting("Atlas:Seo:PublicOrigin", "https://demo.thepublicthinktank.com")
            .UseSetting("Atlas:Seo:IndexingEnabled", "true"));
        using var client = Client(factory, "https://demo.thepublicthinktank.com");

        var home = await client.GetAsync("/?sort=recent");
        Assert.AreEqual(HttpStatusCode.OK, home.StatusCode);
        Assert.AreEqual("noindex", home.Headers.GetValues("X-Robots-Tag").Single());
        var html = await home.Content.ReadAsStringAsync();
        StringAssert.Contains(html, "<title>Atlas | The Public Think Tank</title>");
        StringAssert.Contains(html, "rel=\"canonical\" href=\"https://demo.thepublicthinktank.com/\"");
        StringAssert.Contains(html, "property=\"og:image\" content=\"https://demo.thepublicthinktank.com/share-images/discover.png?v=7\"");
        StringAssert.Contains(html, "property=\"og:image:width\" content=\"1200\"");
        StringAssert.Contains(html, "property=\"og:image:height\" content=\"630\"");

        var globe = await client.GetAsync("/?globe=open");
        Assert.AreEqual("noindex", globe.Headers.GetValues("X-Robots-Tag").Single());
        var globeHtml = await globe.Content.ReadAsStringAsync();
        StringAssert.Contains(globeHtml, "property=\"og:title\" content=\"Put your pin on the Atlas globe\"");
        StringAssert.Contains(globeHtml, "property=\"og:url\" content=\"https://demo.thepublicthinktank.com/?globe=open\"");
        StringAssert.Contains(globeHtml, "property=\"og:image\" content=\"https://demo.thepublicthinktank.com/share-images/globe.png?v=1\"");

        var animatedGlobe = await client.GetStringAsync("/?globe=open&motion=gif");
        StringAssert.Contains(animatedGlobe, "property=\"og:url\" content=\"https://demo.thepublicthinktank.com/?globe=open&amp;motion=gif\"");
        StringAssert.Contains(animatedGlobe, "property=\"og:image\" content=\"https://demo.thepublicthinktank.com/share-images/globe.gif?v=2\"");
        StringAssert.Contains(animatedGlobe, "property=\"og:image:width\" content=\"960\"");
        StringAssert.Contains(animatedGlobe, "property=\"og:image:height\" content=\"504\"");

        var globeView = await client.GetStringAsync("/?globe=open&view=90");
        StringAssert.Contains(globeView, "property=\"og:url\" content=\"https://demo.thepublicthinktank.com/?globe=open&amp;view=90\"");
        StringAssert.Contains(globeView, "property=\"og:image\" content=\"https://demo.thepublicthinktank.com/share-images/globe.png?v=1&amp;view=90\"");

        var connections = await client.GetAsync("/share?design=connections");
        Assert.AreEqual(HttpStatusCode.OK, connections.StatusCode);
        Assert.AreEqual("noindex", connections.Headers.GetValues("X-Robots-Tag").Single());
        var connectionsHtml = await connections.Content.ReadAsStringAsync();
        StringAssert.Contains(connectionsHtml, "property=\"og:title\" content=\"See how ideas connect\"");
        StringAssert.Contains(connectionsHtml, "property=\"og:url\" content=\"https://demo.thepublicthinktank.com/share?design=connections\"");
        StringAssert.Contains(connectionsHtml, "property=\"og:image\" content=\"https://demo.thepublicthinktank.com/atlas-share-connections.png\"");
        Assert.AreEqual(HttpStatusCode.OK, (await client.GetAsync("/share")).StatusCode);

        var about = await client.GetStringAsync("/about");
        StringAssert.Contains(about, "<title>About Atlas | The Public Think Tank</title>");
        var robots = await client.GetStringAsync("/robots.txt");
        StringAssert.Contains(robots, "Allow: /");
        Assert.IsFalse(robots.Contains("Sitemap:", StringComparison.Ordinal));
        Assert.AreEqual(HttpStatusCode.NotFound, (await client.GetAsync("/sitemap.xml")).StatusCode);
        var legacy = await client.GetAsync("/index.html?sort=recent");
        Assert.AreEqual(HttpStatusCode.MovedPermanently, legacy.StatusCode);
        Assert.AreEqual("/", legacy.Headers.Location?.OriginalString);
    }

    [TestMethod]
    [DataRow(false, "/atlas-share.png")]
    [DataRow(true, "/share-images/globe.png?v=1")]
    public async Task Deployed_share_images_require_blob_storage(bool configureBlobStorage, string expectedImage)
    {
        using var web = CreateWebRoot();
        using var root = new ApiFactory();
        using var factory = root.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production").UseWebRoot(web.Path)
                .UseSetting("ATLAS_DEPLOYMENT_VARIANT", "demo")
                .UseSetting("Atlas:Seo:PublicOrigin", "https://demo.example.org")
                .UseSetting("Atlas:Seo:CaptureDirectory", web.Path)
                .UseSetting("BROWSERLESS_IO_API_KEY", "test-key");
            if (configureBlobStorage)
            {
                builder.UseSetting("Atlas:Seo:BlobContainerUrl", "https://example.blob.core.windows.net/share-images")
                    .UseSetting("Atlas:Seo:BlobIdentityClientId", "11111111-1111-1111-1111-111111111111");
            }
        });
        using var client = Client(factory, "https://demo.example.org");

        var html = await client.GetStringAsync("/?globe=open");
        StringAssert.Contains(html, $"property=\"og:image\" content=\"https://demo.example.org{expectedImage}\"");
        if (!configureBlobStorage)
            Assert.AreEqual(HttpStatusCode.NotFound, (await client.GetAsync("/share-images/globe.png")).StatusCode);
    }

    [TestMethod]
    public async Task Anonymous_private_links_redirect_to_feed_without_forwarding_queries()
    {
        using var web = CreateWebRoot();
        using var root = new ApiFactory();
        using var factory = root.WithWebHostBuilder(builder => builder.UseWebRoot(web.Path));
        using var client = Client(factory, "https://localhost");
        foreach (var path in new[] { "/my-drafts", "/my-settings", "/profiles/me", "/moderation" })
        {
            var response = await client.GetAsync(path + "?private=example");
            Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode, path);
            Assert.AreEqual("/", response.Headers.Location?.OriginalString);
            Assert.AreEqual("noindex", response.Headers.GetValues("X-Robots-Tag").Single());
        }

        var login = await client.GetAsync("/login");
        Assert.AreEqual(HttpStatusCode.OK, login.StatusCode);
        Assert.AreEqual("noindex", login.Headers.GetValues("X-Robots-Tag").Single());
        StringAssert.Contains(await login.Content.ReadAsStringAsync(),
            "property=\"og:url\" content=\"https://localhost/\"");
        Assert.AreEqual(HttpStatusCode.NotFound, (await client.GetAsync("/unknown-page")).StatusCode);
    }

    [TestMethod]
    public async Task Only_canonical_live_host_can_enable_indexing()
    {
        using var web = CreateWebRoot();
        using var root = new ApiFactory();
        using var factory = root.WithWebHostBuilder(builder => builder.UseWebRoot(web.Path)
            .UseSetting("ATLAS_DEPLOYMENT_VARIANT", "live")
            .UseSetting("Atlas:Seo:PublicOrigin", "https://atlas.example.org")
            .UseSetting("Atlas:Seo:RedirectHost", "www.example.org")
            .UseSetting("Atlas:Seo:IndexingEnabled", "true"));
        using var live = Client(factory, "https://atlas.example.org");
        using var www = Client(factory, "https://www.example.org");
        using var azure = Client(factory, "https://atlas-live.azurewebsites.net");

        var redirect = await www.GetAsync("/nodes?id=example&type=comments");
        Assert.AreEqual(HttpStatusCode.PermanentRedirect, redirect.StatusCode);
        Assert.AreEqual("https://atlas.example.org/nodes?id=example&type=comments",
            redirect.Headers.Location?.AbsoluteUri);
        StringAssert.Contains(await live.GetStringAsync("/robots.txt"),
            "Sitemap: https://atlas.example.org/sitemap.xml");
        Assert.IsFalse((await live.GetAsync("/")).Headers.Contains("X-Robots-Tag"));
        Assert.IsTrue((await azure.GetAsync("/")).Headers.Contains("X-Robots-Tag"));
        Assert.AreEqual(HttpStatusCode.NotFound, (await azure.GetAsync("/sitemap.xml")).StatusCode);
    }

    internal static HttpClient Client(WebApplicationFactory<Program> factory, string origin) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri(origin),
            AllowAutoRedirect = false
        });

    internal sealed class TempWebRoot : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "atlas-seo-" + Guid.NewGuid());

        public TempWebRoot()
        {
            Directory.CreateDirectory(Path);
            File.WriteAllText(System.IO.Path.Combine(Path, "index.html"), "<html><head></head><body>SPA</body></html>");
        }

        public void Dispose() => Directory.Delete(Path, recursive: true);
    }
}

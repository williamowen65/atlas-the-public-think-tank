using Microsoft.Playwright;

namespace Atlas.Api;

internal enum SharePage { Node, Community, Profile }

internal readonly record struct ShareTarget(SharePage Kind, Guid Id)
{
    internal string PagePath => Kind switch
    {
        SharePage.Community => $"/communities/{Id:D}",
        SharePage.Profile => $"/profiles?id={Id:D}",
        _ => $"/nodes?id={Id:D}"
    };

    internal string CapturePath => Kind == SharePage.Node ? PagePath + "&share=feed" : PagePath;

    internal string ImagePath => Kind switch
    {
        SharePage.Community => $"/share-images/communities/{Id:D}.png",
        SharePage.Profile => $"/share-images/profiles/{Id:D}.png",
        _ => $"/share-images/nodes/{Id:D}.png"
    };
    internal string Selector => Kind is SharePage.Node
        ? "#main-content [data-share-node] article.node-card"
        : "#main-content [data-share-preview]";
}

internal sealed class ShareImageCapture(
    IConfiguration configuration, IWebHostEnvironment environment, ILogger<ShareImageCapture> logger)
{
    // Bump the URL and disk cache together when the capture layout changes.
    private static string LayoutVersion(ShareTarget? target) => target is { Kind: SharePage.Node } ? "5" : "4";
    internal const int ImageWidth = 1200;
    internal const int ImageHeight = 630;
    private const string CaptureStyle = """
        /* A centered 16:9 crop of 1200 × 630 trims 40 pixels from each edge. */
        body { margin: 0 !important; padding: 0 4.67vw !important; box-sizing: border-box; }
        .app-shell, #main-content, #main-content .node-feed,
        #main-content .feed-intro,
        #main-content article.node-page, #main-content article.node-card {
            max-width: none !important;
            margin-inline: 0 !important;
        }
        #main-content .feed-intro { padding-inline: 1rem; }
        .app-shell > :not(main), .skip-link, [data-share-hide] { display: none !important; }
        """;

    private readonly object gate = new();
    private readonly Dictionary<string, Task> pending = new();
    private readonly SemaphoreSlim browserSlot = new(1);
    private readonly string? key = configuration["BROWSERLESS_IO_API_KEY"];
    private readonly Uri? origin = PublicPageSeo.ReadOrigin(environment.IsDevelopment()
        ? configuration["Atlas:Seo:CaptureOrigin"] ?? "https://localhost:7240"
        : configuration["Atlas:Seo:PublicOrigin"]);
    private readonly string? directory = ResolveDirectory(configuration, environment);

    internal bool Enabled => directory is not null && origin is not null &&
        (environment.IsDevelopment() ? origin.IsLoopback : !origin.IsLoopback && !string.IsNullOrWhiteSpace(key));

    internal static string ImagePath(ShareTarget? id) => (id?.ImagePath ?? "/share-images/discover.png") + "?v=" + LayoutVersion(id);

    internal async Task<IResult> Image(HttpContext context, ShareTarget? id)
    {
        if (!Enabled) return Results.NotFound();
        var revision = await PublicPageSeo.ShareRevision(context, id);
        if (revision is null) return Results.NotFound();

        var name = id is { } target ? $"{target.Kind}-{target.Id:D}" : "discover";
        if (id is { Kind: SharePage.Node } node) name = node.Id.ToString("D");
        var file = Path.Combine(directory!, $"v{LayoutVersion(id)}", name + ".png");
        if (Current(file, revision.Value)) return Results.File(file, "image/png");

        var job = Schedule(name, file, id);
        if (job is not null)
        {
            try { await job.WaitAsync(TimeSpan.FromSeconds(7), context.RequestAborted); }
            catch (TimeoutException) { /* The existing capture continues for later requests. */ }
        }

        // The node might have been edited or hidden while Chromium was rendering it.
        revision = await PublicPageSeo.ShareRevision(context, id);
        if (revision is null) return Results.NotFound();
        if (Current(file, revision.Value)) return Results.File(file, "image/png");

        context.Response.Headers.CacheControl = "no-store";
        var fallback = environment.WebRootFileProvider.GetFileInfo("atlas-share.png");
        return fallback.Exists ? Results.File(fallback.CreateReadStream(), "image/png") : Results.NotFound();
    }

    private Task? Schedule(string name, string file, ShareTarget? id)
    {
        lock (gate)
        {
            if (pending.TryGetValue(name, out var existing)) return existing;
            if (pending.Count >= 8) return null;
            var job = Task.Run(() => Capture(name, file, id));
            pending.Add(name, job);
            return job;
        }
    }

    private async Task Capture(string name, string file, ShareTarget? id)
    {
        await browserSlot.WaitAsync();
        var temporary = file + "." + Guid.NewGuid().ToString("N") + ".tmp";
        var stage = "starting Chromium";
        try
        {
            var started = DateTime.UtcNow;
            var url = new Uri(origin!, id?.CapturePath ?? "/");
            using var playwright = await Playwright.CreateAsync();
            await using var browser = environment.IsDevelopment()
                ? await playwright.Chromium.LaunchAsync(new()
                {
                    Timeout = 12000,
                    ExecutablePath = configuration["ATLAS_LOCAL_CHROMIUM"]
                })
                : await playwright.Chromium.ConnectOverCDPAsync(
                    "wss://production-sfo.browserless.io?token=" + Uri.EscapeDataString(key!),
                    new() { Timeout = 12000 });
            var scale = id is null ? 1f : 2.5f;
            var captureWidth = (int)(ImageWidth / scale);
            var captureHeight = (int)(ImageHeight / scale);
            await using var session = await browser.NewContextAsync(new()
            {
                ViewportSize = new() { Width = captureWidth, Height = 1000 },
                DeviceScaleFactor = scale,
                IgnoreHTTPSErrors = environment.IsDevelopment(),
                ServiceWorkers = ServiceWorkerPolicy.Block,
                ColorScheme = ColorScheme.Light
            });
            var page = await session.NewPageAsync();
            await page.RouteAsync("**/*", async route =>
            {
                var request = route.Request;
                if (request.Method == "GET" && Uri.TryCreate(request.Url, UriKind.Absolute, out var requestedUri))
                {
                    if (requestedUri.GetLeftPart(UriPartial.Authority) == origin!.GetLeftPart(UriPartial.Authority) &&
                        (!request.IsNavigationRequest || requestedUri == url) &&
                        !requestedUri.AbsolutePath.StartsWith("/atlas-session-api", StringComparison.OrdinalIgnoreCase))
                    {
                        await route.ContinueAsync();
                        return;
                    }
                    // Demo seed photos use this CDN. Never follow third-party redirects from the capture browser.
                    if (id is { Kind: SharePage.Node } && request.ResourceType == "image" &&
                        !request.IsNavigationRequest && requestedUri.GetLeftPart(UriPartial.Authority) == "https://images.unsplash.com" &&
                        requestedUri.AbsolutePath.StartsWith("/photo-", StringComparison.Ordinal) &&
                        requestedUri.Query == "?w=900&fit=max")
                    {
                        try
                        {
                            var photo = await route.FetchAsync(new() { MaxRedirects = 0, Timeout = 2000 });
                            try
                            {
                                if (photo.Ok && photo.Headers.TryGetValue("content-type", out var type) &&
                                    type.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                                {
                                    await route.FulfillAsync(new() { Response = photo });
                                    return;
                                }
                            }
                            finally { await photo.DisposeAsync(); }
                        }
                        catch (PlaywrightException) { /* A missing seed photo should not fail the entire capture. */ }
                    }
                }
                await route.AbortAsync();
            });
            stage = "loading the page";
            var response = await page.GotoAsync(url.AbsoluteUri, new() { Timeout = 12000, WaitUntil = WaitUntilState.DOMContentLoaded });
            if (response?.Ok != true || page.Url != url.AbsoluteUri)
            {
                logger.LogWarning("Atlas share capture for {Image} did not reach the requested page (HTTP {Status}).",
                    name, response?.Status);
                return;
            }

            stage = "waiting for the share card";
            var card = page.Locator(id?.Selector ?? "#main-content article.node-card").First;
            await card.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 12000 });
            await page.AddStyleTagAsync(new() { Content = CaptureStyle });
            if (id is { Kind: SharePage.Node })
            {
                stage = "waiting for the feed preview";
                await card.ScrollIntoViewIfNeededAsync();
                // Title-only cards can finish loading with a zero-height preview body.
                // The marker signals that loading settled, even when the body has no visible content.
                await card.Locator("[data-feed-preview-ready='true']")
                    .WaitForAsync(new() { State = WaitForSelectorState.Attached, Timeout = 12000 });
            }
            var top = 0;
            if (id is null)
            {
                await card.Locator("[data-feed-preview-ready='true']")
                    .WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 12000 });
                await page.EvaluateAsync("window.scrollTo({ top: 0, behavior: 'instant' })");
            }
            else
            {
                await card.EvaluateAsync("""
                    element => {
                        element.style.setProperty('scroll-margin-block-start', '0', 'important');
                        element.scrollIntoView({block: 'start', behavior: 'instant'});
                    }
                    """);
                var bounds = await card.BoundingBoxAsync();
                if (bounds is null) return;
                top = (int)Math.Ceiling(Math.Max(0, bounds.Y));
            }
            if (id is { Kind: SharePage.Node })
            {
                // The card can be visible while its lazy photos are still downloading.
                // Wait only for images that overlap the actual crop.
                await card.EvaluateAsync("""
                    async (element, crop) => {
                        const images = Array.from(element.querySelectorAll('img')).filter(image => {
                            const bounds = image.getBoundingClientRect();
                            return bounds.width > 0 && bounds.bottom > crop.top &&
                                bounds.top < crop.top + crop.height &&
                                bounds.right > 0 && bounds.left < window.innerWidth;
                        });
                        await Promise.race([
                            Promise.allSettled(images.map(image => {
                                image.loading = 'eager';
                                return image.decode();
                            })),
                            new Promise(resolve => setTimeout(resolve, 2500))
                        ]);
                    }
                    """, new { top, height = captureHeight });
            }
            stage = "writing the screenshot";
            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            var image = await page.ScreenshotAsync(new()
            {
                Clip = new() { X = 0, Y = top, Width = captureWidth, Height = captureHeight },
                Animations = ScreenshotAnimations.Disabled,
                Style = id is { Kind: not SharePage.Node }
                    ? "body * { visibility: hidden !important; } [data-share-preview], [data-share-preview] * { visibility: visible !important; }"
                    : null,
                Timeout = 12000
            });
            await File.WriteAllBytesAsync(temporary, image);
            File.SetLastWriteTimeUtc(temporary, started);
            File.Move(temporary, file, overwrite: true);
        }
        catch (Exception error)
        {
            // Browser connection errors can contain the credential-bearing URL.
            logger.LogWarning("Atlas share capture for {Image} failed while {Stage} ({ErrorType}).",
                name, stage, error.GetType().Name);
        }
        finally
        {
            try { File.Delete(temporary); }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException) { }
            finally
            {
                lock (gate) pending.Remove(name);
                browserSlot.Release();
            }
        }
    }

    private static bool Current(string path, DateTimeOffset revision)
    {
        var modified = File.GetLastWriteTimeUtc(path);
        return modified >= revision.UtcDateTime && modified >= DateTime.UtcNow.AddHours(-24) && File.Exists(path);
    }

    private static string? ResolveDirectory(IConfiguration config, IWebHostEnvironment environment)
    {
        var configured = config["Atlas:Seo:CaptureDirectory"];
        if (!string.IsNullOrWhiteSpace(configured)) return Path.GetFullPath(configured, environment.ContentRootPath);
        if (environment.IsDevelopment())
            return Path.GetFullPath("../Atlas.Web/.share-captures/server", environment.ContentRootPath);
        return OperatingSystem.IsLinux() ? "/home/data/atlas-share-captures" : null;
    }
}

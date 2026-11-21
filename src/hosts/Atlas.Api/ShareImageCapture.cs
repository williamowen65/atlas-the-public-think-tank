using Microsoft.Playwright;

namespace Atlas.Api;

internal enum SharePage { Node, Community, Profile, Globe }

internal readonly record struct ShareTarget(SharePage Kind, Guid Id)
{
    internal static ShareTarget Globe => new(SharePage.Globe, Guid.Empty);

    internal string PagePath => Kind switch
    {
        SharePage.Globe => "/?globe=open",
        SharePage.Community => $"/communities/{Id:D}",
        SharePage.Profile => $"/profiles?id={Id:D}",
        _ => $"/nodes?id={Id:D}"
    };

    internal string CapturePath => Kind == SharePage.Node ? PagePath + "&share=feed" : PagePath;

    internal string ImagePath => Kind switch
    {
        SharePage.Globe => "/share-images/globe.png",
        SharePage.Community => $"/share-images/communities/{Id:D}.png",
        SharePage.Profile => $"/share-images/profiles/{Id:D}.png",
        _ => $"/share-images/nodes/{Id:D}.png"
    };
    internal string Selector => Kind switch
    {
        SharePage.Node => "#main-content [data-share-node] article.node-card",
        SharePage.Globe => "dialog[data-globe-share][open]",
        _ => "#main-content [data-share-preview]"
    };
}

internal sealed class ShareImageCapture(
    IConfiguration configuration, IWebHostEnvironment environment, ILogger<ShareImageCapture> logger)
{
    // Bump the URL and disk cache together when the capture layout changes.
    private static string LayoutVersion(ShareTarget? target) => target switch
    {
        null => "6",
        { Kind: SharePage.Node } => "8",
        { Kind: SharePage.Globe } => "1",
        _ => "4"
    };
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
        .skip-link, [data-share-hide] { display: none !important; }
        """;
    private const string DiscoverCaptureStyle = """
        .app-shell > :not(main, .atlas-header, .mini-launcher) { display: none !important; }
        .app-shell > .mini-launcher { right: 4.67vw !important; }
        """;
    private const string GlobeCaptureStyle = """
        .app-shell > :not([data-globe-share]) { display: none !important; }
        dialog[data-globe-share] {
            inset: 25px 56px !important;
            box-sizing: border-box;
            width: auto !important;
            height: auto !important;
            margin: 0 !important;
            padding: 32px !important;
            display: grid !important;
            grid-template-columns: 1fr 1fr;
            grid-template-rows: auto 1fr auto;
            align-items: center;
            gap: 0 32px;
            --globe-size: 415px !important;
        }
        [data-globe-share] > .toolbar { grid-area: 1 / 1; }
        [data-globe-share] > .world { grid-area: 2 / 1; margin: auto; }
        [data-globe-share] > .globe-caption { grid-area: 3 / 1; justify-content: center; }
        [data-globe-share] > .globe-actions,
        [data-globe-share] > .feedback,
        [data-globe-share] .close,
        [data-globe-share] .connection { display: none !important; }
        [data-globe-invite] { display: block !important; grid-area: 1 / 2 / 4; padding-inline: 12px; }
        [data-globe-invite] strong { color: var(--atlas-accent); font-size: 19px; }
        [data-globe-invite] h2 { font-size: 54px; line-height: 1.04; margin: 18px 0; }
        [data-globe-invite] p { font-size: 23px; line-height: 1.4; }
        """;

    private readonly object gate = new();
    private readonly Dictionary<string, Task> pending = new();
    private readonly SemaphoreSlim browserSlot = new(1);
    private readonly string? key = configuration["BROWSERLESS_IO_API_KEY"];
    private readonly Uri? origin = PublicPageSeo.ReadOrigin(environment.IsDevelopment()
        ? configuration["Atlas:Seo:CaptureOrigin"] ?? "https://localhost:7240"
        : configuration["Atlas:Seo:PublicOrigin"]);
    private readonly ShareImageCache? cache = ShareImageCache.Create(configuration, environment);

    internal bool Enabled => cache is not null && origin is not null &&
        (environment.IsDevelopment() ? origin.IsLoopback : !origin.IsLoopback && !string.IsNullOrWhiteSpace(key));

    internal static string ImagePath(ShareTarget? id) => (id?.ImagePath ?? "/share-images/discover.png") + "?v=" + LayoutVersion(id);

    internal async Task<IResult> Image(HttpContext context, ShareTarget? id)
    {
        if (!Enabled) return Results.NotFound();
        var revision = await PublicPageSeo.ShareRevision(context, id);
        if (revision is null) return Results.NotFound();

        var name = id is { } target ? $"{target.Kind}-{target.Id:D}" : "discover";
        if (id is { Kind: SharePage.Node } node) name = node.Id.ToString("D");
        if (id is { Kind: SharePage.Globe }) name = "globe";
        var cacheKey = $"v{LayoutVersion(id)}/{name}.png";
        if (await ReadCached(cacheKey, revision.Value, context.RequestAborted) is { } cached) return cached;

        var job = Schedule(name, cacheKey, id);
        if (job is not null)
        {
            try { await job.WaitAsync(TimeSpan.FromSeconds(7), context.RequestAborted); }
            catch (TimeoutException) { /* The existing capture continues for later requests. */ }
        }

        // The node might have been edited or hidden while Chromium was rendering it.
        revision = await PublicPageSeo.ShareRevision(context, id);
        if (revision is null) return Results.NotFound();
        if (await ReadCached(cacheKey, revision.Value, context.RequestAborted) is { } generated) return generated;

        context.Response.Headers.CacheControl = "no-store";
        var fallback = environment.WebRootFileProvider.GetFileInfo("atlas-share.png");
        return fallback.Exists ? Results.File(fallback.CreateReadStream(), "image/png") : Results.NotFound();
    }

    private async Task<IResult?> ReadCached(string cacheKey, DateTimeOffset revision, CancellationToken cancellationToken)
    {
        try
        {
            return await cache!.Read(cacheKey, revision, cancellationToken);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            logger.LogWarning("Atlas share image cache read failed ({ErrorType}).", error.GetType().Name);
            return null;
        }
    }

    private Task? Schedule(string name, string cacheKey, ShareTarget? id)
    {
        lock (gate)
        {
            if (pending.TryGetValue(name, out var existing)) return existing;
            if (pending.Count >= 8) return null;
            var job = Task.Run(() => Capture(name, cacheKey, id));
            pending.Add(name, job);
            return job;
        }
    }

    private async Task Capture(string name, string cacheKey, ShareTarget? id)
    {
        await browserSlot.WaitAsync();
        var stage = "starting Chromium";
        try
        {
            var started = DateTime.UtcNow;
            var url = new Uri(origin!, id?.CapturePath ?? "/");
            var globe = id is { Kind: SharePage.Globe };
            var fullFrame = id is null || globe;
            using var playwright = await Playwright.CreateAsync();
            await using var browser = environment.IsDevelopment()
                ? await playwright.Chromium.LaunchAsync(new()
                {
                    Timeout = 12000,
                    ExecutablePath = configuration["ATLAS_LOCAL_CHROMIUM"],
                    Args = globe ? ["--use-angle=swiftshader", "--enable-unsafe-swiftshader"] : null
                })
                : await playwright.Chromium.ConnectOverCDPAsync(
                    "wss://production-sfo.browserless.io?token=" + Uri.EscapeDataString(key!),
                    new() { Timeout = 12000 });
            var scale = fullFrame ? 1f : 2.5f;
            var captureWidth = (int)(ImageWidth / scale);
            var captureHeight = (int)(ImageHeight / scale);
            await using var session = await browser.NewContextAsync(new()
            {
                ViewportSize = new() { Width = captureWidth, Height = fullFrame ? captureHeight : 1000 },
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
                        (!requestedUri.AbsolutePath.StartsWith("/atlas-session-api", StringComparison.OrdinalIgnoreCase) ||
                            (globe && requestedUri.AbsolutePath == "/atlas-session-api/globe/snapshot")))
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
            await page.AddStyleTagAsync(new()
            {
                Content = CaptureStyle + (id is null
                    ? DiscoverCaptureStyle
                    : globe ? GlobeCaptureStyle : ".app-shell > :not(main) { display: none !important; }")
            });
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
                await card.ScrollIntoViewIfNeededAsync();
                await card.Locator("[data-feed-preview-ready='true']")
                    .WaitForAsync(new() { State = WaitForSelectorState.Attached, Timeout = 12000 });
                await page.EvaluateAsync("window.scrollTo({ top: 0, behavior: 'instant' })");
            }
            else if (globe)
            {
                stage = "waiting for the globe renderer";
                await card.Locator(".world [data-map-ready='true'] canvas[data-ready='true']")
                    .WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 12000 });
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
            var image = await page.ScreenshotAsync(new()
            {
                Clip = new() { X = 0, Y = top, Width = captureWidth, Height = captureHeight },
                Animations = ScreenshotAnimations.Disabled,
                Style = id is { Kind: SharePage.Community or SharePage.Profile }
                    ? "body * { visibility: hidden !important; } [data-share-preview], [data-share-preview] * { visibility: visible !important; }"
                    : null,
                Timeout = 12000
            });
            await cache!.Write(cacheKey, image, started);
        }
        catch (Exception error)
        {
            // Browser connection errors can contain the credential-bearing URL.
            logger.LogWarning("Atlas share capture for {Image} failed while {Stage} ({ErrorType}).",
                name, stage, error.GetType().Name);
        }
        finally
        {
            lock (gate) pending.Remove(name);
            browserSlot.Release();
        }
    }
}

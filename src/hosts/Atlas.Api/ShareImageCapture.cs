using Microsoft.Playwright;
using ImageMagick;

namespace Atlas.Api;

internal enum SharePage { Node, Community, Profile, Globe }

internal readonly record struct ShareTarget(SharePage Kind, Guid Id, bool Animated = false, int? GlobeAngle = null)
{
    internal static ShareTarget Globe => new(SharePage.Globe, Guid.Empty);
    internal static ShareTarget GlobeAnimation => Globe with { Animated = true };

    internal static ShareTarget GlobeView(string? view) => view switch
    {
        "0" => Globe with { GlobeAngle = 0 },
        "90" => Globe with { GlobeAngle = 90 },
        "180" => Globe with { GlobeAngle = 180 },
        "270" => Globe with { GlobeAngle = 270 },
        _ => Globe
    };

    private string GlobeQuery => Animated ? "&motion=gif" : GlobeAngle is { } angle ? $"&view={angle}" : "";

    internal string PagePath => Kind switch
    {
        SharePage.Globe => "/?globe=open" + GlobeQuery,
        SharePage.Community => $"/communities/{Id:D}",
        SharePage.Profile => $"/profiles?id={Id:D}",
        _ => $"/nodes?id={Id:D}"
    };

    internal string CapturePath => Kind switch
    {
        SharePage.Node => PagePath + "&share=feed",
        SharePage.Globe when Animated || GlobeAngle is not null => PagePath + "&capture=globe",
        _ => PagePath
    };

    internal string ImagePath => Kind switch
    {
        SharePage.Globe => Animated ? "/share-images/globe.gif" : "/share-images/globe.png",
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
        null => "7",
        { Kind: SharePage.Node } => "9",
        { Kind: SharePage.Globe, Animated: true } => "5",
        { Kind: SharePage.Globe } => "1",
        _ => "4"
    };
    internal const int ImageWidth = 1200;
    internal const int ImageHeight = 630;
    internal const int AnimationWidth = 960;
    internal const int AnimationHeight = 504;
    private const float AnimationScale = (float)AnimationWidth / ImageWidth;
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

    internal static string ImagePath(ShareTarget? id) =>
        (id?.ImagePath ?? "/share-images/discover.png") + "?v=" + LayoutVersion(id) +
        (id?.GlobeAngle is { } angle ? $"&view={angle}" : "");

    internal async Task<IResult> Image(HttpContext context, ShareTarget? id)
    {
        if (!Enabled) return Results.NotFound();
        var revision = await PublicPageSeo.ShareRevision(context, id);
        if (revision is null) return Results.NotFound();

        var name = id is { } target ? $"{target.Kind}-{target.Id:D}" : "discover";
        if (id is { Kind: SharePage.Node } node) name = node.Id.ToString("D");
        if (id is { Kind: SharePage.Globe })
            name = id.Value.Animated ? "globe-motion" : id.Value.GlobeAngle is { } angle ? $"globe-{angle}" : "globe";
        var extension = id is { Animated: true } ? "gif" : "png";
        var cacheKey = $"v{LayoutVersion(id)}/{name}.{extension}";
        if (await ReadCached(cacheKey, revision.Value, context.RequestAborted) is { } cached) return cached;

        var job = Schedule(name, cacheKey, id);
        if (job is not null)
        {
            try { await job.WaitAsync(TimeSpan.FromSeconds(id is { Animated: true } ? 20 : 7), context.RequestAborted); }
            catch (TimeoutException) { /* The existing capture continues for later requests. */ }
        }

        // The node might have been edited or hidden while Chromium was rendering it.
        revision = await PublicPageSeo.ShareRevision(context, id);
        if (revision is null) return Results.NotFound();
        if (await ReadCached(cacheKey, revision.Value, context.RequestAborted) is { } generated) return generated;

        context.Response.Headers.CacheControl = "no-store";
        if (id is { Animated: true } &&
            await ReadCached($"v{LayoutVersion(ShareTarget.Globe)}/globe.png", revision.Value, context.RequestAborted) is { } still)
            return still;
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
                DeviceScaleFactor = id is { Animated: true } ? AnimationScale : scale,
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
            var screenshot = new PageScreenshotOptions
            {
                Clip = new() { X = 0, Y = top, Width = captureWidth, Height = captureHeight },
                Animations = ScreenshotAnimations.Disabled,
                Style = id is { Kind: SharePage.Community or SharePage.Profile }
                    ? "body * { visibility: hidden !important; } [data-share-preview], [data-share-preview] * { visibility: visible !important; }"
                    : null,
                Timeout = 12000
            };
            var image = id is { Animated: true }
                ? await CaptureGlobeAnimation(page, screenshot)
                : await page.ScreenshotAsync(screenshot);
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

    private static async Task<byte[]> CaptureGlobeAnimation(IPage page, PageScreenshotOptions screenshot)
    {
        const int frames = 240;
        // Match the globe's default 3° per second. GIF delay is measured in centiseconds.
        const int frameDelay = 120 * 100 / frames;
        await page.Locator("dialog[data-globe-share][data-globe-pins-ready='true']")
            .WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 12000 });

        var canvas = page.Locator("dialog[data-globe-share] .world canvas");
        var bounds = await canvas.BoundingBoxAsync() ??
            throw new InvalidOperationException("The globe canvas has no capture bounds.");
        var x = (int)Math.Round(bounds.X * AnimationScale);
        var y = (int)Math.Round(bounds.Y * AnimationScale);
        var width = (uint)Math.Round(bounds.Width * AnimationScale);
        var height = (uint)Math.Round(bounds.Height * AnimationScale);

        // Hide the transparent WebGL canvas without changing the layout or its backing buffer.
        screenshot.Style = "dialog[data-globe-share] .world canvas { visibility: hidden !important; }";
        using var background = new MagickImage(await page.ScreenshotAsync(screenshot));
        using var globeBackground = background.Clone();
        globeBackground.Crop(new MagickGeometry(x, y, width, height));
        globeBackground.ResetPage();

        using var animation = new MagickImageCollection();
        const int batchSize = 12;
        for (var start = 0; start < frames; start += batchSize)
        {
            var angles = Enumerable.Range(start, Math.Min(batchSize, frames - start))
                .Select(index => -25 + index * 360.0 / frames).ToArray();
            var captures = await canvas.EvaluateAsync<string[]>("""
                (canvas, angles) => {
                    const frames = [];
                    canvas.dispatchEvent(new CustomEvent('atlas-globe-capture-frames', { detail: { angles, frames } }));
                    return frames;
                }
                """, angles);
            if (captures.Length != angles.Length)
                throw new InvalidOperationException("The globe renderer did not return every requested frame.");

            foreach (var capture in captures)
            {
                using var globe = new MagickImage(Convert.FromBase64String(capture));
                var first = animation.Count == 0;
                var frame = first ? background.Clone() : globeBackground.Clone();
                animation.Add(frame);
                frame.Composite(globe, first ? x : 0, first ? y : 0, CompositeOperator.Over);
                frame.AnimationDelay = frameDelay;
                frame.GifDisposeMethod = GifDisposeMethod.None;
                if (first)
                    frame.AnimationIterations = 0;
                else
                    frame.Page = new MagickGeometry(x, y, background.Width, background.Height);
            }
        }

        using var output = new MemoryStream();
        await animation.WriteAsync(output, MagickFormat.Gif);
        return output.ToArray();
    }
}

using System.Diagnostics;
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
        SharePage.Globe => PagePath + "&capture=globe",
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
        { Kind: SharePage.Globe, Animated: true } => "14",
        { Kind: SharePage.Globe } => "3",
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
            grid-template-columns: repeat(2, minmax(0, 1fr));
            grid-template-rows: 1fr;
            align-items: center;
            gap: 0 32px;
        }
        [data-globe-share] > .world {
            grid-area: 1 / 1;
            place-self: center;
            width: min(100%, 480px) !important;
            margin: 0 !important;
        }
        [data-globe-share] > .toolbar,
        [data-globe-share] > .globe-caption,
        [data-globe-share] > .globe-actions,
        [data-globe-share] > .feedback,
        [data-globe-share] .close,
        [data-globe-share] .connection { display: none !important; }
        [data-globe-invite] { display: block !important; grid-area: 1 / 2; padding-inline: 12px; }
        [data-globe-invite] strong { color: var(--atlas-accent); font-size: 21px; font-weight: 800; }
        [data-globe-invite] h2 { font-size: 54px; font-weight: 800; line-height: 1.04; margin: 18px 0; }
        [data-globe-invite] p { font-size: 25px; font-weight: 700; line-height: 1.4; }
        [data-globe-invite] span {
            display: inline-block;
            margin-top: 12px;
            padding: 10px 18px;
            border: 2px solid var(--atlas-accent);
            border-radius: 999px;
            color: var(--atlas-accent);
            font-size: 22px;
            font-weight: 800;
        }
        """;

    private readonly object gate = new();
    private readonly Dictionary<string, Task> pending = new();
    private readonly SemaphoreSlim browserSlot = new(1);
    private readonly string? key = configuration["BROWSERLESS_IO_API_KEY"];
    private readonly Uri? origin = PublicPageSeo.ReadOrigin(environment.IsDevelopment()
        ? configuration["Atlas:Seo:CaptureOrigin"] ?? "https://localhost:7240"
        : configuration["Atlas:Seo:PublicOrigin"]);
    private readonly ShareImageCache? cache = ShareImageCache.Create(configuration, environment);

    private bool GlobeWorkerEnabled => configuration.GetValue<bool>("Atlas:Seo:ExternalGlobeCapture");

    internal bool CanServe(ShareTarget? target) =>
        GlobeWorkerEnabled && target is { Kind: SharePage.Globe } ? cache is not null : Enabled;

    internal bool Enabled => cache is not null && origin is not null &&
        (environment.IsDevelopment() ? origin.IsLoopback : !origin.IsLoopback && !string.IsNullOrWhiteSpace(key));

    internal static string ImagePath(ShareTarget? id) =>
        (id?.ImagePath ?? "/share-images/discover.png") + "?v=" + LayoutVersion(id) +
        (id?.GlobeAngle is { } angle ? $"&view={angle}" : "");

    internal async Task<string> PreviewPath(ShareTarget? target, CancellationToken cancellationToken)
    {
        if (!GlobeWorkerEnabled || target is not { Kind: SharePage.Globe })
            return ImagePath(target);

        var version = await ReadGlobeVersion(cancellationToken);
        return version is null ? ImagePath(target) : ImagePath(target) + "&generation=" + version;
    }

    private static string GlobeImageName(ShareTarget target) =>
        target.Animated ? "globe-motion.gif" : target.GlobeAngle is { } angle ? $"globe-{angle}.png" : "globe.png";

    private async Task<string?> ReadGlobeVersion(CancellationToken cancellationToken)
    {
        try
        {
            return cache is null ? null : await cache.ReadGlobeVersion(cancellationToken);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            logger.LogWarning("Could not read the published globe preview version ({ErrorType}).", error.GetType().Name);
            return null;
        }
    }

    internal async Task<bool> PublishGlobe()
    {
        if (cache is null || origin is null)
        {
            logger.LogError("Configure the globe capture origin and output storage.");
            return false;
        }

        var version = Guid.NewGuid().ToString("N");
        var targets = new[] { ShareTarget.Globe, ShareTarget.GlobeView("0"), ShareTarget.GlobeView("90"),
            ShareTarget.GlobeView("180"), ShareTarget.GlobeView("270"), ShareTarget.GlobeAnimation };
        var previous = await cache.ReadGlobeVersion(CancellationToken.None);
        foreach (var target in targets)
        {
            var name = GlobeImageName(target);
            if (!await Capture(name, $"globe-published/{version}/{name}", target, worker: true))
                return false;
        }

        if (!await cache.PublishGlobeVersion(version, previous))
        {
            logger.LogWarning("Another globe execution published while this worker was running.");
            return false;
        }

        logger.LogInformation("Published globe preview generation {Generation}.", version);
        return true;
    }

    internal async Task<IResult> Image(HttpContext context, ShareTarget? id)
    {
        if (GlobeWorkerEnabled && id is { Kind: SharePage.Globe } globe)
        {
            if (cache is null) return Results.NotFound();
            var requested = context.Request.Query["generation"].ToString();
            if (requested.Length > 0 && !ShareImageCache.IsGlobeVersion(requested)) return Results.NotFound();
            var version = requested.Length == 0 ? await ReadGlobeVersion(context.RequestAborted) : requested;
            if (version is not null && await ReadCached($"globe-published/{version}/{GlobeImageName(globe)}",
                    DateTimeOffset.MinValue, context.RequestAborted) is { } published)
                return published;

            // Keep serving the previous layout while waiting for the first worker publication.
            if (await ReadCached($"v{LayoutVersion(globe)}/{GlobeImageName(globe)}",
                    DateTimeOffset.MinValue, context.RequestAborted) is { } previous)
                return previous;
            context.Response.Headers.CacheControl = "no-store";
            var placeholder = environment.WebRootFileProvider.GetFileInfo("atlas-share.png");
            return placeholder.Exists ? Results.File(placeholder.CreateReadStream(), "image/png") : Results.NotFound();
        }

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
            logger.LogWarning("Atlas share image cache read for {CacheKey} failed ({ErrorType}, root cause {RootErrorType}).",
                cacheKey, error.GetType().Name, error.GetBaseException().GetType().Name);
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

    private async Task<bool> Capture(string name, string cacheKey, ShareTarget? id, bool worker = false)
    {
        await browserSlot.WaitAsync();
        var stage = "starting Chromium";
        try
        {
            var started = DateTime.UtcNow;
            if (worker) logger.LogInformation("Capturing globe preview {Image}.", name);
            var url = new Uri(origin!, id?.CapturePath ?? "/");
            var globe = id is { Kind: SharePage.Globe };
            var fullFrame = id is null || globe;
            var elapsed = Stopwatch.StartNew();
            GlobeFrames? globeFrames = null;
            byte[]? image = null;
            var sessionCount = id is { Animated: true } ? 4 : 1;
            // Four local sessions must cover all 770 frames of the 11 FPS trial.
            var framesPerSession = worker && environment.IsDevelopment() ? 193 : 180;
            var browserStartupTimeout = id is { Animated: true } ? 25000 : 12000;
            string? globeSnapshot = null;
            for (var sessionIndex = 0; sessionIndex < sessionCount; sessionIndex++)
            {
                stage = id is { Animated: true }
                    ? $"starting Chromium for globe GIF session {sessionIndex + 1}/{sessionCount}"
                    : "starting Chromium";
                var sessionElapsed = Stopwatch.StartNew();
                using var playwright = await Playwright.CreateAsync();
                await using var browser = worker || environment.IsDevelopment()
                    ? await playwright.Chromium.LaunchAsync(new()
                    {
                        Timeout = 12000,
                        ExecutablePath = configuration["ATLAS_LOCAL_CHROMIUM"],
                        Args = globe ? ["--use-angle=swiftshader", "--enable-unsafe-swiftshader"] : null
                    })
                    : await playwright.Chromium.ConnectOverCDPAsync(
                        "wss://production-sfo.browserless.io?token=" + Uri.EscapeDataString(key!),
                        new() { Timeout = browserStartupTimeout });
                stage = "creating capture context";
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
                stage = "creating capture page";
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
                            if (id is { Animated: true } && requestedUri.AbsolutePath == "/atlas-session-api/globe/snapshot")
                            {
                                if (globeSnapshot is null)
                                {
                                    var snapshot = await route.FetchAsync(new() { MaxRedirects = 0, Timeout = 10000 });
                                    try
                                    {
                                        if (!snapshot.Ok)
                                        {
                                            await route.FulfillAsync(new() { Response = snapshot });
                                            return;
                                        }
                                        globeSnapshot = await snapshot.TextAsync();
                                    }
                                    finally { await snapshot.DisposeAsync(); }
                                }
                                await route.FulfillAsync(new() { Body = globeSnapshot, ContentType = "application/json" });
                                return;
                            }
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
                var response = await page.GotoAsync(url.AbsoluteUri, new() { Timeout = browserStartupTimeout, WaitUntil = WaitUntilState.DOMContentLoaded });
                if (response?.Ok != true || page.Url != url.AbsoluteUri)
                {
                    logger.LogWarning("Atlas share capture for {Image} did not reach the requested page (HTTP {Status}).",
                        name, response?.Status);
                    return false;
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
                    if (bounds is null) return false;
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
                if (id is { Animated: true })
                {
                    stage = $"capturing globe frames in session {sessionIndex + 1}/{sessionCount}";
                    globeFrames = await CaptureGlobeFrames(page, screenshot, globeFrames,
                        sessionIndex * framesPerSession, framesPerSession, sessionCount * framesPerSession);
                    logger.LogInformation("Atlas globe GIF session {Session}/{SessionCount} captured {FrameCount} frames in {ElapsedMs} ms.",
                        sessionIndex + 1, sessionCount, framesPerSession, sessionElapsed.ElapsedMilliseconds);
                }
                else
                {
                    image = await page.ScreenshotAsync(screenshot);
                }
            }

            if (globeFrames is not null)
            {
                logger.LogInformation("Atlas globe GIF captured {FrameCount} frames in {ElapsedMs} ms.",
                    globeFrames.Frames.Count, elapsed.ElapsedMilliseconds);
                stage = "compositing and encoding globe GIF batches";
                elapsed.Restart();
                image = worker
                    ? await EncodeShareableGlobe(globeFrames, elapsed)
                    : await EncodeGlobeFrames(globeFrames, elapsed);
                logger.LogInformation("Atlas globe GIF encoded in {ElapsedMs} ms ({ImageBytes} bytes).",
                    elapsed.ElapsedMilliseconds, image.Length);
            }

            stage = "uploading the share image";
            elapsed.Restart();
            if (globeFrames is not null)
                logger.LogInformation("Atlas globe GIF upload started for {CacheKey} ({ImageBytes} bytes).",
                    cacheKey, image!.Length);
            await cache!.Write(cacheKey, image!, started);
            if (globeFrames is not null)
                logger.LogInformation("Atlas globe GIF saved as {CacheKey} in {ElapsedMs} ms.",
                    cacheKey, elapsed.ElapsedMilliseconds);
            return true;
        }
        catch (Exception error)
        {
            // Browser connection errors can contain the credential-bearing URL.
            logger.LogWarning("Atlas share capture for {Image} failed while {Stage} ({ErrorType}).",
                name, stage, error.GetType().Name);
            return false;
        }
        finally
        {
            lock (gate) pending.Remove(name);
            browserSlot.Release();
        }
    }

    private sealed record GlobeFrames(byte[] Background, LocatorBoundingBoxResult Bounds, Clip Clip, List<string> Frames);

    private static async Task<GlobeFrames> CaptureGlobeFrames(IPage page, PageScreenshotOptions screenshot,
        GlobeFrames? previous, int firstFrame, int sessionFrameCount, int totalFrameCount)
    {
        await page.Locator("dialog[data-globe-share][data-globe-pins-ready='true']")
            .WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 12000 });

        var canvas = page.Locator("dialog[data-globe-share] .world canvas");
        const int globeSize = 480;
        if (previous is null)
        {
            // Match the still image's left column in the 1200 × 630 viewport.
            var bounds = new LocatorBoundingBoxResult { X = 92.5f, Y = 75, Width = globeSize, Height = globeSize };
            screenshot.Style = "dialog[data-globe-share] .world canvas { visibility: hidden !important; }";
            var background = await page.ScreenshotAsync(screenshot);
            previous = new GlobeFrames(background, bounds, screenshot.Clip!, new List<string>(totalFrameCount));
        }

        var endFrame = firstFrame + sessionFrameCount;
        const int batchSize = 24;
        for (var start = firstFrame; start < endFrame; start += batchSize)
        {
            var angles = Enumerable.Range(start, Math.Min(batchSize, endFrame - start))
                .Select(index => -25 + index * 360.0 / totalFrameCount).ToArray();
            var captures = await canvas.EvaluateAsync<string[]>("""
                (canvas, { angles, size }) => {
                    const frames = [];
                    canvas.dispatchEvent(new CustomEvent('atlas-globe-capture-frames', { detail: { angles, frames, size } }));
                    return frames;
                }
                """, new { angles, size = globeSize });
            if (captures.Length != angles.Length)
                throw new InvalidOperationException("The globe renderer did not return every requested frame.");

            previous.Frames.AddRange(captures);
        }
        return previous;
    }

    private const int BaselineGlobeLoopSeconds = 120;

    private async Task<byte[]> EncodeShareableGlobe(GlobeFrames capture, Stopwatch elapsed)
    {
        const int loopSeconds = 70;
        const int byteBudget = 7_000_000;
        var comparison = environment.IsDevelopment() ? Guid.NewGuid().ToString("N") : null;
        var baseline = new[] { (Colors: 128, Loss: 20) };
        var frameRates = comparison is null ? new[] { 8, 7, 6 } : new[] { 11, 10, 9 };
        byte[]? selected = null;
        var selectedFps = 0;
        var selectedSettings = baseline[0];

        foreach (var fps in frameRates)
        {
            var encoded = await EncodeGlobeFrames(capture, elapsed, loopSeconds, fps, preserveFrames: true);
            var settings = (comparison is not null, fps) switch
            {
                (true, 11) => new[] { (Colors: 128, Loss: 95), (Colors: 128, Loss: 100), (Colors: 128, Loss: 110) },
                (true, 10) => new[] { (Colors: 128, Loss: 75), (Colors: 128, Loss: 80), (Colors: 128, Loss: 85) },
                (true, 9) => new[] { (Colors: 128, Loss: 70) },
                _ => baseline
            };
            foreach (var (colors, loss) in settings)
            {
                var optimized = await GlobeGifOptimizer.Optimize(encoded, colors: colors, loss: loss);
                logger.LogInformation(
                    "Globe GIF candidate: {LoopSeconds} s at {Fps} FPS, {Colors} colors, lossy {Loss}, {SizeMb:F2} MB. Target under 7 MB, Facebook limit 8 MB.",
                    loopSeconds, fps, colors, loss, optimized.Length / 1_000_000d);

                if (comparison is not null)
                {
                    var path = $"globe-comparison/{comparison}/globe-70s-{fps}fps-{colors}colors-lossy{loss}.gif";
                    await cache!.Write(path, optimized, DateTimeOffset.UtcNow);
                    logger.LogInformation("Saved local globe GIF comparison: {Path}.", path);
                }

                if (selected is null && optimized.Length < byteBudget)
                {
                    selected = optimized;
                    selectedFps = fps;
                    selectedSettings = (colors, loss);
                }
            }

            if (comparison is null && selected is not null) break;
        }

        if (selected is null)
            throw new InvalidOperationException("No 70-second globe GIF met the 7 MB publication budget.");

        logger.LogInformation("Selected globe GIF: {LoopSeconds} s at {Fps} FPS, {Colors} colors, lossy {Loss}, {SizeMb:F2} MB.",
            loopSeconds, selectedFps, selectedSettings.Colors, selectedSettings.Loss, selected.Length / 1_000_000d);
        return selected;
    }

    private async Task<byte[]> EncodeGlobeFrames(GlobeFrames capture, Stopwatch elapsed, int loopSeconds = BaselineGlobeLoopSeconds, int fps = 6, bool preserveFrames = false)
    {
        // Match the globe's default 3° per second. GIF delay is measured in centiseconds.
        var loopDuration = loopSeconds * 100;
        var previousFrameEnd = 0;
        using var background = new MagickImage(capture.Background);
        background.Alpha(AlphaAction.Off);

        var clip = capture.Clip;
        var scaleX = background.Width / (double)clip.Width;
        var scaleY = background.Height / (double)clip.Height;
        var canvasX = (capture.Bounds.X - clip.X) * scaleX;
        var canvasY = (capture.Bounds.Y - clip.Y) * scaleY;
        var canvasWidth = (uint)Math.Max(1, Math.Round(capture.Bounds.Width * scaleX));
        var canvasHeight = (uint)Math.Max(1, Math.Round(capture.Bounds.Height * scaleY));

        // Cover the entire canvas (including its pin margin) plus a few pixels for rounding.
        const int padding = 4;
        var left = Math.Max(0, (int)Math.Floor(canvasX) - padding);
        var top = Math.Max(0, (int)Math.Floor(canvasY) - padding);
        var right = Math.Min((int)background.Width, (int)Math.Ceiling(canvasX + capture.Bounds.Width * scaleX) + padding);
        var bottom = Math.Min((int)background.Height, (int)Math.Ceiling(canvasY + capture.Bounds.Height * scaleY) + padding);
        using var globeBackground = background.Clone();
        globeBackground.Crop(new MagickGeometry(left, top, (uint)(right - left), (uint)(bottom - top)));
        globeBackground.ResetPage();

        using var output = new MemoryStream();
        using var encodedBatch = new MemoryStream();
        const int batchSize = 24;
        var frameCount = Math.Min(capture.Frames.Count, loopSeconds * fps);
        for (var batchStart = 0; batchStart < frameCount; batchStart += batchSize)
        {
            using var animation = new MagickImageCollection();
            var batchEnd = Math.Min(batchStart + batchSize, frameCount);
            for (var frameIndex = batchStart; frameIndex < batchEnd; frameIndex++)
            {
                // Sample across the whole rotation, including when the stride is fractional.
                var sourceIndex = frameIndex * capture.Frames.Count / frameCount;
                var frameData = capture.Frames[sourceIndex];
                if (!preserveFrames) capture.Frames[sourceIndex] = string.Empty;
                using var globe = new MagickImage(Convert.FromBase64String(frameData));
                if (globe.Width != canvasWidth || globe.Height != canvasHeight)
                    globe.Resize(new MagickGeometry(canvasWidth, canvasHeight) { IgnoreAspectRatio = true });
                var first = frameIndex == 0;
                var frame = first ? background.Clone() : globeBackground.Clone();
                animation.Add(frame);
                var x = (int)Math.Round(canvasX);
                var y = (int)Math.Round(canvasY);
                frame.Composite(globe, first ? x : x - left, first ? y : y - top, CompositeOperator.Over);
                frame.Alpha(AlphaAction.Off);
                // Distribute rounding across frames. Sampling speeds up the full rotation.
                var frameEnd = (frameIndex + 1) * loopDuration / frameCount;
                frame.AnimationDelay = (uint)(frameEnd - previousFrameEnd);
                previousFrameEnd = frameEnd;
                frame.GifDisposeMethod = GifDisposeMethod.None;
                if (first)
                    frame.AnimationIterations = 0;
                else
                    frame.Page = new MagickGeometry(left, top, background.Width, background.Height);

            }

            encodedBatch.SetLength(0);
            await animation.WriteAsync(encodedBatch, MagickFormat.Gif);
            GifFrameBatches.Append(output, encodedBatch.GetBuffer().AsSpan(0, (int)encodedBatch.Length), batchStart == 0);
            if (batchEnd % 120 == 0 || batchEnd == frameCount)
                logger.LogInformation(
                    "Atlas globe GIF compositing and encoding progress {FramesDone}/{FrameCount} in {ElapsedMs} ms ({WorkingSetMiB} MiB process memory).",
                    batchEnd, frameCount, elapsed.ElapsedMilliseconds, Environment.WorkingSet / (1024 * 1024));
        }

        output.WriteByte(0x3b); // GIF trailer
        return output.ToArray();
    }
}

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

    internal string ImagePath => Kind switch
    {
        SharePage.Community => $"/share-images/communities/{Id:D}.png",
        SharePage.Profile => $"/share-images/profiles/{Id:D}.png",
        _ => $"/share-images/nodes/{Id:D}.png"
    };
    internal string Selector => Kind is SharePage.Node
        ? "#main-content article.node-page"
        : "#main-content [data-share-preview]";
}

internal sealed class ShareImageCapture(
    IConfiguration configuration, IWebHostEnvironment environment, ILogger<ShareImageCapture> logger)
{
    private readonly object gate = new();
    private readonly Dictionary<string, Task> pending = new();
    private readonly SemaphoreSlim browserSlot = new(1);
    private readonly string? key = configuration["BROWSERLESS_IO_API_KEY"];
    private readonly Uri? origin = PublicPageSeo.ReadOrigin(environment.IsDevelopment()
        ? configuration["Atlas:Seo:CaptureOrigin"] ?? "http://127.0.0.1:5173"
        : configuration["Atlas:Seo:PublicOrigin"]);
    private readonly string? directory = ResolveDirectory(configuration, environment);

    internal bool Enabled => directory is not null && origin is not null &&
        (environment.IsDevelopment() ? origin.IsLoopback : !origin.IsLoopback && !string.IsNullOrWhiteSpace(key));

    internal static string ImagePath(ShareTarget? id) => id?.ImagePath ?? "/share-images/discover.png";

    internal async Task<IResult> Image(HttpContext context, ShareTarget? id)
    {
        if (!Enabled) return Results.NotFound();
        var revision = await PublicPageSeo.ShareRevision(context, id);
        if (revision is null) return Results.NotFound();

        var name = id is { } target ? $"{target.Kind}-{target.Id:D}" : "discover";
        if (id is { Kind: SharePage.Node } node) name = node.Id.ToString("D");
        var file = Path.Combine(directory!, name + ".png");
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

        var home = Path.Combine(directory!, "discover.png");
        if (id is not null && Current(home, (await PublicPageSeo.ShareRevision(context, null))!.Value))
            return Results.File(home, "image/png");
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
        try
        {
            var started = DateTime.UtcNow;
            var url = new Uri(origin!, id?.PagePath ?? "/");
            using var playwright = await Playwright.CreateAsync();
            await using var browser = environment.IsDevelopment()
                ? await playwright.Chromium.LaunchAsync(new() { Timeout = 12000 })
                : await playwright.Chromium.ConnectOverCDPAsync(
                    "wss://production-sfo.browserless.io?token=" + Uri.EscapeDataString(key!),
                    new() { Timeout = 12000 });
            await using var session = await browser.NewContextAsync(new()
            {
                ViewportSize = new() { Width = id is null ? 1200 : 480, Height = 1000 },
                DeviceScaleFactor = id is null ? 1 : 2.5f,
                IgnoreHTTPSErrors = environment.IsDevelopment(),
                ServiceWorkers = ServiceWorkerPolicy.Block,
                ColorScheme = ColorScheme.Light
            });
            var page = await session.NewPageAsync();
            await page.RouteAsync("**/*", route =>
            {
                var request = route.Request;
                if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var requestedUri) ||
                    requestedUri.GetLeftPart(UriPartial.Authority) != origin!.GetLeftPart(UriPartial.Authority) ||
                    request.Method != "GET" ||
                    (request.IsNavigationRequest && requestedUri != url) ||
                    requestedUri.AbsolutePath.StartsWith("/atlas-session-api", StringComparison.OrdinalIgnoreCase))
                    return route.AbortAsync();
                return route.ContinueAsync();
            });
            var response = await page.GotoAsync(url.AbsoluteUri, new() { Timeout = 12000, WaitUntil = WaitUntilState.DOMContentLoaded });
            if (response?.Ok != true || page.Url != url.AbsoluteUri) return;

            var card = page.Locator(id?.Selector ?? "#main-content article.node-card").First;
            await card.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 12000 });
            if (id is not null)
                await card.EvaluateAsync("element => element.scrollIntoView({block: 'start', behavior: 'instant'})");
            var top = id is null ? 0 : (int)Math.Ceiling(Math.Max(0, (await card.BoundingBoxAsync())?.Y ?? 0));
            Directory.CreateDirectory(directory!);
            var image = await page.ScreenshotAsync(new()
            {
                Clip = new() { X = 0, Y = top, Width = id is null ? 1200 : 480, Height = id is null ? 630 : 252 },
                Animations = ScreenshotAnimations.Disabled,
                Style = id switch
                {
                    null => null,
                    { Kind: SharePage.Node } => ".app-shell > :not(main) { visibility: hidden; }",
                    _ => "body * { visibility: hidden !important; } [data-share-preview], [data-share-preview] * { visibility: visible !important; }"
                },
                Timeout = 12000
            });
            await File.WriteAllBytesAsync(temporary, image);
            File.SetLastWriteTimeUtc(temporary, started);
            File.Move(temporary, file, overwrite: true);
        }
        catch (Exception error)
        {
            // Browser connection errors can contain the credential-bearing URL.
            logger.LogWarning("Atlas share capture for {Image} failed ({ErrorType}).", name, error.GetType().Name);
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

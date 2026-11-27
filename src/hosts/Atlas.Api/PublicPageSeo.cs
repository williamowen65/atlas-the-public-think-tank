using Atlas.Communities.Communities;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Xml.Linq;
using Atlas.Api.Controllers;
using Atlas.Content.Blocks;
using Atlas.Content.Documents;
using Atlas.Graph.Nodes;
using Atlas.Moderation;
using Atlas.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Api;

internal sealed class PublicPageSeo(IConfiguration configuration)
{
    private const string ShareTitle = "Atlas | The Public Think Tank";
    private const string HomeDescription =
        "Explore connected questions, evidence, and ideas in Atlas, the Public Think Tank.";
    private const string AboutDescription =
        "Discover Atlas, an experiment in organizing questions, evidence, experiences, and ideas into a public think tank.";
    private const string NoIndexHeader = "X-Robots-Tag";
    private static readonly XNamespace SitemapNamespace = "http://www.sitemaps.org/schemas/sitemap/0.9";

    private readonly Uri? origin = ReadOrigin(configuration["Atlas:Seo:PublicOrigin"]);
    private readonly bool live = configuration["ATLAS_DEPLOYMENT_VARIANT"] == "live";
    private readonly bool indexingEnabled = configuration.GetValue<bool>("Atlas:Seo:IndexingEnabled");

    private readonly string? redirectHost = configuration["Atlas:Seo:RedirectHost"];

    private bool HasProductionOrigin => origin is { Scheme: "https", IsLoopback: false, IsDefaultPort: true };

    private bool CanIndex(HttpRequest request) =>
        live && indexingEnabled && HasProductionOrigin && IsHost(request, origin!.Host);

    public async Task RedirectAliases(HttpContext context, RequestDelegate next)
    {
        var request = context.Request;
        if (HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method))
        {
            if (live && HasProductionOrigin && redirectHost is not null &&
                !IsHost(request, origin!.Host) && IsHost(request, redirectHost))
            {
                context.Response.Redirect(origin!.GetLeftPart(UriPartial.Authority) +
                    request.PathBase.ToUriComponent() + request.Path.ToUriComponent() + request.QueryString,
                    permanent: true, preserveMethod: true);
                return;
            }

            if (string.Equals(request.Path.Value, "/index.html", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.Redirect("/", permanent: true);
                return;
            }
        }
        await next(context);
    }

    public IResult Robots(HttpRequest request) => Results.Text(CanIndex(request)
        ? $"User-agent: *\nAllow: /\nSitemap: {Url(request, "/sitemap.xml")}\n"
        : "User-agent: *\nAllow: /\n", "text/plain");

    public async Task<IResult> Sitemap(HttpContext context)
    {
        if (!CanIndex(context.Request)) return Results.NotFound();
        if (HttpMethods.IsHead(context.Request.Method)) return Results.Text("", "application/xml");

        using var db = context.RequestServices.GetRequiredService<Func<AtlasDataContext>>()();
        // Leave room for the two editorial routes under the sitemap protocol's 50,000 URL limit.
        var ids = await EligibleNodes(db).OrderBy(node => node.Id).Select(node => node.Id)
            .Take(49_998).ToListAsync(context.RequestAborted);
        var paths = new[] { "/", "/about" }.Concat(ids.Select(NodePath));
        var xml = new XElement(SitemapNamespace + "urlset", paths.Select(path =>
            new XElement(SitemapNamespace + "url",
                new XElement(SitemapNamespace + "loc", Url(context.Request, path)))));
        return Results.Text(xml.ToString(SaveOptions.DisableFormatting), "application/xml");
    }

    public async Task ServePage(HttpContext context, IWebHostEnvironment environment)
    {
        var request = context.Request;
        var response = context.Response;
        var path = request.Path.Value?.TrimEnd('/').ToLowerInvariant() ?? "";
        if (path == "") path = "/";
        if ((!HttpMethods.IsGet(request.Method) && !HttpMethods.IsHead(request.Method)) || !IsClientRoute(path))
        {
            response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        if (RequiresSignIn(path) && context.User.Identity?.IsAuthenticated != true)
        {
            response.Headers[NoIndexHeader] = "noindex";
            response.Redirect("/");
            return;
        }

        var shell = environment.WebRootFileProvider.GetFileInfo("index.html");
        if (!shell.Exists)
        {
            response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        var home = new Preview(ShareTitle, HomeDescription, "/", "website");
        var globe = request.Query["motion"] == "gif"
            ? ShareTarget.GlobeAnimation
            : ShareTarget.GlobeView(request.Query["view"]);
        var preview = path switch
        {
            "/" when request.Query["globe"] == "open" => new Preview(
                "Put your pin on the Atlas globe",
                "See where people are joining from. Sign in to add your pin.",
                globe.PagePath, "website", Target: globe),
            "/" => home,
            "/share" when request.Query["design"] == "connections" => new Preview(
                "See how ideas connect",
                "Follow a question through an issue to a possible solution on Atlas.",
                "/share?design=connections", "website", ShareArtwork: "/atlas-share-connections.png"),
            "/about" => new Preview("About Atlas | The Public Think Tank", AboutDescription, "/about", "website"),
            "/nodes" => await ReadNode(context),
            "/profiles" when request.Query.TryGetValue("id", out var ids) &&
                ids.Count == 1 && Guid.TryParse(ids[0], out var profile) =>
                await ReadPublicCard(context, new(SharePage.Profile, profile)),
            _ when path.StartsWith("/communities/", StringComparison.Ordinal) &&
                Guid.TryParse(path["/communities/".Length..], out var community) =>
                await ReadPublicCard(context, new(SharePage.Community, community)),
            _ => null
        };

        var captureView = path == "/nodes" && request.Query["share"] == "feed";
        if (captureView || path == "/share" || preview is null || preview.Target is { Kind: not SharePage.Node } || !CanIndex(request))
            response.Headers[NoIndexHeader] = "noindex";
        response.ContentType = "text/html; charset=utf-8";
        if (HttpMethods.IsHead(request.Method)) return;

        using var reader = new StreamReader(shell.CreateReadStream());
        var html = await reader.ReadToEndAsync(context.RequestAborted);
        var endOfHead = html.IndexOf("</head>", StringComparison.OrdinalIgnoreCase);
        if (endOfHead >= 0) html = html.Insert(endOfHead, await Head(request, preview ?? home, preview is not null));
        await response.WriteAsync(html, context.RequestAborted);
    }

    private static async Task<Preview?> ReadNode(HttpContext context)
    {
        if (!context.Request.Query.TryGetValue("id", out var ids) ||
            ids.Count != 1 || !Guid.TryParse(ids[0], out var id) || id == Guid.Empty)
            return null;

        using var db = context.RequestServices.GetRequiredService<Func<AtlasDataContext>>()();
        var node = await EligibleNodes(db).Where(node => node.Id == id)
            .Select(node => new { node.Title, node.DescriptionId }).SingleOrDefaultAsync(context.RequestAborted);
        if (node is null)
        {
            if (!await db.NodeRows.AnyAsync(node => node.Id == id, context.RequestAborted))
                context.Response.StatusCode = StatusCodes.Status404NotFound;
            return null;
        }

        var documents = context.RequestServices.GetRequiredService<IDocumentRepository>();
        var document = documents.GetById(new DocumentId(node.DescriptionId!.Value));
        var text = "";
        Uri? image = null;
        if (document is not null)
        {
            foreach (var block in documents.GetBlocks(document))
            {
                if (block is MarkdownTextBlock markdown && text.Length == 0)
                    text = NodePublicationText.PlainText(markdown.Markdown);
                else if (block is ImageBlock photo && image is null)
                    image = ReadImage(photo.Url);
                else if (block is SectionedBlock sectioned)
                    ReadSection(sectioned, ref text, ref image);
            }
        }
        return new Preview(node.Title, text.Length == 0 ? HomeDescription : Excerpt(text),
            NodePath(id), "article", image, new(SharePage.Node, id));
    }

    private static void ReadSection(SectionedBlock sectioned, ref string text, ref Uri? image)
    {
        using var composition = JsonDocument.Parse(sectioned.Composition);
        foreach (var block in composition.RootElement.GetProperty("blocks").EnumerateArray())
        {
            if (text.Length == 0 && block.GetProperty("kind").GetString() == "text")
                text = NodePublicationText.PlainText(block.GetProperty("text").GetString() ?? "");
            if (image is null && block.TryGetProperty("photos", out var photos))
                foreach (var photo in photos.EnumerateArray())
                {
                    image = ReadImage(photo.GetProperty("src").GetString());
                    if (image is not null) break;
                }
        }
    }

    // Graph nodes are published records. Drafts are stored separately. Match ModerationCase.IsHidden.
    // Use the same anonymous projection for the sitemap and page regardless of session identity.
    private static IQueryable<NodeRow> EligibleNodes(AtlasDataContext db) => db.NodeRows.AsNoTracking()
        .Where(node => node.Status == nameof(NodeStatus.Active) && node.DescriptionId != null &&
            !db.ModerationCaseRows.Any(report => report.NodeId == node.Id &&
                report.Status == (int)ModerationStatus.Actioned && report.VisibilityRestoredAt == null));

    private sealed record PublicCardData(string Title, string Description, DateTimeOffset UpdatedAt);

    private static IQueryable<PublicCardData> PublicCard(AtlasDataContext db, ShareTarget target) =>
        target.Kind == SharePage.Community
            ? db.CommunityRows.Where(row => row.Id == target.Id && row.Status == nameof(CommunityStatus.Active))
                .Select(row => new PublicCardData(row.Name, row.Description, row.UpdatedAt))
            : db.ParticipantRows.Where(row => row.Id == target.Id && row.IsActive)
                .Select(row => new PublicCardData(row.DisplayName, row.Bio, row.UpdatedAt));

    private static async Task<Preview?> ReadPublicCard(HttpContext context, ShareTarget target)
    {
        using var db = context.RequestServices.GetRequiredService<Func<AtlasDataContext>>()();
        var card = await PublicCard(db, target).SingleOrDefaultAsync(context.RequestAborted);
        if (card is null) return null;
        var description = string.IsNullOrWhiteSpace(card.Description)
            ? target.Kind == SharePage.Profile ? "View this participant’s public profile on Atlas." : "Explore this community on Atlas."
            : Excerpt(card.Description);
        return new Preview(card.Title, description, target.PagePath,
            target.Kind == SharePage.Profile ? "profile" : "website", Target: target);
    }

    internal static async Task<DateTimeOffset?> ShareRevision(HttpContext context, ShareTarget? target)
    {
        if (target is { Kind: SharePage.Globe }) return DateTimeOffset.MinValue;

        using var db = context.RequestServices.GetRequiredService<Func<AtlasDataContext>>()();
        if (target is null)
        {
            var moderation = await db.ModerationCaseRows.MaxAsync(
                report => report.VisibilityRestoredAt ?? report.DecidedAt, context.RequestAborted);
            return moderation ?? DateTimeOffset.MinValue;
        }
        if (target.Value.Kind != SharePage.Node)
            return (await PublicCard(db, target.Value).SingleOrDefaultAsync(context.RequestAborted))?.UpdatedAt;

        var id = target.Value.Id;
        var revision = await (from node in EligibleNodes(db)
                              join document in db.DocumentRows on node.DescriptionId equals (Guid?)document.Id
                              where node.Id == id
                              select new
                              {
                                  Node = node.UpdatedAt,
                                  Document = document.UpdatedAt,
                                  Block = (from link in document.Blocks
                                           join block in db.BlockRows on link.BlockId equals block.Id
                                           select (DateTimeOffset?)block.UpdatedAt).Max()
                              }).SingleOrDefaultAsync(context.RequestAborted);
        return revision is null ? null : new[] { revision.Node, revision.Document, revision.Block ?? DateTimeOffset.MinValue }.Max();
    }

    private async Task<string> Head(HttpRequest request, Preview preview, bool canonical)
    {
        var url = Url(request, preview.Path);
        var html = new StringBuilder();
        var encode = HtmlEncoder.Default;
        html.AppendLine($"<title>{encode.Encode(preview.Title)}</title>");
        void Meta(string attribute, string name, string value) =>
            html.AppendLine($"<meta data-atlas-server-seo {attribute}=\"{name}\" content=\"{encode.Encode(value)}\">");

        Meta("name", "description", preview.Description);
        if (url is not null && canonical)
            html.AppendLine($"<link data-atlas-server-seo rel=\"canonical\" href=\"{encode.Encode(url)}\">");
        var socialTitle = preview.ShareArtwork is not null || preview.Target is { Kind: SharePage.Globe }
            ? preview.Title : ShareTitle;
        Meta("property", "og:title", socialTitle);
        Meta("property", "og:description", preview.Description);
        Meta("property", "og:type", preview.Type);
        Meta("property", "og:site_name", ShareTitle);
        if (url is not null) Meta("property", "og:url", url);
        var capture = request.HttpContext.RequestServices.GetRequiredService<ShareImageCapture>();
        var image = preview.ShareArtwork is { } artwork ? Url(request, artwork) : capture.CanServe(preview.Target)
            ? Url(request, await capture.PreviewPath(preview.Target, request.HttpContext.RequestAborted))
            : preview.Image?.AbsoluteUri ?? Url(request, "/atlas-share.png");
        if (image is not null)
        {
            Meta("property", "og:image", image);
            if (capture.CanServe(preview.Target) || preview.Image is null)
            {
                Meta("property", "og:image:width", ShareImageCapture.ImageWidth.ToString(System.Globalization.CultureInfo.InvariantCulture));
                Meta("property", "og:image:height", ShareImageCapture.ImageHeight.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
        }
        Meta("name", "twitter:card", image is null ? "summary" : "summary_large_image");
        Meta("name", "twitter:title", socialTitle);
        Meta("name", "twitter:description", preview.Description);
        if (image is not null) Meta("name", "twitter:image", image);
        return html.ToString();
    }

    private string? Url(HttpRequest request, string path)
    {
        // Use configured deployment origins. Only infer a host for loopback development.
        var authority = origin?.GetLeftPart(UriPartial.Authority);
        if (authority is null &&
            Uri.TryCreate($"{request.Scheme}://{request.Host}", UriKind.Absolute, out var local) && local.IsLoopback)
            authority = local.GetLeftPart(UriPartial.Authority);
        return authority is null ? null : authority + path;
    }

    internal static Uri? ReadOrigin(string? value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttps || (uri.Scheme == Uri.UriSchemeHttp && uri.IsLoopback)) &&
        uri.AbsolutePath == "/" && uri.Query == "" && uri.Fragment == "" && uri.UserInfo == "" ? uri : null;

    private static Uri? ReadImage(string? value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps &&
        uri.UserInfo == "" && !uri.IsLoopback ? uri : null;

    private static string Excerpt(string text)
    {
        const int maxLength = 180;
        var elements = StringInfo.ParseCombiningCharacters(text);
        if (elements.Length <= maxLength) return text;
        var prefix = text[..elements[maxLength]];
        var space = prefix.LastIndexOf(' ');
        return (space > 0 ? prefix[..space] : prefix) + "…";
    }

    private static bool IsHost(HttpRequest request, string host) =>
        string.Equals(request.Host.Host, host, StringComparison.OrdinalIgnoreCase);

    private static string NodePath(Guid id) => $"/nodes?id={id:D}";

    private static bool RequiresSignIn(string path) =>
        path is "/my-drafts" or "/my-settings" or "/me" or "/moderation" or "/profiles/me";

    private static bool IsClientRoute(string path) =>
        RequiresSignIn(path) || path is "/" or "/about" or "/share" or "/communities" or "/components"
        or "/how-to-write-a-node" or "/feedback" or "/help" or "/support" or "/privacy" or "/terms"
        or "/login" or "/new" or "/nodes" or "/profiles" or "/register"
        or "/confirm-email" or "/forgot-password" or "/reset-password"
        || path.StartsWith("/communities/", StringComparison.Ordinal);

    private sealed record Preview(string Title, string Description, string Path, string Type, Uri? Image = null, ShareTarget? Target = null, string? ShareArtwork = null);
}

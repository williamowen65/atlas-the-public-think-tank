using System.Net;
using Atlas.Persistence;
using Atlas.Discovery;
using Atlas.Persistence.Discovery;
using Microsoft.EntityFrameworkCore;

// Standalone local developer tool. Not registered in Atlas.Api or its production routing.
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddUserSecrets<Program>(optional: true).AddEnvironmentVariables();
builder.WebHost.UseUrls("http://127.0.0.1:7251");
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 64 * 1024);
var app = builder.Build();
app.Use(async (context, next) =>
{
    if (context.Connection.RemoteIpAddress is not { } ip || !IPAddress.IsLoopback(ip) ||
        context.Request.Host.Host is not ("localhost" or "127.0.0.1" or "[::1]"))
    { context.Response.StatusCode = 403; return; }
    if (context.Request.Method == "POST" && context.Request.Headers.Origin.ToString() is not
        ("http://127.0.0.1:6006" or "http://localhost:6006" or "http://127.0.0.1:6007" or "http://localhost:6007"))
    { context.Response.StatusCode = 403; return; }
    context.Response.Headers.CacheControl = "no-store";
    await next();
});
app.MapGet("/health", () => Results.Ok(new { ready = true }));
// Read the same local application database as Atlas.Api. No setup, seeding or writes.
app.MapPost("/run", (SearchRequest request) =>
{
    var connection = builder.Configuration["ATLAS_SQL_CONNECTION_STRING"];
    if (string.IsNullOrWhiteSpace(connection)) return Results.Json(new { message = "Set ATLAS_SQL_CONNECTION_STRING to the same local database used by Atlas.Api, in shared user secrets or the environment." }, statusCode: 503);
    try
    {
        var options = new DbContextOptionsBuilder<AtlasDataContext>().UseSqlServer(connection).Options;
        var search = new SqlPublicSearch(() => new AtlasDataContext(options));
        var explained = search.Explain(request.Query, request.Scope);
        using var db = new AtlasDataContext(options);
        var stoplist = db.Database.SqlQueryRaw<StoplistInfo>(
            "SELECT stoplist_id AS Id FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID(N'dbo.SearchDocuments')").SingleOrDefault();
        string[] stopWords = [];
        if (stoplist?.Id == 0)
            stopWords = db.Database.SqlQueryRaw<string>("SELECT stopword AS Value FROM sys.fulltext_system_stopwords WHERE language_id = 1033").ToArray();
        else if (stoplist?.Id is { } id)
            stopWords = db.Database.SqlQuery<string>($"SELECT stopword AS Value FROM sys.fulltext_stopwords WHERE stoplist_id = {id} AND language_id = 1033").ToArray();
        var ids = explained.Evidence.Select(e => e.Id).ToArray();
        var documents = db.Database.SqlQueryRaw<SearchDocument>("SELECT ResultId, SearchText FROM dbo.SearchDocuments")
            .Where(d => ids.Contains(d.ResultId)).ToDictionary(d => d.ResultId, d => d.SearchText);
        var nodeIds = explained.Evidence.Where(e => e.Kind == "node").Select(e => e.NodeId).ToArray();
        var nodes = db.NodeRows.AsNoTracking().Where(n => nodeIds.Contains(n.Id))
            .Select(n => new { n.Id, n.Title, n.ShortDescription, n.DescriptionId }).ToDictionary(n => n.Id);
        var documentIds = nodes.Values.Where(n => n.DescriptionId.HasValue).Select(n => n.DescriptionId!.Value).ToArray();
        var blocks = (from link in db.Set<DocumentBlockRow>().AsNoTracking()
                      join block in db.BlockRows.AsNoTracking() on link.BlockId equals block.Id
                      where documentIds.Contains(link.DocumentId)
                      orderby link.Position
                      select new { link.DocumentId, link.Position, block.Id, block.Kind,
                          Text = EF.Property<string>(block, "SearchText") }).ToArray();
        var commentIds = explained.Evidence.Where(e => e.Kind == "comment").Select(e => e.Id).ToArray();
        var comments = db.CommentRows.AsNoTracking().Where(c => commentIds.Contains(c.Id))
            .ToDictionary(c => c.Id, c => c.Body);
        TextSource[] Sources(SearchRankEvidence evidence)
        {
            if (evidence.Kind == "comment") return [new("Comment body", comments.GetValueOrDefault(evidence.Id, ""))];
            if (!nodes.TryGetValue(evidence.NodeId, out var node)) return [];
            return new[] { new TextSource("Title", node.Title), new TextSource("Short description", node.ShortDescription) }
                .Concat(blocks.Where(b => b.DocumentId == node.DescriptionId)
                    .Select(b => new TextSource($"Block {b.Position + 1} ({b.Kind})", b.Text ?? "", b.Id))).ToArray();
        }
        return Results.Ok(new { policy = request.Query.VotingWeight.HasValue ? SqlPublicSearch.WeightedRankingPolicyVersion : SqlPublicSearch.RankingPolicyVersion, explained.Page,
            collectedAt = DateTimeOffset.UtcNow,
            stoplist = stoplist is null ? "Unknown" : stoplist.Id is null ? "Off" : stoplist.Id == 0 ? "System (English)" : "Custom (English)",
            stopWords,
            rows = explained.Evidence.Select((e, i) => new {
                position = (explained.Page.Page - 1) * explained.Page.PageSize + i + 1,
                item = explained.Page.Items[i], evidence = e, searchText = documents.GetValueOrDefault(e.Id, ""),
                sources = Sources(e)
            }) });
    }
    catch (ArgumentException error) { return Results.BadRequest(new { message = error.Message }); }
    catch (Exception error)
    {
        app.Logger.LogWarning("Local ranking search failed ({ErrorType}).", error.GetType().Name);
        return Results.Json(new { message = "Could not search the local Atlas database. Check the connection, database setup and full-text search availability." }, statusCode: 503);
    }
});
app.Run();
public sealed record SearchRequest(DiscoveryQuery Query, string Scope = "all");
public sealed class SearchDocument
{
    public Guid ResultId { get; set; }
    public string SearchText { get; set; } = "";
}

public sealed record TextSource(string Field, string Text, Guid? BlockId = null);

public sealed class StoplistInfo { public int? Id { get; set; } }

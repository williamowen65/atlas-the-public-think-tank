using System.Net;
using Atlas.Persistence;
using Atlas.Discovery;
using Atlas.Persistence.Discovery;
using Microsoft.Data.SqlClient;
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
        var ids = explained.Evidence.Select(e => e.Id).ToArray();
        var documents = db.Database.SqlQueryRaw<SearchDocument>("SELECT ResultId, SearchText FROM dbo.SearchDocuments")
            .Where(d => ids.Contains(d.ResultId)).ToDictionary(d => d.ResultId, d => d.SearchText);
        return Results.Ok(new { policy = SqlPublicSearch.RankingPolicyVersion, explained.Page,
            rows = explained.Evidence.Select((e, i) => new {
                position = (explained.Page.Page - 1) * explained.Page.PageSize + i + 1,
                item = explained.Page.Items[i], evidence = e, searchText = documents.GetValueOrDefault(e.Id, "")
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

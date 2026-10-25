using System.Net;
using Atlas.Persistence;
using Atlas.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

// Standalone local developer tool. Not registered in Atlas.Api or its production routing.
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddUserSecrets<Program>(optional: true).AddEnvironmentVariables();
builder.WebHost.UseUrls("http://127.0.0.1:7251");
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 64 * 1024);
var app = builder.Build();
var gate = new SemaphoreSlim(1, 1);
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
app.MapPost("/run", async (RankingScenario scenario) =>
{
    try { SearchRankingFixture.Validate(scenario); }
    catch (ArgumentException) { return Results.BadRequest(new { message = "Invalid scenario. Check text lengths, keys, dates and vote values." }); }
    var template = builder.Configuration["ATLAS_SQL_TEST_CONNECTION_STRING"];
    if (string.IsNullOrWhiteSpace(template)) return Results.Json(new { message = "Set ATLAS_SQL_TEST_CONNECTION_STRING in shared user secrets or the environment. Ranking Lab creates disposable databases; it does not use your Atlas application database." }, statusCode: 503);
    if (!await gate.WaitAsync(0)) return Results.Conflict(new { message = "Another scenario is running. Wait for it to finish." });
    try
    {
        // Never accept a database name from the browser. All creation/deletion uses this generated name.
        return await Task.Run<IResult>(() =>
        {
            var databaseName = $"AtlasRankingLab_{Guid.NewGuid():N}";
            var connection = new SqlConnectionStringBuilder(template) { InitialCatalog = databaseName }.ConnectionString;
            var options = new DbContextOptionsBuilder<AtlasDataContext>().UseSqlServer(connection, sql => sql.CommandTimeout(60)).Options;
            using var db = new AtlasDataContext(options);
            try
            {
                db.Database.EnsureCreated();
                AtlasDatabaseSetup.InstallSupplementalSchema(db);
                return Results.Ok(SearchRankingFixture.Run(() => new AtlasDataContext(options), scenario));
            }
            finally
            {
                // Cleanup also runs after SQL errors. A cleanup failure reports the generated name for manual cleanup.
                try { db.Database.EnsureDeleted(); }
                catch { app.Logger.LogError("Could not remove disposable Ranking Lab database {Database}. Remove it manually.", databaseName); throw; }
            }
        });
    }
    catch (Exception error)
    {
        // Keep connection strings and submitted content out of browser errors.
        app.Logger.LogWarning("Ranking Lab failed ({ErrorType}). Check SQL Server, full-text English support and disposable-database permissions.", error.GetType().Name);
        return Results.Json(new { message = "Could not run the SQL scenario. Check the Ranking Lab terminal, SQL Server full-text support and database creation/deletion permissions." }, statusCode: 503);
    }
    finally { gate.Release(); }
});
app.Run();

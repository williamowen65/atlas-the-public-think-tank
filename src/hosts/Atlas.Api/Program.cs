using Atlas.Api;
using Microsoft.OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.StaticFiles;
using System.Threading.RateLimiting;

// Top-level statements are the application's Main method. ASP.NET Core supplies
// the web host; this file chooses services and the order requests pass through them.

// 1. Build configuration and register services (no HTTP requests are handled yet).
var builder = WebApplication.CreateBuilder(args);
// Avoid routine framework logs containing raw URLs/search strings or SQL commands.
builder.Logging.AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
// Terraform supplies the Azure Monitor connection string. The .NET host wires
// production exporters and keeps local development isolated from cloud costs.
if (!builder.Environment.IsDevelopment() &&
    builder.Configuration.GetValue<bool?>("Atlas:Telemetry:Enabled") != false)
{
    if (string.IsNullOrWhiteSpace(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        throw new InvalidOperationException("Production requires APPLICATIONINSIGHTS_CONNECTION_STRING.");

    builder.UseMicrosoftOpenTelemetry(options =>
    {
        options.Exporters = ExportTarget.AzureMonitor;
        // Cap trace ingestion for the student deployment's small telemetry budget.
        options.AzureMonitor.TracesPerSecond = 2;
        options.AzureMonitor.EnableLiveMetrics = false;
        // SQL statements can contain user data; Atlas emits its own bounded
        // operation metrics rather than sending raw SQL client diagnostics.
        options.Instrumentation.EnableSqlClientInstrumentation = false;
    });
    builder.Services.ConfigureOpenTelemetryTracerProvider(
        (_, tracer) => tracer.AddSource("Atlas.Contracts"));
    builder.Services.ConfigureOpenTelemetryMeterProvider(
        (_, meter) => meter.AddMeter("Atlas.Observability"));
}
builder.Services.AddAtlasApi(builder.Configuration);
builder.Services.AddSingleton<ShareImageCapture>();
// Globe routes use the existing API host in every environment; schema updates remain explicit.
builder.Services.AddSingleton<Atlas.Persistence.Globe.IGlobeHistory, Atlas.Persistence.Globe.SqlGlobeHistory>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<Atlas.Api.Globe.GlobeService>();
// The built-in antiforgery filter protects every unsafe controller request, even
// on future controllers. AddControllersWithViews registers its MVC filter services; MapControllers
// below still exposes only JSON endpoints, with no views or frontend assets.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    options.Filters.Add(new AntiforgeryProblemDetailsFilter());
});
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiErrors>();
// The browser keeps the cookie; the client sends the paired JSON token in a header.
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "__Host-Atlas.Csrf";
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});
// Kestrel is the network server. Bound request bodies before model binding.
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 64 * 1024);
// A per-process safety ceiling, not a replacement for participant/domain quotas or
// the distributed operation-family budgets tracked by PTT-137.
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new FixedWindowRateLimiterOptions
        { PermitLimit = 20, Window = TimeSpan.FromMinutes(1), QueueLimit = 0, AutoReplenishment = true }));
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(_ =>
        RateLimitPartition.GetConcurrencyLimiter("api", _ => new ConcurrencyLimiterOptions
        { PermitLimit = 64, QueueLimit = 0 }));
});
// 2. Create the host, then assemble the middleware pipeline in request order.
var app = builder.Build();
var seo = new PublicPageSeo(app.Configuration);
// Observe the final status after error handling, including early middleware denials.
app.UseMiddleware<HttpTelemetryMiddleware>();
// Wrap downstream execution so exceptions become safe JSON ProblemDetails.
app.UseExceptionHandler();
if (!app.Environment.IsDevelopment()) app.UseHsts();
app.UseHttpsRedirection();
app.Use(seo.RedirectAliases);

// Production forwards the browser's two API prefixes to the existing /api routes.
// Vite already performs this mapping in development, but does not run in Azure.
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/atlas-public-api", out var publicPath))
    {
        if (!HttpMethods.IsGet(context.Request.Method))
        {
            context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
            context.Response.Headers.Allow = "GET";
            return;
        }
        context.Request.Headers.Remove("Cookie");
        context.Request.Headers.Remove("Authorization");
        context.Request.Path = new PathString("/api").Add(publicPath);
    }
    else if (context.Request.Path.StartsWithSegments("/atlas-session-api", out var sessionPath))
    {
        context.Request.Path = new PathString("/api").Add(sessionPath);
    }
    await next();
});

app.Use(async (context, next) =>
{
    // Match antiforgery cache policy so token responses need no header override.
    context.Response.Headers.CacheControl = "no-cache, no-store";
    context.Response.Headers.Pragma = "no-cache";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    await next();
});
// The release pipeline places the SvelteKit static build under API wwwroot.
var staticContentTypes = new FileExtensionContentTypeProvider();
staticContentTypes.Mappings[".geojson"] = "application/geo+json";
app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = staticContentTypes });
// Match a route before applying endpoint-specific rate limits and policies.
app.UseRouting();
app.UseRateLimiter();
// Authentication reads the cookie; authorization evaluates the endpoint's policy.
app.UseAuthentication();
app.UseAuthorization();
// Resolve the participant and enrich the existing HTTP telemetry scope.
app.UseMiddleware<OperationContextMiddleware>();
// Deployment liveness checks the HTTP process independently of SQL readiness.
app.MapGet("/health/live", () => Results.Ok(new { status = "ok" }));
app.MapControllers();
app.MapMethods("/robots.txt", ["GET", "HEAD"], (HttpRequest request) => seo.Robots(request));
app.MapMethods("/sitemap.xml", ["GET", "HEAD"], (Func<HttpContext, Task<IResult>>)seo.Sitemap);
app.MapGet("/share-images/discover.png", (HttpContext context, ShareImageCapture capture) => capture.Image(context, null));
app.MapGet("/share-images/globe.png", (HttpContext context, ShareImageCapture capture) => capture.Image(context, ShareTarget.Globe));
app.MapGet("/share-images/nodes/{id:guid}.png", (HttpContext context, ShareImageCapture capture, Guid id) => capture.Image(context, new(SharePage.Node, id)));
app.MapGet("/share-images/communities/{id:guid}.png", (HttpContext context, ShareImageCapture capture, Guid id) =>
    capture.Image(context, new(SharePage.Community, id)));
app.MapGet("/share-images/profiles/{id:guid}.png", (HttpContext context, ShareImageCapture capture, Guid id) =>
    capture.Image(context, new(SharePage.Profile, id)));
app.MapFallback(context => seo.ServePage(context, app.Environment));
app.Run();
// Top-level statements generate a Program type containing Main. This declaration
// merges with that generated type (partial) and makes it public so the separate
// test assembly can use WebApplicationFactory<Program>. No second entry point is created.
public partial class Program { }

using Atlas.Api;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

// Top-level statements are the application's Main method. ASP.NET Core supplies
// the web host; this file chooses services and the order requests pass through them.

// 1. Build configuration and register services (no HTTP requests are handled yet).
var builder = WebApplication.CreateBuilder(args);
// Avoid routine framework logs containing raw URLs/search strings or SQL commands.
builder.Logging.AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
builder.Services.AddAtlasApi(builder.Configuration);
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
// Observe the final status after error handling, including early middleware denials.
app.UseMiddleware<HttpTelemetryMiddleware>();
// Wrap downstream execution so exceptions become safe JSON ProblemDetails.
app.UseExceptionHandler();
if (!app.Environment.IsDevelopment()) app.UseHsts();
app.UseHttpsRedirection();

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
app.UseDefaultFiles();
app.UseStaticFiles();
// Match a route before applying endpoint-specific rate limits and policies.
app.UseRouting();
app.UseRateLimiter();
// Authentication reads the cookie; authorization evaluates the endpoint's policy.
app.UseAuthentication();
app.UseAuthorization();
// Resolve the participant and enrich the existing HTTP telemetry scope.
app.UseMiddleware<OperationContextMiddleware>();
// 3. Expose controller actions and start listening. No view/static-file fallback exists.
app.MapControllers();
// Fall back to the static SPA only for known UI pages, never for unknown APIs
// or missing assets (which must keep an HTTP 404 status).
app.MapFallback(async context =>
{
    var path = context.Request.Path.Value ?? "/";
    var clientRoute = path is "/" or "/about" or "/communities" or "/components"
        or "/login" or "/me" or "/moderation" or "/my-drafts" or "/my-settings"
        or "/new" or "/nodes" or "/profiles" or "/profiles/me" or "/register"
        || path.StartsWith("/communities/", StringComparison.OrdinalIgnoreCase);
    var indexFile = Path.Combine(app.Environment.WebRootPath ?? "", "index.html");
    if (!clientRoute || !File.Exists(indexFile))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(indexFile);
});
app.Run();
// Top-level statements generate a Program type containing Main. This declaration
// merges with that generated type (partial) and makes it public so the separate
// test assembly can use WebApplicationFactory<Program>. No second entry point is created.
public partial class Program { }

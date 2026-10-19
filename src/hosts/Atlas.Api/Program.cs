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
// Controllers handle routing/JSON; the global filter checks CSRF on unsafe methods.
builder.Services.AddControllers(options => options.Filters.Add(new Atlas.Api.CsrfFilter()));
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
// Wrap downstream execution so exceptions become safe JSON ProblemDetails.
app.UseExceptionHandler();
if (!app.Environment.IsDevelopment()) app.UseHsts();
app.UseHttpsRedirection();
app.Use(async (context, next) =>
{
    context.Response.Headers.CacheControl = "no-store";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    await next();
});
// Match a route before applying endpoint-specific rate limits and policies.
app.UseRouting();
app.UseRateLimiter();
// Authentication reads the cookie; authorization evaluates the endpoint's policy.
app.UseAuthentication();
app.UseAuthorization();
// Resolve the participant and open the ambient telemetry scope around the endpoint.
app.UseMiddleware<OperationContextMiddleware>();
// 3. Expose controller actions and start listening. No view/static-file fallback exists.
app.MapControllers();
app.Run();
// Top-level statements generate a Program type containing Main. This declaration
// merges with that generated type (partial) and makes it public so the separate
// test assembly can use WebApplicationFactory<Program>. No second entry point is created.
public partial class Program { }

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Atlas.Api.Globe;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Atlas.Api.Controllers;

[ApiController, Route("api/globe"), AutoValidateAntiforgeryToken]
public sealed class GlobeController(GlobePresence presence, IDataProtectionProvider protection,
    RequestActor actor, ILogger<GlobeController> logger) : ControllerBase
{
    private const string Cookie = "__Host-Atlas.Globe";
    private readonly IDataProtector protector = protection.CreateProtector("Atlas.LocalGlobe.Visitor.v1");
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    [HttpPost("visit"), EnableRateLimiting("auth")]
    public async Task<IActionResult> Visit(CancellationToken cancellationToken)
    {
        var browser = Browser(create: true)!;
        var visit = await presence.Visit(browser, Account(), cancellationToken);
        return Ok(new { visit.HasLocation });
    }

    [HttpGet("events")]
    public async Task Events(CancellationToken cancellationToken)
    {
        var browser = Browser(create: false);
        if (browser is null) { Response.StatusCode = 400; return; }
        var visit = await presence.Visit(browser, Account(), cancellationToken);
        (Guid Id, System.Threading.Channels.ChannelReader<GlobeSnapshot> Updates) connection;
        try { connection = await presence.Connect(visit.VisitorId, cancellationToken); }
        catch (GlobeConnectionLimitException)
        {
            Response.StatusCode = StatusCodes.Status429TooManyRequests;
            Response.Headers.RetryAfter = "5";
            await Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = 429, Title = "The globe connection limit has been reached. Please retry shortly.",
                Extensions = { ["code"] = "globe_connection_limit" }
            }, cancellationToken);
            return;
        }
        Response.ContentType = "text/event-stream";
        Response.Headers["X-Accel-Buffering"] = "no";
        HttpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();
        try
        {
            // One timer and one pending channel read; neither accumulates work on idle streams.
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(15));
            var heartbeat = timer.WaitForNextTickAsync(cancellationToken).AsTask();
            var update = connection.Updates.WaitToReadAsync(cancellationToken).AsTask();
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.WhenAny(update, heartbeat);
                if (update.IsCompleted)
                {
                    if (!await update) break;
                    while (connection.Updates.TryRead(out var snapshot))
                        await Response.WriteAsync($"data: {JsonSerializer.Serialize(snapshot, Json)}\n\n", cancellationToken);
                    update = connection.Updates.WaitToReadAsync(cancellationToken).AsTask();
                }
                if (heartbeat.IsCompleted)
                {
                    if (!await heartbeat) break;
                    await Response.WriteAsync(": heartbeat\n\n", cancellationToken);
                    heartbeat = timer.WaitForNextTickAsync(cancellationToken).AsTask();
                }
                await Response.Body.FlushAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        finally
        {
            try { await presence.Disconnect(connection.Id); }
            catch (Exception) { logger.LogWarning("Globe disconnected; snapshot refresh unavailable."); }
        }
    }

    [HttpPut("location"), EnableRateLimiting("auth")]
    public async Task<IActionResult> Location(GlobeLocation input, CancellationToken cancellationToken)
    {
        var browser = Browser(create: false);
        if (browser is null) return BadRequest();
        var visit = await presence.Visit(browser, Account(), cancellationToken);
        await presence.SetLocation(visit.VisitorId, input.Longitude, input.Latitude, cancellationToken);
        return NoContent();
    }

    private string? Account() => actor.Id is { } id ? Key("a:", id.ToString()) : null;
    private static string Key(string prefix, string value) => prefix + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    private string? Browser(bool create)
    {
        if (Request.Cookies.TryGetValue(Cookie, out var cookie))
        {
            try { if (Guid.TryParse(protector.Unprotect(cookie), out var id)) return Key("b:", id.ToString()); }
            catch (CryptographicException) { /* Expired/invalid visitor cookie: never trust submitted identity. */ }
        }
        if (!create) return null;
        var visitor = Guid.NewGuid();
        Response.Cookies.Append(Cookie, protector.Protect(visitor.ToString()), new CookieOptions
        { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict, Path = "/", MaxAge = TimeSpan.FromDays(365) });
        return Key("b:", visitor.ToString());
    }
}

public sealed record GlobeLocation(double? Longitude, double? Latitude);

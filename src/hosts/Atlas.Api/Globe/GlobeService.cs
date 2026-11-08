using Atlas.Persistence.Globe;

namespace Atlas.Api.Globe;

/// <summary>Short snapshot requests and bounded-age activity, with no connection registry or broadcasts.</summary>
public sealed class GlobeService(IGlobeHistory history, TimeProvider clock)
{
    public const int OnlineWindowMinutes = 5;
    private readonly SemaphoreSlim cacheGate = new(1, 1);
    private GlobeSnapshot? cached;
    private long revision;

    public async Task<GlobeView> View(string? account, CancellationToken cancellationToken)
    {
        var snapshot = await Snapshot(cancellationToken);
        var hasPin = account is not null && await history.HasPin(account, cancellationToken);
        return new(snapshot.Live, snapshot.Historical, snapshot.CapturedAt, account is not null, hasPin);
    }

    public async Task<GlobeView> Activity(string account, CancellationToken cancellationToken)
    {
        if (await history.Touch(account, clock.GetUtcNow(), cancellationToken)) Invalidate();
        return await View(account, cancellationToken);
    }

    public async Task<GlobeView> SetLocation(string account, double? longitude, double? latitude, CancellationToken cancellationToken, string? color = null)
    {
        if (longitude is { } lon && (!double.IsFinite(lon) || lon < -180 || lon > 180)
            || latitude is { } lat && (!double.IsFinite(lat) || lat < -90 || lat > 90)
            || (longitude is null) != (latitude is null)) throw new ArgumentException("Invalid globe coordinates.");
        color ??= "#b73539";
        if (!System.Text.RegularExpressions.Regex.IsMatch(color, "^#[0-9a-fA-F]{6}\\z", System.Text.RegularExpressions.RegexOptions.CultureInvariant))
            throw new ArgumentException("Invalid globe pin color. Use a six-digit hex color.");
        color = color.ToLowerInvariant();
        await history.SetLocation(account, longitude is null ? null : Math.Round(longitude.Value),
            latitude is null ? null : Math.Round(latitude.Value), clock.GetUtcNow(), cancellationToken, color);
        Invalidate();
        return await View(account, cancellationToken);
    }

    private void Invalidate() { Interlocked.Increment(ref revision); cached = null; }

    private async Task<GlobeSnapshot> Snapshot(CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        if (cached is { } hit && now - hit.CapturedAt < TimeSpan.FromSeconds(10)) return hit;
        await cacheGate.WaitAsync(cancellationToken);
        try
        {
            now = clock.GetUtcNow();
            if (cached is { } existing && now - existing.CapturedAt < TimeSpan.FromSeconds(10)) return existing;
            var version = Interlocked.Read(ref revision);
            var snapshot = await history.Snapshot(now, now.AddMinutes(-OnlineWindowMinutes), cancellationToken);
            if (version == Interlocked.Read(ref revision)) cached = snapshot;
            return snapshot;
        }
        finally { cacheGate.Release(); }
    }
}

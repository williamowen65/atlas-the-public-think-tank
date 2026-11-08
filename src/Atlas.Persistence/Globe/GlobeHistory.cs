using Microsoft.EntityFrameworkCore;

namespace Atlas.Persistence.Globe;

// Pseudonymous local visitor records, separate from profiles and content.
public sealed class GlobeVisitorRow
{
    public string Id { get; set; } = "";
    public DateTimeOffset FirstSeenAt { get; set; }
    public double? Longitude { get; set; }
    public double? Latitude { get; set; }
}

public sealed class GlobeBrowserRow
{
    public string Id { get; set; } = "";
    public string VisitorId { get; set; } = "";
}

public sealed record GlobePin(double Longitude, double Latitude, int Visitors);
public sealed record GlobeHistorySummary(int Visitors, DateTimeOffset? Since, GlobePin[] Pins);
public sealed record GlobeVisit(string VisitorId, string? MergedVisitorId, bool HasLocation);

public interface IGlobeHistory
{
    Task<GlobeVisit> Visit(string browser, string? account, CancellationToken cancellationToken);
    Task SetLocation(string visitor, double? longitude, double? latitude, CancellationToken cancellationToken);
    Task<GlobeHistorySummary> Summary(string[]? visitors, CancellationToken cancellationToken);
}

/// <summary>Called under the local presence service's gate; every operation owns its context.</summary>
public sealed class SqlGlobeHistory(Func<AtlasDataContext> open) : IGlobeHistory
{
    public async Task<GlobeVisit> Visit(string browser, string? account, CancellationToken cancellationToken)
    {
        await using var db = open();
        var alias = await db.Set<GlobeBrowserRow>().FindAsync([browser], cancellationToken);
        var id = account ?? alias?.VisitorId ?? browser;
        var visitor = await db.Set<GlobeVisitorRow>().FindAsync([id], cancellationToken);
        if (visitor is null)
        {
            visitor = new() { Id = id, FirstSeenAt = DateTimeOffset.UtcNow };
            db.Add(visitor);
        }
        string? merged = null;
        // Upgrade anonymous history on sign-in, but never merge two different accounts.
        if (account is not null && alias is not null && alias.VisitorId.StartsWith("b:") && alias.VisitorId != id)
        {
            var previous = await db.Set<GlobeVisitorRow>().FindAsync([alias.VisitorId], cancellationToken);
            if (previous is not null)
            {
                merged = previous.Id;
                visitor.FirstSeenAt = visitor.FirstSeenAt < previous.FirstSeenAt ? visitor.FirstSeenAt : previous.FirstSeenAt;
                // Sharing as a guest does not silently grant an account permission to share.
                // Require a fresh explicit action after the anonymous-to-account upgrade.
                var aliases = await db.Set<GlobeBrowserRow>().Where(x => x.VisitorId == previous.Id).ToListAsync(cancellationToken);
                foreach (var other in aliases) other.VisitorId = id;
                db.Remove(previous);
            }
        }
        if (alias is null) db.Add(new GlobeBrowserRow { Id = browser, VisitorId = id });
        else alias.VisitorId = id;
        await db.SaveChangesAsync(cancellationToken);
        return new(id, merged, visitor.Longitude is not null);
    }

    public async Task SetLocation(string visitor, double? longitude, double? latitude, CancellationToken cancellationToken)
    {
        await using var db = open();
        var row = await db.Set<GlobeVisitorRow>().SingleAsync(x => x.Id == visitor, cancellationToken);
        row.Longitude = longitude;
        row.Latitude = latitude;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<GlobeHistorySummary> Summary(string[]? visitors, CancellationToken cancellationToken)
    {
        await using var db = open();
        var query = db.Set<GlobeVisitorRow>().AsNoTracking();
        if (visitors is not null) query = query.Where(x => visitors.Contains(x.Id));
        var count = await query.CountAsync(cancellationToken);
        var since = await query.MinAsync(x => (DateTimeOffset?)x.FirstSeenAt, cancellationToken);
        var pins = await query.Where(x => x.Longitude != null && x.Latitude != null)
            .GroupBy(x => new { x.Longitude, x.Latitude })
            .Select(x => new GlobePin(x.Key.Longitude!.Value, x.Key.Latitude!.Value, x.Count()))
            .ToArrayAsync(cancellationToken);
        return new(count, since, pins);
    }
}

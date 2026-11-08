using Microsoft.EntityFrameworkCore;

namespace Atlas.Persistence.Globe;

/// <summary>One opted-in pin per account. IDs are server-derived account hashes, never browser identities.</summary>
public sealed class GlobeVisitorRow
{
    public string Id { get; set; } = "";
    public DateTimeOffset FirstSeenAt { get; set; }
    public DateTimeOffset? LastSeenAt { get; set; }
    public double? Longitude { get; set; }
    public double? Latitude { get; set; }
}

public sealed record GlobePin(double Longitude, double Latitude, int Visitors);
public sealed record GlobeHistorySummary(int Visitors, DateTimeOffset? Since, GlobePin[] Pins);
public sealed record GlobeSnapshot(GlobeHistorySummary Live, GlobeHistorySummary Historical, DateTimeOffset CapturedAt);
public sealed record GlobeView(GlobeHistorySummary Live, GlobeHistorySummary Historical, DateTimeOffset CapturedAt,
    bool CanShare, bool HasLocation, int OnlineWindowMinutes = 5);

public interface IGlobeHistory
{
    Task<bool> HasPin(string account, CancellationToken cancellationToken);
    Task SetLocation(string account, double? longitude, double? latitude, DateTimeOffset now, CancellationToken cancellationToken);
    Task<bool> Touch(string account, DateTimeOffset now, CancellationToken cancellationToken);
    Task<GlobeSnapshot> Snapshot(DateTimeOffset now, DateTimeOffset onlineSince, CancellationToken cancellationToken);
}

public sealed class SqlGlobeHistory(Func<AtlasDataContext> open) : IGlobeHistory
{
    public async Task<bool> HasPin(string account, CancellationToken cancellationToken)
    {
        await using var db = open();
        return await db.Set<GlobeVisitorRow>().AnyAsync(x => x.Id == account && x.Id.StartsWith("a:")
            && x.Longitude != null && x.Latitude != null, cancellationToken);
    }

    public async Task SetLocation(string account, double? longitude, double? latitude, DateTimeOffset now, CancellationToken cancellationToken)
    {
        if (!account.StartsWith("a:")) throw new ArgumentException("An account is required.");
        await using var db = open();
        var row = await db.Set<GlobeVisitorRow>().FindAsync([account], cancellationToken);
        if (longitude is null)
        {
            if (row is null) return;
            db.Remove(row);
        }
        else
        {
            if (row is null)
            {
                row = new() { Id = account, FirstSeenAt = now };
                db.Add(row);
            }
            else if (row.Longitude is null) row.FirstSeenAt = now;
            row.Longitude = longitude;
            row.Latitude = latitude;
            row.LastSeenAt = now;
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> Touch(string account, DateTimeOffset now, CancellationToken cancellationToken)
    {
        await using var db = open();
        // Polls from multiple tabs update an existing pin at most once per 45 seconds.
        // Unpinned accounts and anonymous visitors never create activity/history records.
        var before = now.AddSeconds(-45);
        return await db.Set<GlobeVisitorRow>().Where(x => x.Id == account && x.Id.StartsWith("a:")
            && x.Longitude != null && x.Latitude != null && (x.LastSeenAt == null || x.LastSeenAt <= before))
            .ExecuteUpdateAsync(set => set.SetProperty(x => x.LastSeenAt, now), cancellationToken) > 0;
    }

    public async Task<GlobeSnapshot> Snapshot(DateTimeOffset now, DateTimeOffset onlineSince, CancellationToken cancellationToken)
    {
        await using var db = open();
        // Legacy anonymous rows are deliberately excluded. No anonymous pin is adopted by an account.
        var pins = await db.Set<GlobeVisitorRow>().AsNoTracking()
            .Where(x => x.Id.StartsWith("a:") && x.Longitude != null && x.Latitude != null)
            .Select(x => new { Longitude = x.Longitude!.Value, Latitude = x.Latitude!.Value, x.FirstSeenAt, x.LastSeenAt })
            .ToArrayAsync(cancellationToken);
        GlobeHistorySummary Summarize(bool live)
        {
            var selected = pins.Where(x => !live || x.LastSeenAt >= onlineSince).ToArray();
            return new(selected.Length, selected.Length == 0 ? null : selected.Min(x => x.FirstSeenAt),
                selected.GroupBy(x => new { x.Longitude, x.Latitude })
                    .Select(x => new GlobePin(x.Key.Longitude, x.Key.Latitude, x.Count())).ToArray());
        }
        return new(Summarize(true), Summarize(false), now);
    }
}

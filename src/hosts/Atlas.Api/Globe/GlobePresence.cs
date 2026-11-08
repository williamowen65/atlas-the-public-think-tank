using System.Threading.Channels;
using Atlas.Persistence.Globe;

namespace Atlas.Api.Globe;

public sealed record GlobeSnapshot(GlobeHistorySummary Live, GlobeHistorySummary Historical);

/// <summary>Single-process live presence. Historical totals and opted-in pins live in SQL.</summary>
public sealed class GlobePresence(IGlobeHistory history)
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly Dictionary<Guid, (string Visitor, Channel<GlobeSnapshot> Channel)> connections = [];

    public async Task<GlobeVisit> Visit(string browser, string? account, CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            var visit = await history.Visit(browser, account, cancellationToken);
            if (visit.MergedVisitorId is { } old)
                foreach (var (key, connection) in connections.ToArray())
                    if (connection.Visitor == old) connections[key] = (visit.VisitorId, connection.Channel);
            await Publish(cancellationToken);
            return visit;
        }
        finally { gate.Release(); }
    }

    public async Task<(Guid Id, ChannelReader<GlobeSnapshot> Updates)> Connect(string visitor, CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken);
        Guid? added = null;
        try
        {
            if (connections.Count >= 100 || connections.Values.Count(x => x.Visitor == visitor) >= 8)
                throw new InvalidOperationException("Too many globe connections.");
            var channel = Channel.CreateBounded<GlobeSnapshot>(new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropOldest });
            var id = Guid.NewGuid();
            connections.Add(id, (visitor, channel));
            added = id;
            await Publish(cancellationToken);
            return (id, channel.Reader);
        }
        catch
        {
            if (added is { } id) connections.Remove(id);
            throw;
        }
        finally { gate.Release(); }
    }

    public async Task Disconnect(Guid id)
    {
        await gate.WaitAsync();
        try
        {
            if (connections.Remove(id, out var connection)) connection.Channel.Writer.TryComplete();
            // Disconnect must always release presence, including when the request was cancelled.
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await Publish(timeout.Token);
        }
        finally { gate.Release(); }
    }

    public async Task SetLocation(string visitor, double? longitude, double? latitude, CancellationToken cancellationToken)
    {
        if (longitude is { } lon && (!double.IsFinite(lon) || lon < -180 || lon > 180)
            || latitude is { } lat && (!double.IsFinite(lat) || lat < -90 || lat > 90)
            || (longitude is null) != (latitude is null)) throw new ArgumentException("Invalid globe coordinates.");
        await gate.WaitAsync(cancellationToken);
        try
        {
            // Whole-degree cells prevent exact positions reaching persistence or other clients.
            await history.SetLocation(visitor, longitude is null ? null : Math.Round(longitude.Value),
                latitude is null ? null : Math.Round(latitude.Value), cancellationToken);
            await Publish(cancellationToken);
        }
        finally { gate.Release(); }
    }

    private async Task Publish(CancellationToken cancellationToken)
    {
        if (connections.Count == 0) return;
        var live = await history.Summary(connections.Values.Select(x => x.Visitor).Distinct().ToArray(), cancellationToken);
        var historical = await history.Summary(null, cancellationToken);
        var snapshot = new GlobeSnapshot(live, historical);
        foreach (var connection in connections.Values) connection.Channel.Writer.TryWrite(snapshot);
    }
}

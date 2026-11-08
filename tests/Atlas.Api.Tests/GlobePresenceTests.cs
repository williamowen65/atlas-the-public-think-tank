using Atlas.Api.Globe;
using Atlas.Persistence.Globe;

namespace Atlas.Api.Tests;

[TestClass]
public sealed class GlobePresenceTests
{
    [TestMethod]
    public async Task Tabs_count_once_and_history_remains_after_last_disconnect()
    {
        var history = new MemoryGlobeHistory();
        var service = new GlobePresence(history);
        await service.Visit("browser", null, default);
        var tab1 = await service.Connect("browser", default);
        var tab2 = await service.Connect("browser", default);
        Assert.AreEqual(1, (await tab1.Updates.ReadAsync()).Live.Visitors);
        await service.Visit("other", null, default);
        var other = await service.Connect("other", default);
        Assert.AreEqual(2, (await other.Updates.ReadAsync()).Live.Visitors);
        await service.Disconnect(tab1.Id);
        Assert.AreEqual(2, (await other.Updates.ReadAsync()).Live.Visitors);
        await service.Disconnect(tab2.Id);
        var remaining = await other.Updates.ReadAsync();
        Assert.AreEqual(1, remaining.Live.Visitors);
        Assert.AreEqual(2, remaining.Historical.Visitors);
        await service.Disconnect(other.Id);
    }

    [TestMethod]
    public async Task Share_rounds_coordinates_and_removal_clears_both_views()
    {
        var service = new GlobePresence(new MemoryGlobeHistory());
        await service.Visit("one", null, default);
        var connection = await service.Connect("one", default);
        await service.SetLocation("one", -122.43123, 47.65432, default);
        var shared = await connection.Updates.ReadAsync();
        Assert.AreEqual(new GlobePin(-122, 48, 1), shared.Live.Pins.Single());
        Assert.AreEqual(shared.Live.Pins.Single(), shared.Historical.Pins.Single());
        await service.SetLocation("one", null, null, default);
        var removed = await connection.Updates.ReadAsync();
        Assert.IsEmpty(removed.Live.Pins);
        Assert.IsEmpty(removed.Historical.Pins);
        Assert.AreEqual(1, removed.Historical.Visitors);
        await service.Disconnect(connection.Id);
    }

    [TestMethod]
    public async Task Invalid_coordinates_never_reach_history()
    {
        var history = new MemoryGlobeHistory();
        var service = new GlobePresence(history);
        foreach (var (longitude, latitude) in new (double?, double?)[] { (181, 0), (0, 91), (double.NaN, 0), (0, double.PositiveInfinity), (null, 0) })
            await Assert.ThrowsAsync<ArgumentException>(() => service.SetLocation("one", longitude, latitude, default));
        Assert.AreEqual(0, history.LocationWrites);
    }

    [TestMethod]
    public async Task Anonymous_upgrade_rekeys_live_tabs_without_double_counting()
    {
        var service = new GlobePresence(new MemoryGlobeHistory());
        await service.Visit("b:guest", null, default);
        var guest = await service.Connect("b:guest", default);
        await service.Visit("b:guest", "a:account", default);
        var signedIn = await service.Connect("a:account", default);
        var snapshot = await guest.Updates.ReadAsync();
        Assert.AreEqual(1, snapshot.Live.Visitors);
        Assert.AreEqual(1, snapshot.Historical.Visitors);
        await service.Disconnect(guest.Id);
        await service.Disconnect(signedIn.Id);
    }

    [TestMethod]
    public async Task Total_stream_cap_leaves_room_for_other_API_requests()
    {
        var service = new GlobePresence(new MemoryGlobeHistory());
        var connections = new List<Guid>();
        for (var i = 0; i < 32; i++)
        {
            var visitor = $"visitor-{i}";
            await service.Visit(visitor, null, default);
            connections.Add((await service.Connect(visitor, default)).Id);
        }
        await service.Visit("overflow", null, default);
        await Assert.ThrowsAsync<GlobeConnectionLimitException>(() => service.Connect("overflow", default));
        await service.Disconnect(connections[0]);
        var available = await service.Connect("overflow", default);
        Assert.AreEqual(32, (await available.Updates.ReadAsync()).Live.Visitors);
        foreach (var id in connections.Skip(1)) await service.Disconnect(id);
        await service.Disconnect(available.Id);
    }

    [TestMethod]
    public async Task Connection_limits_do_not_leave_ghost_visitors()
    {
        var service = new GlobePresence(new MemoryGlobeHistory());
        await service.Visit("one", null, default);
        var ids = new List<Guid>();
        for (var i = 0; i < 8; i++) ids.Add((await service.Connect("one", default)).Id);
        await Assert.ThrowsAsync<GlobeConnectionLimitException>(() => service.Connect("one", default));
        foreach (var id in ids) await service.Disconnect(id);
        var fresh = await service.Connect("one", default);
        Assert.AreEqual(1, (await fresh.Updates.ReadAsync()).Live.Visitors);
        await service.Disconnect(fresh.Id);
    }
}

// Deterministic transport test double. Real SQL behavior has separate persistence tests.
internal sealed class MemoryGlobeHistory : IGlobeHistory
{
    private readonly Dictionary<string, (double? Lon, double? Lat)> visitors = [];
    private readonly Dictionary<string, string> browsers = [];
    public int LocationWrites { get; private set; }
    public Task<GlobeVisit> Visit(string browser, string? account, CancellationToken cancellationToken)
    {
        var previous = browsers.GetValueOrDefault(browser, browser);
        var id = account ?? previous;
        string? merged = null;
        if (previous != id && previous.StartsWith("b:") && visitors.Remove(previous)) merged = previous;
        visitors.TryAdd(id, (null, null));
        browsers[browser] = id;
        return Task.FromResult(new GlobeVisit(id, merged, visitors[id].Lon != null));
    }
    public Task SetLocation(string visitor, double? longitude, double? latitude, CancellationToken cancellationToken)
    {
        LocationWrites++;
        visitors[visitor] = (longitude, latitude);
        return Task.CompletedTask;
    }
    public Task<GlobeHistorySummary> Summary(string[]? selected, CancellationToken cancellationToken)
    {
        var rows = visitors.Where(x => selected is null || selected.Contains(x.Key)).ToArray();
        var pins = rows.Where(x => x.Value.Lon != null).GroupBy(x => x.Value)
            .Select(x => new GlobePin(x.Key.Lon!.Value, x.Key.Lat!.Value, x.Count())).ToArray();
        return Task.FromResult(new GlobeHistorySummary(rows.Length, rows.Length == 0 ? null : DateTimeOffset.UtcNow, pins));
    }
}

using Atlas.Api.Globe;
using Atlas.Persistence.Globe;

namespace Atlas.Api.Tests;

[TestClass]
public sealed class GlobeServiceTests
{
    [TestMethod]
    public async Task Only_opted_in_accounts_appear_and_more_than_100_are_supported()
    {
        var clock = new GlobeClock();
        var history = new MemoryGlobeHistory();
        history.Rows["b:old-guest"] = new() { Id = "b:old-guest", FirstSeenAt = clock.GetUtcNow(), LastSeenAt = clock.GetUtcNow(), Longitude = 1, Latitude = 2 };
        var service = new GlobeService(history, clock);
        await service.Activity("a:no-pin", default);
        Assert.AreEqual(1, history.Rows.Count);
        for (var i = 0; i < 120; i++) await service.SetLocation($"a:{i}", i - 60, 10, default);
        var view = await service.View(null, default);
        Assert.AreEqual(120, view.Live.Visitors);
        Assert.AreEqual(120, view.Historical.Visitors);
        Assert.AreEqual(120, view.Live.Pins.Sum(x => x.Visitors));
        Assert.IsFalse(view.CanShare);
        Assert.IsFalse(view.HasLocation);
    }

    [TestMethod]
    public async Task Activity_expires_from_live_but_retains_history_and_account_uniqueness()
    {
        var clock = new GlobeClock();
        var service = new GlobeService(new MemoryGlobeHistory(), clock);
        await service.SetLocation("a:one", 10, 20, default);
        await service.SetLocation("a:one", 10, 20, default);
        Assert.AreEqual(1, (await service.Activity("a:one", default)).Live.Visitors);
        clock.Now = clock.Now.AddMinutes(6);
        var away = await service.View("a:one", default);
        Assert.AreEqual(0, away.Live.Visitors);
        Assert.AreEqual(1, away.Historical.Visitors);
        Assert.IsTrue(away.HasLocation);
        Assert.AreEqual(1, (await service.Activity("a:one", default)).Live.Visitors);
    }

    [TestMethod]
    public async Task Removing_pin_clears_both_views_and_activity_does_not_recreate_it()
    {
        var service = new GlobeService(new MemoryGlobeHistory(), new GlobeClock());
        var added = await service.SetLocation("a:one", -122.432, 47.654, default);
        Assert.AreEqual(new GlobePin(-122, 48, 1), added.Live.Pins.Single());
        var removed = await service.SetLocation("a:one", null, null, default);
        Assert.IsFalse(removed.HasLocation);
        Assert.AreEqual(0, removed.Historical.Visitors);
        Assert.AreEqual(0, (await service.Activity("a:one", default)).Live.Visitors);
    }

    [TestMethod]
    public async Task Public_snapshot_is_cached_but_personal_sharing_state_is_not_shared()
    {
        var clock = new GlobeClock();
        var history = new MemoryGlobeHistory();
        var service = new GlobeService(history, clock);
        await service.SetLocation("a:one", 10, 20, default);
        for (var i = 0; i < 100; i++) Assert.IsFalse((await service.View(null, default)).HasLocation);
        Assert.IsTrue((await service.View("a:one", default)).HasLocation);
        Assert.IsFalse((await service.View("a:other", default)).HasLocation);
        Assert.AreEqual(1, history.SnapshotReads);
        clock.Now = clock.Now.AddSeconds(11);
        await service.View(null, default);
        Assert.AreEqual(2, history.SnapshotReads);
    }

    [TestMethod]
    public async Task Invalid_coordinates_never_reach_storage()
    {
        var history = new MemoryGlobeHistory();
        var service = new GlobeService(history, new GlobeClock());
        foreach (var (longitude, latitude) in new (double?, double?)[] { (181, 0), (0, 91), (double.NaN, 0), (0, double.PositiveInfinity), (null, 0) })
            await Assert.ThrowsAsync<ArgumentException>(() => service.SetLocation("a:one", longitude, latitude, default));
        Assert.AreEqual(0, history.Rows.Count);
    }
}

internal sealed class GlobeClock : TimeProvider
{
    public DateTimeOffset Now { get; set; } = DateTimeOffset.Parse("2026-11-08T12:00:00Z");
    public override DateTimeOffset GetUtcNow() => Now;
}

internal sealed class MemoryGlobeHistory : IGlobeHistory
{
    public Dictionary<string, GlobeVisitorRow> Rows { get; } = [];
    public int SnapshotReads { get; private set; }
    public Task<bool> HasPin(string account, CancellationToken cancellationToken) => Task.FromResult(
        Rows.TryGetValue(account, out var row) && row.Id.StartsWith("a:") && row.Longitude != null && row.Latitude != null);
    public Task SetLocation(string account, double? longitude, double? latitude, DateTimeOffset now, CancellationToken cancellationToken)
    {
        if (longitude is null) Rows.Remove(account);
        else Rows[account] = new() { Id = account, Longitude = longitude, Latitude = latitude, LastSeenAt = now,
            FirstSeenAt = Rows.TryGetValue(account, out var old) ? old.FirstSeenAt : now };
        return Task.CompletedTask;
    }
    public Task<bool> Touch(string account, DateTimeOffset now, CancellationToken cancellationToken)
    {
        if (!Rows.TryGetValue(account, out var row) || row.Longitude is null || row.LastSeenAt > now.AddSeconds(-45)) return Task.FromResult(false);
        row.LastSeenAt = now;
        return Task.FromResult(true);
    }
    public Task<GlobeSnapshot> Snapshot(DateTimeOffset now, DateTimeOffset onlineSince, CancellationToken cancellationToken)
    {
        SnapshotReads++;
        var rows = Rows.Values.Where(x => x.Id.StartsWith("a:") && x.Longitude != null && x.Latitude != null).ToArray();
        GlobeHistorySummary Summary(bool live)
        {
            var selected = rows.Where(x => !live || x.LastSeenAt >= onlineSince).ToArray();
            return new(selected.Length, selected.Length == 0 ? null : selected.Min(x => x.FirstSeenAt),
                selected.GroupBy(x => (x.Longitude, x.Latitude)).Select(x => new GlobePin(x.Key.Longitude!.Value, x.Key.Latitude!.Value, x.Count())).ToArray());
        }
        return Task.FromResult(new GlobeSnapshot(Summary(true), Summary(false), now));
    }
}

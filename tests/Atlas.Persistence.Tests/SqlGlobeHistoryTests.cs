using Atlas.Persistence.Globe;

namespace Atlas.Persistence.Tests;

[TestClass]
public sealed class SqlGlobeHistoryTests
{
    [TestMethod]
    public async Task Pin_history_persists_activity_expires_and_removal_clears_both_views()
    {
        using var database = SqlTestDatabase.Create();
        var history = new SqlGlobeHistory(database.Open);
        var now = DateTimeOffset.UtcNow;
        Assert.IsFalse(await history.Touch("a:no-pin", now, default));
        Assert.AreEqual(0, (await history.Snapshot(now, now.AddMinutes(-5), default)).Historical.Visitors);
        await history.SetLocation("a:member", -122, 48, now, default);
        var reloaded = new SqlGlobeHistory(database.Open);
        Assert.IsTrue(await reloaded.HasPin("a:member", default));
        var later = now.AddMinutes(6);
        var away = await reloaded.Snapshot(later, later.AddMinutes(-5), default);
        Assert.AreEqual(0, away.Live.Visitors);
        Assert.AreEqual(1, away.Historical.Visitors);
        Assert.IsTrue(await reloaded.Touch("a:member", later, default));
        Assert.IsFalse(await reloaded.Touch("a:member", later.AddSeconds(1), default));
        Assert.AreEqual(1, (await history.Snapshot(later, later.AddMinutes(-5), default)).Live.Visitors);
        await reloaded.SetLocation("a:member", null, null, later, default);
        Assert.AreEqual(0, (await history.Snapshot(later, later.AddMinutes(-5), default)).Historical.Visitors);
    }

    [TestMethod]
    public async Task Snapshots_ignore_legacy_anonymous_rows_and_include_over_100_account_pins()
    {
        using var database = SqlTestDatabase.Create();
        var now = DateTimeOffset.UtcNow;
        using (var db = database.Open())
        {
            db.Add(new GlobeVisitorRow { Id = "b:legacy", Longitude = 1, Latitude = 2, FirstSeenAt = now, LastSeenAt = now });
            for (var i = 0; i < 120; i++) db.Add(new GlobeVisitorRow { Id = $"a:{i}", Longitude = 10, Latitude = 20, FirstSeenAt = now, LastSeenAt = now });
            await db.SaveChangesAsync();
        }
        var view = await new SqlGlobeHistory(database.Open).Snapshot(now, now.AddMinutes(-5), default);
        Assert.AreEqual(120, view.Live.Visitors);
        Assert.AreEqual(new GlobePin(10, 20, 120), view.Historical.Pins.Single());
    }
}

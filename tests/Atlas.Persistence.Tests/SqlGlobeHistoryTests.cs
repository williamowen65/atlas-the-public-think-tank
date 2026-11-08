using Atlas.Persistence.Globe;

namespace Atlas.Persistence.Tests;

[TestClass]
public sealed class SqlGlobeHistoryTests
{
    [TestMethod]
    public async Task History_survives_new_repository_and_anonymous_account_merge()
    {
        using var database = SqlTestDatabase.Create();
        var history = new SqlGlobeHistory(database.Open);
        await history.Visit("b:first", null, default);
        await history.SetLocation("b:first", -122, 48, default);
        var reloaded = new SqlGlobeHistory(database.Open);
        Assert.AreEqual(1, (await reloaded.Summary(null, default)).Visitors);
        Assert.HasCount(1, (await reloaded.Summary(null, default)).Pins);
        var upgrade = await reloaded.Visit("b:first", "a:member", default);
        Assert.AreEqual("b:first", upgrade.MergedVisitorId);
        await reloaded.Visit("b:second", "a:member", default);
        Assert.AreEqual(1, (await reloaded.Summary(null, default)).Visitors);
        Assert.IsEmpty((await reloaded.Summary(null, default)).Pins);
        await reloaded.SetLocation("a:member", -123, 49, default);
        Assert.AreEqual(new GlobePin(-123, 49, 1), (await history.Summary(null, default)).Pins.Single());
        await reloaded.SetLocation("a:member", null, null, default);
        Assert.IsEmpty((await history.Summary(null, default)).Pins);
    }

    [TestMethod]
    public async Task Coincident_pins_aggregate_and_accounts_on_one_browser_remain_distinct()
    {
        using var database = SqlTestDatabase.Create();
        var history = new SqlGlobeHistory(database.Open);
        await history.Visit("b:shared", "a:first", default);
        await history.Visit("b:shared", "a:second", default);
        await history.SetLocation("a:first", 10, 20, default);
        await history.SetLocation("a:second", 10, 20, default);
        var all = await history.Summary(null, default);
        Assert.AreEqual(2, all.Visitors);
        Assert.AreEqual(new GlobePin(10, 20, 2), all.Pins.Single());
        Assert.AreEqual(1, (await history.Summary(["a:first"], default)).Visitors);
        Assert.AreEqual(0, (await history.Summary([], default)).Visitors);
        Assert.IsNull((await history.Summary([], default)).Since);
    }
}

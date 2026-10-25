using Atlas.Testing;

namespace Atlas.Persistence.Tests;

[TestClass]
public sealed class DiscoveryRankingScenarioTests
{
    private static RankingScenario[] Load() => SearchRankingFixture.Load(Path.Combine(AppContext.BaseDirectory, "SearchRanking", "scenarios.json"));
    public static IEnumerable<object[]> Scenarios => Load().Select(s => new object[] { s.Id });

    [TestMethod]
    public void SharedStorybookFixturesAreValidWithoutSql()
    {
        var scenarios = Load();
        Assert.AreEqual(scenarios.Length, scenarios.Select(s => s.Id).Distinct().Count());
        foreach (var scenario in scenarios) SearchRankingFixture.Validate(scenario);
        Assert.IsTrue(scenarios.Any(s => s.Phases.Any(p => p.SameScoresAs is not null)), "Undo coverage must stay in the shared fixtures.");
        Assert.IsTrue(scenarios.Any(s => s.Entries.Length > 3), "Exercise a real page boundary.");
    }

    [TestMethod, DynamicData(nameof(Scenarios))]
    public void SharedStorybookScenarioMeetsRankingExpectations(string id)
    {
        using var database = SqlTestDatabase.Create(useCurrentModel: true);
        var result = SearchRankingFixture.Run(database.Open, Load().Single(s => s.Id == id));
        foreach (var phase in result.Phases)
            foreach (var check in phase.Checks)
                Assert.IsTrue(check.Passed, $"{id} / {phase.Name}: {check.Description}\n{System.Text.Json.JsonSerializer.Serialize(phase.Rows, SearchRankingFixture.Json)}");
    }
}

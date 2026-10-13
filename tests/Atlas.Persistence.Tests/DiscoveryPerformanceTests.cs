using System.Data.Common;
using System.Diagnostics;
using System.Text;
using Atlas.Discovery;
using Atlas.Persistence.Discovery;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Atlas.Persistence.Tests;

[TestClass]
public sealed class DiscoveryPerformanceTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    [DataRow(100)]
    [DataRow(1000)]
    [DataRow(10000)]
    public void Pages_materialize_only_three_cards_with_constant_query_count(int nodes)
    {
        using var fixture = SqlTestDatabase.Create(useCurrentModel: true);
        using (var seed = fixture.Open())
            seed.Database.ExecuteSqlInterpolated($"""
                WITH Numbers AS (SELECT TOP ({nodes}) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS Number
                    FROM sys.all_objects a CROSS JOIN sys.all_objects b)
                INSERT INTO NodeRows (Id, Title, Status, CreatedAt, UpdatedAt)
                SELECT NEWID(), CONCAT(N'Node ', RIGHT(CONCAT(N'000000', Number), 6)), N'Active', '2026-10-01', '2026-10-01' FROM Numbers
                """);
        var collector = new QueryCollector();
        using var configuration = fixture.Open();
        var connection = configuration.Database.GetConnectionString()!;
        AtlasDataContext Open() => new(new DbContextOptionsBuilder<AtlasDataContext>().UseSqlServer(connection).AddInterceptors(collector).Options);
        using var cache = new DiscoveryCache();
        var query = new SqlDiscoveryReadQuery(Open, cache);
        var watch = Stopwatch.StartNew();
        var cold = query.DiscoverPage(new()); watch.Stop();
        var coldMs = watch.Elapsed.TotalMilliseconds;
        Assert.AreEqual(nodes, cold.TotalCount);
        Assert.HasCount(3, cold.Items);
        Assert.AreEqual(3, cold.Items.Count(item => item.Card is not null));
        Assert.HasCount(4, collector.Commands); // revision, total, ranked page, batched cards
        var plans = collector.Commands.Where(c => c.Text.Contains("WITH NodeStats") || c.Text.Contains("WITH Selected")).ToArray();
        collector.Commands.Clear();
        watch.Restart(); query.DiscoverPage(new()); watch.Stop(); var hotMs = watch.Elapsed.TotalMilliseconds;
        Assert.HasCount(1, collector.Commands); // committed generation/visibility freshness check
        collector.Commands.Clear();
        var changed = query.DiscoverPage(new(SearchText: "Node"));
        Assert.HasCount(3, collector.Commands); // details reused across filter keys
        Assert.HasCount(3, changed.Items);
        collector.Commands.Clear();
        var next = query.DiscoverPage(new(Page: 2));
        Assert.HasCount(4, collector.Commands);
        CollectionAssert.AreEqual(new[] { 4, 5, 6 }, next.Items.Select(item => item.Rank).ToArray());
        collector.Commands.Clear();
        Assert.IsEmpty(query.DiscoverPage(new(Page: int.MaxValue)).Items);
        Assert.HasCount(2, collector.Commands);
        using var tracking = Open();
        Assert.AreEqual(0, tracking.ChangeTracker.Entries().Count());

        var folder = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../TestResults"));
        Directory.CreateDirectory(folder);
        var report = $"Nodes: {nodes}\nCold page: {coldMs:F2} ms; warm page: {hotMs:F2} ms\nQueries cold/warm/filter-change/next/beyond: 4/1/3/4/2\nMaterialized ranked IDs/cards per normal page: 3/3\nCache page hits/misses: {cache.PageHits}/{cache.PageMisses}; detail hits/misses: {cache.DetailHits}/{cache.DetailMisses}\n";
        File.WriteAllText(Path.Combine(folder, $"discovery-{nodes}-measurements.txt"), report);
        TestContext.WriteLine(report);
        // Re-execute the same parameterized statements with actual execution-plan collection.
        // This uses only isolated fixture data and does not change schema or migration files.
        using var sql = new SqlConnection(connection); sql.Open();
        using (var enable = sql.CreateCommand()) { enable.CommandText = "SET STATISTICS XML ON"; enable.ExecuteNonQuery(); }
        for (var i = 0; i < plans.Length; i++)
        {
            using var command = sql.CreateCommand(); command.CommandText = plans[i].Text;
            foreach (var parameter in plans[i].Parameters) command.Parameters.Add(((ICloneable)parameter).Clone());
            using var reader = command.ExecuteReader();
            do
            {
                while (reader.Read())
                    if (reader.FieldCount == 1 && reader.GetFieldType(0) == typeof(string) && reader.GetString(0).Contains("ShowPlanXML"))
                        File.WriteAllText(Path.Combine(folder, $"discovery-{nodes}-plan-{i}.sqlplan"), reader.GetString(0));
            } while (reader.NextResult());
        }
    }

    private sealed class QueryCollector : DbCommandInterceptor
    {
        public List<(string Text, SqlParameter[] Parameters)> Commands { get; } = [];
        public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData data, InterceptionResult<DbDataReader> result)
        {
            Commands.Add((command.CommandText, command.Parameters.Cast<SqlParameter>().Select(p => (SqlParameter)((ICloneable)p).Clone()).ToArray()));
            return result;
        }
    }
}

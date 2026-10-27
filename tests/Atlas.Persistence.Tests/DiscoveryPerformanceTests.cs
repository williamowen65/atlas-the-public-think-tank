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
    [TestCategory("FullTextSearch")]
    [DataRow(100)]
    [DataRow(1000)]
    [DataRow(10000)]
    public void Pages_materialize_only_three_cards_with_constant_query_count(int nodes)
    {
        var totalTime = Stopwatch.StartNew();
        var phaseTime = new Stopwatch();
        string? phase = null;
        SqlTestDatabase? fixture = null;
        Exception? testFailure = null;
        void Log(string message) => TestContext.WriteLine(
            $"[{DateTimeOffset.UtcNow:O}] nodes={nodes}; total={totalTime.Elapsed.TotalSeconds:F2}s; {message}");
        void Phase(string name)
        {
            if (phase is not null) Log($"DONE {phase}; phase={phaseTime.Elapsed.TotalSeconds:F2}s");
            phase = name;
            phaseTime.Restart();
            Log($"START {phase}");
        }
        try
        {
            Phase("Create isolated database, schema and full-text indexes");
            fixture = SqlTestDatabase.Create(useCurrentModel: true);
            Phase("Seed participants, nodes, documents, blocks and votes");
            using (var seed = fixture.Open())
                seed.Database.ExecuteSqlInterpolated($"""
                    DECLARE @voter uniqueidentifier = NEWID(), @type uniqueidentifier = NEWID();
                    INSERT INTO ParticipantRows (Id, DisplayName, Bio, IsActive, CreatedAt, UpdatedAt)
                    VALUES (@voter, N'Performance author', N'', 1, '2026-10-01', '2026-10-01');
                    INSERT INTO NodeTypeRows (Id, Name, Description, IsSystemDefined, IsArchived, AutoPluralize, CreatedAt, UpdatedAt)
                    VALUES (@type, N'Idea', N'Ideas', 1, 0, 1, '2026-10-01', '2026-10-01');
                    DECLARE @nodes TABLE (Id uniqueidentifier, DocumentId uniqueidentifier, BlockId uniqueidentifier, Number int);
                    INSERT INTO @nodes SELECT TOP ({nodes}) NEWID(), NEWID(), NEWID(), ROW_NUMBER() OVER (ORDER BY (SELECT NULL))
                        FROM sys.all_objects a CROSS JOIN sys.all_objects b;
                    INSERT INTO BlockRows (Id, Kind, Markdown, CreatedAt, UpdatedAt)
                    SELECT BlockId, N'markdown', REPLICATE(N'Climate adaptation evidence 世界 🐋. ', 16), '2026-10-01', '2026-10-01' FROM @nodes;
                    INSERT INTO DocumentRows (Id, CreatedAt, UpdatedAt)
                    SELECT DocumentId, '2026-10-01', '2026-10-01' FROM @nodes;
                    INSERT INTO DocumentBlocks (DocumentId, Position, BlockId) SELECT DocumentId, 0, BlockId FROM @nodes;
                    INSERT INTO NodeRows (Id, DescriptionId, TypeId, AuthorId, Title, Status, CreatedAt, UpdatedAt)
                    SELECT Id, DocumentId, @type, @voter, CONCAT(N'Node ', RIGHT(CONCAT(N'000000', Number), 6)), N'Active', '2026-10-01', '2026-10-01' FROM @nodes;
                    INSERT INTO VoteRows (Id, ParticipantId, TargetId, TargetType, Value, CreatedAt, UpdatedAt)
                    SELECT NEWID(), @voter, Id, N'Node', Number % 11, '2026-10-01', '2026-10-01' FROM @nodes;
                    """);
            var collector = new QueryCollector();
            Phase("Wait for full-text index readiness");
            using var configuration = fixture.OpenForDiscovery();
            var connection = configuration.Database.GetConnectionString()!;
            AtlasDataContext Open() => new(new DbContextOptionsBuilder<AtlasDataContext>().UseSqlServer(connection).AddInterceptors(collector).Options);
            using var cache = new DiscoveryCache();
            var query = new SqlDiscoveryReadQuery(Open, cache);
            var watch = Stopwatch.StartNew();
            Phase("Cold discovery page");
            var cold = query.DiscoverPage(new()); watch.Stop();
            var coldMs = watch.Elapsed.TotalMilliseconds;
            Assert.AreEqual(nodes, cold.TotalCount);
            Assert.HasCount(3, cold.Items);
            Assert.AreEqual(3, cold.Items.Count(item => item.Card is not null));
            Assert.HasCount(6, collector.Commands); // revision, total, ranked page, EF metadata, SQL summaries, root-hop check
            var plans = collector.Commands.Where(c => c.Text.Contains("Discovery count") || c.Text.Contains("Discovery ranked page") || c.Text.Contains("WITH Selected") || c.Text.Contains("Discovery card metadata") || c.Text.Contains("Discovery breadcrumb hop")).ToList();
            collector.Commands.Clear();
            Phase("Warm cached discovery page");
            watch.Restart(); query.DiscoverPage(new()); watch.Stop(); var hotMs = watch.Elapsed.TotalMilliseconds;
            Assert.HasCount(1, collector.Commands); // committed generation/visibility freshness check
            collector.Commands.Clear();
            Phase("Text-filtered discovery page");
            watch.Restart();
            var changed = query.DiscoverPage(new(SearchText: "Node"));
            watch.Stop(); var filterMs = watch.Elapsed.TotalMilliseconds;
            plans.AddRange(collector.Commands.Where(command => command.Text.Contains("Discovery count") || command.Text.Contains("Discovery ranked page")));
            Assert.HasCount(4, collector.Commands); // availability check, revision, count, page; details reused
            Assert.HasCount(3, changed.Items);
            collector.Commands.Clear();
            Phase("Next discovery page");
            var next = query.DiscoverPage(new(Page: 2));
            Assert.HasCount(6, collector.Commands);
            CollectionAssert.AreEqual(new[] { 4, 5, 6 }, next.Items.Select(item => item.Rank).ToArray());
            collector.Commands.Clear();
            Phase("Beyond-last discovery page");
            Assert.IsEmpty(query.DiscoverPage(new(Page: int.MaxValue)).Items);
            Assert.HasCount(2, collector.Commands);
            using var tracking = Open();
            Assert.AreEqual(0, tracking.ChangeTracker.Entries().Count());
    
            Phase("Read database size and write measurements");
            var folder = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../TestResults"));
            Directory.CreateDirectory(folder);
            using var sizeRead = fixture.Open();
            var reservedBytes = sizeRead.Database.SqlQueryRaw<long>("SELECT CONVERT(bigint, SUM(reserved_page_count)) * 8192 AS [Value] FROM sys.dm_db_partition_stats").AsEnumerable().Single();
            var report = $"Nodes/documents/blocks/ratings: {nodes} each; database reserved bytes: {reservedBytes}\nCold page: {coldMs:F2} ms; warm page: {hotMs:F2} ms; filter-change page: {filterMs:F2} ms\nQueries cold/warm/filter-change/next/beyond: 6/1/3/6/2\nMaterialized ranked IDs/cards per normal page: 3/3\nCache page hits/misses: {cache.PageHits}/{cache.PageMisses}; detail hits/misses: {cache.DetailHits}/{cache.DetailMisses}\n";
            File.WriteAllText(Path.Combine(folder, $"discovery-{nodes}-measurements.txt"), report);
            TestContext.WriteLine(report);
            // Re-execute the same parameterized statements with actual execution-plan collection.
            // This uses only isolated fixture data and does not change schema or migration files.
            Phase("Open execution-plan connection and enable STATISTICS XML");
            using var sql = new SqlConnection(connection); sql.Open();
            using (var enable = sql.CreateCommand()) { enable.CommandText = "SET STATISTICS XML ON"; enable.ExecuteNonQuery(); }
            for (var i = 0; i < plans.Count; i++)
            {
                Phase($"Collect execution plan {i + 1}/{plans.Count} (execute and drain results)");
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
            Log($"DONE {phase}; phase={phaseTime.Elapsed.TotalSeconds:F2}s");
            phase = null;
        }
        catch (Exception error)
        {
            testFailure = error;
            Log($"FAILED {phase}; phase={phaseTime.Elapsed.TotalSeconds:F2}s; {error}");
            throw;
        }
        finally
        {
            if (fixture is not null)
            {
                phase = null;
                Phase("Delete isolated test database");
                try
                {
                    fixture.Dispose();
                    Log($"DONE {phase}; phase={phaseTime.Elapsed.TotalSeconds:F2}s");
                }
                catch (Exception cleanupError) when (testFailure is not null)
                {
                    // Keep the original failing SQL/assertion stack as the reported test failure.
                    Log($"CLEANUP ALSO FAILED; phase={phaseTime.Elapsed.TotalSeconds:F2}s; {cleanupError}");
                }
                catch (Exception cleanupError)
                {
                    Log($"FAILED cleanup; phase={phaseTime.Elapsed.TotalSeconds:F2}s; {cleanupError}");
                    throw;
                }
            }
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

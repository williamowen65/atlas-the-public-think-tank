using Microsoft.EntityFrameworkCore;

namespace Atlas.Persistence.Tests;

[TestClass]
public sealed class SqlTestDatabasePoolTests
{
    [TestMethod]
    public void SequentialLeasesStartEmptyAndKeepFullTextAndConstraints()
    {
        // Pool reset checks committed rows and SQL metadata. It does not need the
        // asynchronous indexer to finish: dedicated search tests cover indexed results.
        using (var first = SqlTestDatabase.Create(useCurrentModel: true))
        {
            var now = DateTimeOffset.UtcNow;
            DiscoverySqlFixture.Seed(first, new DiscoveryCandidate(
                Guid.NewGuid(), "Pool sentinel", "Only the first lease", Guid.NewGuid(),
                Guid.NewGuid(), false, now, now, 0, null, [], []));
            using (var seeded = first.Open())
                Assert.AreEqual(1, seeded.NodeRows.Count());
            // Check this exact database after reset, independent of parallel pool scheduling.
            first.ResetData();
            using var reset = first.Open();
            Assert.AreEqual(0, reset.NodeRows.Count());
            Assert.AreEqual(0, reset.ParticipantRows.Count());
            Assert.AreEqual(0, reset.Database.SqlQuery<int>($"SELECT COUNT(*) AS [Value] FROM dbo.SearchDocuments").Single());
            first.AddParticipant();
            first.AddNode();
        }

        // Scheduling may choose another clean database; either lease must be isolated.
        using var second = SqlTestDatabase.Create(useCurrentModel: true);
        using var db = second.Open();
        Assert.AreEqual(0, db.NodeRows.Count());
        Assert.AreEqual(0, db.ParticipantRows.Count());
        Assert.AreEqual(0, db.Database.SqlQuery<int>($"SELECT COUNT(*) AS [Value] FROM dbo.SearchDocuments").Single());
        Assert.AreEqual(4, db.Database.SqlQuery<int>($"SELECT COUNT(*) AS [Value] FROM sys.fulltext_indexes WHERE is_enabled = 1").Single());
        Assert.AreEqual(0, db.Database.SqlQuery<int>($"SELECT COUNT(*) AS [Value] FROM sys.foreign_keys WHERE is_disabled = 1 OR is_not_trusted = 1").Single());
        Assert.AreEqual(0, db.Database.SqlQuery<int>($"SELECT COUNT(*) AS [Value] FROM sys.triggers WHERE parent_class = 1 AND is_disabled = 1").Single());
        // The seeded singleton survives reset and still supports committed writes.
        second.AddParticipant();
        second.AddNode();
    }

    [TestMethod]
    public void SimultaneousLeasesUseDifferentDatabases()
    {
        // Holding both leases models the class-level parallel runner.
        using var first = SqlTestDatabase.Create(useCurrentModel: true);
        using var second = SqlTestDatabase.Create(useCurrentModel: true);
        using var left = first.Open();
        using var right = second.Open();
        Assert.AreNotEqual(left.Database.GetDbConnection().Database, right.Database.GetDbConnection().Database);
        first.AddParticipant();
        Assert.AreEqual(0, right.ParticipantRows.Count());
    }
}

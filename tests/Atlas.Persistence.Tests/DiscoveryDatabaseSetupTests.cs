using Atlas.Discovery;
using Atlas.Persistence.Discovery;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Persistence.Tests;

[TestClass]
public sealed class DiscoveryDatabaseSetupTests
{
    [TestMethod]
    public void FreshModelWithoutHistoricalMigrationSqlGainsWorkingSearchAndSetupIsRepeatable()
    {
        // EnsureCreated represents the current EF schema without replaying historical SQL.
        // This deliberately bypasses the fixture's normal full-text installation.
        using var fixture = SqlTestDatabase.Create(useCurrentModel: true, installFullText: false, reuse: false);
        using (var db = fixture.Open())
        {
            Assert.AreEqual(0, db.Database.SqlQuery<int>($"SELECT COUNT(*) AS [Value] FROM sys.fulltext_indexes").Single());
        }
        var now = DateTimeOffset.UtcNow;
        var node = new DiscoveryCandidate(Guid.NewGuid(), "Protecting salmon", "Healthy rivers", Guid.NewGuid(), Guid.NewGuid(), false, now, now, 0, null, [], []);
        DiscoverySqlFixture.Seed(fixture, node);
        using (var db = fixture.Open()) AtlasDatabaseSetup.InstallSupplementalSchema(db);
        var mixed = new SqlPublicSearch(fixture.OpenForDiscovery);
        Assert.AreEqual(node.NodeId, mixed.Search(new(SearchText: "salmon")).Items.Single().NodeId);
        var search = new SqlDiscoveryReadQuery(fixture.OpenForDiscovery);
        Assert.AreEqual(node.NodeId, search.DiscoverPage(new(SearchText: "salmon")).Items.Single().NodeId);
        using (var db = fixture.Open())
        {
            AtlasDatabaseSetup.InstallSupplementalSchema(db);
            Assert.AreEqual(4, db.Database.SqlQuery<int>($"SELECT COUNT(*) AS [Value] FROM sys.fulltext_indexes").Single());
            Assert.AreEqual(node.Title, db.NodeRows.Single().Title);
            Assert.AreEqual(1, db.Database.SqlQuery<int>($"SELECT CONVERT(int, snapshot_isolation_state) AS [Value] FROM sys.databases WHERE database_id = DB_ID()").Single());
        }
        Assert.AreEqual(node.NodeId, search.DiscoverPage(new(SearchText: "rivers")).Items.Single().NodeId);
    }

    [TestMethod]
    public void ExplicitSetupCanRunRepeatedlyAfterTheCurrentMigrationChain()
    {
        using var fixture = SqlTestDatabase.Create(reuse: false);
        using var db = fixture.Open();
        AtlasDatabaseSetup.Apply(db);
        AtlasDatabaseSetup.Apply(db);
        Assert.AreEqual(4, db.Database.SqlQuery<int>($"SELECT COUNT(*) AS [Value] FROM sys.fulltext_indexes").Single());
    }

    [TestMethod]
    public void SupplementalSetupRejectsAnOuterTransaction()
    {
        using var fixture = SqlTestDatabase.Create(useCurrentModel: true, reuse: false);
        using var db = fixture.Open();
        using var transaction = db.Database.BeginTransaction();
        Assert.Throws<InvalidOperationException>(() => AtlasDatabaseSetup.InstallSupplementalSchema(db));
    }
}

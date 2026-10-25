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
        using var fixture = SqlTestDatabase.Create(useCurrentModel: true, installFullText: false);
        using (var db = fixture.Open())
        {
            Assert.AreEqual(0, db.Database.SqlQuery<int>($"SELECT COUNT(*) AS [Value] FROM sys.fulltext_indexes").Single());
            AtlasDatabaseSetup.InstallSupplementalSchema(db);
        }
        var now = DateTimeOffset.UtcNow;
        var node = new DiscoveryCandidate(Guid.NewGuid(), "Protecting salmon", "Healthy rivers", Guid.NewGuid(), Guid.NewGuid(), false, now, now, 0, null, [], []);
        DiscoverySqlFixture.Seed(fixture, node);
        var search = new SqlDiscoveryReadQuery(fixture.OpenForDiscovery);
        Assert.AreEqual(node.NodeId, search.DiscoverPage(new(SearchText: "salmon")).Items.Single().NodeId);
        using (var db = fixture.Open())
        {
            AtlasDatabaseSetup.InstallSupplementalSchema(db);
            Assert.AreEqual(3, db.Database.SqlQuery<int>($"SELECT COUNT(*) AS [Value] FROM sys.fulltext_indexes").Single());
            Assert.AreEqual(node.Title, db.NodeRows.Single().Title);
        }
        Assert.AreEqual(node.NodeId, search.DiscoverPage(new(SearchText: "rivers")).Items.Single().NodeId);
    }

    [TestMethod]
    public void ExplicitSetupCanRunRepeatedlyAfterTheCurrentMigrationChain()
    {
        using var fixture = SqlTestDatabase.Create();
        using var db = fixture.Open();
        AtlasDatabaseSetup.Apply(db);
        AtlasDatabaseSetup.Apply(db);
        Assert.AreEqual(3, db.Database.SqlQuery<int>($"SELECT COUNT(*) AS [Value] FROM sys.fulltext_indexes").Single());
    }

    [TestMethod]
    public void SupplementalSetupRejectsAnOuterTransaction()
    {
        using var fixture = SqlTestDatabase.Create(useCurrentModel: true);
        using var db = fixture.Open();
        using var transaction = db.Database.BeginTransaction();
        Assert.Throws<InvalidOperationException>(() => AtlasDatabaseSetup.InstallSupplementalSchema(db));
    }
}

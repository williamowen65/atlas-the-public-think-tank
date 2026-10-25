using Atlas.Discovery;
using Atlas.Persistence.Discovery;
using Atlas.Testing;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Persistence.Tests;

[TestClass]
public sealed class DiscoveryFullTextTests
{
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void UnavailableFullTextIndexRaisesAnExplicitSearchFailure(bool removeIndex)
    {
        using var fixture = SqlTestDatabase.Create(useCurrentModel: true);
        using var db = fixture.Open();
        db.Database.ExecuteSqlRaw(removeIndex
            ? "DROP FULLTEXT INDEX ON dbo.NodeRows"
            : "ALTER FULLTEXT INDEX ON dbo.NodeRows DISABLE");
        var query = new SqlDiscoveryReadQuery(fixture.Open);
        var error = Assert.Throws<SearchUnavailableException>(() => query.DiscoverPage(new(SearchText: "salmon")));
        Assert.IsInstanceOfType<InvalidOperationException>(error.InnerException);
        StringAssert.Contains(error.Message, "Text search is temporarily unavailable");
        // Filtering without free text does not depend on the disabled full-text index.
        Assert.AreEqual(0, query.DiscoverPage(new()).TotalCount);
    }

    [TestMethod]
    public void NaturalLanguageMatchesReorderedWordsInflectionsAndCombinedFilters()
    {
        using var fixture = SqlTestDatabase.Create(useCurrentModel: true);
        var now = DateTimeOffset.UtcNow;
        var first = new DiscoveryCandidate(Guid.NewGuid(), "Orcas need protection", "Protecting salmon habitats", Guid.NewGuid(), Guid.NewGuid(), false, now, now, 0, null, [], []);
        var archived = first with { NodeId = Guid.NewGuid(), IsArchived = true };
        var hidden = first with { NodeId = Guid.NewGuid(), IsModerationExcluded = true };
        var unrelated = first with { NodeId = Guid.NewGuid(), Title = "Desert geology", SearchableContent = "Sandstone erosion" };
        DiscoverySqlFixture.Seed(fixture, first, archived, hidden, unrelated);
        using var cache = new DiscoveryCache();
        var query = new SqlDiscoveryReadQuery(fixture.OpenForDiscovery, cache);
        foreach (var text in new[] { "protection orcas", "habitat", "salmon", "ORCAS", "salmon missingword" })
            Assert.AreEqual(first.NodeId, query.DiscoverPage(new(SearchText: text, NodeTypeId: first.NodeTypeId, AuthorParticipantId: first.AuthorParticipantId)).Items.Single().NodeId, text);
        Assert.IsEmpty(query.DiscoverPage(new(SearchText: "sandstone", NodeId: first.NodeId)).Items);
        Assert.AreEqual(2, query.DiscoverPage(new(SearchText: "orcas", IncludeArchived: true)).TotalCount);
        Assert.IsEmpty(query.DiscoverAuthoredPage(new(SearchText: "orcas"), [hidden.NodeId]).Items);
        Assert.IsEmpty(query.DiscoverPage(new(SearchText: "the and")).Items);
        // Quotes/operators are natural-language input, never executable SQL/CONTAINS syntax.
        query.DiscoverPage(new(SearchText: "\" OR 1=1 --"));
        Assert.AreEqual(3, query.DiscoverPage(new(IncludeArchived: true)).TotalCount); // hidden excluded
        Assert.AreEqual(0L, cache.PageHits);
    }

    [TestMethod]
    public void UpdatedBlockTextIsIndexedAndEarlierEmptySearchPagesAreNotCached()
    {
        using var fixture = SqlTestDatabase.Create(useCurrentModel: true);
        var now = DateTimeOffset.UtcNow;
        var candidate = new DiscoveryCandidate(Guid.NewGuid(), "Wildlife", "Forests", Guid.NewGuid(), Guid.NewGuid(), false, now, now, 0, null, [], []);
        DiscoverySqlFixture.Seed(fixture, candidate);
        using var cache = new DiscoveryCache();
        var query = new SqlDiscoveryReadQuery(fixture.OpenForDiscovery, cache);
        Assert.IsEmpty(query.DiscoverPage(new(SearchText: "migration")).Items);
        using (var db = fixture.Open())
        {
            db.BlockRows.Single().Markdown = "Seasonal migrations";
            db.SaveChanges();
        }
        using (var db = fixture.Open())
            SqlFullTextSupport.WaitForIndexing(db.Database.GetConnectionString()!);
        Assert.AreEqual(candidate.NodeId, query.DiscoverPage(new(SearchText: "migration")).Items.Single().NodeId);
        Assert.AreEqual(0L, cache.PageHits);
    }
}

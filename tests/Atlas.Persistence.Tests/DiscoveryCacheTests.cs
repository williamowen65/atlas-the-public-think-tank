using Atlas.Discovery;
using Atlas.Persistence.Discovery;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Persistence.Tests;

[TestClass]
public sealed class DiscoveryCacheTests
{
    private static DiscoveryCandidate Candidate(Guid id, double average, int votes = 1) => new(id, "Café " + id, "evidence", Guid.NewGuid(), Guid.NewGuid(), false, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, votes, average, [], []);

    [TestMethod]
    public void Filter_changes_reuse_details_and_node_votes_invalidate_only_pages()
    {
        using var fixture = SqlTestDatabase.Create(useCurrentModel: true);
        var a = Candidate(Guid.NewGuid(), 8); var b = Candidate(Guid.NewGuid(), 5);
        DiscoverySqlFixture.Seed(fixture, a, b);
        using var cache = new DiscoveryCache();
        var query = new SqlDiscoveryReadQuery(fixture.Open, cache);
        var page = query.DiscoverPage(new());
        Assert.AreEqual(2L, cache.DetailMisses);
        query.DiscoverPage(new(SearchText: "cafe"));
        Assert.AreEqual(2L, cache.PageMisses);
        Assert.AreEqual(2L, cache.DetailHits);
        query.DiscoverPage(new(SearchText: "  cafe  "));
        Assert.AreEqual(1L, cache.PageHits);
        using (var db = fixture.Open())
        {
            db.VoteRows.Single(row => row.TargetId == b.NodeId).Value = 10;
            db.SaveChanges();
        }
        Assert.AreEqual(b.NodeId, query.DiscoverPage(new()).Items[0].NodeId);
        Assert.AreEqual(3L, cache.PageMisses);
        Assert.AreEqual(2L, cache.DetailMisses);
    }

    [TestMethod]
    public void Rollbacks_keep_cache_generation_and_committed_hide_restore_are_seen_by_other_instances()
    {
        using var fixture = SqlTestDatabase.Create(useCurrentModel: true);
        var candidate = Candidate(Guid.NewGuid(), 8);
        DiscoverySqlFixture.Seed(fixture, candidate);
        using var cache = new DiscoveryCache(); using var otherCache = new DiscoveryCache();
        var query = new SqlDiscoveryReadQuery(fixture.Open, cache);
        var other = new SqlDiscoveryReadQuery(fixture.Open, otherCache);
        query.DiscoverPage(new()); other.DiscoverPage(new());
        using (var db = fixture.Open())
        using (var transaction = db.Database.BeginTransaction())
        {
            db.NodeRows.Single().Title = "Rolled back secret";
            db.SaveChanges();
            transaction.Rollback();
        }
        Assert.AreEqual(candidate.Title, query.DiscoverPage(new()).Items.Single().Card!.Title);
        Assert.AreEqual(1L, cache.PageHits);
        using (var db = fixture.Open())
        {
            db.ModerationCaseRows.Add(new ModerationCaseRow { Id = Guid.NewGuid(), NodeId = candidate.NodeId, ReporterId = candidate.AuthorParticipantId, ReportedTitle = candidate.Title, Reason = "Spam", Status = 2, PublicReason = 1, CreatedAt = DateTimeOffset.UtcNow });
            db.SaveChanges();
        }
        Assert.IsEmpty(query.DiscoverPage(new()).Items);
        Assert.IsEmpty(other.DiscoverPage(new()).Items);
        var authored = query.DiscoverAuthorPage(new(), candidate.AuthorParticipantId);
        StringAssert.Contains(authored.Items.Single().Card!.Title, "Hidden by moderator");
        Assert.IsEmpty(query.DiscoverAuthorPage(new(SearchText: "cafe"), candidate.AuthorParticipantId).Items);
        using (var db = fixture.Open())
        { db.ModerationCaseRows.Single().VisibilityRestoredAt = DateTimeOffset.UtcNow; db.SaveChanges(); }
        Assert.AreEqual(candidate.Title, other.DiscoverPage(new()).Items.Single().Card!.Title);
    }

    [TestMethod]
    public async Task Concurrent_misses_are_coordinated_and_failed_saves_do_not_advance_generation()
    {
        using var fixture = SqlTestDatabase.Create(useCurrentModel: true);
        var candidate = Candidate(Guid.NewGuid(), 8);
        DiscoverySqlFixture.Seed(fixture, candidate);
        using var cache = new DiscoveryCache();
        var query = new SqlDiscoveryReadQuery(fixture.Open, cache);
        await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Task.Run(() => query.DiscoverPage(new()))));
        Assert.AreEqual(1L, cache.PageMisses);
        Assert.AreEqual(7L, cache.PageHits);
        using (var db = fixture.Open())
        {
            db.NodeRows.Single().AuthorId = Guid.NewGuid();
            await Assert.ThrowsAsync<DbUpdateException>(async () => await db.SaveChangesAsync());
        }
        Assert.AreEqual(1, query.DiscoverPage(new()).TotalCount);
        Assert.AreEqual(8L, cache.PageHits);
    }

    [TestMethod]
    public void Content_relationship_archive_and_personalized_changes_are_visible()
    {
        using var fixture = SqlTestDatabase.Create(useCurrentModel: true);
        var candidate = Candidate(Guid.NewGuid(), 8);
        DiscoverySqlFixture.Seed(fixture, candidate);
        using var cache = new DiscoveryCache();
        var query = new SqlDiscoveryReadQuery(fixture.Open, cache);
        query.DiscoverPage(new(SearchText: "evidence"));
        using (var db = fixture.Open()) { db.BlockRows.Single().Markdown = "different content"; db.SaveChanges(); }
        Assert.IsEmpty(query.DiscoverPage(new(SearchText: "evidence")).Items);
        StringAssert.Contains(query.DiscoverPage(new()).Items.Single().Card!.Description, "different content");
        using (var db = fixture.Open()) { db.NodeRows.Single().Status = "Archived"; db.SaveChanges(); }
        Assert.IsEmpty(query.DiscoverPage(new()).Items);
        Assert.AreEqual(1, query.DiscoverPage(new(IncludeArchived: true)).TotalCount);
        using var read = fixture.Open();
        var voter = read.VoteRows.Single().ParticipantId;
        Assert.AreEqual(8, query.ReadParticipantVotes(voter, [candidate.NodeId])[candidate.NodeId]);
        Assert.IsEmpty(query.ReadParticipantVotes(candidate.AuthorParticipantId, [candidate.NodeId]));
    }
}

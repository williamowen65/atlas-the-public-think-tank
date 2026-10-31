using Atlas.Discovery;
using Atlas.Persistence.Discovery;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Persistence.Tests;

[TestClass]
public sealed class DiscoveryCacheTests
{
    private static DiscoveryCandidate Candidate(Guid id, double average, int votes = 1) => new(id, "Café " + id, "evidence", Guid.NewGuid(), Guid.NewGuid(), false, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, votes, average, [], []);

    [TestMethod]
    public void Node_id_lookup_is_exact_cached_and_still_obeys_visibility_and_filters()
    {
        using var fixture = SqlTestDatabase.Create(useCurrentModel: true);
        var a = Candidate(Guid.NewGuid(), 8); var b = Candidate(Guid.NewGuid(), 5);
        DiscoverySqlFixture.Seed(fixture, a, b);
        using var cache = new DiscoveryCache();
        var query = new SqlDiscoveryReadQuery(fixture.Open, cache);
        Assert.AreEqual(a.NodeId, query.DiscoverPage(new(NodeId: a.NodeId)).Items.Single().NodeId);
        Assert.AreEqual(b.NodeId, query.DiscoverPage(new(NodeId: b.NodeId)).Items.Single().NodeId);
        Assert.AreEqual(a.NodeId, query.DiscoverPage(new(NodeId: a.NodeId)).Items.Single().NodeId);
        Assert.AreEqual(1L, cache.PageHits);
        Assert.IsEmpty(query.DiscoverPage(new(NodeId: Guid.NewGuid())).Items);
        fixture.WaitForDiscoveryIndexing();
        Assert.IsEmpty(query.DiscoverPage(new(NodeId: a.NodeId, SearchText: "missing search term")).Items);
        using (var db = fixture.Open())
        {
            db.NodeRows.Single(row => row.Id == a.NodeId).Status = "Archived";
            db.SaveChanges();
        }
        Assert.IsEmpty(query.DiscoverPage(new(NodeId: a.NodeId)).Items);
        Assert.AreEqual(a.NodeId, query.DiscoverPage(new(NodeId: a.NodeId, IncludeArchived: true)).Items.Single().NodeId);
        using (var db = fixture.Open())
        {
            db.ModerationCaseRows.Add(new ModerationCaseRow { Id = Guid.NewGuid(), NodeId = a.NodeId, ReporterId = a.AuthorParticipantId, ReportedTitle = a.Title, Reason = "Spam", Status = 2, PublicReason = 1, CreatedAt = DateTimeOffset.UtcNow });
            db.SaveChanges();
        }
        Assert.IsEmpty(query.DiscoverPage(new(NodeId: a.NodeId, IncludeArchived: true)).Items);
    }

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
        fixture.WaitForDiscoveryIndexing();
        query.DiscoverPage(new(SearchText: "cafe"));
        Assert.AreEqual(1L, cache.PageMisses);
        Assert.AreEqual(2L, cache.DetailHits);
        fixture.WaitForDiscoveryIndexing();
        query.DiscoverPage(new(SearchText: "  cafe  "));
        // Full-text population does not change the page revision, so neither search is cached.
        Assert.AreEqual(0L, cache.PageHits);
        Assert.AreEqual(4L, cache.DetailHits);
        using (var db = fixture.Open())
        {
            db.VoteRows.Single(row => row.TargetId == b.NodeId).Value = 10;
            db.SaveChanges();
        }
        Assert.AreEqual(b.NodeId, query.DiscoverPage(new()).Items[0].NodeId);
        Assert.AreEqual(2L, cache.PageMisses);
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
        fixture.WaitForDiscoveryIndexing();
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
        fixture.WaitForDiscoveryIndexing();
        query.DiscoverPage(new(SearchText: "evidence"));
        using (var db = fixture.Open()) { db.BlockRows.Single().Markdown = "different content"; db.SaveChanges(); }
        fixture.WaitForDiscoveryIndexing();
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
    [TestMethod]
    public void Oversized_details_are_returned_but_not_cached_and_cache_cannot_cross_databases()
    {
        using var fixture = SqlTestDatabase.Create(useCurrentModel: true);
        var candidate = Candidate(Guid.NewGuid(), 8);
        DiscoverySqlFixture.Seed(fixture, candidate);
        using (var db = fixture.Open()) { db.BlockRows.Single().Markdown = new string('x', 80_000); db.SaveChanges(); }
        using var cache = new DiscoveryCache();
        var query = new SqlDiscoveryReadQuery(fixture.Open, cache);
        Assert.AreEqual(80_000, query.DiscoverPage(new()).Items.Single().Card!.Description.Length);
        query.DiscoverPage(new());
        Assert.AreEqual(1L, cache.PageHits);
        Assert.AreEqual(2L, cache.DetailMisses);
        Assert.AreEqual(2L, cache.RejectedEntries);
        using var other = SqlTestDatabase.Create(useCurrentModel: true);
        Assert.Throws<InvalidOperationException>(() => new SqlDiscoveryReadQuery(other.Open, cache).DiscoverPage(new()));
    }

    [TestMethod]
    public async Task Related_writes_invalidate_detail_projections_and_reaction_filters()
    {
        using var fixture = SqlTestDatabase.Create(useCurrentModel: true);
        var community = Guid.NewGuid(); var reaction = Guid.NewGuid();
        var candidate = Candidate(Guid.NewGuid(), 8) with { CommunityIds = [community], ReactionDefinitionIds = [reaction] };
        DiscoverySqlFixture.Seed(fixture, candidate);
        using var cache = new DiscoveryCache();
        var query = new SqlDiscoveryReadQuery(fixture.Open, cache);
        query.DiscoverPage(new(CommunityId: community, ReactionDefinitionIds: [reaction]));
        using (var db = fixture.Open())
        {
            db.CommunityRows.Single().Name = "New community name";
            db.ParticipantRows.Single(row => row.Id == candidate.AuthorParticipantId).DisplayName = "New author name";
            db.NodeTypeRows.Single().Name = "New type";
            await db.SaveChangesAsync();
        }
        var changed = query.DiscoverPage(new(CommunityId: community, ReactionDefinitionIds: [reaction])).Items.Single().Card!;
        Assert.AreEqual("New type", changed.TypeName);
        Assert.AreEqual("New author name", changed.AuthorName);
        Assert.AreEqual("New community name", changed.Communities);
        using (var db = fixture.Open())
        {
            db.NodeReactionRows.Single().LifecycleState = "Removed";
            db.CommunityNodeRows.RemoveRange(db.CommunityNodeRows);
            db.NodeRows.Single().Title = "New title";
            await db.SaveChangesAsync();
        }
        Assert.IsEmpty(query.DiscoverPage(new(CommunityId: community)).Items);
        Assert.IsEmpty(query.DiscoverPage(new(ReactionDefinitionIds: [reaction])).Items);
        Assert.AreEqual("New title", query.DiscoverPage(new()).Items.Single().Card!.Title);
        var misses = cache.DetailMisses;
        using (var db = fixture.Open())
        { db.NodeRows.Single().Status = "Archived"; await db.SaveChangesAsync(); }
        Assert.IsEmpty(query.DiscoverPage(new()).Items);
        query.DiscoverPage(new(IncludeArchived: true));
        Assert.IsGreaterThan(misses, cache.DetailMisses);
    }

    [TestMethod]
    public async Task Expiration_reloads_both_layers_without_changing_results()
    {
        using var fixture = SqlTestDatabase.Create(useCurrentModel: true);
        var candidate = Candidate(Guid.NewGuid(), 8);
        DiscoverySqlFixture.Seed(fixture, candidate);
        using var cache = new DiscoveryCache(expiration: TimeSpan.FromMilliseconds(10));
        var query = new SqlDiscoveryReadQuery(fixture.Open, cache);
        var first = query.DiscoverPage(new());
        await Task.Delay(100);
        var second = query.DiscoverPage(new());
        Assert.AreEqual(first.Items.Single(), second.Items.Single());
        Assert.AreEqual(2L, cache.PageMisses);
        Assert.AreEqual(2L, cache.DetailMisses);
    }

}

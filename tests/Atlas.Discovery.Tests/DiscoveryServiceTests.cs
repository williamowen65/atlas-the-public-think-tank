using Atlas.Discovery;

namespace Atlas.Discovery.Tests;

[TestClass]
public sealed class DiscoveryServiceTests
{
    private static readonly Guid CommunityA = Guid.NewGuid();
    private static readonly Guid ReactionA = Guid.NewGuid();
    private static readonly Guid ReactionB = Guid.NewGuid();

    [TestMethod]
    public void Discover_returns_active_candidates_in_ranked_order()
    {
        var low = Candidate("Low", average: 4, votes: 20);
        var high = Candidate("High", average: 9, votes: 2);
        var archived = Candidate("Archived", average: 10, votes: 100, archived: true);

        var results = Service(low, archived, high).Discover(new DiscoveryQuery());

        CollectionAssert.AreEqual(
            new[] { high.NodeId, low.NodeId },
            results.Select(result => result.NodeId).ToArray());
        CollectionAssert.AreEqual(new[] { 1, 2 }, results.Select(result => result.Rank).ToArray());
    }

    [TestMethod]
    public void Discover_filters_to_one_community_before_ranking()
    {
        var included = Candidate("Included", average: 5, communities: [CommunityA]);
        var excluded = Candidate("Excluded", average: 10);

        var results = Service(excluded, included).Discover(new DiscoveryQuery(CommunityId: CommunityA));

        Assert.AreEqual(included.NodeId, results.Single().NodeId);
    }

    [TestMethod]
    public void Discover_searches_title_and_content_case_insensitively()
    {
        var titleMatch = Candidate("Climate resilience", average: 5);
        var contentMatch = Candidate("Coastal planning", average: 5, content: "CLIMATE adaptation evidence");
        var unrelated = Candidate("Transit", average: 10);

        var results = Service(contentMatch, unrelated, titleMatch)
            .Discover(new DiscoveryQuery(SearchText: "climate"));

        CollectionAssert.AreEqual(
            new[] { titleMatch.NodeId, contentMatch.NodeId },
            results.Select(result => result.NodeId).ToArray());
    }

    [TestMethod]
    public void Discover_uses_vote_count_then_updated_time_as_deterministic_ties()
    {
        var now = DateTimeOffset.UtcNow;
        var fewerVotes = Candidate("A", average: 7, votes: 1, updatedAt: now);
        var older = Candidate("B", average: 7, votes: 5, updatedAt: now.AddDays(-1));
        var newer = Candidate("C", average: 7, votes: 5, updatedAt: now);

        var results = Service(fewerVotes, older, newer).Discover(new DiscoveryQuery());

        CollectionAssert.AreEqual(
            new[] { newer.NodeId, older.NodeId, fewerVotes.NodeId },
            results.Select(result => result.NodeId).ToArray());
    }

    [TestMethod]
    public void Discover_requires_all_selected_reactions()
    {
        var both = Candidate("Both", average: 5, reactions: [ReactionA, ReactionB]);
        var one = Candidate("One", average: 9, reactions: [ReactionA]);

        var results = Service(one, both).Discover(new DiscoveryQuery(
            ReactionDefinitionIds: [ReactionA, ReactionB]));

        Assert.AreEqual(both.NodeId, results.Single().NodeId);
    }

    [TestMethod]
    public void Discover_filters_vote_count_by_inclusive_range()
    {
        var below = Candidate("Below", average: 5, votes: 1);
        var included = Candidate("Included", average: 5, votes: 3);
        var above = Candidate("Above", average: 5, votes: 7);

        var results = Service(below, included, above).Discover(new DiscoveryQuery(
            MinimumVoteCount: 2,
            MaximumVoteCount: 5));

        Assert.AreEqual(included.NodeId, results.Single().NodeId);
    }

    [TestMethod]
    public void Discover_filters_average_vote_by_inclusive_range_and_excludes_unrated_nodes()
    {
        var unrated = Candidate("Unrated", average: null);
        var included = Candidate("Included", average: 7.5, votes: 2);
        var above = Candidate("Above", average: 9, votes: 2);

        var results = Service(unrated, included, above).Discover(new DiscoveryQuery(
            MinimumAverageVote: 6,
            MaximumAverageVote: 8));

        Assert.AreEqual(included.NodeId, results.Single().NodeId);
    }

    [TestMethod]
    public void Discover_composes_text_community_reaction_and_vote_filters()
    {
        var match = Candidate("Climate plan", average: 8, votes: 4,
            communities: [CommunityA], reactions: [ReactionA]);
        var wrongReaction = Candidate("Climate evidence", average: 8, votes: 4,
            communities: [CommunityA]);

        var results = Service(match, wrongReaction).Discover(new DiscoveryQuery(
            SearchText: "climate",
            CommunityId: CommunityA,
            ReactionDefinitionIds: [ReactionA],
            MinimumVoteCount: 3,
            MaximumVoteCount: 5,
            MinimumAverageVote: 7,
            MaximumAverageVote: 9));

        Assert.AreEqual(match.NodeId, results.Single().NodeId);
    }

    [TestMethod]
    public void Discover_filters_created_date_by_inclusive_range()
    {
        var before = Candidate("Before", average: 5, createdAt: new DateTimeOffset(2026, 8, 31, 23, 59, 0, TimeSpan.Zero));
        var firstDay = Candidate("First day", average: 5, createdAt: new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero));
        var lastDay = Candidate("Last day", average: 5, createdAt: new DateTimeOffset(2026, 9, 30, 23, 59, 0, TimeSpan.Zero));
        var after = Candidate("After", average: 5, createdAt: new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero));

        var results = Service(before, firstDay, lastDay, after).Discover(new DiscoveryQuery(
            CreatedFrom: new DateOnly(2026, 9, 1),
            CreatedThrough: new DateOnly(2026, 9, 30)));

        CollectionAssert.AreEquivalent(
            new[] { firstDay.NodeId, lastDay.NodeId },
            results.Select(result => result.NodeId).ToArray());
    }

    private static DiscoveryService Service(params DiscoveryCandidate[] candidates) =>
        new(new MemorySource(candidates));

    private static DiscoveryCandidate Candidate(
        string title,
        double? average,
        int votes = 0,
        string content = "",
        bool archived = false,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? updatedAt = null,
        IReadOnlyCollection<Guid>? communities = null,
        IReadOnlyCollection<Guid>? reactions = null) =>
        new(Guid.NewGuid(), title, content, archived,
            createdAt ?? updatedAt ?? DateTimeOffset.UtcNow,
            updatedAt ?? createdAt ?? DateTimeOffset.UtcNow,
            votes, average, communities ?? Array.Empty<Guid>(), reactions ?? Array.Empty<Guid>());

    private sealed class MemorySource(IReadOnlyCollection<DiscoveryCandidate> candidates)
        : IDiscoveryCandidateSource
    {
        public IReadOnlyCollection<DiscoveryCandidate> GetCandidates() => candidates;
    }
}

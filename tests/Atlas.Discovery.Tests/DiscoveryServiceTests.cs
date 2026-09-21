using Atlas.Discovery;

namespace Atlas.Discovery.Tests;

[TestClass]
public sealed class DiscoveryServiceTests
{
    private static readonly Guid CommunityA = Guid.NewGuid();

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

    private static DiscoveryService Service(params DiscoveryCandidate[] candidates) =>
        new(new MemorySource(candidates));

    private static DiscoveryCandidate Candidate(
        string title,
        double? average,
        int votes = 0,
        string content = "",
        bool archived = false,
        DateTimeOffset? updatedAt = null,
        IReadOnlyCollection<Guid>? communities = null) =>
        new(Guid.NewGuid(), title, content, archived, updatedAt ?? DateTimeOffset.UtcNow,
            votes, average, communities ?? Array.Empty<Guid>());

    private sealed class MemorySource(IReadOnlyCollection<DiscoveryCandidate> candidates)
        : IDiscoveryCandidateSource
    {
        public IReadOnlyCollection<DiscoveryCandidate> GetCandidates() => candidates;
    }
}

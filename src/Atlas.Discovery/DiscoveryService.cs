namespace Atlas.Discovery;

/// <summary>Filters, searches, and ranks the content exposed to consumers.</summary>
public sealed class DiscoveryService : IDiscoveryService
{
    private readonly IDiscoveryCandidateSource _source;

    public DiscoveryService(IDiscoveryCandidateSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        _source = source;
    }

    public IReadOnlyList<RankedDiscoveryItem> Discover(DiscoveryQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        Validate(query);
        var search = query.SearchText?.Trim();
        var reactions = query.ReactionDefinitionIds ?? Array.Empty<Guid>();

        var ranked = _source.GetCandidates()
            .Where(candidate => query.IncludeArchived || !candidate.IsArchived)
            .Where(candidate => query.CommunityId is null ||
                candidate.CommunityIds.Contains(query.CommunityId.Value))
            .Where(candidate => reactions.All(
                reactionId => candidate.ReactionDefinitionIds.Contains(reactionId)))
            .Where(candidate => query.MinimumVoteCount is null ||
                candidate.VoteCount >= query.MinimumVoteCount.Value)
            .Where(candidate => query.MaximumVoteCount is null ||
                candidate.VoteCount <= query.MaximumVoteCount.Value)
            .Where(candidate => query.MinimumAverageVote is null ||
                candidate.AverageVote >= query.MinimumAverageVote.Value)
            .Where(candidate => query.MaximumAverageVote is null ||
                candidate.AverageVote <= query.MaximumAverageVote.Value)
            .Where(candidate => string.IsNullOrWhiteSpace(search) ||
                candidate.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                candidate.SearchableContent.Contains(search, StringComparison.OrdinalIgnoreCase))
            .Select(candidate => new
            {
                Candidate = candidate,
                Score = Score(candidate, search)
            })
            .OrderByDescending(item => item.Score)
            .ThenByDescending(item => item.Candidate.VoteCount)
            .ThenByDescending(item => item.Candidate.UpdatedAt)
            .ThenBy(item => item.Candidate.Title, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return ranked.Select((item, index) => new RankedDiscoveryItem(
            item.Candidate.NodeId,
            index + 1,
            item.Score,
            item.Candidate.VoteCount,
            item.Candidate.AverageVote)).ToList();
    }

    private static void Validate(DiscoveryQuery query)
    {
        if (query.MinimumVoteCount < 0 || query.MaximumVoteCount < 0)
            throw new ArgumentOutOfRangeException(nameof(query), "Vote-count bounds cannot be negative.");
        if (query.MinimumVoteCount.HasValue && query.MaximumVoteCount.HasValue &&
            query.MinimumVoteCount.Value > query.MaximumVoteCount.Value)
            throw new ArgumentException("Minimum vote count cannot exceed maximum vote count.", nameof(query));
        if (query.MinimumAverageVote is < 0 or > 10 || query.MaximumAverageVote is < 0 or > 10)
            throw new ArgumentOutOfRangeException(nameof(query), "Average-vote bounds must be from 0 through 10.");
        if (query.MinimumAverageVote.HasValue && query.MaximumAverageVote.HasValue &&
            query.MinimumAverageVote.Value > query.MaximumAverageVote.Value)
            throw new ArgumentException("Minimum average vote cannot exceed maximum average vote.", nameof(query));
    }

    private static double Score(DiscoveryCandidate candidate, string? search)
    {
        // Average rating is primary; vote count breaks confidence ties without
        // allowing popularity alone to overpower the 0-10 quality signal.
        var score = candidate.AverageVote ?? 0d;
        score += Math.Log10(candidate.VoteCount + 1) / 100d;

        // A title match is more intentional than a match elsewhere in the
        // searchable projection, so search results receive a small boost.
        if (!string.IsNullOrWhiteSpace(search) &&
            candidate.Title.Contains(search, StringComparison.OrdinalIgnoreCase))
        {
            score += 0.1d;
        }

        return score;
    }
}

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
        var search = query.SearchText?.Trim();

        var ranked = _source.GetCandidates()
            .Where(candidate => query.IncludeArchived || !candidate.IsArchived)
            .Where(candidate => query.CommunityId is null ||
                candidate.CommunityIds.Contains(query.CommunityId.Value))
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

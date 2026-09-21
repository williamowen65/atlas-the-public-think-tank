namespace Atlas.Discovery;

/// <summary>A Node reference returned in ranked Discovery order.</summary>
public sealed record RankedDiscoveryItem(
    Guid NodeId,
    int Rank,
    double RankingScore,
    int VoteCount,
    double? AverageVote);

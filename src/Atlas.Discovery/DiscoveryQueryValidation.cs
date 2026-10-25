namespace Atlas.Discovery;

public static class DiscoveryQueryValidation
{
    public static void Validate(DiscoveryQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.SearchText?.Trim().Length > 4000)
            throw new ArgumentException("Search text cannot exceed 4000 characters.", nameof(query));
        if (query.NodeId == Guid.Empty || query.ParentNodeId == Guid.Empty)
            throw new ArgumentException("A node ID cannot be empty.", nameof(query));
        if (query.VotingWeight is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(query), "Voting weight must be from 0 through 100.");
        if (query.Page < 1)
            throw new ArgumentOutOfRangeException(
                nameof(query),
                "Page must be positive."
            );
        if (query.MinimumVoteCount < 0 || query.MaximumVoteCount < 0)
            throw new ArgumentOutOfRangeException(
                nameof(query),
                "Vote-count bounds cannot be negative."
            );
        if (query.MinimumVoteCount.HasValue && query.MaximumVoteCount.HasValue &&
            query.MinimumVoteCount.Value > query.MaximumVoteCount.Value)
            throw new ArgumentException(
                "Minimum vote count cannot exceed maximum vote count.",
                nameof(query)
            );
        if (query.MinimumAverageVote is { } minimum && !double.IsFinite(minimum) ||
            query.MaximumAverageVote is { } maximum && !double.IsFinite(maximum) ||
            query.MinimumAverageVote is < 0 or > 10 || query.MaximumAverageVote is < 0 or > 10)
            throw new ArgumentOutOfRangeException(
                nameof(query),
                "Average-vote bounds must be from 0 through 10."
            );
        if (query.MinimumAverageVote.HasValue && query.MaximumAverageVote.HasValue &&
            query.MinimumAverageVote.Value > query.MaximumAverageVote.Value)
            throw new ArgumentException(
                "Minimum average vote cannot exceed maximum average vote.",
                nameof(query)
            );
        if (query.CreatedFrom.HasValue && query.CreatedThrough.HasValue &&
            query.CreatedFrom.Value > query.CreatedThrough.Value)
            throw new ArgumentException(
                "Created-from date cannot follow created-through date.",
                nameof(query)
            );
    }

}

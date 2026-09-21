namespace Atlas.Discovery;

/// <summary>
/// Read model supplied to Discovery by upstream boundaries. Discovery does not
/// own or mutate the underlying Node, Content, Voting, or Community data.
/// </summary>
public sealed record DiscoveryCandidate(
    Guid NodeId,
    string Title,
    string SearchableContent,
    bool IsArchived,
    DateTimeOffset UpdatedAt,
    int VoteCount,
    double? AverageVote,
    IReadOnlyCollection<Guid> CommunityIds,
    IReadOnlyCollection<Guid> ReactionDefinitionIds);

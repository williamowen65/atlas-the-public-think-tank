namespace Atlas.Persistence.Tests;

/// <summary>SQL fixture inputs and explicit expected rating/filter data; not a query implementation.</summary>
public sealed record DiscoveryCandidate(
    Guid NodeId,
    string Title,
    string SearchableContent,
    Guid NodeTypeId,
    Guid AuthorParticipantId,
    bool IsArchived,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    int VoteCount,
    double? AverageVote,
    IReadOnlyCollection<Guid> CommunityIds,
    IReadOnlyCollection<Guid> ReactionDefinitionIds,
    bool IsModerationExcluded = false
);

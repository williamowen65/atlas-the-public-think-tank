namespace Atlas.Discovery;

/// <summary>Describes the filters applied to a Discovery request.</summary>
public sealed record DiscoveryQuery(
    string? SearchText = null,
    Guid? CommunityId = null,
    IReadOnlyCollection<Guid>? ReactionDefinitionIds = null,
    int? MinimumVoteCount = null,
    int? MaximumVoteCount = null,
    double? MinimumAverageVote = null,
    double? MaximumAverageVote = null,
    DateOnly? CreatedFrom = null,
    DateOnly? CreatedThrough = null,
    bool IncludeArchived = false);

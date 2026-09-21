namespace Atlas.Discovery;

/// <summary>Describes the filters applied to a Discovery request.</summary>
public sealed record DiscoveryQuery(
    string? SearchText = null,
    Guid? CommunityId = null,
    bool IncludeArchived = false);

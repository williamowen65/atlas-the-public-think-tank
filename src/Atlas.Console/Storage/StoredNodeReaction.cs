namespace Atlas.ConsoleApp.Storage;

/// <summary>Defines the data-only JSON representation of a node-specific tag association.</summary>
public sealed class StoredNodeReaction
{
    public Guid Id { get; set; }
    public Guid NodeId { get; set; }
    public Guid ReactionDefinitionId { get; set; }
    public Guid AppliedByParticipantId { get; set; }
    public required string LifecycleState { get; set; }
    public string Disposition { get; set; } = "Community";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? RemovedAt { get; set; }
    public List<StoredNodeReactionAuditEntry> AuditHistory { get; set; } = [];
}

/// <summary>Defines the data-only JSON representation of a node-reaction audit entry.</summary>
public sealed class StoredNodeReactionAuditEntry
{
    public string Action { get; set; } = string.Empty;
    public Guid? ActorParticipantId { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public string LifecycleState { get; set; } = string.Empty;
    public string Disposition { get; set; } = string.Empty;
    public Guid? RelatedNodeReactionId { get; set; }
}

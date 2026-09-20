namespace Atlas.Graph.Reactions;

/// <summary>Records who changed a node reaction, when it changed, and its resulting state.</summary>
public sealed record NodeReactionAuditEntry
{
    public NodeReactionAuditAction Action { get; }
    public Guid? ActorParticipantId { get; }
    public DateTimeOffset OccurredAt { get; }
    public NodeReactionLifecycleState LifecycleState { get; }
    public NodeReactionDisposition Disposition { get; }
    public NodeReactionId? RelatedNodeReactionId { get; }

    /// <summary>Creates one immutable node-reaction audit record.</summary>
    public NodeReactionAuditEntry(
        NodeReactionAuditAction action,
        Guid? actorParticipantId,
        DateTimeOffset occurredAt,
        NodeReactionLifecycleState lifecycleState,
        NodeReactionDisposition disposition,
        NodeReactionId? relatedNodeReactionId = null)
    {
        if (actorParticipantId == Guid.Empty)
        {
            throw new ArgumentException(
                "An audit actor must be a non-empty participant ID when present.",
                nameof(actorParticipantId));
        }

        if (action != NodeReactionAuditAction.LegacyImported &&
            actorParticipantId is null)
        {
            throw new ArgumentException(
                "A node-reaction change must identify its acting participant.",
                nameof(actorParticipantId));
        }

        if (relatedNodeReactionId?.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "A related node-reaction ID must be non-empty when present.",
                nameof(relatedNodeReactionId));
        }

        if (action == NodeReactionAuditAction.Superseded &&
            relatedNodeReactionId is null)
        {
            throw new ArgumentException(
                "A supersession audit entry must identify its replacement.",
                nameof(relatedNodeReactionId));
        }

        Action = action;
        ActorParticipantId = actorParticipantId;
        OccurredAt = occurredAt;
        LifecycleState = lifecycleState;
        Disposition = disposition;
        RelatedNodeReactionId = relatedNodeReactionId;
    }
}

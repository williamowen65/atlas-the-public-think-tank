namespace Atlas.Graph.Tags;

/// <summary>Records who changed a node tag, when it changed, and its resulting state.</summary>
public sealed record NodeTagAuditEntry
{
    public NodeTagAuditAction Action { get; }
    public Guid? ActorParticipantId { get; }
    public DateTimeOffset OccurredAt { get; }
    public NodeTagLifecycleState LifecycleState { get; }
    public NodeTagDisposition Disposition { get; }
    public NodeTagId? RelatedNodeTagId { get; }

    /// <summary>Creates one immutable node-tag audit record.</summary>
    public NodeTagAuditEntry(
        NodeTagAuditAction action,
        Guid? actorParticipantId,
        DateTimeOffset occurredAt,
        NodeTagLifecycleState lifecycleState,
        NodeTagDisposition disposition,
        NodeTagId? relatedNodeTagId = null)
    {
        if (actorParticipantId == Guid.Empty)
        {
            throw new ArgumentException(
                "An audit actor must be a non-empty participant ID when present.",
                nameof(actorParticipantId));
        }

        if (action != NodeTagAuditAction.LegacyImported &&
            actorParticipantId is null)
        {
            throw new ArgumentException(
                "A node-tag change must identify its acting participant.",
                nameof(actorParticipantId));
        }

        if (relatedNodeTagId?.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "A related node-tag ID must be non-empty when present.",
                nameof(relatedNodeTagId));
        }

        if (action == NodeTagAuditAction.Superseded &&
            relatedNodeTagId is null)
        {
            throw new ArgumentException(
                "A supersession audit entry must identify its replacement.",
                nameof(relatedNodeTagId));
        }

        Action = action;
        ActorParticipantId = actorParticipantId;
        OccurredAt = occurredAt;
        LifecycleState = lifecycleState;
        Disposition = disposition;
        RelatedNodeTagId = relatedNodeTagId;
    }
}

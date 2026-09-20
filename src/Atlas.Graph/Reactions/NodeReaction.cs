using Atlas.Graph.Nodes;

namespace Atlas.Graph.Reactions;

/// <summary>Represents one reusable reaction definition applied to one node.</summary>
public sealed class NodeReaction
{
    private readonly List<NodeReactionAuditEntry> _auditHistory;

    public NodeReactionId Id { get; }
    public NodeId NodeId { get; }
    public ReactionDefinitionId ReactionDefinitionId { get; }
    public Guid AppliedByParticipantId { get; }
    public NodeReactionLifecycleState LifecycleState { get; private set; }
    public NodeReactionDisposition Disposition { get; private set; }
    public bool IsRemoved => LifecycleState != NodeReactionLifecycleState.Active;
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? RemovedAt { get; private set; }
    public IReadOnlyCollection<NodeReactionAuditEntry> AuditHistory =>
        _auditHistory.AsReadOnly();

    private NodeReaction(
        NodeReactionId id,
        NodeId nodeId,
        ReactionDefinitionId tagDefinitionId,
        Guid appliedByParticipantId,
        NodeReactionLifecycleState lifecycleState,
        NodeReactionDisposition disposition,
        DateTimeOffset createdAt,
        DateTimeOffset? removedAt,
        IEnumerable<NodeReactionAuditEntry> auditHistory)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("A node-reaction ID is required.", nameof(id));
        }

        if (nodeId.Value == Guid.Empty)
        {
            throw new ArgumentException("A node ID is required.", nameof(nodeId));
        }

        if (tagDefinitionId.Value == Guid.Empty)
        {
            throw new ArgumentException("A tag-definition ID is required.", nameof(tagDefinitionId));
        }

        if (appliedByParticipantId == Guid.Empty)
        {
            throw new ArgumentException("An applying participant is required.", nameof(appliedByParticipantId));
        }

        if ((lifecycleState != NodeReactionLifecycleState.Active) != removedAt.HasValue)
        {
            throw new ArgumentException("Removed state and removal time must agree.");
        }

        Id = id;
        NodeId = nodeId;
        ReactionDefinitionId = tagDefinitionId;
        AppliedByParticipantId = appliedByParticipantId;
        LifecycleState = lifecycleState;
        Disposition = disposition;
        CreatedAt = createdAt;
        RemovedAt = removedAt;
        _auditHistory = auditHistory.ToList();

        if (_auditHistory.Any(entry => entry.OccurredAt < CreatedAt))
        {
            throw new ArgumentException(
                "Audit entries cannot precede node-reaction creation.",
                nameof(auditHistory));
        }

        for (var index = 1; index < _auditHistory.Count; index++)
        {
            if (_auditHistory[index].OccurredAt <
                _auditHistory[index - 1].OccurredAt)
            {
                throw new ArgumentException(
                    "Audit entries must be in chronological order.",
                nameof(auditHistory));
            }
        }

        if (_auditHistory.Count > 0)
        {
            var latestAudit = _auditHistory[^1];

            if (latestAudit.LifecycleState != LifecycleState ||
                latestAudit.Disposition != Disposition)
            {
                throw new ArgumentException(
                    "The latest audit entry must match the node reaction's current state.",
                    nameof(auditHistory));
            }
        }
    }

    /// <summary>Creates an active node-specific tag association.</summary>
    public static NodeReaction Create(
        NodeId nodeId,
        ReactionDefinitionId tagDefinitionId,
        Guid appliedByParticipantId,
        Guid nodeAuthorParticipantId,
        DateTimeOffset createdAt)
    {
        var disposition = appliedByParticipantId == nodeAuthorParticipantId
            ? NodeReactionDisposition.Endorsed
            : NodeReactionDisposition.Community;

        return new NodeReaction(
            NodeReactionId.New(),
            nodeId,
            tagDefinitionId,
            appliedByParticipantId,
            NodeReactionLifecycleState.Active,
            disposition,
            createdAt,
            removedAt: null,
            [new NodeReactionAuditEntry(
                NodeReactionAuditAction.Applied,
                appliedByParticipantId,
                createdAt,
                NodeReactionLifecycleState.Active,
                disposition)]);
    }

    /// <summary>Rebuilds a persisted association without replaying creation behavior.</summary>
    public static NodeReaction Reconstitute(
        NodeReactionId id,
        NodeId nodeId,
        ReactionDefinitionId tagDefinitionId,
        Guid appliedByParticipantId,
        NodeReactionLifecycleState lifecycleState,
        NodeReactionDisposition disposition,
        DateTimeOffset createdAt,
        DateTimeOffset? removedAt,
        IEnumerable<NodeReactionAuditEntry>? auditHistory = null)
    {
        var restoredAuditHistory = auditHistory?.ToList();

        if (restoredAuditHistory is null || restoredAuditHistory.Count == 0)
        {
            restoredAuditHistory =
            [
                new NodeReactionAuditEntry(
                    NodeReactionAuditAction.LegacyImported,
                    actorParticipantId: null,
                    removedAt ?? createdAt,
                    lifecycleState,
                    disposition)
            ];
        }

        return new NodeReaction(
            id,
            nodeId,
            tagDefinitionId,
            appliedByParticipantId,
            lifecycleState,
            disposition,
            createdAt,
            removedAt,
            restoredAuditHistory);
    }

    /// <summary>Withdraws the association for its proposer or records administrative removal.</summary>
    public void Remove(
        Guid actorParticipantId,
        Guid nodeAuthorParticipantId,
        bool actorIsActive,
        bool actorIsModerator,
        DateTimeOffset removedAt)
    {
        EnsureCanRemove(
            actorParticipantId,
            nodeAuthorParticipantId,
            actorIsActive,
            actorIsModerator);

        if (IsRemoved)
        {
            return;
        }

        EnsureAuditTime(removedAt, nameof(removedAt));

        LifecycleState = actorIsModerator
            ? NodeReactionLifecycleState.AdministrativelyRemoved
            : NodeReactionLifecycleState.Withdrawn;
        RemovedAt = removedAt;

        _auditHistory.Add(
            new NodeReactionAuditEntry(
                actorIsModerator
                    ? NodeReactionAuditAction.AdministrativelyRemoved
                    : NodeReactionAuditAction.Withdrawn,
                actorParticipantId,
                removedAt,
                LifecycleState,
                Disposition));
    }

    /// <summary>Records the node author's presentation decision without erasing the association.</summary>
    public void SetDisposition(
        NodeReactionDisposition disposition,
        Guid actorParticipantId,
        Guid nodeAuthorParticipantId,
        DateTimeOffset changedAt)
    {
        if (actorParticipantId != nodeAuthorParticipantId)
        {
            throw new UnauthorizedAccessException("Only the node author may change tag disposition.");
        }

        if (IsRemoved)
        {
            throw new InvalidOperationException("An inactive node reaction cannot change disposition.");
        }

        if (Disposition == disposition)
        {
            return;
        }

        EnsureAuditTime(changedAt, nameof(changedAt));
        Disposition = disposition;
        _auditHistory.Add(
            new NodeReactionAuditEntry(
                NodeReactionAuditAction.DispositionChanged,
                actorParticipantId,
                changedAt,
                LifecycleState,
                Disposition));
    }

    /// <summary>Marks this association as replaced while preserving its audit record.</summary>
    public void Supersede(
        Guid actorParticipantId,
        NodeReactionId replacementNodeReactionId,
        DateTimeOffset supersededAt)
    {
        EnsureAuditTime(supersededAt, nameof(supersededAt));

        LifecycleState = NodeReactionLifecycleState.Superseded;
        RemovedAt = supersededAt;
        _auditHistory.Add(
            new NodeReactionAuditEntry(
                NodeReactionAuditAction.Superseded,
                actorParticipantId,
                supersededAt,
                LifecycleState,
                Disposition,
                replacementNodeReactionId));
    }

    /// <summary>Checks removal authority without changing the association.</summary>
    public void EnsureCanRemove(
        Guid actorParticipantId,
        Guid nodeAuthorParticipantId,
        bool actorIsActive,
        bool actorIsModerator)
    {
        if (!actorIsActive)
        {
            throw new InvalidOperationException("An inactive participant cannot change node reactions.");
        }

        if (actorIsModerator)
        {
            return;
        }

        if (Disposition == NodeReactionDisposition.Endorsed &&
            actorParticipantId != nodeAuthorParticipantId)
        {
            throw new UnauthorizedAccessException(
                "Only the node author or a moderator may remove an endorsed tag.");
        }

        if (Disposition != NodeReactionDisposition.Endorsed &&
            actorParticipantId != AppliedByParticipantId)
        {
            throw new UnauthorizedAccessException(
                "Only the applying participant or a moderator may remove this tag. The node author may manage its presentation.");
        }
    }

    private void EnsureAuditTime(
        DateTimeOffset occurredAt,
        string parameterName)
    {
        var latestAuditTime = _auditHistory.Count == 0
            ? CreatedAt
            : _auditHistory[^1].OccurredAt;

        if (occurredAt < latestAuditTime)
        {
            throw new ArgumentException(
                "An audit time cannot precede the association's recorded history.",
                parameterName);
        }
    }
}

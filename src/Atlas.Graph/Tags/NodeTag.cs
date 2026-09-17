using Atlas.Graph.Nodes;

namespace Atlas.Graph.Tags;

/// <summary>Represents one reusable tag definition applied to one node.</summary>
public sealed class NodeTag
{
    public NodeTagId Id { get; }
    public NodeId NodeId { get; }
    public TagDefinitionId TagDefinitionId { get; }
    public Guid AppliedByParticipantId { get; }
    public NodeTagLifecycleState LifecycleState { get; private set; }
    public NodeTagDisposition Disposition { get; private set; }
    public bool IsRemoved => LifecycleState != NodeTagLifecycleState.Active;
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? RemovedAt { get; private set; }

    private NodeTag(
        NodeTagId id,
        NodeId nodeId,
        TagDefinitionId tagDefinitionId,
        Guid appliedByParticipantId,
        NodeTagLifecycleState lifecycleState,
        NodeTagDisposition disposition,
        DateTimeOffset createdAt,
        DateTimeOffset? removedAt)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("A node-tag ID is required.", nameof(id));
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

        if ((lifecycleState != NodeTagLifecycleState.Active) != removedAt.HasValue)
        {
            throw new ArgumentException("Removed state and removal time must agree.");
        }

        Id = id;
        NodeId = nodeId;
        TagDefinitionId = tagDefinitionId;
        AppliedByParticipantId = appliedByParticipantId;
        LifecycleState = lifecycleState;
        Disposition = disposition;
        CreatedAt = createdAt;
        RemovedAt = removedAt;
    }

    /// <summary>Creates an active node-specific tag association.</summary>
    public static NodeTag Create(
        NodeId nodeId,
        TagDefinitionId tagDefinitionId,
        Guid appliedByParticipantId,
        Guid nodeAuthorParticipantId,
        DateTimeOffset createdAt)
    {
        var disposition = appliedByParticipantId == nodeAuthorParticipantId
            ? NodeTagDisposition.Endorsed
            : NodeTagDisposition.Community;

        return new NodeTag(
            NodeTagId.New(),
            nodeId,
            tagDefinitionId,
            appliedByParticipantId,
            NodeTagLifecycleState.Active,
            disposition,
            createdAt,
            removedAt: null);
    }

    /// <summary>Rebuilds a persisted association without replaying creation behavior.</summary>
    public static NodeTag Reconstitute(
        NodeTagId id,
        NodeId nodeId,
        TagDefinitionId tagDefinitionId,
        Guid appliedByParticipantId,
        NodeTagLifecycleState lifecycleState,
        NodeTagDisposition disposition,
        DateTimeOffset createdAt,
        DateTimeOffset? removedAt)
    {
        return new NodeTag(
            id,
            nodeId,
            tagDefinitionId,
            appliedByParticipantId,
            lifecycleState,
            disposition,
            createdAt,
            removedAt);
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

        if (removedAt < CreatedAt)
        {
            throw new ArgumentException("Removal time cannot precede creation time.", nameof(removedAt));
        }

        LifecycleState = actorIsModerator
            ? NodeTagLifecycleState.AdministrativelyRemoved
            : NodeTagLifecycleState.Withdrawn;
        RemovedAt = removedAt;
    }

    /// <summary>Records the node author's presentation decision without erasing the association.</summary>
    public void SetDisposition(
        NodeTagDisposition disposition,
        Guid actorParticipantId,
        Guid nodeAuthorParticipantId)
    {
        if (actorParticipantId != nodeAuthorParticipantId)
        {
            throw new UnauthorizedAccessException("Only the node author may change tag disposition.");
        }

        if (IsRemoved)
        {
            throw new InvalidOperationException("An inactive node tag cannot change disposition.");
        }

        Disposition = disposition;
    }

    /// <summary>Marks this association as replaced while preserving its audit record.</summary>
    public void Supersede(DateTimeOffset supersededAt)
    {
        if (supersededAt < CreatedAt)
        {
            throw new ArgumentException("Supersession time cannot precede creation time.", nameof(supersededAt));
        }

        LifecycleState = NodeTagLifecycleState.Superseded;
        RemovedAt = supersededAt;
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
            throw new InvalidOperationException("An inactive participant cannot change node tags.");
        }

        if (actorIsModerator)
        {
            return;
        }

        if (Disposition == NodeTagDisposition.Endorsed &&
            actorParticipantId != nodeAuthorParticipantId)
        {
            throw new UnauthorizedAccessException(
                "Only the node author or a moderator may remove an endorsed tag.");
        }

        if (Disposition != NodeTagDisposition.Endorsed &&
            actorParticipantId != AppliedByParticipantId)
        {
            throw new UnauthorizedAccessException(
                "Only the applying participant or a moderator may remove this tag. The node author may manage its presentation.");
        }
    }
}

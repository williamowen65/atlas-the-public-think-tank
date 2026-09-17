using Atlas.Graph.Nodes;

namespace Atlas.Graph.Tags;

/// <summary>Represents one reusable tag definition applied to one node.</summary>
public sealed class NodeTag
{
    public NodeTagId Id { get; }
    public NodeId NodeId { get; }
    public TagDefinitionId TagDefinitionId { get; }
    public Guid AppliedByParticipantId { get; }
    public bool IsRemoved { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? RemovedAt { get; private set; }

    private NodeTag(
        NodeTagId id,
        NodeId nodeId,
        TagDefinitionId tagDefinitionId,
        Guid appliedByParticipantId,
        bool isRemoved,
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

        if (isRemoved != removedAt.HasValue)
        {
            throw new ArgumentException("Removed state and removal time must agree.");
        }

        Id = id;
        NodeId = nodeId;
        TagDefinitionId = tagDefinitionId;
        AppliedByParticipantId = appliedByParticipantId;
        IsRemoved = isRemoved;
        CreatedAt = createdAt;
        RemovedAt = removedAt;
    }

    /// <summary>Creates an active node-specific tag association.</summary>
    public static NodeTag Create(
        NodeId nodeId,
        TagDefinitionId tagDefinitionId,
        Guid appliedByParticipantId,
        DateTimeOffset createdAt)
    {
        return new NodeTag(
            NodeTagId.New(),
            nodeId,
            tagDefinitionId,
            appliedByParticipantId,
            isRemoved: false,
            createdAt,
            removedAt: null);
    }

    /// <summary>Rebuilds a persisted association without replaying creation behavior.</summary>
    public static NodeTag Reconstitute(
        NodeTagId id,
        NodeId nodeId,
        TagDefinitionId tagDefinitionId,
        Guid appliedByParticipantId,
        bool isRemoved,
        DateTimeOffset createdAt,
        DateTimeOffset? removedAt)
    {
        return new NodeTag(
            id,
            nodeId,
            tagDefinitionId,
            appliedByParticipantId,
            isRemoved,
            createdAt,
            removedAt);
    }

    /// <summary>Removes the association when the actor owns the application, owns the node, or moderates tags.</summary>
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

        IsRemoved = true;
        RemovedAt = removedAt;
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

        if (!actorIsModerator &&
            actorParticipantId != AppliedByParticipantId &&
            actorParticipantId != nodeAuthorParticipantId)
        {
            throw new UnauthorizedAccessException(
                "Only the applying participant, node author, or a moderator may remove this tag.");
        }
    }
}

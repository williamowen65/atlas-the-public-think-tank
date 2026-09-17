using Atlas.Graph.Nodes;

namespace Atlas.Graph.Tags;

/// <summary>Coordinates reusable definition resolution and authorized node-tag mutations.</summary>
public sealed class NodeTagApplicationService
{
    private readonly ITagDefinitionRepository _definitions;
    private readonly INodeTagRepository _nodeTags;

    public NodeTagApplicationService(
        ITagDefinitionRepository definitions,
        INodeTagRepository nodeTags)
    {
        _definitions = definitions;
        _nodeTags = nodeTags;
    }

    /// <summary>Resolves reusable vocabulary and applies it once to an active node.</summary>
    public NodeTag Apply(
        Node node,
        string tagText,
        Guid actorParticipantId,
        bool actorIsActive,
        DateTimeOffset appliedAt)
    {
        EnsureMutable(node, actorParticipantId, actorIsActive);

        var definition = ResolveDefinition(tagText, actorParticipantId, appliedAt);

        if (definition.IsSuppressed)
        {
            throw new InvalidOperationException("A suppressed tag cannot be applied.");
        }

        var existing = _nodeTags.GetActive(node.Id, definition.Id);

        if (existing is not null)
        {
            return existing;
        }

        var nodeTag = NodeTag.Create(
            node.Id,
            definition.Id,
            actorParticipantId,
            node.AuthorId.Value,
            appliedAt);

        _nodeTags.Save(nodeTag);
        return nodeTag;
    }

    /// <summary>Removes one node-specific association after checking node state and actor authority.</summary>
    public void Remove(
        Node node,
        NodeTagId nodeTagId,
        Guid actorParticipantId,
        bool actorIsActive,
        bool actorIsModerator,
        DateTimeOffset removedAt)
    {
        EnsureMutable(node, actorParticipantId, actorIsActive);

        var nodeTag = _nodeTags.GetById(nodeTagId)
            ?? throw new KeyNotFoundException("The node tag does not exist.");

        if (nodeTag.NodeId != node.Id)
        {
            throw new InvalidOperationException("The node tag belongs to a different node.");
        }

        nodeTag.Remove(
            actorParticipantId,
            node.AuthorId.Value,
            actorIsActive,
            actorIsModerator,
            removedAt);

        _nodeTags.Save(nodeTag);
    }

    /// <summary>Replaces one association without renaming its shared definition.</summary>
    public NodeTag Replace(
        Node node,
        NodeTagId existingNodeTagId,
        string replacementText,
        Guid actorParticipantId,
        bool actorIsActive,
        bool actorIsModerator,
        DateTimeOffset replacedAt)
    {
        EnsureMutable(node, actorParticipantId, actorIsActive);

        var existing = _nodeTags.GetById(existingNodeTagId)
            ?? throw new KeyNotFoundException("The node tag does not exist.");

        if (existing.NodeId != node.Id)
        {
            throw new InvalidOperationException("The node tag belongs to a different node.");
        }

        if (existing.IsRemoved)
        {
            throw new InvalidOperationException("A removed node tag cannot be replaced.");
        }

        existing.EnsureCanRemove(
            actorParticipantId,
            node.AuthorId.Value,
            actorIsActive,
            actorIsModerator);

        var replacementDefinition = ResolveDefinition(
            replacementText,
            actorParticipantId,
            replacedAt);

        if (replacementDefinition.IsSuppressed)
        {
            throw new InvalidOperationException("A suppressed tag cannot be applied.");
        }

        if (!existing.IsRemoved &&
            existing.TagDefinitionId == replacementDefinition.Id)
        {
            return existing;
        }

        var replacement = _nodeTags.GetActive(node.Id, replacementDefinition.Id);

        existing.EnsureCanRemove(actorParticipantId, node.AuthorId.Value, actorIsActive, actorIsModerator);
        existing.Supersede(replacedAt);
        _nodeTags.Save(existing);

        if (replacement is not null)
        {
            return replacement;
        }

        replacement = NodeTag.Create(
            node.Id,
            replacementDefinition.Id,
            actorParticipantId,
            node.AuthorId.Value,
            replacedAt);
        _nodeTags.Save(replacement);
        return replacement;
    }

    /// <summary>Changes how the node author presents one active tag.</summary>
    public void SetDisposition(Node node, NodeTagId nodeTagId, NodeTagDisposition disposition,
        Guid actorParticipantId, bool actorIsActive)
    {
        EnsureMutable(node, actorParticipantId, actorIsActive);
        var nodeTag = _nodeTags.GetById(nodeTagId)
            ?? throw new KeyNotFoundException("The node tag does not exist.");

        if (nodeTag.NodeId != node.Id)
        {
            throw new InvalidOperationException("The node tag belongs to a different node.");
        }

        nodeTag.SetDisposition(disposition, actorParticipantId, node.AuthorId.Value);
        _nodeTags.Save(nodeTag);
    }

    private TagDefinition ResolveDefinition(
        string tagText,
        Guid actorParticipantId,
        DateTimeOffset createdAt)
    {
        var normalizedText = TagDefinition.Normalize(tagText);
        var existing = _definitions.GetByNormalizedText(normalizedText);

        if (existing is not null)
        {
            return existing;
        }

        var definition = TagDefinition.Create(tagText, actorParticipantId, createdAt);
        _definitions.Save(definition);
        return definition;
    }

    private static void EnsureMutable(
        Node node,
        Guid actorParticipantId,
        bool actorIsActive)
    {
        if (actorParticipantId == Guid.Empty)
        {
            throw new ArgumentException("An acting participant is required.", nameof(actorParticipantId));
        }

        if (!actorIsActive)
        {
            throw new InvalidOperationException("An inactive participant cannot change node tags.");
        }

        if (node.Status == NodeStatus.Archived)
        {
            throw new InvalidOperationException("Tags on an archived node cannot be changed.");
        }
    }
}

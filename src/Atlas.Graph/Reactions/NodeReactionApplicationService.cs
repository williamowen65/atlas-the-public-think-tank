using Atlas.Graph.Nodes;

namespace Atlas.Graph.Reactions;

/// <summary>Coordinates reusable definition resolution and authorized node-reaction mutations.</summary>
public sealed class NodeReactionApplicationService
{
    private readonly IReactionDefinitionRepository _definitions;
    private readonly INodeReactionRepository _nodeTags;

    public NodeReactionApplicationService(
        IReactionDefinitionRepository definitions,
        INodeReactionRepository nodeTags)
    {
        _definitions = definitions;
        _nodeTags = nodeTags;
    }

    /// <summary>Applies one curated reaction once to an active node.</summary>
    public NodeReaction Apply(
        Node node,
        string reactionText,
        Guid actorParticipantId,
        bool actorIsActive,
        DateTimeOffset appliedAt)
    {
        EnsureMutable(node, actorParticipantId, actorIsActive);

        var definition = ResolveDefinition(reactionText);

        if (definition.IsSuppressed)
        {
            throw new InvalidOperationException("A suppressed reaction cannot be applied.");
        }

        var existing = _nodeTags.GetActive(node.Id, definition.Id);

        if (existing is not null)
        {
            return existing;
        }

        var nodeReaction = NodeReaction.Create(
            node.Id,
            definition.Id,
            actorParticipantId,
            node.AuthorId.Value,
            appliedAt);

        _nodeTags.Save(nodeReaction);
        return nodeReaction;
    }

    /// <summary>Removes one node-specific association after checking node state and actor authority.</summary>
    public void Remove(
        Node node,
        NodeReactionId nodeTagId,
        Guid actorParticipantId,
        bool actorIsActive,
        bool actorIsModerator,
        DateTimeOffset removedAt)
    {
        EnsureMutable(node, actorParticipantId, actorIsActive);

        var nodeTag = _nodeTags.GetById(nodeTagId)
            ?? throw new KeyNotFoundException("The node reaction does not exist.");

        if (nodeTag.NodeId != node.Id)
        {
            throw new InvalidOperationException("The node reaction belongs to a different node.");
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
    public NodeReaction Replace(
        Node node,
        NodeReactionId existingNodeReactionId,
        string replacementText,
        Guid actorParticipantId,
        bool actorIsActive,
        bool actorIsModerator,
        DateTimeOffset replacedAt)
    {
        EnsureMutable(node, actorParticipantId, actorIsActive);

        var existing = _nodeTags.GetById(existingNodeReactionId)
            ?? throw new KeyNotFoundException("The node reaction does not exist.");

        if (existing.NodeId != node.Id)
        {
            throw new InvalidOperationException("The node reaction belongs to a different node.");
        }

        if (existing.IsRemoved)
        {
            throw new InvalidOperationException("A removed node reaction cannot be replaced.");
        }

        existing.EnsureCanRemove(
            actorParticipantId,
            node.AuthorId.Value,
            actorIsActive,
            actorIsModerator);

        var replacementDefinition = ResolveDefinition(replacementText);

        if (replacementDefinition.IsSuppressed)
        {
            throw new InvalidOperationException("A suppressed reaction cannot be applied.");
        }

        if (!existing.IsRemoved &&
            existing.ReactionDefinitionId == replacementDefinition.Id)
        {
            return existing;
        }

        var replacement = _nodeTags.GetActive(node.Id, replacementDefinition.Id);

        if (replacement is null)
        {
            replacement = NodeReaction.Create(
                node.Id,
                replacementDefinition.Id,
                actorParticipantId,
                node.AuthorId.Value,
                replacedAt);
            _nodeTags.Save(replacement);
        }

        existing.Supersede(
            actorParticipantId,
            replacement.Id,
            replacedAt);
        _nodeTags.Save(existing);
        return replacement;
    }

    /// <summary>Changes how the node author presents one active tag.</summary>
    public void SetDisposition(
        Node node,
        NodeReactionId nodeTagId,
        NodeReactionDisposition disposition,
        Guid actorParticipantId,
        bool actorIsActive,
        DateTimeOffset changedAt)
    {
        EnsureMutable(node, actorParticipantId, actorIsActive);
        var nodeTag = _nodeTags.GetById(nodeTagId)
            ?? throw new KeyNotFoundException("The node reaction does not exist.");

        if (nodeTag.NodeId != node.Id)
        {
            throw new InvalidOperationException("The node reaction belongs to a different node.");
        }

        nodeTag.SetDisposition(
            disposition,
            actorParticipantId,
            node.AuthorId.Value,
            changedAt);
        _nodeTags.Save(nodeTag);
    }

    private ReactionDefinition ResolveDefinition(string reactionText)
    {
        var normalizedText = ReactionDefinition.Normalize(reactionText);
        var existing = _definitions.GetByNormalizedText(normalizedText);

        if (existing is null)
        {
            throw new InvalidOperationException(
                "That reaction is not part of the curated reaction catalog.");
        }

        return existing;
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
            throw new InvalidOperationException("An inactive participant cannot change node reactions.");
        }

        if (node.Status == NodeStatus.Archived)
        {
            throw new InvalidOperationException("Reactions on an archived node cannot be changed.");
        }
    }
}

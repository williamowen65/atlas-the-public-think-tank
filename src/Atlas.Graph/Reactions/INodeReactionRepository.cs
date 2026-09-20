using Atlas.Graph.Nodes;

namespace Atlas.Graph.Reactions;

/// <summary>Defines Graph persistence operations for node-specific tag associations.</summary>
public interface INodeReactionRepository
{
    /// <summary>Loads all node-reaction associations, including removed history.</summary>
    IReadOnlyCollection<NodeReaction> GetAll();

    /// <summary>Loads one association by its Graph-owned identifier.</summary>
    NodeReaction? GetById(NodeReactionId id);

    /// <summary>Loads active associations for one node.</summary>
    IReadOnlyCollection<NodeReaction> GetActiveForNode(NodeId nodeId);

    /// <summary>Loads the active association for one node and definition.</summary>
    NodeReaction? GetActive(NodeId nodeId, ReactionDefinitionId tagDefinitionId);

    /// <summary>Persists the current association state.</summary>
    void Save(NodeReaction nodeTag);
}

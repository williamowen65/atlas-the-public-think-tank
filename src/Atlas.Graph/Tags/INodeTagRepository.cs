using Atlas.Graph.Nodes;

namespace Atlas.Graph.Tags;

/// <summary>Defines Graph persistence operations for node-specific tag associations.</summary>
public interface INodeTagRepository
{
    /// <summary>Loads all node-tag associations, including removed history.</summary>
    IReadOnlyCollection<NodeTag> GetAll();

    /// <summary>Loads one association by its Graph-owned identifier.</summary>
    NodeTag? GetById(NodeTagId id);

    /// <summary>Loads active associations for one node.</summary>
    IReadOnlyCollection<NodeTag> GetActiveForNode(NodeId nodeId);

    /// <summary>Loads the active association for one node and definition.</summary>
    NodeTag? GetActive(NodeId nodeId, TagDefinitionId tagDefinitionId);

    /// <summary>Persists the current association state.</summary>
    void Save(NodeTag nodeTag);
}

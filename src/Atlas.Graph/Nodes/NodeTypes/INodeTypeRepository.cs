namespace Atlas.Graph.Nodes.NodeTypes;

/// <summary>Defines the persistence operations required by the Graph boundary for node types.</summary>
public interface INodeTypeRepository
{
    /// <summary>Loads all persisted domain objects.</summary>
    IReadOnlyCollection<NodeTypeDefinition> GetAll();

    /// <summary>Loads a domain object by its boundary-owned identifier.</summary>
    NodeTypeDefinition? GetById(NodeTypeId id);

    /// <summary>Persists the current domain-object state.</summary>
    void Save(NodeTypeDefinition nodeType);
}
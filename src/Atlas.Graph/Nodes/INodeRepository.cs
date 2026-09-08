namespace Atlas.Graph.Nodes;

/// <summary>Defines the persistence operations required by the Graph boundary for nodes.</summary>
public interface INodeRepository
{
    /// <summary>Loads all persisted domain objects.</summary>
    IReadOnlyCollection<Node> GetAll();

    /// <summary>Loads a domain object by its boundary-owned identifier.</summary>
    Node? GetById(NodeId id);

    /// <summary>Persists the current domain-object state.</summary>
    void Save(Node node);
}

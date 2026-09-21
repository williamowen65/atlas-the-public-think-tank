namespace Atlas.Graph.Nodes;

/// <summary>Defines the persistence operations required by the Graph boundary for nodes.</summary>
public interface INodeRepository
{
    /// <summary>Loads a domain object by its boundary-owned identifier.</summary>
    Node? GetById(NodeId id);

    /// <summary>Loads Nodes directly parented by the selected Node.</summary>
    IReadOnlyCollection<Node> GetChildren(NodeId parentId);

    /// <summary>Loads Nodes attributed to one author.</summary>
    IReadOnlyCollection<Node> GetByAuthor(NodeAuthorId authorId);

    /// <summary>Loads Nodes eligible to be considered as a new parent.</summary>
    IReadOnlyCollection<Node> GetParentCandidates(
        NodeId childId,
        IReadOnlyCollection<NodeId> existingParentIds);

    /// <summary>Persists the current domain-object state.</summary>
    void Save(Node node);
}

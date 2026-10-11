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
        IReadOnlyCollection<NodeId> existingParentIds
    );

    /// <summary>Persists the current domain-object state.</summary>
    void Save(Node node);

    /// <summary>Updates metadata on an existing node without replacing its parent relationships.</summary>
    /// <remarks>Use this for edits such as rename, status, type and requested types. SQL adapters preserve
    /// stored parents and avoid the graph application lock used when creating nodes or replacing/editing
    /// parent relationships. Ordinary transaction and row/range locks still apply. Adapters without
    /// separate relationship storage may use their normal save.</remarks>
    void SaveMetadata(Node node) => Save(node);
}

using Atlas.Persistence;
using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;
using Atlas.ConsoleApp.Discovery;

namespace Atlas.ConsoleApp.Storage;

/// <summary>Maps domain objects directly to EF Core rows in SQL Server.</summary>
public sealed class SqlNodeRepository(Func<AtlasDataContext>? contextFactory = null) : SqlRepository(contextFactory), INodeRepository, IDiscoveryNodeReader, INodeTopologyBoundary
{
    /// <summary>Coordinates creation and parent changes before graph checks and saves.</summary>
    /// <remarks>Metadata edits use the ordinary boundary and never request this named lock.</remarks>
    public T ExecuteTopology<T>(Func<T> operation) => ExecuteWithLock(operation, "AtlasGraphTopology");

    public Node? GetById(NodeId id) => FindRow<NodeRow>(id.Value) is { } row ? ToDomain(row) : null;
    private IReadOnlyCollection<Node> All() => ReadRows<NodeRow>().Select(ToDomain).ToList();
    public IReadOnlyCollection<Node> GetChildren(NodeId parentId) => All().Where(node => node.ParentNodeIds.Contains(parentId)).ToList();
    public IReadOnlyCollection<Node> GetByAuthor(NodeAuthorId authorId) => All().Where(node => node.AuthorId == authorId).ToList();
    public IReadOnlyCollection<Node> GetParentCandidates(NodeId childId, IReadOnlyCollection<NodeId> existingParentIds) =>
        All().Where(node => node.Id != childId && !existingParentIds.Contains(node.Id)).ToList();
    IReadOnlyCollection<Node> IDiscoveryNodeReader.ReadNodesForDiscovery() => All();
    /// <summary>Persists a complete node, including its parents, under graph coordination.</summary>
    public void Save(Node node) => ExecuteTopology(() => { SaveCore(node, false); return true; });

    /// <summary>Updates an existing node's metadata without acquiring the graph lock or rewriting parents.</summary>
    public void SaveMetadata(Node node) => Execute(() => { SaveCore(node, true); return true; });

    /// <summary>Validates references and persists the selected metadata or complete-node operation.</summary>
    private void SaveCore(Node node, bool metadataOnly)
    {
        ArgumentNullException.ThrowIfNull(node);
        var previous = FindRow<NodeRow>(node.Id.Value);
        if (metadataOnly && previous is null) throw new InvalidOperationException("Metadata edits require an existing node.");
        ReferenceValidation.Require(this, "Participant", node.AuthorId.Value, previous is null);
        ReferenceValidation.Require(this, "Document", node.DescriptionId.Value, false);
        ReferenceValidation.Require(this, "NodeType", node.TypeId.Value, previous is null || previous.TypeId != node.TypeId.Value);
        foreach (var request in node.RequestedSubNodeTypes)
            ReferenceValidation.Require(this, "NodeType", request.TypeId.Value,
                previous is null || !(previous.RequestedSubNodeTypeIds ?? []).Contains(request.TypeId.Value));
        if (!metadataOnly)
        {
            GraphRelationshipValidation.EnsureAcyclic(node.Id, node.ParentNodeIds, parentId =>
                FindRow<NodeRow>(parentId.Value) is { } parent ? (parent.ParentNodeIds ?? []).Select(id => new NodeId(id)).ToList() : null);
            foreach (var parent in node.ParentNodeIds)
                ReferenceValidation.Require(this, "Node", parent.Value, previous is null || !(previous.ParentNodeIds ?? []).Contains(parent.Value));
        }
        SaveRow(ToStorage(node), metadataOnly ? [nameof(NodeRow.Parents)] : []);
    }

    private static NodeRow ToStorage(Node node)
    {
        return new NodeRow
        {
            Id = node.Id.Value,
            Title = node.Title.Value,
            DescriptionId = node.DescriptionId.Value,
            TypeId = node.TypeId.Value,
            AuthorId = node.AuthorId.Value,
            RequestedSubNodeTypeIds = node.RequestedSubNodeTypes
                .Select(request => request.TypeId.Value)
                .ToList(),
            ParentNodeIds = node.ParentNodeIds
                .Select(parentId => parentId.Value)
                .ToList(),
            Status = node.Status.ToString(),
            CreatedAt = node.CreatedAt,
            UpdatedAt = node.UpdatedAt
        };
    }

    /// <summary>Reconstitutes a domain object from its data-only persistence representation.</summary>
    private Node ToDomain(NodeRow storedNode)
    {
        var status = Enum.Parse<NodeStatus>(
            storedNode.Status,
            ignoreCase: true);

        var descriptionId = storedNode.DescriptionId
            ?? throw new InvalidDataException(
                $"Node {storedNode.Id} has no description ID.");

        var authorId = storedNode.AuthorId
            ?? throw new InvalidDataException(
                $"Node {storedNode.Id} has no author ID.");

        return Node.Reconstitute(
            new NodeId(storedNode.Id),
            new NodeTitle(storedNode.Title),
            new NodeDescriptionId(descriptionId),
            new NodeTypeId(storedNode.TypeId ?? throw new InvalidDataException("Node has no type ID.")),
            new NodeAuthorId(authorId),
            status,
            (storedNode.RequestedSubNodeTypeIds ?? [])
                .Select(id => new NodeTypeId(id)),
            (storedNode.ParentNodeIds ?? [])
                .Select(id => new NodeId(id)),
            storedNode.CreatedAt,
            storedNode.UpdatedAt);
    }

}

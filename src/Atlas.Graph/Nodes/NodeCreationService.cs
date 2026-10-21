using Atlas.Graph.Nodes.NodeTypes;

namespace Atlas.Graph.Nodes;

/// <summary>Graph-owned node construction and checks; knows description IDs, never Content objects.</summary>
public sealed class NodeCreationService(INodeRepository nodes)
{
    public Node Prepare(string title, NodeDescriptionId descriptionId, NodeTypeId typeId,
        Guid authorId, IEnumerable<NodeTypeId> requestedTypes, DateTimeOffset at, NodeId? parentId = null, string shortDescription = "")
    {
        ArgumentNullException.ThrowIfNull(requestedTypes);
        var requests = requestedTypes.ToList();
        ReferenceValidation.Require(nodes, "Participant", authorId);
        ReferenceValidation.Require(nodes, "NodeType", typeId.Value);
        foreach (var request in requests) ReferenceValidation.Require(nodes, "NodeType", request.Value);
        if (parentId is { } parent) ReferenceValidation.Require(nodes, "Node", parent.Value);
        if (nodes is INodeCreationQuota quota) quota.EnsureCreationAllowed(authorId);
        var node = new Node(new NodeTitle(title), descriptionId, typeId, new NodeAuthorId(authorId), requests, at, shortDescription);
        if (parentId is { } parentToAttach)
        {
            GraphRelationshipValidation.EnsureAcyclic(node.Id, [parentToAttach], id => nodes.GetById(id)?.ParentNodeIds);
            node.AttachToParent(parentToAttach, authorId, at);
        }
        return node;
    }

    /// <summary>Persist after the coordinator has stored the description; repository checks still apply.</summary>
    public void Save(Node node)
    {
        ArgumentNullException.ThrowIfNull(node);
        ReferenceValidation.Require(nodes, "Document", node.DescriptionId.Value, false);
        nodes.Save(node);
    }
}

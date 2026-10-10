using Atlas.Graph.Nodes.NodeTypes;

namespace Atlas.Graph.Nodes;

/// <summary>Checks repository-wide ancestry and availability before mutating a node.</summary>
public sealed class NodeRelationshipService(INodeRepository nodes)
{
    public void ChangeType(Node node, NodeTypeId typeId, Guid actorId, DateTimeOffset at) =>
        OperationBoundary.Execute(nodes, () =>
        {
            EnsureCurrent(node, actorId);
            ReferenceValidation.Require(nodes, "NodeType", typeId.Value);
            node.ChangeType(typeId, actorId, at);
            nodes.Save(node);
        });

    public void SetRequestedTypes(Node node, IEnumerable<NodeTypeId> typeIds, Guid actorId, DateTimeOffset at) =>
        OperationBoundary.Execute(nodes, () =>
        {
            ArgumentNullException.ThrowIfNull(typeIds);
            var selected = typeIds.Distinct().ToList();
            EnsureCurrent(node, actorId);
            foreach (var id in selected) ReferenceValidation.Require(nodes, "NodeType", id.Value);
            // Validate the complete proposal on a copy before applying collection changes.
            var proposal = Node.Reconstitute(node.Id, node.Title, node.DescriptionId, node.TypeId, node.AuthorId,
                node.Status, node.RequestedSubNodeTypes.Select(request => request.TypeId), node.ParentNodeIds, node.CreatedAt, node.UpdatedAt);
            Apply(proposal);
            Apply(node);
            nodes.Save(node);
            void Apply(Node value)
            {
                foreach (var old in value.RequestedSubNodeTypes.ToList())
                    if (!selected.Contains(old.TypeId)) value.StopRequestingSubNodeType(old.TypeId, actorId, at);
                foreach (var id in selected) value.RequestSubNodeType(id, actorId, at);
            }
        });

    private void EnsureCurrent(Node node, Guid actorId)
    {
        ArgumentNullException.ThrowIfNull(node);
        ReferenceValidation.Require(nodes, "Participant", actorId);
        ReferenceValidation.Require(nodes, "Node", node.Id.Value, false);
        var current = nodes.GetById(node.Id) ?? throw new InvalidOperationException("Node not found.");
        if (current.UpdatedAt != node.UpdatedAt || current.TypeId != node.TypeId ||
            !current.RequestedSubNodeTypes.SequenceEqual(node.RequestedSubNodeTypes) || !current.ParentNodeIds.SequenceEqual(node.ParentNodeIds))
            throw new InvalidOperationException("The node changed. Reload before changing relationships.");
    }

    public void Attach(Node node, NodeId parentId, Guid actorId, DateTimeOffset at) =>
        OperationBoundary.Execute(nodes, () =>
        {
            ArgumentNullException.ThrowIfNull(node);
            ReferenceValidation.Require(nodes, "Participant", actorId);
            ReferenceValidation.Require(nodes, "Node", node.Id.Value);
            ReferenceValidation.Require(nodes, "Node", parentId.Value);
            var current = nodes.GetById(node.Id) ?? throw new InvalidOperationException("Node not found.");
            if (current.UpdatedAt != node.UpdatedAt || !current.ParentNodeIds.SequenceEqual(node.ParentNodeIds))
                throw new InvalidOperationException("The node changed. Reload before adding a parent.");
            GraphRelationshipValidation.EnsureAcyclic(node.Id, node.ParentNodeIds.Append(parentId),
                id => nodes.GetById(id)?.ParentNodeIds);
            node.AttachToParent(parentId, actorId, at);
            nodes.Save(node);
        });
}

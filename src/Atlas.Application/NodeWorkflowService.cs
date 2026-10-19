using Atlas.Content.Documents;
using Atlas.Contracts.Events;
using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;

namespace Atlas.Application;

/// <summary>Reusable node use cases and after-commit event dispatch. Hosts only supply input.</summary>
public sealed class NodeWorkflowService(INodeRepository nodes, IEventPublisher events)
{
    public Node Create(IDocumentRepository documents, string title, string markdown, NodeTypeId typeId,
        Guid actorId, IEnumerable<NodeTypeId> requested, NodeId? parentId = null)
    {
        var node = new NodeCreationService(nodes, documents).Create(title, markdown, typeId, actorId, requested, parentId);
        Dispatch(node);
        return node;
    }
    public void Rename(Node node, Guid actorId, string title) => Change(node, actorId, () => node.Rename(new NodeTitle(title), actorId, AtlasTime.UtcNow));
    public void Archive(Node node, Guid actorId) => Change(node, actorId, () => node.Archive(actorId, AtlasTime.UtcNow));
    public void Restore(Node node, Guid actorId) => Change(node, actorId, () => node.Restore(actorId, AtlasTime.UtcNow));
    public void Attach(Node node, NodeId parentId, Guid actorId)
    {
        EnsureActor(node, actorId);
        new NodeRelationshipService(nodes).Attach(node, parentId, actorId, AtlasTime.UtcNow);
        Dispatch(node);
    }
    public void Detach(Node node, NodeId parentId, Guid actorId)
    {
        NodeTopologyBoundary.Execute(nodes, () =>
        {
            EnsureActor(node, actorId);
            node.DetachFromParent(parentId, actorId, AtlasTime.UtcNow);
            nodes.Save(node);
        });
        Dispatch(node);
    }
    /// <summary>Change type after checking the catalog reference and current authorship.</summary>
    public void ChangeType(Node node, NodeTypeId typeId, Guid actorId) => Change(node, actorId, () =>
    {
        ReferenceValidation.Require(nodes, "NodeType", typeId.Value);
        node.ChangeType(typeId, actorId, AtlasTime.UtcNow);
    });
    /// <summary>Add or withdraw a requested child type without accepting an actor from transport input.</summary>
    public void RequestType(Node node, NodeTypeId typeId, Guid actorId, bool requested) => Change(node, actorId, () =>
    {
        ReferenceValidation.Require(nodes, "NodeType", typeId.Value);
        if (requested) node.RequestSubNodeType(typeId, actorId, AtlasTime.UtcNow);
        else node.StopRequestingSubNodeType(typeId, actorId, AtlasTime.UtcNow);
    });
    private void Change(Node node, Guid actorId, Action change)
    {
        OperationBoundary.Execute(nodes, () =>
        {
            EnsureActor(node, actorId);
            change();
            nodes.SaveMetadata(node);
        });
        Dispatch(node);
    }
    private void EnsureActor(Node node, Guid actorId)
    {
        ReferenceValidation.Require(nodes, "Participant", actorId);
        var current = nodes.GetById(node.Id) ?? throw new InvalidOperationException("Node not found.");
        current.EnsureAuthoredBy(actorId);
    }
    private void Dispatch(Node node)
    {
        // Boundary returned successfully before dispatch. Subscriber failure does not undo
        // the commit; clear only after all handlers return. This is not a durable event bus.
        foreach (var message in node.DomainEvents) events.Publish(message);
        node.ClearDomainEvents();
    }
}

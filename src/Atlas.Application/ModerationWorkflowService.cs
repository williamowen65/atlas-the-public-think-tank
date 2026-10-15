using Atlas.Content.Documents;
using Atlas.Contracts.Events;
using Atlas.Contracts.Notifications.V1;
using Atlas.Graph.Nodes;
using Atlas.Moderation;

namespace Atlas.Application;

/// <summary>Cross-domain moderation coordination and notification recipients, independent of menus.</summary>
public sealed class ModerationWorkflowService(INodeRepository nodes, IDocumentRepository documents, ModerationService moderation, IEventPublisher events)
{
    public void RequestReview(Node node, Guid actorId)
    {
        var document = documents.GetById(new DocumentId(node.DescriptionId.Value));
        var latest = document is null || document.UpdatedAt < node.UpdatedAt ? node.UpdatedAt : document.UpdatedAt;
        if (document is not null)
            foreach (var block in documents.GetBlocks(document)) if (block.UpdatedAt > latest) latest = block.UpdatedAt;
        moderation.RequestNodeReview(node.Id.Value, actorId, node.AuthorId.Value, latest, AtlasTime.UtcNow);
    }
    public void Restore(Guid nodeId, Guid actorId, string rationale)
    {
        var at = AtlasTime.UtcNow;
        moderation.RestoreNode(actorId, nodeId, rationale, at);
        var node = nodes.GetById(new NodeId(nodeId));
        if (node is not null) Notify(Guid.NewGuid(), node.AuthorId.Value, actorId, "NodeRestored", nodeId, at);
    }
    public IReadOnlyList<ModerationCase> Decide(Guid nodeId, Guid actorId, ModerationDecision decision, string rationale, PublicModerationReason reason)
    {
        var at = AtlasTime.UtcNow;
        var decided = moderation.DecideNode(actorId, nodeId, decision, rationale, at, reason);
        foreach (var report in decided) Notify(report.Id, report.ReporterId, actorId, "ReportDecided", nodeId, at);
        var node = nodes.GetById(new NodeId(nodeId));
        if (node is not null && decision == ModerationDecision.HideNode) Notify(Guid.NewGuid(), node.AuthorId.Value, actorId, "NodeHidden", nodeId, at);
        return decided;
    }
    private void Notify(Guid eventId, Guid recipient, Guid actor, string kind, Guid nodeId, DateTimeOffset at) =>
        events.Publish(new NotificationRequestedV1(eventId, recipient, actor, kind, "Node", nodeId, at));
}

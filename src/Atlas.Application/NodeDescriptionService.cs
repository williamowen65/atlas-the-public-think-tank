using Atlas.Content.Blocks;
using Atlas.Content.Documents;
using Atlas.Contracts.Operations;
using Atlas.Graph.Nodes;

namespace Atlas.Application;

/// <summary>Authorize description mutations through their owning Graph node, then delegate Content work.</summary>
/// <remarks>The host supplies the verified actor ID. Content has no dependency on Graph ownership.</remarks>
public sealed class NodeDescriptionService(INodeRepository nodes, IDocumentRepository documents)
{
    public ContentBlock Add(NodeId nodeId, Guid actorId, BlockInput input, int? index = null) =>
        Execute(nodeId, actorId, (id, at) => new DocumentService(documents).Add(id, input, at, index));
    public ContentBlock Update(NodeId nodeId, Guid actorId, BlockId blockId, BlockInput input) =>
        Execute(nodeId, actorId, (id, at) => new DocumentService(documents).Update(id, blockId, input, at));
    public void Move(NodeId nodeId, Guid actorId, BlockId blockId, int index) =>
        Execute(nodeId, actorId, (id, at) => { new DocumentService(documents).Move(id, blockId, index, at); return true; });
    public void Remove(NodeId nodeId, Guid actorId, BlockId blockId) =>
        Execute(nodeId, actorId, (id, at) => { new DocumentService(documents).Remove(id, blockId, at); return true; });

    private T Execute<T>(NodeId nodeId, Guid actorId, Func<DocumentId, DateTimeOffset, T> change) =>
        OperationBoundary.Execute(nodes, () =>
        {
            ReferenceValidation.Require(nodes, "Participant", actorId);
            var node = nodes.GetById(nodeId) ?? throw new InvalidOperationException("Node not found.");
            node.EnsureAuthoredBy(actorId);
            return change(new DocumentId(node.DescriptionId.Value), AtlasTime.UtcNow);
        });
}

using Atlas.Content.Blocks;
using Atlas.Content.Documents;
using Atlas.Contracts.Operations;
using Atlas.Graph.Nodes;

namespace Atlas.Application;

/// <summary>Authorize description mutations through their owning Graph node, then delegate Content work.</summary>
/// <remarks>The host supplies the verified actor ID. Content has no dependency on Graph ownership.</remarks>
public sealed class NodeDescriptionService(INodeRepository nodes, IDocumentRepository documents)
{
    /// <summary>Add a new block to the node's description after verifying its author.</summary>
    /// <param name="nodeId">The node whose description will change.</param>
    /// <param name="actorId">The participant ID verified by the host.</param>
    /// <param name="input">The typed content used to construct the new block.</param>
    /// <param name="index">The zero-based insertion position; null appends the block.</param>
    /// <returns>The newly created and saved content block.</returns>
    public ContentBlock Add(NodeId nodeId, Guid actorId, BlockInput input, int? index = null) =>
        Execute(nodeId, actorId, (id, at) => new DocumentService(documents).Add(id, input, at, index));
    /// <summary>Update an existing description block while preserving its identity.</summary>
    /// <remarks>Content verifies document membership and that the input matches the block kind.</remarks>
    /// <param name="nodeId">The node whose description owns the block.</param>
    /// <param name="actorId">The participant ID verified by the host.</param>
    /// <param name="blockId">The existing block to update.</param>
    /// <param name="input">The replacement content for that block kind.</param>
    /// <returns>The updated and saved content block.</returns>
    public ContentBlock Update(NodeId nodeId, Guid actorId, BlockId blockId, BlockInput input) =>
        Execute(nodeId, actorId, (id, at) => new DocumentService(documents).Update(id, blockId, input, at));
    /// <summary>Reorder a description block without changing its content.</summary>
    /// <param name="nodeId">The node whose description will change.</param>
    /// <param name="actorId">The participant ID verified by the host.</param>
    /// <param name="blockId">The block already belonging to this description.</param>
    /// <param name="index">The zero-based destination position.</param>
    public void Move(NodeId nodeId, Guid actorId, BlockId blockId, int index) =>
        Execute(nodeId, actorId, (id, at) => { new DocumentService(documents).Move(id, blockId, index, at); return true; });
    /// <summary>Detach a block from the description after verifying the node's author.</summary>
    /// <remarks>The stored block payload remains available; this does not delete it.</remarks>
    /// <param name="nodeId">The node whose description will change.</param>
    /// <param name="actorId">The participant ID verified by the host.</param>
    /// <param name="blockId">The block to remove from the document's composition.</param>
    public void Remove(NodeId nodeId, Guid actorId, BlockId blockId) =>
        Execute(nodeId, actorId, (id, at) => { new DocumentService(documents).Remove(id, blockId, at); return true; });

    // All four commands share this authorization path. The generic result also lets
    // the void commands use the same boundary by returning an ignored boolean.
    private T Execute<T>(NodeId nodeId, Guid actorId, Func<DocumentId, DateTimeOffset, T> change) =>
        OperationBoundary.Execute(nodes, () =>
        {
            // Check participant availability and reload ownership for each mutation.
            // The host's previously rendered node is not the authorization source.
            ReferenceValidation.Require(nodes, "Participant", actorId);
            var node = nodes.GetById(nodeId) ?? throw new InvalidOperationException("Node not found.");
            node.EnsureAuthoredBy(actorId);
            // Translate the Graph-owned reference into Content's ID and use one timestamp.
            // Boundary-capable repositories keep the nested Content writes in this operation.
            return change(new DocumentId(node.DescriptionId.Value), AtlasTime.UtcNow);
        });
}

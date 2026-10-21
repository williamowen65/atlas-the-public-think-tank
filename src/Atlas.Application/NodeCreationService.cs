using Atlas.Content.Documents;
using Atlas.Contracts.Operations;
using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;
using GraphNodeCreation = Atlas.Graph.Nodes.NodeCreationService;

namespace Atlas.Application;

/// <summary>Coordinate Content and Graph under one topology boundary, without depending on a host or SQL.</summary>
public sealed class NodeCreationService(INodeRepository nodes, IDocumentRepository documents)
{
    public Node Create(string title, string markdown, NodeTypeId typeId, Guid authorId,
        IEnumerable<NodeTypeId> requestedTypes, NodeId? parentId = null, string shortDescription = "") =>
        NodeTopologyBoundary.Execute(nodes, () =>
        {
            var at = AtlasTime.UtcNow;
            var content = new DocumentService(documents);
            var graph = new GraphNodeCreation(nodes);
            // Prepare both proposals before writes; Graph validates IDs/ancestry/quota,
            // while Content owns block/document construction. One timestamp is shared.
            var draft = content.PrepareMarkdown(markdown, at);
            var node = graph.Prepare(title, new NodeDescriptionId(draft.Document.Id.Value),
                typeId, authorId, requestedTypes, at, parentId, shortDescription);
            content.Save(draft);
            graph.Save(node);
            // The outer boundary commits before returning. The caller may then publish
            // node.DomainEvents; dispatch remains outside this SQL transaction.
            return node;
        });
}

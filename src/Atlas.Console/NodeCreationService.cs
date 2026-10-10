using Atlas.Content.Blocks;
using Atlas.Content.Documents;
using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;

namespace Atlas.ConsoleApp;

/// <summary>Constructs the whole proposed node before writing, then commits Content and Graph together.</summary>
public sealed class NodeCreationService(INodeRepository nodes, IDocumentRepository documents)
{
    public Node Create(string title, string markdown, NodeTypeId typeId, Guid authorId,
        IEnumerable<NodeTypeId> requestedTypes, NodeId? parentId = null) =>
        OperationBoundary.Execute(nodes, () =>
        {
            ArgumentNullException.ThrowIfNull(requestedTypes);
            var requests = requestedTypes.ToList();
            ReferenceValidation.Require(nodes, "Participant", authorId);
            ReferenceValidation.Require(nodes, "NodeType", typeId.Value);
            foreach (var request in requests) ReferenceValidation.Require(nodes, "NodeType", request.Value);
            if (parentId is { } parent) ReferenceValidation.Require(nodes, "Node", parent.Value);
            var now = AtlasTime.UtcNow;
            var block = new MarkdownTextBlock(markdown, now);
            var document = new Document([block.Id], now);
            var node = new Node(new NodeTitle(title), new NodeDescriptionId(document.Id.Value), typeId,
                new NodeAuthorId(authorId), requests, now);
            if (parentId is { } parentToAttach)
            {
                GraphRelationshipValidation.EnsureAcyclic(node.Id, [parentToAttach], id => nodes.GetById(id)?.ParentNodeIds);
                node.AttachToParent(parentToAttach, authorId, now);
            }
            documents.SaveBlock(block);
            documents.Save(document);
            nodes.Save(node);
            return node;
        });
}

using Atlas.ConsoleApp.Eventing;
using Atlas.Content.Documents;
using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;
using Atlas.Participants.Participants;

namespace Atlas.ConsoleApp;

public static class NodeCreationWorkflow
{
    public static Node? Create(
        INodeRepository nodes,
        INodeTypeRepository nodeTypes,
        IDocumentRepository documents,
        Participant author,
        InMemoryEventPublisher eventPublisher,
        NodeTypeDefinition? preselectedType = null,
        Node? parent = null)
    {
        Console.Write("Title: ");
        var title = Console.ReadLine();

        Console.Write("Description (optional): ");
        var description = Console.ReadLine();

        var nodeType = preselectedType
            ?? ConsoleUi.ReadNodeType(
                nodeTypes,
                author.Id.Value.ToString());

        if (nodeType is null)
        {
            return null;
        }

        if (preselectedType is not null)
        {
            Console.WriteLine($"Type: {nodeType.Name}");
        }

        if (parent is not null)
        {
            Console.WriteLine($"Parent: {parent.Title}");
        }

        var requestedSubNodeTypes =
            ConsoleUi.ReadRequestedSubNodeTypes(nodeTypes);

        if (requestedSubNodeTypes.Count == 0)
        {
            return null;
        }

        try
        {
            var now = DateTimeOffset.UtcNow;
            var nodeTitle = new NodeTitle(title ?? string.Empty);

            var document = new Document(
                description ?? string.Empty,
                now);

            documents.Save(document);

            Console.WriteLine();
            Console.WriteLine(
                $"[ATLAS.CONTENT] Saved description document " +
                $"{document.Id}.");

            var node = new Node(
                nodeTitle,
                new NodeDescriptionId(document.Id.Value),
                nodeType.Id,
                new NodeAuthorId(author.Id.Value),
                requestedSubNodeTypes.Select(type => type.Id),
                now);

            if (parent is not null)
            {
                node.AttachToParent(parent.Id, now);
            }

            nodes.Save(node);

            Console.WriteLine(
                $"[ATLAS.GRAPH] Saved node {node.Id} with " +
                $"description reference {node.DescriptionId}.");

            foreach (var domainEvent in node.DomainEvents)
            {
                eventPublisher.Publish(domainEvent);
            }

            node.ClearDomainEvents();

            var relationship = parent is null
                ? string.Empty
                : $" under {parent.Title}";

            ConsoleUi.Pause(
                $"Node created as {nodeType.Name}{relationship}: " +
                $"{node.Title}");

            return node;
        }
        catch (ArgumentException exception)
        {
            ConsoleUi.Pause(
                $"Unable to create node: {exception.Message}");
            return null;
        }
    }
}

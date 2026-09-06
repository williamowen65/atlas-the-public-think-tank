using Atlas.ConsoleApp.Eventing;
using Atlas.Content.Documents;
using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;
using Atlas.Participants.Participants;
using Atlas.Contracts.Graph.V1;

namespace Atlas.ConsoleApp;

public static class NodeCommands
{
    public static void Run(
        Node node,
        INodeRepository nodes,
        INodeTypeRepository nodeTypes,
        IDocumentRepository documents,
        IParticipantRepository participants,
        InMemoryEventPublisher eventPublisher,
        string actorId)
    {
        var viewingNode = true;

        while (viewingNode)
        {
            Console.Clear();
            NodeDisplay.WriteDetails(node, nodeTypes, documents, participants);

            Console.WriteLine();
            Console.WriteLine("Choose an action:");
            Console.WriteLine("1. Rename");
            Console.WriteLine("2. Change description");
            Console.WriteLine("3. Change type");
            Console.WriteLine("4. Archive");
            Console.WriteLine("5. Restore");
            Console.WriteLine("6. Return to node browser");
            Console.WriteLine();

            Console.Write("Selection: ");

            try
            {
                switch (Console.ReadLine())
                {
                    case "1":
                        Rename(node, nodes);
                        break;

                    case "2":
                        ChangeDescription(node, nodes, documents);
                        break;

                    case "3":
                        ChangeType(
                            node,
                            nodes,
                            nodeTypes,
                            actorId);
                        break;

                    case "4":
                        node.Archive(DateTimeOffset.UtcNow);
                        nodes.Save(node);
                        PublishDomainEvents(node, eventPublisher);
                        ConsoleUi.Pause("Node archived and saved.");
                        break;

                    case "5":
                        node.Restore(DateTimeOffset.UtcNow);
                        nodes.Save(node);
                        PublishDomainEvents(node, eventPublisher);
                        ConsoleUi.Pause("Node restored and saved.");
                        break;

                    case "6":
                        viewingNode = false;
                        break;

                    default:
                        ConsoleUi.Pause("That is not a valid selection.");
                        break;
                }
            }
            catch (ArgumentException exception)
            {
                ConsoleUi.Pause(
                    $"Unable to update node: {exception.Message}");
            }
        }
    }


    private static void PublishDomainEvents(
        Node node,
        InMemoryEventPublisher eventPublisher)
    {
        foreach (var domainEvent in
                 node.DomainEvents.OfType<NodeLifecycleEventV1>())
        {
            eventPublisher.Publish(
                GraphEventContractMapper.ToIntegrationContract(
                    domainEvent));
        }

        node.ClearDomainEvents();
    }

    private static void Rename(Node node, INodeRepository nodes)
    {
        Console.Write("New title: ");
        var title = Console.ReadLine();

        node.Rename(
            new NodeTitle(title ?? string.Empty),
            DateTimeOffset.UtcNow);

        nodes.Save(node);
        ConsoleUi.Pause("Node renamed and saved.");
    }

    private static void ChangeDescription(
        Node node,
        INodeRepository nodes,
        IDocumentRepository documents)
    {
        var currentDocument = documents.GetById(
            new DocumentId(node.DescriptionId.Value));

        Console.WriteLine("Current description:");
        Console.WriteLine(
            string.IsNullOrWhiteSpace(currentDocument?.Content)
                ? "(none)"
                : currentDocument.Content);
        Console.WriteLine();
        Console.Write("New description (blank clears it): ");
        var description = Console.ReadLine();
        var changedAt = DateTimeOffset.UtcNow;

        var replacement = new Document(
            description ?? string.Empty,
            changedAt);

        documents.Save(replacement);

        node.ReplaceDescriptionReference(
            new NodeDescriptionId(replacement.Id.Value),
            changedAt);

        nodes.Save(node);

        ConsoleUi.Pause(
            $"Description replaced with document {replacement.Id}.");
    }

    private static void ChangeType(
        Node node,
        INodeRepository nodes,
        INodeTypeRepository nodeTypes,
        string actorId)
    {
        var nodeType = ConsoleUi.ReadNodeType(
            nodeTypes,
            actorId);

        if (nodeType is null)
        {
            return;
        }

        node.ChangeType(
            nodeType.Id,
            DateTimeOffset.UtcNow);

        nodes.Save(node);

        ConsoleUi.Pause(
            $"Node type changed to {nodeType.Name} and saved.");
    }
}

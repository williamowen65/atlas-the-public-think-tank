using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;

namespace Atlas.ConsoleApp;

public static class NodeCommands
{
    public static void Run(
        Node node,
        INodeRepository nodes,
        INodeTypeRepository nodeTypes,
        string actorId)
    {
        var viewingNode = true;

        while (viewingNode)
        {
            Console.Clear();
            NodeDisplay.WriteDetails(node, nodeTypes);

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
                        ChangeDescription(node, nodes);
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
                        ConsoleUi.Pause("Node archived and saved.");
                        break;

                    case "5":
                        node.Restore(DateTimeOffset.UtcNow);
                        nodes.Save(node);
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
        INodeRepository nodes)
    {
        Console.WriteLine("Current description:");
        Console.WriteLine(
            string.IsNullOrWhiteSpace(node.Description.Value)
                ? "(none)"
                : node.Description.Value);
        Console.WriteLine();
        Console.Write("New description (blank clears it): ");
        var description = Console.ReadLine();

        node.ChangeDescription(
            new NodeDescription(description ?? string.Empty),
            DateTimeOffset.UtcNow);

        nodes.Save(node);
        ConsoleUi.Pause("Node description updated and saved.");
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

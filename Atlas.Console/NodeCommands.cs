using Atlas.Graph.Nodes;

namespace Atlas.ConsoleApp;

public static class NodeCommands
{
    public static void Run(Node node, INodeRepository nodes)
    {
        var viewingNode = true;

        while (viewingNode)
        {
            Console.Clear();
            NodeDisplay.WriteDetails(node);

            Console.WriteLine();
            Console.WriteLine("Choose an action:");
            Console.WriteLine("1. Rename");
            Console.WriteLine("2. Change type");
            Console.WriteLine("3. Archive");
            Console.WriteLine("4. Restore");
            Console.WriteLine("5. Return to node browser");
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
                        ChangeType(node, nodes);
                        break;

                    case "3":
                        node.Archive(DateTimeOffset.UtcNow);
                        nodes.Save(node);
                        ConsoleUi.Pause("Node archived and saved.");
                        break;

                    case "4":
                        node.Restore(DateTimeOffset.UtcNow);
                        nodes.Save(node);
                        ConsoleUi.Pause("Node restored and saved.");
                        break;

                    case "5":
                        viewingNode = false;
                        break;

                    default:
                        ConsoleUi.Pause("That is not a valid selection.");
                        break;
                }
            }
            catch (ArgumentException exception)
            {
                ConsoleUi.Pause($"Unable to update node: {exception.Message}");
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

    private static void ChangeType(Node node, INodeRepository nodes)
    {
        var nodeType = ConsoleUi.ReadNodeType();

        if (nodeType is null)
        {
            return;
        }

        node.ChangeType(nodeType.Value, DateTimeOffset.UtcNow);
        nodes.Save(node);

        ConsoleUi.Pause(
            $"Node type changed to {nodeType.Value} and saved.");
    }
}

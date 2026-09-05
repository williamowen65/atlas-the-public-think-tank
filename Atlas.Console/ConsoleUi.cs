using Atlas.Graph.Nodes;

namespace Atlas.ConsoleApp;

public static class ConsoleUi
{
    public static NodeType? ReadNodeType()
    {
        var nodeTypes = Enum.GetValues<NodeType>();

        Console.WriteLine();
        Console.WriteLine("Node types:");

        for (var index = 0; index < nodeTypes.Length; index++)
        {
            Console.WriteLine($"{index + 1}. {nodeTypes[index]}");
        }

        Console.WriteLine();
        Console.Write("Type: ");

        if (!int.TryParse(Console.ReadLine(), out var selection) ||
            selection < 1 ||
            selection > nodeTypes.Length)
        {
            Pause("That is not a valid node type.");
            return null;
        }

        return nodeTypes[selection - 1];
    }

    public static void Pause(string? message = null)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            Console.WriteLine();
            Console.WriteLine(message);
        }

        Console.WriteLine();
        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }
}

using Atlas.Graph.Nodes.NodeTypes;

namespace Atlas.ConsoleApp;

public static class ConsoleUi
{
    public static NodeTypeDefinition? ReadNodeType(
        INodeTypeRepository nodeTypes,
        string ownerId)
    {
        var availableTypes = nodeTypes
            .GetAll()
            .Where(type => !type.IsArchived)
            .OrderBy(type => type.Name)
            .ToList();

        Console.WriteLine();
        Console.WriteLine("Node types:");

        for (var index = 0; index < availableTypes.Count; index++)
        {
            var kind = availableTypes[index].IsSystemDefined
                ? "system"
                : "custom";

            Console.WriteLine(
                $"{index + 1}. {availableTypes[index].Name} ({kind})");
        }

        Console.WriteLine($"{availableTypes.Count + 1}. Create a custom type");
        Console.WriteLine("0. Cancel");
        Console.WriteLine();
        Console.Write("Type: ");

        if (!int.TryParse(Console.ReadLine(), out var selection))
        {
            Pause("That is not a valid selection.");
            return null;
        }

        if (selection == 0)
        {
            return null;
        }

        if (selection == availableTypes.Count + 1)
        {
            return CreateCustomNodeType(nodeTypes, ownerId);
        }

        if (selection < 1 || selection > availableTypes.Count)
        {
            Pause("That is not a valid node type.");
            return null;
        }

        return availableTypes[selection - 1];
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

    private static NodeTypeDefinition? CreateCustomNodeType(
        INodeTypeRepository nodeTypes,
        string ownerId)
    {
        Console.WriteLine();
        Console.Write("Custom type name: ");
        var name = Console.ReadLine();

        Console.Write("Description: ");
        var description = Console.ReadLine();

        try
        {
            var nodeType = NodeTypeDefinition.CreateCustom(
                name ?? string.Empty,
                description ?? string.Empty,
                ownerId,
                DateTimeOffset.UtcNow);

            nodeTypes.Save(nodeType);
            return nodeType;
        }
        catch (ArgumentException exception)
        {
            Pause($"Unable to create node type: {exception.Message}");
            return null;
        }
    }
}

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


    public static IReadOnlyCollection<NodeTypeDefinition>
        ReadRequestedSubNodeTypes(
            INodeTypeRepository nodeTypes)
    {
        var availableTypes = nodeTypes
            .GetAll()
            .Where(type =>
                !type.IsArchived &&
                !string.Equals(
                    type.Name,
                    "Comment",
                    StringComparison.OrdinalIgnoreCase))
            .OrderBy(type => type.Name)
            .ToList();

        var commentType = nodeTypes
            .GetAll()
            .Single(type =>
                string.Equals(
                    type.Name,
                    "Comment",
                    StringComparison.OrdinalIgnoreCase));

        Console.WriteLine();
        Console.WriteLine(
            "Every node requests Comment sub-nodes by default.");
        Console.WriteLine(
            "Select any additional requested sub-node types:");

        for (var index = 0; index < availableTypes.Count; index++)
        {
            Console.WriteLine(
                $"{index + 1}. {availableTypes[index].Name}");
        }

        Console.WriteLine();
        Console.Write(
            "Selections (comma-separated, blank for Comment only): ");

        var input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
        {
            return [commentType];
        }

        var selectedTypes = new List<NodeTypeDefinition>
        {
            commentType
        };

        foreach (var value in input.Split(
                     ',',
                     StringSplitOptions.RemoveEmptyEntries |
                     StringSplitOptions.TrimEntries))
        {
            if (!int.TryParse(value, out var selection) ||
                selection < 1 ||
                selection > availableTypes.Count)
            {
                Pause(
                    $"'{value}' is not a valid sub-node type selection.");
                return [];
            }

            selectedTypes.Add(availableTypes[selection - 1]);
        }

        return selectedTypes
            .DistinctBy(type => type.Id)
            .ToList();
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

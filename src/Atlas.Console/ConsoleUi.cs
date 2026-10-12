using Atlas.Graph.Nodes.NodeTypes;

namespace Atlas.ConsoleApp;

/// <summary>Provides reusable console input and node-type selection interactions.</summary>
public static class ConsoleUi
{
    /// <summary>Reads node type from the console interaction.</summary>
    public static NodeTypeDefinition? ReadNodeType(
        INodeTypeRepository nodeTypes,
        string ownerId
    )
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

        if (!int.TryParse(
            Console.ReadLine(),
            out var selection
        ))
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
            return CreateCustomNodeType(
                nodeTypes,
                ownerId
            );
        }

        if (selection < 1 || selection > availableTypes.Count)
        {
            Pause("That is not a valid node type.");
            return null;
        }

        return availableTypes[selection - 1];
    }


    /// <summary>Reads optional child-Node type requests; null means cancelled or invalid input.</summary>
    public static IReadOnlyCollection<NodeTypeDefinition>?
        ReadRequestedSubNodeTypes(
            INodeTypeRepository nodeTypes,
            string ownerId
        )
    {
        var allTypes = nodeTypes.GetAll();

        // Comments have their own domain. Requested types only describe actual child Nodes.
        var availableTypes = allTypes
            .Where(type => !type.IsArchived)
            .OrderBy(type => type.Name)
            .ToList();

        Console.WriteLine();
        Console.WriteLine(
            "Select requested sub-node types (comments are separate):");

        for (var index = 0; index < availableTypes.Count; index++)
        {
            Console.WriteLine(
                $"{index + 1}. {availableTypes[index].Name}");
        }

        var createTypeSelection = availableTypes.Count + 1;

        Console.WriteLine(
            $"{createTypeSelection}. Create a custom type");
        Console.WriteLine();
        Console.Write(
            "Selections (comma-separated, blank for none, 0 cancels): ");

        var input = Console.ReadLine();

        // Empty is a valid request list; null distinguishes cancellation/invalid input.
        if (input is null || input.Trim() == "0") return null;
        if (string.IsNullOrWhiteSpace(input))
        {
            return [];
        }

        var selections = new List<int>();

        foreach (var value in input.Split(
            ',',
            StringSplitOptions.RemoveEmptyEntries |
                     StringSplitOptions.TrimEntries
        ))
        {
            if (!int.TryParse(
                value,
                out var selection
            ) ||
                selection < 1 ||
                selection > createTypeSelection)
            {
                Pause(
                    $"'{value}' is not a valid sub-node type selection.");
                return null;
            }

            selections.Add(selection);
        }

        if (selections.Count == 0)
        {
            Pause("Select a type number, or leave the input blank for none.");
            return null;
        }

        var selectedTypes = selections
            .Where(selection => selection != createTypeSelection)
            .Select(selection => availableTypes[selection - 1])
            .ToList();

        if (selections.Contains(createTypeSelection))
        {
            var customType = CreateCustomNodeType(
                nodeTypes,
                ownerId
            );

            if (customType is null)
            {
                return null;
            }

            selectedTypes.Add(customType);
        }

        return selectedTypes
            .DistinctBy(type => type.Id)
            .ToList();
    }

    /// <summary>Pauses the console workflow after optionally displaying a status message.</summary>
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

    /// <summary>Creates custom node type during the current workflow.</summary>
    public static NodeTypeDefinition? CreateCustomNodeType(
        INodeTypeRepository nodeTypes,
        string ownerId
    )
    {
        Console.WriteLine();
        Console.Write("Custom type name: ");
        var name = Console.ReadLine();

        var existingType = nodeTypes
            .GetAll()
            .FirstOrDefault(type =>
                string.Equals(
                    type.Name,
                    name?.Trim(),
                    StringComparison.OrdinalIgnoreCase
                ));

        if (existingType is not null)
        {
            Pause(
                $"The type '{existingType.Name}' already exists. " +
                "Select it from the known types instead.");
            return null;
        }

        Console.Write("Description: ");
        var description = Console.ReadLine();

        Console.Write("Automatically pluralize this type? (Y/n): ");
        var pluralizeResponse = Console.ReadLine()?.Trim();

        bool autoPluralize;

        if (string.IsNullOrWhiteSpace(pluralizeResponse) ||
            string.Equals(
                pluralizeResponse,
                "y",
                StringComparison.OrdinalIgnoreCase
            ) ||
            string.Equals(
                pluralizeResponse,
                "yes",
                StringComparison.OrdinalIgnoreCase
            ))
        {
            autoPluralize = true;
        }
        else if (string.Equals(
            pluralizeResponse,
            "n",
            StringComparison.OrdinalIgnoreCase
        ) ||
                 string.Equals(
                     pluralizeResponse,
                     "no",
                     StringComparison.OrdinalIgnoreCase
                 ))
        {
            autoPluralize = false;
        }
        else
        {
            Pause("Please enter Y or N for automatic pluralization.");
            return null;
        }

        try
        {
            var nodeType = NodeTypeDefinition.CreateCustom(
                name ?? string.Empty,
                description ?? string.Empty,
                ownerId,
                AtlasTime.UtcNow,
                autoPluralize
            );

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

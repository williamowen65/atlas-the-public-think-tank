using Atlas.Graph.Nodes;
using Atlas.Graph.Tags;
using Atlas.Participants.Participants;

namespace Atlas.ConsoleApp;

/// <summary>Coordinates the Console tag-management workflow through Graph-owned services.</summary>
public static class TagCommands
{
    /// <summary>Runs tag application, replacement, and removal actions for one node.</summary>
    public static void Run(
        Node node,
        ITagDefinitionRepository definitions,
        INodeTagRepository nodeTags,
        Participant currentParticipant)
    {
        var service = new NodeTagApplicationService(definitions, nodeTags);
        var managing = true;

        while (managing)
        {
            Console.Clear();
            Console.WriteLine($"TAGS FOR {node.Title.Value.ToUpperInvariant()}");
            Console.WriteLine(new string('-', $"TAGS FOR {node.Title.Value}".Length));
            WriteNumberedTags(node, nodeTags, definitions);
            Console.WriteLine();
            Console.WriteLine("1. Apply tag");
            Console.WriteLine("2. Replace tag");
            Console.WriteLine("3. Remove tag");
            Console.WriteLine("4. Return to node");
            Console.Write("Selection: ");

            try
            {
                switch (Console.ReadLine())
                {
                    case "1":
                        Apply(node, service, definitions, currentParticipant);
                        break;
                    case "2":
                        Replace(node, service, nodeTags, definitions, currentParticipant);
                        break;
                    case "3":
                        Remove(node, service, nodeTags, definitions, currentParticipant);
                        break;
                    case "4":
                        managing = false;
                        break;
                    default:
                        ConsoleUi.Pause("That is not a valid selection.");
                        break;
                }
            }
            catch (Exception exception) when (
                exception is ArgumentException or
                InvalidOperationException or
                UnauthorizedAccessException or
                KeyNotFoundException or
                IOException)
            {
                ConsoleUi.Pause($"Unable to change tags: {exception.Message}");
            }
        }
    }

    private static void Apply(
        Node node,
        NodeTagApplicationService service,
        ITagDefinitionRepository definitions,
        Participant participant)
    {
        var text = ReadTagText(definitions);

        if (text is null)
        {
            return;
        }

        var applied = service.Apply(
            node,
            text,
            participant.Id.Value,
            participant.IsActive,
            DateTimeOffset.UtcNow);

        var definition = definitions.GetById(applied.TagDefinitionId)!;
        ConsoleUi.Pause($"Applied '{definition.Text}'.");
    }

    private static void Replace(
        Node node,
        NodeTagApplicationService service,
        INodeTagRepository nodeTags,
        ITagDefinitionRepository definitions,
        Participant participant)
    {
        var selected = ReadNodeTag(node, nodeTags, definitions, "replace");

        if (selected is null)
        {
            return;
        }

        var text = ReadTagText(definitions);

        if (text is null)
        {
            return;
        }

        service.Replace(
            node,
            selected.Id,
            text,
            participant.Id.Value,
            participant.IsActive,
            actorIsModerator: false,
            DateTimeOffset.UtcNow);

        ConsoleUi.Pause("Tag replaced without renaming the shared definition.");
    }

    private static void Remove(
        Node node,
        NodeTagApplicationService service,
        INodeTagRepository nodeTags,
        ITagDefinitionRepository definitions,
        Participant participant)
    {
        var selected = ReadNodeTag(node, nodeTags, definitions, "remove");

        if (selected is null)
        {
            return;
        }

        service.Remove(
            node,
            selected.Id,
            participant.Id.Value,
            participant.IsActive,
            actorIsModerator: false,
            DateTimeOffset.UtcNow);

        ConsoleUi.Pause("Tag removed from this node.");
    }

    private static string? ReadTagText(ITagDefinitionRepository definitions)
    {
        Console.WriteLine();
        Console.Write("Tag text (blank cancels): ");
        var input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        var normalized = TagDefinition.Normalize(input);
        var exact = definitions.GetByNormalizedText(normalized);

        if (exact is not null)
        {
            return exact.Text;
        }

        var matches = definitions
            .GetAll()
            .Where(definition =>
                !definition.IsSuppressed &&
                definition.NormalizedText.Contains(normalized, StringComparison.Ordinal))
            .OrderBy(definition => definition.Text)
            .Take(8)
            .ToList();

        if (matches.Count == 0)
        {
            return input;
        }

        Console.WriteLine("Existing tags:");

        for (var index = 0; index < matches.Count; index++)
        {
            Console.WriteLine($"{index + 1}. {matches[index].Text}");
        }

        Console.WriteLine("0. Create the entered tag");
        Console.Write("Selection: ");

        if (!int.TryParse(Console.ReadLine(), out var selection) ||
            selection < 0 ||
            selection > matches.Count)
        {
            ConsoleUi.Pause("That is not a valid tag selection.");
            return null;
        }

        return selection == 0 ? input : matches[selection - 1].Text;
    }

    private static NodeTag? ReadNodeTag(
        Node node,
        INodeTagRepository nodeTags,
        ITagDefinitionRepository definitions,
        string action)
    {
        var active = TagDisplay.ResolveActive(node, nodeTags, definitions);

        if (active.Count == 0)
        {
            ConsoleUi.Pause("This node has no active tags.");
            return null;
        }

        WriteNumberedTags(node, nodeTags, definitions);
        Console.Write($"Tag to {action} (0 cancels): ");

        if (!int.TryParse(Console.ReadLine(), out var selection) ||
            selection < 0 ||
            selection > active.Count)
        {
            ConsoleUi.Pause("That is not a valid tag selection.");
            return null;
        }

        return selection == 0 ? null : active[selection - 1].Association;
    }

    private static void WriteNumberedTags(
        Node node,
        INodeTagRepository nodeTags,
        ITagDefinitionRepository definitions)
    {
        var active = TagDisplay.ResolveActive(node, nodeTags, definitions);

        if (active.Count == 0)
        {
            Console.WriteLine("No tags have been applied.");
            return;
        }

        for (var index = 0; index < active.Count; index++)
        {
            Console.WriteLine($"{index + 1}. {active[index].Definition.Text}");
        }
    }
}

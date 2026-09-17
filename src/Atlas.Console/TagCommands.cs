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
            Console.WriteLine($"Acting as: {currentParticipant.DisplayName}");
            Console.WriteLine();
            WriteNumberedTags(node, nodeTags, definitions);
            Console.WriteLine();
            Console.WriteLine("1. Apply tag");
            Console.WriteLine("2. Correct or replace a tag I control");
            Console.WriteLine("3. Withdraw a tag I control");
            if (currentParticipant.Id.Value == node.AuthorId.Value)
            {
                Console.WriteLine("4. Manage how tags appear on my node");
            }
            Console.WriteLine("5. View hidden and disputed tags");
            Console.WriteLine("6. Return to node");
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
                        if (currentParticipant.Id.Value != node.AuthorId.Value)
                        {
                            ConsoleUi.Pause("Only the node author can manage how tags appear on this node.");
                            break;
                        }
                        ManagePresentation(node, service, nodeTags, definitions, currentParticipant);
                        break;
                    case "5":
                        TagDisplay.WriteHiddenAndDisputed(node, nodeTags, definitions);
                        ConsoleUi.Pause();
                        break;
                    case "6":
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

    private static void ManagePresentation(Node node, NodeTagApplicationService service,
        INodeTagRepository nodeTags, ITagDefinitionRepository definitions, Participant participant)
    {
        var selected = ReadNodeTagForPresentation(node, nodeTags, definitions);
        if (selected is null) return;

        Console.WriteLine();
        Console.WriteLine($"Selected: {selected.Value.Definition.Text}");
        Console.WriteLine($"Current presentation: {selected.Value.Association.Disposition}");
        Console.WriteLine();
        Console.WriteLine("1. Community");
        Console.WriteLine("2. Endorsed");
        Console.WriteLine("3. Hidden");
        Console.WriteLine("4. Disputed");
        Console.WriteLine("0. Cancel");
        Console.Write("New presentation: ");

        if (!int.TryParse(Console.ReadLine(), out var selection) || selection < 0 || selection > 4)
        {
            ConsoleUi.Pause("That is not a valid presentation selection.");
            return;
        }

        if (selection == 0) return;

        var disposition = selection switch
        {
            1 => NodeTagDisposition.Community,
            2 => NodeTagDisposition.Endorsed,
            3 => NodeTagDisposition.Hidden,
            4 => NodeTagDisposition.Disputed,
            _ => throw new InvalidOperationException("Unsupported presentation selection.")
        };

        service.SetDisposition(node, selected.Value.Association.Id, disposition,
            participant.Id.Value, participant.IsActive);
        ConsoleUi.Pause($"'{selected.Value.Definition.Text}' now appears as {disposition}.");
    }

    private static (NodeTag Association, TagDefinition Definition)? ReadNodeTagForPresentation(
        Node node,
        INodeTagRepository nodeTags,
        ITagDefinitionRepository definitions)
    {
        var tags = TagDisplay.ResolveActive(node, nodeTags, definitions)
            .OrderBy(item => Array.IndexOf(
                new[]
                {
                    NodeTagDisposition.Community,
                    NodeTagDisposition.Endorsed,
                    NodeTagDisposition.Hidden,
                    NodeTagDisposition.Disputed
                },
                item.Association.Disposition))
            .ThenBy(item => item.Association.CreatedAt)
            .ToList();

        if (tags.Count == 0)
        {
            ConsoleUi.Pause("There are no tags to manage.");
            return null;
        }

        NodeTagDisposition? currentGroup = null;
        for (var index = 0; index < tags.Count; index++)
        {
            var disposition = tags[index].Association.Disposition;
            if (currentGroup != disposition)
            {
                if (currentGroup is not null) Console.WriteLine();
                Console.WriteLine(disposition.ToString().ToUpperInvariant());
                currentGroup = disposition;
            }

            Console.WriteLine($"{index + 1}. {tags[index].Definition.Text}");
        }

        Console.WriteLine();
        Console.Write("Tag to manage (0 cancels): ");
        if (!int.TryParse(Console.ReadLine(), out var selection) ||
            selection < 0 || selection > tags.Count)
        {
            ConsoleUi.Pause("That is not a valid tag selection.");
            return null;
        }

        return selection == 0 ? null : tags[selection - 1];
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
        var selected = ReadNodeTag(node, nodeTags, definitions, "replace",
            association => CanParticipantModify(association, node, participant));

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
        var selected = ReadNodeTag(node, nodeTags, definitions, "withdraw",
            association => CanParticipantModify(association, node, participant));

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

        ConsoleUi.Pause("Tag withdrawn from this node. Its history was preserved.");
    }

    private static bool CanParticipantModify(
        NodeTag association,
        Node node,
        Participant participant)
    {
        return association.Disposition == NodeTagDisposition.Endorsed
            ? participant.Id.Value == node.AuthorId.Value
            : association.AppliedByParticipantId == participant.Id.Value;
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
        string action,
        Func<NodeTag, bool>? predicate = null)
    {
        var active = TagDisplay.ResolveActive(node, nodeTags, definitions)
            .Where(item => predicate?.Invoke(item.Association) ?? true)
            .ToList();

        if (active.Count == 0)
        {
            ConsoleUi.Pause($"There are no tags you can {action}.");
            return null;
        }

        WriteNumberedTags(active);
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
        var active = TagDisplay.ResolveActive(node, nodeTags, definitions)
            .Where(item => item.Association.Disposition is
                NodeTagDisposition.Community or NodeTagDisposition.Endorsed)
            .ToList();

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

    private static void WriteNumberedTags(
        IReadOnlyList<(NodeTag Association, TagDefinition Definition)> tags)
    {
        for (var index = 0; index < tags.Count; index++)
        {
            Console.WriteLine($"{index + 1}. {tags[index].Definition.Text}");
        }
    }
}

using Atlas.Graph.Nodes;
using Atlas.Graph.Tags;

namespace Atlas.ConsoleApp;

/// <summary>Composes Graph-owned tag definitions and node associations for display.</summary>
public static class TagDisplay
{
    /// <summary>Returns up to three active tags in deterministic pre-voting order.</summary>
    public static string FormatCompact(
        Node node,
        INodeTagRepository nodeTags,
        ITagDefinitionRepository definitions,
        NodeTagDisposition disposition)
    {
        var labels = ResolveActive(node, nodeTags, definitions)
            .Where(item => item.Association.Disposition == disposition)
            .Take(3)
            .Select(item => item.Definition.Text)
            .ToList();

        var remaining = ResolveActive(node, nodeTags, definitions)
            .Count(item => item.Association.Disposition == disposition) - labels.Count;

        if (labels.Count == 0)
        {
            return "—";
        }

        return remaining > 0
            ? $"{string.Join(", ", labels)} +{remaining}"
            : string.Join(", ", labels);
    }

    /// <summary>Writes the complete active tag set for a node detail view.</summary>
    public static void WriteDetails(
        Node node,
        INodeTagRepository nodeTags,
        ITagDefinitionRepository definitions)
    {
        var active = ResolveActive(node, nodeTags, definitions)
            .Where(item => item.Association.Disposition is NodeTagDisposition.Endorsed or NodeTagDisposition.Community)
            .ToList();

        Console.WriteLine();
        Console.WriteLine("TAGS");
        Console.WriteLine("----");

        if (active.Count == 0)
        {
            Console.WriteLine("No tags have been applied.");
            return;
        }

        foreach (var group in active.GroupBy(item => item.Association.Disposition))
        {
            Console.WriteLine($"{group.Key}:");
            foreach (var item in group)
            {
                Console.WriteLine($"- {item.Definition.Text}");
            }
        }
    }

    /// <summary>Writes author-hidden and disputed tags for an explicit review action.</summary>
    public static void WriteHiddenAndDisputed(Node node, INodeTagRepository nodeTags,
        ITagDefinitionRepository definitions)
    {
        var items = ResolveActive(node, nodeTags, definitions)
            .Where(item => item.Association.Disposition is NodeTagDisposition.Hidden or NodeTagDisposition.Disputed)
            .ToList();
        Console.WriteLine(items.Count == 0 ? "No hidden or disputed tags." : "HIDDEN OR DISPUTED TAGS");
        foreach (var item in items)
        {
            Console.WriteLine($"- {item.Definition.Text} ({item.Association.Disposition})");
        }
    }

    /// <summary>Resolves active associations to active reusable definitions.</summary>
    public static IReadOnlyList<(NodeTag Association, TagDefinition Definition)> ResolveActive(
        Node node,
        INodeTagRepository nodeTags,
        ITagDefinitionRepository definitions)
    {
        return nodeTags
            .GetActiveForNode(node.Id)
            .Select(association => new
            {
                Association = association,
                Definition = definitions.GetById(association.TagDefinitionId)
            })
            .Where(item => item.Definition is not null && !item.Definition.IsSuppressed)
            .OrderBy(item => item.Association.CreatedAt)
            .ThenBy(item => item.Association.Id.Value)
            .Select(item => (item.Association, item.Definition!))
            .ToList();
    }
}

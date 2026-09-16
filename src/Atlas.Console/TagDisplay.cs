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
        ITagDefinitionRepository definitions)
    {
        var labels = ResolveActive(node, nodeTags, definitions)
            .Take(3)
            .Select(item => item.Definition.Text)
            .ToList();

        var remaining = ResolveActive(node, nodeTags, definitions).Count - labels.Count;

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
        var active = ResolveActive(node, nodeTags, definitions);

        Console.WriteLine();
        Console.WriteLine("TAGS");
        Console.WriteLine("----");

        if (active.Count == 0)
        {
            Console.WriteLine("No tags have been applied.");
            return;
        }

        foreach (var item in active)
        {
            Console.WriteLine($"- {item.Definition.Text}");
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

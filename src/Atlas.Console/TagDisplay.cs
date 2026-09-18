using Atlas.Graph.Nodes;
using Atlas.Graph.Tags;
using Atlas.Voting;
using Atlas.Voting.Data;
using Atlas.Voting.Target;
using Atlas.Voting.Votes;

namespace Atlas.ConsoleApp;

/// <summary>Composes Graph-owned tag definitions and node associations for display.</summary>
public static class TagDisplay
{
    /// <summary>Returns up to three active tags with their current scores.</summary>
    public static string FormatCompact(
        Node node,
        INodeTagRepository nodeTags,
        ITagDefinitionRepository definitions,
        IVoteRepository votes,
        NodeTagDisposition disposition)
    {
        var matchingTags = ResolveActive(node, nodeTags, definitions)
            .Where(item => item.Association.Disposition == disposition)
            .ToList();

        var labels = matchingTags
            .Take(3)
            .Select(item =>
            {
                var score = new GetNodeTagVoteSummary(votes).Execute(
                    new NodeTagVoteTarget(item.Association.Id.Value)).Score;

                return $"({score}) {item.Definition.Text}";
            })
            .ToList();

        var remaining = matchingTags.Count - labels.Count;

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
        ITagDefinitionRepository definitions,
        IVoteRepository votes,
        ParticipantId? participantId = null)
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
        }
        else
        {
            foreach (var group in active.GroupBy(item => item.Association.Disposition))
            {
                Console.WriteLine($"{group.Key}:");
                foreach (var item in group)
                {
                    WriteTagWithScore(item, votes, participantId);
                }
            }
        }

        var superseded = ResolveAll(node, nodeTags, definitions)
            .Where(item => item.Association.LifecycleState == NodeTagLifecycleState.Superseded)
            .ToList();

        if (superseded.Count > 0)
        {
            Console.WriteLine("Superseded (history):");
            foreach (var item in superseded)
            {
                WriteTagWithScore(item, votes, participantId);
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

    /// <summary>Resolves active and historical associations for public history.</summary>
    public static IReadOnlyList<(NodeTag Association, TagDefinition Definition)> ResolveAll(
        Node node,
        INodeTagRepository nodeTags,
        ITagDefinitionRepository definitions)
    {
        return nodeTags.GetAll()
            .Where(association => association.NodeId == node.Id)
            .Select(association => new
            {
                Association = association,
                Definition = definitions.GetById(association.TagDefinitionId)
            })
            .Where(item => item.Definition is not null)
            .OrderBy(item => item.Association.CreatedAt)
            .ThenBy(item => item.Association.Id.Value)
            .Select(item => (item.Association, item.Definition!))
            .ToList();
    }

    private static void WriteTagWithScore(
        (NodeTag Association, TagDefinition Definition) item,
        IVoteRepository votes,
        ParticipantId? participantId)
    {
        var summary = new GetNodeTagVoteSummary(votes).Execute(
            new NodeTagVoteTarget(item.Association.Id.Value),
            participantId);
        var myVote = summary.CurrentParticipantVote switch
        {
            1 => ", my vote: up",
            -1 => ", my vote: down",
            _ => string.Empty
        };

        Console.WriteLine($"- {item.Definition.Text} (score: {summary.Score}{myVote})");
    }
}

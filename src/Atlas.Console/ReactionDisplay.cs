using Atlas.Graph.Nodes;
using Atlas.Graph.Reactions;
using Atlas.Voting;
using Atlas.Voting.Data;
using Atlas.Voting.Target;
using Atlas.Voting.Votes;

namespace Atlas.ConsoleApp;

/// <summary>Composes Graph-owned reaction definitions and node associations for display.</summary>
public static class ReactionDisplay
{
    /// <summary>Returns up to three active reactions with their current scores.</summary>
    public static string FormatCompact(
        Node node,
        INodeReactionRepository nodeTags,
        IReactionDefinitionRepository definitions,
        IVoteRepository votes)
    {
        var matchingTags = ResolveActive(node, nodeTags, definitions)
            .Where(item => votes.GetTargetVotes(
                new NodeReactionVoteTarget(item.Association.Id.Value)).Count > 0)
            .ToList();

        var labels = matchingTags
            .Take(3)
            .Select(item =>
            {
                var score = new GetNodeReactionVoteSummary(votes).Execute(
                    new NodeReactionVoteTarget(item.Association.Id.Value)).Score;

                return $"{item.Definition.Emoji} {item.Definition.Text} ({score})";
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

    /// <summary>Writes the complete active reaction set for a node detail view.</summary>
    public static void WriteDetails(
        Node node,
        INodeReactionRepository nodeTags,
        IReactionDefinitionRepository definitions,
        IVoteRepository votes,
        ParticipantId? participantId = null)
    {
        var active = ResolveActive(node, nodeTags, definitions)
            .Where(item => votes.GetTargetVotes(
                new NodeReactionVoteTarget(item.Association.Id.Value)).Count > 0)
            .ToList();

        Console.WriteLine();
        Console.WriteLine("REACTIONS");
        Console.WriteLine("---------");

        if (active.Count == 0)
        {
            Console.WriteLine("No reactions yet.");
        }
        else
        {
            foreach (var item in active)
            {
                WriteTagWithScore(item, votes, participantId);
            }
        }

        var superseded = ResolveAll(node, nodeTags, definitions)
            .Where(item => item.Association.LifecycleState == NodeReactionLifecycleState.Superseded)
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
    public static void WriteHiddenAndDisputed(Node node, INodeReactionRepository nodeTags,
        IReactionDefinitionRepository definitions)
    {
        var items = ResolveActive(node, nodeTags, definitions)
            .Where(item => item.Association.Disposition is NodeReactionDisposition.Hidden or NodeReactionDisposition.Disputed)
            .ToList();
        Console.WriteLine(items.Count == 0 ? "No hidden or disputed tags." : "HIDDEN OR DISPUTED TAGS");
        foreach (var item in items)
        {
            Console.WriteLine($"- {item.Definition.Text} ({item.Association.Disposition})");
        }
    }

    /// <summary>Resolves active associations to active reusable definitions.</summary>
    public static IReadOnlyList<(NodeReaction Association, ReactionDefinition Definition)> ResolveActive(
        Node node,
        INodeReactionRepository nodeTags,
        IReactionDefinitionRepository definitions)
    {
        return nodeTags
            .GetActiveForNode(node.Id)
            .Select(association => new
            {
                Association = association,
                Definition = definitions.GetById(association.ReactionDefinitionId)
            })
            .Where(item => item.Definition is not null && !item.Definition.IsSuppressed)
            .OrderBy(item => item.Association.CreatedAt)
            .ThenBy(item => item.Association.Id.Value)
            .Select(item => (item.Association, item.Definition!))
            .ToList();
    }

    /// <summary>Resolves active and historical associations for public history.</summary>
    public static IReadOnlyList<(NodeReaction Association, ReactionDefinition Definition)> ResolveAll(
        Node node,
        INodeReactionRepository nodeTags,
        IReactionDefinitionRepository definitions)
    {
        return nodeTags.GetAll()
            .Where(association => association.NodeId == node.Id)
            .Select(association => new
            {
                Association = association,
                Definition = definitions.GetById(association.ReactionDefinitionId)
            })
            .Where(item => item.Definition is not null)
            .OrderBy(item => item.Association.CreatedAt)
            .ThenBy(item => item.Association.Id.Value)
            .Select(item => (item.Association, item.Definition!))
            .ToList();
    }

    private static void WriteTagWithScore(
        (NodeReaction Association, ReactionDefinition Definition) item,
        IVoteRepository votes,
        ParticipantId? participantId)
    {
        var summary = new GetNodeReactionVoteSummary(votes).Execute(
            new NodeReactionVoteTarget(item.Association.Id.Value),
            participantId);
        var myVote = summary.CurrentParticipantVote switch
        {
            1 => ", my vote: up",
            -1 => ", my vote: down",
            _ => string.Empty
        };

        Console.WriteLine($"- {item.Definition.Emoji} {item.Definition.Text} " +
            $"(score: {summary.Score}{myVote})");
    }
}

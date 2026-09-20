using Atlas.Graph.Nodes;
using Atlas.Graph.Reactions;
using Atlas.Participants.Participants;
using Atlas.Voting;
using Atlas.Voting.Data;
using Atlas.Voting.Target;
using Atlas.Voting.Value;
using VotingParticipantId = Atlas.Voting.Votes.ParticipantId;

namespace Atlas.ConsoleApp;

/// <summary>Coordinates selection and voting for curated Node Reactions.</summary>
public static class ReactionCommands
{
    public static void Run(Node node, IReactionDefinitionRepository definitions,
        INodeReactionRepository reactions, IVoteRepository votes, CastVote castVote,
        Participant currentParticipant)
    {
        var service = new NodeReactionApplicationService(definitions, reactions);
        var managing = true;

        while (managing)
        {
            Console.Clear();
            Console.WriteLine($"REACTIONS TO {node.Title.Value.ToUpperInvariant()}");
            Console.WriteLine(new string('-', $"REACTIONS TO {node.Title.Value}".Length));
            Console.WriteLine($"Acting as: {currentParticipant.DisplayName}");
            Console.WriteLine();
            WriteApplied(node, reactions, definitions, votes,
                new VotingParticipantId(currentParticipant.Id.Value));
            Console.WriteLine();
            Console.WriteLine("1. Add a reaction");
            Console.WriteLine("2. Withdraw a reaction I added");
            Console.WriteLine("3. Upvote or downvote a reaction");
            Console.WriteLine("4. Return to node");
            Console.Write("Selection: ");

            try
            {
                switch (Console.ReadLine())
                {
                    case "1": Apply(node, service, definitions, reactions, currentParticipant); break;
                    case "2": Remove(node, service, reactions, definitions, currentParticipant); break;
                    case "3": Vote(node, reactions, definitions, currentParticipant, castVote); break;
                    case "4": managing = false; break;
                    default: ConsoleUi.Pause("That is not a valid selection."); break;
                }
            }
            catch (Exception exception) when (exception is ArgumentException or
                InvalidOperationException or UnauthorizedAccessException or
                KeyNotFoundException or IOException)
            {
                ConsoleUi.Pause($"Unable to change reactions: {exception.Message}");
            }
        }
    }

    private static void Apply(Node node, NodeReactionApplicationService service,
        IReactionDefinitionRepository definitions, INodeReactionRepository reactions,
        Participant participant)
    {
        var appliedIds = reactions.GetActiveForNode(node.Id)
            .Select(reaction => reaction.ReactionDefinitionId).ToHashSet();
        var available = definitions.GetAll()
            .Where(definition => !definition.IsSuppressed && !appliedIds.Contains(definition.Id))
            .OrderBy(definition => definition.Text).ToList();
        var selected = ReadDefinition(available, "Reaction to add");
        if (selected is null) return;

        service.Apply(node, selected.Text, participant.Id.Value,
            participant.IsActive, DateTimeOffset.UtcNow);
        ConsoleUi.Pause($"Added {selected.Emoji} {selected.Text}.");
    }

    private static void Remove(Node node, NodeReactionApplicationService service,
        INodeReactionRepository reactions, IReactionDefinitionRepository definitions,
        Participant participant)
    {
        var available = ReactionDisplay.ResolveActive(node, reactions, definitions)
            .Where(item => item.Association.AppliedByParticipantId == participant.Id.Value)
            .ToList();
        var selected = ReadAssociation(available, "Reaction to withdraw");
        if (selected is null) return;

        service.Remove(node, selected.Value.Association.Id, participant.Id.Value,
            participant.IsActive, actorIsModerator: false, DateTimeOffset.UtcNow);
        ConsoleUi.Pause($"Withdrew {selected.Value.Definition.Emoji} {selected.Value.Definition.Text}.");
    }

    private static void Vote(Node node, INodeReactionRepository reactions,
        IReactionDefinitionRepository definitions, Participant participant, CastVote castVote)
    {
        var selected = ReadAssociation(ReactionDisplay.ResolveActive(node, reactions, definitions),
            "Reaction to vote on");
        if (selected is null) return;

        Console.WriteLine("1. Upvote — this reaction resonates in context");
        Console.WriteLine("2. Downvote — this reaction does not resonate in context");
        Console.WriteLine("0. Cancel");
        Console.Write("Vote: ");
        var value = Console.ReadLine() switch
        {
            "1" => NodeReactionVote.Upvote,
            "2" => NodeReactionVote.Downvote,
            _ => (int?)null
        };
        if (value is null) return;

        castVote.Execute(new NodeReactionVoteTarget(selected.Value.Association.Id.Value),
            new VotingParticipantId(participant.Id.Value), value.Value);
        ConsoleUi.Pause("Your reaction vote was saved.");
    }

    private static ReactionDefinition? ReadDefinition(
        IReadOnlyList<ReactionDefinition> definitions, string prompt)
    {
        if (definitions.Count == 0)
        {
            ConsoleUi.Pause("All curated reactions are already on this node.");
            return null;
        }

        Console.WriteLine();
        for (var index = 0; index < definitions.Count; index++)
        {
            var item = definitions[index];
            Console.WriteLine($"{index + 1}. {item.Emoji} {item.Text} — {item.Description}");
        }
        Console.WriteLine("0. Cancel");
        Console.Write($"{prompt}: ");
        return int.TryParse(Console.ReadLine(), out var selection) &&
               selection > 0 && selection <= definitions.Count
            ? definitions[selection - 1] : null;
    }

    private static (NodeReaction Association, ReactionDefinition Definition)? ReadAssociation(
        IReadOnlyList<(NodeReaction Association, ReactionDefinition Definition)> reactions,
        string prompt)
    {
        if (reactions.Count == 0)
        {
            ConsoleUi.Pause("There are no reactions available for that action.");
            return null;
        }

        Console.WriteLine();
        for (var index = 0; index < reactions.Count; index++)
        {
            var item = reactions[index];
            Console.WriteLine($"{index + 1}. {item.Definition.Emoji} {item.Definition.Text}");
        }
        Console.WriteLine("0. Cancel");
        Console.Write($"{prompt}: ");
        return int.TryParse(Console.ReadLine(), out var selection) &&
               selection > 0 && selection <= reactions.Count
            ? reactions[selection - 1] : null;
    }

    private static void WriteApplied(Node node, INodeReactionRepository reactions,
        IReactionDefinitionRepository definitions, IVoteRepository votes,
        VotingParticipantId participantId)
    {
        var active = ReactionDisplay.ResolveActive(node, reactions, definitions)
            .Where(item => item.Association.Disposition is
                NodeReactionDisposition.Endorsed or NodeReactionDisposition.Community)
            .OrderBy(item => item.Association.Disposition == NodeReactionDisposition.Endorsed ? 0 : 1)
            .ThenBy(item => item.Definition.Text).ToList();
        if (active.Count == 0)
        {
            Console.WriteLine("No reactions yet.");
            return;
        }

        foreach (var group in active.GroupBy(item => item.Association.Disposition))
        {
            Console.WriteLine(group.Key == NodeReactionDisposition.Endorsed
                ? "AUTHOR REACTIONS" : "COMMUNITY REACTIONS");
            foreach (var item in group)
            {
                var summary = new GetNodeReactionVoteSummary(votes).Execute(
                    new NodeReactionVoteTarget(item.Association.Id.Value), participantId);
                var myVote = summary.CurrentParticipantVote switch
                {
                    NodeReactionVote.Upvote => "up",
                    NodeReactionVote.Downvote => "down",
                    _ => "—"
                };
                Console.WriteLine($"- {item.Definition.Emoji} {item.Definition.Text} " +
                    $"(score: {summary.Score}, my vote: {myVote})");
            }
        }
    }
}

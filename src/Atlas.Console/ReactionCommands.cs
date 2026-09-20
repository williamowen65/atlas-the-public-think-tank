using Atlas.Graph.Nodes;
using Atlas.Graph.Reactions;
using Atlas.Participants.Participants;
using Atlas.Voting;
using Atlas.Voting.Data;
using Atlas.Voting.Target;
using Atlas.Voting.Value;
using VotingParticipantId = Atlas.Voting.Votes.ParticipantId;

namespace Atlas.ConsoleApp;

/// <summary>Coordinates voting for the curated reactions available to every Node.</summary>
public static class ReactionCommands
{
    public static void Run(Node node, IReactionDefinitionRepository definitions,
        INodeReactionRepository reactions, IVoteRepository votes, CastVote castVote,
        Participant currentParticipant)
    {
        var service = new NodeReactionApplicationService(definitions, reactions);

        while (true)
        {
            Console.Clear();
            Console.WriteLine($"REACTIONS TO {node.Title.Value.ToUpperInvariant()}");
            Console.WriteLine(new string('-', $"REACTIONS TO {node.Title.Value}".Length));
            Console.WriteLine($"Acting as: {currentParticipant.DisplayName}");
            Console.WriteLine();

            var choices = BuildChoices(node, definitions, reactions, votes,
                new VotingParticipantId(currentParticipant.Id.Value));
            WriteChoices(choices);
            Console.WriteLine("0. Return to node");
            Console.Write("Reaction to vote on: ");

            if (!int.TryParse(Console.ReadLine(), out var selection) || selection < 0 ||
                selection > choices.Count)
            {
                ConsoleUi.Pause("That is not a valid reaction selection.");
                continue;
            }

            if (selection == 0) return;

            Console.WriteLine();
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
            if (value is null) continue;

            try
            {
                var choice = choices[selection - 1];
                var association = choice.Association ?? service.Apply(
                    node, choice.Definition.Text, currentParticipant.Id.Value,
                    currentParticipant.IsActive, DateTimeOffset.UtcNow);

                castVote.Execute(new NodeReactionVoteTarget(association.Id.Value),
                    new VotingParticipantId(currentParticipant.Id.Value), value.Value);
                ConsoleUi.Pause("Your reaction vote was saved.");
            }
            catch (Exception exception) when (exception is ArgumentException or
                InvalidOperationException or UnauthorizedAccessException or
                KeyNotFoundException or IOException)
            {
                ConsoleUi.Pause($"Unable to vote on the reaction: {exception.Message}");
            }
        }
    }

    private static IReadOnlyList<ReactionChoice> BuildChoices(Node node,
        IReactionDefinitionRepository definitions, INodeReactionRepository reactions,
        IVoteRepository votes, VotingParticipantId participantId)
    {
        var associations = reactions.GetActiveForNode(node.Id)
            .ToDictionary(item => item.ReactionDefinitionId);

        return definitions.GetAll()
            .Where(definition => !definition.IsSuppressed)
            .OrderBy(definition => definition.Text)
            .Select(definition =>
            {
                associations.TryGetValue(definition.Id, out var association);
                var summary = association is null
                    ? null
                    : new GetNodeReactionVoteSummary(votes).Execute(
                        new NodeReactionVoteTarget(association.Id.Value), participantId);
                return new ReactionChoice(definition, association,
                    summary?.Score ?? 0, summary?.TotalVotes ?? 0,
                    summary?.CurrentParticipantVote);
            })
            .ToList();
    }

    private static void WriteChoices(IReadOnlyList<ReactionChoice> choices)
    {
        for (var index = 0; index < choices.Count; index++)
        {
            var choice = choices[index];
            var myVote = choice.CurrentParticipantVote switch
            {
                NodeReactionVote.Upvote => "up",
                NodeReactionVote.Downvote => "down",
                _ => "—"
            };
            Console.WriteLine($"{index + 1}. {choice.Definition.Emoji} " +
                $"{choice.Definition.Text,-18} score: {choice.Score,3}  " +
                $"votes: {choice.TotalVotes,2}  my vote: {myVote}");
        }
        Console.WriteLine();
    }

    private sealed record ReactionChoice(ReactionDefinition Definition,
        NodeReaction? Association, int Score, int TotalVotes, int? CurrentParticipantVote);
}

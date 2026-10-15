using Atlas.Graph.Nodes;
using Atlas.Graph.Reactions;
using Atlas.Participants.Participants;
using Atlas.Voting;
using Atlas.Voting.Data;
using Atlas.Voting.Target;
using VotingParticipantId = Atlas.Voting.Votes.ParticipantId;

namespace Atlas.Application;

/// <summary>Coordinate reaction association and its vote under one operation boundary.</summary>
public sealed class ReactionVotingService(IReactionDefinitionRepository definitions, INodeReactionRepository reactions,
    CastVote castVote, IParticipantRepository participants)
{
    public void Vote(Node node, string reactionText, Guid actorId, int value) => OperationBoundary.Execute(reactions, () =>
    {
        var participant = participants.GetById(new ParticipantId(actorId))
            ?? throw new InvalidOperationException("The authenticated participant was not found.");
        var association = reactions.GetActiveForNode(node.Id).FirstOrDefault(item =>
            definitions.GetById(item.ReactionDefinitionId)?.Text == reactionText)
            ?? new NodeReactionApplicationService(definitions, reactions).Apply(node, reactionText, actorId, participant.IsActive, AtlasTime.UtcNow);
        castVote.Execute(new NodeReactionVoteTarget(association.Id.Value), new VotingParticipantId(actorId), value);
    });
    public static IReadOnlyList<ReactionChoice> BuildChoices(
        Node node,
        IReactionDefinitionRepository definitions,
        INodeReactionRepository reactions,
        IVoteRepository votes,
        VotingParticipantId participantId
    )
    {
        var associations = reactions.GetActiveForNode(node.Id)
            .ToDictionary(item => item.ReactionDefinitionId);

        return definitions.GetAll()
            .Where(definition => !definition.IsSuppressed)
            .OrderBy(definition => definition.Text)
            .Select(definition =>
            {
                associations.TryGetValue(
                    definition.Id,
                    out var association
                );
                var summary = association is null
                    ? null
                    : new GetNodeReactionVoteSummary(votes).Execute(
                        new NodeReactionVoteTarget(association.Id.Value),
                        participantId
                    );
                return new ReactionChoice(
                    definition,
                    association,
                    summary?.Score ?? 0,
                    summary?.TotalVotes ?? 0,
                    summary?.CurrentParticipantVote
                );
            })
            .ToList();
    }

    public sealed record ReactionChoice(
        ReactionDefinition Definition,
        NodeReaction? Association,
        int Score,
        int TotalVotes,
        int? CurrentParticipantVote
    );
}

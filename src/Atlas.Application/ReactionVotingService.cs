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
    /// <summary>Find or apply a node reaction, then create or change the participant's vote.</summary>
    /// <remarks>
    /// The SQL adapter shares this operation boundary with the nested Graph and Voting
    /// writes. A rejected vote therefore rolls back a newly created association.
    /// Atomicity depends on the repositories supporting a shared operation boundary.
    /// </remarks>
    /// <param name="node">The node receiving the reaction vote.</param>
    /// <param name="reactionText">The curated reaction text to resolve or apply.</param>
    /// <param name="actorId">The participant ID verified by the host.</param>
    /// <param name="value">The vote value accepted by Voting: +1 or -1.</param>
    public void Vote(Node node, string reactionText, Guid actorId, int value) => OperationBoundary.Execute(reactions, () =>
    {
        // Load current participant state before asking Graph to apply a new reaction.
        var participant = participants.GetById(new ParticipantId(actorId))
            ?? throw new InvalidOperationException("The authenticated participant was not found.");
        // Reuse the node's active association when present; otherwise Graph validates
        // and creates it. A reaction definition and a node association have different IDs.
        var association = reactions.GetActiveForNode(node.Id).FirstOrDefault(item =>
            definitions.GetById(item.ReactionDefinitionId)?.Text == reactionText)
            ?? new NodeReactionApplicationService(definitions, reactions).Apply(node, reactionText, actorId, participant.IsActive, AtlasTime.UtcNow);
        // Vote on the node association. Voting owns eligibility and vote-value checks.
        castVote.Execute(new NodeReactionVoteTarget(association.Id.Value), new VotingParticipantId(actorId), value);
    });
    /// <summary>Build the available reaction choices and their current vote summaries.</summary>
    /// <remarks>
    /// This is a read-only projection: listing a curated reaction does not apply it to
    /// the node. Unapplied choices have zero score/count and no current participant vote.
    /// Suppressed definitions are excluded and the remaining choices are sorted by text.
    /// </remarks>
    /// <param name="node">The node whose reaction choices are being displayed.</param>
    /// <param name="definitions">The curated reaction catalog.</param>
    /// <param name="reactions">The source of active node reaction associations.</param>
    /// <param name="votes">The source of vote totals and participant votes.</param>
    /// <param name="participantId">The participant whose current vote should be included.</param>
    /// <returns>Host-independent choices for presentation by any host.</returns>
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
                // Query votes only when this definition has been applied to the node.
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

    /// <summary>A curated reaction together with its optional node association and vote totals.</summary>
    /// <param name="Definition">The curated reaction available for selection.</param>
    /// <param name="Association">Null when the reaction has not been applied to this node.</param>
    /// <param name="Score">The net score; zero for an unapplied reaction.</param>
    /// <param name="TotalVotes">The number of current votes; zero for an unapplied reaction.</param>
    /// <param name="CurrentParticipantVote">The participant's vote, or null when none exists.</param>
    public sealed record ReactionChoice(
        ReactionDefinition Definition,
        NodeReaction? Association,
        int Score,
        int TotalVotes,
        int? CurrentParticipantVote
    );
}

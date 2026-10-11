using Atlas.Persistence;
using Atlas.Voting;
using Atlas.Voting.Data;
using Atlas.Voting.Target;
using Atlas.Voting.Votes;

namespace Atlas.ConsoleApp.Storage;

/// <summary>Maps domain objects directly to EF Core rows in SQL Server.</summary>
public sealed class SqlVoteRepository(Func<AtlasDataContext>? contextFactory = null) : SqlRepository(contextFactory), IVoteRepository
{

    // Validate references and save the vote within one SQL operation.
    public void Save(Vote vote) => Execute(() =>
    {
        ArgumentNullException.ThrowIfNull(vote);

        // The voter and supported target must be available before the vote is written.
        ReferenceValidation.Require(
            this,
            "Participant",
            vote.ParticipantId.Id
        );
        ReferenceValidation.Require(
            this,
            TargetKind(vote.Target),
            vote.Target.Id
        );

        // Persist the typed target, rating and timestamps; an outer workflow still owns its commit.
        SaveRow(ToStorage(vote));
        return true;
    });

    // Delete only this vote ID. Single-ID reads return null when the vote is absent.
    public void Delete(VoteId id) => DeleteRow<VoteRow>(id.Value);
    public Vote? GetById(VoteId id) => FindRow<VoteRow>(id.Value) is { } row ? ToDomain(row) : null;

    // Translate the target subtype into the stored discriminator; reject unsupported target types.
    private static string TargetKind(VoteTarget target) => target switch
    {
        NodeVoteTarget => "Node", NodeReactionVoteTarget => "NodeReaction",
        _ => throw new InvalidOperationException("Unsupported vote target type.")
    };

    // Match both target kind and ID, so different target types cannot share a vote set accidentally.
    public IReadOnlyCollection<Vote> GetTargetVotes(VoteTarget target)
    {
        var kind = TargetKind(target);
        return QueryRows<VoteRow>(row => row.TargetId == target.Id && row.TargetType == kind).Select(ToDomain).ToList();
    }

    // Find the single vote belonging to this participant and this typed target.
    public Vote? GetByParticipantAndTarget(
        ParticipantId participantId,
        VoteTarget target
    )
    {
        var kind = TargetKind(target);
        return QueryRows<VoteRow>(row => row.ParticipantId == participantId.Id && row.TargetId == target.Id && row.TargetType == kind)
            .Select(ToDomain).SingleOrDefault();
    }


        // Store the target discriminator with its ID so reads can reconstruct the correct subtype.
        private static VoteRow ToStorage(Vote vote)
        {
            return new VoteRow
            {
                Id = vote.Id.Value,
                ParticipantId = vote.ParticipantId.Id,
                TargetId = vote.Target.Id,
                TargetType = vote.Target switch
                {
                    NodeVoteTarget => "Node",
                    NodeReactionVoteTarget => "NodeReaction",

                    _ => throw new InvalidOperationException(
                        "Unsupported vote target type.")
                },
                Value = vote.Value.Value,
                CreatedAt = vote.CreatedAt,
                UpdatedAt = vote.UpdatedAt
            };
        }

        private static Vote ToDomain(VoteRow storedVote)
        {

            // Decode the stored target type first; unknown kinds are invalid persisted data.
            VoteTarget target = storedVote.TargetType switch
            {
                "Node" => new NodeVoteTarget(
                    storedVote.TargetId),
                "NodeReaction" => new NodeReactionVoteTarget(
                    storedVote.TargetId),

                _ => throw new InvalidDataException(
                    $"Unsupported stored vote target type " +
                    $"'{storedVote.TargetType}'.")
            };


            // Reconstitute the vote with its original identity and timestamps, applying domain validation.
            return Vote.Reconstitute(
                new VoteId(storedVote.Id),
                target,
                new ParticipantId(storedVote.ParticipantId),
                storedVote.Value,
                storedVote.CreatedAt,
                storedVote.UpdatedAt
            );
        }
}

using Atlas.Persistence;
using Atlas.Voting;
using Atlas.Voting.Data;
using Atlas.Voting.Target;
using Atlas.Voting.Votes;

namespace Atlas.Application.Storage;

/// <summary>Maps domain objects directly to EF Core rows in SQL Server.</summary>
public sealed class SqlVoteRepository(Func<AtlasDataContext>? contextFactory = null, VoteActionQuotaPolicy? quotaPolicy = null)
    : SqlRepository(contextFactory), IVoteRepository, IVoteActionQuota
{

    private readonly VoteActionQuotaPolicy _quotaPolicy = quotaPolicy ?? VoteActionQuotaPolicy.Default;

    /// <summary>Serializes this participant's check-and-write across processes, inside the shared SQL transaction.</summary>
    public T ExecuteVoteAction<T>(ParticipantId participantId, Func<T> operation)
    {
        ArgumentNullException.ThrowIfNull(participantId);
        return ExecuteWithLock(operation, $"AtlasVoteActions:{participantId.Id:N}");
    }

    /// <summary>Read-only estimate; only the atomic mutation can reserve a slot.</summary>
    public VoteActionAllowance GetActionAllowance(ParticipantId participantId)
    {
        ArgumentNullException.ThrowIfNull(participantId);
        var now = AtlasTime.UtcNow;
        var history = QueryRows<VoteActionQuotaRow>(
            row => row.ParticipantId == participantId.Id && row.OccurredAt > now - _quotaPolicy.Window,
            query => query.OrderByDescending(row => row.OccurredAt).Take(_quotaPolicy.Limit));
        return _quotaPolicy.Evaluate(history.Select(row => row.OccurredAt), now);
    }

    public void EnsureActionAllowed(ParticipantId participantId)
    {
        var allowance = GetActionAllowance(participantId);
        if (allowance.Remaining == 0) throw new VoteActionQuotaExceededException(allowance);
    }

    private void RecordAction(ParticipantId participantId) => SaveRow(new VoteActionQuotaRow
    {
        Id = Guid.NewGuid(), ParticipantId = participantId.Id, OccurredAt = AtlasTime.UtcNow
    });

    // Validate references and save the vote within one SQL operation.
    public void Save(Vote vote)
    {
        ArgumentNullException.ThrowIfNull(vote);
        ExecuteVoteAction(vote.ParticipantId, () =>
        {

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
            EnsureActionAllowed(vote.ParticipantId);
            SaveRow(ToStorage(vote));
            RecordAction(vote.ParticipantId);
            return true;
        });
    }

    // Delete only this vote ID. Single-ID reads return null when the vote is absent.
    public void Delete(VoteId id)
    {
        var found = FindRow<VoteRow>(id.Value);
        if (found is null) return;
        var participantId = new ParticipantId(found.ParticipantId);
        ExecuteVoteAction(participantId, () =>
        {
            // A competing undo may have removed it before this participant lock was acquired.
            if (FindRow<VoteRow>(id.Value) is null) return false;
            EnsureActionAllowed(participantId);
            DeleteRow<VoteRow>(id.Value);
            RecordAction(participantId);
            return true;
        });
    }
    public Vote? GetById(VoteId id) => FindRow<VoteRow>(id.Value) is { } row ? ToDomain(row) : null;

    // Translate the target subtype into the stored discriminator; reject unsupported target types.
    private static string TargetKind(VoteTarget target) => target switch
    {
        NodeVoteTarget => "Node", NodeReactionVoteTarget => "NodeReaction", CommentVoteTarget => "Comment",
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
                    CommentVoteTarget => "Comment",

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
                "Comment" => new CommentVoteTarget(storedVote.TargetId),
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

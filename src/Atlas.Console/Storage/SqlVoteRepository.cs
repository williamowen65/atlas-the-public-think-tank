using Atlas.Persistence;
using Atlas.Voting;
using Atlas.Voting.Data;
using Atlas.Voting.Target;
using Atlas.Voting.Votes;

namespace Atlas.ConsoleApp.Storage;

/// <summary>Maps domain objects directly to EF Core rows in SQL Server.</summary>
public sealed class SqlVoteRepository(Func<AtlasDataContext>? contextFactory = null) : SqlRepository(contextFactory), IVoteRepository
{
    public void Save(Vote vote) { ArgumentNullException.ThrowIfNull(vote); SaveRow(ToStorage(vote)); }
    public void Delete(VoteId id) => DeleteRow<VoteRow>(id.Value);
    public Vote? GetById(VoteId id) => FindRow<VoteRow>(id.Value) is { } row ? ToDomain(row) : null;
    private static string TargetKind(VoteTarget target) => target switch
    {
        NodeVoteTarget => "Node", NodeReactionVoteTarget => "NodeReaction",
        _ => throw new InvalidOperationException("Unsupported vote target type.")
    };
    public IReadOnlyCollection<Vote> GetTargetVotes(VoteTarget target)
    {
        var kind = TargetKind(target);
        return QueryRows<VoteRow>(row => row.TargetId == target.Id && row.TargetType == kind).Select(ToDomain).ToList();
    }
    public Vote? GetByParticipantAndTarget(ParticipantId participantId, VoteTarget target)
    {
        var kind = TargetKind(target);
        return QueryRows<VoteRow>(row => row.ParticipantId == participantId.Id && row.TargetId == target.Id && row.TargetType == kind)
            .Select(ToDomain).SingleOrDefault();
    }

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

            return Vote.Reconstitute(
                new VoteId(storedVote.Id),
                target,
                new ParticipantId(storedVote.ParticipantId),
                storedVote.Value,
                storedVote.CreatedAt,
                storedVote.UpdatedAt);
        }
}

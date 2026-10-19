using Atlas.Contracts.Observability;
using Atlas.Contracts.Operations;
using Atlas.Voting.Votes;

namespace Atlas.Voting;

/// <summary>Voting-owned rolling allowance shared by ratings and reaction vote actions.</summary>
public sealed record VoteActionQuotaPolicy
{
    /// <summary>Provisional MVP pacing: 60 successful actions per rolling ten minutes.</summary>
    public static VoteActionQuotaPolicy Default { get; } = new(60, TimeSpan.FromMinutes(10));
    public int Limit { get; }
    public TimeSpan Window { get; }
    public VoteActionQuotaPolicy(int limit, TimeSpan window)
    {
        if (limit < 1 || limit > 10_000) throw new ArgumentOutOfRangeException(nameof(limit));
        if (window <= TimeSpan.Zero || window > TimeSpan.FromDays(7)) throw new ArgumentOutOfRangeException(nameof(window));
        Limit = limit;
        Window = window;
    }
    /// <summary>Exact cutoff entries have expired; tightening uses the Limit-th newest action.</summary>
    public VoteActionAllowance Evaluate(IEnumerable<DateTimeOffset> history, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(history);
        now = now.ToUniversalTime();
        var recent = history.Where(at => at > now - Window).OrderByDescending(at => at).Take(Limit).ToArray();
        return new(Limit, Window, Math.Max(0, Limit - recent.Length), now,
            recent.Length == 0 ? null : recent[^1] + Window);
    }
}

/// <summary>Advisory display/retry information; the mutation rechecks inside its transaction.</summary>
public sealed record VoteActionAllowance(int Limit, TimeSpan Window, int Remaining,
    DateTimeOffset CheckedAt, DateTimeOffset? NextSlotAt)
{
    public TimeSpan RetryAfter => Remaining > 0 || NextSlotAt is null ? TimeSpan.Zero : NextSlotAt.Value - CheckedAt;
}

/// <summary>A temporary voting policy denial, not a storage outage or a refund of an earlier action.</summary>
public sealed class VoteActionQuotaExceededException(VoteActionAllowance allowance)
    : OperationRejectedException($"Voting action limit reached: {allowance.Limit} actions per {allowance.Window.TotalMinutes:g} minutes. " +
        $"Try again at {allowance.NextSlotAt:yyyy-MM-dd HH:mm:ss} UTC.")
{
    public VoteActionAllowance Allowance { get; } = allowance;
    public override EventReason ReasonCode => EventReason.QuotaExceeded;
}

/// <summary>Adapter capability for durable allowance reads and participant-scoped transaction coordination.</summary>
/// <remarks>Save/Delete consume a slot atomically with their write. Coordination alone never consumes one.</remarks>
public interface IVoteActionQuota
{
    VoteActionAllowance GetActionAllowance(ParticipantId participantId);
    void EnsureActionAllowed(ParticipantId participantId);
    T ExecuteVoteAction<T>(ParticipantId participantId, Func<T> operation);
}

/// <summary>Keeps policy checks and vote mutation inside an adapter's participant-coordinated boundary.</summary>
public static class VoteActionBoundary
{
    public static T Execute<T>(object repository, ParticipantId participantId, Func<T> operation)
    {
        ArgumentNullException.ThrowIfNull(participantId);
        return OperationBoundary.Execute(repository, () => repository is IVoteActionQuota quota
            ? quota.ExecuteVoteAction(participantId, operation) : operation());
    }
    public static void EnsureAllowed(object repository, ParticipantId participantId)
    {
        if (repository is IVoteActionQuota quota) quota.EnsureActionAllowed(participantId);
    }
}

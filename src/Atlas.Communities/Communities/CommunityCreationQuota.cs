using Atlas.Contracts.Observability;

namespace Atlas.Communities.Communities;

/// <summary>Product policy: one community per participant per rolling 24 hours by default.</summary>
public sealed record CommunityCreationQuotaPolicy
{
    public static CommunityCreationQuotaPolicy Default { get; } = new(
        1,
        TimeSpan.FromHours(24)
    );
    public int Limit { get; }
    public TimeSpan Window { get; }

    public CommunityCreationQuotaPolicy(
        int limit,
        TimeSpan window
    )
    {
        if (limit < 1 || limit > 1000) throw new ArgumentOutOfRangeException(nameof(limit));
        if (window <= TimeSpan.Zero || window > TimeSpan.FromDays(7)) throw new ArgumentOutOfRangeException(nameof(window));
        Limit = limit;
        Window = window;
    }

    /// <summary>Evaluate bounded recent history. Exact cutoff timestamps have expired.</summary>
    public CommunityCreationAllowance Evaluate(
        IEnumerable<DateTimeOffset> history,
        DateTimeOffset now
    )
    {
        ArgumentNullException.ThrowIfNull(history);
        now = now.ToUniversalTime();
        var recent = history.Where(at => at > now - Window).OrderByDescending(at => at).Take(Limit).ToArray();
        // When a policy is tightened, use the Limit-th newest creation, not the oldest
        // historical record: enough entries must expire before another creation is allowed.
        var next = recent.Length == 0 ? (DateTimeOffset?)null : recent[^1] + Window;
        return new CommunityCreationAllowance(
            Limit,
            Window,
            Math.Max(
                0,
                Limit - recent.Length
            ),
            now,
            next
        );
    }
}

/// <summary>Read-only allowance data that Console and a future API can present.</summary>
public sealed record CommunityCreationAllowance(
    int Limit,
    TimeSpan Window,
    int Remaining,
    DateTimeOffset CheckedAt,
    DateTimeOffset? NextSlotAt
)
{
    public TimeSpan RetryAfter => Remaining > 0 || NextSlotAt is null ? TimeSpan.Zero : NextSlotAt.Value - CheckedAt;
}

/// <summary>Persistence capability; creation checks and consumption run under participant coordination.</summary>
public interface ICommunityCreationQuota
{
    CommunityCreationAllowance GetCreationAllowance(Guid participantId);
    void EnsureCreationAllowed(Guid participantId);
    T ExecuteCreation<T>(Guid participantId, Func<T> operation);
}

/// <summary>Expected product rejection with structured data for future HTTP/UI handling.</summary>
public sealed class CommunityCreationQuotaExceededException(CommunityCreationAllowance allowance)
    : OperationRejectedException($"Community creation limit reached: {allowance.Limit} per {allowance.Window.TotalHours:g} hours. " +
        $"Try again at {allowance.NextSlotAt:yyyy-MM-dd HH:mm:ss} UTC.")
{
    public CommunityCreationAllowance Allowance { get; } = allowance;
    public override EventReason ReasonCode => EventReason.QuotaExceeded;
}

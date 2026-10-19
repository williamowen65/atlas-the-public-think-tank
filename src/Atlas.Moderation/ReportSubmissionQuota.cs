using Atlas.Contracts.Observability;

namespace Atlas.Moderation;

/// <summary>Product policy: three reports per participant per rolling hour by default.</summary>
public sealed record ReportSubmissionQuotaPolicy
{
    public static ReportSubmissionQuotaPolicy Default { get; } = new(
        3,
        TimeSpan.FromHours(1)
    );
    public int Limit { get; }
    public TimeSpan Window { get; }

    public ReportSubmissionQuotaPolicy(
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
    public ReportSubmissionAllowance Evaluate(
        IEnumerable<DateTimeOffset> history,
        DateTimeOffset now
    )
    {
        ArgumentNullException.ThrowIfNull(history);
        now = now.ToUniversalTime();
        var recent = history.Where(at => at > now - Window).OrderByDescending(at => at).Take(Limit).ToArray();
        // When a policy is tightened, use the Limit-th newest report submission, not the oldest
        // historical record: enough entries must expire before another report submission is allowed.
        var next = recent.Length == 0 ? (DateTimeOffset?)null : recent[^1] + Window;
        return new ReportSubmissionAllowance(
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
public sealed record ReportSubmissionAllowance(
    int Limit,
    TimeSpan Window,
    int Remaining,
    DateTimeOffset CheckedAt,
    DateTimeOffset? NextSlotAt
)
{
    public TimeSpan RetryAfter => Remaining > 0 || NextSlotAt is null ? TimeSpan.Zero : NextSlotAt.Value - CheckedAt;
}

/// <summary>Persistence capability; report submission checks and consumption run under participant coordination.</summary>
public interface IReportSubmissionQuota
{
    ReportSubmissionAllowance GetSubmissionAllowance(Guid participantId);
    void EnsureSubmissionAllowed(Guid participantId);
    T ExecuteSubmission<T>(Guid participantId, Func<T> operation);
}

/// <summary>Expected product rejection with structured data for future HTTP/UI handling.</summary>
public sealed class ReportSubmissionQuotaExceededException(ReportSubmissionAllowance allowance)
    : OperationRejectedException($"Report limit reached: {allowance.Limit} reports in {allowance.Window.TotalMinutes:g} minutes. " +
        $"Try again at {allowance.NextSlotAt:yyyy-MM-dd HH:mm:ss} UTC. " +
        "For additional legitimate concerns about nodes, contact support staff.")
{
    public ReportSubmissionAllowance Allowance { get; } = allowance;
    public override EventReason ReasonCode => EventReason.QuotaExceeded;
}

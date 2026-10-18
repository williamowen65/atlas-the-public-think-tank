using Atlas.Contracts.Observability;

namespace Atlas.Graph.Nodes;

/// <summary>Product policy; defaults are provisional pending PTT-133 review.</summary>
public sealed record NodeCreationQuotaPolicy
{
    public static NodeCreationQuotaPolicy Default { get; } = new(
        3,
        TimeSpan.FromHours(2)
    );
    public int Limit { get; }
    public TimeSpan Window { get; }

    public NodeCreationQuotaPolicy(
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
    public NodeCreationAllowance Evaluate(
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
        return new NodeCreationAllowance(
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
public sealed record NodeCreationAllowance(
    int Limit,
    TimeSpan Window,
    int Remaining,
    DateTimeOffset CheckedAt,
    DateTimeOffset? NextSlotAt
)
{
    public TimeSpan RetryAfter => Remaining > 0 || NextSlotAt is null ? TimeSpan.Zero : NextSlotAt.Value - CheckedAt;
}

/// <summary>Persistence capability; creation checks must run inside topology coordination.</summary>
public interface INodeCreationQuota
{
    NodeCreationAllowance GetCreationAllowance(Guid participantId);
    void EnsureCreationAllowed(Guid participantId);
}

/// <summary>Expected product rejection with structured data for future HTTP/UI handling.</summary>
public sealed class NodeCreationQuotaExceededException(NodeCreationAllowance allowance)
    : OperationRejectedException($"Node creation limit reached: {allowance.Limit} Nodes per {allowance.Window.TotalHours:g} hours. " +
        $"Try again in {Math.Max(
            1,
            Math.Ceiling(allowance.RetryAfter.TotalMinutes)
        ):g} minute(s), " +
        $"at {allowance.NextSlotAt:yyyy-MM-dd HH:mm:ss} UTC.")
{
    public NodeCreationAllowance Allowance { get; } = allowance;
    public override EventReason ReasonCode => EventReason.QuotaExceeded;
}

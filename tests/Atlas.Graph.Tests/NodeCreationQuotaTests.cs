using Atlas.Contracts.Observability;
using Atlas.Contracts.Operations;
using Atlas.Graph.Nodes;

namespace Atlas.Graph.Tests;

[TestClass]
public sealed class NodeCreationQuotaTests
{
    private static readonly DateTimeOffset Now = new(
        2026,
        10,
        12,
        15,
        0,
        0,
        TimeSpan.Zero
    );

    /// <summary>A quota denial is a bounded, correlated policy signal, never a storage outage or a success.</summary>
    [TestMethod]
    public void ExhaustionIsReportedOnceAsDeniedWithQuotaReason()
    {
        var observer = new RecordingObserver();
        using var observe = Telemetry.UseObserver(observer);
        using var operation = Telemetry.BeginOperation(Guid.NewGuid());
        var allowance = NodeCreationQuotaPolicy.Default.Evaluate([Now, Now, Now], Now);
        var error = new NodeCreationQuotaExceededException(allowance);
        Assert.ThrowsExactly<NodeCreationQuotaExceededException>(() =>
            OperationBoundary.Execute(new object(), () => throw error));
        // An outer handler sees the same exception; it must not double-count the denial.
        Telemetry.Failure(EventKind.NodeChanged, error);
        var recorded = observer.Events.Single();
        Assert.AreEqual(EventOutcome.Denied, recorded.Outcome);
        Assert.AreEqual(EventReason.QuotaExceeded, recorded.ReasonCode);
        Assert.AreEqual(Telemetry.OperationId, recorded.ObservabilityOperationId);
        Assert.AreSame(allowance, error.Allowance);
    }

    private sealed class RecordingObserver : ITelemetryObserver
    {
        public List<TelemetryEvent> Events { get; } = [];
        public void Record(TelemetryEvent value) => Events.Add(value);
    }

    [TestMethod]
    public void FinalSlotAndExactExpiryUseRollingWindow()
    {
        var policy = new NodeCreationQuotaPolicy(
            3,
            TimeSpan.FromHours(2)
        );
        var history = new[] {
            Now.AddHours(-1),
            Now.AddMinutes(-30)
        };
        Assert.AreEqual(
            1,
            policy.Evaluate(
                history,
                Now
            ).Remaining
        );

        // Three committed entries exhaust allowance. The oldest returns one slot,
        // exactly two hours after that creation, not at a calendar-hour reset.
        history = [.. history, Now];
        var full = policy.Evaluate(
            history,
            Now
        );
        Assert.AreEqual(
            0,
            full.Remaining
        );
        Assert.AreEqual(
            Now.AddHours(1),
            full.NextSlotAt
        );
        Assert.AreEqual(
            TimeSpan.FromHours(1),
            full.RetryAfter
        );
        Assert.AreEqual(
            1,
            policy.Evaluate(
                history,
                Now.AddHours(1)
            ).Remaining
        );
    }

    [TestMethod]
    public void TightenedPolicyWaitsUntilEnoughHistoryExpires()
    {
        var policy = new NodeCreationQuotaPolicy(
            2,
            TimeSpan.FromHours(2)
        );
        var result = policy.Evaluate(
            [Now.AddMinutes(-90), Now.AddMinutes(-60), Now.AddMinutes(-30)],
            Now
        );
        // Expiring the oldest of three is insufficient: the second-newest must expire.
        Assert.AreEqual(
            Now.AddHours(1),
            result.NextSlotAt
        );
        Assert.AreEqual(
            0,
            result.Remaining
        );
    }

    [TestMethod]
    public void RejectionCarriesAllowanceAndUsefulWaitMessage()
    {
        var allowance = NodeCreationQuotaPolicy.Default.Evaluate(
            [Now, Now, Now],
            Now
        );
        var error = new NodeCreationQuotaExceededException(allowance);
        Assert.AreSame(
            allowance,
            error.Allowance
        );
        StringAssert.Contains(
            error.Message,
            "3 Nodes per 2 hours"
        );
        StringAssert.Contains(
            error.Message,
            "120 minute(s)"
        );
        StringAssert.Contains(
            error.Message,
            "UTC"
        );
    }
}

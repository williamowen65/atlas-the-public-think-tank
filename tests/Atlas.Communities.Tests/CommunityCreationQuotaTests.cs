using Atlas.Communities.Communities;
using Atlas.Contracts.Observability;

namespace Atlas.Communities.Tests;

[TestClass]
public sealed class CommunityCreationQuotaTests
{
    private static readonly DateTimeOffset At = DateTimeOffset.Parse("2026-10-19T12:00:00Z");

    [TestMethod]
    public void OneCreationPerRollingDayExpiresExactlyAtCutoff()
    {
        var policy = CommunityCreationQuotaPolicy.Default;
        var full = policy.Evaluate([At], At.AddHours(23));
        Assert.AreEqual(0, full.Remaining);
        Assert.AreEqual(At.AddHours(24), full.NextSlotAt);
        Assert.AreEqual(TimeSpan.FromHours(1), full.RetryAfter);
        Assert.AreEqual(1, policy.Evaluate([At], At.AddHours(24)).Remaining);
    }

    [TestMethod]
    public void ExhaustionExplainsLimitAndWhenCreationCanResume()
    {
        var allowance = CommunityCreationQuotaPolicy.Default.Evaluate([At], At);
        var error = new CommunityCreationQuotaExceededException(allowance);
        Assert.AreEqual("Community creation limit reached: 1 per 24 hours. " +
            "Try again at 2026-10-20 12:00:00 UTC.", error.Message);
        Assert.AreEqual(EventReason.QuotaExceeded, Telemetry.Reason(error));
        Console.WriteLine(error.Message);
    }
}

using Atlas.Contracts.Observability;
using Atlas.Contracts.Operations;

namespace Atlas.Moderation.Tests;

[TestClass]
public sealed class ReportSubmissionQuotaTests
{
    private static readonly DateTimeOffset At = DateTimeOffset.Parse("2026-10-19T12:00:00Z");

    [TestMethod]
    public void ThirdReportIsAllowedAndFourthWaitsUntilOldestSubmissionExpires()
    {
        var policy = ReportSubmissionQuotaPolicy.Default;
        Assert.AreEqual(1, policy.Evaluate([At, At.AddMinutes(10)], At.AddMinutes(20)).Remaining);
        DateTimeOffset[] history = [At, At.AddMinutes(10), At.AddMinutes(20)];
        var full = policy.Evaluate(history, At.AddMinutes(30));
        Assert.AreEqual(0, full.Remaining);
        Assert.AreEqual(At.AddHours(1), full.NextSlotAt);
        Assert.AreEqual(TimeSpan.FromMinutes(30), full.RetryAfter);
        Assert.AreEqual(0, policy.Evaluate(history, At.AddHours(1).AddTicks(-1)).Remaining);
        Assert.AreEqual(1, policy.Evaluate(history, At.AddHours(1)).Remaining);
    }

    [TestMethod]
    public void MessageExplainsLimitRetryAndSupportForAdditionalLegitimateConcerns()
    {
        var allowance = ReportSubmissionQuotaPolicy.Default.Evaluate([At, At, At], At);
        var error = new ReportSubmissionQuotaExceededException(allowance);
        Assert.AreEqual("Report limit reached: 3 reports in 60 minutes. " +
            "Try again at 2026-10-19 13:00:00 UTC. " +
            "For additional legitimate concerns about nodes, contact support staff.", error.Message);
        Assert.AreEqual(EventReason.QuotaExceeded, Telemetry.Reason(error));
        Console.WriteLine(error.Message);
    }

    [TestMethod]
    public void ReportWorkflowAcceptsThreeAndDoesNotSaveTheRejectedFourth()
    {
        using var clock = AtlasTime.Use(new Clock(At));
        var cases = new QuotaCases();
        var reporter = Guid.NewGuid();
        var service = new ModerationService(cases, new NoModerator());
        for (var index = 0; index < 3; index++)
            service.ReportNode(Guid.NewGuid(), reporter, "A node", "spam", null, At);
        var denied = Assert.ThrowsExactly<ReportSubmissionQuotaExceededException>(() =>
            service.ReportNode(Guid.NewGuid(), reporter, "A node", "spam", null, At));
        Assert.AreEqual(0, denied.Allowance.Remaining);
        Assert.HasCount(3, cases.GetAll());
        Assert.AreEqual(3, cases.SaveCalls);
    }

    [TestMethod]
    public void WindowCanBeChangedWithoutChangingTheSubmissionWorkflow()
    {
        var policy = new ReportSubmissionQuotaPolicy(3, TimeSpan.FromHours(5));
        Assert.AreEqual(0, policy.Evaluate([At, At, At], At.AddHours(1)).Remaining);
        Assert.AreEqual(3, policy.Evaluate([At, At, At], At.AddHours(5)).Remaining);
    }
    private sealed class Clock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
    private sealed class NoModerator : IModeratorAuthorization
    {
        public bool IsAtlasModerator(Guid participantId) => false;
    }
    // Single-threaded adapter checks service orchestration; SQL tests cover durable coordination.
    private sealed class QuotaCases : IModerationCaseRepository, IReportSubmissionQuota, IReferenceLookup
    {
        private readonly Dictionary<Guid, ModerationCase> _items = [];
        private readonly List<(Guid Reporter, DateTimeOffset At)> _history = [];
        public int SaveCalls { get; private set; }
        public ModerationCase? GetById(Guid id) => _items.GetValueOrDefault(id);
        public IReadOnlyCollection<ModerationCase> GetAll() => _items.Values.ToList();
        public bool IsAvailable(string kind, Guid id, bool requireActive) => id != Guid.Empty;
        public ReportSubmissionAllowance GetSubmissionAllowance(Guid id) =>
            ReportSubmissionQuotaPolicy.Default.Evaluate(_history.Where(item => item.Reporter == id).Select(item => item.At), AtlasTime.UtcNow);
        public void EnsureSubmissionAllowed(Guid id)
        {
            var allowance = GetSubmissionAllowance(id);
            if (allowance.Remaining == 0) throw new ReportSubmissionQuotaExceededException(allowance);
        }
        public T ExecuteSubmission<T>(Guid id, Func<T> operation) => operation();
        public void Save(ModerationCase item)
        {
            SaveCalls++;
            if (!_items.ContainsKey(item.Id)) _history.Add((item.ReporterId, AtlasTime.UtcNow));
            _items[item.Id] = item;
        }
    }

}

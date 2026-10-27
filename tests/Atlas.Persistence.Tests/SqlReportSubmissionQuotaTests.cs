using Atlas.Application.Storage;
using Atlas.Contracts.Operations;
using Atlas.Moderation;

namespace Atlas.Persistence.Tests;

/// <summary>Fresh-model evidence; developer-generated migration/upgrade tests remain required.</summary>
[TestClass]
public sealed class SqlReportSubmissionQuotaTests
{
    [TestMethod]
    public void FourthReportIsRejectedWithoutQueueOrAuditGrowthAndModeratorCanStillDecide()
    {
        using var database = SqlTestDatabase.Create(useCurrentModel: true, installFullText: false);
        var at = DateTimeOffset.Parse("2026-10-19T12:00:00Z");
        using var clock = AtlasTime.Use(new Clock(at));
        var reporter = database.AddParticipant();
        var other = database.AddParticipant();
        var moderator = database.AddParticipant();
        var nodes = Enumerable.Range(0, 4).Select(_ => database.AddNode()).ToArray();
        var repo = new SqlModerationCaseRepository(database.Open);
        var service = new ModerationService(repo, new Moderator(moderator));
        foreach (var node in nodes.Take(3))
            service.ReportNode(node, reporter, "private-canary-title", "spam", "private-canary-evidence", at.AddYears(-1));
        int audits, tickets;
        using (var check = database.Open())
        {
            audits = check.AuditRecords.Count();
            tickets = check.AuditOutbox.Count();
        }
        var reopened = new SqlModerationCaseRepository(database.Open);
        var denied = Assert.ThrowsExactly<ReportSubmissionQuotaExceededException>(() =>
            new ModerationService(reopened, new Moderator(moderator))
                .ReportNode(nodes[3], reporter, "A node", "spam", null, at.AddYears(-1)));
        Assert.AreEqual("Report limit reached: 3 reports in 60 minutes. " +
            "Try again at 2026-10-19 13:00:00 UTC. " +
            "For additional legitimate concerns about nodes, contact support staff.", denied.Message);
        Console.WriteLine(denied.Message);
        Assert.HasCount(3, service.Queue(moderator));
        Assert.AreEqual(0, reopened.GetSubmissionAllowance(reporter).Remaining);
        Assert.AreEqual(3, reopened.GetSubmissionAllowance(other).Remaining);
        using (var check = database.Open())
        {
            Assert.AreEqual(3, check.ModerationCaseRows.Count());
            Assert.AreEqual(3, check.ReportSubmissionQuotaRows.Count());
            Assert.AreEqual(audits, check.AuditRecords.Count());
            Assert.AreEqual(tickets, check.AuditOutbox.Count());
            Assert.IsTrue(check.ReportSubmissionQuotaRows.All(row => row.OccurredAt == at));
            foreach (var record in check.AuditRecords.ToList()) Assert.DoesNotContain("canary", record.PayloadJson);
        }
        // Decisions do not consume/report/refund submission slots, even when the reporter is exhausted.
        service.DecideNode(moderator, nodes[0], ModerationDecision.Dismiss, "Reviewed", at);
        Assert.AreEqual(0, repo.GetSubmissionAllowance(reporter).Remaining);
        service.ReportNode(nodes[3], other, "A node", "spam", null, at);
        using var later = AtlasTime.Use(new Clock(at.AddHours(1)));
        service.ReportNode(nodes[3], reporter, "A node", "spam", null, at.AddHours(1));
        Assert.AreEqual(2, repo.GetSubmissionAllowance(reporter).Remaining);
    }

    [TestMethod]
    public void InvalidAndRolledBackReportsConsumeNothingAndDirectSavesCannotBypassQuota()
    {
        using var database = SqlTestDatabase.Create(useCurrentModel: true, installFullText: false);
        var reporter = database.AddParticipant();
        var node = database.AddNode();
        var repo = new SqlModerationCaseRepository(database.Open);
        var service = new ModerationService(repo, new Moderator(Guid.Empty));
        int audits, tickets;
        using (var check = database.Open()) { audits = check.AuditRecords.Count(); tickets = check.AuditOutbox.Count(); }
        Assert.Throws<ArgumentException>(() => service.ReportNode(node, reporter, "A node", "", null, AtlasTime.UtcNow));
        Assert.ThrowsExactly<InvalidOperationException>(() => OperationBoundary.Execute(repo, () =>
        {
            service.ReportNode(node, reporter, "A node", "spam", null, AtlasTime.UtcNow);
            throw new InvalidOperationException("Injected workflow failure");
        }));
        using (var check = database.Open())
        {
            Assert.AreEqual(0, check.ModerationCaseRows.Count());
            Assert.AreEqual(0, check.ReportSubmissionQuotaRows.Count());
            Assert.AreEqual(audits, check.AuditRecords.Count());
            Assert.AreEqual(tickets, check.AuditOutbox.Count());
        }
        for (var index = 0; index < 3; index++)
            repo.Save(new ModerationCase(node, reporter, "spam", null, "A node", AtlasTime.UtcNow.AddDays(-7)));
        Assert.ThrowsExactly<ReportSubmissionQuotaExceededException>(() =>
            repo.Save(new ModerationCase(node, reporter, "spam", null, "A node", AtlasTime.UtcNow.AddDays(-7))));
    }

    [TestMethod]
    public async Task CompetingReportsCannotBothConsumeTheThirdSlot()
    {
        using var database = SqlTestDatabase.Create(useCurrentModel: true, installFullText: false);
        var reporter = database.AddParticipant();
        var node = database.AddNode();
        var repo = new SqlModerationCaseRepository(database.Open);
        for (var index = 0; index < 2; index++)
            repo.Save(new ModerationCase(node, reporter, "spam", null, "A node", AtlasTime.UtcNow));
        using var start = new Barrier(2);
        async Task<bool> Attempt() => await Task.Run(() =>
        {
            start.SignalAndWait();
            try
            {
                var repository = new SqlModerationCaseRepository(database.Open);
                new ModerationService(repository, new Moderator(Guid.Empty))
                    .ReportNode(node, reporter, "A node", "spam", null, AtlasTime.UtcNow);
                return true;
            }
            catch (ReportSubmissionQuotaExceededException) { return false; }
        });
        var results = await Task.WhenAll(Attempt(), Attempt());
        Assert.AreEqual(1, results.Count(success => success));
        using var check = database.Open();
        Assert.AreEqual(3, check.ModerationCaseRows.Count());
        Assert.AreEqual(3, check.ReportSubmissionQuotaRows.Count());
    }

    private sealed class Moderator(Guid id) : IModeratorAuthorization
    {
        public bool IsAtlasModerator(Guid participantId) => participantId == id && id != Guid.Empty;
    }
    private sealed class Clock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}

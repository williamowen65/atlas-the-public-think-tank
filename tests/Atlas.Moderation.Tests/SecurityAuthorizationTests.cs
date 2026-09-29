namespace Atlas.Moderation.Tests;

/// <summary>Security regression tests for moderator-only operations.</summary>
[TestClass]
public class SecurityAuthorizationTests
{
    [TestMethod]
    public void SEC003_UnprivilegedActor_CannotReadQueueOrHistoryOrDecide()
    {
        var moderatorId = Guid.NewGuid();
        var outsiderId = Guid.NewGuid();
        var nodeId = Guid.NewGuid();
        var repository = new CaseRepository();
        var service = new ModerationService(repository, new ModeratorPolicy(moderatorId));
        var report = service.ReportNode(nodeId, outsiderId, "Node", "spam", null, DateTimeOffset.UtcNow);

        Assert.ThrowsExactly<UnauthorizedAccessException>(() => service.Queue(outsiderId));
        Assert.ThrowsExactly<UnauthorizedAccessException>(() => service.NodeQueue(outsiderId));
        Assert.ThrowsExactly<UnauthorizedAccessException>(() => service.NodeHistory(outsiderId, nodeId));
        Assert.ThrowsExactly<UnauthorizedAccessException>(() =>
            service.DecideNode(outsiderId, nodeId, ModerationDecision.HideNode,
                "Rejected", DateTimeOffset.UtcNow));

        Assert.AreEqual(ModerationStatus.Submitted, repository.GetById(report.Id)!.Status);
        Assert.AreEqual(report.Id, service.Queue(moderatorId).Single().Id);
    }

    private sealed class ModeratorPolicy(Guid moderatorId) : IModeratorAuthorization
    {
        public bool IsAtlasModerator(Guid participantId) => participantId == moderatorId;
    }

    private sealed class CaseRepository : IModerationCaseRepository
    {
        private readonly Dictionary<Guid, ModerationCase> _cases = [];
        public ModerationCase? GetById(Guid caseId) => _cases.GetValueOrDefault(caseId);
        public IReadOnlyCollection<ModerationCase> GetAll() => _cases.Values.ToList();
        public void Save(ModerationCase moderationCase) => _cases[moderationCase.Id] = moderationCase;
    }
}

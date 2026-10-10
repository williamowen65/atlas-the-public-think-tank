using Atlas.Discovery;

namespace Atlas.Moderation.Tests;

[TestClass]
public sealed class ModerationWorkflowTests
{
    private readonly Guid _moderator = Guid.NewGuid();
    private readonly Guid _reporter = Guid.NewGuid();
    private readonly Guid _node = Guid.NewGuid();

    [TestMethod]
    public void Report_review_and_exclusion_hide_node_even_when_archived_is_included()
    {
        var repository = new Cases();
        var service = new ModerationService(repository, new Moderator(_moderator));
        var reported = service.ReportNode(_node, _reporter, "A Node", "harassment", "details", DateTimeOffset.UtcNow);
        Assert.AreEqual(reported.Id, service.Queue(_moderator).Single().Id);

        var decided = service.Decide(_moderator, reported.Id, ModerationDecision.HideNode,
            "Rule violation reviewed", DateTimeOffset.UtcNow);
        Assert.AreEqual(ModerationStatus.Actioned, decided.Status);
        Assert.AreEqual(_moderator, decided.ReviewerId);
        Assert.AreEqual(0, service.Queue(_moderator).Count);
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            service.Decide(_moderator, reported.Id, ModerationDecision.Dismiss, "retry", DateTimeOffset.UtcNow));

        var candidate = Candidate(decided.Status == ModerationStatus.Actioned);
        Assert.AreEqual(0, new DiscoveryService(new Source(candidate))
            .Discover(new DiscoveryQuery(IncludeArchived: true)).Count);
    }

    [TestMethod]
    public void Node_decision_closes_all_pending_reports_and_preserves_history()
    {
        var service = new ModerationService(new Cases(), new Moderator(_moderator));
        var first = service.ReportNode(_node, _reporter, "A Node", "spam", "first", DateTimeOffset.UtcNow);
        var second = service.ReportNode(_node, Guid.NewGuid(), "A Node", "harassment", "second", DateTimeOffset.UtcNow);
        var other = service.ReportNode(Guid.NewGuid(), _reporter, "Other Node", "spam", null, DateTimeOffset.UtcNow);
        var groups = service.NodeQueue(_moderator);
        Assert.AreEqual(2, groups.Count);
        Assert.AreEqual(2, groups.Single(group => group.NodeId == _node).PendingReports.Count);

        var decided = service.DecideNode(_moderator, _node, ModerationDecision.HideNode,
            "Reviewed both reports", DateTimeOffset.UtcNow);
        Assert.AreEqual(2, decided.Count);
        Assert.IsTrue(decided.All(report => report.Status == ModerationStatus.Actioned &&
            report.ReviewerId == _moderator && report.DecisionReason == "Reviewed both reports"));
        Assert.AreEqual(2, service.NodeHistory(_moderator, _node).Count);
        Assert.AreEqual(other.Id, service.Queue(_moderator).Single().Id);
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            service.Decide(_moderator, second.Id, ModerationDecision.Dismiss, "retry", DateTimeOffset.UtcNow));
    }

    [TestMethod]
    public void Dismiss_by_case_closes_other_pending_reports_on_the_same_node()
    {
        var service = new ModerationService(new Cases(), new Moderator(_moderator));
        var first = service.ReportNode(_node, _reporter, "A Node", "spam", null, DateTimeOffset.UtcNow);
        service.ReportNode(_node, Guid.NewGuid(), "A Node", "spam", null, DateTimeOffset.UtcNow);
        service.Decide(_moderator, first.Id, ModerationDecision.Dismiss, "No violation", DateTimeOffset.UtcNow);
        Assert.AreEqual(0, service.Queue(_moderator).Count);
        Assert.IsTrue(service.NodeHistory(_moderator, _node).All(report => report.Status == ModerationStatus.Dismissed));
    }

    [TestMethod]
    public void Unauthorized_reviewer_cannot_see_queue_or_decide()
    {
        var service = new ModerationService(new Cases(), new Moderator(_moderator));
        var report = service.ReportNode(_node, _reporter, "A Node", "spam", null, DateTimeOffset.UtcNow);
        Assert.ThrowsExactly<UnauthorizedAccessException>(() => service.Queue(_reporter));
        Assert.ThrowsExactly<UnauthorizedAccessException>(() =>
            service.Decide(_reporter, report.Id, ModerationDecision.HideNode, "reason", DateTimeOffset.UtcNow));
        Assert.AreEqual(ModerationStatus.Submitted, service.Queue(_moderator).Single().Status);
    }

    [TestMethod]
    public void Dismissal_keeps_node_discoverable()
    {
        var service = new ModerationService(new Cases(), new Moderator(_moderator));
        var report = service.ReportNode(_node, _reporter, "A Node", "disagreement", null, DateTimeOffset.UtcNow);
        var dismissed = service.Decide(_moderator, report.Id, ModerationDecision.Dismiss,
            "No violation", DateTimeOffset.UtcNow);
        Assert.AreEqual(ModerationStatus.Dismissed, dismissed.Status);
        Assert.AreEqual(1, new DiscoveryService(new Source(Candidate(false)))
            .Discover(new DiscoveryQuery()).Count);
    }

    [TestMethod]
    public void Hidden_node_shows_public_reason_and_author_review_can_restore_visibility()
    {
        var repository = new Cases();
        var service = new ModerationService(repository, new Moderator(_moderator));
        var reported = service.ReportNode(_node, _reporter, "Private title", "spam", null,
            DateTimeOffset.UtcNow.AddMinutes(-10));
        var decisionTime = DateTimeOffset.UtcNow.AddMinutes(-5);
        service.DecideNode(_moderator, _node, ModerationDecision.HideNode, "Internal details",
            decisionTime, PublicModerationReason.Spam);
        Assert.IsTrue(service.CanViewHiddenOriginal(_moderator, _reporter, _node));
        Assert.IsTrue(service.CanViewHiddenOriginal(_reporter, _reporter, _node));
        Assert.IsFalse(service.CanViewHiddenOriginal(Guid.NewGuid(), _reporter, _node));
        Assert.AreEqual("[Hidden by moderator: Spam]", service.Visibility(_node).Notice);
        Assert.ThrowsExactly<InvalidOperationException>(() => service.ReportNode(
            _node, Guid.NewGuid(), "Private title", "another report", null, DateTimeOffset.UtcNow));
        Assert.ThrowsExactly<UnauthorizedAccessException>(() => service.RequestNodeReview(
            _node, _reporter, Guid.NewGuid(), decisionTime.AddMinutes(1), DateTimeOffset.UtcNow));
        Assert.ThrowsExactly<InvalidOperationException>(() => service.RequestNodeReview(
            _node, _reporter, _reporter, decisionTime, DateTimeOffset.UtcNow));
        service.RequestNodeReview(_node, _reporter, _reporter,
            decisionTime.AddMinutes(1), DateTimeOffset.UtcNow);
        Assert.IsTrue(service.NodeQueue(_moderator).Single().ReviewRequested);
        service.RestoreNode(_moderator, _node, "Revision accepted", DateTimeOffset.UtcNow.AddMinutes(1));
        Assert.IsFalse(service.Visibility(_node).IsHidden);
        Assert.IsFalse(service.CanViewHiddenOriginal(_moderator, _reporter, _node));
        Assert.IsNotNull(service.ReportNode(_node, Guid.NewGuid(), "Revised title", "new concern", null,
            DateTimeOffset.UtcNow.AddMinutes(2)));
        Assert.IsNotNull(repository.GetById(reported.Id)?.VisibilityRestoredAt);
        Assert.AreEqual(_moderator, repository.GetById(reported.Id)?.RestoredBy);
    }



    private DiscoveryCandidate Candidate(bool excluded) =>
        new(_node, "A Node", "text", Guid.NewGuid(), Guid.NewGuid(), false,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, 0, null, [], [], excluded);

    private sealed class Moderator(Guid id) : IModeratorAuthorization
    {
        public bool IsAtlasModerator(Guid participantId) => participantId == id;
    }

    private sealed class Source(params DiscoveryCandidate[] candidates) : IDiscoveryCandidateSource
    {
        public IReadOnlyCollection<DiscoveryCandidate> GetCandidates() => candidates;
    }

    private sealed class Cases : IModerationCaseRepository, Atlas.Contracts.Operations.IReferenceLookup
    {
        public bool IsAvailable(string kind, Guid id, bool requireActive) => id != Guid.Empty;

        private readonly Dictionary<Guid, ModerationCase> _items = [];
        public ModerationCase? GetById(Guid caseId) => _items.GetValueOrDefault(caseId);
        public IReadOnlyCollection<ModerationCase> GetAll() => _items.Values.ToList();
        public void Save(ModerationCase moderationCase) => _items[moderationCase.Id] = moderationCase;
    }
}

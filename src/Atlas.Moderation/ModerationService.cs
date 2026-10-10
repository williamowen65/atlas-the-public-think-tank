namespace Atlas.Moderation;

public interface IModerationCaseRepository
{
    ModerationCase? GetById(Guid caseId);
    IReadOnlyCollection<ModerationCase> GetAll();
    void Save(ModerationCase moderationCase);
}

public interface IModeratorAuthorization
{
    bool IsAtlasModerator(Guid participantId);
}

public sealed record NodeReportGroup(Guid NodeId, string ReportedTitle,
    IReadOnlyList<ModerationCase> PendingReports, bool ReviewRequested);
public sealed record ModerationVisibility(bool IsHidden, PublicModerationReason PublicReason, bool ReviewRequested)
{
    public static ModerationVisibility Visible { get; } = new(false, PublicModerationReason.Other, false);
    public string Notice => $"[Hidden by moderator: {ReasonLabel}]";
    private string ReasonLabel => PublicReason switch
    {
        PublicModerationReason.UnsafeContent => "Unsafe content",
        PublicModerationReason.OffTopic => "Off topic",
        _ => PublicReason.ToString()
    };
}

/// <summary>Moderation owns cases; the host coordinates enforcement in the consumer boundary.</summary>
public sealed class ModerationService
{
    private readonly IModerationCaseRepository _cases;
    private readonly IModeratorAuthorization _authorization;

    public ModerationService(IModerationCaseRepository cases, IModeratorAuthorization authorization)
    {
        _cases = cases; _authorization = authorization;
    }

    public ModerationCase ReportNode(Guid nodeId, Guid reporterId, string title,
        string reason, string? explanation, DateTimeOffset createdAt)
    {
        return OperationBoundary.Execute(_cases, () =>
        {
        ReferenceValidation.Require(_cases, "Participant", reporterId);
        ReferenceValidation.Require(_cases, "Node", nodeId);
        if (Visibility(nodeId).IsHidden)
            throw new InvalidOperationException("This Node is already hidden by a moderator.");
        var item = new ModerationCase(nodeId, reporterId, reason, explanation, title, createdAt);
        _cases.Save(item);
        return item;
            });
    }

    public IReadOnlyCollection<ModerationCase> Queue(Guid actorId)
    {
        EnsureModerator(actorId);
        return _cases.GetAll().Where(c => c.Status == ModerationStatus.Submitted)
            .OrderBy(c => c.CreatedAt).ToList();
    }

    public IReadOnlyList<NodeReportGroup> NodeQueue(Guid actorId)
    {
        EnsureModerator(actorId);
        return _cases.GetAll()
            .GroupBy(report => report.NodeId)
            .Where(group => group.Any(report => report.Status == ModerationStatus.Submitted ||
                report.IsHidden && report.ReviewRequestedAt is not null))
            .Select(group => new NodeReportGroup(group.Key, group.First().ReportedTitle,
                group.Where(report => report.Status == ModerationStatus.Submitted).ToList(),
                group.Any(report => report.IsHidden && report.ReviewRequestedAt is not null)))
            .ToList();
    }

    public IReadOnlyList<ModerationCase> NodeHistory(Guid actorId, Guid nodeId)
    {
        EnsureModerator(actorId);
        return _cases.GetAll().Where(report => report.NodeId == nodeId)
            .OrderBy(report => report.CreatedAt).ToList();
    }

    public ModerationVisibility Visibility(Guid nodeId)
    {
        var hidden = _cases.GetAll().Where(report => report.NodeId == nodeId && report.IsHidden)
            .OrderByDescending(report => report.DecidedAt).FirstOrDefault();
        return hidden is null ? ModerationVisibility.Visible
            : new ModerationVisibility(true, hidden.PublicReason, hidden.ReviewRequestedAt is not null);
    }

    public bool CanViewHiddenOriginal(Guid actorId, Guid authorId, Guid nodeId) =>
        Visibility(nodeId).IsHidden &&
        (actorId == authorId || _authorization.IsAtlasModerator(actorId));

    public void RequestNodeReview(Guid nodeId, Guid actorId, Guid authorId,
        DateTimeOffset nodeUpdatedAt, DateTimeOffset requestedAt)
    {
        OperationBoundary.Execute(_cases, () =>
        {
        ReferenceValidation.Require(_cases, "Participant", actorId);
        ReferenceValidation.Require(_cases, "Node", nodeId, false);
        if (actorId == Guid.Empty || authorId == Guid.Empty || nodeId == Guid.Empty) throw new ArgumentException("Node and actor identifiers are required.");
        if (requestedAt < nodeUpdatedAt) throw new ArgumentException("Review request cannot precede the node edit.");
        if (actorId != authorId) throw new UnauthorizedAccessException("Node author required.");
        var hidden = _cases.GetAll().Where(report => report.NodeId == nodeId && report.IsHidden).ToList();
        if (hidden.Count == 0 || hidden.Any(report => report.ReviewRequestedAt is not null) ||
            hidden.Max(report => report.DecidedAt) >= nodeUpdatedAt)
            throw new InvalidOperationException("Edit the hidden Node before requesting review.");
        foreach (var report in hidden) Copy(report).RequestReview(requestedAt);
        foreach (var report in hidden) { report.RequestReview(requestedAt); _cases.Save(report); }
            });
    }

    public void RestoreNode(Guid actorId, Guid nodeId, string rationale, DateTimeOffset restoredAt)
    {
        OperationBoundary.Execute(_cases, () =>
        {
        EnsureModerator(actorId);
        var hidden = _cases.GetAll().Where(report => report.NodeId == nodeId && report.IsHidden).ToList();
        if (hidden.Count == 0 || hidden.Any(report => report.ReviewRequestedAt is null))
            throw new InvalidOperationException("No review request is pending for this Node.");
        if (string.IsNullOrWhiteSpace(rationale) || rationale.Length > 2000 ||
            hidden.Any(report => report.ReviewRequestedAt > restoredAt))
            throw new ArgumentException("A valid restoration rationale and time are required.");
        foreach (var report in hidden) Copy(report).RestoreVisibility(actorId, rationale, restoredAt);
        foreach (var report in hidden) { report.RestoreVisibility(actorId, rationale, restoredAt); _cases.Save(report); }
            });
    }

    public IReadOnlyList<ModerationCase> DecideNode(Guid actorId, Guid nodeId,
        ModerationDecision decision, string rationale, DateTimeOffset decidedAt,
        PublicModerationReason publicReason = PublicModerationReason.Other)
    {
        return OperationBoundary.Execute(_cases, () =>
        {
        EnsureModerator(actorId);
        var pending = _cases.GetAll().Where(report => report.NodeId == nodeId &&
            report.Status == ModerationStatus.Submitted).ToList();
        if (pending.Count == 0) throw new InvalidOperationException("No pending reports for this Node.");
        // Validate every case before saving any of the decisions.
        foreach (var report in pending)
            ModerationCase.Reconstitute(report.Id, report.NodeId, report.ReporterId, report.Reason,
                report.Explanation, report.ReportedTitle, report.CreatedAt, report.Status,
                report.ReviewerId, report.DecisionReason, report.DecidedAt)
                .Decide(actorId, decision, rationale, decidedAt, publicReason);
        foreach (var report in pending)
        {
            report.Decide(actorId, decision, rationale, decidedAt, publicReason);
            _cases.Save(report);
        }
        return pending;
            });
    }

    public ModerationCase Decide(Guid actorId, Guid caseId, ModerationDecision decision,
        string rationale, DateTimeOffset decidedAt)
    {
        return OperationBoundary.Execute(_cases, () =>
        {
        EnsureModerator(actorId);
        var item = _cases.GetById(caseId) ?? throw new KeyNotFoundException("Report not found.");
        if (item.Status != ModerationStatus.Submitted)
            throw new InvalidOperationException("Case already decided.");
        DecideNode(actorId, item.NodeId, decision, rationale, decidedAt);
        return item;
            });
    }

    public bool CanModerate(Guid actorId) => _authorization.IsAtlasModerator(actorId);

    private static ModerationCase Copy(ModerationCase item) => ModerationCase.Reconstitute(
        item.Id, item.NodeId, item.ReporterId, item.Reason, item.Explanation, item.ReportedTitle,
        item.CreatedAt, item.Status, item.ReviewerId, item.DecisionReason, item.DecidedAt,
        item.PublicReason, item.ReviewRequestedAt, item.VisibilityRestoredAt, item.RestoredBy, item.RestorationReason);

    private void EnsureModerator(Guid actorId)
    {
        ReferenceValidation.Require(_cases, "Participant", actorId);
        if (!CanModerate(actorId))
            throw new UnauthorizedAccessException("Atlas moderator access required.");
    }
}

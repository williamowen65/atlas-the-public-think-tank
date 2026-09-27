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

public sealed record NodeReportGroup(Guid NodeId, string ReportedTitle, IReadOnlyList<ModerationCase> PendingReports);

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
        var item = new ModerationCase(nodeId, reporterId, reason, explanation, title, createdAt);
        _cases.Save(item);
        return item;
    }

    public IReadOnlyCollection<ModerationCase> Queue(Guid actorId)
    {
        EnsureModerator(actorId);
        return _cases.GetAll().Where(c => c.Status == ModerationStatus.Submitted)
            .OrderBy(c => c.CreatedAt).ToList();
    }

    public IReadOnlyList<NodeReportGroup> NodeQueue(Guid actorId) => Queue(actorId)
        .GroupBy(report => report.NodeId)
        .Select(group => new NodeReportGroup(group.Key, group.First().ReportedTitle, group.ToList()))
        .ToList();

    public IReadOnlyList<ModerationCase> NodeHistory(Guid actorId, Guid nodeId)
    {
        EnsureModerator(actorId);
        return _cases.GetAll().Where(report => report.NodeId == nodeId)
            .OrderBy(report => report.CreatedAt).ToList();
    }

    public IReadOnlyList<ModerationCase> DecideNode(Guid actorId, Guid nodeId,
        ModerationDecision decision, string rationale, DateTimeOffset decidedAt)
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
                .Decide(actorId, decision, rationale, decidedAt);
        foreach (var report in pending)
        {
            report.Decide(actorId, decision, rationale, decidedAt);
            _cases.Save(report);
        }
        return pending;
    }

    public ModerationCase Decide(Guid actorId, Guid caseId, ModerationDecision decision,
        string rationale, DateTimeOffset decidedAt)
    {
        EnsureModerator(actorId);
        var item = _cases.GetById(caseId) ?? throw new KeyNotFoundException("Report not found.");
        if (item.Status != ModerationStatus.Submitted)
            throw new InvalidOperationException("Case already decided.");
        DecideNode(actorId, item.NodeId, decision, rationale, decidedAt);
        return item;
    }

    private void EnsureModerator(Guid actorId)
    {
        if (!_authorization.IsAtlasModerator(actorId))
            throw new UnauthorizedAccessException("Atlas moderator access required.");
    }
}

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

    public ModerationCase Decide(Guid actorId, Guid caseId, ModerationDecision decision,
        string rationale, DateTimeOffset decidedAt)
    {
        EnsureModerator(actorId);
        var item = _cases.GetById(caseId) ?? throw new KeyNotFoundException("Report not found.");
        item.Decide(actorId, decision, rationale, decidedAt);
        _cases.Save(item);
        return item;
    }

    private void EnsureModerator(Guid actorId)
    {
        if (!_authorization.IsAtlasModerator(actorId))
            throw new UnauthorizedAccessException("Atlas moderator access required.");
    }
}

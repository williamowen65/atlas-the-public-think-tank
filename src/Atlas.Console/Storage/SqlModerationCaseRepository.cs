using Atlas.Persistence;
using Atlas.Moderation;

namespace Atlas.ConsoleApp.Storage;

/// <summary>Maps domain objects directly to EF Core rows in SQL Server.</summary>
public sealed class SqlModerationCaseRepository(Func<AtlasDataContext>? contextFactory = null) : SqlRepository(contextFactory), IModerationCaseRepository
{

    // Read one case or all cases, then reconstitute the saved moderation history.
    public ModerationCase? GetById(Guid id) => FindRow<ModerationCaseRow>(id) is { } row ? ToDomain(row) : null;
    public IReadOnlyCollection<ModerationCase> GetAll() => ReadRows<ModerationCaseRow>().Select(ToDomain).ToList();

    // Persist the report, decision and restoration state together. SaveRow joins an outer transaction,
    // so a grouped moderation workflow can save several cases atomically.
    public void Save(ModerationCase item) => SaveRow(new ModerationCaseRow { Id = item.Id, NodeId = item.NodeId, ReporterId = item.ReporterId,
        Reason = item.Reason, Explanation = item.Explanation, ReportedTitle = item.ReportedTitle, CreatedAt = item.CreatedAt, Status = (int)item.Status,
        ReviewerId = item.ReviewerId, DecisionReason = item.DecisionReason, DecidedAt = item.DecidedAt, PublicReason = (int)item.PublicReason,
        ReviewRequestedAt = item.ReviewRequestedAt, VisibilityRestoredAt = item.VisibilityRestoredAt, RestoredBy = item.RestoredBy, RestorationReason = item.RestorationReason });

    // Convert stored enum numbers and lifecycle fields back into a validated moderation case.
    private static ModerationCase ToDomain(ModerationCaseRow row) => ModerationCase.Reconstitute(row.Id, row.NodeId, row.ReporterId,
        row.Reason, row.Explanation, row.ReportedTitle, row.CreatedAt, (ModerationStatus)row.Status, row.ReviewerId, row.DecisionReason,
        row.DecidedAt, (PublicModerationReason)row.PublicReason, row.ReviewRequestedAt, row.VisibilityRestoredAt, row.RestoredBy, row.RestorationReason);
}

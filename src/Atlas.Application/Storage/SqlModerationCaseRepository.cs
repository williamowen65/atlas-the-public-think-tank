using Atlas.Persistence;
using Atlas.Moderation;

namespace Atlas.Application.Storage;

/// <summary>Maps domain objects directly to EF Core rows in SQL Server.</summary>
public sealed class SqlModerationCaseRepository(Func<AtlasDataContext>? contextFactory = null, ReportSubmissionQuotaPolicy? quotaPolicy = null) : SqlRepository(contextFactory), IModerationCaseRepository, IReportSubmissionQuota
{

    private readonly ReportSubmissionQuotaPolicy _quotaPolicy = quotaPolicy ?? ReportSubmissionQuotaPolicy.Default;

    /// <summary>Serialize report submission checks and writes per reporter across host processes.</summary>
    public T ExecuteSubmission<T>(Guid reporterId, Func<T> operation)
    {
        if (reporterId == Guid.Empty) throw new ArgumentException("A reporter is required.", nameof(reporterId));
        return ExecuteWithLock(operation, $"AtlasReportSubmissions:{reporterId:N}");
    }

    /// <summary>Advisory allowance; only a committed new report consumes a slot.</summary>
    public ReportSubmissionAllowance GetSubmissionAllowance(Guid reporterId)
    {
        if (reporterId == Guid.Empty) throw new ArgumentException("A reporter is required.", nameof(reporterId));
        var now = AtlasTime.UtcNow;
        var history = QueryRows<ReportSubmissionQuotaRow>(
            row => row.ParticipantId == reporterId && row.OccurredAt > now - _quotaPolicy.Window,
            query => query.OrderByDescending(row => row.OccurredAt).Take(_quotaPolicy.Limit));
        return _quotaPolicy.Evaluate(history.Select(row => row.OccurredAt), now);
    }

    public void EnsureSubmissionAllowed(Guid reporterId)
    {
        var allowance = GetSubmissionAllowance(reporterId);
        if (allowance.Remaining == 0) throw new ReportSubmissionQuotaExceededException(allowance);
    }

    // Read one case or all cases, then reconstitute the saved moderation history.
    public ModerationCase? GetById(Guid id) => FindRow<ModerationCaseRow>(id) is { } row ? ToDomain(row) : null;
    public IReadOnlyCollection<ModerationCase> GetAll() => ReadRows<ModerationCaseRow>().Select(ToDomain).ToList();

    // Persist the report, decision and restoration state together. SaveRow joins an outer transaction,
    // so a grouped moderation workflow can save several cases atomically.
    public void Save(ModerationCase item)
    {
        ArgumentNullException.ThrowIfNull(item);
        ExecuteSubmission(item.ReporterId, () =>
        {
            var isNew = FindRow<ModerationCaseRow>(item.Id) is null;
            if (isNew)
            {
                ReferenceValidation.Require(this, "Participant", item.ReporterId);
                ReferenceValidation.Require(this, "Node", item.NodeId);
                EnsureSubmissionAllowed(item.ReporterId);
            }
            SaveRow(new ModerationCaseRow {
                Id = item.Id,
                NodeId = item.NodeId,
                ReporterId = item.ReporterId,
                Reason = item.Reason,
                Explanation = item.Explanation,
                ReportedTitle = item.ReportedTitle,
                CreatedAt = item.CreatedAt,
                Status = (int)item.Status,
                ReviewerId = item.ReviewerId,
                DecisionReason = item.DecisionReason,
                DecidedAt = item.DecidedAt,
                PublicReason = (int)item.PublicReason,
                ReviewRequestedAt = item.ReviewRequestedAt,
                VisibilityRestoredAt = item.VisibilityRestoredAt,
                RestoredBy = item.RestoredBy,
                RestorationReason = item.RestorationReason
            });
            if (isNew)
                SaveRow(new ReportSubmissionQuotaRow
                {
                    Id = item.Id, ParticipantId = item.ReporterId, OccurredAt = AtlasTime.UtcNow
                });
            return true;
        });
    }


    // Convert stored enum numbers and lifecycle fields back into a validated moderation case.
    private static ModerationCase ToDomain(ModerationCaseRow row) => ModerationCase.Reconstitute(
        row.Id,
        row.NodeId,
        row.ReporterId,
        row.Reason,
        row.Explanation,
        row.ReportedTitle,
        row.CreatedAt,
        (ModerationStatus)row.Status,
        row.ReviewerId,
        row.DecisionReason,
        row.DecidedAt,
        (PublicModerationReason)row.PublicReason,
        row.ReviewRequestedAt,
        row.VisibilityRestoredAt,
        row.RestoredBy,
        row.RestorationReason
    );
}

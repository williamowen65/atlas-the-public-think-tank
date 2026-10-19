using Atlas.Persistence;
using Microsoft.EntityFrameworkCore;
using Atlas.Comments.Comments;

namespace Atlas.Application.Storage;

/// <summary>Maps domain objects directly to EF Core rows in SQL Server.</summary>
public sealed class SqlCommentRepository(Func<AtlasDataContext>? contextFactory = null) : SqlRepository(contextFactory), ICommentRepository
{
    // Find the stored row, then rebuild a validated domain object; missing IDs return null.
    public Comment? GetById(CommentId id) => FindRow<CommentRow>(id.Value) is { } row ? ToDomain(row) : null;

    // Select comments for this exact target kind and ID, then reconstitute each result.
    public IReadOnlyCollection<Comment> GetByTarget(CommentTarget target) =>
        QueryRows<CommentRow>(row => row.TargetKind == target.Kind && row.TargetId == target.Id).Select(ToDomain).ToList();

    /// <summary>Fetch only the requested sibling page and one lookahead row in SQL, never an entire target.</summary>
    public async Task<CommentPage> ReadPageAsync(CommentPageRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        using var lease = OpenReadContext();
        var rows = await BuildPageQuery(lease.Context, request).ToListAsync(cancellationToken);
        var items = rows.Take(request.Size).Select(ToDomain).ToList();
        var next = rows.Count > request.Size && items.Count > 0
            ? new CommentCursor(items[^1].CreatedAt, items[^1].Id) : null;
        return new CommentPage(items, next);
    }

    // Kept separate so generated SQL (TOP, filters and cursor predicates) can be checked without a database.
    internal static IQueryable<CommentRow> BuildPageQuery(AtlasDataContext db, CommentPageRequest request)
    {
        var parentId = request.ParentId?.Value;
        var query = db.CommentRows.AsNoTracking().Where(row => row.TargetKind == request.Target.Kind
            && row.TargetId == request.Target.Id && row.ParentCommentId == parentId);
        if (request.After is { } after)
            query = query.Where(row => row.CreatedAt > after.CreatedAt
                || row.CreatedAt == after.CreatedAt && row.Id.CompareTo(after.Id.Value) > 0);
        // SQL Server orders/comparisons use the same uniqueidentifier order, including timestamp ties.
        return query.OrderBy(row => row.CreatedAt).ThenBy(row => row.Id).Take(request.Size + 1)
            .TagWith("Atlas.Comments.SiblingPage");
    }

    public int CountByTarget(CommentTarget target)
    {
        ArgumentNullException.ThrowIfNull(target);
        using var lease = OpenReadContext();
        return lease.Context.CommentRows.AsNoTracking().Count(row => row.TargetKind == target.Kind && row.TargetId == target.Id);
    }

    // Validate and save in one SQL operation. Nested calls join an existing transaction.
    public void Save(Comment comment) => Execute(() =>
    {
        // Reject a null object before reading its properties.
        ArgumentNullException.ThrowIfNull(comment);

        // Check cross-domain references before writing. Live comments require an active author
        // and an available Node; removed comments require existence only, so removal can be saved
        // after an author is deactivated or a Node becomes unavailable.
        ReferenceValidation.Require(
            this,
            "Participant",
            comment.AuthorParticipantId,
            !comment.IsRemoved
        );
        if (comment.Target.Kind != "Node") throw new ArgumentException("Only Node comment targets are supported.");
        ReferenceValidation.Require(
            this,
            "Node",
            comment.Target.Id,
            !comment.IsRemoved
        );

        // Replies must point to an existing comment on the same target. Their creation time
        // cannot be earlier than the parent's; root comments skip this check.
        if (comment.ParentCommentId is { } parentId) // Equivalent to HasValue; unwraps Value into parentId.
        {
            var parent = FindRow<CommentRow>(parentId.Value) ?? throw new ArgumentException("Comment parent does not exist.");
            if (parent.TargetKind != comment.Target.Kind || parent.TargetId != comment.Target.Id || parent.CreatedAt > comment.CreatedAt)
                throw new ArgumentException("Reply must share its parent's target and cannot precede it.");
        }

        // Copy domain values into the SQL row. SaveRow inserts or updates this comment
        // and flushes the write within the transaction; it does not commit an outer workflow.
        SaveRow(new CommentRow {
            Id = comment.Id.Value,
            TargetKind = comment.Target.Kind,
            TargetId = comment.Target.Id,
            ParentCommentId = comment.ParentCommentId?.Value,
            AuthorParticipantId = comment.AuthorParticipantId,
            Body = comment.Body,
            Status = comment.Status.ToString(),
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            RemovedAt = comment.RemovedAt
        });

        // Execute requires a result; this internal value is not a persistence ID or response.
        // The owning boundary commits on success and rolls back if validation or saving throws.
        return true;
    });

    // Reconstitution checks stored values against domain rules too. Convert nullable parent IDs
    // and the stored status text back to their domain types, preserving lifecycle timestamps.
    private static Comment ToDomain(CommentRow row) => Comment.Reconstitute(
        new CommentId(row.Id),
        new CommentTarget(
            row.TargetKind,
            row.TargetId
        ),
        row.ParentCommentId.HasValue ? new CommentId(row.ParentCommentId.Value) : null,
        row.AuthorParticipantId,
        row.Body,
        Enum.Parse<CommentStatus>(
            row.Status,
            true
        ),
        row.CreatedAt,
        row.UpdatedAt,
        row.RemovedAt
    );
}

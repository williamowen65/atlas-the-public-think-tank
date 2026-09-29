using Atlas.Persistence;
using Atlas.Comments.Comments;

namespace Atlas.ConsoleApp.Storage;

/// <summary>Maps domain objects directly to EF Core rows in SQL Server.</summary>
public sealed class SqlCommentRepository(Func<AtlasDataContext>? contextFactory = null) : SqlRepository(contextFactory), ICommentRepository
{
    public Comment? GetById(CommentId id) => FindRow<CommentRow>(id.Value) is { } row ? ToDomain(row) : null;
    public IReadOnlyCollection<Comment> GetByTarget(CommentTarget target) =>
        QueryRows<CommentRow>(row => row.TargetKind == target.Kind && row.TargetId == target.Id).Select(ToDomain).ToList();
    public void Save(Comment comment) => SaveRow(new CommentRow { Id = comment.Id.Value, TargetKind = comment.Target.Kind,
        TargetId = comment.Target.Id, ParentCommentId = comment.ParentCommentId?.Value, AuthorParticipantId = comment.AuthorParticipantId,
        Body = comment.Body, Status = comment.Status.ToString(), CreatedAt = comment.CreatedAt, UpdatedAt = comment.UpdatedAt, RemovedAt = comment.RemovedAt });
    private static Comment ToDomain(CommentRow row) => Comment.Reconstitute(new CommentId(row.Id), new CommentTarget(row.TargetKind, row.TargetId),
        row.ParentCommentId.HasValue ? new CommentId(row.ParentCommentId.Value) : null, row.AuthorParticipantId, row.Body,
        Enum.Parse<CommentStatus>(row.Status, true), row.CreatedAt, row.UpdatedAt, row.RemovedAt);
}

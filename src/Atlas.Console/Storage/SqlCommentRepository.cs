using Atlas.Persistence;
using Atlas.Comments.Comments;

namespace Atlas.ConsoleApp.Storage;

/// <summary>Maps domain objects directly to EF Core rows in SQL Server.</summary>
public sealed class SqlCommentRepository(Func<AtlasDataContext>? contextFactory = null) : SqlRepository(contextFactory), ICommentRepository
{
    public Comment? GetById(CommentId id) => FindRow<CommentRow>(id.Value) is { } row ? ToDomain(row) : null;
    public IReadOnlyCollection<Comment> GetByTarget(CommentTarget target) =>
        QueryRows<CommentRow>(row => row.TargetKind == target.Kind && row.TargetId == target.Id).Select(ToDomain).ToList();
    public void Save(Comment comment) => Execute(() =>
    {
        ArgumentNullException.ThrowIfNull(comment);
        ReferenceValidation.Require(this, "Participant", comment.AuthorParticipantId, !comment.IsRemoved);
        if (comment.Target.Kind != "Node") throw new ArgumentException("Only Node comment targets are supported.");
        ReferenceValidation.Require(this, "Node", comment.Target.Id, !comment.IsRemoved);
        if (comment.ParentCommentId is { } parentId)
        {
            var parent = FindRow<CommentRow>(parentId.Value) ?? throw new ArgumentException("Comment parent does not exist.");
            if (parent.TargetKind != comment.Target.Kind || parent.TargetId != comment.Target.Id || parent.CreatedAt > comment.CreatedAt)
                throw new ArgumentException("Reply must share its parent's target and cannot precede it.");
        }
        SaveRow(new CommentRow { Id = comment.Id.Value, TargetKind = comment.Target.Kind,
        TargetId = comment.Target.Id, ParentCommentId = comment.ParentCommentId?.Value, AuthorParticipantId = comment.AuthorParticipantId,
        Body = comment.Body, Status = comment.Status.ToString(), CreatedAt = comment.CreatedAt, UpdatedAt = comment.UpdatedAt, RemovedAt = comment.RemovedAt });
        return true;
    });
    private static Comment ToDomain(CommentRow row) => Comment.Reconstitute(new CommentId(row.Id), new CommentTarget(row.TargetKind, row.TargetId),
        row.ParentCommentId.HasValue ? new CommentId(row.ParentCommentId.Value) : null, row.AuthorParticipantId, row.Body,
        Enum.Parse<CommentStatus>(row.Status, true), row.CreatedAt, row.UpdatedAt, row.RemovedAt);
}

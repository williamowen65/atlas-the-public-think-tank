namespace Atlas.Comments.Comments;

public interface ICommentRepository
{
    Comment? GetById(CommentId id);
    IReadOnlyCollection<Comment> GetByTarget(CommentTarget target);
    /// <summary>Read at most Size siblings plus one lookahead row, using stable oldest-first keyset ordering.</summary>
    Task<CommentPage> ReadPageAsync(CommentPageRequest request, CancellationToken cancellationToken = default);
    /// <summary>Count without loading comment bodies. Intended for node-card summaries, not page traversal.</summary>
    int CountByTarget(CommentTarget target);
    void Save(Comment comment);
}

namespace Atlas.Comments.Comments;

public interface ICommentRepository
{
    Comment? GetById(CommentId id);
    IReadOnlyCollection<Comment> GetByTarget(CommentTarget target);
    void Save(Comment comment);
}

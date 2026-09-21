namespace Atlas.Comments.Comments;

/// <summary>Port used by Comments to ask whether an external discussion target currently accepts mutations.</summary>
public interface ICommentTargetAvailability
{
    bool IsAvailable(CommentTarget target);
}

namespace Atlas.Comments.Comments;

/// <summary>Coordinates comment use cases and invariants that require repository or target state.</summary>
public sealed class CommentService
{
    private readonly ICommentRepository _comments;
    private readonly ICommentTargetAvailability _targets;

    public CommentService(ICommentRepository comments, ICommentTargetAvailability targets)
    {
        _comments = comments;
        _targets = targets;
    }

    public Comment AddTopLevel(CommentTarget target, Guid authorParticipantId, string body, DateTimeOffset createdAt)
    {
        EnsureTargetAvailable(target);
        var comment = new Comment(target, null, authorParticipantId, body, createdAt);
        _comments.Save(comment);
        return comment;
    }

    public Comment Reply(CommentId parentId, Guid authorParticipantId, string body, DateTimeOffset createdAt)
    {
        var parent = _comments.GetById(parentId)
            ?? throw new InvalidOperationException("The parent comment does not exist.");

        EnsureTargetAvailable(parent.Target);

        // Policy decision: removed ancestors remain as thread placeholders and may still receive replies.
        var reply = new Comment(parent.Target, parent.Id, authorParticipantId, body, createdAt);
        _comments.Save(reply);
        return reply;
    }

    public void Edit(CommentId commentId, Guid actorParticipantId, string body, DateTimeOffset changedAt)
    {
        var comment = GetRequired(commentId);
        EnsureTargetAvailable(comment.Target);
        comment.Edit(actorParticipantId, body, changedAt);
        _comments.Save(comment);
    }

    public void Remove(CommentId commentId, Guid actorParticipantId, bool isModerator, DateTimeOffset removedAt)
    {
        var comment = GetRequired(commentId);
        if (actorParticipantId == comment.AuthorParticipantId)
            comment.RemoveByAuthor(actorParticipantId, removedAt);
        else if (isModerator)
            comment.RemoveByModerator(removedAt);
        else
            throw new UnauthorizedAccessException("Only the comment author or a moderator may remove this comment.");

        _comments.Save(comment);
    }

    public IReadOnlyList<CommentThreadItem> GetThread(CommentTarget target)
    {
        var comments = _comments.GetByTarget(target).ToList();
        var children = comments
            .GroupBy(x => x.ParentCommentId)
            .ToDictionary(x => x.Key, x => x.OrderBy(c => c.CreatedAt).ThenBy(c => c.Id.Value).ToList());

        var result = new List<CommentThreadItem>();
        AppendChildren(null, 0, children, result);
        return result;
    }

    private static void AppendChildren(
        CommentId? parentId,
        int depth,
        IReadOnlyDictionary<CommentId?, List<Comment>> children,
        List<CommentThreadItem> result)
    {
        if (!children.TryGetValue(parentId, out var directChildren)) return;
        foreach (var child in directChildren)
        {
            result.Add(new CommentThreadItem(child, depth));
            AppendChildren(child.Id, depth + 1, children, result);
        }
    }

    private Comment GetRequired(CommentId id) =>
        _comments.GetById(id) ?? throw new InvalidOperationException("The comment does not exist.");

    private void EnsureTargetAvailable(CommentTarget target)
    {
        if (!_targets.IsAvailable(target))
            throw new InvalidOperationException("The discussion target is unavailable for comment changes.");
    }
}

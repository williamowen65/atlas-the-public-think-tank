namespace Atlas.Comments.Comments;

/// <summary>Coordinates comment use cases and invariants that require repository or target state.</summary>
public interface ICommentModeratorAuthorization
{
    bool CanModerate(Guid participantId);
}

public sealed class CommentService
{
    private readonly ICommentRepository _comments;
    private readonly ICommentTargetAvailability _targets;
    private readonly ICommentModeratorAuthorization _moderators;

    public CommentService(
        ICommentRepository comments,
        ICommentTargetAvailability targets,
        ICommentModeratorAuthorization moderators
    )
    {
        _comments = comments;
        _targets = targets;
        _moderators = moderators;
    }

    public Comment AddTopLevel(
        CommentTarget target,
        Guid authorParticipantId,
        string body,
        DateTimeOffset createdAt
    )
    {
        return OperationBoundary.Execute(
            _comments,
            () =>
        {
            ReferenceValidation.Require(
                _comments,
                "Participant",
                authorParticipantId
            );
            EnsureTargetAvailable(target);
            var comment = new Comment(
                target,
                null,
                authorParticipantId,
                body,
                createdAt
            );
            _comments.Save(comment);
            return comment;
        }
        );
    }

    public Comment Reply(
        CommentId parentId,
        Guid authorParticipantId,
        string body,
        DateTimeOffset createdAt
    )
    {
        return OperationBoundary.Execute(
            _comments,
            () =>
        {
            if (parentId.Value == Guid.Empty) throw new ArgumentException("A parent comment ID is required.");
            ReferenceValidation.Require(
                _comments,
                "Participant",
                authorParticipantId
            );
            var parent = _comments.GetById(parentId)
                ?? throw new InvalidOperationException("The parent comment does not exist.");

            EnsureTargetAvailable(parent.Target);

            // Policy decision: removed ancestors remain as thread placeholders and may still receive replies.
            var reply = new Comment(
                parent.Target,
                parent.Id,
                authorParticipantId,
                body,
                createdAt
            );
            _comments.Save(reply);
            return reply;
        }
        );
    }

    public void Edit(
        CommentId commentId,
        Guid actorParticipantId,
        string body,
        DateTimeOffset changedAt
    )
    {
        OperationBoundary.Execute(
            _comments,
            () =>
        {
            ReferenceValidation.Require(
                _comments,
                "Participant",
                actorParticipantId
            );
            var comment = GetRequired(commentId);
            EnsureTargetAvailable(comment.Target);
            comment.Edit(
                actorParticipantId,
                body,
                changedAt
            );
            _comments.Save(comment);
        }
        );
    }

    public void Remove(
        CommentId commentId,
        Guid actorParticipantId,
        DateTimeOffset removedAt
    )
    {
        OperationBoundary.Execute(
            _comments,
            () =>
        {
            ReferenceValidation.Require(
                _comments,
                "Participant",
                actorParticipantId
            );
            var comment = GetRequired(commentId);
            if (actorParticipantId == comment.AuthorParticipantId)
                comment.RemoveByAuthor(
                    actorParticipantId,
                    removedAt
                );
            else if (_moderators.CanModerate(actorParticipantId))
                comment.RemoveByModerator(removedAt);
            else
                throw new UnauthorizedAccessException("Only the comment author or a moderator may remove this comment.");

            _comments.Save(comment);
        }
        );
    }

    /// <summary>Load roots or direct replies on demand, without loading descendants or the full thread.</summary>
    /// <remarks>Removed parents remain readable placeholders. Parent scope is checked before returning replies.
    /// Cancellation propagates to the repository; a future HTTP host can pass RequestAborted.</remarks>
    public Task<CommentPage> GetPageAsync(CommentPageRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        if (request.ParentId is { } parentId)
        {
            var parent = GetRequired(parentId);
            if (parent.Target != request.Target) throw new ArgumentException("The parent belongs to a different discussion.", nameof(request));
        }
        return _comments.ReadPageAsync(request, cancellationToken);
    }

    /// <summary>Read a thread in stable depth-first order without recursive call-stack growth.</summary>
    /// <remarks>Deep valid threads remain supported. This legacy full-thread utility still materializes the thread. Interactive and future API reads must use GetPageAsync.</remarks>
    public IReadOnlyList<CommentThreadItem> GetThread(CommentTarget target)
    {
        var comments = _comments.GetByTarget(target).ToList();
        var roots = comments
            .Where(x => x.ParentCommentId is null)
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id.Value)
            .ToList();
        var children = comments
            .Where(x => x.ParentCommentId is not null)
            .GroupBy(x => x.ParentCommentId!.Value)
            .ToDictionary(
                x => x.Key,
                x => x.OrderBy(c => c.CreatedAt).ThenBy(c => c.Id.Value).ToList()
            );

        var result = new List<CommentThreadItem>();
        var visited = new HashSet<CommentId>();
        var pending = new Stack<(Comment Comment, int Depth)>();
        // Reverse pushes preserve the existing oldest-first depth-first display order.
        for (var index = roots.Count - 1; index >= 0; index--)
            pending.Push((roots[index], 0));
        while (pending.TryPop(out var item))
        {
            if (!visited.Add(item.Comment.Id))
                throw new InvalidOperationException("The stored comment thread contains duplicate or cyclic links.");
            result.Add(new CommentThreadItem(item.Comment, item.Depth));
            if (!children.TryGetValue(item.Comment.Id, out var directChildren)) continue;
            for (var index = directChildren.Count - 1; index >= 0; index--)
                pending.Push((directChildren[index], checked(item.Depth + 1)));
        }
        // Never silently return a partial thread for an orphan or disconnected cycle.
        if (result.Count != comments.Count)
            throw new InvalidOperationException("The stored comment thread contains unreachable parent links.");
        return result;
    }

    private Comment GetRequired(CommentId id) =>
        _comments.GetById(id) ?? throw new InvalidOperationException("The comment does not exist.");

    private void EnsureTargetAvailable(CommentTarget target)
    {
        ArgumentNullException.ThrowIfNull(target);
        if (!_targets.IsAvailable(target))
            throw new InvalidOperationException("The discussion target is unavailable for comment changes.");
    }
}

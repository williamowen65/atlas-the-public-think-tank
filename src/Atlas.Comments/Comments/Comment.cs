namespace Atlas.Comments.Comments;

/// <summary>
/// A single comment in a Comments-owned, single-parent discussion tree.
/// </summary>
public sealed class Comment
{
    public const int MaximumBodyLength = 10_000;

    public CommentId Id { get; }
    public CommentTarget Target { get; }
    public CommentId? ParentCommentId { get; }
    public Guid AuthorParticipantId { get; }
    public string Body { get; private set; }
    public CommentStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? RemovedAt { get; private set; }

    public bool IsRemoved => Status != CommentStatus.Active;

    public Comment(
        CommentTarget target,
        CommentId? parentCommentId,
        Guid authorParticipantId,
        string body,
        DateTimeOffset createdAt)
        : this(CommentId.New(), target, parentCommentId, authorParticipantId, body,
            CommentStatus.Active, createdAt, createdAt, null)
    {
    }

    private Comment(
        CommentId id,
        CommentTarget target,
        CommentId? parentCommentId,
        Guid authorParticipantId,
        string body,
        CommentStatus status,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        DateTimeOffset? removedAt)
    {
        if (authorParticipantId == Guid.Empty) throw new ArgumentException("A comment author is required.", nameof(authorParticipantId));
        if (parentCommentId == id) throw new ArgumentException("A comment cannot be its own parent.", nameof(parentCommentId));
        if (updatedAt < createdAt) throw new ArgumentException("Updated time cannot precede created time.");
        if (removedAt is not null && removedAt < createdAt) throw new ArgumentException("Removed time cannot precede created time.");

        Id = id;
        Target = target ?? throw new ArgumentNullException(nameof(target));
        ParentCommentId = parentCommentId;
        AuthorParticipantId = authorParticipantId;
        Body = ValidateBody(body);
        Status = status;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        RemovedAt = removedAt;
    }

    public static Comment Reconstitute(
        CommentId id,
        CommentTarget target,
        CommentId? parentCommentId,
        Guid authorParticipantId,
        string body,
        CommentStatus status,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        DateTimeOffset? removedAt) =>
        new(id, target, parentCommentId, authorParticipantId, body, status, createdAt, updatedAt, removedAt);

    public void Edit(Guid actorParticipantId, string body, DateTimeOffset changedAt)
    {
        EnsureAuthor(actorParticipantId);
        EnsureActive();
        EnsureLater(changedAt);
        Body = ValidateBody(body);
        UpdatedAt = changedAt;
    }

    public void RemoveByAuthor(Guid actorParticipantId, DateTimeOffset removedAt)
    {
        EnsureAuthor(actorParticipantId);
        EnsureActive();
        EnsureLater(removedAt);
        Status = CommentStatus.RemovedByAuthor;
        RemovedAt = removedAt;
        UpdatedAt = removedAt;
    }

    public void RemoveByModerator(DateTimeOffset removedAt)
    {
        EnsureActive();
        EnsureLater(removedAt);
        Status = CommentStatus.RemovedByModerator;
        RemovedAt = removedAt;
        UpdatedAt = removedAt;
    }

    private void EnsureAuthor(Guid actorParticipantId)
    {
        if (actorParticipantId != AuthorParticipantId)
            throw new UnauthorizedAccessException("Only the comment author may perform this action.");
    }

    private void EnsureActive()
    {
        if (IsRemoved) throw new InvalidOperationException("Removed comments cannot be changed.");
    }

    private void EnsureLater(DateTimeOffset changedAt)
    {
        if (changedAt <= UpdatedAt) throw new ArgumentException("Change time must be later than the current update time.", nameof(changedAt));
    }

    private static string ValidateBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) throw new ArgumentException("Comment text is required.", nameof(body));
        var trimmed = body.Trim();
        if (trimmed.Length > MaximumBodyLength) throw new ArgumentException($"Comment text cannot exceed {MaximumBodyLength} characters.", nameof(body));
        return trimmed;
    }
}

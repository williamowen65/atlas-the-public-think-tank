namespace Atlas.Comments.Comments;

/// <summary>Exclusive position in oldest-first sibling order. Hosts may encode this as an opaque API cursor.</summary>
public sealed record CommentCursor(DateTimeOffset CreatedAt, CommentId Id);

/// <summary>One bounded sibling query: null parent selects roots; a parent selects only its direct replies.</summary>
public sealed record CommentPageRequest
{
    public const int DefaultSize = 20;
    public const int MaximumSize = 50;
    public CommentTarget Target { get; }
    public CommentId? ParentId { get; }
    public CommentCursor? After { get; }
    public int Size { get; }

    public CommentPageRequest(CommentTarget target, CommentId? parentId = null, CommentCursor? after = null, int size = DefaultSize)
    {
        ArgumentNullException.ThrowIfNull(target);
        if (parentId is { Value: var parent } && parent == Guid.Empty) throw new ArgumentException("A parent ID must not be empty.", nameof(parentId));
        if (after is { Id.Value: var id } && id == Guid.Empty) throw new ArgumentException("A cursor ID must not be empty.", nameof(after));
        if (size < 1 || size > MaximumSize) throw new ArgumentOutOfRangeException(nameof(size), $"Page size must be between 1 and {MaximumSize}.");
        Target = target; ParentId = parentId; After = after; Size = size;
    }
}

/// <summary>No total-thread materialization or recursive descendants; Next is null when no extra sibling was found.</summary>
public sealed record CommentPage(IReadOnlyList<Comment> Items, CommentCursor? Next);

namespace Atlas.Comments.Comments;

/// <summary>A stable cross-boundary reference to the discussion target.</summary>
public sealed record CommentTarget
{
    public string Kind { get; }
    public Guid Id { get; }

    public CommentTarget(string kind, Guid id)
    {
        if (string.IsNullOrWhiteSpace(kind)) throw new ArgumentException("A target kind is required.", nameof(kind));
        if (id == Guid.Empty) throw new ArgumentException("A target ID is required.", nameof(id));
        Kind = kind.Trim();
        Id = id;
    }

    public static CommentTarget Node(Guid nodeId) => new("Node", nodeId);
}

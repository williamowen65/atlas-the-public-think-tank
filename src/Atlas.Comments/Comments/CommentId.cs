namespace Atlas.Comments.Comments;

public readonly record struct CommentId(Guid Value)
{
    public static CommentId New() => new(Guid.NewGuid());

    public CommentId
    {
        if (Value == Guid.Empty) throw new ArgumentException("A comment ID is required.", nameof(Value));
    }

    public override string ToString() => Value.ToString();
}

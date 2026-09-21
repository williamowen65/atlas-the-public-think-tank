namespace Atlas.Comments.Comments;

public readonly record struct CommentId
{
    public Guid Value { get; }

    public CommentId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("A comment ID is required.", nameof(value));

        Value = value;
    }

    public static CommentId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}

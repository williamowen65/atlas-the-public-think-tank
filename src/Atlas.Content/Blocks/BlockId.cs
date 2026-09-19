namespace Atlas.Content.Blocks;

public sealed record BlockId
{
    public Guid Value { get; }

    public BlockId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("A block ID is required.", nameof(value));
        }

        Value = value;
    }

    public static BlockId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}

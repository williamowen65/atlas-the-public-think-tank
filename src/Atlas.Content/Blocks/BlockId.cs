namespace Atlas.Content.Blocks;

/// <summary>Strongly typed, stable identity for a block within Content.</summary>
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

    /// <summary>Creates a random identity for a newly created block.</summary>
    public static BlockId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}

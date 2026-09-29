namespace Atlas.Communities.Communities;

/// <summary>Identifies a Community-owned aggregate.</summary>
public readonly record struct CommunityId
{
    public Guid Value { get; }

    public CommunityId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("A community ID cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public static CommunityId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}

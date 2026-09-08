namespace Atlas.Content.Documents;

/// <summary>Identifies a document within the Content boundary.</summary>
public sealed record DocumentId(Guid Value)
{
    /// <summary>Creates a new identifier.</summary>
    public static DocumentId New()
    {
        return new DocumentId(Guid.NewGuid());
    }

    /// <summary>Returns the identifier's display value.</summary>
    public override string ToString()
    {
        return Value.ToString();
    }
}
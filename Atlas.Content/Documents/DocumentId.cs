namespace Atlas.Content.Documents;

public sealed record DocumentId(Guid Value)
{
    public static DocumentId New()
    {
        return new DocumentId(Guid.NewGuid());
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
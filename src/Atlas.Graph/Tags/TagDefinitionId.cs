namespace Atlas.Graph.Tags;

/// <summary>Identifies a reusable tag definition within Graph.</summary>
public readonly record struct TagDefinitionId(Guid Value)
{
    /// <summary>Creates a new tag-definition identifier.</summary>
    public static TagDefinitionId New() => new(Guid.NewGuid());
}

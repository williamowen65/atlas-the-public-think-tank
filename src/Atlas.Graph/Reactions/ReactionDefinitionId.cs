namespace Atlas.Graph.Reactions;

/// <summary>Identifies a reusable reaction definition within Graph.</summary>
public readonly record struct ReactionDefinitionId(Guid Value)
{
    /// <summary>Creates a new tag-definition identifier.</summary>
    public static ReactionDefinitionId New() => new(Guid.NewGuid());
}

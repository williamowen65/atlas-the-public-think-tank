namespace Atlas.Graph.Tags;

/// <summary>Identifies one tag application to one node.</summary>
public readonly record struct NodeTagId(Guid Value)
{
    /// <summary>Creates a new node-tag identifier.</summary>
    public static NodeTagId New() => new(Guid.NewGuid());
}

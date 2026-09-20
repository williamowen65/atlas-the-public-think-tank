namespace Atlas.Graph.Reactions;

/// <summary>Identifies one tag application to one node.</summary>
public readonly record struct NodeReactionId(Guid Value)
{
    /// <summary>Creates a new node-reaction identifier.</summary>
    public static NodeReactionId New() => new(Guid.NewGuid());
}

namespace Atlas.Graph.Reactions;

/// <summary>Describes whether a node-reaction association remains active and why it ended.</summary>
public enum NodeReactionLifecycleState
{
    Active,
    Withdrawn,
    Superseded,
    AdministrativelyRemoved
}

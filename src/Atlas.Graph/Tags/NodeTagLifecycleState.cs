namespace Atlas.Graph.Tags;

/// <summary>Describes whether a node-tag association remains active and why it ended.</summary>
public enum NodeTagLifecycleState
{
    Active,
    Withdrawn,
    Superseded,
    AdministrativelyRemoved
}

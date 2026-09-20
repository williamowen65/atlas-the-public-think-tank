namespace Atlas.Graph.Reactions;

/// <summary>Identifies an auditable change in a node-reaction association.</summary>
public enum NodeReactionAuditAction
{
    Applied,
    DispositionChanged,
    Withdrawn,
    Superseded,
    AdministrativelyRemoved,
    LegacyImported
}

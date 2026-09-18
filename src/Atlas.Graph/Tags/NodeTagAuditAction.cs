namespace Atlas.Graph.Tags;

/// <summary>Identifies an auditable change in a node-tag association.</summary>
public enum NodeTagAuditAction
{
    Applied,
    DispositionChanged,
    Withdrawn,
    Superseded,
    AdministrativelyRemoved,
    LegacyImported
}

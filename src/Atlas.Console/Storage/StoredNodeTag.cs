using System.Text.Json.Serialization;

namespace Atlas.ConsoleApp.Storage;

/// <summary>Defines the data-only JSON representation of a node-specific tag association.</summary>
public sealed class StoredNodeTag
{
    public Guid Id { get; set; }
    public Guid NodeId { get; set; }
    public Guid TagDefinitionId { get; set; }
    public Guid AppliedByParticipantId { get; set; }
    public string? LifecycleState { get; set; }
    public string Disposition { get; set; } = "Community";
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsRemoved { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? RemovedAt { get; set; }
    public List<StoredNodeTagAuditEntry> AuditHistory { get; set; } = [];
}

/// <summary>Defines the data-only JSON representation of a node-tag audit entry.</summary>
public sealed class StoredNodeTagAuditEntry
{
    public string Action { get; set; } = string.Empty;
    public Guid? ActorParticipantId { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public string LifecycleState { get; set; } = string.Empty;
    public string Disposition { get; set; } = string.Empty;
    public Guid? RelatedNodeTagId { get; set; }
}

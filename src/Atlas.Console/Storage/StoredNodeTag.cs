namespace Atlas.ConsoleApp.Storage;

/// <summary>Defines the data-only JSON representation of a node-specific tag association.</summary>
public sealed class StoredNodeTag
{
    public Guid Id { get; set; }
    public Guid NodeId { get; set; }
    public Guid TagDefinitionId { get; set; }
    public Guid AppliedByParticipantId { get; set; }
    public bool IsRemoved { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? RemovedAt { get; set; }
}

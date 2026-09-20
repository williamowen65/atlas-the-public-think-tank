namespace Atlas.ConsoleApp.Storage;

public sealed class StoredCommunityNode
{
    public Guid CommunityId { get; set; }
    public Guid NodeId { get; set; }
    public Guid AssociatedByParticipantId { get; set; }
    public DateTimeOffset AssociatedAt { get; set; }
}

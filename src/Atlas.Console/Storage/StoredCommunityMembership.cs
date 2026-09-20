namespace Atlas.ConsoleApp.Storage;

public sealed class StoredCommunityMembership
{
    public Guid CommunityId { get; set; }
    public Guid ParticipantId { get; set; }
    public DateTimeOffset JoinedAt { get; set; }
    public DateTimeOffset? LeftAt { get; set; }
}

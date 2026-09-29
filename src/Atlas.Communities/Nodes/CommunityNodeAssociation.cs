using Atlas.Communities.Communities;

namespace Atlas.Communities.Nodes;

/// <summary>Associates one existing node with one community without changing Graph topology.</summary>
public sealed record CommunityNodeAssociation
{
    public CommunityId CommunityId { get; }
    public Guid NodeId { get; }
    public Guid AssociatedByParticipantId { get; }
    public DateTimeOffset AssociatedAt { get; }

    public CommunityNodeAssociation(CommunityId communityId, Guid nodeId, Guid associatedByParticipantId, DateTimeOffset associatedAt)
    {
        if (nodeId == Guid.Empty) throw new ArgumentException("A node ID is required.", nameof(nodeId));
        if (associatedByParticipantId == Guid.Empty) throw new ArgumentException("An associating participant is required.", nameof(associatedByParticipantId));
        CommunityId = communityId;
        NodeId = nodeId;
        AssociatedByParticipantId = associatedByParticipantId;
        AssociatedAt = associatedAt;
    }
}

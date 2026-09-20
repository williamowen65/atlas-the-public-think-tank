using Atlas.Communities.Communities;

namespace Atlas.Communities.Memberships;

/// <summary>Records one participant's current membership in one community.</summary>
public sealed class CommunityMembership
{
    public CommunityId CommunityId { get; }
    public Guid ParticipantId { get; }
    public DateTimeOffset JoinedAt { get; private set; }
    public DateTimeOffset? LeftAt { get; private set; }
    public bool IsActive => LeftAt is null;

    public CommunityMembership(CommunityId communityId, Guid participantId, DateTimeOffset joinedAt)
        : this(communityId, participantId, joinedAt, null)
    {
    }

    private CommunityMembership(CommunityId communityId, Guid participantId, DateTimeOffset joinedAt, DateTimeOffset? leftAt)
    {
        if (participantId == Guid.Empty) throw new ArgumentException("A membership participant is required.", nameof(participantId));
        if (leftAt < joinedAt) throw new ArgumentException("Leave time cannot precede join time.");
        CommunityId = communityId;
        ParticipantId = participantId;
        JoinedAt = joinedAt;
        LeftAt = leftAt;
    }

    public static CommunityMembership Reconstitute(CommunityId communityId, Guid participantId, DateTimeOffset joinedAt, DateTimeOffset? leftAt) =>
        new(communityId, participantId, joinedAt, leftAt);

    public void Join(DateTimeOffset joinedAt)
    {
        if (IsActive) return;
        JoinedAt = joinedAt;
        LeftAt = null;
    }

    public void Leave(DateTimeOffset leftAt)
    {
        if (!IsActive) return;
        if (leftAt < JoinedAt) throw new ArgumentException("Leave time cannot precede join time.");
        LeftAt = leftAt;
    }
}

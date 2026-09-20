using Atlas.Communities.Communities;

namespace Atlas.Communities.Memberships;

public interface ICommunityMembershipRepository
{
    IReadOnlyCollection<CommunityMembership> GetByCommunity(CommunityId communityId);
    IReadOnlyCollection<CommunityMembership> GetByParticipant(Guid participantId);
    CommunityMembership? Get(CommunityId communityId, Guid participantId);
    void Save(CommunityMembership membership);
}

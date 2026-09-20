using Atlas.Communities.Communities;

namespace Atlas.Communities.Nodes;

public interface ICommunityNodeRepository
{
    IReadOnlyCollection<CommunityNodeAssociation> GetByCommunity(CommunityId communityId);
    IReadOnlyCollection<CommunityNodeAssociation> GetByNode(Guid nodeId);
    void Add(CommunityNodeAssociation association);
    void Remove(CommunityId communityId, Guid nodeId);
}

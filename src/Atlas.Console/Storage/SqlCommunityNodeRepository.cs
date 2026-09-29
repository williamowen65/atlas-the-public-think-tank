using Atlas.Persistence;
using Atlas.Communities.Communities;
using Atlas.Communities.Nodes;

namespace Atlas.ConsoleApp.Storage;

/// <summary>Maps domain objects directly to EF Core rows in SQL Server.</summary>
public sealed class SqlCommunityNodeRepository(Func<AtlasDataContext>? contextFactory = null) : SqlRepository(contextFactory), ICommunityNodeRepository
{
    public IReadOnlyCollection<CommunityNodeAssociation> GetByCommunity(CommunityId id) => QueryRows<CommunityNodeRow>(row => row.CommunityId == id.Value).Select(ToDomain).ToList();
    public IReadOnlyCollection<CommunityNodeAssociation> GetByNode(Guid id) => QueryRows<CommunityNodeRow>(row => row.NodeId == id).Select(ToDomain).ToList();
    public void Add(CommunityNodeAssociation association)
    {
        if (FindRow<CommunityNodeRow>(association.CommunityId.Value, association.NodeId) is not null) return;
        SaveRow(new CommunityNodeRow { CommunityId = association.CommunityId.Value, NodeId = association.NodeId,
            AssociatedByParticipantId = association.AssociatedByParticipantId, AssociatedAt = association.AssociatedAt });
    }
    public void Remove(CommunityId communityId, Guid nodeId) => DeleteRow<CommunityNodeRow>(communityId.Value, nodeId);

    private static CommunityNodeAssociation ToDomain(CommunityNodeRow x) => new(new CommunityId(x.CommunityId), x.NodeId, x.AssociatedByParticipantId, x.AssociatedAt);
}

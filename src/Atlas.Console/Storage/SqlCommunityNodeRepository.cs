using Atlas.Persistence;
using Atlas.Communities.Communities;
using Atlas.Communities.Nodes;

namespace Atlas.ConsoleApp.Storage;

/// <summary>Maps domain objects directly to EF Core rows in SQL Server.</summary>
public sealed class SqlCommunityNodeRepository(Func<AtlasDataContext>? contextFactory = null) : SqlRepository(contextFactory), ICommunityNodeRepository
{

    // Read associations from either the community or Node side of the relationship.
    public IReadOnlyCollection<CommunityNodeAssociation> GetByCommunity(CommunityId id) => QueryRows<CommunityNodeRow>(row => row.CommunityId == id.Value).Select(ToDomain).ToList();
    public IReadOnlyCollection<CommunityNodeAssociation> GetByNode(Guid id) => QueryRows<CommunityNodeRow>(row => row.NodeId == id).Select(ToDomain).ToList();
    public void Add(CommunityNodeAssociation association) => Execute(() =>
    {
        ArgumentNullException.ThrowIfNull(association);

        // Validate the community, acting participant and Node before checking for an existing link.
        ReferenceValidation.Require(
            this,
            "Community",
            association.CommunityId.Value
        );
        ReferenceValidation.Require(
            this,
            "Participant",
            association.AssociatedByParticipantId
        );
        ReferenceValidation.Require(
            this,
            "Node",
            association.NodeId
        );

        // The composite key identifies the link. Adding it again succeeds without replacing its audit data.
        if (FindRow<CommunityNodeRow>(
            association.CommunityId.Value,
            association.NodeId
        ) is not null) return true;

        // Insert the new association, including who added it and when, within the operation transaction.
        SaveRow(new CommunityNodeRow {
            CommunityId = association.CommunityId.Value,
            NodeId = association.NodeId,
            AssociatedByParticipantId = association.AssociatedByParticipantId,
            AssociatedAt = association.AssociatedAt
        });
        return true;
    });

    // Delete only the addressed community/Node pair; other associations remain intact.
    public void Remove(
        CommunityId communityId,
        Guid nodeId
    ) => DeleteRow<CommunityNodeRow>(
        communityId.Value,
        nodeId
    );


    // Convert stored IDs and association metadata back into the domain representation.
    private static CommunityNodeAssociation ToDomain(CommunityNodeRow x) => new(
        new CommunityId(x.CommunityId),
        x.NodeId,
        x.AssociatedByParticipantId,
        x.AssociatedAt
    );
}

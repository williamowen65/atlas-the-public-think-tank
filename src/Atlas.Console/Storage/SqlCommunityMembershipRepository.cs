using Atlas.Persistence;
using Atlas.Communities.Communities;
using Atlas.Communities.Memberships;

namespace Atlas.ConsoleApp.Storage;

/// <summary>Maps domain objects directly to EF Core rows in SQL Server.</summary>
public sealed class SqlCommunityMembershipRepository(Func<AtlasDataContext>? contextFactory = null) : SqlRepository(contextFactory), ICommunityMembershipRepository
{

    // Read membership rows by either side of the relationship, then rebuild domain objects.
    public IReadOnlyCollection<CommunityMembership> GetByCommunity(CommunityId id) => QueryRows<CommunityMembershipRow>(row => row.CommunityId == id.Value).Select(ToDomain).ToList();
    public IReadOnlyCollection<CommunityMembership> GetByParticipant(Guid id) => QueryRows<CommunityMembershipRow>(row => row.ParticipantId == id).Select(ToDomain).ToList();

    // The community/participant pair is the composite key; a missing membership returns null.
    public CommunityMembership? Get(
        CommunityId communityId,
        Guid participantId
    ) => FindRow<CommunityMembershipRow>(
        communityId.Value,
        participantId
    ) is { } row ? ToDomain(row) : null;
    public void Save(CommunityMembership membership) => Execute(() =>
    {
        ArgumentNullException.ThrowIfNull(membership);

        // Active memberships require available references. Departed memberships only require existence,
        // so leaving can be persisted after a community or participant becomes inactive.
        ReferenceValidation.Require(
            this,
            "Community",
            membership.CommunityId.Value,
            membership.IsActive
        );
        ReferenceValidation.Require(
            this,
            "Participant",
            membership.ParticipantId,
            membership.IsActive
        );

        // Persist the join/leave timestamps in the current operation transaction.
        SaveRow(new CommunityMembershipRow {
            CommunityId = membership.CommunityId.Value,
            ParticipantId = membership.ParticipantId,
            JoinedAt = membership.JoinedAt,
            LeftAt = membership.LeftAt
        });
        return true;
    });


    // Reconstitution restores membership history and applies its domain validation.
    private static CommunityMembership ToDomain(CommunityMembershipRow x) => CommunityMembership.Reconstitute(
        new CommunityId(x.CommunityId),
        x.ParticipantId,
        x.JoinedAt,
        x.LeftAt
    );
}

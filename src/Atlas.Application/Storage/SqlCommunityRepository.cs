using Atlas.Persistence;
using Atlas.Communities.Communities;

namespace Atlas.Application.Storage;

/// <summary>Maps domain objects directly to EF Core rows in SQL Server.</summary>
public sealed class SqlCommunityRepository(Func<AtlasDataContext>? contextFactory = null) : SqlRepository(contextFactory), ICommunityRepository
{

    // Read stored communities and rebuild domain objects; a missing single ID returns null.
    public IReadOnlyCollection<Community> GetAll() => ReadRows<CommunityRow>().Select(ToDomain).ToList();
    public Community? GetById(CommunityId id) => FindRow<CommunityRow>(id.Value) is { } row ? ToDomain(row) : null;
    public void Save(Community community) => Execute(() =>
    {
        ArgumentNullException.ThrowIfNull(community);

        // A new community needs an active owner. Existing communities still require the owner to exist.
        ReferenceValidation.Require(
            this,
            "Participant",
            community.OwnerParticipantId,
            FindRow<CommunityRow>(community.Id.Value) is null
        );

        // Reject a case-insensitive name collision with another community before writing.
        if (ReadRows<CommunityRow>().Any(row => row.Id != community.Id.Value && string.Equals(
            row.Name,
            community.Name,
            StringComparison.OrdinalIgnoreCase
        )))
            throw new InvalidOperationException($"A community named '{community.Name}' already exists.");

        // Map the complete community state and save within the current operation transaction.
        SaveRow(new CommunityRow {
            Id = community.Id.Value,
            Name = community.Name,
            Description = community.Description,
            OwnerParticipantId = community.OwnerParticipantId,
            Status = community.Status.ToString(),
            CreatedAt = community.CreatedAt,
            UpdatedAt = community.UpdatedAt
        });
        return true;
    });


    // Parse the stored status and reconstitute identity, ownership and lifecycle timestamps.
    private static Community ToDomain(CommunityRow stored) => Community.Reconstitute(
        new CommunityId(stored.Id),
        stored.Name,
        stored.Description,
        stored.OwnerParticipantId,
        Enum.Parse<CommunityStatus>(
            stored.Status,
            true
        ),
        stored.CreatedAt,
        stored.UpdatedAt
    );
}

using Atlas.Persistence;
using Atlas.Communities.Communities;

namespace Atlas.ConsoleApp.Storage;

/// <summary>Maps domain objects directly to EF Core rows in SQL Server.</summary>
public sealed class SqlCommunityRepository(Func<AtlasDataContext>? contextFactory = null) : SqlRepository(contextFactory), ICommunityRepository
{
    public IReadOnlyCollection<Community> GetAll() => ReadRows<CommunityRow>().Select(ToDomain).ToList();
    public Community? GetById(CommunityId id) => FindRow<CommunityRow>(id.Value) is { } row ? ToDomain(row) : null;
    public void Save(Community community) => Execute(() =>
    {
        ArgumentNullException.ThrowIfNull(community);
        ReferenceValidation.Require(this, "Participant", community.OwnerParticipantId, FindRow<CommunityRow>(community.Id.Value) is null);
        if (ReadRows<CommunityRow>().Any(row => row.Id != community.Id.Value && string.Equals(row.Name, community.Name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"A community named '{community.Name}' already exists.");
        SaveRow(new CommunityRow { Id = community.Id.Value, Name = community.Name, Description = community.Description,
            OwnerParticipantId = community.OwnerParticipantId, Status = community.Status.ToString(), CreatedAt = community.CreatedAt, UpdatedAt = community.UpdatedAt });
        return true;
    });

    private static Community ToDomain(CommunityRow stored) => Community.Reconstitute(
        new CommunityId(stored.Id), stored.Name, stored.Description, stored.OwnerParticipantId,
        Enum.Parse<CommunityStatus>(stored.Status, true), stored.CreatedAt, stored.UpdatedAt);
}

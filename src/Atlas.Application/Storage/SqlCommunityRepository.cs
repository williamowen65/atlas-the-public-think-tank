using Atlas.Persistence;
using Atlas.Communities.Communities;

namespace Atlas.Application.Storage;

/// <summary>Maps domain objects directly to EF Core rows in SQL Server.</summary>
public sealed class SqlCommunityRepository(Func<AtlasDataContext>? contextFactory = null, CommunityCreationQuotaPolicy? quotaPolicy = null) : SqlRepository(contextFactory), ICommunityRepository, ICommunityCreationQuota
{

    private readonly CommunityCreationQuotaPolicy _quotaPolicy = quotaPolicy ?? CommunityCreationQuotaPolicy.Default;

    /// <summary>Serialize creation checks and writes for this participant, including across host processes.</summary>
    public T ExecuteCreation<T>(Guid participantId, Func<T> operation)
    {
        if (participantId == Guid.Empty) throw new ArgumentException("A participant is required.", nameof(participantId));
        return ExecuteWithLock(operation, $"AtlasCommunityCreation:{participantId:N}");
    }

    /// <summary>Advisory allowance; a slot is consumed only by a committed new-community save.</summary>
    public CommunityCreationAllowance GetCreationAllowance(Guid participantId)
    {
        if (participantId == Guid.Empty) throw new ArgumentException("A participant is required.", nameof(participantId));
        var now = AtlasTime.UtcNow;
        var history = QueryRows<CommunityCreationQuotaRow>(
            row => row.ParticipantId == participantId && row.OccurredAt > now - _quotaPolicy.Window,
            query => query.OrderByDescending(row => row.OccurredAt).Take(_quotaPolicy.Limit));
        return _quotaPolicy.Evaluate(history.Select(row => row.OccurredAt), now);
    }

    public void EnsureCreationAllowed(Guid participantId)
    {
        var allowance = GetCreationAllowance(participantId);
        if (allowance.Remaining == 0) throw new CommunityCreationQuotaExceededException(allowance);
    }

    // Read stored communities and rebuild domain objects; a missing single ID returns null.
    public IReadOnlyCollection<Community> GetAll() => ReadRows<CommunityRow>().Select(ToDomain).ToList();
    public Community? GetById(CommunityId id) => FindRow<CommunityRow>(id.Value) is { } row ? ToDomain(row) : null;
    public void Save(Community community)
    {
        ArgumentNullException.ThrowIfNull(community);
        ExecuteCreation(community.OwnerParticipantId, () =>
        {
            var isNew = FindRow<CommunityRow>(community.Id.Value) is null;
            if (isNew) EnsureCreationAllowed(community.OwnerParticipantId);

            // A new community needs an active owner. Existing communities still require the owner to exist.
            ReferenceValidation.Require(
                this,
                "Participant",
                community.OwnerParticipantId,
                isNew
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
        if (isNew)
            SaveRow(new CommunityCreationQuotaRow
            {
                Id = community.Id.Value, ParticipantId = community.OwnerParticipantId, OccurredAt = AtlasTime.UtcNow
            });
        return true;
        });
    }


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

using System.Text.Json;
using Atlas.Communities.Communities;
using Atlas.Communities.Memberships;

namespace Atlas.ConsoleApp.Storage;

public sealed class JsonCommunityMembershipRepository : ICommunityMembershipRepository
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };
    public JsonCommunityMembershipRepository(string filePath) => _filePath = filePath;

    public IReadOnlyCollection<CommunityMembership> GetByCommunity(CommunityId id) => Read().Where(x => x.CommunityId == id.Value).Select(ToDomain).ToList();
    public IReadOnlyCollection<CommunityMembership> GetByParticipant(Guid id) => Read().Where(x => x.ParticipantId == id).Select(ToDomain).ToList();
    public CommunityMembership? Get(CommunityId communityId, Guid participantId)
    {
        var stored = Read().SingleOrDefault(x => x.CommunityId == communityId.Value && x.ParticipantId == participantId);
        return stored is null ? null : ToDomain(stored);
    }

    public void Save(CommunityMembership membership)
    {
        var stored = Read();
        var replacement = new StoredCommunityMembership
        {
            CommunityId = membership.CommunityId.Value,
            ParticipantId = membership.ParticipantId,
            JoinedAt = membership.JoinedAt,
            LeftAt = membership.LeftAt
        };
        var index = stored.FindIndex(x => x.CommunityId == replacement.CommunityId && x.ParticipantId == replacement.ParticipantId);
        if (index >= 0) stored[index] = replacement; else stored.Add(replacement);
        JsonStorage.Write(_filePath, stored, _options);
    }

    private List<StoredCommunityMembership> Read() => JsonStorage.Read<List<StoredCommunityMembership>>(_filePath, _options) ?? [];
    private static CommunityMembership ToDomain(StoredCommunityMembership x) => CommunityMembership.Reconstitute(new CommunityId(x.CommunityId), x.ParticipantId, x.JoinedAt, x.LeftAt);
}

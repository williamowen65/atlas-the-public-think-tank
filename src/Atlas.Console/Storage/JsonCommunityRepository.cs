using System.Text.Json;
using Atlas.Communities.Communities;

namespace Atlas.ConsoleApp.Storage;

public sealed class JsonCommunityRepository : ICommunityRepository
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    public JsonCommunityRepository(string filePath) => _filePath = filePath;

    public IReadOnlyCollection<Community> GetAll() => Read().Select(ToDomain).ToList();

    public Community? GetById(CommunityId id)
    {
        var stored = Read().SingleOrDefault(item => item.Id == id.Value);
        return stored is null ? null : ToDomain(stored);
    }

    public void Save(Community community)
    {
        var stored = Read();
        if (stored.Any(item => item.Id != community.Id.Value && string.Equals(item.Name, community.Name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"A community named '{community.Name}' already exists.");
        }

        var replacement = new StoredCommunity
        {
            Id = community.Id.Value,
            Name = community.Name,
            Description = community.Description,
            OwnerParticipantId = community.OwnerParticipantId,
            Status = community.Status.ToString(),
            CreatedAt = community.CreatedAt,
            UpdatedAt = community.UpdatedAt
        };
        var index = stored.FindIndex(item => item.Id == community.Id.Value);
        if (index >= 0) stored[index] = replacement; else stored.Add(replacement);
        Write(stored);
    }

    private List<StoredCommunity> Read() => JsonStorage.Read<List<StoredCommunity>>(_filePath, _options) ?? [];
    private void Write(List<StoredCommunity> items) => JsonStorage.Write(_filePath, items, _options);

    private static Community ToDomain(StoredCommunity stored) => Community.Reconstitute(
        new CommunityId(stored.Id), stored.Name, stored.Description, stored.OwnerParticipantId,
        Enum.Parse<CommunityStatus>(stored.Status, true), stored.CreatedAt, stored.UpdatedAt);
}

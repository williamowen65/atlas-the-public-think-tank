using System.Text.Json;
using Atlas.Communities.Communities;
using Atlas.Communities.Nodes;

namespace Atlas.ConsoleApp.Storage;

public sealed class JsonCommunityNodeRepository : ICommunityNodeRepository
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };
    public JsonCommunityNodeRepository(string filePath) => _filePath = filePath;

    public IReadOnlyCollection<CommunityNodeAssociation> GetByCommunity(CommunityId id) => Read().Where(x => x.CommunityId == id.Value).Select(ToDomain).ToList();
    public IReadOnlyCollection<CommunityNodeAssociation> GetByNode(Guid id) => Read().Where(x => x.NodeId == id).Select(ToDomain).ToList();

    public void Add(CommunityNodeAssociation association)
    {
        var stored = Read();
        if (stored.Any(x => x.CommunityId == association.CommunityId.Value && x.NodeId == association.NodeId)) return;
        stored.Add(new StoredCommunityNode
        {
            CommunityId = association.CommunityId.Value,
            NodeId = association.NodeId,
            AssociatedByParticipantId = association.AssociatedByParticipantId,
            AssociatedAt = association.AssociatedAt
        });
        JsonStorage.Write(_filePath, stored, _options);
    }

    public void Remove(CommunityId communityId, Guid nodeId)
    {
        var stored = Read();
        stored.RemoveAll(x => x.CommunityId == communityId.Value && x.NodeId == nodeId);
        JsonStorage.Write(_filePath, stored, _options);
    }

    private List<StoredCommunityNode> Read() => JsonStorage.Read<List<StoredCommunityNode>>(_filePath, _options) ?? [];
    private static CommunityNodeAssociation ToDomain(StoredCommunityNode x) => new(new CommunityId(x.CommunityId), x.NodeId, x.AssociatedByParticipantId, x.AssociatedAt);
}

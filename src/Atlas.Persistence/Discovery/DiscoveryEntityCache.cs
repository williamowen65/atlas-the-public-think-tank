using System.Text.Json;
using Atlas.Discovery;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Persistence.Discovery;

internal static class DiscoveryEntityCache
{
    private static readonly JsonSerializerOptions WebJson = new(JsonSerializerDefaults.Web);

    internal sealed record Manifest(Guid NodeId, Guid? DocumentId, string TitleVersion, BlockVersion[] Blocks);
    internal sealed record BlockVersion(Guid Id, string Version);
    internal sealed record Payload(string Version, string Text, string? PreviewJson);

    internal sealed class VersionBatch
    {
        public long PageVersion { get; set; }
        public long DetailVersion { get; set; }
        public string EntitiesJson { get; set; } = "[]";
    }

    internal sealed class BlockPayload
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = "";
        public string? PreviewJson { get; set; }
    }

    internal static VersionBatch ReadVersions(AtlasDataContext db, Guid[] ids, bool checkPage = true)
    {
        var tag = checkPage ? "-- Discovery revision check\n" : "-- Discovery entity versions for rebuilt page\n";
        var sql = tag + SqlDiscoveryReadQuery.ReadResource("DiscoveryEntityVersions.sql");
        return db.Database.SqlQueryRaw<VersionBatch>(
            sql, new SqlParameter("@ids", JsonSerializer.Serialize(ids))).AsEnumerable().Single();
    }

    internal static Dictionary<Guid, DiscoveryCard> Assemble(
        AtlasDataContext db, DiscoveryCache cache, VersionBatch batch,
        Dictionary<Guid, DiscoveryCard> cards, List<(string Key, Payload Value)> writes)
    {
        var manifests = JsonSerializer.Deserialize<Manifest[]>(batch.EntitiesJson)!
            .Where(node => cards.ContainsKey(node.NodeId)).ToArray();
        var titles = manifests.ToDictionary(node => node.NodeId, node => node.TitleVersion);
        var blocks = manifests.SelectMany(node => node.Blocks).DistinctBy(block => block.Id)
            .ToDictionary(block => block.Id, block => block.Version);
        string TitleKey(Guid id) => cache.ExternalKey($"title-v1:{id:N}");
        string BlockKey(Guid id) => cache.ExternalKey($"block-v1:{id:N}");
        var requested = titles.Select(title => (Key: TitleKey(title.Key), Version: title.Value))
            .Concat(blocks.Select(block => (Key: BlockKey(block.Key), Version: block.Value))).ToArray();
        var hits = cache.External!.ReadEntries<Payload>(requested.Select(piece => piece.Key).ToArray());
        var pieces = new Dictionary<string, Payload>();
        for (var index = 0; index < requested.Length; index++)
        {
            var piece = requested[index];
            if (hits[index]?.Value is { } value && value.Version == piece.Version)
                pieces.Add(piece.Key, value);
        }

        void Add(string key, Payload value)
        {
            pieces.Add(key, value);
            writes.Add((key, value));
        }

        var missingTitles = titles.Keys.Where(id => !pieces.ContainsKey(TitleKey(id))).ToArray();
        if (missingTitles.Length > 0)
        {
            var rows = db.NodeRows.AsNoTracking().Where(row => missingTitles.Contains(row.Id))
                .Select(row => new { row.Id, row.Title }).TagWith("Discovery missing titles").ToList();
            foreach (var row in rows)
                Add(TitleKey(row.Id), new(titles[row.Id], row.Title, null));
        }

        var missingBlocks = blocks.Keys.Where(id => !pieces.ContainsKey(BlockKey(id))).ToArray();
        if (missingBlocks.Length > 0)
        {
            var sql = SqlDiscoveryReadQuery.ReadResource("DiscoveryEntityBlocks.sql");
            var rows = db.Database.SqlQueryRaw<BlockPayload>(
                sql, new SqlParameter("@ids", JsonSerializer.Serialize(missingBlocks))).ToList();
            foreach (var row in rows)
                Add(BlockKey(row.Id), new(blocks[row.Id], row.Description, row.PreviewJson));
        }

        var assembled = new Dictionary<Guid, DiscoveryCard>();
        foreach (var node in manifests)
        {
            var content = node.Blocks.Select(block => pieces[BlockKey(block.Id)]).ToArray();
            var preview = "[" + string.Join(",", content.Select(block => block.PreviewJson)
                .OfType<string>().Take(8)) + "]";
            assembled.Add(node.NodeId, cards[node.NodeId] with
            {
                Title = pieces[TitleKey(node.NodeId)].Text,
                Description = node.DocumentId is null ? "Description document not found."
                    : content.Length == 0 ? "No description has been provided."
                    : string.Join("\n\n", content.Select(block => block.Text)),
                PreviewBlocks = preview.Length <= 48 * 1024
                    ? JsonSerializer.Deserialize<DiscoveryPreviewBlock[]>(preview, WebJson) ?? []
                    : []
            });
        }
        return assembled;
    }
}

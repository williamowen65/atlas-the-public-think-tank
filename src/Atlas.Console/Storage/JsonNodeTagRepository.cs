using System.Text.Json;
using Atlas.Graph.Nodes;
using Atlas.Graph.Tags;

namespace Atlas.ConsoleApp.Storage;

/// <summary>Persists Graph node-tag associations in a dedicated JSON file.</summary>
public sealed class JsonNodeTagRepository : INodeTagRepository
{
    private readonly string _filePath;
    private readonly object _gate = new();
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    /// <summary>Initializes the adapter for its dedicated data file.</summary>
    public JsonNodeTagRepository(string filePath)
    {
        _filePath = filePath;
    }

    /// <summary>Loads all associations, including removed history.</summary>
    public IReadOnlyCollection<NodeTag> GetAll()
    {
        lock (_gate)
        {
            return ReadStored().Select(ToDomain).ToList();
        }
    }

    /// <summary>Loads an association by identifier.</summary>
    public NodeTag? GetById(NodeTagId id)
    {
        lock (_gate)
        {
            var stored = ReadStored().SingleOrDefault(item => item.Id == id.Value);
            return stored is null ? null : ToDomain(stored);
        }
    }

    /// <summary>Loads active associations for one node.</summary>
    public IReadOnlyCollection<NodeTag> GetActiveForNode(NodeId nodeId)
    {
        lock (_gate)
        {
            return ReadStored()
                .Where(item => item.NodeId == nodeId.Value && ResolveLifecycle(item) == NodeTagLifecycleState.Active)
                .Select(ToDomain)
                .ToList();
        }
    }

    /// <summary>Loads the active association for one node and definition.</summary>
    public NodeTag? GetActive(NodeId nodeId, TagDefinitionId tagDefinitionId)
    {
        lock (_gate)
        {
            var stored = ReadStored().SingleOrDefault(item =>
                item.NodeId == nodeId.Value &&
                item.TagDefinitionId == tagDefinitionId.Value &&
                ResolveLifecycle(item) == NodeTagLifecycleState.Active);
            return stored is null ? null : ToDomain(stored);
        }
    }

    /// <summary>Saves an association while preventing duplicate active applications.</summary>
    public void Save(NodeTag nodeTag)
    {
        lock (_gate)
        {
            var stored = ReadStored();

            if (!nodeTag.IsRemoved && stored.Any(item =>
                    item.Id != nodeTag.Id.Value &&
                    item.NodeId == nodeTag.NodeId.Value &&
                    item.TagDefinitionId == nodeTag.TagDefinitionId.Value &&
                    ResolveLifecycle(item) == NodeTagLifecycleState.Active))
            {
                throw new InvalidOperationException(
                    "That tag is already applied to this node.");
            }

            var index = stored.FindIndex(item => item.Id == nodeTag.Id.Value);
            var replacement = ToStorage(nodeTag);

            if (index >= 0)
            {
                stored[index] = replacement;
            }
            else
            {
                stored.Add(replacement);
            }

            WriteStored(stored);
        }
    }

    private List<StoredNodeTag> ReadStored()
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        var json = File.ReadAllText(_filePath);
        return string.IsNullOrWhiteSpace(json)
            ? []
            : JsonSerializer.Deserialize<List<StoredNodeTag>>(json, _jsonOptions) ?? [];
    }

    private void WriteStored(List<StoredNodeTag> nodeTags)
    {
        var directory = Path.GetDirectoryName(_filePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(
            _filePath,
            JsonSerializer.Serialize(nodeTags, _jsonOptions));
    }

    private static StoredNodeTag ToStorage(NodeTag nodeTag) => new()
    {
        Id = nodeTag.Id.Value,
        NodeId = nodeTag.NodeId.Value,
        TagDefinitionId = nodeTag.TagDefinitionId.Value,
        AppliedByParticipantId = nodeTag.AppliedByParticipantId,
        LifecycleState = nodeTag.LifecycleState.ToString(),
        Disposition = nodeTag.Disposition.ToString(),
        CreatedAt = nodeTag.CreatedAt,
        RemovedAt = nodeTag.RemovedAt,
        AuditHistory = nodeTag.AuditHistory
            .Select(entry => new StoredNodeTagAuditEntry
            {
                Action = entry.Action.ToString(),
                ActorParticipantId = entry.ActorParticipantId,
                OccurredAt = entry.OccurredAt,
                LifecycleState = entry.LifecycleState.ToString(),
                Disposition = entry.Disposition.ToString(),
                RelatedNodeTagId = entry.RelatedNodeTagId?.Value
            })
            .ToList()
    };

    private static NodeTag ToDomain(StoredNodeTag stored) =>
        NodeTag.Reconstitute(
            new NodeTagId(stored.Id),
            new NodeId(stored.NodeId),
            new TagDefinitionId(stored.TagDefinitionId),
            stored.AppliedByParticipantId,
            ResolveLifecycle(stored),
            Enum.Parse<NodeTagDisposition>(stored.Disposition),
            stored.CreatedAt,
            stored.RemovedAt,
            stored.AuditHistory.Select(entry =>
                new NodeTagAuditEntry(
                    Enum.Parse<NodeTagAuditAction>(entry.Action),
                    entry.ActorParticipantId,
                    entry.OccurredAt,
                    Enum.Parse<NodeTagLifecycleState>(entry.LifecycleState),
                    Enum.Parse<NodeTagDisposition>(entry.Disposition),
                    entry.RelatedNodeTagId.HasValue
                        ? new NodeTagId(entry.RelatedNodeTagId.Value)
                        : null)));

    private static NodeTagLifecycleState ResolveLifecycle(StoredNodeTag stored)
    {
        if (!string.IsNullOrWhiteSpace(stored.LifecycleState))
        {
            return Enum.Parse<NodeTagLifecycleState>(stored.LifecycleState);
        }

        return stored.IsRemoved == true
            ? NodeTagLifecycleState.Withdrawn
            : NodeTagLifecycleState.Active;
    }
}

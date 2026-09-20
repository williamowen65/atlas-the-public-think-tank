using System.Text.Json;
using Atlas.Graph.Nodes;
using Atlas.Graph.Reactions;

namespace Atlas.ConsoleApp.Storage;

/// <summary>Persists Graph node-reaction associations in a dedicated JSON file.</summary>
public sealed class JsonNodeReactionRepository : INodeReactionRepository
{
    private readonly string _filePath;
    private readonly object _gate = new();
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    /// <summary>Initializes the adapter for its dedicated data file.</summary>
    public JsonNodeReactionRepository(string filePath)
    {
        _filePath = filePath;
    }

    /// <summary>Loads all associations, including removed history.</summary>
    public IReadOnlyCollection<NodeReaction> GetAll()
    {
        lock (_gate)
        {
            return ReadStored().Select(ToDomain).ToList();
        }
    }

    /// <summary>Loads an association by identifier.</summary>
    public NodeReaction? GetById(NodeReactionId id)
    {
        lock (_gate)
        {
            var stored = ReadStored().SingleOrDefault(item => item.Id == id.Value);
            return stored is null ? null : ToDomain(stored);
        }
    }

    /// <summary>Loads active associations for one node.</summary>
    public IReadOnlyCollection<NodeReaction> GetActiveForNode(NodeId nodeId)
    {
        lock (_gate)
        {
            return ReadStored()
                .Where(item =>
                    item.NodeId == nodeId.Value &&
                    ParseLifecycle(item) == NodeReactionLifecycleState.Active)
                .Select(ToDomain)
                .ToList();
        }
    }

    /// <summary>Loads the active association for one node and definition.</summary>
    public NodeReaction? GetActive(NodeId nodeId, ReactionDefinitionId tagDefinitionId)
    {
        lock (_gate)
        {
            var stored = ReadStored().SingleOrDefault(item =>
                item.NodeId == nodeId.Value &&
                item.ReactionDefinitionId == tagDefinitionId.Value &&
                ParseLifecycle(item) == NodeReactionLifecycleState.Active);
            return stored is null ? null : ToDomain(stored);
        }
    }

    /// <summary>Saves an association while preventing duplicate active applications.</summary>
    public void Save(NodeReaction nodeTag)
    {
        lock (_gate)
        {
            var stored = ReadStored();

            if (!nodeTag.IsRemoved && stored.Any(item =>
                    item.Id != nodeTag.Id.Value &&
                    item.NodeId == nodeTag.NodeId.Value &&
                    item.ReactionDefinitionId == nodeTag.ReactionDefinitionId.Value &&
                    ParseLifecycle(item) == NodeReactionLifecycleState.Active))
            {
                throw new InvalidOperationException(
                    "That reaction is already applied to this node.");
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

    private List<StoredNodeReaction> ReadStored()
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        var json = File.ReadAllText(_filePath);
        return string.IsNullOrWhiteSpace(json)
            ? []
            : JsonSerializer.Deserialize<List<StoredNodeReaction>>(json, _jsonOptions) ?? [];
    }

    private void WriteStored(List<StoredNodeReaction> nodeTags)
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

    private static StoredNodeReaction ToStorage(NodeReaction nodeTag) => new()
    {
        Id = nodeTag.Id.Value,
        NodeId = nodeTag.NodeId.Value,
        ReactionDefinitionId = nodeTag.ReactionDefinitionId.Value,
        AppliedByParticipantId = nodeTag.AppliedByParticipantId,
        LifecycleState = nodeTag.LifecycleState.ToString(),
        Disposition = nodeTag.Disposition.ToString(),
        CreatedAt = nodeTag.CreatedAt,
        RemovedAt = nodeTag.RemovedAt,
        AuditHistory = nodeTag.AuditHistory
            .Select(entry => new StoredNodeReactionAuditEntry
            {
                Action = entry.Action.ToString(),
                ActorParticipantId = entry.ActorParticipantId,
                OccurredAt = entry.OccurredAt,
                LifecycleState = entry.LifecycleState.ToString(),
                Disposition = entry.Disposition.ToString(),
                RelatedNodeReactionId = entry.RelatedNodeReactionId?.Value
            })
            .ToList()
    };

    private static NodeReaction ToDomain(StoredNodeReaction stored) =>
        NodeReaction.Reconstitute(
            new NodeReactionId(stored.Id),
            new NodeId(stored.NodeId),
            new ReactionDefinitionId(stored.ReactionDefinitionId),
            stored.AppliedByParticipantId,
            ParseLifecycle(stored),
            Enum.Parse<NodeReactionDisposition>(stored.Disposition),
            stored.CreatedAt,
            stored.RemovedAt,
            stored.AuditHistory.Select(entry =>
                new NodeReactionAuditEntry(
                    Enum.Parse<NodeReactionAuditAction>(entry.Action),
                    entry.ActorParticipantId,
                    entry.OccurredAt,
                    Enum.Parse<NodeReactionLifecycleState>(entry.LifecycleState),
                    Enum.Parse<NodeReactionDisposition>(entry.Disposition),
                    entry.RelatedNodeReactionId.HasValue
                        ? new NodeReactionId(entry.RelatedNodeReactionId.Value)
                        : null)));

    private static NodeReactionLifecycleState ParseLifecycle(StoredNodeReaction stored) =>
        Enum.Parse<NodeReactionLifecycleState>(stored.LifecycleState);
}

using System.Text.Json;
using Atlas.Graph.Nodes.NodeTypes;

namespace Atlas.ConsoleApp.Storage;

/// <summary>Persists Graph node-type definitions as JSON and reconstitutes them as domain objects.</summary>
public sealed class SerializedNodeTypeRepository : INodeTypeRepository
{
    private readonly string _filePath;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    /// <summary>Initializes the JSON adapter and ensures its backing file is available.</summary>
    public SerializedNodeTypeRepository(string filePath)
    {
        _filePath = filePath;
    }

    /// <summary>Loads all persisted domain objects.</summary>
    public IReadOnlyCollection<NodeTypeDefinition> GetAll()
    {
        return ReadStoredTypes()
            .Select(ToDomain)
            .ToList();
    }

    /// <summary>Loads a domain object by its boundary-owned identifier.</summary>
    public NodeTypeDefinition? GetById(NodeTypeId id)
    {
        var storedType = ReadStoredTypes()
            .SingleOrDefault(type => type.Id == id.Value);

        return storedType is null ? null : ToDomain(storedType);
    }

    /// <summary>Persists the current domain-object state.</summary>
    public void Save(NodeTypeDefinition nodeType)
    {
        var storedTypes = ReadStoredTypes();

        var existingIndex = storedTypes.FindIndex(
            type => type.Id == nodeType.Id.Value);

        var replacement = ToStorage(nodeType);

        if (existingIndex >= 0)
        {
            storedTypes[existingIndex] = replacement;
        }
        else
        {
            storedTypes.Add(replacement);
        }

        WriteStoredTypes(storedTypes);
    }

    /// <summary>Reads node-type persistence records from a serialized collection.</summary>
    private List<StoredNodeType> ReadStoredTypes()
    {
        if (!SqlStorage.Exists(_filePath))
        {
            return [];
        }

        var json = SqlStorage.ReadText(_filePath);

        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<StoredNodeType>>(
                   json,
                   _jsonOptions)
               ?? [];
    }

    /// <summary>Writes node-type persistence records to a serialized collection.</summary>
    private void WriteStoredTypes(List<StoredNodeType> nodeTypes)
    {

        var json = JsonSerializer.Serialize(nodeTypes, _jsonOptions);
        SqlStorage.WriteText(_filePath, json);
    }

    /// <summary>Maps a domain object to its data-only persistence representation.</summary>
    private static StoredNodeType ToStorage(
        NodeTypeDefinition nodeType)
    {
        return new StoredNodeType
        {
            Id = nodeType.Id.Value,
            Name = nodeType.Name,
            Description = nodeType.Description,
            OwnerId = nodeType.OwnerId,
            IsSystemDefined = nodeType.IsSystemDefined,
            IsArchived = nodeType.IsArchived,
            AutoPluralize = nodeType.AutoPluralize,
            CreatedAt = nodeType.CreatedAt,
            UpdatedAt = nodeType.UpdatedAt
        };
    }

    /// <summary>Reconstitutes a domain object from its data-only persistence representation.</summary>
    private static NodeTypeDefinition ToDomain(
        StoredNodeType storedType)
    {
        return NodeTypeDefinition.Reconstitute(
            new NodeTypeId(storedType.Id),
            storedType.Name,
            storedType.Description,
            storedType.OwnerId,
            storedType.IsSystemDefined,
            storedType.IsArchived,
            storedType.CreatedAt,
            storedType.UpdatedAt,
            storedType.AutoPluralize ?? true);
    }
}

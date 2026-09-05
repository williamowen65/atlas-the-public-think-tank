using System.Text.Json;
using Atlas.Graph.Nodes.NodeTypes;

namespace Atlas.ConsoleApp.Storage;

public sealed class JsonNodeTypeRepository : INodeTypeRepository
{
    private readonly string _filePath;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public JsonNodeTypeRepository(string filePath)
    {
        _filePath = filePath;
    }

    public IReadOnlyCollection<NodeTypeDefinition> GetAll()
    {
        return ReadStoredTypes()
            .Select(ToDomain)
            .ToList();
    }

    public NodeTypeDefinition? GetById(NodeTypeId id)
    {
        var storedType = ReadStoredTypes()
            .SingleOrDefault(type => type.Id == id.Value);

        return storedType is null ? null : ToDomain(storedType);
    }

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

    private List<StoredNodeType> ReadStoredTypes()
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        var json = File.ReadAllText(_filePath);

        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<StoredNodeType>>(
                   json,
                   _jsonOptions)
               ?? [];
    }

    private void WriteStoredTypes(List<StoredNodeType> nodeTypes)
    {
        var directory = Path.GetDirectoryName(_filePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(nodeTypes, _jsonOptions);
        File.WriteAllText(_filePath, json);
    }

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
            CreatedAt = nodeType.CreatedAt,
            UpdatedAt = nodeType.UpdatedAt
        };
    }

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
            storedType.UpdatedAt);
    }
}

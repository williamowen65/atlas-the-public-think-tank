using System.Text.Json;
using System.Text.Json.Serialization;
using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;

namespace Atlas.ConsoleApp.Storage;

public sealed class JsonNodeRepository : INodeRepository
{
    private readonly string _filePath;
    private readonly INodeTypeRepository _nodeTypes;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public JsonNodeRepository(
        string filePath,
        INodeTypeRepository nodeTypes)
    {
        _filePath = filePath;
        _nodeTypes = nodeTypes;
    }

    public IReadOnlyCollection<Node> GetAll()
    {
        return ReadStoredNodes()
            .Select(ToDomain)
            .ToList();
    }

    public Node? GetById(NodeId id)
    {
        var storedNode = ReadStoredNodes()
            .SingleOrDefault(node => node.Id == id.Value);

        return storedNode is null ? null : ToDomain(storedNode);
    }

    public void Save(Node node)
    {
        var storedNodes = ReadStoredNodes();

        var existingIndex = storedNodes.FindIndex(
            storedNode => storedNode.Id == node.Id.Value);

        var replacement = ToStorage(node);

        if (existingIndex >= 0)
        {
            storedNodes[existingIndex] = replacement;
        }
        else
        {
            storedNodes.Add(replacement);
        }

        WriteStoredNodes(storedNodes);
    }

    private List<StoredNode> ReadStoredNodes()
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

        return JsonSerializer.Deserialize<List<StoredNode>>(
                   json,
                   _jsonOptions)
               ?? [];
    }

    private void WriteStoredNodes(List<StoredNode> nodes)
    {
        var directory = Path.GetDirectoryName(_filePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(nodes, _jsonOptions);
        File.WriteAllText(_filePath, json);
    }

    private static StoredNode ToStorage(Node node)
    {
        return new StoredNode
        {
            Id = node.Id.Value,
            Title = node.Title.Value,
            Description = node.Description.Value,
            TypeId = node.TypeId.Value,
            Type = null,
            Status = node.Status.ToString(),
            CreatedAt = node.CreatedAt,
            UpdatedAt = node.UpdatedAt
        };
    }

    private Node ToDomain(StoredNode storedNode)
    {
        var status = Enum.Parse<NodeStatus>(
            storedNode.Status,
            ignoreCase: true);

        return Node.Reconstitute(
            new NodeId(storedNode.Id),
            new NodeTitle(storedNode.Title),
            new NodeDescriptionId(storedNode.Description),
            ResolveTypeId(storedNode),
            status,
            storedNode.CreatedAt,
            storedNode.UpdatedAt);
    }

    private NodeTypeId ResolveTypeId(StoredNode storedNode)
    {
        if (storedNode.TypeId is Guid typeId &&
            typeId != Guid.Empty)
        {
            return new NodeTypeId(typeId);
        }

        var legacyType = _nodeTypes
            .GetAll()
            .SingleOrDefault(type =>
                string.Equals(
                    type.Name,
                    storedNode.Type,
                    StringComparison.OrdinalIgnoreCase));

        return legacyType?.Id
            ?? throw new InvalidDataException(
                $"The stored node type '{storedNode.Type}' was not found.");
    }
}

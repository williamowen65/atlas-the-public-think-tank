using System.Text.Json;
using Atlas.Graph.Nodes;

namespace Atlas.ConsoleApp.Storage;

public sealed class JsonNodeRepository : INodeRepository
{
    private readonly string _filePath;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public JsonNodeRepository(string filePath)
    {
        _filePath = filePath;
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

        return storedNode is null
            ? null
            : ToDomain(storedNode);
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
            Type = node.Type.ToString(),
            Status = node.Status.ToString(),
            CreatedAt = node.CreatedAt,
            UpdatedAt = node.UpdatedAt
        };
    }

    private static Node ToDomain(StoredNode storedNode)
    {
        var type = Enum.Parse<NodeType>(
            storedNode.Type,
            ignoreCase: true);

        var status = Enum.Parse<NodeStatus>(
            storedNode.Status,
            ignoreCase: true);

        return Node.Reconstitute(
            new NodeId(storedNode.Id),
            new NodeTitle(storedNode.Title),
            type,
            status,
            storedNode.CreatedAt,
            storedNode.UpdatedAt);
    }
}
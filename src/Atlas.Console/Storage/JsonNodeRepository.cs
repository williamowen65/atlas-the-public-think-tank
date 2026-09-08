using System.Text.Json;
using System.Text.Json.Serialization;
using Atlas.Content.Documents;
using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;

namespace Atlas.ConsoleApp.Storage;

/// <summary>Persists Graph nodes as JSON and reconstitutes them as domain aggregates.</summary>
public sealed class JsonNodeRepository : INodeRepository
{
    private readonly string _filePath;
    private readonly INodeTypeRepository _nodeTypes;
    private readonly IDocumentRepository _documents;
    private readonly NodeAuthorId _legacyAuthorId;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>Initializes the JSON adapter and ensures its backing file is available.</summary>
    public JsonNodeRepository(
        string filePath,
        INodeTypeRepository nodeTypes,
        IDocumentRepository documents,
        NodeAuthorId legacyAuthorId)
    {
        _filePath = filePath;
        _nodeTypes = nodeTypes;
        _documents = documents;
        _legacyAuthorId = legacyAuthorId;
    }

    /// <summary>Loads all persisted domain objects.</summary>
    public IReadOnlyCollection<Node> GetAll()
    {
        return ReadAndMigrateStoredNodes()
            .Select(ToDomain)
            .ToList();
    }

    /// <summary>Loads a domain object by its boundary-owned identifier.</summary>
    public Node? GetById(NodeId id)
    {
        var storedNode = ReadAndMigrateStoredNodes()
            .SingleOrDefault(node => node.Id == id.Value);

        return storedNode is null ? null : ToDomain(storedNode);
    }

    /// <summary>Persists the current domain-object state.</summary>
    public void Save(Node node)
    {
        var storedNodes = ReadAndMigrateStoredNodes();

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

    /// <summary>Loads stored nodes and applies the host's compatibility migration before reconstitution.</summary>
    private List<StoredNode> ReadAndMigrateStoredNodes()
    {
        var storedNodes = ReadStoredNodes();
        var migrated = false;

        foreach (var storedNode in storedNodes)
        {
            if (storedNode.DescriptionId is not Guid descriptionId ||
                descriptionId == Guid.Empty)
            {
                var document = new Document(
                    storedNode.Description ?? string.Empty,
                    storedNode.CreatedAt);

                _documents.Save(document);
                storedNode.DescriptionId = document.Id.Value;
                storedNode.Description = null;
                migrated = true;
            }

            if (storedNode.AuthorId is not Guid authorId ||
                authorId == Guid.Empty)
            {
                storedNode.AuthorId = _legacyAuthorId.Value;
                migrated = true;
            }

            if (storedNode.RequestedSubNodeTypeIds is null)
            {
                var commentType = _nodeTypes
                    .GetAll()
                    .Single(type =>
                        string.Equals(
                            type.Name,
                            "Comment",
                            StringComparison.OrdinalIgnoreCase));

                storedNode.RequestedSubNodeTypeIds =
                    [commentType.Id.Value];
                migrated = true;
            }
        }

        if (migrated)
        {
            WriteStoredNodes(storedNodes);
        }

        return storedNodes;
    }

    /// <summary>Reads node persistence records from JSON.</summary>
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

    /// <summary>Writes node persistence records to JSON.</summary>
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

    /// <summary>Maps a domain object to its data-only persistence representation.</summary>
    private static StoredNode ToStorage(Node node)
    {
        return new StoredNode
        {
            Id = node.Id.Value,
            Title = node.Title.Value,
            DescriptionId = node.DescriptionId.Value,
            Description = null,
            TypeId = node.TypeId.Value,
            Type = null,
            AuthorId = node.AuthorId.Value,
            RequestedSubNodeTypeIds = node.RequestedSubNodeTypes
                .Select(request => request.TypeId.Value)
                .ToList(),
            ParentNodeIds = node.ParentNodeIds
                .Select(parentId => parentId.Value)
                .ToList(),
            Status = node.Status.ToString(),
            CreatedAt = node.CreatedAt,
            UpdatedAt = node.UpdatedAt
        };
    }

    /// <summary>Reconstitutes a domain object from its data-only persistence representation.</summary>
    private Node ToDomain(StoredNode storedNode)
    {
        var status = Enum.Parse<NodeStatus>(
            storedNode.Status,
            ignoreCase: true);

        var descriptionId = storedNode.DescriptionId
            ?? throw new InvalidDataException(
                $"Node {storedNode.Id} has no description ID.");

        var authorId = storedNode.AuthorId
            ?? throw new InvalidDataException(
                $"Node {storedNode.Id} has no author ID.");

        return Node.Reconstitute(
            new NodeId(storedNode.Id),
            new NodeTitle(storedNode.Title),
            new NodeDescriptionId(descriptionId),
            ResolveTypeId(storedNode),
            new NodeAuthorId(authorId),
            status,
            (storedNode.RequestedSubNodeTypeIds ?? [])
                .Select(id => new NodeTypeId(id)),
            (storedNode.ParentNodeIds ?? [])
                .Select(id => new NodeId(id)),
            storedNode.CreatedAt,
            storedNode.UpdatedAt);
    }

    /// <summary>Resolves a stored node's current node-type identifier during migration.</summary>
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

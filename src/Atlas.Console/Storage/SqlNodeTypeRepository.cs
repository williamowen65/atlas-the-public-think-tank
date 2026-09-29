using Atlas.Persistence;
using Atlas.Graph.Nodes.NodeTypes;

namespace Atlas.ConsoleApp.Storage;

/// <summary>Maps domain objects directly to EF Core rows in SQL Server.</summary>
public sealed class SqlNodeTypeRepository(Func<AtlasDataContext>? contextFactory = null) : SqlRepository(contextFactory), INodeTypeRepository
{
    public IReadOnlyCollection<NodeTypeDefinition> GetAll() => ReadRows<NodeTypeRow>().Select(ToDomain).ToList();
    public NodeTypeDefinition? GetById(NodeTypeId id) => FindRow<NodeTypeRow>(id.Value) is { } row ? ToDomain(row) : null;
    public void Save(NodeTypeDefinition nodeType) => SaveRow(ToStorage(nodeType));

    private static NodeTypeRow ToStorage(
        NodeTypeDefinition nodeType)
    {
        return new NodeTypeRow
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
        NodeTypeRow storedType)
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

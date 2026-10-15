using Atlas.Persistence;
using Atlas.Graph.Nodes.NodeTypes;

namespace Atlas.Infrastructure.Storage;

/// <summary>Maps domain objects directly to EF Core rows in SQL Server.</summary>
public sealed class SqlNodeTypeRepository(Func<AtlasDataContext>? contextFactory = null) : SqlRepository(contextFactory), INodeTypeRepository
{

    // Read type definitions and rebuild their saved domain state.
    public IReadOnlyCollection<NodeTypeDefinition> GetAll() => ReadRows<NodeTypeRow>().Select(ToDomain).ToList();
    public NodeTypeDefinition? GetById(NodeTypeId id) => FindRow<NodeTypeRow>(id.Value) is { } row ? ToDomain(row) : null;
    public void Save(NodeTypeDefinition nodeType) => Execute(() =>
    {
        ArgumentNullException.ThrowIfNull(nodeType);

        // Custom types need a Participant owner. New custom types require that owner to be active;
        // system-defined types do not use this ownership check.
        if (!nodeType.IsSystemDefined)
        {
            if (!Guid.TryParse(
                nodeType.OwnerId,
                out var owner
            )) throw new ArgumentException("A custom type owner must identify a Participant.");
            ReferenceValidation.Require(
                this,
                "Participant",
                owner,
                FindRow<NodeTypeRow>(nodeType.Id.Value) is null
            );
        }

        // Save definition flags and timestamps inside the current operation transaction.
        SaveRow(ToStorage(nodeType));
        return true;
    });


    // Copy identity, ownership and presentation/lifecycle options into the SQL row.
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

        // Restore the saved definition. Older rows without AutoPluralize use the existing true default.
        return NodeTypeDefinition.Reconstitute(
            new NodeTypeId(storedType.Id),
            storedType.Name,
            storedType.Description,
            storedType.OwnerId,
            storedType.IsSystemDefined,
            storedType.IsArchived,
            storedType.CreatedAt,
            storedType.UpdatedAt,
            storedType.AutoPluralize ?? true
        );
    }
}

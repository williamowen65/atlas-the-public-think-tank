namespace Atlas.Graph.Nodes.NodeTypes;

/// <summary>Reusable custom type creation. SQL uniqueness remains authoritative under races.</summary>
public sealed class NodeTypeCreationService(INodeTypeRepository types)
{
    public NodeTypeDefinition? FindByName(string? name) => types.GetAll().FirstOrDefault(type =>
        string.Equals(type.Name, name?.Trim(), StringComparison.OrdinalIgnoreCase));

    public NodeTypeDefinition Create(string name, string description, string ownerId, DateTimeOffset at, bool autoPluralize = true) =>
        OperationBoundary.Execute(types, () =>
        {
            if (FindByName(name) is not null)
                throw new InvalidOperationException("The node type already exists. Select it from the known types instead.");
            var type = NodeTypeDefinition.CreateCustom(name, description, ownerId, at, autoPluralize);
            types.Save(type);
            return type;
        });
}

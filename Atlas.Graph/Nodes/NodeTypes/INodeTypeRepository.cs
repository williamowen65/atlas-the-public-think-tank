namespace Atlas.Graph.NodeTypes;

public interface INodeTypeRepository
{
    IReadOnlyCollection<NodeTypeDefinition> GetAll();

    NodeTypeDefinition? GetById(NodeTypeId id);

    void Save(NodeTypeDefinition nodeType);
}
namespace Atlas.Graph.Nodes;

public interface INodeRepository
{
    IReadOnlyCollection<Node> GetAll();

    Node? GetById(NodeId id);

    void Save(Node node);
}

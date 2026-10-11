namespace Atlas.Graph.Nodes;

/// <summary>Optional adapter capability for coordinating creation and parent relationship changes.</summary>
public interface INodeTopologyBoundary
{
    T ExecuteTopology<T>(Func<T> operation);
}

/// <summary>Coordinates topology checks and writes; other node edits use the ordinary operation boundary.</summary>
public static class NodeTopologyBoundary
{
    public static T Execute<T>(INodeRepository nodes, Func<T> operation) =>
        nodes is INodeTopologyBoundary boundary ? boundary.ExecuteTopology(operation) : OperationBoundary.Execute(nodes, operation);

    public static void Execute(INodeRepository nodes, Action operation) =>
        Execute(nodes, () => { operation(); return true; });
}

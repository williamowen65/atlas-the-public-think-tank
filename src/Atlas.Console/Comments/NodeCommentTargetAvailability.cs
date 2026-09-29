using Atlas.Comments.Comments;
using Atlas.Graph.Nodes;

namespace Atlas.ConsoleApp.Comments;

/// <summary>
/// Adapts Graph node state to the availability question owned by Comments.
/// </summary>
public sealed class NodeCommentTargetAvailability : ICommentTargetAvailability
{
    private readonly INodeRepository _nodes;

    public NodeCommentTargetAvailability(INodeRepository nodes) => _nodes = nodes;

    public bool IsAvailable(CommentTarget target)
    {
        if (!string.Equals(target.Kind, "Node", StringComparison.OrdinalIgnoreCase))
            return false;

        var node = _nodes.GetById(new NodeId(target.Id));
        return node is not null && node.Status == NodeStatus.Active;
    }
}

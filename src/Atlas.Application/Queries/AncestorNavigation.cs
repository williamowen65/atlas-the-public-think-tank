using Atlas.Discovery;
using Atlas.Graph.Nodes;

namespace Atlas.Application.Queries;

/// <summary>Bounded ancestry reads for any host; follows one route instead of expanding a tree.</summary>
public static class AncestorNavigation
{
    /// <summary>Loads up to three nearest ancestors, preserving visited IDs between successive pages.</summary>
    public static AncestorSegment ReadSegment(
        Node origin,
        INodeRepository nodes,
        HashSet<NodeId> visited,
        NodeId? firstParent = null)
    {
        var ancestors = new List<Node>();
        var current = origin;
        var alternatives = false;
        var cycle = false;
        visited.Add(origin.Id);
        for (var index = 0; index < BreadcrumbPage.PageSize; index++)
        {
            if (current.ParentNodeIds.Count == 0)
            {
                break;
            }
            alternatives |= current.ParentNodeIds.Count > 1;
            var id = index == 0 && firstParent is { } chosen ? chosen : current.ParentNodeIds.First();
            if (!current.ParentNodeIds.Contains(id))
            {
                throw new ArgumentException("The selected route must be an immediate parent.", nameof(firstParent));
            }
            if (!visited.Add(id))
            {
                cycle = true;
                break;
            }
            var parent = nodes.GetById(id);
            if (parent is null)
            {
                break;
            }
            ancestors.Add(parent);
            current = parent;
        }
        // Inspect IDs only; do not load a fourth ancestor just to display the continuation.
        var hasMore = !cycle && ancestors.Count == BreadcrumbPage.PageSize && current.ParentNodeIds.Count > 0;
        return new AncestorSegment(ancestors.AsReadOnly(), hasMore, alternatives, cycle);
    }
}

/// <summary>One route segment; continuation starts above the last displayed ancestor.</summary>
public sealed record AncestorSegment(
    IReadOnlyList<Node> Items,
    bool HasMore,
    bool HasAlternativeRoutes,
    bool CycleDetected);

namespace Atlas.Graph.Nodes;

/// <summary>Graph owns ancestry rules; the caller supplies repository facts inside its write transaction.</summary>
public static class GraphRelationshipValidation
{
    public static void EnsureAcyclic(NodeId childId, IEnumerable<NodeId> parents,
        Func<NodeId, IReadOnlyCollection<NodeId>?> getParents)
    {
        if (childId.Value == Guid.Empty) throw new ArgumentException("A child ID is required.");
        ArgumentNullException.ThrowIfNull(parents);
        ArgumentNullException.ThrowIfNull(getParents);
        var pending = new Stack<NodeId>(parents);
        var visited = new HashSet<NodeId>();
        while (pending.TryPop(out var candidate))
        {
            if (candidate.Value == Guid.Empty) throw new ArgumentException("A parent ID is required.");
            if (candidate == childId) throw new InvalidOperationException("The parent relationship would create a graph cycle.");
            if (!visited.Add(candidate)) continue;
            var ancestors = getParents(candidate) ?? throw new InvalidOperationException("A referenced parent does not exist.");
            foreach (var ancestor in ancestors) pending.Push(ancestor);
        }
    }
}

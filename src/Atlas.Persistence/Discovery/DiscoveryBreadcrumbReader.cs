using Atlas.Discovery;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Persistence.Discovery;

/// <summary>Reads at most three ancestor hops with EF; never enumerates every route to every root.</summary>
public static class DiscoveryBreadcrumbReader
{
    /// <summary>
    /// Batches the first segment for selected cards. Each hop chooses the earliest stored
    /// parent position. Alternative routes are indicated rather than recursively expanded.
    /// The caller's read transaction keeps all three hops in one committed view.
    /// </summary>
    public static BreadcrumbPage ReadPage(
        AtlasDataContext db,
        Guid cursorNodeId,
        IReadOnlyCollection<Guid>? visitedNodeIds = null) =>
        ReadFirstPages(db, [cursorNodeId], visitedNodeIds)[cursorNodeId];

    public static Dictionary<Guid, BreadcrumbPage> ReadFirstPages(
        AtlasDataContext db,
        Guid[] nodeIds,
        IReadOnlyCollection<Guid>? visitedNodeIds = null)
    {
        var items = nodeIds.ToDictionary(id => id, _ => new List<BreadcrumbItem>());
        var visited = nodeIds.ToDictionary(id => id, id => new HashSet<Guid>((visitedNodeIds ?? []).Append(id)));
        var alternatives = nodeIds.ToDictionary(id => id, _ => false);
        var cycles = nodeIds.ToDictionary(id => id, _ => false);
        var cursors = nodeIds.ToDictionary(id => id, id => id);
        var continuations = nodeIds.ToDictionary(id => id, _ => (Guid?)null);

        for (var depth = 0; depth < BreadcrumbPage.PageSize && cursors.Count > 0; depth++)
        {
            var ids = cursors.Values.Distinct().ToArray();
            // Fixed-depth EF joins replace recursive SQL. One row per active cursor,
            // at most the number of selected cards, leaves the database at each hop.
            var edges = db.NodeRows.AsNoTracking().Where(node => ids.Contains(node.Id))
                .Select(node => new
                {
                    NodeId = node.Id,
                    ParentId = node.Parents.OrderBy(parent => parent.Position)
                        .Select(parent => (Guid?)parent.ParentNodeId).FirstOrDefault(),
                    ParentCount = node.Parents.Count
                });
            var rows = (from edge in edges
                        join ancestor in db.NodeRows on edge.ParentId equals (Guid?)ancestor.Id into ancestors
                        from ancestor in ancestors.DefaultIfEmpty()
                        select new
                        {
                            edge.NodeId,
                            edge.ParentId,
                            edge.ParentCount,
                            Title = ancestor == null ? "Unknown" : ancestor.Title,
                            TypeName = ancestor == null ? "Unknown" : db.NodeTypeRows
                                .Where(type => type.Id == ancestor.TypeId).Select(type => type.Name).FirstOrDefault() ?? "Unknown",
                            HasParents = ancestor != null && ancestor.Parents.Any(),
                            PublicReason = ancestor == null ? null : db.ModerationCaseRows
                                .Where(report => report.NodeId == ancestor.Id && report.Status == 2 && report.VisibilityRestoredAt == null)
                                .OrderByDescending(report => report.DecidedAt).ThenBy(report => report.Id)
                                .Select(report => (int?)report.PublicReason).FirstOrDefault()
                        })
                .TagWith("Discovery breadcrumb hop")
                .ToDictionary(row => row.NodeId);

            var next = new Dictionary<Guid, Guid>();
            foreach (var (origin, cursor) in cursors)
            {
                if (!rows.TryGetValue(cursor, out var row) || row.ParentId is not { } parent)
                {
                    continue;
                }
                alternatives[origin] |= row.ParentCount > 1;
                if (!visited[origin].Add(parent))
                {
                    cycles[origin] = true;
                    continue;
                }
                var title = row.PublicReason is { } reason
                    ? "[Hidden by moderator: " + (reason switch
                    {
                        1 => "Spam", 2 => "Harassment", 3 => "Unsafe content", 4 => "Off topic", _ => "Other"
                    }) + "]"
                    : row.Title;
                items[origin].Add(new BreadcrumbItem(parent, row.TypeName + " · " + title));
                if (row.HasParents)
                {
                    next[origin] = parent;
                    if (depth == BreadcrumbPage.PageSize - 1)
                    {
                        continuations[origin] = parent;
                    }
                }
            }
            cursors = next;
        }

        return nodeIds.ToDictionary(id => id, id => new BreadcrumbPage(
            Array.AsReadOnly(items[id].ToArray()), continuations[id], alternatives[id], cycles[id]));
    }
}

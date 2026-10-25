using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using Atlas.Discovery;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Persistence.Discovery;

/// <summary>Filters and pages heterogeneous search hits in SQL, materializing only the selected page.</summary>
public sealed class SqlPublicSearch(Func<AtlasDataContext> open) : IPublicSearch
{
    public PublicSearchPage Search(DiscoveryQuery query, string scope = "all")
    {
        DiscoveryQueryValidation.Validate(query);
        scope = scope?.Trim().ToLowerInvariant() ?? "all";
        if (scope is not ("all" or "nodes" or "comments")) throw new ArgumentException("Use scope all, nodes or comments.");
        query = query with { SearchText = string.IsNullOrWhiteSpace(query.SearchText) ? null : query.SearchText.Trim() };
        try
        {
            using var db = open();
            // Filter-only searches keep their established node-only behavior.
            if (query.SearchText is null && scope != "comments")
            {
                var nodes = new SqlDiscoveryReadQuery(open).DiscoverPage(query);
                return new(nodes.Items.Select(n => new PublicSearchItem(n.NodeId, n.VoteCount, n.AverageVote, n.Card, null)).ToArray(), nodes.TotalCount, nodes.Page, nodes.PageSize);
            }
            if (query.SearchText is not null) DiscoveryFullTextSchema.EnsureAvailable(db, includeComments: true);
            using var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable);
            var hits = BuildHits(db, query, scope);
            var total = hits.Count();
            var offset = ((long)query.Page - 1) * IDiscoveryService.PageSize;
            var selected = offset >= total ? [] : hits.Skip((int)offset).Take(IDiscoveryService.PageSize).ToArray();
            var nodeIds = selected.Where(h => h.Kind == 0).Select(h => h.NodeId).ToArray();
            var cards = SqlDiscoveryReadQuery.ReadCards(db, nodeIds);
            var nodeScope = DiscoveryEfQuery.Build(db, query with { SearchText = null }, null, false);
            var aggregates = nodeScope.Where(n => nodeIds.Contains(n.NodeId)).ToDictionary(n => n.NodeId);
            var commentIds = selected.Where(h => h.Kind == 1).Select(h => h.Id).ToArray();
            var details = (from c in db.CommentRows.AsNoTracking()
                           join n in db.NodeRows.AsNoTracking() on c.TargetId equals n.Id
                           join a in db.ParticipantRows.AsNoTracking() on c.AuthorParticipantId equals a.Id
                           where commentIds.Contains(c.Id)
                           select new { c.Id, c.Body, c.CreatedAt, c.AuthorParticipantId, n.Title, a.DisplayName }).ToDictionary(c => c.Id);
            var items = selected.Select(h => h.Kind == 0
                ? new PublicSearchItem(h.NodeId, aggregates[h.NodeId].VoteCount, aggregates[h.NodeId].AverageVote, cards.GetValueOrDefault(h.NodeId), null)
                : new PublicSearchItem(h.NodeId, 0, null, null, new(h.Id, details[h.Id].Title, details[h.Id].AuthorParticipantId,
                    details[h.Id].DisplayName, details[h.Id].CreatedAt, Excerpt(details[h.Id].Body, query.SearchText ?? ""), h.CommentScore))).ToArray();
            transaction.Commit();
            return new(items, total, query.Page, IDiscoveryService.PageSize);
        }
        catch (SqlException error) { throw new SearchUnavailableException(error); }
    }

    private static IQueryable<Hit> BuildHits(AtlasDataContext db, DiscoveryQuery query, string scope)
    {
        IQueryable<Match> matches = query.SearchText is null
            ? db.CommentRows.Select(c => new Match { ResultId = c.Id, NodeId = c.TargetId, Kind = 1, Score = 0 })
            : db.Database.SqlQueryRaw<Match>(SqlDiscoveryReadQuery.ReadResource("PublicSearchMatches.sql"),
                new SqlParameter("@search", SqlDbType.NVarChar, 4000) { Value = query.SearchText });
        // The shared index contains exactly one document per node or comment.
        var matched = matches;
        var nodeScope = DiscoveryEfQuery.Build(db, query with { SearchText = null }, null, false);
        var nodeHits = from match in matched
                       join node in nodeScope on match.NodeId equals node.NodeId
                       where match.Kind == 0 && scope != "comments"
                       select new Hit { Id = match.ResultId, NodeId = node.NodeId, Kind = 0, Score = match.Score, CommentScore = 0, At = node.UpdatedAt };
        // Structural/voting filters concern the containing node; author/date concern the comment.
        var commentNodes = DiscoveryEfQuery.Build(db, query with { SearchText = null, AuthorParticipantId = null, CreatedFrom = null, CreatedThrough = null }, null, false);
        var comments = db.CommentRows.AsNoTracking().Where(c => c.TargetKind == "Node" && c.Status == "Active" && c.RemovedAt == null);
        if (query.AuthorParticipantId is { } author) comments = comments.Where(c => c.AuthorParticipantId == author);
        if (query.CreatedFrom is { } from) { var date = from.ToDateTime(TimeOnly.MinValue); comments = comments.Where(c => c.CreatedAt.Date >= date); }
        if (query.CreatedThrough is { } through) { var date = through.ToDateTime(TimeOnly.MinValue); comments = comments.Where(c => c.CreatedAt.Date <= date); }
        var commentHits = from match in matched
                          join comment in comments on match.ResultId equals comment.Id
                          join node in commentNodes on comment.TargetId equals node.NodeId
                          where match.Kind == 1 && scope != "nodes"
                          select new Hit { Id = comment.Id, NodeId = node.NodeId, Kind = 1, Score = match.Score, CommentScore = db.VoteRows.Where(v => v.TargetType == "Comment" && v.TargetId == comment.Id).Sum(v => (int?)v.Value) ?? 0, At = comment.CreatedAt };
        // Comment support adjusts text relevance by less than 25% in either direction.
        // With no text query, a neutral base ranks comment-only browsing by net support.
        return nodeHits.Concat(commentHits)
            .OrderByDescending(h => (h.Score == 0 ? 1.0 : h.Score) *
                (1.0 + 0.25 * h.CommentScore / (10.0 + Math.Abs(h.CommentScore))))
            .ThenBy(h => h.Kind).ThenByDescending(h => h.At).ThenBy(h => h.Id);
    }

    /// <summary>Resolve only the ancestor chain, retaining removed ancestors as blank placeholders.</summary>
    public IReadOnlyList<CommentPathItem> CommentPath(Guid nodeId, Guid commentId)
    {
        if (nodeId == Guid.Empty || commentId == Guid.Empty) throw new ArgumentException("Node and comment IDs are required.");
        using var db = open();
        using var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable);
        if (!db.NodeRows.Any(n => n.Id == nodeId) || db.ModerationCaseRows.Any(r => r.NodeId == nodeId && r.Status == 2 && r.VisibilityRestoredAt == null)) throw new KeyNotFoundException();
        var path = new List<CommentPathItem>();
        var seen = new HashSet<Guid>();
        Guid? id = commentId;
        while (id is { } next)
        {
            if (!seen.Add(next) || path.Count >= 128) throw new InvalidOperationException("Comment ancestry is unavailable.");
            var c = db.CommentRows.AsNoTracking().SingleOrDefault(c => c.Id == next && c.TargetKind == "Node" && c.TargetId == nodeId) ?? throw new KeyNotFoundException();
            var removed = c.Status != "Active" || c.RemovedAt != null;
            if (path.Count == 0 && removed) throw new KeyNotFoundException();
            path.Add(new(c.Id, c.TargetKind, c.TargetId, c.ParentCommentId, c.AuthorParticipantId, removed ? "" : c.Body, c.CreatedAt, c.UpdatedAt, removed));
            id = c.ParentCommentId;
        }
        path.Reverse();
        transaction.Commit();
        return path;
    }

    private static string Excerpt(string body, string search)
    {
        var index = -1;
        foreach (System.Text.RegularExpressions.Match term in Regex.Matches(search, @"[\p{L}\p{N}]+"))
        {
            var found = CultureInfo.InvariantCulture.CompareInfo.IndexOf(body, term.Value, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace);
            if (found >= 0 && (index < 0 || found < index)) index = found;
        }
        var start = Math.Max(0, index - 65);
        var count = Math.Min(240, body.Length - start);
        return (start > 0 ? "…" : "") + body.Substring(start, count) + (start + count < body.Length ? "…" : "");
    }
    public sealed class Match { public Guid ResultId { get; set; } public Guid NodeId { get; set; } public int Kind { get; set; } public int Score { get; set; } }
    private sealed class Hit { public int CommentScore { get; set; } public Guid Id { get; set; } public Guid NodeId { get; set; } public int Kind { get; set; } public int Score { get; set; } public DateTimeOffset At { get; set; } }
}

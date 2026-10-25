using System.Data;
using Atlas.Discovery;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Persistence.Discovery;

/// <summary>Builds a deferred EF query. Filtering, aggregates and ranking execute in the database.</summary>
internal static class DiscoveryEfQuery
{
    /// <summary>Applies optional filters before the caller counts or selects a page.</summary>
    internal static IQueryable<Candidate> Build(
        AtlasDataContext db,
        DiscoveryQuery request,
        IReadOnlyCollection<Guid>? authoredIds,
        bool includeHidden)
    {
        var nodes = db.NodeRows.AsNoTracking();
        if (request.ParentNodeId is { } parentId)
            nodes = nodes.Where(node => node.Parents.Any(link => link.ParentNodeId == parentId)
                && !db.ModerationCaseRows.Any(report => report.NodeId == parentId && report.Status == 2 && report.VisibilityRestoredAt == null));
        if (request.NodeId is { } nodeId)
            nodes = nodes.Where(node => node.Id == nodeId);
        if (authoredIds is not null)
        {
            var ids = authoredIds.ToArray();
            nodes = nodes.Where(node => ids.Contains(node.Id));
        }
        if (request.IsArchived is { } archived)
        {
            var status = archived ? "Archived" : "Active";
            nodes = nodes.Where(node => node.Status == status);
        }
        else if (!request.IncludeArchived)
        {
            nodes = nodes.Where(node => node.Status != "Archived");
        }
        if (request.NodeTypeId is { } type)
        {
            nodes = nodes.Where(node => node.TypeId == type);
        }
        if (request.AuthorParticipantId is { } author)
        {
            nodes = nodes.Where(node => node.AuthorId == author);
        }
        if (request.CommunityId is { } community)
        {
            nodes = nodes.Where(node => db.CommunityNodeRows.Any(
                link => link.NodeId == node.Id && link.CommunityId == community));
        }
        // One EXISTS predicate per distinct requested reaction implements “all selected”.
        // No membership join can multiply the node's vote aggregates.
        foreach (var reaction in request.ReactionDefinitionIds ?? [])
        {
            nodes = nodes.Where(node => db.NodeReactionRows.Any(link =>
                link.NodeId == node.Id && link.LifecycleState == "Active" &&
                link.ReactionDefinitionId == reaction));
        }
        if (request.CreatedFrom is { } from)
        {
            var date = from.ToDateTime(TimeOnly.MinValue);
            nodes = nodes.Where(node => node.CreatedAt.Date >= date);
        }
        if (request.CreatedThrough is { } through)
        {
            var date = through.ToDateTime(TimeOnly.MinValue);
            nodes = nodes.Where(node => node.CreatedAt.Date <= date);
        }

        var candidates = nodes.Select(node => new Candidate
        {
            NodeId = node.Id,
            Title = node.Title,
            ShortDescription = node.ShortDescription,
            UpdatedAt = node.UpdatedAt,
            Hidden = db.ModerationCaseRows.Any(report => report.NodeId == node.Id &&
                report.Status == 2 && report.VisibilityRestoredAt == null),
            VoteCount = db.VoteRows.Count(vote => vote.TargetType == "Node" && vote.TargetId == node.Id),
            AverageVote = db.VoteRows.Where(vote => vote.TargetType == "Node" && vote.TargetId == node.Id)
                .Select(vote => (double?)vote.Value).Average()
        });
        if (!includeHidden)
        {
            candidates = candidates.Where(node => !node.Hidden);
        }
        if (request.MinimumVoteCount is { } minimumCount)
        {
            candidates = candidates.Where(node => node.VoteCount >= minimumCount);
        }
        if (request.MaximumVoteCount is { } maximumCount)
        {
            candidates = candidates.Where(node => node.VoteCount <= maximumCount);
        }
        if (request.MinimumAverageVote is { } minimumAverage)
        {
            candidates = candidates.Where(node => node.AverageVote >= minimumAverage);
        }
        if (request.MaximumAverageVote is { } maximumAverage)
        {
            candidates = candidates.Where(node => node.AverageVote <= maximumAverage);
        }

        var search = request.SearchText;
        if (search is not null)
        {
            // FREETEXT accepts a parameterized natural-language string, never CONTAINS syntax.
            // The composable query uses full-text indexes, and filters still intersect in SQL.
            var matches = db.Database.SqlQueryRaw<SearchMatch>(
                SqlDiscoveryReadQuery.ReadResource("DiscoverySearchContent.sql"),
                new SqlParameter("@search", SqlDbType.NVarChar, 4000) { Value = search });
            candidates = candidates.Where(node => !node.Hidden &&
                matches.Any(match => match.NodeId == node.NodeId));
        }

        return candidates.Select(node => new Candidate
        {
            NodeId = node.NodeId,
            Title = node.Title,
            ShortDescription = node.ShortDescription,
            UpdatedAt = node.UpdatedAt,
            Hidden = node.Hidden,
            VoteCount = node.VoteCount,
            AverageVote = node.AverageVote,
            RankingScore = (node.AverageVote ?? 0) + Math.Log10(node.VoteCount + 1.0) / 100.0 +
                (search != null && node.Title.Contains(search) ? 0.1 : 0.0)
        });
    }

    /// <summary>Untracked query projection, never a second in-memory ranking implementation.</summary>
    internal sealed class Candidate
    {
        public Guid NodeId { get; set; }
        public string Title { get; set; } = "";
        public string ShortDescription { get; set; } = "";
        public DateTimeOffset UpdatedAt { get; set; }
        public bool Hidden { get; set; }
        public int VoteCount { get; set; }
        public double? AverageVote { get; set; }
        public double RankingScore { get; set; }
    }

    /// <summary>Node IDs exposed by the composable full-text query.</summary>
    public sealed class SearchMatch
    {
        public Guid NodeId { get; set; }
    }
}

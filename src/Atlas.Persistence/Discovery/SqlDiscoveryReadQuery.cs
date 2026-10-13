using System.Data;
using System.Text.Json;
using Atlas.Discovery;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Persistence.Discovery;

/// <summary>Ranks and pages in SQL; only selected read projections leave the database.</summary>
public sealed class SqlDiscoveryReadQuery(Func<AtlasDataContext> open, DiscoveryCache? cache = null) : IDiscoveryReadQuery, IDiscoveryAuthorListing, IDiscoveryPersonalVotes
{
    public DiscoveryPage DiscoverPage(DiscoveryQuery query) => Read(query, null, true);
    public DiscoveryPage DiscoverAuthoredPage(DiscoveryQuery query, IReadOnlyCollection<Guid> authoredNodeIds) =>
        Read(query, authoredNodeIds, true, true);
    public IReadOnlyList<RankedDiscoveryItem> Discover(DiscoveryQuery query) => Read(query, null, false).Items;
    public IReadOnlyList<RankedDiscoveryItem> DiscoverAuthored(DiscoveryQuery query, IReadOnlyCollection<Guid> authoredNodeIds) =>
        Read(query, authoredNodeIds, false, true).Items;

    public DiscoveryPage DiscoverAuthorPage(DiscoveryQuery query, Guid authorParticipantId)
    {
        if (authorParticipantId == Guid.Empty) throw new ArgumentException("An author is required.", nameof(authorParticipantId));
        // Intersect a requested author filter rather than silently replacing it.
        if (query.AuthorParticipantId is { } requested && requested != authorParticipantId)
            return Read(query, [], true, true);
        return Read(query with { AuthorParticipantId = authorParticipantId }, null, true, true);
    }

    private readonly object _uncachedGate = new();
    private DiscoveryPage Read(DiscoveryQuery query, IReadOnlyCollection<Guid>? authoredIds, bool paged, bool includeHidden = false)
    {
        ArgumentNullException.ThrowIfNull(query);
        DiscoveryQueryValidation.Validate(query);
        query = query with { SearchText = string.IsNullOrWhiteSpace(query.SearchText) ? null : query.SearchText.Trim(),
            ReactionDefinitionIds = query.ReactionDefinitionIds?.Distinct().Order().ToArray() };
        authoredIds = authoredIds?.Distinct().Order().ToArray();
        // A fixed gate avoids an unbounded dictionary of per-filter locks. It coordinates
        // misses and the non-thread-safe cache counters without sharing any DbContext.
        lock (cache?.Gate ?? _uncachedGate)
        {
            using var db = open();
            cache?.BindDatabase(db.Database.GetConnectionString()!);
            using var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable);
            // This read holds a shared lock until count/page/cards are complete. Writers update
            // these generations in their own transaction; hidden pages cannot outlive that commit.
            var revision = db.Set<DiscoveryRevisionRow>().AsNoTracking().Single(row => row.Id == 1);
            var key = DiscoveryCache.PageKey(query, authoredIds, revision.PageVersion, includeHidden);
            DiscoveryPage page = null!;
            var pageMiss = !paged || cache is null || !cache.TryPage(key, out page!);
            if (pageMiss)
            {
                var sql = BuildSql(query, authoredIds, includeHidden);
                var countSql = sql.Text + " SELECT COUNT(*) AS [Value] FROM Filtered";
                var total = db.Database.SqlQueryRaw<int>(countSql, sql.Parameters()).AsEnumerable().Single();
                var offset = paged ? ((long)query.Page - 1) * IDiscoveryService.PageSize : 0;
                var pageSql = sql.Text + """
                     SELECT NodeId, VoteCount, AverageVote, Score AS RankingScore
                     FROM Filtered
                     ORDER BY Score DESC, VoteCount DESC, UpdatedAt DESC,
                              Title COLLATE Latin1_General_100_CI_AI_SC,
                              CONVERT(char(36), NodeId) COLLATE Latin1_General_100_BIN2
                     OFFSET @offset ROWS FETCH NEXT @take ROWS ONLY
                    """;
                var rows = offset >= total ? [] : db.Database.SqlQueryRaw<RankRow>(pageSql, sql.Parameters(new SqlParameter("@offset", offset), new SqlParameter("@take", paged ? IDiscoveryService.PageSize : Math.Max(1, total)))).ToList();
                var items = rows.Select((row, index) => new RankedDiscoveryItem(row.NodeId, checked((int)offset + index + 1),
                    row.RankingScore, row.VoteCount, row.AverageVote)).ToArray();
                page = new DiscoveryPage(Array.AsReadOnly(items), total, query.Page, IDiscoveryService.PageSize);
            }
            var cards = new Dictionary<Guid, DiscoveryCard>();
            var missing = new List<Guid>();
            foreach (var item in page.Items)
                if (cache is not null && cache.TryDetail(revision.DetailVersion, item.NodeId, out var card)) cards.Add(item.NodeId, card);
                else missing.Add(item.NodeId);
            var loaded = ReadCards(db, missing.ToArray());
            foreach (var card in loaded.Values) cards[card.NodeId] = card;
            transaction.Commit();
            // Publish only successfully completed committed reads. Detail entries are immutable
            // and independent of filters/ordered page IDs and contain no participant-specific votes.
            if (pageMiss && paged) cache?.PutPage(key, page);
            foreach (var card in loaded.Values) cache?.PutDetail(revision.DetailVersion, card);
            return page with { Items = Array.AsReadOnly(page.Items.Select(item => item with { Card = cards.GetValueOrDefault(item.NodeId) }).ToArray()) };
        }
    }

    /// <summary>Fresh, batched personalized values; intentionally excluded from shared caches.</summary>
    public IReadOnlyDictionary<Guid, int> ReadParticipantVotes(Guid participantId, IReadOnlyCollection<Guid> nodeIds)
    {
        using var db = open();
        var ids = nodeIds.ToArray();
        return db.VoteRows.AsNoTracking().Where(row => row.TargetType == "Node" && row.ParticipantId == participantId && ids.Contains(row.TargetId))
            .Select(row => new { row.TargetId, row.Value }).ToDictionary(row => row.TargetId, row => row.Value);
    }

    internal static SqlRequest BuildSql(DiscoveryQuery query, IReadOnlyCollection<Guid>? authoredIds, bool includeHidden = false)
    {
        var predicates = new List<string>();
        var values = new List<SqlParameter>();
        void Filter(string predicate, string name, object? value)
        {
            if (value is null) return;
            predicates.Add(predicate);
            values.Add(new SqlParameter(name, value));
        }
        if (!includeHidden) predicates.Add("Hidden = 0");
        if (authoredIds is not null) Filter("NodeId IN (SELECT CONVERT(uniqueidentifier, [value]) FROM OPENJSON(@authored))", "@authored", JsonSerializer.Serialize(authoredIds.Distinct()));
        if (query.IsArchived is { } archived) Filter("Status = @status", "@status", archived ? "Archived" : "Active");
        else if (!query.IncludeArchived) predicates.Add("Status <> N'Archived'");
        Filter("TypeId = @type", "@type", query.NodeTypeId);
        Filter("AuthorId = @author", "@author", query.AuthorParticipantId);
        Filter("EXISTS (SELECT 1 FROM CommunityNodeRows c WHERE c.NodeId = NodeStats.NodeId AND c.CommunityId = @community)", "@community", query.CommunityId);
        if (query.ReactionDefinitionIds is { Count: > 0 })
            Filter("NOT EXISTS (SELECT 1 FROM OPENJSON(@reactions) r WHERE NOT EXISTS (SELECT 1 FROM NodeReactionRows nr WHERE nr.NodeId = NodeStats.NodeId AND nr.LifecycleState = N'Active' AND nr.ReactionDefinitionId = CONVERT(uniqueidentifier, r.[value])))", "@reactions", JsonSerializer.Serialize(query.ReactionDefinitionIds.Distinct()));
        Filter("VoteCount >= @minCount", "@minCount", query.MinimumVoteCount);
        Filter("VoteCount <= @maxCount", "@maxCount", query.MaximumVoteCount);
        Filter("AverageVote >= @minAverage", "@minAverage", query.MinimumAverageVote);
        Filter("AverageVote <= @maxAverage", "@maxAverage", query.MaximumAverageVote);
        Filter("CONVERT(date, CreatedAt) >= @from", "@from", query.CreatedFrom?.ToDateTime(TimeOnly.MinValue));
        Filter("CONVERT(date, CreatedAt) <= @through", "@through", query.CreatedThrough?.ToDateTime(TimeOnly.MinValue));
        var search = string.IsNullOrWhiteSpace(query.SearchText) ? null : query.SearchText.Trim();
        values.Add(new SqlParameter("@search", SqlDbType.NVarChar, -1) { Value = (object?)search ?? DBNull.Value });
        if (search is not null) predicates.Add("Hidden = 0 AND (CHARINDEX(@search COLLATE Latin1_General_100_CI_AI_SC, Title) > 0 OR CHARINDEX(@search COLLATE Latin1_General_100_CI_AI_SC, SearchableContent) > 0)");
        // Ordered concatenation preserves searches spanning adjacent fields/blocks, matching
        // the previous projection. CHARINDEX treats %, _ and [ as literal user input.
        var text = """
            WITH NodeStats AS (
              SELECT n.Id AS NodeId, n.Title, n.TypeId, n.AuthorId, n.Status, n.CreatedAt, n.UpdatedAt,
                     v.VoteCount, v.AverageVote,
                     CASE WHEN EXISTS (SELECT 1 FROM ModerationCaseRows m WHERE m.NodeId = n.Id AND m.Status = 2 AND m.VisibilityRestoredAt IS NULL) THEN 1 ELSE 0 END AS Hidden,
                     content.SearchableContent
              FROM NodeRows n
              CROSS APPLY (SELECT COUNT(*) AS VoteCount, AVG(CONVERT(float, Value)) AS AverageVote
                           FROM VoteRows WHERE TargetType = N'Node' AND TargetId = n.Id) v
              OUTER APPLY (
                SELECT STRING_AGG(CONVERT(nvarchar(max), CASE b.Kind
                  WHEN N'markdown' THEN COALESCE(b.Markdown, N'')
                  WHEN N'image' THEN CONCAT(b.AltText, N' ', b.Caption)
                  WHEN N'video' THEN CONCAT(b.Caption, N' ', b.Url COLLATE Latin1_General_100_CI_AI_SC)
                  WHEN N'link-preview' THEN CONCAT(b.Title, N' ', b.Description, N' ', b.Url COLLATE Latin1_General_100_CI_AI_SC)
                  WHEN N'chart-reference' THEN COALESCE(b.Title, N'') ELSE N'' END), N' ')
                  WITHIN GROUP (ORDER BY link.Position) AS SearchableContent
                FROM DocumentBlocks link JOIN BlockRows b ON b.Id = link.BlockId
                WHERE link.DocumentId = n.DescriptionId AND @search IS NOT NULL
              ) content
            ), Filtered AS (
              SELECT *, COALESCE(AverageVote, 0.0) + LOG10(CONVERT(float, VoteCount) + 1.0) / 100.0
                     + CASE WHEN @search IS NOT NULL AND CHARINDEX(@search COLLATE Latin1_General_100_CI_AI_SC, Title) > 0 THEN 0.1 ELSE 0.0 END AS Score
              FROM NodeStats WHERE
            """ + (predicates.Count == 0 ? "1 = 1" : string.Join(" AND ", predicates.Select(p => "(" + p + ")"))) + "\n)\n";
        return new SqlRequest(text, values);
    }

    internal static Dictionary<Guid, DiscoveryCard> ReadCards(AtlasDataContext db, Guid[] ids)
    {
        if (ids.Length == 0) return [];
        var sql = ReadResource("DiscoveryCards.sql");
        var rows = db.Database.SqlQueryRaw<CardRow>(sql, new SqlParameter("@ids", JsonSerializer.Serialize(ids))).ToList();
        return rows.ToDictionary(row => row.NodeId, row => new DiscoveryCard(row.NodeId, row.Title, row.TypeName,
            row.AuthorName, row.Status, row.CreatedAt, row.Description, row.Breadcrumb, row.SubNodes, row.Reactions, row.Communities, row.CommentCount, row.IsHidden));
    }

    private static string ReadResource(string name)
    {
        using var stream = typeof(SqlDiscoveryReadQuery).Assembly.GetManifestResourceStream("Atlas.Persistence.Discovery." + name)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    internal sealed record SqlRequest(string Text, List<SqlParameter> Values)
    {
        public object[] Parameters(params SqlParameter[] extra) => Values.Select(value =>
            (object)new SqlParameter(value.ParameterName, value.SqlDbType, value.Size) { Value = value.Value }).Concat(extra).ToArray();
    }
    public sealed class RankRow
    {
        public Guid NodeId { get; set; }
        public int VoteCount { get; set; }
        public double? AverageVote { get; set; }
        public double RankingScore { get; set; }
    }
    public sealed class CardRow
    {
        public Guid NodeId { get; set; }
        public string Title { get; set; } = "";
        public string TypeName { get; set; } = "";
        public string AuthorName { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTimeOffset CreatedAt { get; set; }
        public string Description { get; set; } = "";
        public string Breadcrumb { get; set; } = "";
        public string SubNodes { get; set; } = "";
        public string Reactions { get; set; } = "";
        public string Communities { get; set; } = "";
        public int CommentCount { get; set; }
        public bool IsHidden { get; set; }
    }
}

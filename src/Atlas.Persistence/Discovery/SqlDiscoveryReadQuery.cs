using System.Data;
using System.Text.Json;
using Atlas.Discovery;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Persistence.Discovery;

/// <summary>
/// Executes filtering, ranking, totals and pagination in SQL, then reads public cards
/// only for the selected IDs. Each request owns a context and a consistent read transaction.
/// Optional caches reuse immutable projections after checking committed database revisions.
/// </summary>
public sealed class SqlDiscoveryReadQuery(
    Func<AtlasDataContext> open,
    // The primary-constructor parameter is available throughout this class.
    // Console Program.cs creates one DiscoveryCache and passes it here. Tests and
    // other callers may omit it to run the same database query without cache reuse.
    DiscoveryCache? cache = null)
    : IDiscoveryReadQuery, IDiscoveryAuthorListing, IDiscoveryPersonalVotes
{
    /// <summary>Returns a public page, excluding nodes hidden by moderation.</summary>
    public DiscoveryPage DiscoverPage(DiscoveryQuery query) => Read(query, null, true);
    /// <summary>Returns a page restricted to supplied authored IDs, including hidden placeholders.</summary>
    public DiscoveryPage DiscoverAuthoredPage(DiscoveryQuery query, IReadOnlyCollection<Guid> authoredNodeIds)
    {
        ArgumentNullException.ThrowIfNull(authoredNodeIds);
        return Read(query, authoredNodeIds, true, true);
    }
    /// <summary>Returns all matching public results for the unpaged contract; Console uses pages.</summary>
    public IReadOnlyList<RankedDiscoveryItem> Discover(DiscoveryQuery query) => Read(query, null, false).Items;
    /// <summary>Returns all matches within the supplied authored IDs, including hidden placeholders.</summary>
    public IReadOnlyList<RankedDiscoveryItem> DiscoverAuthored(DiscoveryQuery query, IReadOnlyCollection<Guid> authoredNodeIds)
    {
        ArgumentNullException.ThrowIfNull(authoredNodeIds);
        return Read(query, authoredNodeIds, false, true).Items;
    }

    /// <summary>Filters an authored page in SQL without first loading all of the author’s nodes.</summary>
    public DiscoveryPage DiscoverAuthorPage(DiscoveryQuery query, Guid authorParticipantId)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (authorParticipantId == Guid.Empty)
        {
            throw new ArgumentException("An author is required.", nameof(authorParticipantId));
        }
        // Intersect a requested author filter rather than silently replacing it.
        if (query.AuthorParticipantId is { } requested && requested != authorParticipantId)
            return Read(query, [], true, true);
        return Read(query with { AuthorParticipantId = authorParticipantId }, null, true, true);
    }

    // This is a lock object, not a cache. Without an injected cache, this instance
    // still serializes its Read calls using this fallback gate.
    private readonly object _uncachedGate = new();

    /// <summary>Resolves ranked results and public cards against one committed database view.</summary>
    private DiscoveryPage Read(DiscoveryQuery query, IReadOnlyCollection<Guid>? authoredIds, bool paged, bool includeHidden = false)
    {
        // 1. Validate and normalize the request before touching the database/cache.
        ArgumentNullException.ThrowIfNull(query);
        DiscoveryQueryValidation.Validate(query);

        // Normalize equivalent set inputs and surrounding whitespace before key generation.
        query = query with
        {
            SearchText = string.IsNullOrWhiteSpace(query.SearchText) ? null : query.SearchText.Trim(),
            ReactionDefinitionIds = query.ReactionDefinitionIds?.Distinct().Order().ToArray()
        };
        authoredIds = authoredIds?.Distinct().Order().ToArray();

        // 2. Coordinate this read with other reads using the same gate.
        // cache?.Gate means “get Gate if cache exists”; ?? chooses _uncachedGate
        // when it does not. lock allows only one caller through this gate at a time.
        // A cache shared by multiple query instances supplies their shared gate.
        // This process lock is separate from the database transaction locks below.
        // It coordinates misses/counters without storing or sharing a DbContext.
        lock (cache?.Gate ?? _uncachedGate)
        {
            // 3. Create a fresh EF context. BindDatabase does not find/create a database:
            // it remembers this connection configuration and rejects reuse of the same
            // cache for a different one (whose IDs/revision numbers could overlap).
            // ?. skips this call entirely when caching was omitted.
            using var db = open();
            cache?.BindDatabase(db.Database.GetConnectionString()!);

            using var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable);
            // 4. Read the committed freshness counters, even when cache is absent.
            // The singleton row has Id = 1. PageVersion and DetailVersion are generation
            // numbers, not page numbers, node limits or timestamps. Relevant writes bump
            // them atomically; new versions make older cache keys unusable.
            // This read holds a shared lock until count/page/cards are complete. Writers update
            // these generations in their own transaction; hidden pages cannot outlive that commit.
            var revision = db.Set<DiscoveryRevisionRow>().AsNoTracking().Single(row => row.Id == 1);

            // 5. Look for a ranked page matching the request and current page revision.
            // PageKey is a static helper, so it can be called without a cache instance.
            var key = DiscoveryCache.PageKey(query, authoredIds, revision.PageVersion, includeHidden);
            // null! suppresses the nullable warning; it does not create a page.
            // A successful TryPage supplies it, otherwise the database branch below does.
            DiscoveryPage page = null!;

            // || short-circuits from left to right. Query storage when:
            // - this is an unpaged request (ranked-page caching is bypassed), or
            // - no cache was injected, or
            // - TryPage cannot find this key (first use, new revision, expiry/eviction).
            // Unpaged requests can still reuse detail cards if a cache exists.
            var pageMiss = !paged || cache is null || !cache.TryPage(key, out page!);

            if (pageMiss)
            {
                // 6. Build deferred EF query expressions, then execute count and page.
                // Count and page use the same filter predicates. The count includes all
                // matches; OFFSET/FETCH limits only the ranked rows returned to the host.
                var filtered = DiscoveryEfQuery.Build(db, query, authoredIds, includeHidden);
                // Build returns IQueryable: no candidates have been enumerated yet.
                // TagWith adds a SQL comment for identifying this statement in plans/logs.
                // Count executes a database aggregate, not an in-memory list count.
                var total = filtered.TagWith("Discovery count").Count();

                var offset = paged ? ((long)query.Page - 1) * IDiscoveryService.PageSize : 0;
                // Count is an int, so an offset below total fits Skip(int). Test the
                // long offset first to safely handle even int.MaxValue page numbers.
                // The ternary returns an empty list beyond the total; otherwise ToList
                // executes the ordered/paged SQL. Only selected rows reach the application.
                var rows = offset >= total
                    ? []
                    : filtered
                        .OrderByDescending(node => node.RankingScore)
                        .ThenByDescending(node => node.VoteCount)
                        .ThenByDescending(node => node.UpdatedAt)
                        .ThenBy(node => EF.Functions.Collate(node.Title, DiscoveryTextCollation.Name))
                        .ThenBy(node => EF.Functions.Collate(node.NodeId.ToString(), "Latin1_General_100_BIN2"))
                        .Skip((int)offset)
                        .Take(paged ? IDiscoveryService.PageSize : Math.Max(1, total))
                        .Select(node => new RankRow
                        {
                            NodeId = node.NodeId,
                            VoteCount = node.VoteCount,
                            AverageVote = node.AverageVote,
                            RankingScore = node.RankingScore
                        })
                        .TagWith("Discovery ranked page")
                        .ToList();

                // Assign global ranks locally to the already selected rows, not to all nodes.
                var items = rows.Select((row, index) => new RankedDiscoveryItem(
                    row.NodeId,
                    checked((int)offset + index + 1),
                    row.RankingScore,
                    row.VoteCount,
                    row.AverageVote))
                    .ToArray();
                page = new DiscoveryPage(Array.AsReadOnly(items), total, query.Page, IDiscoveryService.PageSize);
            }

            // 7. Resolve public display cards for the ranked IDs.
            // cards collects both cache hits and database results for this response.
            // missing contains IDs whose display data is absent from the detail cache;
            // it does NOT mean the node is missing from the database.
            var cards = new Dictionary<Guid, DiscoveryCard>();
            var missing = new List<Guid>();
            // A page hit does not imply a card hit: the two layers have separate revisions.
            // Collect missing IDs before ReadCards' fixed metadata/summary batches,
            // avoiding a query per node. With no cache, every selected ID is missing here.
            foreach (var item in page.Items)
            {
                if (cache is not null && cache.TryDetail(revision.DetailVersion, item.NodeId, out var card))
                {
                    cards.Add(item.NodeId, card);
                }
                else
                {
                    missing.Add(item.NodeId);
                }
            }

            // 8. Load only missing cards. ReadCards uses one EF metadata batch and one
            // SQL summary batch; an empty missing list returns without either query.
            var loaded = ReadCards(db, missing.ToArray());
            foreach (var card in loaded.Values)
            {
                cards[card.NodeId] = card;
            }

            // 9. Finish the consistent read before publishing any newly loaded entries.
            // Exceptions before this point leave these attempted results out of the cache.
            transaction.Commit();

            // Publish only successfully completed committed reads. Detail entries are immutable
            // and independent of filters/ordered page IDs and contain no participant-specific votes.
            if (pageMiss && paged)
            {
                cache?.PutPage(key, page);
            }
            foreach (var card in loaded.Values)
            {
                cache?.PutDetail(revision.DetailVersion, card);
            }

            // 10. Return a copy with its public cards attached. Cached ranked pages remain
            // separate from detail entries; personalized participant votes are not included.
            return page with
            {
                Items = Array.AsReadOnly(page.Items
                    .Select(item => item with { Card = cards.GetValueOrDefault(item.NodeId) })
                    .ToArray())
            };
        }
    }

    /// <summary>Fresh, batched personalized values; intentionally excluded from shared caches.</summary>
    public IReadOnlyDictionary<Guid, int> ReadParticipantVotes(Guid participantId, IReadOnlyCollection<Guid> nodeIds)
    {
        using var db = open();
        var ids = nodeIds.ToArray();
        return db.VoteRows.AsNoTracking()
            .Where(row => row.TargetType == "Node" &&
                row.ParticipantId == participantId &&
                ids.Contains(row.TargetId))
            .Select(row => new { row.TargetId, row.Value })
            .ToDictionary(row => row.TargetId, row => row.Value);
    }

    /// <summary>Loads missing display cards using fixed EF metadata and SQL summary batches.</summary>
    internal static Dictionary<Guid, DiscoveryCard> ReadCards(AtlasDataContext db, Guid[] ids)
    {
        if (ids.Length == 0)
        {
            return [];
        }
        // Ordinary card metadata uses EF projections. Only page IDs reach this query;
        // it is one batch, not a repository call for each selected node.
        var metadata = db.NodeRows.AsNoTracking()
            .Where(node => ids.Contains(node.Id))
            .Select(node => new
            {
                NodeId = node.Id,
                TypeName = db.NodeTypeRows.Where(type => type.Id == node.TypeId)
                    .Select(type => type.Name).FirstOrDefault() ?? "Unknown",
                AuthorName = db.ParticipantRows.Where(author => author.Id == node.AuthorId)
                    .Select(author => author.DisplayName).FirstOrDefault() ?? "Unknown",
                node.Status,
                node.CreatedAt,
                CommentCount = db.CommentRows.Count(comment =>
                    comment.TargetKind == "Node" && comment.TargetId == node.Id)
            })
            .TagWith("Discovery card metadata")
            .ToDictionary(node => node.NodeId);

        // Recursive multi-parent paths and ordered text aggregation remain isolated SQL.
        // Their specific translation limits are documented in DiscoveryCards.sql.
        var sql = ReadResource("DiscoveryCards.sql");
        var rows = db.Database.SqlQueryRaw<CardRow>(
            sql,
            new SqlParameter("@ids", JsonSerializer.Serialize(ids)))
            .ToList();
        return rows.ToDictionary(
            row => row.NodeId,
            row => new DiscoveryCard(
                row.NodeId,
                row.Title,
                metadata[row.NodeId].TypeName,
                metadata[row.NodeId].AuthorName,
                metadata[row.NodeId].Status,
                metadata[row.NodeId].CreatedAt,
                row.Description,
                row.Breadcrumb,
                FormatChildren(row.SubNodes),
                row.Reactions,
                row.Communities,
                metadata[row.NodeId].CommentCount,
                row.IsHidden));
    }

    /// <summary>Converts SQL child-count JSON to labels using the shared pluralization policy.</summary>
    private static string FormatChildren(string json)
    {
        if (json is "None" or "[]")
        {
            return "None";
        }
        using var children = JsonDocument.Parse(json);
        return string.Join(" · ", children.RootElement.EnumerateArray().Select(child =>
            DiscoveryDisplayText.FormatTypeCount(child.GetProperty("Name").GetString()!, child.GetProperty("Count").GetInt32(), child.GetProperty("AutoPluralize").GetBoolean())));
    }

    /// <summary>Loads the embedded card query so SQL projection logic remains readable in its own file.</summary>
    internal static string ReadResource(string name)
    {
        using var stream = typeof(SqlDiscoveryReadQuery).Assembly.GetManifestResourceStream("Atlas.Persistence.Discovery." + name)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>Maps ranked SQL columns without materializing tracked EF entities.</summary>
    public sealed class RankRow
    {
        public Guid NodeId { get; set; }
        public int VoteCount { get; set; }
        public double? AverageVote { get; set; }
        public double RankingScore { get; set; }
    }
    /// <summary>Maps the batched SQL card columns before constructing immutable public cards.</summary>
    public sealed class CardRow
    {
        public Guid NodeId { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Breadcrumb { get; set; } = "";
        public string SubNodes { get; set; } = "";
        public string Reactions { get; set; } = "";
        public string Communities { get; set; } = "";
        public bool IsHidden { get; set; }
    }
}

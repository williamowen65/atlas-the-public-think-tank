using System.Data;
using System.Diagnostics;
using Atlas.Contracts.Observability;
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
    public DiscoveryPage DiscoverModeratorPage(DiscoveryQuery query) => Read(query, null, true, true);
    /// <summary>Returns a public page, excluding nodes hidden by moderation.</summary>
    public DiscoveryPage DiscoverPage(DiscoveryQuery query) => Read(query, null, true);
    /// <summary>Returns a page restricted to supplied authored IDs, including hidden placeholders.</summary>
    public DiscoveryPage DiscoverAuthoredPage(DiscoveryQuery query, IReadOnlyCollection<Guid> authoredNodeIds)
    {
        ArgumentNullException.ThrowIfNull(authoredNodeIds);
        return Read(query, authoredNodeIds, true, true, authorListing: true);
    }
    /// <summary>Returns all matching public results for the unpaged contract; Console uses pages.</summary>
    public IReadOnlyList<RankedDiscoveryItem> Discover(DiscoveryQuery query) => Read(query, null, false).Items;
    /// <summary>Returns all matches within the supplied authored IDs, including hidden placeholders.</summary>
    public IReadOnlyList<RankedDiscoveryItem> DiscoverAuthored(DiscoveryQuery query, IReadOnlyCollection<Guid> authoredNodeIds)
    {
        ArgumentNullException.ThrowIfNull(authoredNodeIds);
        return Read(query, authoredNodeIds, false, true, authorListing: true).Items;
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
            return Read(query, [], true, true, authorListing: true);
        return Read(query with { AuthorParticipantId = authorParticipantId }, null, true, true, authorListing: true);
    }

    // This is a lock object, not a cache. Without an injected cache, this instance
    // still serializes its Read calls using this fallback gate.
    private readonly object _uncachedGate = new();

    /// <summary>Resolves ranked results and public cards against one committed database view.</summary>
    private DiscoveryPage Read(DiscoveryQuery query, IReadOnlyCollection<Guid>? authoredIds, bool paged, bool includeHidden = false, bool authorListing = false)
    {
        try
        {
            return ReadCore(query, authoredIds, paged, includeHidden, authorListing);
        }
        catch (SqlException error) when (!string.IsNullOrWhiteSpace(query?.SearchText))
        {
            // This includes stopped full-text services, disabled/missing indexes and connection
            // failures. Preserve the SQL cause for backend logging, never expose it to visitors.
            throw new SearchUnavailableException(error);
        }
    }

    private DiscoveryPage ReadCore(DiscoveryQuery query, IReadOnlyCollection<Guid>? authoredIds, bool paged, bool includeHidden, bool authorListing)
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
        var readStarted = Stopwatch.GetTimestamp();
        lock (cache?.Gate ?? _uncachedGate)
        {
            var lockWaitMs = Stopwatch.GetElapsedTime(readStarted).TotalMilliseconds;
            var sqlCountMs = 0.0;
            var sqlRankMs = 0.0;
            // 3. Create a fresh EF context. BindDatabase does not find/create a database:
            // it remembers this connection configuration and rejects reuse of the same
            // cache for a different one (whose IDs/revision numbers could overlap).
            // ?. skips this call entirely when caching was omitted.
            using var db = open();
            cache?.BindDatabase(db.Database.GetConnectionString()!);
            if (query.SearchText is not null)
                DiscoveryFullTextSchema.EnsureAvailable(db);

            var cachePage = paged && query.SearchText is null;
            var external = paged && !includeHidden ? cache?.External : null;
            var pageKey = DiscoveryCache.PageKey(query, authoredIds, includeHidden);
            var externalPageKey = cachePage && external is not null ? cache!.ExternalKey("page:" + pageKey) : null;
            var memoryPage = cachePage ? cache?.PeekPage(pageKey) : null;
            var redisPageStarted = Stopwatch.GetTimestamp();
            var remotePage = memoryPage is null && externalPageKey is not null
                ? external!.ReadEntries<DiscoveryPage>([externalPageKey])[0]
                : null;
            var redisPageMs = memoryPage is null && externalPageKey is not null
                ? Stopwatch.GetElapsedTime(redisPageStarted).TotalMilliseconds : 0;
            var candidateIds = (memoryPage ?? remotePage)?.Value.Items.Select(item => item.NodeId).ToArray() ?? [];

            var sqlStarted = Stopwatch.GetTimestamp();
            using var transaction = db.Database.BeginTransaction(IsolationLevel.Snapshot);
            var sqlStartMs = Stopwatch.GetElapsedTime(sqlStarted).TotalMilliseconds;
            var revisionStarted = Stopwatch.GetTimestamp();
            var revision = DiscoveryEntityCache.ReadVersions(db, external is null ? [] : candidateIds);
            var revisionMs = Stopwatch.GetElapsedTime(revisionStarted).TotalMilliseconds;
            Telemetry.MeasureDiscovery(DiscoveryPhase.RevisionCheck, DiscoveryMeasurementOutcome.Completed, revisionStarted);

            DiscoveryPage page = null!;
            var pageMiss = !cachePage || cache is null || !cache.TryPage(pageKey, revision.PageVersion, out page!);
            var externalPage = pageMiss && remotePage?.Revision == revision.PageVersion
                ? remotePage.Value
                : null;
            if (externalPage is not null)
            {
                page = externalPage;
                pageMiss = false;
            }

            if (pageMiss)
            {
                var rebuildStarted = Stopwatch.GetTimestamp();
                // 6. Build deferred EF query expressions, then execute count and page.
                // Count and page use the same filter predicates. The count includes all
                // matches; OFFSET/FETCH limits only the ranked rows returned to the host.
                var filtered = DiscoveryEfQuery.Build(db, query, authoredIds,
                    includeHidden && !(authorListing && query.SearchText is not null));
                // Build returns IQueryable: no candidates have been enumerated yet.
                // TagWith adds a SQL comment for identifying this statement in plans/logs.
                // Count executes a database aggregate, not an in-memory list count.
                var countStarted = Stopwatch.GetTimestamp();
                var total = filtered.TagWith("Discovery count").Count();

                sqlCountMs = Stopwatch.GetElapsedTime(countStarted).TotalMilliseconds;

                var offset = paged ? ((long)query.Page - 1) * IDiscoveryService.PageSize : 0;
                // Count is an int, so an offset below total fits Skip(int). Test the
                // long offset first to safely handle even int.MaxValue page numbers.
                // The ternary returns an empty list beyond the total; otherwise ToList
                // executes the ordered/paged SQL. Only selected rows reach the application.
                var rankStarted = Stopwatch.GetTimestamp();
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

                if (offset < total) sqlRankMs = Stopwatch.GetElapsedTime(rankStarted).TotalMilliseconds;

                // Assign global ranks locally to the already selected rows, not to all nodes.
                var items = rows.Select((row, index) => new RankedDiscoveryItem(
                    row.NodeId,
                    checked((int)offset + index + 1),
                    row.RankingScore,
                    row.VoteCount,
                    row.AverageVote))
                    .ToArray();
                page = new DiscoveryPage(Array.AsReadOnly(items), total, query.Page, IDiscoveryService.PageSize);
                Telemetry.MeasureDiscovery(DiscoveryPhase.PageRebuild, DiscoveryMeasurementOutcome.Completed, rebuildStarted);
            }

            // 7. Resolve public display cards for the ranked IDs.
            // cards collects both cache hits and database results for this response.
            // missing contains IDs whose display data is absent from the detail cache;
            // it does NOT mean the node is missing from the database.
            var cardsStarted = Stopwatch.GetTimestamp();
            var cards = new Dictionary<Guid, DiscoveryCard>();
            var missing = new List<Guid>();
            // A page hit does not imply a card hit: the two layers have separate revisions.
            // Collect missing IDs before ReadCards' fixed metadata/summary batches,
            // avoiding a query per node. With no cache, every selected ID is missing here.
            var moderatorVisible = query.IncludeHidden && includeHidden;
            foreach (var item in page.Items)
            {
                if (cache is not null && cache.TryDetail(revision.DetailVersion, item.NodeId, moderatorVisible, out var card))
                {
                    cards.Add(item.NodeId, card);
                }
                else
                {
                    missing.Add(item.NodeId);
                }
            }

            var memoryCardCount = cards.Count;
            var externalCards = new List<DiscoveryCard>();
            string DetailKey(Guid id) => cache!.ExternalKey($"card-metadata-v2:{id:N}");
            if (external is not null && missing.Count > 0)
            {
                var remote = external.Read<DiscoveryCard>(missing.Select(DetailKey).ToArray(), revision.DetailVersion);
                for (var index = missing.Count - 1; index >= 0; index--)
                {
                    var card = remote[index];
                    if (card is null || card.NodeId != missing[index]) continue;
                    externalCards.Add(card);
                    cards.Add(card.NodeId, card);
                    missing.RemoveAt(index);
                }
            }

            // 8. Load only missing cards. ReadCards uses one EF metadata batch and one
            // SQL summary batch plus up to three bounded EF breadcrumb hops.
            // An empty missing list returns without any card query.
            var detailStarted = Stopwatch.GetTimestamp();
            var loaded = ReadCards(db, missing.ToArray(), moderatorVisible, components: external is not null);
            foreach (var card in loaded.Values)
            {
                cards[card.NodeId] = card;
            }

            var entityWrites = new List<(string Key, DiscoveryEntityCache.Payload Value)>();
            if (external is not null && (externalCards.Count > 0 || loaded.Count > 0))
            {
                var components = externalCards.Concat(loaded.Values).ToDictionary(card => card.NodeId);
                // A ranking rebuild can select nodes outside the candidate page.
                var manifest = components.Keys.All(candidateIds.Contains)
                    ? revision
                    : DiscoveryEntityCache.ReadVersions(db, components.Keys.ToArray(), checkPage: false);
                foreach (var id in components.Keys) cards.Remove(id);
                foreach (var (id, card) in DiscoveryEntityCache.Assemble(db, cache!, manifest, components, entityWrites))
                    cards.Add(id, card);
            }
            if (missing.Count > 0 || entityWrites.Count > 0)
                Telemetry.MeasureDiscovery(DiscoveryPhase.DetailRebuild, DiscoveryMeasurementOutcome.Completed, detailStarted);

            // 9. Finish the consistent read before publishing any newly loaded entries.
            // Exceptions before this point leave these attempted results out of the cache.
            // Attach the first ancestry segment in one query per Discovery page.
            // Keep fresh vote totals out of the cached card metadata.
            var ancestors = new Dictionary<Guid, AncestorPreview>();
            if (paged && query.ParentNodeId is null)
            {
                var ancestorIds = cards.Values.Where(card => !card.IsHidden)
                    .SelectMany(card => card.BreadcrumbSegment?.Items ?? [])
                    .Select(item => item.NodeId).Distinct().ToArray();
                if (ancestorIds.Length > 0)
                    ancestors = ReadAncestorPreviews(db, ancestorIds);
            }

            var cardsMs = Stopwatch.GetElapsedTime(cardsStarted).TotalMilliseconds;
            var commitStarted = Stopwatch.GetTimestamp();
            transaction.Commit();

            // Publish only successfully completed committed reads. Detail entries are immutable
            // and independent of filters/ordered page IDs and contain no participant-specific votes.
            if ((pageMiss || externalPage is not null) && cachePage)
            {
                cache?.PutPage(pageKey, revision.PageVersion, page);
            }
            foreach (var id in loaded.Keys.Concat(externalCards.Select(card => card.NodeId)))
                if (cards.TryGetValue(id, out var card))
                    cache?.PutDetail(revision.DetailVersion, card, moderatorVisible);

            if (pageMiss && externalPageKey is not null && page.Items.Count > 0)
                external!.Write(externalPageKey, revision.PageVersion, page);
            if (external is not null)
            {
                foreach (var (key, payload) in entityWrites)
                    external.Write(key, 0, payload);
                foreach (var card in loaded.Values)
                    if (cards.ContainsKey(card.NodeId))
                        external.Write(DetailKey(card.NodeId), revision.DetailVersion, card);
            }

            var commitAndPublishMs = Stopwatch.GetElapsedTime(commitStarted).TotalMilliseconds;

            // 10. Return a copy with its public cards attached. Cached ranked pages remain
            // separate from detail entries; personalized participant votes are not included.
            var result = page with
            {
                Items = Array.AsReadOnly(page.Items
                    .Select(item => item with { Card = WithAncestorPreviews(cards.GetValueOrDefault(item.NodeId), ancestors) })
                    .ToArray())
            };
            var rankingSource = pageMiss ? DiscoveryCacheSource.Sql
                : externalPage is not null ? DiscoveryCacheSource.Redis : DiscoveryCacheSource.ProcessMemory;
            Telemetry.RecordDiscoveryCache(new(page.Page, rankingSource, memoryCardCount,
                externalCards.Count, loaded.Count, page.Items.Count - cards.Count)
            {
                Scope = query.NodeId is not null ? "node" : query.ParentNodeId is not null ? "subnodes"
                    : query.SearchText is not null || authoredIds is not null || authorListing ? "filtered" : "discovery",
                TotalMs = Stopwatch.GetElapsedTime(readStarted).TotalMilliseconds,
                LockWaitMs = lockWaitMs,
                RedisPageMs = redisPageMs,
                SqlStartMs = sqlStartMs,
                RevisionMs = revisionMs,
                SqlCountMs = sqlCountMs,
                SqlRankMs = sqlRankMs,
                CardsMs = cardsMs,
                CommitAndPublishMs = commitAndPublishMs
            });
            return result;
        }
    }

    private static DiscoveryCard? WithAncestorPreviews(DiscoveryCard? card, Dictionary<Guid, AncestorPreview> ancestors)
    {
        if (card is null || card.IsHidden || card.BreadcrumbSegment is not { } segment || ancestors.Count == 0)
            return card;

        return card with
        {
            BreadcrumbSegment = segment with
            {
                Items = Array.AsReadOnly(segment.Items.Select(item =>
                    item with { Preview = ancestors.GetValueOrDefault(item.NodeId) }).ToArray())
            }
        };
    }

    private static Dictionary<Guid, AncestorPreview> ReadAncestorPreviews(AtlasDataContext db, Guid[] ids)
    {
        return db.NodeRows.AsNoTracking()
            .Where(node => ids.Contains(node.Id) && node.Status != "Archived" &&
                !db.ModerationCaseRows.Any(report => report.NodeId == node.Id &&
                    report.Status == 2 && report.VisibilityRestoredAt == null))
            .Select(node => new
            {
                node.Id,
                node.Title,
                TypeName = db.NodeTypeRows.Where(type => type.Id == node.TypeId)
                    .Select(type => type.Name).FirstOrDefault() ?? "Unknown",
                VoteCount = db.VoteRows.Count(vote => vote.TargetType == "Node" && vote.TargetId == node.Id),
                AverageVote = db.VoteRows.Where(vote => vote.TargetType == "Node" && vote.TargetId == node.Id)
                    .Select(vote => (double?)vote.Value).Average()
            })
            .TagWith("Discovery ancestor mini cards")
            .ToDictionary(row => row.Id, row => new AncestorPreview(
                row.Title, row.TypeName, row.VoteCount, row.AverageVote));
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
    internal static Dictionary<Guid, DiscoveryCard> ReadCards(AtlasDataContext db, Guid[] ids, bool showHidden = false, bool components = false)
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
                node.AuthorId,
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

        // Only ordered display aggregation remains SQL; breadcrumbs use bounded EF hops.
        // Their specific translation limits are documented in DiscoveryCards.sql.
        var sql = ReadResource("DiscoveryCards.sql");
        var rows = db.Database.SqlQueryRaw<CardRow>(
            sql,
            new SqlParameter("@ids", JsonSerializer.Serialize(ids)),
            new SqlParameter("@showHidden", SqlDbType.Bit) { Value = showHidden },
            new SqlParameter("@components", SqlDbType.Bit) { Value = components })
            .ToList();
        var breadcrumbs = DiscoveryBreadcrumbReader.ReadFirstPages(db, ids);
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
                breadcrumbs[row.NodeId].DisplayText,
                FormatChildren(row.SubNodes),
                row.Reactions,
                row.Communities,
                metadata[row.NodeId].CommentCount,
                row.IsHidden)
            {
                ReactionCount = row.ReactionCount,
                AuthorId = row.IsHidden && !showHidden ? null : metadata[row.NodeId].AuthorId,
                ChildTypes = row.IsHidden && !showHidden ? Array.Empty<DiscoveryChildType>() : ReadChildTypes(row.SubNodes),
                PreviewBlocks = row.PreviewJson.Length <= 48 * 1024
                    ? JsonSerializer.Deserialize<DiscoveryPreviewBlock[]>(row.PreviewJson, PreviewJsonOptions) ?? []
                    : [],
                BreadcrumbSegment = breadcrumbs[row.NodeId]
            });
    }

    private static readonly JsonSerializerOptions PreviewJsonOptions = new(JsonSerializerDefaults.Web);

    private static IReadOnlyList<DiscoveryChildType> ReadChildTypes(string json)
    {
        if (json is "None" or "[]") return Array.Empty<DiscoveryChildType>();
        using var children = JsonDocument.Parse(json);
        return Array.AsReadOnly(children.RootElement.EnumerateArray().Select(child =>
            new DiscoveryChildType(
                child.GetProperty("TypeId").GetGuid(),
                DiscoveryDisplayText.FormatTypeCount(child.GetProperty("Name").GetString()!, child.GetProperty("Count").GetInt32(), child.GetProperty("AutoPluralize").GetBoolean()),
                child.GetProperty("Count").GetInt32())).ToArray());
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
        public string PreviewJson { get; set; } = "[]";
        public string SubNodes { get; set; } = "";
        public string Reactions { get; set; } = "";
        public int ReactionCount { get; set; }
        public string Communities { get; set; } = "";
        public bool IsHidden { get; set; }
    }
}

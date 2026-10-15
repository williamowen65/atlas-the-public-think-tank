# Discovery reads, pagination and caching (PTT-132)

## Start here

- **Discovery runs in the database.** EF builds the filtering, statistics, ranking and paging queries. There is no production in-memory ranking implementation.

- **The host owns the cache.** Console startup creates it, passes it to the query adapter and disposes it when the startup scope exits.

- **Pages and cards are different.** A page identifies the selected nodes and their ranks. A card contains the public information displayed for one node.

- **Breadcrumbs are bounded.** Each segment contains up to three ancestor nodes toward a root, rather than every complete ancestor route.

- For a visual explanation, open the [read-flow blackboard](../blackboards/workflows/Discovery/DiscoveryReadFlow.excalidraw) or its [code walkthrough](../blackboards/workflows/Discovery/DiscoveryReadFlow.md). The [performance guide](discovery-performance.md) explains the test sequence and links individual database reads to code.

## Follow the request through the code

1. **Console constructs a request.**
   - [ConsoleApplication.cs](../../src/Atlas.Console/ConsoleApplication.cs) and [AnonymousBrowseCommands.cs](../../src/Atlas.Console/AnonymousBrowseCommands.cs) collect filters and a page number.
   - [DiscoveryQuery](../../src/Atlas.Discovery/DiscoveryQuery.cs) is the request record, not a query executor.

2. **The service validates and delegates.**
   - [DiscoveryService](../../src/Atlas.Discovery/DiscoveryService.cs) rejects invalid requests before storage access.
   - `IDiscoveryReadQuery` is the storage boundary; [SqlDiscoveryReadQuery](../../src/Atlas.Persistence/Discovery/SqlDiscoveryReadQuery.cs) implements it.
   - Both have `DiscoverPage` methods. The service delegates to the adapter; this does not execute the query twice.

3. **The adapter resolves a page and its cards.**
   - `Read` opens its own EF context and serializable read transaction.
   - It checks committed cache revisions, then looks for reusable ranked results.
   - On a page miss, it executes count and paging queries.
   - It resolves selected cards separately, loading only missing details.
   - After the read transaction commits, it publishes new cache entries and returns the results.

## EF query execution: building versus running

- [DiscoveryEfQuery.Build](../../src/Atlas.Persistence/Discovery/DiscoveryEfQuery.cs) returns an `IQueryable`. This is a deferred query expression: building it does not load all candidate nodes.

- `Where`, `Select`, `OrderBy`, `Skip` and `Take` describe the database work. EF translates the supported expressions into SQL.

- `Count()` executes the total-count query. `ToList()` executes the selected-page query and materializes only its results.

- `TagWith("Discovery count")` adds an identifying comment to generated SQL. It helps identify statements in logs and execution plans; it does not execute an additional query.

- Ordinary card labels, status/date and comment counts use an EF metadata batch. Ordered descriptions and other display summaries use a separate SQL batch for the same IDs.

- Listing flows do not reconstruct every domain `Node`. Opening or reporting a node uses its domain repository workflow. Authored-profile totals come from the database listing rather than loading all authored aggregates.

## Ranking and result-page pagination

- **Importance score:** `(AverageVote ?? 0) + log10(VoteCount + 1) / 100`, with an additional `0.1` when the search text matches the title.
  - `?? 0` gives an unvoted node a zero average for scoring.
  - The small participation term distinguishes otherwise similar averages.
  - This is Atlas importance ranking, not full-text relevance ranking.

- **Stable ordering:** score descending, vote count descending, updated time descending, title ascending, then canonical GUID text ascending.
  - Title ordering uses the Discovery collation.
  - Binary comparison of canonical GUID text preserves the earlier .NET-compatible tie-breaker; SQL Server's native GUID ordering differs.

- **Page size:** three results. Offset is `(page - 1) * 3`, calculated using long arithmetic.
  - Example: eight matches, page two → skip three → take three → global ranks 4, 5 and 6.
  - An offset at or beyond the total returns an empty page without running the ranked-row query.
  - An offset below the int-sized count safely fits EF's `Skip(int)`.

- A request's count, ranked IDs, card details and breadcrumb hops share one serializable read transaction.
  - A later request can see a changed ordering after a vote or edit.
  - This is offset paging, not a frozen multi-request snapshot or cursor API.
  - Unpaged contracts remain for existing callers/tests; normal Console browsing uses pages.

## Filters and search behavior

- **Text:** case/accent-insensitive substring matching over titles and searchable description-block fields.
  - EF `Contains` translation escapes parameters; `%`, `_` and `[` remain literal search characters.
  - Ordered space-separated block text allows a phrase to span adjacent fields or blocks.
  - Poll reference IDs are not searchable.
  - This is not SQL Server full-text search; it does not provide the legacy FREETEXTTABLE relevance behavior.

- **Membership:** community and reaction filters use existence checks rather than joins that could multiply vote rows.
  - Every requested reaction must have an active association.
  - Suppressed reaction definitions affect card display but do not change the association-filter rule.

- **Average bounds:** unvoted nodes are excluded because their actual average is null, even though scoring uses zero as its fallback.

- **Dates:** EF compares the timestamp's stored calendar date with the requested bounds, including the full DateOnly range. This is not a conversion to the viewer's local date.

- **Moderation versus archive:** public Discovery excludes moderation-hidden nodes; archive inclusion is a separate request setting.
  - Authored listings may show hidden nodes as placeholders, not their original titles/descriptions.
  - Hidden text never matches search; ancestor labels are also redacted.
  - Authored scope limits the listing. It is not permission to reveal hidden original content.

## Unicode and collation

- `Latin1_General_100_CI_AI_SC` is explicit on node titles and block Markdown, AltText, Caption, Title and Description.

| Suffix | Meaning | Example or purpose |
|---|---|---|
| CI | Case-insensitive comparison | `CAFE` and `cafe` compare equivalently |
| AI | Accent-insensitive comparison | `cafe` can find `Café` |
| SC | Supplementary-character support | Supports Unicode characters beyond the basic multilingual plane |

- Existing `nvarchar` storage preserves the original text. Matching `Café` with `cafe` does not rewrite its spelling, and no UTF-8 storage conversion is required.

- SQL defines the search/title ordering policy. One collation is not a promise of culturally ideal sorting for every language or exact equivalence with every .NET linguistic comparer.

- URL identity/equality, Identity identifiers and unique community/reaction keys retain their existing rules. URL text participating in concatenated Discovery search/display receives explicit expression collation to avoid conflicts.

## Console wiring and cache ownership

- [Program.cs](../../src/Atlas.Console/Program.cs) is the **composition root**: the startup code that constructs and connects these components.

```csharp
// Namespace prefixes omitted for readability.
using var discoveryCache = new DiscoveryCache();
IDiscoveryService discovery = new DiscoveryService(
    new SqlDiscoveryReadQuery(SqlStorage.Open, discoveryCache));
```

- **Console creates, owns and disposes the cache.** It reuses the same instance across browsing requests.

- **The adapter receives it.** `SqlDiscoveryReadQuery` does not create a cache lazily. `SqlStorage.Open` supplies a fresh context for each read.

- **The service receives the adapter.** `DiscoveryService` validates/delegates; it does not manage cache lifetime.

- **Caching is optional.** Omitting `DiscoveryCache? cache = null` disables both page and detail reuse; the database read still works. Normal Console startup supplies the cache.

### Future REST API wiring

- Provide an application-lifetime cache per database configuration if reuse is wanted. A new cache per HTTP request would lose cross-request reuse.

- Supply a context factory and dispose each context after its read. Do not capture a request-scoped context in a long-lived adapter/cache.

- Inject the adapter into `DiscoveryService` and use the service from endpoints. Keep personal participant ratings out of shared cards.

- Each API process has its own cache; it does not inherit Console memory. Committed database revisions coordinate freshness across instances.

- This is host-wiring guidance, not an implemented API registration. Review the serializing gate and transaction behavior for concurrent HTTP workloads before reusing them unchanged.

## Two bounded cache layers

| Layer | Stored data | Default budget/lifetime |
|---|---|---|
| Ranked page | Selected IDs, global ranks, scores, statistics and total | 1 MiB estimated sizes; 30-second absolute expiration |
| Detail card | Immutable public node display fields and bounded breadcrumb segment | 8 MiB estimated sizes; 30-second absolute expiration; maximum entry 128 KiB |
| Personal ratings | Queried fresh for the authenticated participant and selected IDs | Not shared/cached |

- **Page key** — [`PageKey` construction](https://github.com/williamowen65/atlas-the-public-think-tank/blob/256fb2c79a843b777ec999bad87602c49b90e85b/src/Atlas.Persistence/Discovery/DiscoveryCache.cs#L71): a hash of filters, page number/size, authored scope and visibility policy, plus an ordering-policy prefix and committed page revision.
  - It is a cache lookup key, not a cursor or authorization token.
  - Search text is trimmed; set-like inputs are sorted/deduplicated.
  - Case/accents are preserved in the key rather than assuming SQL and .NET normalization are identical.

- **Detail key** — [lookup in `TryDetail`](https://github.com/williamowen65/atlas-the-public-think-tank/blob/256fb2c79a843b777ec999bad87602c49b90e85b/src/Atlas.Persistence/Discovery/DiscoveryCache.cs#L143) and [storage in `PutDetail`](https://github.com/williamowen65/atlas-the-public-think-tank/blob/256fb2c79a843b777ec999bad87602c49b90e85b/src/Atlas.Persistence/Discovery/DiscoveryCache.cs#L158): `(DetailVersion, NodeId)`. It is independent of the filter and result-page number.
  - Example: changing a filter returns the same three IDs → recompute the ranked page → reuse those three display cards.
  - If it returns uncached IDs, their cards must be loaded.

- **Size budgets** — [constructor budgets](https://github.com/williamowen65/atlas-the-public-think-tank/blob/256fb2c79a843b777ec999bad87602c49b90e85b/src/Atlas.Persistence/Discovery/DiscoveryCache.cs#L33), [page size accounting](https://github.com/williamowen65/atlas-the-public-think-tank/blob/256fb2c79a843b777ec999bad87602c49b90e85b/src/Atlas.Persistence/Discovery/DiscoveryCache.cs#L125) and [detail size accounting](https://github.com/williamowen65/atlas-the-public-think-tank/blob/256fb2c79a843b777ec999bad87602c49b90e85b/src/Atlas.Persistence/Discovery/DiscoveryCache.cs#L158) — **are estimates**, not exact managed-heap measurements. Oversized content is still returned to the caller but is not cached.

- [Counters](https://github.com/williamowen65/atlas-the-public-think-tank/blob/256fb2c79a843b777ec999bad87602c49b90e85b/src/Atlas.Persistence/Discovery/DiscoveryCache.cs#L26) expose hits, misses and oversized-entry rejections. See [page lookup](https://github.com/williamowen65/atlas-the-public-think-tank/blob/256fb2c79a843b777ec999bad87602c49b90e85b/src/Atlas.Persistence/Discovery/DiscoveryCache.cs#L110) and [detail lookup](https://github.com/williamowen65/atlas-the-public-think-tank/blob/256fb2c79a843b777ec999bad87602c49b90e85b/src/Atlas.Persistence/Discovery/DiscoveryCache.cs#L143) for hit/miss increments. No DbContext, EF-tracked entity, mutable domain aggregate or personal vote is cached.

- [`BindDatabase`](https://github.com/williamowen65/atlas-the-public-think-tank/blob/256fb2c79a843b777ec999bad87602c49b90e85b/src/Atlas.Persistence/Discovery/DiscoveryCache.cs#L56) remembers the connection configuration and rejects sharing one cache across different configurations. It does not locate or create a database.

- [The gate used by `Read`](https://github.com/williamowen65/atlas-the-public-think-tank/blob/256fb2c79a843b777ec999bad87602c49b90e85b/src/Atlas.Persistence/Discovery/SqlDiscoveryReadQuery.cs#L78) is an in-process lock coordinating reads and counter updates. `_uncachedGate` is the fallback lock object when there is no cache—not another cache.
  - [Shared cache gate](https://github.com/williamowen65/atlas-the-public-think-tank/blob/256fb2c79a843b777ec999bad87602c49b90e85b/src/Atlas.Persistence/Discovery/DiscoveryCache.cs#L25) coordinates adapters sharing that cache; [`_uncachedGate`](https://github.com/williamowen65/atlas-the-public-think-tank/blob/256fb2c79a843b777ec999bad87602c49b90e85b/src/Atlas.Persistence/Discovery/SqlDiscoveryReadQuery.cs#L55) belongs to the uncached adapter instance.
  - This deliberately serializes reads through that gate.
  - It is a simple correctness baseline, not a high-throughput distributed-cache design.

## Cache counters are diagnostic metrics

- [DiscoveryCache](../../src/Atlas.Persistence/Discovery/DiscoveryCache.cs) stores these counters as properties on the cache object. They describe observed cache activity; they do not decide whether an entry is reusable or control invalidation.

| Counter | What increments it |
|---|---|
| PageHits | A TryPage lookup finds a reusable ranked page |
| PageMisses | A TryPage lookup does not find its key |
| DetailHits | A TryDetail lookup finds a reusable card |
| DetailMisses | A TryDetail lookup does not find its version/ID key |
| RejectedEntries | PutPage/PutDetail rejects an entry because its estimated size exceeds the configured per-entry/budget check |

- **Lifetime:** totals accumulate for one cache instance. Creating a new cache starts them at zero. Different application processes have separate counters; they are not persisted in the database.

- **Current consumers:** cache tests assert behavior, performance tests include the counters in measurement reports, and a developer can inspect the object in the debugger. They are not currently exported to a runtime dashboard or monitoring service.

- **Hits count lookups, not distinct nodes.** Reading the same three cards twice adds six detail hits. A page hit may still need card reads if detail entries are missing.

- **Misses count attempted cache lookups.** Requests with no cache do not increment these counters. Unpaged requests bypass TryPage, although they may still attempt detail lookups. A page miss does not necessarily indicate a data change: first use, new filters/pages, expiration and eviction can also produce misses.

- **Rejections are not eviction totals.** MemoryCache may expire, evict or decline retention under its aggregate budget; RejectedEntries records only this wrapper's explicit oversized-entry checks. It is not a count of every unsuccessful insertion or every database error.

- **Example hit rate:** page hits divided by total page lookups, using floating-point arithmetic:

```csharp
var lookups = cache.PageHits + cache.PageMisses;
double? hitRate = lookups == 0
    ? null // No lookups yet; a rate is not meaningful.
    : (double)cache.PageHits / lookups;
```

- Compare rates with the workload, query counts and elapsed times. A low rate can be expected when exploring many different filters; a high rate alone does not prove good performance or correctness.

- Counter updates occur under the query adapter's cache gate. A future concurrent monitoring reader should obtain a consistent snapshot under that same gate, or use an appropriate metrics/synchronization implementation; unsynchronized separate property reads are not guaranteed to describe one instant.

## When to change the page-key prefix (`order-v2`)

- **Location:** the literal `order-v2:` returned by [DiscoveryCache.PageKey](../../src/Atlas.Persistence/Discovery/DiscoveryCache.cs). Change it in the same change that introduces incompatible ranked-page behavior, before that behavior is released.

- **Key structure:** `order-v2:<PageVersion>:<request hash>`.
  - The prefix versions the code's ordering/query contract.
  - `PageVersion` tracks committed data changes.
  - The request hash identifies filters, scope and the requested page.
  - `v2` does not mean page two or database revision two.

- **Rule:** bump to `order-v3` (then v4, etc.) when an identical request and database revision could produce a page whose membership, order, ranks, totals or statistics should no longer be considered equivalent to the previous implementation.
  - Do not reuse an older prefix for a different contract.

| Example change | Bump the prefix? | Reason |
|---|---|---|
| Change participation bonus or title-match boost | Yes | Identical data/request may rank differently |
| Add relevance to importance scoring | Yes | The ranked-page contract changes |
| Change a tie-breaker or title collation | Yes | Tied nodes may move between pages |
| Change existing search/filter/visibility semantics | Yes | The same request may select different nodes or totals |
| Correct an ordering, total or statistics bug | Yes, if cached results would differ | Old cached pages reflect the behavior being corrected |
| Cast a vote, edit content, hide/restore a node | No | Relevant writes advance database revisions instead |
| Request another page or change a filter value | No | Request fields already change the hash |
| Change page size | Not solely for the size | PageSize is already included in the hash; bump if ranking/paging semantics also change |
| Refactor EF code while preserving results | No | Equivalent pages remain compatible |
| Change logging, comments, indexes or TTL | No, absent a behavior change | These do not change the meaning of ranked results |
| Change card formatting or breadcrumb display only | Not the page prefix alone | Detail entries use separate keys; review their own invalidation/lifetime |

- **Why:** a new prefix creates a separate cache namespace. Entries under the previous prefix cannot satisfy lookups under the new one, even when data revisions and filter hashes match.
  - Old entries can expire or be evicted normally; a prefix bump does not physically delete them.
  - It does not update the database revision row or require a schema migration by itself.

- **Current limitation:** caches are process-local and normally start empty after an application restart. A normal restarted deployment already loses its old entries; the prefix still makes compatibility explicit in code and protects against reuse if entries survive a behavior transition or caching is later extended.
  - It is not a deployment coordination mechanism: an old running instance continues using its old code/prefix.
  - It does not invalidate detail cards or fix direct writes that bypass revision updates.

- **Review steps:** decide whether behavior changed; bump the literal if necessary; update behavior assertions and any key tests/docs; explain the contract change in the PR. Preserve the prefix for a verified behavior-equivalent optimization.

## Cache invalidation: when old entries stop being reusable

- [DiscoveryRevisionRow](../../src/Atlas.Persistence/Discovery/DiscoveryRevisionRow.cs) is a seeded singleton row, Id = 1, with `PageVersion` and `DetailVersion`.
  - These are freshness generations, not node limits, page numbers or timestamps.

- [DiscoveryInvalidation](../../src/Atlas.Persistence/Discovery/DiscoveryInvalidation.cs) classifies pending tracked writes. [AtlasDataContext.SaveChanges](../../src/Atlas.Persistence/AtlasDataContext.cs) advances the applicable counters once per relevant save.

| Change | Page version | Detail version |
|---|---|---|
| Node importance votes | Advance | Retain |
| Reaction votes | Advance conservatively | Advance |
| Node metadata, topology, types/requested types | Advance | Advance |
| Documents, blocks and their links | Advance | Advance |
| Community records/links, reaction associations/definitions | Advance | Advance |
| Moderation hide/restore | Advance | Advance |
| Participant labels and comments | Advance conservatively | Advance |
| Unrelated Identity tokens, notification delivery and quota ledger | Retain | Retain |

- **Example: node vote.** Ranking/statistics may change, so the page key changes. Its public description and labels have not changed, so detail cards can remain reusable.

- **Example: moderation hide.** Both versions advance. Neither the old ranked page nor its old card may be reused.

- Invalidation is deliberately database-wide, not per-node. Relevant metadata changes make older detail keys unusable across all cards. Finer-grained invalidation is a later optimization to measure.

- [Revision-aware page-key lookup](https://github.com/williamowen65/atlas-the-public-think-tank/blob/256fb2c79a843b777ec999bad87602c49b90e85b/src/Atlas.Persistence/Discovery/SqlDiscoveryReadQuery.cs#L98) and [detail lookup](https://github.com/williamowen65/atlas-the-public-think-tank/blob/256fb2c79a843b777ec999bad87602c49b90e85b/src/Atlas.Persistence/Discovery/SqlDiscoveryReadQuery.cs#L168) make old entries unusable without deleting them immediately: new revisions produce different keys. Expiration/eviction governs retention; revision checking governs committed freshness.

## Transactions, commits and rollbacks

- A relevant write and its revision increment happen in the **same transaction**. SaveChanges joins an existing repository transaction or owns one when needed.
  - Failure or outer rollback undoes both the data and the counter increments.

- Each read checks the committed revision under a shared serializable lock before using caches. Writers update that row under an exclusive transaction lock.
  - The read holds its lock until count/page/card work finishes.
  - A committed visibility change cannot be bypassed by using an older cache generation.
  - Another write may commit after the read finishes; this guarantee concerns that read's committed view, not indefinite freshness of data already displayed.

- [Read transaction commit](https://github.com/williamowen65/atlas-the-public-think-tank/blob/256fb2c79a843b777ec999bad87602c49b90e85b/src/Atlas.Persistence/Discovery/SqlDiscoveryReadQuery.cs#L189) precedes [page publication](https://github.com/williamowen65/atlas-the-public-think-tank/blob/256fb2c79a843b777ec999bad87602c49b90e85b/src/Atlas.Persistence/Discovery/SqlDiscoveryReadQuery.cs#L195) and [detail publication](https://github.com/williamowen65/atlas-the-public-think-tank/blob/256fb2c79a843b777ec999bad87602c49b90e85b/src/Atlas.Persistence/Discovery/SqlDiscoveryReadQuery.cs#L199). New cache entries are published only after the read transaction commits. Failed read attempts are not published.

- Direct administrative SQL, imports and ExecuteUpdate/ExecuteDelete can bypass SaveChanges. They must advance both revisions in the same transaction as relevant changes:

```sql
UPDATE DiscoveryRevisions
SET PageVersion = PageVersion + 1,
    DetailVersion = DetailVersion + 1
WHERE Id = 1;
```

- Do not reset the live revision row while processes retain cached entries. Stop/restart application processes for a complete database reset.

## Breadcrumb pages toward the root

- [DiscoveryBreadcrumbReader](../../src/Atlas.Persistence/Discovery/DiscoveryBreadcrumbReader.cs) reads **up to three ancestor nodes**, nearest first, along one route.
  - It uses up to three batched EF hop queries for selected cards.
  - It follows the first stored parent position by default and flags alternatives.
  - It does not expand every route or load a fourth ancestor to detect continuation.

- [`DiscoveryCard.BreadcrumbSegment`](../../src/Atlas.Discovery/DiscoveryCard.cs) contains items, a continuation node ID, an alternative-route indicator and a cycle indicator.
  - Example: child → A → B → C → D. First segment: A, B, C. Continue above C to receive D and any further ancestors.
  - `ReadPage` accepts visited IDs to stop cycles across successive segments.

  - The host (Console today; an API later) controls the database transaction around a breadcrumb read. The reader does not start or retain a transaction itself.

  - A transaction can keep the queries for **one segment** consistent. It does not freeze the graph while a person reads the screen and later requests the next segment.

  - Example: you receive A, B and C. Before you continue above C, someone changes C’s parent from D to E. The next request can show E, because it reads the current committed graph. Keeping the same route across requests would require a separate snapshot/version contract; that is not implemented here.

- Display text ends with an ellipsis when more ancestors exist. Hidden ancestors use moderation notices. Roots, unavailable links or cycles can end a segment early.

- Console Explore parents offers three ancestors at a time; N continues above the last one. Immediate-parent route choices are also offered in sets of three.
  - Inside a segment, the first stored parent is provisional.
  - Open an intermediate ancestor and explore its parents to choose another branch there.
  - The future scrollable breadcrumb/path-feed route-switching UI remains open.

### Which breadcrumb reads use the cache?

- **Discovery card display:** the detail cache stores both the breadcrumb text and [`DiscoveryCard.BreadcrumbSegment`](../../src/Atlas.Discovery/DiscoveryCard.cs). A detail-cache hit reuses those first three ancestor labels and continuation/route indicators. See [card loading and cache lookup](../../src/Atlas.Persistence/Discovery/SqlDiscoveryReadQuery.cs).

- **Console ancestor navigation:** opening a node or using **Explore parents**, including **N** for another segment, uses [AncestorNavigation](../../src/Atlas.Application/Queries/AncestorNavigation.cs) through the node repository. This path does **not** reuse the discovery detail cache. An ancestor appearing in a cached discovery card does not mean navigating to that ancestor retrieves its node from that cache.

- Relevant topology, title and moderation changes advance the detail revision, so subsequent discovery reads stop reusing breadcrumb data from older revisions. See [DiscoveryInvalidation](../../src/Atlas.Persistence/Discovery/DiscoveryInvalidation.cs).

- **Possible future improvement:** if measurements show repeated ancestor navigation is expensive, share a breadcrumb reader and cache bounded segments by detail revision, starting node and selected parent route. Preserve visited-node cycle checks for each navigation request, and apply expiry/memory limits. This additional navigation cache is not implemented.

### Try the longer demo route in the Console

- [SeedBreadcrumbDemo.cs](../../src/Atlas.Infrastructure/Storage/SeedData/SeedBreadcrumbDemo.cs) adds a ten-link chain below the existing Atlas root, plus community and research routes that intersect the chain. First-parent ordering keeps the long chain as the default route.

- Search discovery for **Breadcrumb demo: ten-level route**, then open that node.

- Its first breadcrumb segment shows levels **9 → 8 → 7 → …**. Continue above level 7 to see the alternative-route notice: that next segment examines level 7’s two parent links. The first segment does not inspect a fourth hop merely to find alternatives above its endpoint.

- Choose **Explore parents** (action 24), then **N** to continue toward the root. The default segments contain levels **9, 8, 7**, then **6, 5, 4**, then **3, 2, 1**, then the **Atlas root**. The last segment has no ellipsis.

- To try another route, open **level 7** and explore its parents: choose level 6 for the primary chain or **research route** for the alternate. Level 4 also offers **community route**. These branches reconnect with earlier ancestors without creating cycles.

- This is seed content, so **no new schema migration is needed**. Run `dotnet run --project src/Atlas.Console -- --seed-demo` against an empty development database, then launch normally. The command refuses an already populated database.

- For an existing local demo database, either configure a fresh development database or follow the [local reset instructions](../../src/Atlas.Persistence/README.md#reset-the-local-database-to-demo-seed-data-powershell). Resetting deletes that database’s accounts and content; it is not required just to update application code.

## Why some raw SQL remains

- **Breadcrumb recursion no longer requires SQL.** The three-hop bound made it practical to express traversal in EF.

- [DiscoverySearchContent.sql](../../src/Atlas.Persistence/Discovery/DiscoverySearchContent.sql) preserves ordered concatenation across description fields/blocks. Checking each block independently would lose matches spanning boundaries.

- [DiscoveryCards.sql](../../src/Atlas.Persistence/Discovery/DiscoveryCards.sql) preserves ordered display aggregation. Its child-type JSON could move into another EF batch, but remains here to avoid a further round trip.

- Ordered aggregation uses SQL Server-specific `STRING_AGG ... WITHIN GROUP`; the current query does not have a suitable ordered aggregate LINQ translation. Atomic revision increments also remain parameterized SQL.

- EF expresses more of the implementation, but portability is not automatic. Changing providers still requires reviewing collations, translations and these SQL helpers.

## Database reads and performance limits

| Request | Expected reads |
|---|---:|
| Cold root-only page | 6 |
| Cold page with deeper breadcrumbs | Up to 8 |
| Fully cached page | 1 |
| Changed filter reusing all selected cards | 3 |
| Beyond-last page | 2 |

- Cold work consists of revision, total count, ranked IDs, EF metadata, SQL summaries and one to three breadcrumb hops. Hop queries batch selected IDs rather than querying each card separately.

- Authenticated personal ratings add a separate batch. These numbers count Discovery reads, not every operation in the Console.

- The counts document this implementation; they are not a claim of the fewest possible reads. Batching differently could reduce round trips at the expense of query complexity.

- Returning only three results bounds application materialization. SQL may still examine thousands of candidates to rank them or inspect their text. Database-side paging does not make database cost constant.

- Vote, reaction and moderation indexes support the query paths, alongside document/parent/community and primary-key indexes. Plans and realistic workloads should guide later search, statistics, invalidation or cursor changes.

## Tests and evidence

| Test group | What it checks |
|---|---|
| DiscoverySqlParityTests | Expected filters, ranking, totals, paging, authored visibility and title boosts |
| DiscoveryEdgeCaseTests | Accent/GUID ties, calendar dates, extreme bounds, literal wildcards and cross-block search |
| DiscoveryCacheTests | Reuse, invalidation, commits/rollbacks, misses, cross-instance visibility, oversized entries and personal isolation |
| DiscoveryBreadcrumbTests | Bounded deep segments, continuation, branches, roots, hidden ancestors and cycles |
| AncestorNavigationTests | Console route choice, bounded reads and continuation |
| DiscoveryPerformanceTests | 100/1,000/10,000-node root fixtures, query counts, timings, reserved space and actual plans |

- SQL behavior tests run against isolated databases; their fixture records are test data, not a second in-memory query implementation.

- The SQL Discovery CI job uploads `discovery-sql-evidence`. See the [performance guide](discovery-performance.md) for interpretation and linked read locations.

- Earlier raw-SQL and pre-breadcrumb timings/plans are historical. Use the matching commit/run's evidence when evaluating the current implementation.

## Migration status and developer workflow

- The developer-generated collation/index/revision-table migration is already committed in this PR. The later EF/readability/breadcrumb changes do not require another migration.

- [Persistence AGENTS.md](../../src/Atlas.Persistence/AGENTS.md) requires AI to change model configuration and provide commands, leaving migration generation/review to the developer.

- For a future model change, run commands individually from the solution root, using a migration name describing that change:

```powershell
dotnet tool restore
dotnet ef migrations add YourDescriptiveMigrationName --project src/Atlas.Persistence
# Review the migration, designer and snapshot before applying.
dotnet ef migrations has-pending-model-changes --project src/Atlas.Persistence
dotnet ef database update --project src/Atlas.Persistence
dotnet test tests/Atlas.Persistence.Tests/Atlas.Persistence.Tests.csproj
dotnet test tests/Atlas.Discovery.Tests/Atlas.Discovery.Tests.csproj
dotnet test tests/Atlas.Console.Tests/Atlas.Console.Tests.csproj
dotnet test tests/Atlas.Moderation.Tests/Atlas.Moderation.Tests.csproj
```

- Preserve developer-generated migration history. If a collation-only migration has already been applied/committed, retain it and generate any remaining changes separately rather than removing it.

- Current-model Discovery fixtures use EnsureCreated on disposable, uniquely named databases. Ordinary migrated integration jobs and pending-model checks separately verify the migration path. EnsureCreated alone does not prove deployability through migrations.

## References

- [SQL Server collation and Unicode support](https://learn.microsoft.com/en-us/sql/relational-databases/collations/collation-and-unicode-support)

- [Column collation changes](https://learn.microsoft.com/en-us/sql/relational-databases/collations/set-or-change-the-column-collation)

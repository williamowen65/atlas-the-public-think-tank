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

- **Page key:** a hash of filters, page number/size, authored scope and visibility policy, plus an ordering-policy prefix and committed page revision.
  - It is a cache lookup key, not a cursor or authorization token.
  - Search text is trimmed; set-like inputs are sorted/deduplicated.
  - Case/accents are preserved in the key rather than assuming SQL and .NET normalization are identical.

- **Detail key:** `(DetailVersion, NodeId)`. It is independent of the filter and result-page number.
  - Example: changing a filter returns the same three IDs → recompute the ranked page → reuse those three display cards.
  - If it returns uncached IDs, their cards must be loaded.

- **Size budgets are estimates**, not exact managed-heap measurements. Oversized content is still returned to the caller but is not cached.

- Counters expose hits, misses and oversized-entry rejections. No DbContext, EF-tracked entity, mutable domain aggregate or personal vote is cached.

- `BindDatabase` remembers the connection configuration and rejects sharing one cache across different configurations. It does not locate or create a database.

- The gate is an in-process lock coordinating reads and counter updates. `_uncachedGate` is the fallback lock object when there is no cache—not another cache.
  - This deliberately serializes reads through that gate.
  - It is a simple correctness baseline, not a high-throughput distributed-cache design.

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

- Old entries need not be physically deleted immediately: new revisions produce different keys. Expiration/eviction governs retention; revision checking governs committed freshness.

## Transactions, commits and rollbacks

- A relevant write and its revision increment happen in the **same transaction**. SaveChanges joins an existing repository transaction or owns one when needed.
  - Failure or outer rollback undoes both the data and the counter increments.

- Each read checks the committed revision under a shared serializable lock before using caches. Writers update that row under an exclusive transaction lock.
  - The read holds its lock until count/page/card work finishes.
  - A committed visibility change cannot be bypassed by using an older cache generation.
  - Another write may commit after the read finishes; this guarantee concerns that read's committed view, not indefinite freshness of data already displayed.

- New cache entries are published only after the read transaction commits. Failed read attempts are not published.

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

- `DiscoveryCard.BreadcrumbSegment` contains items, a continuation node ID, an alternative-route indicator and a cycle indicator.
  - Example: child → A → B → C → D. First segment: A, B, C. Continue above C to receive D and any further ancestors.
  - `ReadPage` accepts visited IDs to stop cycles across successive segments.
  - Hosts own transaction scope; separate requests do not promise one frozen graph view.

- Display text ends with an ellipsis when more ancestors exist. Hidden ancestors use moderation notices. Roots, unavailable links or cycles can end a segment early.

- Console Explore parents offers three ancestors at a time; N continues above the last one. Immediate-parent route choices are also offered in sets of three.
  - Inside a segment, the first stored parent is provisional.
  - Open an intermediate ancestor and explore its parents to choose another branch there.
  - The future scrollable breadcrumb/path-feed route-switching UI remains open.

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

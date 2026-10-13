# SQL-backed Discovery and layered caching (PTT-132)

Discovery runs through `DiscoveryService` → `IDiscoveryReadQuery` → `SqlDiscoveryReadQuery` in Persistence. Console uses this SQL implementation for anonymous, authenticated and authored listings. There is no production in-memory query or candidate-source adapter. The previous behavior assertions now run against isolated SQL fixtures; their input records are test data, not a second ranking implementation.

## Query and display behavior

SQL applies all supported filters, computes vote aggregates and ranking scores, counts matches, orders results, and applies OFFSET/FETCH before materialization. Normal pages contain three ranked IDs/statistics. EF metadata and SQL summary batches return immutable display cards for only those IDs. Up to three batched EF ancestor hops provide bounded breadcrumb segments; all complete ancestor routes are no longer expanded. Console renders those cards without per-node repository calls. It loads a domain Node only when the user opens/reports it. Authenticated participant profiles obtain authored totals from the SQL listing rather than loading every authored aggregate.

The score remains `(AverageVote ?? 0) + log10(VoteCount + 1) / 100`, plus 0.1 for a title search match. Order is score descending, vote count descending, updated time descending, title ascending under the selected collation, then GUID ascending. Canonical GUID text under `Latin1_General_100_BIN2` preserves .NET Guid ordering rather than SQL Server's native uniqueidentifier ordering. Ranks are global across pages. Offset calculations use long arithmetic, including pages beyond the result set; there is no candidate cap.

Text search uses EF Contains translation with escaped parameters, so `%`, `_` and `[` are literal search characters. Searchable block fields and ordered space-separated concatenation preserve searches spanning adjacent fields or blocks. Poll IDs are not searchable. Community/reaction membership uses EXISTS predicates, avoiding join multiplication of votes. Every selected reaction must be active; definition suppression affects card display but does not alter the existing association-filter rule. Average bounds exclude unvoted nodes. Dates compare the timestamp's stored calendar day with SQL date parameters, including the full DateOnly range.

Count, page and cards share a serializable read transaction. Each request reflects one committed database view. Separate page requests can shift when votes/content change; cursor paging would require a product/API decision about ranking changes. Unpaged methods remain for existing contracts/tests, but Console page flows do not call them.

## Unicode and collation

`Latin1_General_100_CI_AI_SC` is explicit on NodeRows.Title and BlockRows Markdown, AltText, Caption, Title and Description. Existing nvarchar storage preserves original Unicode text and spelling; CI ignores case, AI ignores accents and SC supports supplementary characters. No UTF-8 storage conversion is required. SQL is the authoritative text ordering/search implementation; there is no requirement to match a .NET linguistic comparer for every Unicode sequence. The intentional policy change means cafe finds Café.

URL columns retain their previous identity/equality semantics. URL text participating in Discovery concatenation receives the search/display collation at the expression boundary to avoid collation conflicts. Identity/account identifiers and unique community/reaction keys retain their existing rules. This is a Discovery text policy, not a database-wide change or a claim that one collation gives culturally correct ordering for every language.

## Host wiring and cache lifetime

- **Console is the composition root:** [Program.cs](../../src/Atlas.Console/Program.cs) creates the cache and connects the Discovery components. A composition root is the host startup code that constructs and connects services.
- The startup sequence is equivalent to the following (namespace prefixes omitted for readability):

```csharp
using var discoveryCache = new DiscoveryCache();
IDiscoveryService discovery = new DiscoveryService(
    new SqlDiscoveryReadQuery(SqlStorage.Open, discoveryCache));
```

- **Console creates and owns `DiscoveryCache`.** The same instance is reused across browsing requests, and `using var` disposes it when the owning startup scope exits.
- **`SqlDiscoveryReadQuery` receives the cache.** It does not create one lazily. It also receives `SqlStorage.Open`, the factory used to create a fresh EF context for each read.
- **`DiscoveryService` receives the query adapter**, validates requests and delegates. It does not own cache creation or disposal.
- The constructor's `DiscoveryCache? cache = null` permits callers/tests to omit caching. Without a cache, both page and detail reuse are disabled; the database read still works. Normal Console startup supplies the cache.
- Cache entries live only in that process. Another Console instance or API process has independent memory caches; committed database revisions coordinate their freshness.

### Future REST API host

- Its startup/dependency-injection wiring must explicitly provide the cache if reuse is wanted; it will not inherit the Console's cache.
- Plan one host-managed cache per database configuration with application lifetime. Recreating it per HTTP request would discard cross-request reuse. Dispose it at host shutdown.
- Supply a context factory so reads create/dispose their own EF contexts. Do not capture a request-scoped context inside an application-lifetime query adapter or cache.
- Wire the query adapter into `DiscoveryService`, then use the service from endpoints.
- Keep authenticated participant ratings outside the shared public cache.
- This is a wiring guide, not an implemented API registration. Review the current serializing gate and database transaction behavior for concurrent HTTP workloads before carrying it over unchanged.

## Bounded caches

Console owns and disposes one `DiscoveryCache` per process/database. Page caching can be disabled by constructing the SQL adapter without a cache. No DbContext, EF-tracked row, mutable domain aggregate or personalized vote is cached.

| Layer | Entry and key | Default budget/freshness |
|---|---|---|
| Ranked page | Immutable IDs, global ranks, scores, statistics and total; SHA-256 of normalized filters, ordered/deduplicated reaction IDs and authored scope, page, size, ordering version, visibility scope and committed page generation | 1 MiB estimated entry sizes; 30-second absolute expiration |
| Display detail | Immutable public card keyed by node identity and committed detail generation; independent of filter/page | 8 MiB estimated entry sizes; 30-second absolute expiration; maximum entry 128 KiB |
| Personalized ratings | Fresh SQL batch keyed by the authenticated host participant and selected page IDs | Never shared/cached |

Whitespace normalization, sorted sets and explicit scope fields make equivalent requests reusable without assuming Unicode case-folding equivalence outside SQL. A changed filter can miss the page cache while reusing the same detail cards. Cache counters expose page/detail hits/misses and oversized-entry rejections. Oversized descriptions are returned without being cached. The cache rejects reuse across different database connection configurations.

A fixed instance gate coordinates concurrent misses without an unbounded dictionary of per-filter locks. This simple Console baseline serializes reads within one instance; it does not pretend to be a high-throughput distributed cache. Other instances have independent bounded memory caches and coordinate freshness through the database generation row. Expiration controls reuse/memory, not moderation correctness.

## Commit, rollback and visibility correctness

The model defines a seeded singleton `DiscoveryRevisions` row with separate page and detail generations. `AtlasDataContext.SaveChanges` and its async counterpart increment generations atomically in the same transaction as relevant writes, joining an existing repository operation transaction when present. An unsuccessful save or outer rollback rolls back the generation update as well. New cache entries are published only after a successful read transaction completes.

Each request reads the committed generation row under a shared serializable lock before using cached page/detail entries. Writers update that row under an exclusive transaction lock. Consequently a committed hide, restore, archive, rating or content change cannot reuse an earlier generation; a cached page cannot bypass current visibility. The read lock stays until the page/cards are resolved. As with any ordinary read, a later commit can occur after its read transaction completes; this is not indefinite stale-cache exposure.

| Change | Page generation | Detail generation |
|---|---|---|
| Node importance votes | Advance: ranking/statistics may change | Retain: title/body/card metadata unchanged |
| Reaction votes | Advance conservatively | Advance: reaction scores change |
| Node creation/title/type/author/archive/restore; topology/requested types | Advance | Advance |
| Documents, block contents and document/block links | Advance | Advance |
| Community records/links; reaction associations/definitions | Advance | Advance |
| Moderation decisions/hide/restore | Advance | Advance |
| Participant labels and comments | Advance conservatively | Advance |
| Unrelated Identity tokens, notification delivery and quota ledger entries | Retain | Retain |

Detail invalidation is deliberately coarse across all cards for relevant metadata changes; page invalidation is database-wide. This trades fine-grained write throughput for a small, auditable correctness baseline. It still provides detail reuse across filter changes and node-rating updates. Per-node versions/read models are possible later if measurement justifies them.

Public queries exclude hidden nodes. Authored queries include only their supplied IDs or requested author and render hidden titles/descriptions as moderation placeholders. Hidden text never matches search. Authored scope is a listing restriction, not authorization to reveal original content. Ancestor paths redact hidden ancestors. SQL integration tests cover hide/restore across independently cached instances.

All application writes affecting Discovery must use AtlasDataContext.SaveChanges. Direct administrative SQL, ExecuteUpdate/ExecuteDelete and imports that bypass it must advance both generations in the same transaction as their data changes:

```sql
UPDATE DiscoveryRevisions
SET PageVersion = PageVersion + 1, DetailVersion = DetailVersion + 1
WHERE Id = 1;
```

Do not reset a live database's generation row while processes retain cached entries; stop/restart the application for a complete database reset.

## Verification and profiling

`DiscoverySqlParityTests` retains explicit assertions for ordering, paging, totals, filters, authored visibility and title-match boosts. `DiscoveryEdgeCaseTests` covers accent-insensitive title ties, equivalent GUID tie-breaking, timestamp calendar dates, full date bounds, literal wildcard input and searches across ordered blocks. `DiscoveryCacheTests` covers filter-change detail reuse, vote updates, successful commits, failed async saves, explicit rollbacks, concurrent misses, separate app-instance caches, hide/restore, content/relationship changes, oversized entries and personalized isolation. These exercise SQL, not an in-memory query.

`DiscoveryPerformanceTests` uses 100, 1,000 and 10,000 nodes with persisted ratings. It asserts three materialized ranked results/cards per page and query counts of six on a cold root-only page, one on a warm page, three after a filter change that reuses cards, six on a new root-only page and two beyond the result set. It captures timings, reserved database bytes and actual SQL execution plans for unfiltered count/ranking/cards and text-filtered count/ranking. The SQL CI job uploads `discovery-sql-evidence`; measurements and plan review are recorded in [the performance notes](discovery-performance.md).

The model adds target-first vote, active-reaction membership and moderation-visibility indexes. Existing document-link, parent, community-link and primary-key indexes support batched joins. Free-text substring search still requires examining matching text and ranking requires processing matching statistics; database-side execution does not make those costs disappear. Plans and larger workload measurements should guide any later full-text search, cursor API or derived-summary work.

## Developer-generated migration

AI instructions in `src/Atlas.Persistence/AGENTS.md` prohibit generating or editing migrations/snapshots. The model changes are complete, but the developer must generate/review/commit the collation, indexes and seeded revision-table migration. No migration or snapshot is authored by this implementation.

From the solution root, run these commands individually in PowerShell:

```powershell
dotnet tool restore
dotnet ef migrations add CompleteDiscoveryReadModel --project src/Atlas.Persistence
# Review the generated migration, designer and snapshot before applying.
dotnet ef migrations has-pending-model-changes --project src/Atlas.Persistence
dotnet ef database update --project src/Atlas.Persistence
dotnet test tests/Atlas.Persistence.Tests/Atlas.Persistence.Tests.csproj
dotnet test tests/Atlas.Discovery.Tests/Atlas.Discovery.Tests.csproj
dotnet test tests/Atlas.Console.Tests/Atlas.Console.Tests.csproj
dotnet test tests/Atlas.Moderation.Tests/Atlas.Moderation.Tests.csproj
```

If a local collation-only migration has already been generated but has not been applied or committed, remove it with EF tooling before generating the combined migration. If it has already been applied/committed, keep it and generate an additional migration for the remaining read-model changes.

SQL Discovery CI uses EnsureCreated only for disposable, uniquely named current-model fixtures. It validates behavior before the developer creates migrations. The ordinary migration/model check and migrated repository integration jobs remain intact and require the generated migration. Current-model tests are not evidence that a missing migration can deploy successfully.

References: [SQL Server collation and Unicode support](https://learn.microsoft.com/en-us/sql/relational-databases/collations/collation-and-unicode-support) and [column collation changes](https://learn.microsoft.com/en-us/sql/relational-databases/collations/set-or-change-the-column-collation).

## EF readability refactor

Filtering, vote aggregates, scoring, count and ordered Skip/Take now use `DiscoveryEfQuery` and deferred EF LINQ. Card metadata uses a page-restricted EF projection; ordered text summaries remain isolated SQL; breadcrumbs now use bounded EF hops. The metadata split adds one read, and bounded breadcrumbs add one to three hop reads (six to eight cold reads total) while fully cached reads remain one. See the [walkthrough and blackboard](../blackboards/workflows/Discovery/DiscoveryReadFlow.md) for the code sequence and SQL exceptions. Earlier performance measurements describe the previous raw-SQL implementation and must not be attributed to the EF queries.

## Bounded breadcrumb pagination

- A breadcrumb page contains **three ancestor nodes**, moving from the immediate parent toward a root, not three complete routes.
- `DiscoveryBreadcrumbReader` uses at most three EF hop queries, batched across selected cards. Each hop follows the first stored parent position and records whether alternatives exist. It does not enumerate all ancestor routes or load a fourth node to detect continuation.
- `DiscoveryCard.BreadcrumbSegment` exposes immutable items, a continuation node ID, alternative-route and cycle indicators. Continue above that ID with `ReadPage`, supplying visited IDs to stop cycles across segments. Keep successive reads in a host-managed read transaction when a consistent view is required.
- Display text is nearest-first, ends in an ellipsis when more ancestors exist, and redacts moderated titles. The ancestor page may stop before three items at a root, missing link or cycle.
- Console opened-node displays use the same three-node bound. Explore parents reads three ancestors at a time; N continues above the last one. Multiple immediate parents are offered in sets of three. Inside a segment the stored first parent is provisional; open an intermediate ancestor and explore its parents to select another branch.
- Root-only performance fixtures now use six cold/new-page reads: the previous five plus one batched root-hop check. Deep pages can add up to three breadcrumb queries, for eight cold reads. Fully cached pages still use one revision read; filter changes with reused cards still use three; beyond-last pages use two.
- Breadcrumb recursion has been removed from `DiscoveryCards.sql`. Raw SQL remains for ordered display aggregation and ordered searchable block text, not for ancestor traversal. No schema/migration change is needed.
- A future frontend can use the structured continuation for a scrollable breadcrumb or path feed. Its visual route-switching design remains open; the Console policy is a bounded provisional behavior.

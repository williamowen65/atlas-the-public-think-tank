# SQL-backed Discovery and layered caching (PTT-132)

Discovery runs through `DiscoveryService` → `IDiscoveryReadQuery` → `SqlDiscoveryReadQuery` in Persistence. Console uses this SQL implementation for anonymous, authenticated and authored listings. There is no production in-memory query or candidate-source adapter. The previous behavior assertions now run against isolated SQL fixtures; their input records are test data, not a second ranking implementation.

## Query and display behavior

SQL applies all supported filters, computes vote aggregates and ranking scores, counts matches, orders results, and applies OFFSET/FETCH before materialization. Normal pages contain three ranked IDs/statistics. A separate batched SQL statement returns immutable display cards for only those IDs, including descriptions, author/type labels, ancestor paths, child-type counts, reaction summaries, communities and comment counts. Console renders those cards without per-node repository calls. It loads a domain Node only when the user opens/reports it. Participant profiles obtain authored totals from the SQL listing rather than loading every authored aggregate.

The score remains `(AverageVote ?? 0) + log10(VoteCount + 1) / 100`, plus 0.1 for a title search match. Order is score descending, vote count descending, updated time descending, title ascending under the selected collation, then GUID ascending. Canonical GUID text under `Latin1_General_100_BIN2` preserves .NET Guid ordering rather than SQL Server's native uniqueidentifier ordering. Ranks are global across pages. Offset calculations use long arithmetic, including pages beyond the result set; there is no candidate cap.

Text search uses parameterized CHARINDEX, so `%`, `_` and `[` are literal characters. Searchable block fields and ordered space-separated concatenation preserve searches spanning adjacent fields or blocks. Poll IDs are not searchable. Community/reaction membership uses EXISTS predicates, avoiding join multiplication of votes. Every selected reaction must be active; definition suppression affects card display but does not alter the existing association-filter rule. Average bounds exclude unvoted nodes. Dates compare the timestamp's stored calendar day with SQL date parameters, including the full DateOnly range.

Count, page and cards share a serializable read transaction. Each request reflects one committed database view. Separate page requests can shift when votes/content change; cursor paging would require a product/API decision about ranking changes. Unpaged methods remain for existing contracts/tests, but Console page flows do not call them.

## Unicode and collation

`Latin1_General_100_CI_AI_SC` is explicit on NodeRows.Title and BlockRows Markdown, AltText, Caption, Title and Description. Existing nvarchar storage preserves original Unicode text and spelling; CI ignores case, AI ignores accents and SC supports supplementary characters. No UTF-8 storage conversion is required. SQL is the authoritative text ordering/search implementation; there is no requirement to match a .NET linguistic comparer for every Unicode sequence. The intentional policy change means cafe finds Café.

URL columns retain their previous identity/equality semantics. URL text participating in Discovery concatenation receives the search/display collation at the expression boundary to avoid collation conflicts. Identity/account identifiers and unique community/reaction keys retain their existing rules. This is a Discovery text policy, not a database-wide change or a claim that one collation gives culturally correct ordering for every language.

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

`DiscoveryPerformanceTests` uses 100, 1,000 and 10,000 nodes with persisted ratings. It asserts three materialized ranked results/cards per page and query counts of four on a cold page, one on a warm page, three after a filter change that reuses cards, four on a new page and two beyond the result set. It captures timings and actual SQL execution plans for count, ranking/page and card projection. The SQL CI job uploads `discovery-sql-evidence`; measurements and plan review are recorded in [the performance notes](discovery-performance.md).

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

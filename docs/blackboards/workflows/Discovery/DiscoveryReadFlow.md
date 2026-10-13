# Discovery read walkthrough (PTT-132)

Open [the editable blackboard](DiscoveryReadFlow.excalidraw). Its numbered regions follow the code below. This board complements the older Discovery domain board; it focuses on storage reads, paging and cache freshness.

## Before a request: Console startup wiring

- [Console Program.cs](../../../../src/Atlas.Console/Program.cs) creates one `DiscoveryCache` and passes it to `SqlDiscoveryReadQuery` together with the `SqlStorage.Open` context factory.
- Console wraps that adapter in `DiscoveryService`. The service delegates reads; it does not construct the cache.
- Console owns cache disposal and reuses the instance across requests. Omitting the optional cache disables reuse, not database access.
- A future REST API must supply its own host-lifetime cache and per-read context factory. See [host wiring and cache lifetime](../../../architecture/discovery-read-query.md#host-wiring-and-cache-lifetime).

## 1. One request

1. `DiscoveryService` validates the filters. `SqlDiscoveryReadQuery.Read` trims search text and normalizes set inputs.
2. `Read` opens its own context and serializable transaction. It reads the singleton `DiscoveryRevisionRow` before attempting any cache lookup. That lock remains until the read commits.
3. `DiscoveryCache.PageKey` hashes the filters, selected page, page size, authored scope and hidden visibility policy. The ordering-policy prefix and committed page revision also participate. It is a lookup key, not a paging cursor or authorization token.
4. A page miss calls `DiscoveryEfQuery.Build`. This constructs an `IQueryable`; it does not enumerate candidates. EF translates optional predicates, vote count/average, moderation visibility and scoring into SQL. `Count` gets the total; ordering, `Skip` and `Take` select the page in SQL.
5. Each selected ID is checked in the separate detail cache. Missing cards use two fixed batches: an EF metadata projection and the SQL summary projection. Ordinary labels, dates, status and comment counts are in EF. Breadcrumb ancestry is read in up to three bounded EF hops; ordered display text remains in SQL.
6. After the read commits, the adapter publishes immutable page/card entries and attaches cards to ranked results. Personal participant ratings are queried separately, without shared caching.

## 2. Pagination

- **Page size:** three nodes. The offset is how many matching nodes to skip: `(page - 1) * 3`.

- **Eight matching nodes:**
  - Page 1 skips zero and returns ranks 1, 2 and 3.
  - Page 2 skips three and returns ranks 4, 5 and 6.
  - Page 3 skips six and returns ranks 7 and 8.
  - Page 4 is beyond the matching total and returns an empty page without a ranked-row query.

- **Sort rules:** the next rule only decides the order when all earlier values tie. See [the database ordering](../../../../src/Atlas.Persistence/Discovery/SqlDiscoveryReadQuery.cs).
  1. Higher ranking score first.
  2. More votes first if scores tie.
  3. Most recently updated first if score and vote count tie.
  4. Title in ascending order under the Discovery text collation if the first three values tie.
  5. Canonical GUID text in ascending binary order if titles also tie. This final rule makes equal results deterministic and preserves the earlier .NET GUID ordering policy.

- **Example:** two nodes have equal scores. One has 12 votes and one has 5. The node with 12 votes comes first, regardless of their updated times. Updated time only decides when their vote counts also tie.

- **Where paging happens:** EF sends the filtering, sorting and page selection to the database. The application receives the selected nodes rather than assembling all matching candidates first. The offset uses long arithmetic and is checked against the matching total before conversion to EF's `Skip(int)`.

- **Between requests:** if votes or other data change after page 1, nodes can move before page 2 is requested. Each discovery read has its own consistent transaction; there is no snapshot held across the user's separate page requests.

## 3. Cache invalidation

- **Why invalidate:** cached ordering, statistics, display text or breadcrumb links can become outdated after a saved change. Invalidation means preventing a new request from reusing that outdated result.

- **Example: a new node importance vote.**
  - [DiscoveryInvalidation.Changes](../../../../src/Atlas.Persistence/Discovery/DiscoveryInvalidation.cs) recognizes that ranking/statistics may change.
  - [AtlasDataContext.SaveChanges](../../../../src/Atlas.Persistence/AtlasDataContext.cs) increments PageVersion, for example from 7 to 8, in the same transaction as the vote write.
  - The next discovery read checks the committed revision and constructs a page key containing version 8.
  - A cached page keyed with version 7 does not match that key, so the reader obtains a fresh ranked page.
  - DetailVersion stays unchanged for a node importance vote. Public display cards, including their first breadcrumb segments, can still be reused; the new ranked page carries the updated vote statistics.

- **Changes to public details:** titles, content, parent links, moderation visibility, reactions, comments and related labels advance both revisions. The new DetailVersion also prevents reuse of outdated cards.

- **Rollback:** row writes and revision increments share a transaction. If it rolls back, both are undone; a failed operation does not leave a committed revision increase.

- **Old entries need not be deleted immediately:** [cache keys](../../../../src/Atlas.Persistence/Discovery/DiscoveryCache.cs) contain revisions. Readers use current keys and therefore do not select older entries, even if those entries remain in memory temporarily.

- **Expiry and memory limits:** the 30-second expiry and size budgets limit retention. Revision checks are what prevent stale reuse after a committed change, including moderation changes. Each app instance checks the shared database revision before cache lookup.

## Why some SQL remains

- `DiscoverySearchContent.sql` is a composable SELECT used only during text search. Its ordered cross-block `STRING_AGG ... WITHIN GROUP` preserves phrases spanning adjacent fields/blocks. A simple EF `Any` over individual blocks would change behavior. The current query does not have a suitable ordered aggregate LINQ translation.
- `DiscoveryCards.sql` now handles ordered text summaries only. The former recursive traversal was replaced by bounded EF ancestor hops. Its child-type JSON could move to another EF batch; it remains here to avoid a further round trip, not because child counts inherently require raw SQL.
- Revision updates remain parameterized SQL inside the write transaction so increments are atomic and do not depend on reading a tracked revision first.

This reduces SQL-specific code; it does not promise drop-in portability. Explicit SQL Server collations and the remaining SQL helpers must be reviewed when changing providers. No schema or migration change is required by this refactor.

## Reads and verification

A cold root-only page uses six reads: revision, count, ranked IDs, EF metadata, SQL summaries and one batched ancestor/root check. Deeper cards need up to three hop queries, making eight cold reads at most. A fully cached page uses one revision read. A changed filter reusing all cards uses three. An out-of-range page uses two. Personalized votes add a separate batch.

`DiscoverySqlParityTests`, `DiscoveryCacheTests`, and `DiscoveryPerformanceTests` verify behavior, freshness and read counts. Performance figures recorded before this EF refactor are historical; the new generated queries require fresh CI evidence.

## Breadcrumb segments

- Read at most three ancestors toward the root, nearest first. Cards show the first segment and an ellipsis for continuation.
- Follow the first stored parent by default, with an alternative-route notice instead of expanding every branch.
- Explore parents offers branch choices and ancestor segments in sets of three; N continues above the last ancestor.
- See [bounded breadcrumb pagination](../../../architecture/discovery-read-query.md#bounded-breadcrumb-pagination) and `DiscoveryBreadcrumbReader` for the structured continuation contract.

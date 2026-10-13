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

The page size is three. Offset is `(page - 1) * 3`, calculated with long arithmetic. For eight matches, page two skips three and takes three, returning global ranks four through six. Page three returns ranks seven and eight. An offset at or beyond the count returns an empty page without executing its ranked-row query.

Ordering is score descending, vote count descending, updated time descending, title ascending under the Discovery collation, then canonical GUID text ascending under binary collation. The GUID tie-breaker preserves the earlier .NET ordering policy. Since the total is an int, a long offset below the total can safely be cast for EF's `Skip(int)`.

Each request has a consistent committed view; different page requests can shift after writes. This is offset paging, not cursor paging or a snapshot spanning several requests.

## 3. Cache invalidation

`DiscoveryInvalidation.Changes` classifies tracked EF writes. `AtlasDataContext.SaveChanges` updates the appropriate revision numbers in the same transaction as those writes. Failure or outer rollback undoes both. Node importance votes change ranking/statistics and advance only the page revision. Changes to public details or moderation advance both revisions.

Every read checks the committed revision row, including cache hits and other app instances. The old cache entries need not be actively deleted: the new revision produces different keys. Thirty-second expiration and size budgets bound reuse and retention; they are not the moderation freshness mechanism.

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

# Discovery performance: a guide to the evidence

## Start here

- **Current implementation:** EF handles filtering, vote aggregates, scoring, ordering, totals, paging and ordinary card metadata. SQL helpers handle recursive paths and ordered text summaries.
- **Current expected reads:** cold/new root-only page **6**, repeated cached page **1**, changed filter with reused cards **3**, beyond-last page **2**.
- **Historical timings below:** measured before the EF refactor. They are not measurements of the current EF queries.
- **Current verification:** [refactor CI run 38331824386](https://github.com/williamowen65/atlas-the-public-think-tank/actions/runs/38331824386) passed all 29 Discovery SQL tests and the SQL Persistence, Identity and Console jobs. Use that run's artifact for the refactored query plans and measurements.
- **Code walkthrough:** [DiscoveryReadFlow.md](../blackboards/workflows/Discovery/DiscoveryReadFlow.md).
- **Editable blackboard:** [DiscoveryReadFlow.excalidraw](../blackboards/workflows/Discovery/DiscoveryReadFlow.excalidraw).

## What is a “detail card”?

- A `DiscoveryCard` is the **display data for one node in a listing**, not the full domain `Node` object.
- It contains the node ID, title, type/author labels, status/date, description, breadcrumb, child counts, reactions, communities, comment count and hidden-state indicator.
- It is immutable and contains no participant-specific vote. Personal ratings are queried separately.
- The page cache stores **which nodes appear, their ranks/statistics and the total count**.
- The detail cache stores **what to display for each selected node**.
- Separating these lets a new filter produce a new ranked page while reusing display data for the same nodes.
- Opening or reporting a node still loads the domain object through the appropriate workflow.

## What the fixture contains

The test runs once for each size: 100, 1,000 and 10,000.

| Data | At the 10,000 size |
|---|---:|
| Nodes | 10,000 |
| Description documents | 10,000 |
| Markdown blocks | 10,000 |
| Node importance ratings | 10,000 |
| Participants | 1 |
| Node types | 1 |

- Each node has one document, one block and one vote from the same participant.
- Vote values cycle through 0–10.
- Each block repeats a short Unicode climate-adaptation sentence 16 times.
- This tests increasing stored data volume, **not** 10,000 concurrent users.
- Other behavior fixtures cover communities, multiple block kinds, reactions, ancestry, archive and moderation.

## Walk through the test and its cache reuse

Use A, B and C as the first three ranked node IDs, and D, E and F as the next three.

1. **First request: page 1, no filter**
   - The application caches start empty.
   - Count and rank the results in SQL; return A, B and C.
   - Load and cache three cards: A, B and C.
   - Expected database reads: **6**.
     - [Committed revision — execute `Single` to check cache freshness.](https://github.com/williamowen65/atlas-the-public-think-tank/blob/61c19c383cd8af130ae25e80a509958d0123e5d7/src/Atlas.Persistence/Discovery/SqlDiscoveryReadQuery.cs#L94)
     - [Total count — execute the filtered `Count`.](https://github.com/williamowen65/atlas-the-public-think-tank/blob/61c19c383cd8af130ae25e80a509958d0123e5d7/src/Atlas.Persistence/Discovery/SqlDiscoveryReadQuery.cs#L119)
     - [Ranked page — execute ordered `Skip`/`Take` with `ToList`.](https://github.com/williamowen65/atlas-the-public-think-tank/blob/61c19c383cd8af130ae25e80a509958d0123e5d7/src/Atlas.Persistence/Discovery/SqlDiscoveryReadQuery.cs#L143)
     - [EF card metadata — execute the selected-ID projection with `ToDictionary`.](https://github.com/williamowen65/atlas-the-public-think-tank/blob/61c19c383cd8af130ae25e80a509958d0123e5d7/src/Atlas.Persistence/Discovery/SqlDiscoveryReadQuery.cs#L248)
     - [SQL card summaries — execute `DiscoveryCards.sql` with `ToList`.](https://github.com/williamowen65/atlas-the-public-think-tank/blob/61c19c383cd8af130ae25e80a509958d0123e5d7/src/Atlas.Persistence/Discovery/SqlDiscoveryReadQuery.cs#L254)
     - [Ancestor/root check — execute a batched EF hop; stop immediately for roots.](../../src/Atlas.Persistence/Discovery/DiscoveryBreadcrumbReader.cs)
2. **Repeat page 1 with the same filters**
   - Check the committed revisions, then reuse the cached page and its three cards.
   - These are **three detail-cache hits**, not three newly loaded cards.
   - Expected database reads: **1**.
     - [Committed revision — execute `Single` to check cache freshness.](https://github.com/williamowen65/atlas-the-public-think-tank/blob/61c19c383cd8af130ae25e80a509958d0123e5d7/src/Atlas.Persistence/Discovery/SqlDiscoveryReadQuery.cs#L94)
3. **Change the text filter to `Node`**
   - The filter changes the page key, so count/ranking must be queried again.
   - Every fixture title contains `Node`; the same top three IDs are returned in this fixture.
   - Reuse A, B and C from the detail cache: **three more detail-cache hits**.
   - Expected database reads: **3**.
     - [Committed revision — execute `Single` to check cache freshness.](https://github.com/williamowen65/atlas-the-public-think-tank/blob/61c19c383cd8af130ae25e80a509958d0123e5d7/src/Atlas.Persistence/Discovery/SqlDiscoveryReadQuery.cs#L94)
     - [Total count — execute the filtered `Count`.](https://github.com/williamowen65/atlas-the-public-think-tank/blob/61c19c383cd8af130ae25e80a509958d0123e5d7/src/Atlas.Persistence/Discovery/SqlDiscoveryReadQuery.cs#L119)
     - [Ranked page — execute ordered `Skip`/`Take` with `ToList`.](https://github.com/williamowen65/atlas-the-public-think-tank/blob/61c19c383cd8af130ae25e80a509958d0123e5d7/src/Atlas.Persistence/Discovery/SqlDiscoveryReadQuery.cs#L143)
   - A different filter returning uncached IDs would need card reads too.
4. **Request page 2 without the text filter**
   - Return D, E and F, with global ranks 4, 5 and 6.
   - Load three new cards. Across pages 1 and 2, **six distinct cards have been loaded**.
   - Expected database reads: **6**.
     - [Committed revision — execute `Single` to check cache freshness.](https://github.com/williamowen65/atlas-the-public-think-tank/blob/61c19c383cd8af130ae25e80a509958d0123e5d7/src/Atlas.Persistence/Discovery/SqlDiscoveryReadQuery.cs#L94)
     - [Total count — execute the filtered `Count`.](https://github.com/williamowen65/atlas-the-public-think-tank/blob/61c19c383cd8af130ae25e80a509958d0123e5d7/src/Atlas.Persistence/Discovery/SqlDiscoveryReadQuery.cs#L119)
     - [Ranked page — execute ordered `Skip`/`Take` with `ToList`.](https://github.com/williamowen65/atlas-the-public-think-tank/blob/61c19c383cd8af130ae25e80a509958d0123e5d7/src/Atlas.Persistence/Discovery/SqlDiscoveryReadQuery.cs#L143)
     - [EF card metadata — execute the selected-ID projection with `ToDictionary`.](https://github.com/williamowen65/atlas-the-public-think-tank/blob/61c19c383cd8af130ae25e80a509958d0123e5d7/src/Atlas.Persistence/Discovery/SqlDiscoveryReadQuery.cs#L248)
     - [SQL card summaries — execute `DiscoveryCards.sql` with `ToList`.](https://github.com/williamowen65/atlas-the-public-think-tank/blob/61c19c383cd8af130ae25e80a509958d0123e5d7/src/Atlas.Persistence/Discovery/SqlDiscoveryReadQuery.cs#L254)
     - [Ancestor/root check — execute a batched EF hop; stop immediately for roots.](../../src/Atlas.Persistence/Discovery/DiscoveryBreadcrumbReader.cs)
5. **Request a page beyond the result set**
   - Check revisions and total; return an empty page.
   - Skip ranking and card reads because there are no selected IDs.
   - Expected database reads: **2**.
     - [Committed revision — execute `Single` to check cache freshness.](https://github.com/williamowen65/atlas-the-public-think-tank/blob/61c19c383cd8af130ae25e80a509958d0123e5d7/src/Atlas.Persistence/Discovery/SqlDiscoveryReadQuery.cs#L94)
     - [Total count — execute the filtered `Count`.](https://github.com/williamowen65/atlas-the-public-think-tank/blob/61c19c383cd8af130ae25e80a509958d0123e5d7/src/Atlas.Persistence/Discovery/SqlDiscoveryReadQuery.cs#L119)

**Why the report mentions six reused cards:** it counts six successful detail lookups across steps 2 and 3. Those are two reuses of the same three cards, not six distinct cards already cached before the filter change.

The fixture nodes are roots, so one batched hop query establishes that there are no ancestors. A deep graph needs up to three hop queries; cold reads are bounded at eight rather than always six. Breadcrumbs no longer expand complete routes.

## What each database read does

| Read | Purpose | Example |
|---|---|---|
| Committed revision | Check whether cached results still match committed writes | A moderator hides a node; the next read cannot reuse the old revision |
| Total count | Count all nodes matching the filters | Eight matches, even though only three appear on page 1 |
| Ranked page | Order matches and select the requested slice | Page 2 skips three and returns ranks 4–6 |
| EF card metadata | Load labels, status/date and comment counts for selected IDs | One batch for A, B and C |
| Ancestor/root hop | Read one parent step for selected IDs; stop at roots or after three hops | This root-only fixture needs one hop query |
| SQL card summaries | Load recursive breadcrumbs and ordered display summaries | One batch for the same selected IDs |

- These counts cover Discovery's database reads, not the entire Console operation.
- Reading an authenticated participant's own ratings adds a separate batch.
- Both card queries operate on the selected IDs; there is no repository query per card.

## Are these read counts optimal?

- **No claim of an absolute minimum is made.** The assertions document the current design and detect unexpected extra reads.
- Example: metadata and summaries previously shared one raw-SQL statement. That used **four** cold reads. Splitting ordinary metadata into readable EF adds one read; bounded breadcrumbs now add one to three EF hop reads.
- Count and page could also be combined in a different design; handling an empty/out-of-range page and obtaining its total still needs deliberate treatment.
- A fully cached request uses one revision read because this design checks committed freshness on every request. Zero reads would require a different invalidation/freshness mechanism.
- Fewer reads are not automatically faster: statement complexity, compilation, rows examined and network latency also matter.
- The proven property is **bounded selected-page data returned to the application**, with fixed batch counts for these fixture scenarios.
- SQL may still examine thousands of matching nodes to choose the best three. Pagination does not make database work constant.

## Read the measurement file

| Field | Interpretation |
|---|---|
| Cold page | Stopwatch time for the first request with empty application caches; SQL Server's own caches are not deliberately cleared |
| Warm/cached page | Stopwatch time for a repeated request, including the revision check |
| Filter-change page | Time to recompute the filtered page while eligible cards are reused |
| Reserved database bytes | Reserved table/index space across the model, including fixed Identity overhead; not just text payload size |
| Ranked IDs/cards 3/3 | Three ranked results and three public cards returned to the application |
| Page/detail hits and misses | Cumulative lookup counters across the request sequence, not counts of distinct nodes |

- Query counts and returned-result sizes are **assertions**.
- Timings are **observations**, without a speed threshold.
- One measurement per scenario is not an average, percentile or production capacity forecast.
- “Cold” includes compilation and transient runner conditions; it means empty application caches, not a fully reset SQL Server.

## Historical timing baseline (before the EF refactor)

- [Run 38327316002](https://github.com/williamowen65/atlas-the-public-think-tank/actions/runs/38327316002) passed the 29 Discovery SQL tests and all eleven regular unit-test jobs.
- Its artifact contains fifteen plans: unfiltered count/page/cards and filtered count/page, at each of three sizes.
- Its read counts were **4/1/3/4/2**, because card metadata and summaries were still one query.

| Nodes/documents/blocks/ratings (each) | Reserved database bytes | Cold page (ms) | Cached page (ms) | Changed text filter (ms) | Ranked rows/cards returned |
|---|---:|---:|---:|---:|---:|
| 100 | 4,923,392 | 100.71 | 2.28 | 18.66 | 3 / 3 |
| 1,000 | 7,602,176 | 119.09 | 2.19 | 38.09 | 3 / 3 |
| 10,000 | 27,918,336 | 742.21 | 1.97 | 282.53 | 3 / 3 |

## Historical execution-plan findings: 10,000-node fixture

These details refer to the historical run above, not the EF-generated plans.

- **Unfiltered count**
  - Returned one total after scanning 10,000 node rows and the moderation-visibility index.
  - SQL eliminated vote/content aggregates that the count did not need.
  - Captured execution: 5 ms; reported compilation: 158 ms.
- **Ranking/page**
  - Top/Sort returned three results after evaluating 10,000 matching nodes.
  - The vote index included `Value`, avoiding clustered vote lookups from an earlier baseline.
  - Captured execution: 61 ms; reported compilation: 143 ms.
  - GUID conversion produced a cardinality-estimation warning. Its purpose was to preserve the .NET-compatible tie-breaker.
- **Combined cards**
  - Returned three rows using document/parent/community, reaction and comment indexes.
  - No operator reported more than 1,000 rows read in that captured card plan.
  - Execution rounded to 0 ms; compilation was 218 ms. A displayed zero does not mean literally no work.
  - The JSON input estimate was 50 IDs versus three actual IDs.
  - An excessive-grant warning reported about 2,008 KiB requested, with no recorded grant memory used in this simple root-node fixture.
- **Text-filtered count/page**
  - Examined 10,000 nodes and document/block lookups for concatenated searchable text.
  - Captured execution: 88 ms for count; 169 ms for ranking/page.
  - Only three results reached the application; their cards were reused.
- **Other warnings and memory**
  - No sort-spill warning appeared.
  - Ranking requested about 2,296 KiB and recorded 1,528 KiB used.
  - Conversion/formatting warnings remain evidence to investigate, not proof of an incorrect result or an optimal plan.

## How plans and artifacts are generated

- `DiscoveryPerformanceTests` uses a command interceptor to record executed SQL and parameter values.
- Stopwatch measurements are collected during the request sequence.
- Selected statements are **replayed afterward** with `SET STATISTICS XML ON`.
- SQL Server returns actual execution-plan XML from those replayed executions, saved as `.sqlplan` files.
- Therefore the saved plan is not from the exact execution timed by the stopwatch.
- In the EF version, plans 0–6 correspond to count, ranked page, EF metadata, SQL summaries, the root-hop check, filtered count and filtered page. Check the statement text inside each plan to confirm its identity.
- Open a downloaded `.sqlplan` in SQL Server Management Studio to see its graphical plan. A text editor shows the underlying XML.
- Start by comparing actual versus estimated rows, rows read versus returned, index accesses, sorts and warning properties.
- GitHub Actions uploads `TestResults/discovery-*` as `discovery-sql-evidence`.

## Limits and possible later improvements

- These fixtures establish working reads, cache reuse and bounded application projections; they do not simulate a production workload.
- More text, deeper ancestry, more connections, concurrent readers/writers and network distance may materially change performance.
- Full-text search, reusable statistics, narrower invalidation, cursor paging and query/compilation tuning are future options to measure.
- Earlier fixtures without descriptions were faster, illustrating why workload details belong alongside every timing comparison.

## Reproduce

- Set `ATLAS_SQL_TEST_CONNECTION_STRING` to a SQL Server test connection.
- Run from the solution root:

```powershell
dotnet test tests/Atlas.Persistence.Tests/Atlas.Persistence.Tests.csproj --filter FullyQualifiedName~DiscoveryPerformanceTests
```

- Tests create and delete uniquely named databases from the current model; they do not generate migration files.
- Output appears under `tests/Atlas.Persistence.Tests/TestResults/discovery-*`.
- Do not commit machine-specific test output directories.
- The ordinary migrated integration jobs separately verify the developer-generated migration path.

# Discovery SQL performance evidence

The [SQL Discovery CI run 38327316002](https://github.com/williamowen65/atlas-the-public-think-tank/actions/runs/38327316002) passed all 29 Discovery SQL behavior/cache/performance tests against SQL Server 2022. All eleven regular unit-test jobs also passed. Its `discovery-sql-evidence` artifact contains measurements and fifteen actual execution plans: unfiltered count/page/cards and text-filtered count/page at three dataset sizes.

Each fixture node has one document, one Unicode Markdown block (16 repetitions of a short climate-adaptation sentence), an author/type, and one persisted importance rating cycling through 0–10. Separate behavior fixtures cover multiple content kinds, communities, reactions, ancestors, archive and moderation. These are synthetic baselines, not a production capacity forecast or a comparison against the previous application.

| Nodes/documents/blocks/ratings (each) | Reserved database bytes | Cold page (ms) | Cached page (ms) | Changed text filter (ms) | Ranked rows/cards returned |
|---|---:|---:|---:|---:|---:|
| 100 | 4,923,392 | 100.71 | 2.28 | 18.66 | 3 / 3 |
| 1,000 | 7,602,176 | 119.09 | 2.19 | 38.09 | 3 / 3 |
| 10,000 | 27,918,336 | 742.21 | 1.97 | 282.53 | 3 / 3 |

Reserved bytes come from sys.dm_db_partition_stats and include all model tables/indexes, including fixed Identity/model overhead; this is not merely content-text size.

These single cold/warm measurements include application/database round trips and, on cold calls, SQL compilation; do not treat them as stable percentiles. Filters and data distribution affect SQL work. Cache-hit totals/statistics remain protected by a committed-generation read, so a warm hit still makes one SQL call.

All three sizes assert query counts of 4 for a cold page, 1 for the repeated page, 3 for a changed filter reusing eligible details, 4 for the next page, and 2 for a page beyond the result set. The changed filter and repeated page reused six detail cards total; the first/next pages loaded six cards total. Selected-page materialization stays at three ranked references and three immutable cards rather than all candidate/domain graphs.

## Actual-plan review (10,000-node description fixture)

- Unfiltered count: one count is returned after a NodeRows scan of 10,000 rows and a moderation-visibility index scan. SQL eliminates vote/content aggregates when the count predicates do not depend on them. The captured execution took 5 ms; reported compilation took 158 ms.
- Ranking/page: Top/Sort returns three rows after evaluating statistics for 10,000 matching nodes. The target-first vote index includes Value, removing the clustered vote lookups seen in the earlier baseline. Captured execution took 61 ms; reported compilation took 143 ms. GUID conversion still produces a cardinality-estimation warning; this preserves the .NET-compatible tie-breaker. A persisted canonical sort key is a later, measured schema option.
- Cards: the projection returns three rows through page-linked joins; no operator reports more than 1,000 rows read in this card plan. It uses the document/parent/community indexes, typed OPENJSON, active-reaction index and comment-target index. Card execution rounded to 0 ms in the captured plan; compilation took 218 ms. OPENJSON's estimate of 50 versus three actual IDs and wide string formatting contribute to an excessive-grant warning: roughly 2,008 KiB requested while this simple root-node fixture used no recorded grant memory.
- Text-filtered count/page: SQL reads the 10,000 node rows and performs indexed document-link/block lookups for searchable concatenations. Captured executions took 88 ms for count and 169 ms for page/ranking. The application still materializes only three results and reuses the already cached cards. Free-text filtering costs database work even when matching the title.
- No sort-spill warning appears. Ranking requests roughly 2,296 KiB (1,528 KiB recorded use). Formatting/canonical-ID conversions retain cardinality-estimation warnings; they are recorded rather than treated as proof that every plan is optimal.

Cold timings include first-use compilation and transient runner/database conditions. They explain why one 10,000-node cold page is substantially slower than a warm page; these measurements are not stable percentiles. The covering vote index, comment-count index and typed ID projection were added after inspecting an earlier successful SQL baseline. Earlier lighter fixtures without descriptions measured 76.94/89.46/153.96 ms cold and 1.81/1.92/1.60 ms cached at 100/1,000/10,000 nodes, illustrating why description workload and compilation context must be recorded.

These plans establish bounded application materialization and working index paths, not constant database cost. A larger, more connected or more text-heavy graph can differ markedly, especially for substring search and ancestor paths. Full-text search, reusable statistics, fine-grained generations, cursor paging and further card-query/compilation tuning are possible later optimizations. The current implementation remains one SQL-authoritative query with bounded, correctly invalidated read caches.

## Reproduce

Set `ATLAS_SQL_TEST_CONNECTION_STRING` to a SQL Server test connection and run from the solution root:

```powershell
dotnet test tests/Atlas.Persistence.Tests/Atlas.Persistence.Tests.csproj --filter FullyQualifiedName~DiscoveryPerformanceTests
```

Tests create and delete uniquely named databases from the current model. They do not generate migration files. Measurements and actual plans appear under `tests/Atlas.Persistence.Tests/TestResults/discovery-*` and are uploaded by the SQL Discovery CI job. Do not commit machine-specific test output directories. Normal migration/upgrade verification remains a separate check after the developer generates and commits the required migration.

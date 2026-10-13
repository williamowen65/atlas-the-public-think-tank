# Discovery SQL performance evidence

The [SQL Discovery CI run 38326519135](https://github.com/williamowen65/atlas-the-public-think-tank/actions/runs/38326519135) passed all 27 Discovery SQL behavior/cache/performance tests against SQL Server 2022. Its `discovery-sql-evidence` artifact contains measurement text and nine actual execution plans: count, page/ranking and batched cards at three dataset sizes. Fixtures persist one importance rating per node, cycling through 0–10. These are small synthetic baselines, not a production capacity forecast or a comparison against the previous application.

| Nodes/ratings | Cold page (ms) | Cached page (ms) | Ranked rows/cards returned |
|---|---:|---:|---:|
| 100 | 76.94 | 1.81 | 3 / 3 |
| 1,000 | 89.46 | 1.92 | 3 / 3 |
| 10,000 | 153.96 | 1.60 | 3 / 3 |

These single cold/warm measurements include application/database round trips and, on cold calls, SQL compilation; do not treat them as stable percentiles. Filters and data distribution affect SQL work. Cache-hit totals/statistics remain protected by a committed-generation read, so a warm hit still makes one SQL call.

All three sizes assert query counts of 4 for a cold page, 1 for the repeated page, 3 for a changed filter reusing eligible details, 4 for the next page, and 2 for a page beyond the result set. The changed filter and repeated page reused six detail cards total; the first/next pages loaded six cards total. Selected-page materialization stays at three ranked references and three immutable cards rather than all candidate/domain graphs.

## Actual-plan review (10,000-node fixture)

- Count: the final stream aggregate returns one count; the NodeRows clustered scan examines 10,000 rows and moderation visibility uses the composite visibility index. Vote aggregates are eliminated when the count's predicates do not depend on them.
- Ranking/page: the top/sort returns three rows. SQL evaluates statistics for 10,000 matching nodes; the target-first vote index is used. The initial plan also performs clustered lookups for the vote Value, so the final model adds Value as an included column. GUID conversion produces a cardinality-estimation warning; this conversion preserves the chosen .NET-compatible tie-breaker. A persisted canonical sort key would be a later, measured schema decision.
- Cards: the projection returns three rows, with nested-loop joins to page-linked data. It uses existing document/parent/community indexes and the new active-reaction index. The OPENJSON default estimate is 50 versus three actual IDs; the final query uses a typed uniqueidentifier OPENJSON projection to remove the explicit ID conversion. The initial empty-comment fixture scans CommentRows; the final model adds a target-kind/target-ID index for populated comment workloads.
- No sort-spill warning appears in these plans. Ranking requests a roughly 2,296 KiB memory grant and the initial card plan roughly 2,000 KiB. Card compilation in this sample takes about 65 ms, a substantial portion of first-request cost.

The final index/projection adjustments are based on this review. Repeat the same performance test to collect fresh plans after generating the developer migration; current-model CI tests also exercise those model changes. A text-heavy or highly connected graph can have very different costs, especially ordered substring searches and ancestor paths. Full-text search, summary read models, fine-grained generations and cursor paging are possible later optimizations; none is needed to prove that this implementation pages in SQL and avoids complete application-memory candidates.

## Reproduce

Set `ATLAS_SQL_TEST_CONNECTION_STRING` to a SQL Server test connection and run from the solution root:

```powershell
dotnet test tests/Atlas.Persistence.Tests/Atlas.Persistence.Tests.csproj --filter FullyQualifiedName~DiscoveryPerformanceTests
```

Tests create and delete uniquely named databases from the current model. They do not generate migration files. Measurements and actual plans appear under `tests/Atlas.Persistence.Tests/TestResults/discovery-*` and are uploaded by the SQL Discovery CI job. Do not commit machine-specific test output directories. Normal migration/upgrade verification remains a separate check after the developer generates and commits the required migration.

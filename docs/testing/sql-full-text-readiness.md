# SQL full-text test readiness

Two user-reported coverage-run failures occurred waiting for full-text population:
IdenticalNodeAndCommentDocumentsHaveEqualRanksAndSharePagination and
Pages_materialize_only_three_cards_with_constant_query_count (10,000 rows).

Both require real full-text search: the former compares FREETEXTTABLE ranks and
mixed pagination; the latter includes a text-filter query as well as non-text
paging. Removing indexes or bypassing readiness would invalidate those checks.

## Current change

- Vote-action quota, report-submission quota and comment-paging tests explicitly
  use installFullText:false with the current EF model. These eight fixture
  invocations test quotas/paging rather than full-text behavior.
- The two reported methods are labeled FullTextSearch. This is a focused label,
  not yet an exhaustive inventory of every full-text-dependent test.
- Shared test index-readiness budget increases from 45 seconds to 180 seconds.
  Successful population returns immediately; no fixed extra delay is added.
- On timeout, report each relevant index's enabled, crawl-completed,
  population-status and pending-change state, plus the isolated database name.
  Connection credentials and document text are not logged.

The wait budget is setup readiness, not an application latency target. Production
search and its timeouts are unchanged. SQL tests still fail if readiness never
completes, and original ranking/paging/query-count assertions remain unchanged.

## Boundaries

SqlTestDatabase currently defaults to installing supplemental search schema.
The opt-out applies only to the audited non-search current-model fixtures above.
Migration-mode setup still applies full Atlas database setup, including search;
installFullText:false is not a general migration-mode switch.

Further reducing indexing overhead requires auditing dependencies: supplemental
setup also creates search projections, tracking triggers and snapshot isolation.
Do not remove it from a test merely because its name does not mention search.
This change does not claim all full-text overhead or failures are fixed.

## Verify before another complete coverage run

From repository root, run the affected tests and non-search opt-outs:

```powershell
dotnet test tests/Atlas.Persistence.Tests/Atlas.Persistence.Tests.csproj --configuration Release --filter "FullyQualifiedName~IdenticalNodeAndCommentDocumentsHaveEqualRanksAndSharePagination|FullyQualifiedName~Pages_materialize_only_three_cards_with_constant_query_count|FullyQualifiedName~SqlVoteActionQuotaTests|FullyQualifiedName~SqlReportSubmissionQuotaTests|FullyQualifiedName~SqlCommentPagingTests"
```

If that passes, repeat with
`--collect:"XPlat Code Coverage" --settings tests/backend-coverage.runsettings`
to compare under instrumentation. Preserve timeout diagnostics if readiness
still fails; a larger bound does not establish why the original indexer was slow.

Editing environment lacks dotnet/SQL execution. Windows and CI validation pending.
Full coverage CI remains manual-only.

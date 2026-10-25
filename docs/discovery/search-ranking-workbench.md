# Search ranking workbench

Open Storybook → **Compositions / Search ranking / Workbench** to compare real SQL search ranks on a controlled corpus. The workbench uses the same version-controlled fixture JSON and C# runner as `DiscoveryRankingScenarioTests`; it does not implement ranking in JavaScript. No migration or additional SQL scripts are required.

## Start

From `src/hosts/Atlas.Web`:

```powershell
npm run storybook:ranking
```

This starts Storybook on port 6006 and the standalone loopback Ranking Lab on port 7251. Configure `ATLAS_SQL_TEST_CONNECTION_STRING` in the existing shared .NET user-secrets file or environment. The account must be able to create and delete disposable databases, enable snapshot isolation and install SQL Server full-text indexes with English resources. The app's normal `ATLAS_SQL_CONNECTION_STRING` is not used. You do not need to start Atlas.Api for this story.

1. Select a scenario and click **Run scenario**. The first run can take several seconds for database setup and initial full-text population.
2. Expand entries to change title, short description, Markdown blocks, comment body, individual vote values or the ranking date. A comment can copy a node's complete indexed text to guarantee identical raw relevance.
3. Run again. Changed inputs retain saved expectations as comparisons; a red FAIL can reveal an intended tradeoff or an input change, not necessarily a code regression.
4. Inspect raw relevance, votes, average/net score, effective support, multiplier, final score, actual searchable text, IDs and date tie-breaks. All pages are collected, with the page number shown per result.
5. Select a result phase to compare baseline, upvote, change and undo. Phase vote overrides carry forward. Text stays fixed across phases; no reindexing is performed between vote changes.
6. **Reset scenario** restores the saved definition and clears old results. Exportable diagnostics contain the actual submitted inputs and returned evidence, even if inputs have since changed.

The story needs the development proxy for live runs. A static Storybook build renders the workbench and scenario controls but cannot run SQL without equivalent local routing. Browser transport stubs test the UI only; real ranking assertions run in SQL integration tests.

## Scenarios and assertions

`tests/Fixtures/SearchRanking/scenarios.json` is the single source for Storybook and backend tests:

- Equal text: a node rated 10 once precedes +1 and unvoted comments.
- Equal text/counts: higher node average wins.
- Equal text/average: more ratings increase confidence, with diminishing returns.
- Comment baseline → upvote → downvote → undo: expected order changes and exact baseline scores/order return.
- Strong match across query terms versus a weaker match with many high ratings.
- `test` / `testing`: the observed short-comment/longer-node example, explicitly exploratory without an assumed cross-kind order.
- Equal text/votes: type/date tie-breaks and unique results across a real page boundary.

Expected order is authored in the fixtures, not computed from today's ranking formula. Identical-text fixtures also assert equal raw relevance across result types. Every case asserts complete, unique results. Adding or adjusting an expected order changes an explicit ranking contract; inspect and discuss changes rather than replacing expected values with whatever the implementation happens to return.

`SqlPublicSearch.Explain` executes the same read, SQL expression, sorting and pagination as `Search`. SQL returns the final score used for ordering; the multiplier is derived from that score and raw relevance. Diagnostics do not change the normal public search response. The ranking algorithm is unchanged by this workbench.

## Isolation and reproducibility

Every run creates a generated `AtlasRankingLab_<GUID>` database, uses EF's current model and the existing supplemental setup, seeds only the submitted corpus, waits for its initial indexing, and deletes it in `finally`. The browser cannot choose the database name. The shared seeder refuses populated databases. One run at a time is allowed. The host accepts loopback traffic and allowlisted Storybook origins only; the development bridge forwards neither session cookies nor authorization headers.

Normal errors trigger cleanup. Abrupt process termination or lost database connectivity can leave a disposable database; its generated name is logged if cleanup fails. Only databases with the tool's generated prefix are candidates for manual cleanup. The main Atlas demo corpus is never reset or reseeded by this tool.

The corpus includes the submitted entries plus one fixed “Discussion container” node that hosts comments. Avoid that control text as a custom query; a matching container is reported as an error. Fixed timestamps, stable result IDs, the corpus, query and individual votes are reproducible. SQL Server full-text rank depends on corpus statistics and English linguistic resources; absolute raw scores can vary with SQL Server versions/index state and differ from a larger demo database. Assertions target meaningful ordering relationships, not hard-coded SQL rank numbers. Initial indexing has a deadline; an indexing failure is reported rather than returning a misleading PASS.

## Tests

From the solution root:

```powershell
dotnet test tests/Atlas.Persistence.Tests/Atlas.Persistence.Tests.csproj --filter FullyQualifiedName~DiscoveryRankingScenarioTests
```

Without SQL credentials, fixture validation runs and SQL cases report inconclusive. SQL CI supplies credentials and runs all scenarios in disposable databases. The same tests also run in the full Persistence job.

From `src/hosts/Atlas.Web`:

```powershell
npm run check
npx playwright test --config playwright.ranking.config.ts
```

CI builds the isolated host and Storybook, checks Svelte diagnostics, runs the workbench browser checks, and exercises the shared ranking fixtures in the SQL jobs.

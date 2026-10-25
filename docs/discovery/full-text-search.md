# SQL Server full-text discovery search

## How text and filters work together

You can put search words **before, between, or after filters**.

```text
salmon type:issue habitat votes:2.. protection
```

Atlas separates that input into:

| Part | Value sent to the API |
| --- | --- |
| Text search | `salmon habitat protection` |
| Node type | The ID for `Issue` |
| Minimum votes | `2` |

- The parser reads the whole input, from left to right.
- Recognized filters are pulled out and sent as separate API parameters.
- Remaining words are joined with spaces, preserving their order.
- SQL Server searches that combined text and applies the filters together.
- Direct API clients must also send plain `SearchText` and separate filter parameters.

**Which tokens count as filters?**

- Supported keys: `type`, `votes`, `average`, `author`, `community`, `from`, `through`, `archived`.
- Filter names are case insensitive; node-type names are case insensitive too.
- Use quotes for a type name containing spaces: `type:"Research Question"`.
- A recognized filter with an invalid value produces a validation error and blocks submission.
- An unrecognized token such as `topic:salmon` remains text. A bare token such as `type:` also remains text under the current parser.
- If the same filter is repeated, the last valid value wins; repeated filters do not create multiple conditions.

## What full-text matching means

- **Word order is flexible:** words do not have to appear as one exact phrase.
- **English word forms match:** for example, `habitat` can match `habitats`.
- **Any-term matching:** a result can match one meaningful search word; every word is not required.
- **Punctuation is word-broken:** this replaces literal substring/wildcard matching.

Current limits:

- No edit-distance typo correction.
- No guaranteed synonyms without a configured thesaurus.
- Quoted free text does not request an exact phrase.
- Global search orders the combined node/comment hits by SQL Server full-text relevance adjusted by bounded comment support (see comment-support-v1 below), then result kind, date and ID for deterministic ties. Node importance and comment support both supply bounded ranking adjustments (support-v2 below). The browsing feed keeps its existing importance-based ordering.
- Global search uses **one full-text index on `SearchDocuments.SearchText`** for both nodes and comments. Scores therefore come from the same corpus instead of comparing unrelated per-table ranks.
- One node document combines its title, short description and all currently linked block text. One comment document contains its body. A match across multiple node fields contributes to that single document.
- Results may cluster by type when their text earns similar scores. There is no alternating quota or automatic preference for comments; identical documents earn identical scores, with nodes first only when scores tie.

## What gets searched

| Location | Indexed text |
| --- | --- |
| Node | Title and short description |
| Markdown block | Markdown text |
| Image block | Alt text and caption |
| Video block | Caption and URL |
| Link preview | Title, description and URL |
| Chart reference | Title |
| Comment | Visible comment body |

- Poll/reference IDs are excluded.
- `DocumentBlocks` links restrict matches to the node's current document. Removing a link updates the node's search document in the same transaction; full-text matching catches up asynchronously.
- Moderation, archive policy, other filters, counting and paging remain server-side constraints.
- Text is sent as an SQL parameter, never executable SQL syntax.
- Search text is limited to **4,000 characters** after trimming surrounding whitespace.

### Index settings and timing

- English language: LCID `1033`.
- Accent-insensitive catalog matching.
- SQL Server's system stoplist.
- Automatic change tracking.

**Indexing happens in the background.** New or edited text may take a short time to appear.
Count and page reads can also see indexing progress between statements.

- Text-search pages bypass the result-page cache so early empty results do not stay cached.
- Card details and browsing pages keep their database revision checks.
- Live moderation checks still apply before results are returned.

## Local SQL Server prerequisite

1. Use SQL Server Developer or another edition supporting Full-Text Search. LocalDB does not provide this feature.
2. On Windows, open SQL Server Setup and choose **Add features to an existing instance**.
3. Select the instance Atlas actually uses.
4. Install **Full-Text and Semantic Extractions for Search**.
5. Follow Setup's restart requirements.

You do not need a semantic language statistics database for `FREETEXT`. A container alternative is shown below.

Check the instance in SSMS:

```sql
SELECT FULLTEXTSERVICEPROPERTY('IsFullTextInstalled') AS IsFullTextInstalled;
SELECT name, lcid FROM sys.fulltext_languages WHERE lcid = 1033;
```

Expected results:

- `IsFullTextInstalled` is **1**.
- The language query returns **English**.

Separate test and deployment instances need the same feature. CI installs it automatically.

## Database setup and optional future migration squash

The developer-generated baseline is already committed. **For normal updates, skip to step 2** and rerun explicit setup; do not delete your database or regenerate migrations. Step 1 documents the optional disposable-database squash workflow.

**This reset is for your disposable local database.** Do not use these reset steps on a production/shared database with data to preserve.

### 1. Generate the new baseline

Pull the latest PR branch, then run from the solution root:

```powershell
dotnet tool restore
dotnet restore src/Atlas.Persistence/Atlas.Persistence.csproj
dotnet ef migrations add InitialAtlasBaseline --project src/Atlas.Persistence
dotnet ef migrations has-pending-model-changes --project src/Atlas.Persistence
```

- You will delete/recreate your local database for this test; an existing database has the old migration history.
- The new migration should create the entire EF schema, including `BlockRows.SearchText`.
- No full-text SQL needs to be copied into that migration. The explicit setup command owns that step.
- Review and commit the generated migration, designer and snapshot. CI's model check remains enabled and will fail until the baseline is included.

### 2. Create/update the database and install supplemental objects

Configure the shared `ATLAS_SQL_CONNECTION_STRING`, then run:

```powershell
dotnet run --project src/hosts/Atlas.Console -- --setup-database
```

The command:

1. Applies EF migrations, creating the database when necessary.
2. Installs and backfills the search projection, then creates full-text indexes outside a transaction.
3. Checks that full-text support, English resources and all four enabled indexes are available.
4. Prints success and exits, without opening the interactive console or seeding demo data.

- Normal API startup does not run this installer.
- Repeating setup leaves existing data intact and does not duplicate indexes.
- Setup fails with a nonzero exit code if migrations, permissions or full-text support are missing.
- Index population continues in the background after setup succeeds.
- Deployment automation should run this explicit command before serving application traffic, using a database account with schema-change permissions.

### 3. Seed and check search

```powershell
dotnet run --project src/hosts/Atlas.Console -- --seed-demo
dotnet test tests/Atlas.Persistence.Tests/Atlas.Persistence.Tests.csproj --filter FullyQualifiedName~Discovery
```

Demo seeding requires an empty database.

**Seeding can finish before the seeded content is searchable.**

- Saving rows and updating the full-text indexes are separate operations.
- A fresh database followed by bulk seeding can leave indexing work queued in the background.
- During that interval, text search may show no results or only some of the expected results. A successfully saved node can still open directly by ID.
- Allow indexing to catch up, then submit the search again. The application does not automatically refresh an already displayed empty result.
- Do not delete/reseed the database again solely because the first text search is empty.
- There is no fixed indexing delay. If results remain missing, check index population/pending changes and SQL Server diagnostics rather than assuming indexing is still progressing.

### Why SQL search tests wait too

- Discovery SQL fixtures explicitly wait for the full-text indexes to finish their tracked work before asserting search results.
- The wait polls indexing status and pending changes, with a 45-second timeout that fails the test if indexing does not finish.
- Search fixtures generally seed small, isolated sets of records rather than the full demo dataset. Smaller batches can finish sooner, but the wait still matters; tests must not assume immediate visibility.
- The 45-second timeout is a test deadline, not a promised maximum delay for the application.
- Setup and normal application requests do not perform that test-only wait.

For SQL tests:

- Set `ATLAS_SQL_TEST_CONNECTION_STRING` in shared user secrets or the environment.
- Use a full-text-capable test instance, not production.
- The test account needs permission to create/drop disposable databases and their full-text indexes.

### Why migration squashing preserves this setup

`AtlasDatabaseSetup`, `DiscoveryFullTextSchema` and the SQL resources live outside `Migrations`.
A new EF baseline supplies the tables/computed column; explicit setup supplies the catalog/indexes afterward.

Tests cover:

- Creating the EF schema without historical migration SQL, then installing working text search.
- Repeating setup without losing seeded content or creating duplicate indexes.
- Running the same setup service with the current migration chain.
- Rejecting an explicit transaction around the full-text installer.

The fresh-model test checks the supplemental setup independently of migration history. Your generated baseline and migration-based tests complete the actual squash verification.

## CI and reproducible test instance

### CI setup

All SQL jobs in `.github/workflows/AtlasRewriteTests.yml` use `tests/infrastructure/sqlserver/Dockerfile`.

- The image extends Microsoft's SQL Server 2022 image and installs matching engine/full-text packages.
- SQL Server runs as the `mssql` user; BuildKit caches the image layers.
- Before tests start, CI requires a working SQL connection, `IsFullTextInstalled = 1`, and English resources.
- Missing support fails the job before tests run.
- Failure output includes container logs; the container is removed afterward.

### Optional local test container

For a local disposable container with the same feature set (choose your own password):

```powershell
docker build -t atlas-sql-fts tests/infrastructure/sqlserver
docker run -d --name atlas-sql-fts -p 1433:1433 -e ACCEPT_EULA=Y -e MSSQL_PID=Developer -e 'MSSQL_SA_PASSWORD=Your_Strong_Test_Password1!' atlas-sql-fts
```

- Point the test connection at this instance.
- If port `1433` is already used, map another host port and use it in the connection string.
- The image follows the repository's existing `2022-latest` policy and installs matching engine/full-text versions.

### How SQL tests handle indexing

- **Fresh-model tests:** use `EnsureCreated`, then invoke the production supplemental installer.
- **Migration tests:** run the production setup sequence: EF migrations, then the explicit supplemental installer.
- **Discovery fixtures:** wait for indexing with a 45-second deadline, failing if it is stuck.
- **Application requests:** do not wait for indexing.

Coverage includes reordered words, inflections, any-term matching, block edits, type parsing, moderation/archives, combined filters and uncached text pages. Punctuation expectations use token matching rather than substring matching.

## Unavailable or disabled full-text search

When a text-search dependency fails:

1. The backend creates `SearchUnavailableException` and logs the original SQL error.
2. The API returns **HTTP 503**, with code `search_unavailable`.
3. The dropdown shows the following message and offers **Retry**:

> Text search is temporarily unavailable. Please try again later.

- SQL diagnostics are not sent to visitors.
- A failure does not appear as "No results found."
- Paging failures keep already loaded results.
- Filters without free text do not invoke the full-text engine.

If search is unavailable locally, check:

- **Feature installed:** Full-Text Search is installed for the correct SQL Server instance.
- **Service running:** that instance's **SQL Full-text Filter Daemon Launcher** service is running.
- **Indexes ready:** the explicit setup command created the indexes and they are enabled in Atlas's database.
- **Database reachable:** a connection/database failure can also cause this error.

The application reports failure when a search is attempted; it does not continuously monitor the service.

## Sources

- [Microsoft: FREETEXT](https://learn.microsoft.com/en-us/sql/t-sql/queries/freetext-transact-sql)
- [Microsoft: CREATE FULLTEXT INDEX](https://learn.microsoft.com/en-us/sql/t-sql/statements/create-fulltext-index-transact-sql)
- [Microsoft: Install Full-Text Search on Linux](https://learn.microsoft.com/en-us/sql/linux/install-upgrade/setup-full-text-search)
- [Microsoft: EF Core SQL Server full-text support and older-version migration SQL](https://learn.microsoft.com/en-us/ef/core/providers/sql-server/full-text-search)

## Comment search and combined pagination

After pulling this update, rerun the explicit setup command to install the comment table's full-text index:

```powershell
dotnet run --project src/hosts/Atlas.Console -- --setup-database
```

No EF model change or new migration is required. Existing comment text is populated in the background, so allow indexing to catch up.

- The header uses `/api/search`; `/api/nodes` remains a node-only discovery endpoint.
- `in:all` (default) mixes nodes and comments. Use `in:nodes` or `in:comments`, or the Search in control.
- Text-free searches keep the established node-only behavior unless `in:comments` is explicitly selected.
- Type, community, parent, archive, reaction and node-importance filters concern the containing node.
- Author and creation-date filters concern the node author/date for node results, or the comment author/date for comments.
- Removed comments and comments on hidden/unavailable nodes are excluded. Archived nodes and their comments follow the existing archive filter.
- Comment entries show **Comment on [node title]**, the **comment author**, the comment creation date and an excerpt centered on the first literal search term when available. Linguistic-only matches fall back to the opening excerpt; excerpts are escaped plain text.

### One result sequence

Node and comment candidates are combined, ranked and counted **before** SQL pagination. The existing three-result page size applies to that combined sequence. No independent comment page is appended to a node page. Each node has one combined search document; each comment remains its own result.

Search results are not cached. Index population and concurrent edits can change ordering between requests; the client deduplicates appended entries, and rerunning the search starts from current data. Relevance does not guarantee a fixed ratio of nodes to comments per page.

### Opening a comment

A comment result links to `/nodes?id=NODE&type=comments&comment=COMMENT`.

- The backend checks current node visibility and resolves at most 128 ancestors, without loading the entire discussion.
- Removed ancestors remain blank placeholders; a removed target itself is unavailable.
- The UI opens Comments, pins the relevant sibling at each level even if it is outside that level's first page, expands the ancestor chain, then highlights and focuses the selected comment.
- Normal sibling pagination and replying remain available. A stale or unavailable link shows an error with Retry comment and Show discussion controls.

## Shared relevance ranking: updating an existing database

After pulling the shared-ranking update, rerun:

```sh
dotnet run --project src/hosts/Atlas.Console -- --setup-database
```

- No database deletion or reseeding is needed.
- Setup creates the rebuildable `SearchDocuments` projection, refresh procedures and tracking triggers, backfills existing nodes/comments, and creates its full-text index.
- These supplemental objects live outside EF migrations, so squashing migrations does not remove their installation logic. No new EF migration is required for this projection.
- Existing node/block indexes remain for discovery feed queries; the comment index is retained for compatibility. Global search uses the shared index exclusively.
- Setup requires permission to create tables, types, procedures, triggers and full-text indexes. Run it before starting the updated API or seed/import jobs, with application writes stopped during deployment.
- On repeat runs, setup repairs projection text and removes stale documents without replacing unchanged document keys.
- Database triggers update the projection in the same transaction as node, block, document-link and comment writes, including raw SQL imports. Removed/deleted comments lose their documents.
- EF uses its trigger-compatible SQL write path for those four source tables. The projection is derived infrastructure, not an additional domain entity.
- Full-text indexing still happens **in the background**. A first backfill or bulk seed may take longer than normal edits; allow the indexer to finish before comparing results.
- Missing or disabled shared search infrastructure produces the existing search-unavailable response, rather than silently falling back to incompatible ranks.

### Ranking regression checks

- Identical node and comment documents must receive identical scores from the shared full-text query.
- Combined counting and pagination must retain every distinct node/comment result.
- Tests cover projection backfill, repeated setup, and subsequent node, block, link, comment and raw SQL edits.
- SQL fixtures wait for all four indexes before asserting search results. No browser/UI change is required for the new ordering.


## Comment support votes

Comments support one current +1 or -1 vote per participant, including on the participant's own comment. Repeating PUT changes the existing vote; DELETE removes it. Comment votes use `VoteRows` with `TargetType = "Comment"`, the existing unique participant/target index, participant lock, eligibility checks and shared action quota. No EF model change, migration, index rebuild or database setup is required for this addition.

- `GET /api/comments/{id}/votes` returns `score` (upvotes minus downvotes), `totalVotes`, and `currentParticipantVote` (null, +1 or -1). Anonymous reads omit personal state. Removed comments and hidden/missing targets return 404.
- `PUT /api/comments/{id}/vote` accepts `{ "value": 1 }` or `{ "value": -1 }`; `DELETE` on the same path undoes the current vote. Both require a verified member and antiforgery protection. Removed comments and archived/hidden/unavailable nodes cannot receive cast/change/undo actions.
- Discussion controls follow View replies and Reply. Selecting the current direction undoes it; selecting the other changes it. Totals always come from the server. Failed/uncertain writes require refreshing totals before another attempt. Signed-out visitors see a dismissible sign-in prompt; removed placeholders have no voting controls.
- Search displays the net score in the same left column as node importance. Votes are read directly from current SQL rows, so changes affect search without waiting for full-text indexing.

Original ranking policy (comment-support-v1, superseded by support-v2 below): let `r` be the shared full-text relevance and `s` the comment net score. Final relevance is `r * (1 + 0.25 * s / (10 + abs(s)))`. Nodes use `s = 0`. A comment score of +10 boosts its relevance by 12.5%; -10 reduces it by 12.5%. The adjustment stays below 25% in either direction, preserving relevance as the primary signal. Identical text retains identical raw full-text ranks; vote support can change the final order. Zero-vote ties keep the existing kind/date/ID order. Comment-only browsing without text uses a base relevance of 1 and therefore sorts by net support, then the same deterministic ties. Node-only browsing/importance filters retain their existing behavior.

SQL tests cover a positive comment above an equally relevant node, a negative comment below it, authoritative score projection, and reverting to neutral ordering after undo. Additional tests cover vote persistence/change/undo and removed/archived target rejection. Run the Voting tests, DiscoveryMixedSearchTests, SqlThinHostWorkflowTests, API SqlWorkflowTests, and frontend comments/global-search browser suites.


## Concurrent search and voting; support-v2

Search, feed discovery and comment-path reads use EF transactions with `IsolationLevel.Snapshot` instead of serializable read locks. Each request reads committed relational data as of its snapshot; ordinary votes, edits and moderation writes can proceed alongside it. A request already in progress may finish with the previous committed state. Later requests see newer committed data/revisions. Full-text population is still asynchronous, so text edits can take time to become searchable; requests do not wait for population. Schema changes and explicit database setup can still require coordination.

Votes never update `SearchDocuments` and never require full-text reindexing. They update vote rows and discovery freshness counters. The former serializable reads could contend with those writes; seeing a loading indicator alone does not establish that indexing caused a wait. The previous vote commit's backend CI passed, but no trace of the developer's database blocking session was captured.

**Existing databases:** stop the API, run the explicit setup command, then restart:

```powershell
dotnet run --project src/hosts/Atlas.Console -- --setup-database
```

Setup now enables `ALLOW_SNAPSHOT_ISOLATION ON` for the current database, using its quoted database name, outside a transaction. The setup account needs permission to alter the database option. The operation does not force-disconnect users or change the default isolation of unrelated queries. It is idempotent and verifies readiness. Normal search/API startup never changes database options. No EF table/model change or migration is involved, and no new SQL files were added. Existing databases must run setup before using this version's snapshot reads. This setting introduces SQL Server row-version storage overhead; keep read transactions bounded to a request.

References: [Microsoft snapshot isolation](https://learn.microsoft.com/en-us/sql/connect/ado-net/sql/snapshot-isolation-sql-server) and [transaction isolation levels](https://learn.microsoft.com/en-us/sql/t-sql/statements/set-transaction-isolation-level-transact-sql).

**support-v2:** the same bounded multiplier applies to both result kinds:

- Comment support `s` is upvotes minus downvotes.
- Node support `s` is `averageImportance * voteCount / (voteCount + 1)`. Unrated nodes have zero support. More votes progressively give the average its full weight.
- Final rank is `textRelevance * (1 + 0.25 * s / (10 + abs(s)))`. Comment-only browsing without text uses a base relevance of 1. Ties retain kind/date/ID ordering.

For equally relevant text, a node rated 10 by one participant has support 5 and an 8.33% boost; a comment at +1 has a 2.27% boost, so the node comes first. Stronger text relevance or sufficiently supported comments can still outrank nodes; result kind does not impose a separate first group. Node-only browsing keeps its existing ordering.

Regression tests hold an actual SQL writer transaction open after a vote or content edit, require global search and feed reads to complete before releasing the writer, and verify they return the prior committed state. Ranking coverage verifies the one-vote 10/10 node versus +1 comment example without waiting for reindexing.


For editable controlled scenarios, real SQL diagnostic tables and shared regression fixtures, see the [Search ranking workbench](search-ranking-workbench.md).

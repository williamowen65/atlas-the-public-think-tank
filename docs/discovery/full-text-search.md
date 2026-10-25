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
- Result ordering still uses importance/votes and the existing title substring bonus; it does not use full-text relevance ranking.

## What gets searched

| Location | Indexed text |
| --- | --- |
| Node | Title and short description |
| Markdown block | Markdown text |
| Image block | Alt text and caption |
| Video block | Caption and URL |
| Link preview | Title, description and URL |
| Chart reference | Title |

- Poll/reference IDs are excluded.
- `DocumentBlocks` links restrict matches to the node's current document. Removing a link removes that block's match immediately.
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

## Regenerate the local baseline and run explicit setup

The previous migrations and snapshot have been removed at your request. Generate and commit a new baseline before running the application or migration-based CI tests.

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
2. Runs the full-text setup SQL outside a transaction.
3. Checks that full-text support, English resources and both enabled indexes are available.
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

Demo seeding requires an empty database. After seeding, open Atlas and try a text search once indexing has caught up.

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

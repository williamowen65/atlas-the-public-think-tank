# PTT-87 SQL Server console transition

The console now requires `ATLAS_SQL_CONNECTION_STRING`. On startup it applies the initial EF Core migration, then reads and writes SQL Server. The demo records are declared in `src/Atlas.Console/Storage/DemoData.cs`; no checked-in JSON data files are required. Existing JSON repository contract tests still exercise file adapters in temporary directories without configuring SQL.

## Local setup

1. Create an empty SQL Server database (for example, `Atlas`). Use an account with permission to create tables for the first run.
2. Set `ATLAS_SQL_CONNECTION_STRING` as an environment variable or .NET user secret for `src/Atlas.Console` (for example, `Server=localhost;Database=Atlas;Trusted_Connection=True;TrustServerCertificate=True`). Do not commit credentials.
3. From the repository root, run `dotnet run --project src/Atlas.Console -- --seed-demo` once. The app applies the migration and inserts the code-defined demo collections in one transaction, preserving IDs and timestamps. The command refuses to overwrite a nonempty database. Run it only when you want sample data.
4. Run `dotnet run --project src/Atlas.Console` to use the SQL database.

Back up a database before applying further migrations. To retry a failed seed, inspect the database first; the seed operation refuses a nonempty collection set. Notification data created after import is also stored in SQL.

## Current implementation and follow-up

This first SQL boundary stores each repository collection as a serialized payload in `AtlasCollections`. It keeps domain repository contracts and existing reconstitution behavior intact, but it is **not a relational per-aggregate EF model**. A SQL rowversion detects conflicting writes made after an adapter reads a row within the same operation; it does not protect a repository's separate read/modify/write calls from lost updates across processes. In particular, the participant/target uniqueness rule for votes and the occurrence/recipient uniqueness rule for notifications are not enforced by database indexes yet. Do not treat this as the final concurrency solution. Follow-up migrations should introduce separate domain tables and unique indexes before multi-process use.

The previous `data/*.json` fixtures have been removed. The running console reads and writes SQL collections. The data viewer displays the corresponding SQL collection. `--seed-demo` targets an empty SQL database only; it does not migrate changes from a prior local JSON data directory.

# PTT-87 SQL Server persistence

The console needs `ATLAS_SQL_CONNECTION_STRING`. `Atlas.Persistence` owns the EF Core `AtlasDataContext`, persistence row types, Fluent API mappings, and migration. Domain models and repository interfaces remain in their domain projects. The console wires repository adapters to that persistence boundary.

## Local setup

1. Create a **new empty** SQL Server database (for example `Atlas`). Set `ATLAS_SQL_CONNECTION_STRING` in the console project user secrets or environment. For local SQL Server with Windows authentication, one example is `Server=localhost;Database=Atlas;Trusted_Connection=True;TrustServerCertificate=True`. Do not commit credentials.
2. From the repository root run `dotnet run --project src/Atlas.Console -- --seed-demo` once. It applies the migration and inserts the code-defined demo records into an empty database, retaining IDs and timestamps. Normal startup does not seed the demo.
3. Run `dotnet run --project src/Atlas.Console`. The console reads and writes SQL rows.

The migration in this draft replaces the former `AtlasCollections` draft schema. If you already ran the earlier draft locally, use a new empty database for this revision. Back up any data you want to retain first. `--seed-demo` does not import a previous local JSON directory or the earlier serialized-collection SQL table.

## How the mapping works

For example, the Graph domain owns `Node` and `INodeRepository`; `Atlas.Persistence` owns `NodeRow` and its Fluent API table mapping. The console's repository adapter reconstitutes domain nodes from persistence records and saves updated records. There is one SQL row per node, vote, document, block, participant, reaction, community, comment, moderation case, notification, and preference. The vote table has a unique `(ParticipantId, TargetType, TargetId)` index; notifications have a unique `(OccurrenceId, RecipientParticipantId)` index.

The existing adapters still use JSON serialization **in memory** as a compatibility bridge between their stored DTOs and EF rows. The database does not store whole collections as JSON payloads. A few ordered or nested values (`BlockIds`, node parent/type IDs, reaction audit entries, notification delivery attempts) currently use value-converted JSON columns; normal aggregate fields are SQL columns. The prior file-backed repository tests remain, but they do not verify the SQL boundary. The console uses collection keys such as `nodes`, not file paths.

## Review limits

This draft has not been compiled or run in this workspace because it has no .NET SDK or SQL Server. Local build and migration checks are required. The compatibility adapters currently replace all rows in one collection per write, so cross-process read/modify/write can still lose updates. The database unique indexes prevent duplicate vote and notification keys but do not make the entire repository operation atomic. A later pass should move queries and individual upserts directly into EF repositories and normalize the remaining ordered association tables.

## SQL test

`ATLAS_SQL_TEST_CONNECTION_STRING` is an optional connection to a SQL Server instance with permission to create databases. Run `dotnet test tests/Atlas.Persistence.Tests` to create a uniquely named temporary database, apply the migration, seed every demo collection, check the mapped rows, and verify the database rejects a duplicate vote. The test deletes only its own uniquely named database. The existing file adapter tests still use temporary files.

# PTT-87 SQL Server persistence

Domain models and repository interfaces remain in their domains. `Atlas.Persistence` owns EF Core row types, Fluent API mappings, the DbContext, and migrations. The console constructs `Sql*Repository` adapters that map directly between domain objects and EF rows. Saving an object updates that object and its owned child rows; it does not rewrite other records in the table.

## Local setup

1. Create a **new empty** SQL Server database (for example `Atlas`). Set `ATLAS_SQL_CONNECTION_STRING` in the console project user secrets or environment. For local Windows authentication, one example is `Server=localhost;Database=Atlas;Trusted_Connection=True;TrustServerCertificate=True`. Do not commit credentials.
2. Run `dotnet run --project src/Atlas.Console -- --seed-demo` once. It applies the migration and loads typed seed records from `src/Atlas.Console/Storage/SeedData/`, retaining their identities and timestamps. The seed command refuses a populated database.
3. Run `dotnet run --project src/Atlas.Console` to use the SQL database.

The draft migration has been revised. If you already ran an earlier revision, use a new empty database for this revision and retain any previous database you want to keep. This command does not import previous local JSON files or the earlier draft SQL schema.

## Tables and mapping

Each node, type, document, block, participant, vote, reaction definition, node reaction, community, membership, association, comment, moderation case, notification, and preference has an EF row. Node parents, requested types, ordered document block references, reaction audit entries, and notification delivery attempts have child tables. Child keys include their owner and position, preserving order. Fluent API maps ownership with cascading deletion of child rows.

The vote table enforces unique `(ParticipantId, TargetType, TargetId)` values. Notifications enforce unique `(OccurrenceId, RecipientParticipantId)` values. The repositories map explicitly to domain reconstitution methods, preserving identities, timestamps, lifecycle states, preferences, and histories.

The old serialized repositories, stored JSON DTOs, file adapters, JSON seed payloads, and JSON value converters are removed. The console data viewer formats rows as text. Domain projects do not take an EF Core dependency.

## Verification

Set `ATLAS_SQL_TEST_CONNECTION_STRING` to a SQL Server instance where the test login may create databases. Run `dotnet test tests/Atlas.Persistence.Tests/Atlas.Persistence.Tests.csproj`. Each test creates a uniquely named temporary database and deletes only that database. Coverage includes vote creation/update/deletion and uniqueness, content block types/order/timestamps, communities and memberships, moderation decisions, notification preferences/delivery history/read state, and demo seeding. Without that environment variable the integration tests are marked inconclusive. GitHub Actions also runs this suite against an isolated SQL Server container.

No .NET SDK or SQL Server is installed in this workspace; validation runs through GitHub Actions and your local SQL Server. The adapters use short-lived DbContexts. Same-record conflicting edits use ordinary last-write behavior; optimistic version checks remain a separate design choice. Node ancestry filtering currently materializes rows before filtering, while ordinary identifier and target queries use EF predicates.

# PTT-87 SQL Server persistence

Domain models and repository interfaces remain in their domains. `Atlas.Persistence` owns EF Core row types, Fluent API mappings, the DbContext, and migrations. The console constructs `Sql*Repository` adapters that map directly between domain objects and EF rows. Saving an object updates that object and its owned child rows; it does not rewrite other records in the table.

## Local setup

1. Create a **new empty** SQL Server database (for example `Atlas`). Set `ATLAS_SQL_CONNECTION_STRING` in the console project user secrets or environment. For local Windows authentication, one example is `Server=localhost;Database=Atlas;Trusted_Connection=True;TrustServerCertificate=True`. Do not commit credentials.
2. Run `dotnet run --project src/Atlas.Console -- --seed-demo` once. It applies the migration and loads typed seed records from `src/Atlas.Console/Storage/SeedData/`, retaining their identities and timestamps. The seed command refuses a populated database.
3. Run `dotnet run --project src/Atlas.Console` to use the SQL database.

The initial relational migration is already checked in. The developer-generated `20260929202249_AddReferentialIntegrity` migration and its snapshot are now checked in; do not generate a second migration for those same relationships. Keep your existing relational database: the new migration should add constraints and indexes, without dropping tables or resetting demo data. Previous JSON files and the earlier JSON-in-SQL draft schema are not imported.

## Tables and mapping

Each node, type, document, block, participant, vote, reaction definition, node reaction, community, membership, association, comment, moderation case, notification, and preference has an EF row. Node parents, requested types, ordered document block references, reaction audit entries, and notification delivery attempts have child tables. Child keys include their owner and position, preserving order. Fluent API maps ownership with cascading deletion of child rows.

The vote table enforces unique `(ParticipantId, TargetType, TargetId)` values. Notifications enforce unique `(OccurrenceId, RecipientParticipantId)` values. The repositories map explicitly to domain reconstitution methods, preserving identities, timestamps, lifecycle states, preferences, and histories.

The old serialized repositories, stored JSON DTOs, file adapters, JSON seed payloads, and JSON value converters are removed. The console data viewer formats rows as text. Domain projects do not take an EF Core dependency.

## Verification

Set `ATLAS_SQL_TEST_CONNECTION_STRING` to a SQL Server instance where the test login may create databases. Run `dotnet test tests/Atlas.Persistence.Tests/Atlas.Persistence.Tests.csproj`. Each test creates a uniquely named temporary database and deletes only that database. Coverage includes vote creation/update/deletion and uniqueness, content block types/order/timestamps, communities and memberships, moderation decisions, notification preferences/delivery history/read state, and demo seeding. Without that environment variable the integration tests are marked inconclusive. GitHub Actions also runs this suite against an isolated SQL Server container.

No .NET SDK or SQL Server is installed in this workspace; validation runs through GitHub Actions and your local SQL Server. The adapters use short-lived DbContexts. Same-record conflicting edits use ordinary last-write behavior; optimistic version checks remain a separate design choice. Node ancestry filtering currently materializes rows before filtering, while ordinary identifier and target queries use EF predicates.

## Model-first migration workflow

`Rows.cs` and `AtlasDataContext.ConfigureModel` define the current persistence model. Update those files first, then let EF compare the model with the last migration snapshot. Do not manually update the snapshot or write migration operations to substitute for model mappings.

From the repository root, with the .NET 10 SDK installed:

```bash
dotnet tool restore
dotnet restore src/Atlas.Persistence/Atlas.Persistence.csproj
dotnet ef migrations add AddReferentialIntegrity --project src/Atlas.Persistence --context AtlasDataContext --output-dir Migrations
```

Review and commit the generated migration, its designer, and the updated snapshot together. For these relationship changes, expect foreign keys and supporting indexes. Table drops or recreation are unexpected. The design-time factory uses SQL Server metadata without starting the console, seeding records, or connecting to the server during scaffolding. The initial migration's historical model is frozen separately so future snapshot generation cannot change that earlier migration's target model.

Apply the checked-in migration using the shared console user secrets, or start the console, which also applies checked-in migrations:

```bash
dotnet ef database update --project src/Atlas.Persistence --context AtlasDataContext
```

You can override the connection for a particular update:

```bash
dotnet ef database update --project src/Atlas.Persistence --context AtlasDataContext --connection "YOUR_SQL_CONNECTION_STRING"
```

The factory and console both load the same user secrets ID, then environment variables (which override secrets). The configuration key is `ATLAS_SQL_CONNECTION_STRING`; configuration lookup is case-insensitive. A missing or blank connection throws a configuration error instead of silently connecting to localhost. The secrets file is stored locally and is not committed.

Foreign keys cover node descriptions, authors and types; parent nodes and requested types; document blocks; voting participants; reaction definitions, applications and audit references; community owners, memberships and node associations; comment authors and parents; moderation participants and nodes; and notification participants and preferences. Cross-record references use `NoAction` deletion, preserving referenced records. Owned child rows retain cascading deletion.

Deliberate exceptions are polymorphic references (`VoteRow.TargetId`, `CommentRow.TargetId`, `NotificationRow.SubjectId`), block references to poll/chart entities that do not yet have tables, event occurrence IDs, and the opaque string `NodeTypeRow.OwnerId`. These are not claimed to have database referential integrity. Polymorphic targets need a schema design such as separate target columns with foreign keys and a check constraint; a single ordinary foreign key cannot point to different tables based on a kind column.

Existing orphaned references will cause SQL Server to reject the new constraints. Review and correct such records explicitly; migrations do not silently delete data or invent participants. The demo seed's structural references have been checked against its fixed IDs.

The SQL CI job checks that the model has no pending changes and runs the checked-in migrations against SQL Server. It does not scaffold missing migrations automatically. SQL tests cover rejection of missing references, restricted deletion, upgrading existing relational rows, and the demo seed under the constraints.

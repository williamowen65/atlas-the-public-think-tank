# Atlas.Persistence

EF Core row models, SQL Server mappings and migrations for Atlas. Domain behavior remains in the owning domain projects; SQL repositories in Atlas.Application map domain objects to these rows.

## Common database tasks

| Goal | Recommended action |
|---|---|
| Apply committed schema changes | Run `dotnet ef database update` |
| Generate a new schema change | Run `dotnet ef migrations add` and review the output |
| Replace disposable local data | Follow the reset steps below; this deletes data |

## Reset the local database to demo seed data (PowerShell)

This deletes the configured application database, including local accounts and content, and recreates it from all committed migrations. Stop the Console app first. Run from the solution folder with your usual local connection configuration (`ATLAS_SQL_CONNECTION_STRING`). Environment variables override user secrets. Confirm that EF targets the intended local database, not a shared or production database.

Generate and commit any required migration before this reset; `database update` applies existing migrations and does not create them.

```powershell
# Restore the repository's local EF tool.
dotnet tool restore
if ($LASTEXITCODE -ne 0) { throw "EF tool restore failed." }

# Inspect the target database before deleting it.
dotnet ef dbcontext info --project src/Atlas.Persistence
if ($LASTEXITCODE -ne 0) { throw "Could not inspect the database target." }

# Deletes local data. EF asks for confirmation; do not add --force.
dotnet ef database drop --project src/Atlas.Persistence
if ($LASTEXITCODE -ne 0) { throw "Database drop failed; reset stopped." }

# Recreate the database and apply the complete migration history.
dotnet ef database update --project src/Atlas.Persistence
if ($LASTEXITCODE -ne 0) { throw "Database migration failed; seeding stopped." }

# Seed demo accounts and content. This requires an empty database.
dotnet run --project src/hosts/Atlas.Console -- --seed-demo
if ($LASTEXITCODE -ne 0) { throw "Demo seeding failed; normal launch stopped." }

# Launch the Console application normally.
dotnet run --project src/hosts/Atlas.Console
```

If you decline the drop confirmation, stop the reset instead of continuing with database update/seeding. The old database remains, and demo seeding will reject a populated database.

After a successful reset, demo data should be visible and users should have **3 of 3 Node creation slots** available. Demo seeding is exempt from ordinary creation quotas. Creating an ordinary Node consumes its author's allowance.

## Generate a migration locally

**A migration has three reviewable pieces:** the migration code, its designer, and the EF model snapshot. Commit them together.

For the PTT-133 quota model, before applying/resetting the database:

```powershell
dotnet tool restore
dotnet ef migrations add AddNodeCreationQuota --project src/Atlas.Persistence
```

Review the generated table/index changes, then commit the migration, designer and model snapshot to the PR branch. Do not generate another migration with the same purpose if it is already present. If an earlier draft migration was applied locally, reconcile its migration history before applying a replacement; changing source files does not undo database changes.

See [persistence validation and SQL tests](../../docs/security/validation/PERSISTENCE.md) and [Node creation quota behavior](../../docs/security/validation/NODE-CREATION-QUOTA.md).

## Node short description (pending migration)

`NodeRows.ShortDescription` is a plain-text introduction limited to 280 UTF-16 code units, owned by Graph alongside the title. The required column has an empty-string default for existing rows. Existing nodes remain readable but need an authored introduction; the API and Console ask for one on creation. The full Content document and its `DescriptionId` are unchanged. Discovery adds `shortDescription` separately and includes it in text search; hidden cards expose only the moderation notice.

From the solution root, generate and review the migration locally:

```powershell
dotnet tool restore
dotnet ef migrations add AddNodeShortDescription --project src/Atlas.Persistence
dotnet ef migrations has-pending-model-changes --project src/Atlas.Persistence
# Review and commit the migration, designer, and updated model snapshot.
dotnet ef database update --project src/Atlas.Persistence
dotnet test tests/Atlas.Graph.Tests/Atlas.Graph.Tests.csproj
dotnet test tests/Atlas.Persistence.Tests/Atlas.Persistence.Tests.csproj
dotnet test tests/Atlas.Api.Tests/Atlas.Api.Tests.csproj
dotnet test tests/Atlas.Console.Tests/Atlas.Console.Tests.csproj
```

The SQL tests require the documented test connection configuration. Migration-dependent checks remain pending until the generated migration, designer, and snapshot are committed. To see the new authored demo introductions, follow the local reset/reseed script above after generating the migration. A reset is optional and destructive; a normal database update preserves existing data and leaves its new descriptions empty for later editing.

## Historical note: temporal content history (superseded)

**Current direction:** Moderation uses immutable, case-linked reported snapshots and append-only audit events instead of relying on SQL Server temporal content history. See [moderation snapshot migration](../../docs/Architecture/moderation-snapshots-migration.md).

The instructions below describe the earlier temporal-history approach and are retained for historical context; do not generate a new temporal-history migration from them.

### Earlier temporal migration instructions

SQL Server system-versioned history is configured for `NodeRows`, `DocumentRows`,
`BlockRows`, and `DocumentBlocks` (the ordered document/block join). The main tables
remain the source of current state; history tables preserve previous database row versions.
Moderation cases, votes, reactions and comments are intentionally not temporal.
This change does **not** yet provide a revision-comparison API, UI, or retention cleanup.

From the solution root, on the feature branch, generate and review the EF migration:

```powershell
dotnet tool restore
dotnet ef migrations add AddTemporalNodeContentHistory --project src/Atlas.Persistence
dotnet ef migrations has-pending-model-changes --project src/Atlas.Persistence
# Review generated Up/Down SQL, designer and model snapshot before applying.
dotnet ef migrations script --project src/Atlas.Persistence --output temporal-content-migration.sql
# Inspect the configured connection and verify it is your local development database.
dotnet ef dbcontext info --project src/Atlas.Persistence
dotnet ef database update --project src/Atlas.Persistence
```

Do not reset or drop the database to apply this migration. Commit the migration,
designer and model snapshot. Verify the generated migration preserves current rows,
indexes, constraints and SQL Server full-text infrastructure. SQL Server temporal
periods record transaction times in UTC; they do not encode moderation decisions.
Do not purge history while an active moderation review needs its original snapshot.

# Atlas.Persistence

Atlas.Persistence contains **EF Core row models, SQL Server mappings, and migrations**. Domain rules stay in the owning domain projects; SQL repositories in Atlas.Application translate between domain objects and database rows.

## Common database tasks

| Goal | Action |
|---|---|
| Apply committed schema changes | `dotnet ef database update` |
| Create a migration for a model change | `dotnet ef migrations add`, then review generated files |
| Replace disposable local data | Follow the [destructive reset](#reset-a-disposable-local-database) below |

## Reset a disposable local database

> **Data loss warning:** This deletes the database configured by `ATLAS_SQL_CONNECTION_STRING`, including local accounts and content. Stop Console first, inspect the target, and back up anything you need. Never run these steps against shared or production data.

From the solution folder in PowerShell, using your normal local connection configuration (environment variables override user secrets):

```powershell
# Restore the repository's EF tool.
dotnet tool restore
if ($LASTEXITCODE -ne 0) { throw "EF tool restore failed." }

# Inspect the database target before deleting anything.
dotnet ef dbcontext info --project src/Atlas.Persistence
if ($LASTEXITCODE -ne 0) { throw "Could not inspect the database target." }

# Destructive: asks for confirmation. Do not add --force.
dotnet ef database drop --project src/Atlas.Persistence
if ($LASTEXITCODE -ne 0) { throw "Database drop failed; reset stopped." }

# Apply all committed migrations to a recreated database.
dotnet ef database update --project src/Atlas.Persistence
if ($LASTEXITCODE -ne 0) { throw "Database migration failed; seeding stopped." }

# Seed the empty database.
dotnet run --project src/hosts/Atlas.Console -- --seed-demo
if ($LASTEXITCODE -ne 0) { throw "Demo seeding failed; normal launch stopped." }

# Launch Console.
dotnet run --project src/hosts/Atlas.Console
```

Generate and commit any needed migrations **before** resetting. `database update` applies migrations; it does not generate them. If you decline the drop confirmation, stop instead of proceeding with seeding.

After a successful reset, demo users should have **3 of 3 Node creation slots**. Seeding is exempt from ordinary quotas; creating a Node normally consumes an author's allowance.

## Generate and review a migration

An EF migration has **three parts to review and commit together**:

1. Migration code (schema changes)
2. Migration designer
3. Updated EF model snapshot

### Node creation quota example (PTT-133)

If that migration has not already been generated, run:

```powershell
dotnet tool restore
dotnet ef migrations add AddNodeCreationQuota --project src/Atlas.Persistence
```

Review table/index changes before committing. **Do not regenerate an already present migration.** If an earlier draft was applied locally, reconcile migration history before replacing it; editing source does not roll back a database migration.

See [persistence SQL validation](../../docs/security/validation/PERSISTENCE.md) and [creation quota behavior](../../docs/security/validation/NODE-CREATION-QUOTA.md).

## Node short description: migration notes

`NodeRows.ShortDescription` is a Graph-owned plain-text introduction, limited to **280 UTF-16 code units**. The column is required and defaults to empty for existing rows.

- The node's full Content document and `DescriptionId` do not change.
- API and Console request a short description when a Node is created.
- Discovery displays and searches the short description.
- Hidden cards show the moderation notice, not the short description.

To generate/review the change **only if it has not already been committed**:

```powershell
dotnet tool restore
dotnet ef migrations add AddNodeShortDescription --project src/Atlas.Persistence
dotnet ef migrations has-pending-model-changes --project src/Atlas.Persistence
# Review and commit the migration, designer, and updated snapshot.
dotnet ef database update --project src/Atlas.Persistence
dotnet test tests/Atlas.Graph.Tests/Atlas.Graph.Tests.csproj
dotnet test tests/Atlas.Persistence.Tests/Atlas.Persistence.Tests.csproj
dotnet test tests/Atlas.Api.Tests/Atlas.Api.Tests.csproj
dotnet test tests/Atlas.Console.Tests/Atlas.Console.Tests.csproj
```

SQL tests require the documented connection. Migration-dependent checks remain pending until the migration, designer, and snapshot are committed.

A normal database update preserves existing content and leaves new descriptions empty for later editing. To refresh demo introductions, use the **optional, destructive** reset above.

## Moderation content history

**Present and proposed designs are different:**

| State | Approach |
|---|---|
| Existing historical implementation | SQL Server system-versioned temporal tables for node/document/block content |
| Proposed future replacement | Immutable snapshots linked to reports, plus append-only moderation audit events |

See the [moderation snapshot migration plan](../../docs/Architecture/moderation-snapshots-migration.md). **Do not treat that proposal as an applied migration.**

### Historical temporal migration instructions (superseded)

The earlier setup used temporal history on `NodeRows`, `DocumentRows`, `BlockRows`, and `DocumentBlocks` (the ordered document/block join). The current tables stored live content; history tables captured earlier row versions. Moderation cases, votes, reactions, and comments were not temporal.

These steps are retained **only for historical reference**. Do **not** create a new temporal-history migration from this guide.

```powershell
# Historical example only — do not execute as a new migration.
dotnet tool restore
dotnet ef migrations add AddTemporalNodeContentHistory --project src/Atlas.Persistence
dotnet ef migrations has-pending-model-changes --project src/Atlas.Persistence
dotnet ef migrations script --project src/Atlas.Persistence --output temporal-content-migration.sql
dotnet ef dbcontext info --project src/Atlas.Persistence
dotnet ef database update --project src/Atlas.Persistence
```

The original migration guidance called for reviewing generated Up/Down SQL, designer, and snapshot, and preserving rows, indexes, constraints, and full-text infrastructure. SQL Server temporal periods represent transaction times in UTC, **not moderation decisions**.

Do not purge temporal history while an active moderation review depends on its original content.

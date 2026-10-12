# Atlas.Persistence

EF Core row models, SQL Server mappings and migrations for Atlas. Domain behavior remains in the owning domain projects; SQL repositories in the Console host map domain objects to these rows.

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
dotnet run --project src/Atlas.Console -- --seed-demo
if ($LASTEXITCODE -ne 0) { throw "Demo seeding failed; normal launch stopped." }

# Launch the Console application normally.
dotnet run --project src/Atlas.Console
```

If you decline the drop confirmation, stop the reset instead of continuing with database update/seeding. The old database remains, and demo seeding will reject a populated database.

After a successful reset, demo data should be visible and users should have **3 of 3 Node creation slots** available. Demo seeding is exempt from ordinary creation quotas. Creating an ordinary Node consumes its author's allowance.

## Generate a migration locally

For the PTT-133 quota model, before applying/resetting the database:

```powershell
dotnet tool restore
dotnet ef migrations add AddNodeCreationQuota --project src/Atlas.Persistence
```

Review the generated table/index changes, then commit the migration, designer and model snapshot to the PR branch. Do not generate another migration with the same purpose if it is already present. If an earlier draft migration was applied locally, reconcile its migration history before applying a replacement; changing source files does not undo database changes.

See [persistence validation and SQL tests](../../docs/security/validation/PERSISTENCE.md) and [Node creation quota behavior](../../docs/security/validation/NODE-CREATION-QUOTA.md).

# PTT-94: Identity setup and developer-generated migration

PTT-94 adds account infrastructure for the future HTTP host. Participants remains a plain domain project. ASP.NET Core Identity owns credentials, password hashing, security stamps, tokens, lockout, and global roles. [Architecture](architecture/IDENTITY.md) explains the mapping and host responsibilities.

## Generate the migration yourself

The PR deliberately does not include a generated Identity migration or an updated snapshot. Generate these with EF on the PR branch. There is no separate Identity migration command: Identity contributes its model to the existing `AtlasDataContext`, and EF generates the SQL Server migration.

From the repository root:

```powershell
git fetch origin
git switch PTT-94-identity-participants
git pull --ff-only
dotnet tool restore
dotnet restore src/Atlas.Persistence/Atlas.Persistence.csproj
dotnet ef migrations add AddIdentityAccounts --project src/Atlas.Persistence --context AtlasDataContext
```

Use your existing `ATLAS_SQL_CONNECTION_STRING` user secret. The persistence design-time factory already uses the same secrets ID as the console. Scaffolding builds the model without connecting to SQL Server. Do not start the console before scaffolding: its existing startup migration call detects the uncommitted model change.

EF should produce three files in `src/Atlas.Persistence/Migrations`:

- A timestamped `AddIdentityAccounts.cs` migration.
- Its matching `.Designer.cs` historical target model.
- The updated `AtlasDataContextModelSnapshot.cs`.

The migration should add the `identity` schema; Users, Roles, UserRoles, UserClaims, RoleClaims, UserLogins, and UserTokens; three role records; indexes; and the Users.Id → ParticipantRows.Id foreign key. Existing domain tables and rows should not be dropped, recreated, or changed. Numeric claim IDs should retain SQL identity generation. The roles are metadata only: no account, password, administrator, or role assignment is seeded.

After reviewing the generated files, commit and push them to the same PR branch:

```powershell
git add src/Atlas.Persistence/Migrations
git commit -m "PTT-94: generate Identity account migration"
git push origin PTT-94-identity-participants
```

The assistant can then review the generated migration and CI results. The PR remains draft until this is done. The existing pending-model check and SQL tests will fail before the migration is committed; this is expected and must not be bypassed by ignoring pending-model warnings or auto-generating migrations in CI.

## Apply to your existing local SQL database

After migration review, run:

```powershell
dotnet ef migrations has-pending-model-changes --project src/Atlas.Persistence
dotnet ef database update --project src/Atlas.Persistence --context AtlasDataContext
```

Keep the current database and demo data. Do not reset or reseed it, and do not regenerate the earlier referential-integrity migration. Updating applies only migrations missing from `__EFMigrationsHistory`. Console startup also applies committed migrations under the existing prototype behavior; the explicit update command makes this step visible.

## Verify SQL behavior

CI creates an isolated SQL Server database for each Identity test fixture. Local tests use `ATLAS_SQL_TEST_CONNECTION_STRING` as a server connection template and create/drop only uniquely named test databases. Do not give this variable a connection with access only to a pre-existing database; the tests need create-database permissions.

```powershell
dotnet test tests/Atlas.Identity.Tests/Atlas.Identity.Tests.csproj
```

Without the test connection, only the model, domain-dependency, and service-wiring tests run; SQL tests report inconclusive/skipped. After the migration is committed, CI verifies registration/reload, rollback, confirmation, lockout, password changes/resets, policy decisions, existing-row preservation, and SQL constraints against SQL Server.

## What this does not enable yet

The current Console remains the explicit demo participant-switching harness. It does not authenticate its selected participant and does not use the new Identity policy adapter. PTT-94 must not be mistaken for securing that console or publishing an HTTP login API. HTTP endpoints, real confirmation/reset email delivery, antiforgery/rate-limit behavior, trusted first-administrator provisioning, and persistent Data Protection keys are host integration work. No shared demo passwords or automatic linking of an existing public profile to a newly registering user is provided.

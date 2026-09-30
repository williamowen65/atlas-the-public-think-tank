# PTT-94: Identity setup and developer-generated migration

PTT-94 adds account infrastructure for the future HTTP host. Participants remains a plain domain project. ASP.NET Core Identity owns credentials, password hashing, security stamps, tokens, lockout, and global roles. [Architecture](architecture/IDENTITY.md) explains the mapping and host responsibilities.

## Migration status

The developer-generated `20260930213148_AddIdentityAccounts` migration, designer, and updated snapshot are checked in. The console registration/sign-in follow-up changes no EF mappings and needs no additional migration. Keep generating future migrations yourself through `dotnet ef migrations add <Name> --project src/Atlas.Persistence` when the model changes.

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

## Use registration and sign-in

```powershell
dotnet run --project src/Atlas.Console
```

The entry menu offers Sign in, Register, and Exit. Registration creates a new Member account with a new linked Participant, asks for password confirmation, and returns to the entry menu. Password entry requires an interactive terminal and is not echoed. Existing demo profiles are browseable but cannot be selected as authenticated actors.

Email confirmation is still required. Until real email delivery is connected, a trusted local operator can confirm a newly created account in a second terminal:

```powershell
dotnet run --project src/Atlas.Console -- --confirm-email
```

Enter the account email, then type `CONFIRM`. This explicitly confirms the account through Identity's token provider; it is a development operator utility, not verification that someone owns an email address, and must not become a public endpoint. It neither assigns a privileged role nor claims an existing demo profile.

Return to the first terminal and sign in with the email/password. Main-menu option 1 signs out; option 2 opens your profile; option 13 exits. Browsing someone else's profile never changes your signed-in actor. Global moderation access comes from stored Identity roles, replacing the configured moderator-ID list. Public registration only assigns Member; first-administrator/role provisioning remains controlled operator work.

Console sessions use fresh SQL scopes, check account/profile/Member-policy eligibility before each main menu, and invalidate the session when its security stamp changes. Long-running nested menus do not implement continuous session expiry; domain authorization still applies to their actions. Accounts requiring 2FA are rejected with a clear message because the console has no second-factor entry flow yet.

Real confirmation/reset email delivery, account-management HTTP endpoints, browser security controls, and persistent/shared Data Protection keys remain HTTP-host work.

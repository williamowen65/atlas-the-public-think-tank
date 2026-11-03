# PTT-94: Identity setup and migrations

Atlas uses ASP.NET Core Identity to manage accounts. **Participants remain a separate domain** that owns public profiles and contributions.

For account/profile mapping and authorization design, see [Identity architecture](architecture/IDENTITY.md).

## Quick reference

| Task | Where to look |
|---|---|
| Apply an existing migration | [Update your local database](#update-your-local-database) |
| Run SQL identity tests | [Verify SQL behavior](#verify-sql-behavior) |
| Register and sign in | [Local account workflow](#local-account-workflow) |
| Grant Global Moderator access | [Trusted moderator provisioning](#trusted-moderator-provisioning-ptt-159) |
| Load demo accounts | [Demo seed accounts](#demo-seed-accounts) |

## Migration ownership

The **persistence project owns the migration**; the **Console project supplies startup configuration** to EF tooling.

| Argument | Responsibility |
|---|---|
| `--project src/Atlas.Persistence` | Contains `AtlasDataContext`, generated migrations, and the EF model snapshot |
| `--startup-project src/hosts/Atlas.Console` | Supplies runtime configuration and services for design-time context construction |

When Console is the startup project, it must reference `Microsoft.EntityFrameworkCore.Design` with `PrivateAssets=all`. Its EF Core package version must match the rest of the solution.

The checked-in identity migration is `20260930213148_AddIdentityAccounts` (including its designer and updated model snapshot). Later Console registration and sign-in changes do not alter EF mappings and need no new migration.

To generate **future** migrations after a model change:

```powershell
dotnet ef migrations add <Name> --project src/Atlas.Persistence --startup-project src/hosts/Atlas.Console
```

## Update your local database

**This applies missing schema migrations without resetting existing data.**

Review the migration first, then run from the repository root:

```powershell
dotnet ef migrations has-pending-model-changes --project src/Atlas.Persistence --startup-project src/hosts/Atlas.Console
dotnet ef database update --project src/Atlas.Persistence --startup-project src/hosts/Atlas.Console --context AtlasDataContext
```

- Keep your current database and demo data; do not drop or reseed them.
- Do not regenerate the earlier referential-integrity migration.
- EF applies only migrations missing from `__EFMigrationsHistory`.
- Console startup also applies committed migrations under existing prototype behavior. The explicit command makes that action visible.

## Verify SQL behavior

```powershell
dotnet test tests/Atlas.Identity.Tests/Atlas.Identity.Tests.csproj
```

CI uses isolated SQL Server databases for Identity fixtures. Locally, set `ATLAS_SQL_TEST_CONNECTION_STRING` to a **server connection template** whose credentials can create and drop uniquely named test databases. It must not be restricted to an already existing database.

Without that test connection, model, domain-dependency, and service-wiring tests still run; SQL tests report skipped/inconclusive.

After migration, CI verifies registration and reload, rollback, confirmation, lockout, password changes/resets, policy decisions, existing-row preservation, and SQL constraints.

## Local account workflow

### 1. Register

Start the interactive Console:

```powershell
dotnet run --project src/hosts/Atlas.Console
```

The entry menu offers **Sign in**, **Register**, and **Exit**. Registration:

- Creates a new Member account and linked Participant
- Requests password confirmation without echoing the password
- Returns to the entry menu

Existing demo profiles can be viewed but cannot be claimed as your authenticated identity.

### 2. Confirm the email in development

Until actual email delivery exists, a **trusted local operator** can use a second terminal:

```powershell
dotnet run --project src/hosts/Atlas.Console -- --confirm-email
```

Enter the account email and type `CONFIRM`.

This uses Identity's confirmation token provider, **but does not prove that the user owns the email address**. It must not become a public endpoint, assign a privileged role, or claim an existing demo profile.

### 3. Sign in and navigate

Return to the first terminal and sign in with your email and password.

| Main-menu option | Action |
|---|---|
| 1 | Sign out |
| 2 | Open your profile |
| 13 | Exit |

Browsing another profile never changes the signed-in actor. Public registration assigns only **Member**. Global Moderator access must be granted by a trusted operator; Administrator provisioning is separate.

### Session safeguards and limitations

- Console checks account, profile, and `Member` policy eligibility with fresh SQL scopes before each main menu.
- Security-stamp changes invalidate a session.
- Domain authorization still protects actions inside nested menus, although these do not continuously enforce session expiry.
- Accounts requiring 2FA are rejected because Console has no second-factor entry flow yet.

Actual confirmation/reset email delivery, account-management HTTP endpoints, browser security controls, and persistent/shared Data Protection keys remain HTTP-host work.

## Trusted moderator provisioning (PTT-159)

**Who can grant this role?** A trusted operator with terminal and database access—not a member through public registration.

1. Configure the **intended SQL connection** in a trusted interactive terminal.
2. Register the target account normally and confirm its email first.
3. Run:

   ```powershell
   dotnet run --project src/hosts/Atlas.Console -- --grant-global-moderator
   ```

4. Enter the **existing account email** and type `GRANT GLOBAL MODERATOR` exactly.
5. Have the user sign out and sign in again.
6. Verify **Review Node reports** appears in Console, or check the protected API moderation queue.

Repeat the command for each additional moderator.

**Safety rules:**

- Unknown, unconfirmed, locked, or inactive accounts are refused.
- Granting a role already held is a safe no-op.
- The command persists the seeded `GlobalModerator` role and changes the security stamp on a successful grant.
- It is **not** an HTTP endpoint, public registration option, Administrator grant, or email-confirmation bypass.
- Treat terminal and database access as privileged. Exercise the workflow against a development database before any production operation.

No new migration is needed; the role is already seeded.

## Demo seed accounts

`--seed-demo` creates **Demo User 01–05** through `AtlasAccounts.RegisterAsync`. Their generated Participant/Identity GUIDs are reused for authored nodes, votes, communities, memberships, and reactions.

| Setting | Value |
|---|---|
| Emails | `demo01@example.test` through `demo05@example.test` |
| Password | Printed locally by the seed command |
| Intended use | Development seed data only |

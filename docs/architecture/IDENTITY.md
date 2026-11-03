# Identity and Participants

Atlas keeps **public identities** (Participants) separate from **login accounts** (ASP.NET Core Identity). [PTT-94](https://thepublicthinktank.atlassian.net/browse/PTT-94) introduced the infrastructure; [ADR-0006](decisions/ADR-0006-identity-participant-mapping.md) explains the design decision. See [developer setup](../PTT-94-Identity.md) for migrations and operator commands.

## Who owns what?

| Concern | Owner | Implementation |
|---|---|---|
| Public names, bios, active state, and contributions | Participants | `Participant`, `ParticipantId`, `ParticipantRows` |
| Credentials, confirmation, lockout, tokens, global roles | ASP.NET Core Identity | `AtlasIdentityUser : IdentityUser<Guid>`, framework managers |
| Account/profile coordination and authorization | Atlas.Identity | `AtlasAccounts`, `AtlasSignInManager`, policies, adapters |
| Database mappings and migrations | Atlas.Persistence | `AtlasDataContext`, `IdentityModel` |
| Nodes, communities, ownership, moderation cases | Their domain/application workflows | Domain rules still apply after authentication |

### Account-to-profile mapping

An Identity account's `Id` is its `ParticipantId`.

- A one-to-one foreign key links `identity.Users.Id` to `ParticipantRows.Id`, so an account requires a Participant.
- A Participant **can exist without a login account**, preserving existing demo profiles.
- An associated profile cannot be physically deleted while its account exists. Public contributions are not cascade-deleted.
- Identity has no domain navigation property; Participants does not depend on EF or Identity.

### SQL configuration

Atlas retains one physical database and context. Domain tables keep their existing names/schema; seven account tables live under `identity`.

- Configure Identity's framework model first. Atlas's supplied-key rule applies only to Atlas rows; framework claim IDs stay database-generated integers.
- Normalize usernames and non-null emails, with unique SQL indexes.
- Seed roles with stable GUIDs and concurrency stamps so migrations do not show model changes on every build.

## Registration: one transaction

`AtlasAccounts.RegisterAsync(email, password, displayName)` creates a validated Participant, then an account with the same GUID. The shared scoped context uses a **serializable transaction**.

1. Check that the display name is unused under SQL collation; insert the profile.
2. Use `UserManager.CreateAsync` for account/password validation, hashing, and storage.
3. Assign **Member only** through `UserManager.AddToRoleAsync`.
4. Commit profile, account, and membership together.

**On failure:** Validation returns `IdentityResult` errors; transaction disposal rolls back profile and account changes. Unexpected database/configuration failures propagate after rollback. Clear the change tracker so rolled-back objects are not later saved.

**Concurrency rules:**

- Give registration a dedicated request scope; do not share its `DbContext` concurrently or mix unrelated pending changes into it.
- SQL deadlocks and unique-index races can surface as infrastructure failures. If retrying, retry the **whole registration** in a fresh scope.
- SQL constraints remain authoritative; no process-local lock substitutes for them.

The caller cannot pick an account ID, attach an existing profile, or request a privileged role. Demo profiles are not claimable by email or display-name matching.

## Sign-in and account security

**Password verification alone is not authentication.** The complete framework sign-in flow checks account confirmation, lockout, linked-profile eligibility, and 2FA status.

`AddAtlasIdentity(connectionString)` registers `UserManager<AtlasIdentityUser>`, `RoleManager<IdentityRole<Guid>>`, custom `SignInManager<AtlasIdentityUser>`, EF stores, cookies, Data Protection token providers, the account coordinator, policies, and the moderation adapter.

| Rule | Current behavior |
|---|---|
| Password | At least 12 characters; Identity's digit, uppercase, lowercase, and special-character requirements remain |
| Email | Confirmation required for new accounts |
| Lockout | Five failed attempts trigger 15 minutes when sign-in uses `lockoutOnFailure: true` |
| Profile | `AtlasSignInManager.CanSignInAsync` requires an active linked Participant |
| 2FA | Must be handled as part of framework sign-in; Console cannot complete 2FA |

Use framework methods for password changes, confirmation, resets, 2FA, and sign-in/sign-out. **Do not use `CheckPasswordAsync` as the login flow**: it checks a hash, not whether sign-in is permitted.

Tests use `CheckPasswordSignInAsync` to exercise eligibility and lockout without issuing cookies. Test email confirmation uses framework-generated tokens; production confirmation email transport is not implemented yet.

## Authorization policies

| Policy | Stored roles allowed |
|---|---|
| `Atlas.Member` | Member, GlobalModerator, Administrator |
| `Atlas.GlobalModerator` | GlobalModerator, Administrator |
| `Atlas.Administrator` | Administrator |

Every policy also requires an authenticated principal with a **persisted, confirmed, unlocked account and active Participant**.

### Resolving the current actor

`AtlasAccounts.ResolveParticipantAsync` resolves the framework user-ID claim against validated account/profile state. It rejects anonymous, missing, malformed, empty, unknown, locked, unconfirmed, and inactive identities.

- Policy checks read **current database roles**, not old role claims embedded in a cookie. Removal takes effect at the next policy check.
- Cookies retain Identity's security-stamp validation; profile eligibility is rechecked on each cookie request. Password-change stamp invalidation retains the framework validation interval.
- A global role does **not** grant ownership of another member's profile/node or community moderator authority.

### Moderation authorization

`IdentityModeratorAuthorization` implements `IModeratorAuthorization` using persisted GlobalModerator/Administrator membership and active-account eligibility.

The host derives the actor from authenticated identity. A caller-supplied GUID cannot impersonate another Participant. Moderation still owns reports, decisions, hide/restore behavior, and case history.

## Host responsibilities

### HTTP/API host

The HTTP host configures `AddAtlasIdentity(connectionString)` and authentication/authorization middleware. Protected routes use `AtlasPolicies` and resolve the actor through `AtlasAccounts`—**never from a request body's claimed actor ID**.

Registration must go through the account coordinator. Calling `UserManager.CreateAsync` directly from a stock registration endpoint would skip Participant creation and violate the foreign key.

The host handles:

- Actual account-management endpoints and UI
- Confirmation/reset email delivery
- Rate limiting, HTTPS, cookie/CSRF behavior, and persistent/shared Data Protection keys

The cookie is HttpOnly and Secure. Framework managers/token providers alone do not mean a complete account-management HTTP API is available.

### Console host

Console uses `ConsoleAccountCommands` for register/sign-in and `ConsoleIdentitySession` for the authenticated actor.

- Interactive participant switching and direct profile-only creation were removed.
- Each main menu refreshes SQL account/Member-policy/security-stamp eligibility; sign-out discards the session.
- Browsing a profile does not change the actor.
- Global moderation checks the Identity authorization port, not configured moderator GUIDs.
- Console rejects 2FA-enabled accounts rather than treating a password check as complete login.

### Trusted operator utilities

| Command | Purpose | Boundary |
|---|---|---|
| `--confirm-email` | Locally confirm an account with framework tokens | Trusted development tooling; not proof of email ownership |
| `--grant-global-moderator` | Grant the seeded role to a confirmed, active, unlocked existing account | Terminal-only with explicit confirmation; idempotent; not a public endpoint |

See [registration and sign-in](../PTT-94-Identity.md#local-account-workflow) and [moderator provisioning](../PTT-94-Identity.md#trusted-moderator-provisioning-ptt-159).

Administrator provisioning is separate. Public registration never grants a privileged role, and the first registrant is **not** automatically an administrator. Neither Console utility requires an additional migration.

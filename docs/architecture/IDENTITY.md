# Identity and Participants

[PTT-94](https://thepublicthinktank.atlassian.net/browse/PTT-94) introduces infrastructure for authenticated Atlas hosts. [ADR-0006](decisions/ADR-0006-identity-participant-mapping.md) records the key decisions. [Setup](../PTT-94-Identity.md) covers the developer-generated migration.

## Ownership and identity mapping

| Concern | Owner | Implementation |
|---|---|---|
| Public display name, bio, active state, timestamps | Participants | `Participant`, `ParticipantId`, existing `ParticipantRows` |
| Password hashes, email confirmation, lockout, tokens, roles | ASP.NET Core Identity | `AtlasIdentityUser : IdentityUser<Guid>` and framework managers |
| SQL mapping and migration history | Atlas.Persistence | `AtlasDataContext` and `IdentityModel` |
| Account/profile registration and host authorization seam | Atlas.Identity | `AtlasAccounts`, `AtlasSignInManager`, policies and adapters |
| Ownership, node lifecycle, community membership, moderation cases | Atlas domain/application workflows | Existing domain rules still run after authentication |

An Identity user's Id equals its ParticipantId. A one-to-one SQL foreign key from `identity.Users.Id` to `ParticipantRows.Id` prevents an account without a profile. A profile may exist without an account, preserving all existing demo profiles. A profile cannot be physically deleted while its account exists; there is no cascading deletion of public contributions. Identity has no domain navigation property, and Participants references neither EF nor Identity.

The existing shared physical database/context is retained. Domain tables stay at their existing names/schema; the seven account tables use the `identity` schema. Identity's built-in model is configured first; Atlas's supplied-key rule applies only to Atlas row types. Identity claim IDs remain database-generated integers. Normalized usernames and non-null normalized emails have unique SQL indexes. Migration-owned role seed metadata uses fixed GUIDs and concurrency stamps, not generated values that cause pending changes on every build.

## Registration transaction

`AtlasAccounts.RegisterAsync(email, password, displayName)` creates a validated Participant and uses its new GUID for the account. It uses a serializable transaction on the same scoped context as UserManager:

1. Check the display name is unused under SQL collation and insert the profile.
2. Let `UserManager.CreateAsync` validate the account/password and hash/store the password.
3. Assign only Member via `UserManager.AddToRoleAsync`.
4. Commit profile, account, and membership together.

Validation failures return IdentityResult errors; transaction disposal rolls back the profile as well as credentials/membership. Unexpected database/configuration failures propagate after rollback. The tracker is cleared so rolled-back state cannot later be saved accidentally. Use a dedicated request scope; do not share the DbContext concurrently or mix unrelated pending changes into registration. SQL deadlocks/concurrent unique-index conflicts may surface as infrastructure failures; retry a whole registration in a fresh scope if the host chooses to retry. No process-local lock replaces SQL constraints.

The caller cannot select an account ID, existing profile, or privileged role. Existing demo profiles are not claimable by email or display-name matching.

## Framework account management

`AddAtlasIdentity(connectionString)` registers `UserManager<AtlasIdentityUser>`, `RoleManager<IdentityRole<Guid>>`, a custom `SignInManager<AtlasIdentityUser>`, EF stores, cookies, Data Protection token providers, the account coordinator, policies, and the moderation adapter.

Use framework methods for password changes, email confirmation, password resets, 2FA, and sign-in/sign-out. Password rules require at least 12 characters and retain Identity's digit/uppercase/lowercase/non-alphanumeric defaults. New accounts require confirmed email. Five failed password attempts lock the account for 15 minutes when sign-in uses `lockoutOnFailure: true`. `AtlasSignInManager.CanSignInAsync` also requires an active linked profile. Do not authenticate by calling `CheckPasswordAsync` alone: that verifies a hash but does not execute the sign-in eligibility, lockout, or 2FA flow.

Tests use `CheckPasswordSignInAsync` to exercise password/eligibility/lockout behavior without issuing browser cookies. A future HTTP host should use the framework sign-in methods and handle their 2FA results rather than issuing a cookie after a password check itself. The tests confirm account email through framework-generated tokens; no production confirmation email transport exists yet.

## Policies and current actor

| Policy | Allowed stored roles |
|---|---|
| `Atlas.Member` | Member, GlobalGlobalModerator, Administrator |
| `Atlas.Moderator` | GlobalModerator, Administrator |
| `Atlas.Administrator` | Administrator |

All policies require an authenticated principal and a persisted, confirmed, unlocked account with an active Participant. `AtlasAccounts.ResolveParticipantAsync` maps the framework user-ID claim to this verified ID. Missing, malformed, empty, unknown, anonymous, locked, unconfirmed, or inactive identities cannot resolve.

The policy handler reads current database role memberships instead of trusting role claims copied into an older cookie. Removing a role takes effect on the next policy check. Cookie validation retains the framework security-stamp validator and rechecks profile eligibility on every cookie request; password-change stamp invalidation retains the framework's validation interval. Role possession does not grant ownership of someone else's profile/node or community moderator status.

`IdentityModeratorAuthorization` implements the existing `IModeratorAuthorization` port using stored global GlobalModerator/Administrator membership and active-account eligibility. The host must derive its actor ID from authenticated identity before invoking domain services. This adapter does not turn arbitrary caller-supplied GUIDs into authenticated actors. Moderation continues to own reports, decisions, hide/restore behavior, and audit history.

## HTTP host integration contract

A future host registers `builder.Services.AddAtlasIdentity(connectionString)` and enables authentication/authorization middleware. Protected endpoints use `AtlasPolicies` and resolve the actor via `AtlasAccounts`; request bodies must not choose the actor ID. Registration uses the account coordinator rather than directly calling `UserManager.CreateAsync`, including if using framework API endpoints: an unadapted stock registration endpoint would bypass profile creation and violate the foreign key.

The host owns actual endpoints/UI, confirmation and reset email delivery, rate limits, cookie/CSRF behavior, HTTPS, and persistent/shared Data Protection keys. The cookie is HttpOnly and Secure. Framework managers/token providers are infrastructure readiness, not evidence that a complete account-management HTTP API is deployed. Before exposing sign-in, configure trusted administrator provisioning through an authenticated/admin or controlled operator workflow. Public registration never assigns privileged roles; the first user is not automatically an administrator.

The Console now uses `ConsoleAccountCommands` for registration/sign-in and `ConsoleIdentitySession` for the authenticated actor. Participant switching and direct profile-only creation have been removed from its interactive menus. The session checks the Member policy and security stamp using fresh SQL state before every main menu; sign-out discards it. Profile browsing never changes the actor. Global moderation uses the Identity authorization port instead of configured GUIDs. The console explicitly refuses accounts with 2FA enabled rather than treating password verification as completed authentication.

The local `--confirm-email` operator utility uses framework confirmation tokens until email delivery exists. It is explicitly trusted development tooling, not proof of email ownership. See [console setup](../PTT-94-Identity.md#use-registration-and-sign-in). No new migration is needed for this console follow-up.

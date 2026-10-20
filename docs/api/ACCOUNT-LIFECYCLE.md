# MVP account lifecycle

## Scope

In scope: registration, email confirmation/resend, login/logout, password recovery,
request-body ceiling (64 KiB), and process concurrency ceiling (64). Domain validation
remains separate. Deployment-specific trusted proxies, distributed admission budgets
and shared/durable Data Protection key storage are deferred beyond this MVP work.
Their documentation is retained as future deployment guidance, not this PR's acceptance criteria.

## Why custom routes when Identity has API endpoints?

`MapIdentityApi<TUser>` does provide built-in register/login/recovery/manage endpoints.
Atlas currently uses `AddIdentity`, a custom SignInManager, cookie configuration and
controller endpoints; it does not map that endpoint bundle. Built-in registration
would not automatically call `AtlasAccounts.RegisterAsync` to atomically create the
Participant profile and Member role. MVC's global antiforgery filter also does not
protect minimal endpoints simply because they are in the same application.

This implementation keeps one account API adapter and reuses Identity's
GeneratePasswordResetTokenAsync / ResetPasswordAsync / confirmation methods. No token
cryptography, password hashing or UI views are reimplemented. Adopting the built-in
bundle later would require reconciling registration, CSRF and cookie contracts.

## Recovery requests

All writes require the anonymous CSRF cookie/header pair and share the existing auth
rate policy. No additional per-destination cooldown is implemented in this slice.

| Route | JSON | Result |
|---|---|---|
| POST /api/auth/forgot-password | email | 202 for unknown, unconfirmed, inactive or eligible accounts. Only confirmed active accounts receive a private reset payload. |
| POST /api/auth/reset-password | email, token, newPassword | 204 on success; generic 400 for invalid token, user eligibility or password-policy failure. |
| POST /api/auth/resend-confirmation | email | 202 generically. Only unconfirmed active accounts receive a private confirmation payload. |

Development email delivery writes a GUID-named JSON file containing `purpose`, `userId`
and `token` into the existing ignored API mail directory. Registration and resend use
purpose `confirm-email`; recovery uses `reset-password`. The HTTP response never contains
that token. Forgot/resend return 503 outside Development pending a production sender.
Reset validation can accept already-issued valid tokens independently of the delivery
adapter; production delivery itself is not implemented.

To test manually: register, confirm with a private payload, request forgot-password,
read the reset payload, submit email/token/newPassword, then log in with the new password.
Reuse of a successfully consumed reset token should fail. Confirmation tokens are not
password-reset tokens. The server's Identity token-provider lifetime remains its current
framework default; no custom expiry policy is introduced. Successful reset does not
sign the caller in, clear lockout, reactivate an inactive profile, or instantly revoke
every existing session. Identity changes its security stamp; existing sessions follow
the configured stamp revalidation behavior. Immediate global revocation needs a separate
explicit contract/test. Delivery errors may produce a server error; generic accepted
responses are not a claim of constant timing or guaranteed mail delivery.

## Account deletion: current behavior, not an implemented feature

There is no account-delete or self-service deactivation endpoint/application workflow.
Identity credential deletion and deletion of Atlas contributions are different operations.
Posts/comments reference the Participant record, not the credential record. The mappings
use non-cascading (`NoAction`) Participant relationships for node authors, comment authors,
votes/reactions, community ownership/membership, moderation references and notifications.

Consequently, removing credentials alone is not a contribution deletion/anonymization
workflow. Removing a referenced Participant row is blocked by the database rather than
silently cascading through posts and discussion threads. Neither action transfers a
community's ownership, anonymizes display names, removes profile text or resolves votes.
Public reads can still expose the stored author/profile information. Recovery must not
be used to reactivate a deliberately inactive profile; its active-profile guard prevents that.

Product decision required before exposing closure:

- Recommended MVP direction: close login access, retain an inactive author placeholder
  and preserve posts/comments so discussions and shared graph structure remain usable.
- Decide whether to anonymize display name/bio and remove credential/contact data, how
  to present the placeholder, and what happens to owned communities and existing votes.
- Decide whether closure is reversible deactivation or irreversible deletion. Do not call
  deactivation deletion, or promise complete erasure while retaining identifying content.

Once selected, implement one coordinated application operation with password/recent-auth
verification and CSRF, session invalidation, transaction boundaries and audit behavior.
Test a user who owns nodes, comments, votes, community membership/ownership and an active
session, then verify preserved/removed records and rejected future writes/recovery.
No destructive account/content policy has been guessed or exposed by this PR.

## Validation status

Added a SQL-backed HTTP recovery scenario for private delivery, confirmation/resend,
unknown/unconfirmed requests, invalid and reused reset tokens, and old/new password login.
Added recovery routes to missing/invalid-CSRF boundary tests. Tests require the .NET SDK;
the SQL scenario also requires ATLAS_SQL_TEST_CONNECTION_STRING. They have not been run
in the editing environment. Account deletion tests await the product policy above.

## Manual Identity requests

- [auth.http](auth.http): register, confirm, login, session and logout.
- [identity-recovery.http](identity-recovery.http): reset/resend workflows and consumed-token replay.
- [identity-negative.http](identity-negative.http): CSRF, credentials, token and validation failures.

Run requests individually with cookies enabled. Copy the returned CSRF token after every
login/logout into the file you are using; keep credentials and private mail tokens untracked.
These are manual checks with expected outcomes, not automatically verified test results.

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

## Account deletion: accepted policy and implemented workflow

POST `/api/auth/delete-account` requires a signed-in active account, its password,
and the authenticated CSRF pair. Success (204) removes the Identity credential/contact
record, replaces profile name with `Deleted user`, clears bio, deactivates the retained
Participant, and clears the caller's session cookie. Identity deletion and profile
changes commit together. Existing session cookies on other clients fail persisted
eligibility checks because the user is gone. Recovery cannot restore this account.

Posts/comments, votes/reactions, and relationship references remain. The public profile
endpoint returns a minimal `deleted=true` placeholder; inactive profiles remain excluded
from the participant directory. Discovery caches are invalidated by the Participant
write, so the former display name is not served from a stale cached card. No schema
migration is required. The retained GUID still correlates the former user's contributions;
this is account/profile anonymization, not complete unlinkability or erasure. Authored
content, audit history, backups and already-written diagnostics are not rewritten.

Closure is blocked with 409 if the user owns a community, pending ownership transfer.
This avoids silently stranding a community or destroying its content. Membership and
votes remain historical references. Ownership transfer itself is not implemented here.
Wrong-password closure returns generic 400 without modifying the account. Re-registering
an old email creates a new Participant; it cannot reclaim the retained contributions.

See [identity-closure.http](identity-closure.http) for deliberate manual checks using a
disposable account. Do not use Run All against an account you want to keep.

## Validation status

Added a SQL-backed HTTP recovery scenario for private delivery, confirmation/resend,
unknown/unconfirmed requests, invalid and reused reset tokens, and old/new password login.
Added recovery routes to missing/invalid-CSRF boundary tests. Tests require the .NET SDK;
the SQL scenario also requires ATLAS_SQL_TEST_CONNECTION_STRING. They have not been run
in the editing environment. The SQL scenario also closes an account with a post/comment, checks wrong-password rejection, retained content, the deleted profile and removed credentials. These assertions have not been run here.

## Manual Identity requests

- [auth.http](auth.http): register, confirm, login, session and logout.
- [identity-recovery.http](identity-recovery.http): reset/resend workflows and consumed-token replay.
- [identity-negative.http](identity-negative.http): CSRF, credentials, token and validation failures.

Run requests individually with cookies enabled. Copy the returned CSRF token after every
login/logout into the file you are using; keep credentials and private mail tokens untracked.
These are manual checks with expected outcomes, not automatically verified test results.

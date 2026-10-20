# Cookies, CSRF and accounts

[API start](README.md) · [Workflow blackboard](../blackboards/workflows/Api/ApiWorkflows.excalidraw) · [Request guide](HTTP-GUIDE.md)

## Start here: two different pieces of state

| Piece | Held by | Purpose |
|---|---|---|
| `__Host-Atlas.Session` | Browser cookie jar; HttpOnly | Identify the signed-in account |
| `__Host-Atlas.Csrf` | Browser cookie jar; HttpOnly | One half of the antiforgery pair |
| JSON `token` → `X-CSRF-TOKEN` | Client memory / private HTTP editor variable | The other half, submitted explicitly on writes |

- **Session cookie:** Secure, SameSite=Lax; the encrypted ticket expires after eight hours.
  - Login is non-persistent: this is a browser-session cookie, rather than a “remember me” cookie.
  - Expiration does not slide forward with activity.
  - Identity validates the account; protected policies check persisted roles.
- **CSRF cookie:** Secure, HttpOnly, SameSite=Strict.
  - JavaScript reads the token from the JSON response, not from this cookie.
  - The browser sends the cookie; the client explicitly sends the matching header.
- Do not store the session cookie in localStorage or copy it into request JSON.

## Sign in, use the API, then sign out

Use [auth.http](auth.http), keeping the same client cookie jar throughout.

1. **Prepare anonymously:** `GET /api/auth/csrf`.
   - Retain the returned cookie.
   - Copy JSON `token` into your private `csrfToken` variable.
2. **Sign in:** `POST /api/auth/login`.
   - Send email/password JSON and `X-CSRF-TOKEN`.
   - Success: **204**, with the session cookie set by Identity.
   - Failure: **401**, with a generic sign-in message.
3. **Refresh after login:** `GET /api/auth/csrf`.
   - Replace the header variable with this token: tokens are bound to the current principal.
   - You do not need a fresh token before every write while the principal remains unchanged.
4. **Use the session:** `GET /api/auth/session`, then your workflow requests.
   - Reads generally need no CSRF header; protected reads still need the session.
   - POST/PUT/DELETE requests need the session when protected **and** the CSRF pair.
5. **Sign out:** `POST /api/auth/logout`, with the current session and CSRF pair.
   - Success: **204**; Identity sends deletion cookies for its session.
   - Honor those `Set-Cookie` responses; the client cookie jar removes the session.
6. **Return to anonymous state:** clear your client-side user state and cached token.
   - Fetch `GET /api/auth/csrf` again before another anonymous write or login.
   - Logout does not promise to delete the separate CSRF cookie; refreshing the token is what matters.

If the session expires or becomes invalid:

- A protected request returns **401** rather than redirecting to an HTML login page.
- Refresh the anonymous CSRF token, sign in again, then refresh the authenticated token.
- **403** means an authenticated caller lacks permission; signing in again does not grant ownership or a role.
- **400** with `A valid CSRF token is required.` means the cookie/header token pair is missing, invalid or belongs to a different principal.

Logout clears this client's cookie. It is not a new server-side “revoke every copied cookie on every device” facility.

## What CSRF protection prevents

A malicious website can try to make a visitor's browser submit a request to Atlas.
Browsers can attach cookies automatically, so the existence of a session cookie
alone does not establish that Atlas's own client intended a write.

- Atlas requires a second value that the legitimate client obtains from `/api/auth/csrf`.
- A different origin cannot normally read that JSON response under the browser's same-origin policy.
- The attacker therefore cannot simply supply a valid cookie/header pair on the victim's behalf.
- Login and registration are protected too: a forced login can attach the victim's later actions to an attacker's account.
- SameSite cookies provide another defense; they do not replace token validation.

**Why the policy is everywhere:** it protects unsafe controller requests by default,
including future actions. It does not demand a token for normal GET reads.

- ASP.NET Core's built-in `AutoValidateAntiforgeryToken` is registered globally.
- Explicit controller attributes expose the same policy to readers and CodeQL.
- `AntiforgeryProblemDetailsFilter` only formats rejection as JSON; the framework validates tokens.
- These checks do not replace authentication, authorization, quotas, or protection against XSS.
- The future SPA's origin/CORS/cookie deployment still needs its own review.

## Create and confirm a local development account

Follow **regions 11–12** of the [signup blackboard](../blackboards/workflows/Api/ApiWorkflows.excalidraw).
The [reading guide](../blackboards/workflows/Api/README.md#follow-a-new-user) links the HTTP steps to the server-side account transaction.

Registration is currently a **Development-only** path, pending a production mail provider.

1. Run the API in the `Development` environment. No mail setting or user secret is required.
   - The default directory is `src/hosts/Atlas.Api/development-mail/`, under the API host content root and ignored by Git.
   - The directory is created when the first confirmation payload is written. It is not served over HTTP.
   - Optional: set `Atlas:DevelopmentMailDirectory` to override it. Relative paths resolve against the API content root; absolute paths are used directly.
   - Outside Development, registration returns **503 before creating an account**, even with an override.
2. Fetch an anonymous CSRF token, then `POST /api/auth/register`.
   - Success: **202**; the account is unconfirmed.
3. Open the generated `<guid>.json` file in that directory.
   - This is a development substitute for email delivery, not an HTML email or a message sent to an inbox.
   - It contains only `userId` and `token`; the account/profile are stored in SQL, and the password is not written to the file.
   - Copy `userId` and `token` into private HTTP editor variables.
   - The confirmation token is different from the CSRF token.
4. Send `POST /api/auth/confirm-email` with the confirmation payload **and** the anonymous CSRF pair.
   - Success: **204**. Confirmation does not sign the user in.
5. Follow the sign-in sequence above.

Treat confirmation files as account-bearing secrets: restrict access and delete
them after use. Do not commit passwords, cookies, CSRF tokens or confirmation tokens.
Confirmation tokens are not returned by registration JSON or written to application logs.

### Account commit and local delivery are separate

- Account, Participant profile and Member role link commit together through `AtlasAccounts`.
- The host generates the confirmation token and writes the private file **after** that commit.
- A file-write failure can return **500 even though the unconfirmed account already exists**.
- The Development-only guard runs before creation; the default or overridden directory must still be writable.
- Confirmation resend is available through `/api/auth/resend-confirmation`; production delivery remains separate work.

## Still outside this slice

- Production email delivery. Development password recovery and confirmation resend are implemented; see [Account lifecycle](ACCOUNT-LIFECYCLE.md).
- Shared durable encrypted Data Protection keys for production instances.
- Two-factor enrollment/challenges: **PTT-127**. Accounts requiring a second factor are denied rather than bypassing it.

## Recovery and account closure

See [Account lifecycle](ACCOUNT-LIFECYCLE.md) for implemented recovery routes, private delivery, and password-confirmed closure preserving contributions under Deleted user.

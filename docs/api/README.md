# Atlas JSON API — PTT-98 / PTT-125

[Atlas.Api](../../src/hosts/Atlas.Api) is an ASP.NET Core controller host under
`src/hosts`, beside Console. Controllers map HTTP input to existing application
and domain operations. They do not use EF rows as responses, create views, serve
SPA assets, or implement a second set of domain quotas. Actor IDs come from the
validated Identity session, never from JSON. See [workflow parity](WORKFLOWS.md)
and [HTTP security responsibilities](SECURITY.md).

## Run locally

Use the existing migrated Atlas SQL database and shared user-secrets ID. This host
does not migrate or seed at startup. Use the Console's existing `--seed-demo` flow
for an empty development database if desired.

```powershell
dotnet dev-certs https --trust
dotnet user-secrets set "ATLAS_SQL_CONNECTION_STRING" "<your connection string>" --project src/hosts/Atlas.Api
dotnet run --project src/hosts/Atlas.Api --launch-profile https
```

Use the version-controlled [account requests](auth.http) and
[workflow requests](workflows.http) in a `.http`-capable editor. Use a cookie jar
and copy the CSRF response token into an editor-local variable. The examples use
`csrfToken`, `nodeId` and other placeholders deliberately: do not commit passwords,
session cookies, confirmation tokens or populated private environment files.
Requests are executable examples, not an assertion that they have passed. Automated
evidence lives in [Atlas.Api.Tests](../../tests/Atlas.Api.Tests).

## Browser authentication

A successful password login issues an eight-hour, non-sliding HttpOnly, Secure,
SameSite=Lax `__Host-Atlas.Session` cookie. The server validates the account and
active profile; protected policies check current persisted roles. JavaScript does
not need to store the session in localStorage. The API returns 401/403 instead of
redirecting to an HTML login page.

Before any POST/PUT/DELETE, including login, obtain `/api/auth/csrf`. Keep the
antiforgery cookie and send the returned token in `X-CSRF-TOKEN`. Fetch a fresh
token after login/logout because the token is bound to the authenticated identity.
The CSRF cookie is Secure, HttpOnly and SameSite=Strict. SameSite is defense in
depth; it does not replace token validation. The SPA can read the JSON token.

Registration requires confirmed email. For local development only, configure
`Atlas:DevelopmentMailDirectory` in user secrets to a private directory outside
the checkout; registration writes the confirmation payload there. Submit it to
`/api/auth/confirm-email`. These are account-bearing secrets; restrict access and
delete them after use. Tokens are not returned in JSON or application logs.
Without development delivery configured, registration fails with 503 before
creating an account. Production email delivery/recovery is not silently simulated.
Two-factor enrollment/challenges remain PTT-127; accounts requiring 2FA are denied
rather than bypassing the second factor.

## SPA hosting remains independent

A later deployment may serve static SPA assets from a CDN or the API server.
A same-origin reverse proxy can route `/api` here and `/` to the SPA regardless
of which process stores the files. This is the simplest assumption for these
cookies. Cross-origin credentials need an explicit CORS/origin/SameSite review;
this PR enables no CORS origins and no frontend fallback route. Hosting a JSON API
does not decide React, Blazor, PWA installation, or offline synchronization.

## Verification

```powershell
dotnet test tests/Atlas.Api.Tests
# Set the shared ATLAS_SQL_TEST_CONNECTION_STRING secret for the real SQL journey.
dotnet test Atlas.Rewrite.Tests.slnf --settings tests/parallel.runsettings
```

CI runs API boundary tests and a separate SQL-backed cookie/workflow journey using
an isolated database. SQL skips without a connection are not SQL verification.
No schema changes or new migration are part of this host adapter.

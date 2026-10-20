# Start with the Atlas API

The API is another way to use Atlas’s existing application workflows. It returns
JSON instead of console screens. It contains no frontend pages or SPA files.

**Console and API use the same database** when their `ATLAS_SQL_CONNECTION_STRING`
points to the same server/database. They share accounts, records, quotas, the EF
model and migrations. Only automated tests create separate disposable databases.
Starting the API does not migrate or seed your database.

## 1. Start the server

From the repository root, use your existing migrated development database:

```powershell
dotnet dev-certs https --trust
dotnet user-secrets set "ATLAS_SQL_CONNECTION_STRING" "<your connection string>" --project src/hosts/Atlas.Api
dotnet run --project src/hosts/Atlas.Api --launch-profile https
```

The connection secret is shared with Console. If it is already set, skip that command.
Use Console’s existing `--seed-demo` workflow only if you want to seed an empty database.

## 2. Try a public request

Open **https://localhost:7240/api/nodes?page=1** to read the first Discovery page.
For a connection check that does not query SQL, open
**https://localhost:7240/api/auth/csrf**: it returns JSON containing a token.

Check the server’s “Now listening on” line if your port differs. Use **HTTPS** and
all four digits of **7240**. `/` and `/api` return 404 because no endpoint is mapped
there. An empty `items` array is a successful query with no matching records.

## 3. Try a signed-in request

Use [auth.http](auth.http) in your editor to obtain a CSRF token and sign in. Keep
cookies between requests, then obtain a fresh CSRF token after login. The browser
sends the session cookie; write requests also send `X-CSRF-TOKEN`.

Use the [workflow blackboard](../blackboards/workflows/Api/ApiWorkflows.excalidraw)
to follow session, CSRF, middleware and grouped endpoint cycles.

[workflows.http](workflows.http) covers every non-account controller route. The
[request guide](HTTP-GUIDE.md) explains prerequisites, IDs and optional cycles.
Fill placeholders in private editor settings; do not commit real passwords, cookies
or tokens. See [authentication explained](AUTHENTICATION.md) for the full sequence,
local email confirmation, cookie lifetime and remaining production work.

## Where to read next

| You want to understand… | Start here |
|---|---|
| A visual tour of endpoints, sessions and CSRF | [Blackboard guide](../blackboards/workflows/Api/README.md) |
| How to send each example request | [HTTP request guide](HTTP-GUIDE.md) |
| Every controller method and its HTTP example | [Endpoint index](ENDPOINTS.md) |
| What Program, dependency injection and ApiFactory do | [Code walkthrough](CODE-GUIDE.md) |
| Which console workflows have HTTP routes | [Workflow map](WORKFLOWS.md) |
| Cookies, login and CSRF | [Authentication](AUTHENTICATION.md) |
| Current protections and unfinished deployment/browser work | [Security responsibilities](SECURITY.md) |
| The automated requests and assertions | [API tests](../../tests/Atlas.Api.Tests) |

The future SPA can be hosted alongside the API or separately. A same-origin proxy
can send `/api` here and `/` to the frontend. This PR chooses neither a frontend
framework nor static-file hosting; cross-origin cookies/CORS need a separate review.

## Run the tests

```powershell
dotnet test tests/Atlas.Api.Tests
dotnet test tests/Atlas.Rewrite.Tests.slnf --settings tests/parallel.runsettings
```

Set the shared `ATLAS_SQL_TEST_CONNECTION_STRING` secret to run the HTTP/SQL journey.
Without it, that test skips; a skip is not SQL verification. This host requires no
new migration. The current implementation combines PTT-98 and PTT-125.

## Inspect server output

See [API logging and telemetry](LOGGING.md) for terminal formatting, local file locations, and security coverage gaps.

# PTT-125 review against the current API

[PTT-125](https://thepublicthinktank.atlassian.net/browse/PTT-125) originally documents
HTTP/API/browser responsibilities under PTT-97. The architecture now exists, so this
branch implements concrete API controls while retaining explicit frontend/deployment
follow-ups. This is a control/coverage review, not a completed vulnerability scan or
claim of public production readiness.

## Current implementation and validation

| Task concern | Implemented boundary | Evidence / limits |
|---|---|---|
| Cookie configuration/lifecycle | IdentityServices plus ApiServices: Secure, HttpOnly, host-only `__Host-Atlas.Session`, SameSite=Lax, eight-hour ticket, nonpersistent login, no sliding expiry; active profile checked on cookie requests | Existing HTTP/SQL sign-in/logout and closure scenarios. Identity supplies the mechanisms; Atlas configures and integrates their behavior. Password reset stamp revalidation is not instant global revocation. |
| HTTPS/transport | Program uses HTTPS redirection and HSTS outside Development; HTTPS launch profile; Secure cookies | New HttpAbuseTests verifies configured 307 redirect and Production HSTS. Real certificate/TLS termination and trusted proxies remain deployment work. If no HTTPS port can be determined, redirection alone cannot establish TLS; use the HTTPS launch profile. |
| CORS | No policy or cross-origin grant is configured | New foreign-origin/preflight checks assert absence of allow-origin/allow-credentials. This preserves browser same-origin restrictions; it does not block requests from scripts, CLI clients or every cross-site form. CSRF remains necessary. Select exact frontend origins later; never wildcard credentialed CORS. |
| CSRF | Unsafe MVC requests require Secure Strict-SameSite cookie plus `X-CSRF-TOKEN`; login/register/recovery included | Existing boundary/SQL tests, plus new cross-cookie-jar test. Refresh on identity transitions, not every write. |
| Size limits | Kestrel caps body at 65,536 bytes; DTO and domain limits remain | New real Kestrel test reads known-length and chunked 65,537-byte probes, then accepts 65,536 bytes. ApiErrors preserves an allowlisted 413 rather than misclassifying this rejection as 500. TestServer alone does not verify Kestrel. Uploads are not implemented. |
| Security headers | Cache-Control no-cache/no-store, Pragma no-cache, nosniff, no-referrer; HSTS outside Development | New header assertions on JSON responses. Redirects occur before the application header middleware; they are not claimed to carry all these headers. CSP/frame policy for rendered UI belongs with frontend hosting. |
| Endpoint authorization | Explicit Member/GlobalModerator policies plus application/domain ownership checks and verified actor | Existing metadata, anonymous and SQL non-owner tests; new representative anonymous-route matrix includes closure and moderator queue; SQL scenario now checks a Member cannot inspect the moderator queue. Identity authentication alone does not authorize ownership changes. |
| API error exposure | Safe ProblemDetails categories, generic login/recovery errors, bounded quota messages | Existing malformed JSON/CSRF and ownership tests; new oversized response check. Operational exception logs may still include framework diagnostics; no claim of blanket sanitization of every logger. |
| Browser/XSS rendering | JSON strings are returned without an HTML rendering pipeline | Frontend must contextual-escape/sanitize Markdown and URLs, choose CSP and embedding rules, and receive browser tests. Server input constraints still apply now; JSON encoding is not a future HTML sanitizer. |
| Endpoint abuse tests | Short, bounded probes in HttpAbuseTests; existing account burst tests and SQL owner/CSRF cases | Tests cover selected attack shapes, not exhaustive fuzzing or a production load test. Test-only probe controllers are added via an application part only in test factories. |
| API rate limiting | Named `auth` policy: 20/minute/source-IP fixed window, no queue. Global limiter: 64 concurrent requests/process, no queue | AccountsController selects `auth`; Program's limiter runs after routing. Existing tests cover accepted/rejected bursts. New controlled 64-handler test checks rejection at saturation and admission after release. Other routes have concurrency plus applicable domain quotas, not a universal requests/minute cap. |
| Safe observability | Host-attributed allowlisted events; selected early HTTP rejection observations; durable SQL business audit/outbox | Existing telemetry validation and new rejection metadata tests. Production exporters/retention/aggregation remain PTT-136; logs/confirmation payloads must not be committed or served. |

## What the abuse tests are trying to prevent

- Anonymous writes and moderator reads must stop before database/application work.
- A token obtained in one browser cookie jar must not validate with another jar's cookie.
- Foreign origins must not silently gain credentialed cross-origin access.
- A burst of failed login/form attempts must still consume the transport budget.
- 64 admitted, deliberately held requests must cause the next request to get 429,
  then permit normal work once those handlers release. No sleep-based saturation guess.
- Oversized bytes must get 413 even when Content-Length is absent; a valid boundary-sized
  body must remain usable. The probe discards bytes instead of touching SQL or domain state.

Tests do not attempt denial of service against external systems. Kestrel probes bind only
to an ephemeral loopback TLS port and trust only a generated test certificate. The hold
endpoint exists only in test factories; no production debug endpoint is added.

## MVP decisions and remaining work

Keep current numeric limits until observed workflow/load data justifies tuning. Account
reads and writes currently share one 20/minute budget: frequent token fetches and shared
NAT clients can consume it. More granular budgets are a documented proposal, not enabled
by this review. Rate-limit rejection is currently status-only 429; domain quotas provide
Retry-After. A future consistent ProblemDetails/rate response contract should distinguish
fixed-window wait from concurrency saturation, where no exact retry time is known.

Out of this MVP slice: deployment-specific proxy/host configuration, shared/durable Data
Protection keys, distributed budgets and edge controls. Existing follow-ups are PTT-127
(2FA), PTT-136 (production observability), PTT-137 (distributed budgets) and PTT-142
(edge/identity abuse policy). Frontend CORS/CSP/Markdown/XSS review belongs with that layer.
These remain visible responsibilities; they are not duplicated in the business domains.

## Validation status

The editing environment has no .NET SDK or SQL service, so the new tests have not been
compiled or run here. Source/diff checks alone are not a passing security test result.
Run the API tests locally/CI, including the loopback Kestrel test, before accepting coverage:

```powershell
dotnet test tests/Atlas.Api.Tests/Atlas.Api.Tests.csproj
```

SQL-backed tests additionally require the existing ATLAS_SQL_TEST_CONNECTION_STRING.
The browser effects of CORS/SameSite/XSS still require real-browser tests when the UI exists.

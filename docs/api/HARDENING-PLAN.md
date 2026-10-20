# API security decisions before the frontend

Current MVP scope and recovery implementation supersede the planning notes below: see
[Account lifecycle](ACCOUNT-LIFECYCLE.md). Proxy/shared-key/distributed deployment work is
out of scope for this MVP change. Recovery routes are now implemented.

This is an implementation review and proposal, not a completed production security audit.
Values labelled proposed are starting points for local/load testing, not tuned production guarantees.

## HTTP limits: what they catch

The current host enforces 20 account-route requests/minute per source IP, 64 concurrent
requests per process, and a 65,536-byte Kestrel body ceiling. The account budget includes
CSRF and session reads. Keep the body and concurrency ceilings initially; separate
routine session/CSRF reads from sensitive account writes before normal frontend use.

| Operation family | Proposed initial limit | Why / tradeoff |
|---|---|---|
| CSRF + session reads | 60/minute/source IP | Allows reloads/tabs; avoids consuming login/registration budget. A frontend should still reuse CSRF tokens until identity changes. |
| Login | 10/minute/source IP | Slows repeated attempts; complements Identity's five failed attempts / 15-minute account lockout. Shared networks can affect unrelated people. |
| Registration | 3/minute/source IP | Bounds account creation/email cost; IP alone cannot prevent account farming. |
| Reset request / confirmation resend | 3/minute/source IP plus an account/destination cooldown | Prevents email flooding. Account/destination keys stay server-side and never become telemetry dimensions. Must retain generic public responses. |
| Confirm/reset submission | 10/minute/source IP | Bounds token validation work, separate from mail-sending budgets. |
| Public reads / authenticated writes | Measure first; use separate budgets | Expensive reads need protection too. Existing participant/domain quotas remain authoritative for supported business actions. |

Do not apply these proposed values mechanically to every endpoint. Load-test representative
workflows, use a controllable clock in limiter tests, and decide how to handle shared NAT,
IPv6 address rotation, and distributed clients. Configure options with validation when
introducing operation-family budgets; do not bury deployment tuning in controllers.

### Oversized requests

Examples worth rejecting before expensive application/database work:

- A JSON body containing 100 KiB of Markdown sent to a route capped at 64 KiB.
- Thousands of GUIDs in a node request's `requestedTypeIds`, even if the total body is under 64 KiB.
- Deeply nested JSON intended to exhaust parsing resources. Explicitly review the serializer depth separately from byte size.
- A huge query string or excessive page/filter values. Query strings are outside the body limit.
- An image encoded as base64 in JSON. Atlas currently uses image URLs; binary uploads need a separate future endpoint and budget.

Body limits count encoded bytes, not .NET string characters; non-ASCII text may use several
UTF-8 bytes per character. A 64 KiB JSON ceiling is not permission for every field to contain
64 KiB. Align per-field and collection validation with domain limits, and distinguish
malformed/invalid input (400) from too-large bodies (413).

Kestrel enforces the byte ceiling while reading, including bodies without Content-Length.
TestServer is not Kestrel: an in-memory integration test alone cannot verify that server
limit. Add a real local Kestrel test for known-length and streamed/chunked oversized
requests, plus a just-under-limit valid request. Do not claim oversized-body coverage
from a test that only exercises DTO validation. Test global concurrency using controlled
blocked handlers, release them, and verify recovery; avoid a fragile sleep-based test.

## Reverse proxy and distributed limits are separate concepts

A hosting platform may put a reverse proxy/load balancer in front of Atlas. The browser
connects to that gateway; the gateway forwards the request to one of the API instances.
It can handle TLS certificates, route traffic, and enforce edge limits. A direct local
Kestrel connection does not need a proxy.

Without forwarded headers, Atlas may see the proxy's IP and the backend connection's
HTTP scheme, rather than the original client IP/HTTPS scheme. Known proxy configuration
means accepting that information only from the gateway addresses/networks actually used
by the deployment. Process forwarded headers before HTTPS redirection, authentication
and source-IP limiting. Never accept arbitrary client-supplied X-Forwarded-For values.
Select exact known proxies, trusted hop count, allowed hosts and network access restrictions
when the hosting topology is chosen; there is no safe universal proxy address to add now.

Distributed limiting means sharing an admission budget across API instances. If each
of three instances independently allows 20/minute, routing across them can admit roughly
60/minute. An atomic shared store or a gateway-wide limiter can enforce one shared budget.
A proxy is not automatically a distributed limiter; its configuration and product decide
that. Avoid non-atomic read/increment logic in a shared cache. PTT-137/PTT-142 own the
broader distributed and edge policies.

## Identity recovery: framework capabilities versus exposed workflow

`IdentityServices.AddAtlasIdentity` already calls `AddDefaultTokenProviders()` and
registers Data Protection. Identity provides `GeneratePasswordResetTokenAsync` and
`ResetPasswordAsync`; we should use those rather than build token cryptography.

The custom AccountsController now exposes forgot-password, reset-password and confirmation
resend in addition to registration/confirmation/login/logout/session. See
[Account lifecycle](ACCOUNT-LIFECYCLE.md) for the implemented request contracts, development
delivery, security-stamp behavior and test status. It reuses UserManager rather than
implementing token cryptography. No views are required. Production mail delivery is deferred.

## Data Protection keys versus HTTPS

HTTPS/TLS protects bytes while they travel between network peers. Data Protection protects
server-issued state such as cookies, antiforgery and Identity confirmation/reset tokens.
The server must later recognize/decrypt that state, potentially on a different instance.

If instance A issues a cookie but instance B has unrelated keys, B cannot read it. If a
container loses its keys when replaced, existing cookies/tokens can stop working. Even a
single production instance needs durable keys across replacement; multiple replicas need
a shared key ring and consistent application discriminator. Protect key storage with
restricted access and encryption at rest; retain old keys as needed for valid issued state.

Configure the durable store and protection mechanism once the deployment platform is
chosen. Do not commit keys, use the development mail folder for them, or treat a shared
key ring as a substitute for TLS. Local development can retain framework defaults.

## Telemetry: emit origin once, observe HTTP boundaries separately

Events now capture a bounded Console/Api host value at creation. EventJson preserves the
existing event names and emits `atlas-console` or `atlas-api` in `service`. Persisted audit
payloads therefore retain the origin even if another host later delivers them. Old audit
records are not rewritten. The existing `environment` field remains `development`; making
that deployment-aware is a separate metadata task before production.

An outer HTTP telemetry scope now starts before exception handling/rate limiting/authentication/
authorization. The inner participant adapter enriches this same operation after persisted
eligibility is checked. Selected final 400/401/403/413/429 statuses emit
`http.request_rejected`, without actor/target IDs, raw URLs, headers, bodies or IPs.
Reasons are InvalidInput (400/413), NotAuthorized (401/403), or QuotaExceeded (429).
These are HTTP observations, not durable business audit records or proof of an attack.
Domain denial events may coexist and correlate with the HTTP outcome.

This does not cover requests rejected by Kestrel/the proxy before the middleware pipeline,
client aborts/unhandled exceptions, or every possible status. It adds no warning per rejected
request to the terminal. Diagnostics remain best effort and Development-only; production
export/aggregation/sampling and host lifecycle instrumentation remain PTT-136 follow-up.

## Validation in this change

Added tests cover captured/restored host attribution, bounded host validation, early rejection metadata, the 21st
account request returning 429, cache headers, and Production registration returning 503
even with a mail-path override. The latter tests avoid SQL by taking short-circuit paths.
The .NET SDK was unavailable in the editing environment, so these tests have not been run.
Real Kestrel body-limit and controlled concurrency tests remain outstanding.

## References

- [Proxy and load-balancer configuration](https://learn.microsoft.com/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-10.0)
- [Web farms and shared Data Protection](https://learn.microsoft.com/aspnet/core/host-and-deploy/web-farm)
- [Identity reset token generation](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.identity.usermanager-1.generatepasswordresettokenasync?view=aspnetcore-10.0)
- [Identity password reset](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.identity.usermanager-1.resetpasswordasync?view=aspnetcore-10.0)

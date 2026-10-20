# HTTP security responsibilities

This records PTT-125 alongside the concrete PTT-98 host. It is a local-development
API baseline, not a statement that public production deployment is ready.

| Concern | Current implementation / owner | Remaining work |
|---|---|---|
| Identity | Existing ASP.NET Core Identity; active/confirmed participant resolution; current role policies | Production email provider, reset/recovery flow; PTT-127 2FA |
| Cookies / CSRF | Secure HttpOnly host-only cookie; fixed lifetime; CSRF on every write including login/logout; JSON denials | Shared durable encrypted Data Protection keys and operational key lifecycle before multi-instance production |
| Authorization | Protected mutations require Member; moderation decisions require GlobalModerator; domain authorship checks remain authoritative | Expand HTTP adversarial matrix as endpoints evolve |
| Input / SQL | API DTOs, GUID routes, domain validation, 64 KiB Kestrel body ceiling; existing parameterized EF repositories | Review per-operation payload budgets with actual clients |
| Errors | ProblemDetails for rejection categories; no raw SQL, stack traces or exception messages except allowlisted quota messages; quota Retry-After | More granular domain error codes; delivery failure/retry semantics |
| Abuse | Existing node/vote/community/report quotas; per-process concurrency ceiling; source-partitioned account-route limiter | PTT-137 distributed operation-family budgets, trusted proxies and global load limits; PTT-142 edge/Sybil policy |
| Origin / CORS | No cross-origin credential policy enabled | Select actual frontend origins; never wildcard credentialed CORS |
| Transport | HTTPS redirection; HSTS outside Development; Secure cookies | TLS termination, known proxy/origin restrictions, production host validation; avoid redirect loops behind proxies |
| Browser content | Return JSON and original content strings; no HTML rendering or remote URL fetching | Frontend owns contextual escaping, Markdown sanitization, safe URL rendering, CSP, and iframe/media policy |
| Privacy / diagnostics | Bounded Telemetry operation scope; development JSONL plus leased audit delivery; suppress raw request URL and EF command information logs | PTT-136 cloud exporters/access/retention/alerts; avoid enabling body/header logging |
| Cancellation / reads | Comment paging propagates request cancellation and bounds SQL sibling reads; Discovery uses existing paged query | Community/profile/children catalogs currently page in memory after existing repository reads; SQL-bounded ports remain follow-up |
| External delivery | Existing in-app notifications; no real email/push delivery claimed | Provider selection, retries, delivery semantics and frontend grouping remain separate |
| Frontend assets | None | Choose SPA hosting independently |

Do not expose developer SQL/content diagnostics as public endpoints. Do not accept
actor IDs, roles, trusted-import flags, arbitrary SQL, or arbitrary polymorphic CLR
types from request JSON. `.http` samples must never contain real credentials.

Sources: [ASP.NET Core antiforgery](https://learn.microsoft.com/aspnet/core/security/anti-request-forgery?view=aspnetcore-10.0),
[controller APIs](https://learn.microsoft.com/aspnet/core/web-api/?view=aspnetcore-10.0).

## Review the current HTTP limits

| Limit | Current value and scope | Practical boundary |
|---|---|---|
| Account routes | 20 requests per one-minute fixed window per connection source IP; no queue | Includes CSRF/session reads, register, confirm, login and logout. Shared NAT clients share a budget. Multiple API instances each have their own budget. |
| Global concurrency | 64 active requests per process; no queue | Bounds simultaneous work, not requests per minute or total ingress traffic. |
| Request body | 64 KiB at Kestrel | Does not establish per-field collection/depth budgets or protect a different upstream server by itself. |
| Domain quotas | Existing node/vote/community/report policies | Separate from HTTP limits; inspect each operation's existing policy. |

Middleware saturation returns 429; it currently has no structured ProblemDetails
body or Retry-After contract. Domain quota exceptions already include Retry-After.
Do not invent a retry time for concurrency saturation, whose next slot is unknown.

Next focused checks for this PR are HTTP tests for fixed-window saturation,
concurrency rejection, oversized bodies, and registration outside Development.
Tests should exercise the real pipeline without depending on a developer SQL server.
These are recommended additions, not claims of existing coverage.

Before public hosting, prioritize trusted proxy configuration and allowed hosts,
shared encrypted Data Protection keys, production email/recovery, distributed
abuse budgets (PTT-137), and edge/global load controls (PTT-142). Choose allowed
origins when the frontend topology is known. Do not trust arbitrary forwarded-IP
headers to bypass the current source-IP budget.

See [logging and telemetry](LOGGING.md) for inspection steps and known HTTP
rejection/host-attribution gaps.

See [the hardening plan](HARDENING-PLAN.md) for concrete proposals, examples, framework responsibilities, implemented telemetry changes and outstanding tests.

For the current implementation/test matrix and MVP boundaries, see [PTT-125 review](PTT-125-REVIEW.md).

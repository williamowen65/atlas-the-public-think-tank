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

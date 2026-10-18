# Rate limiting and abuse-prevention responsibilities

[Security index](../README.md) · [Negative-path evidence](../negative-paths/README.md) · [Node quota](../validation/NODE-CREATION-QUOTA.md)

PTT-124 records the MVP protections and the requirements for the next API host. This review preserves the agreed creation policy rather than adding arbitrary restrictions to every operation.

## What already slows node spam

The current policy allows **3 node creations per rolling 2 hours per participant**, including roots and children. Bursts within that allowance are allowed. On exhaustion, a typed rejection contains the next available slot and retry duration. Posting becomes possible again as entries expire: this is temporary pacing, not an account ban or lifetime posting cap.

The SQL adapter atomically checks and consumes the allowance with node creation. Failed transactions consume nothing; archive/delete cannot refund a recent creation. Trusted import/seed work is an explicit server-side capability, never a client option. The limit is account-based and does not stop someone using several accounts. The allowance shown before submission is advisory; the atomic save is authoritative.

The product policy remains provisional and configurable in code. This review does not tighten it or add minimum spacing. See [quota implementation and SQL tests](../validation/NODE-CREATION-QUOTA.md).

## Four different protections

| Control | Purpose |
|---|---|
| Creation quota / rate policy | Limits successful creation over time, regardless of host |
| Request throttling and concurrency limits | Bound how often/how many requests reach expensive work, including failed requests |
| Value/resource limits | Bound one payload, object, page or computation; these do not control frequency |
| Moderation and abuse handling | Decide what behavior/content is allowed and how people can report, review and recover |

None substitutes for the others. Voting uniqueness limits current influence but not update frequency. Notification deduplication limits duplicate rows for one occurrence but not many distinct spam comments. Metrics make abuse observable; they do not stop work before it consumes resources.

## Abuse inventory and owning layers

| Operation / abuse case | Current protection and responsible next step |
|---|---|
| Node creation flood | Graph policy + Application SQL quota ledger enforce pacing atomically; API must retain this backend check and add request throttling before expensive work. |
| Repeated title/description edits | Authorship and value validation exist. Creation quota does not apply. API must budget mutation rate/concurrency; future backend shared policy only if a product rule is agreed. |
| Comments/replies and deep threads | Actor/target availability, ownership and body-size validation exist. No comment throughput quota or universal depth/read-work budget. API throttling plus bounded thread traversal/pagination require implementation/testing. |
| Voting/reaction churn | Eligibility, one current vote per actor/target, target availability and transactional association/vote exist. API should limit repeated mutation traffic, not treat uniqueness as a rate limiter. |
| Registration/sign-in, token/reset/email requests | Identity checks credentials, confirmation, active profile and account lockout (5 failed attempts / 15 minutes). API must also budget IP/client traffic, registration and outbound email work. Account lockout alone can be abused to deny someone else access. |
| Search/filter/deep paging/ancestry | Discovery validates inputs and SQL performs page queries; bounded breadcrumb segments exist. API needs query cost/timeouts, request/concurrency budgets and bounded pagination; returning few items does not imply cheap computation. |
| Community membership/association churn | Ownership, active references and uniqueness protect state. API should budget repeated writes and creation; no new community quota is implied here. |
| Reporting/moderation queue flooding | Actor/role checks and lifecycle rules exist. API should budget reports and privileged actions without preventing legitimate reporting; define duplicate/batch policies explicitly. |
| Notification amplification | Recipient ownership, preferences and occurrence deduplication exist. Bound upstream producers and future outbound queues/retries. One unique event per spam comment can still produce many notifications. |
| Multiple accounts and distributed clients | No mature anti-Sybil/reputation system. Do not claim the participant quota or IP limits solve coordinated abuse. Evaluate additional policy from observed use. |
| Rich text/links/images and malicious content | Current validation bounds values, not every unsafe URL or rendered document. The eventual browser host must safely render untrusted text and validate allowed URL schemes; no server-side fetch of arbitrary URLs should be introduced without SSRF protections. |
| Audit/diagnostic flooding and history growth | Allowlisted bounded event fields, local rotation and durable audit outbox exist. Deployment must bound queue/storage growth, restrict access and define retention/cleanup. Do not drop required audit silently to hide pressure. |
| Operator/import bypass | Only trusted server-side callers may use ExecuteTrustedCreation. API authorization and audited operator workflows must gate it; ordinary administrators' posts are not automatically exempt. |

## Signals and response

Quota exhaustion now emits `outcome=denied` and `reason_code=quota_exceeded` through the existing operation failure path. Repeated handling of the same exception in one operation is deduplicated. This is a diagnostic/metric signal, not a committed node-creation audit success. Event kind reflects the responsible boundary (often storage); aggregate denials by reason rather than counting only successful `node.changed` events.

No submitted title/body or raw exception message is added to telemetry. Production dashboards/alerts are not configured by this PR. A useful future dashboard separates creation success, quota denial, authentication denial, request throttling, conflicts, storage failure, latency and pending delivery pressure. Avoid participant IDs as unbounded metric labels; identity-bearing diagnostics remain access-controlled under the existing privacy policy.

Human moderation remains the MVP content safety net. Automated third-party/AI content screening is **post-MVP**, subject to funding and explicit policy. No provider, API key, billable dependency or automated blocking is introduced. Any future screening must address consent/privacy, false positives, appeals, outage behavior and human review; it is not a replacement for quotas or authorization.

## API acceptance requirements

These are requirements for implementation when the API exists, not protections already shipped:

1. Derive participant/roles from verified identity. Treat IP/client information as host context and trust forwarded headers only from configured proxies.
2. Define configurable budgets by operation family: authentication/registration, creation, other mutations, reports, reads/expensive queries and outbound work. Combine authenticated-user limits with appropriate anonymous/IP/global concurrency limits. Do not choose production thresholds without workload measurements.
3. Reject throttled requests before database/content/provider work. Keep backend creation accounting authoritative across processes and restarts. A per-process HTTP limiter alone is not a distributed account quota.
4. Return a stable machine-readable rejection with HTTP 429 and appropriate retry guidance for rate/quota exhaustion. Map NodeCreationQuotaExceededException.Allowance without exposing internals; validation, permission denial and dependency outages remain different outcomes.
5. Bound request body sizes, cancellation/deadlines, deep paging, query execution, thread traversal and queued/outbound work. Preserve ordinary recovery and moderator/operator access under pressure.
6. Test boundary minus/at/plus cases, exact expiry, burst/recovery, distinct participants, anonymous traffic, simultaneous last-slot calls, proxy spoofing, multi-instance behavior, failures that should not consume product quota, and no writes/notifications after rejection.
7. Check that denied/throttled traffic is observable without content/secrets in logs and without high-cardinality metric labels. Load-test the controls; do not infer capacity from unit tests.

The future host/browser security review remains [PTT-125](https://thepublicthinktank.atlassian.net/browse/PTT-125). Follow-up implementation tasks are listed below. This is the final planned pre-API baseline slice, not a guarantee that endpoint or deployment security is complete.

## Tracked follow-ups

- [PTT-137: API request throttling and abuse budgets](https://thepublicthinktank.atlassian.net/browse/PTT-137) — implement with endpoints; coordinate with PTT-98 and PTT-125.
- [PTT-138: Bound comment traversal and expensive collection reads](https://thepublicthinktank.atlassian.net/browse/PTT-138) — cover deep/wide threads and backend work limits before broad API exposure.
- [PTT-139: Quota ledger retention and account-deletion handling](https://thepublicthinktank.atlassian.net/browse/PTT-139) — define policy before adding cleanup.

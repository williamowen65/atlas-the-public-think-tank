# Rate limiting and abuse-prevention responsibilities

[Security index](../README.md) · [Negative-path evidence](../negative-paths/README.md) · [Node quota](../validation/NODE-CREATION-QUOTA.md)

PTT-124 records the MVP protections and the requirements for the next API host. This review preserves the agreed node-creation policy and adds a provisional Voting-owned action allowance a Communities-owned creation allowance, and a Moderation-owned report submission allowance. It also implements URL-form validation for content blocks and iterative comment-thread traversal.

## What already slows node spam

The current policy allows **3 node creations per rolling 2 hours per participant**, including roots and children. Bursts within that allowance are allowed. On exhaustion, a typed rejection contains the next available slot and retry duration. Posting becomes possible again as entries expire: this is temporary pacing, not an account ban or lifetime posting cap.

The SQL adapter atomically checks and consumes the allowance with node creation. Failed transactions consume nothing; archive/delete cannot refund a recent creation. Trusted import/seed work is an explicit server-side capability, never a client option. The limit is account-based and does not stop someone using several accounts. The allowance shown before submission is advisory; the atomic save is authoritative.

The product policy remains provisional and configurable in code. This review does not tighten it or add minimum spacing. See [quota implementation and SQL tests](../validation/NODE-CREATION-QUOTA.md).

## Legitimate use versus overload

A deep discussion, frequent drafting, or several title edits is not inherently abusive. The inventory below identifies where repeated or unusually expensive work could overwhelm the system; it does not define those ordinary behaviors as violations.

The broad “thousands of hits in seconds from one source” concern belongs at the API/edge boundary: reject excessive traffic before invoking backend work, using verified-user and appropriate IP/client/global concurrency budgets. Individual limits can complement that broad budget for unusually costly operations. No Console-only catch-all counter is introduced here.

## MVP protections implemented in this PR

| Protection | What it preserves / what it does not promise |
|---|---|
| Shared Content URL validation | Link previews require bounded HTTP(S) URLs without credentials, raw controls or backslashes. Image/video blocks accept those URLs or simple relative asset paths; executable/data/file schemes and protocol-relative hosts are rejected. Existing valid links and simple media paths remain supported. This is syntax/scheme validation, not malware, reputation or destination screening. |
| Iterative comment-thread traversal | Keeps valid deep discussions, stable depth-first order and existing reply behavior without growing the C# call stack. Detects duplicate/unreachable parent structure rather than silently omitting corrupt replies. No depth cap or new posting restriction. It still materializes a thread; paging/cancellation/work budgets remain PTT-138. |
| Community creation quota | One committed creation per participant per rolling 24 hours; edits/archive/restore remain allowed, and archiving does not refund creation. Separate from API request throttling and membership/association churn. |
| Negative-path regressions | Unsafe URL creation/edit/reconstitution and malformed thread structure fail explicitly; rejected edits preserve payload/timestamps. A 20,000-comment deep valid thread remains readable. |

The URL check performs no DNS lookup or network fetch and does not block private/local destinations as an SSRF defense. Any future preview-fetch service needs separate destination/network protections. URLs embedded inside Markdown remain untrusted content for a future renderer to sanitize; this block-field validator does not parse or secure Markdown/HTML. A syntactically valid HTTPS link can still point to a malicious site.

New writes and reconstitution share the URL rules. Existing media values with unsafe schemes, credentials, encoded/complex relative syntax or controls must be corrected before they can load. The URL validation itself requires no schema migration, automatic rewrite or database scan. The voting, community and report quota ledgers do require a developer-generated migration. Simple existing paths such as `media/image-1` are preserved.

## Voting action allowance

Casts, changes and successful undo now share a participant-level rolling allowance across node ratings and reaction votes. The provisional default is 60 actions per 10 minutes. Undo consumes a new slot without refunding prior work; failed transactions and absent-vote undo consume none. See [policy rationale, migration commands and tests](VOTING-ACTION-QUOTA.md). This requires a developer-generated migration before migration-dependent CI passes.

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
| Comments/replies and deep threads | Actor/target availability, ownership and body-size validation exist. Iterative traversal now avoids recursion and rejects malformed structure, while preserving deep threads. No comment throughput quota or overall read-work budget; paging and API throttling remain follow-ups. |
| Voting/reaction churn | Voting now owns a shared rolling action policy; SQL enforces durable cast/change/undo accounting under participant-scoped coordination. One-current-vote uniqueness remains separate. API must still throttle excessive requests/failed attempts before backend work. See [voting action allowance](VOTING-ACTION-QUOTA.md). |
| Registration/sign-in, token/reset/email requests | Identity checks credentials, confirmation, active profile and account lockout (5 failed attempts / 15 minutes). API must also budget IP/client traffic, registration and outbound email work. Account lockout alone can be abused to deny someone else access. |
| Search/filter/deep paging/ancestry | Discovery validates inputs and SQL performs page queries; bounded breadcrumb segments exist. API needs query cost/timeouts, request/concurrency budgets and bounded pagination; returning few items does not imply cheap computation. |
| Community creation flood | Communities policy + Application SQL ledger enforce **one creation per participant per rolling 24 hours**. Server accounting time and participant-scoped locking prevent backdating and concurrent slot reuse. No lifetime ownership cap or karma gate. See [community creation budget](#community-creation-budget). |
| Community membership/association churn | Ownership, active references and uniqueness protect state. Joining/leaving and linking/unlinking are **not covered by the community creation quota** and have no new throughput quota in this PR. Future API budgets must bound repeated requests separately. |
| Report submission / queue flooding | Moderation policy + SQL ledger allow **three successful reports per participant per rolling hour**; the fourth waits for an expiring slot. A support-staff message provides escalation guidance for additional legitimate concerns. See [report submission budget](#report-submission-budget). Future API throttling must also bound failed/invalid requests. |
| Moderator decisions and review workflows | Reporter submission quota does not consume slots for or block decisions, restoration or review requests. Existing role/ownership/lifecycle checks remain; future API request budgets are separate. |
| Notification amplification | Recipient ownership, preferences and occurrence deduplication exist. Bound upstream producers and future outbound queues/retries. One unique event per spam comment can still produce many notifications. |
| Multiple accounts and distributed clients | No mature anti-Sybil/reputation system. Do not claim the participant quota or IP limits solve coordinated abuse. Evaluate additional policy from observed use. |
| Rich text/links/images and malicious content | Content now rejects unsafe URL forms in link-preview/image/video fields. This does not sanitize Markdown or prove destination safety. The eventual browser host must safely render untrusted text; no server-side fetch of arbitrary URLs should be introduced without SSRF protections. |
| Audit/diagnostic flooding and history growth | Allowlisted bounded event fields, local rotation and durable audit outbox exist. Deployment must bound queue/storage growth, restrict access and define retention/cleanup. Do not drop required audit silently to hide pressure. |
| Operator/import bypass | Only trusted server-side callers may use ExecuteTrustedCreation. API authorization and audited operator workflows must gate it; ordinary administrators' posts are not automatically exempt. |

## Signals and response

Quota exhaustion now emits `outcome=denied` and `reason_code=quota_exceeded` through the existing operation failure path. Repeated handling of the same exception in one operation is deduplicated. This is a diagnostic/metric signal, not a committed node/community creation audit success. Event kind reflects the responsible boundary (often storage); aggregate denials by reason rather than counting only successful `node.changed` events.

No submitted title/body or raw exception message is added to telemetry. Production dashboards/alerts are not configured by this PR. A useful future dashboard separates creation success, quota denial, authentication denial, request throttling, conflicts, storage failure, latency and pending delivery pressure. Avoid participant IDs as unbounded metric labels; identity-bearing diagnostics remain access-controlled under the existing privacy policy.

Human moderation remains the MVP content safety net. Automated third-party/AI content screening is **post-MVP**, subject to funding and explicit policy. No provider, API key, billable dependency or automated blocking is introduced. Any future screening must address consent/privacy, false positives, appeals, outage behavior and human review; it is not a replacement for quotas or authorization.

## API acceptance requirements

These are requirements for implementation when the API exists, not protections already shipped:

1. Derive participant/roles from verified identity. Treat IP/client information as host context and trust forwarded headers only from configured proxies.
2. Define configurable budgets by operation family: authentication/registration, creation, other mutations, reports, reads/expensive queries and outbound work. Combine authenticated-user limits with appropriate anonymous/IP/global concurrency limits. Do not choose production thresholds without workload measurements.
3. Reject throttled requests before database/content/provider work. Keep backend creation accounting authoritative across processes and restarts. A per-process HTTP limiter alone is not a distributed account quota.
4. Return a stable machine-readable rejection with HTTP 429 and appropriate retry guidance for rate/quota exhaustion. Map the structured Allowance on NodeCreationQuotaExceededException, VoteActionQuotaExceededException, CommunityCreationQuotaExceededException and ReportSubmissionQuotaExceededException without exposing internals; validation, permission denial and dependency outages remain different outcomes.
5. Bound request body sizes, cancellation/deadlines, deep paging, query execution, thread traversal and queued/outbound work. Preserve ordinary recovery and moderator/operator access under pressure.
6. Test boundary minus/at/plus cases, exact expiry, burst/recovery, distinct participants, anonymous traffic, simultaneous last-slot calls, proxy spoofing, multi-instance behavior, failures that should not consume product quota, and no writes/notifications after rejection.
7. Check that denied/throttled traffic is observable without content/secrets in logs and without high-cardinality metric labels. Load-test the controls; do not infer capacity from unit tests.

The future host/browser security review remains [PTT-125](https://thepublicthinktank.atlassian.net/browse/PTT-125). Follow-up implementation tasks are listed below. This is the final planned pre-API baseline slice, not a guarantee that endpoint or deployment security is complete.

## Tracked follow-ups

- [PTT-137: API request throttling and abuse budgets](https://thepublicthinktank.atlassian.net/browse/PTT-137) — implement with endpoints; coordinate with PTT-98 and PTT-125.
- [PTT-138: Bound comment traversal and expensive collection reads](https://thepublicthinktank.atlassian.net/browse/PTT-138) — cover deep/wide threads and backend work limits before broad API exposure.
- [PTT-139: Quota ledger retention and account-deletion handling](https://thepublicthinktank.atlassian.net/browse/PTT-139) — define policy before adding cleanup.

## Community creation budget

A participant can create **one community per rolling 24 hours**. This is a Communities
domain policy, backed by `SqlCommunityRepository` and a durable participant/time ledger.
It does not reset at midnight. Changes, archiving and restoration do not consume slots,
and archiving/deletion does not refund the original creation. No karma threshold or
lifetime ownership cap is introduced. Another participant has an independent allowance.

`CommunityService.Create` coordinates the check, community insert and owner membership
in one SQL transaction. A participant-scoped SQL application lock serializes competing
creations, and direct repository inserts enforce the same quota. Accounting uses
`AtlasTime.UtcNow`, not supplied community timestamps. Invalid or rolled-back writes
consume nothing. Non-SQL adapters must implement `ICommunityCreationQuota` to enforce
the budget; an aggregate alone cannot know repository-wide creation history.

Quota exhaustion returns `CommunityCreationQuotaExceededException` with remaining
allowance, retry delay and UTC next-slot time. It reports `Denied` / `QuotaExceeded`
through existing operation telemetry; successful creation reports `CommunityChanged`.
These are diagnostic events, not new security audit categories or automatic alerts.
Use diagnostics/metrics to investigate repeated attempts; cross-account abuse still
requires future API/identity controls. Trusted demo seeding writes directly to the
context and does not consume user slots. Existing communities are not backfilled.

Tests: [CommunityCreationQuotaTests](../../../tests/Atlas.Communities.Tests/CommunityCreationQuotaTests.cs) documents the cutoff/message;
[SqlCommunityCreationQuotaTests](../../../tests/Atlas.Persistence.Tests/SqlCommunityCreationQuotaTests.cs) exercises durable enforcement, participant isolation,
archive/edit behavior, rollback, backdating and competing requests on a fresh model.
SQL execution requires `ATLAS_SQL_TEST_CONNECTION_STRING`; fresh-model tests do not
verify the migration/upgrade path.

### Migration handoff

The community ledger adds a table. Per `src/Atlas.Persistence/AGENTS.md`, migrations
and the snapshot must be generated, reviewed and committed by the developer. If the
voting migration has not been generated yet, one `AddAbusePreventionQuotas` migration
can include all pending quota tables. If earlier quota migrations already exist, keep them and add a migration for the remaining
model changes, such as `AddReportSubmissionQuota` for the report ledger. Do not remove an already-applied migration. From the solution root:

```powershell
dotnet tool restore
dotnet ef migrations add AddAbusePreventionQuotas --project src/Atlas.Persistence
dotnet ef migrations has-pending-model-changes --project src/Atlas.Persistence
dotnet ef database update --project src/Atlas.Persistence
dotnet test tests/Atlas.Communities.Tests/Atlas.Communities.Tests.csproj
dotnet test tests/Atlas.Persistence.Tests/Atlas.Persistence.Tests.csproj --filter FullyQualifiedName~SqlCommunityCreationQuotaTests
```

No migration or snapshot is generated or edited by the assistant. Migration-dependent
checks remain pending until the developer commits these artifacts.

## Report submission budget

A participant may submit **three successful reports per rolling hour** across all
nodes. The third is accepted; the fourth is rejected until a slot expires. This is
a provisional MVP product choice, not a measured optimal threshold. Three reports
over five hours would allow the same initial burst while imposing a longer wait
on legitimate reporters. Start with the shorter cooldown, observe denials and report
outcomes, and tune the configurable `ReportSubmissionQuotaPolicy` from actual use.
Do not assume another participant will discover and report the same issue.

The error names the limit, UTC retry time, and escalation guidance:

> Report limit reached: 3 reports in 60 minutes. Try again at 2026-10-19 13:00:00 UTC. For additional legitimate concerns about nodes, contact support staff.

This does not introduce an email address, support endpoint or automated escalation.
The production host must provide a discoverable support contact route for this advice
to be actionable. Reporting remains a human moderation mechanism; no AI screening is added.

`ReportSubmissionQuotaPolicy`, allowance and rejection belong to Atlas.Moderation.
`ModerationService.ReportNode` coordinates submission; `SqlModerationCaseRepository`
also enforces direct inserts. A reporter-scoped SQL lock covers check/write, and
`ReportSubmissionQuotaRows` records server accounting time with a participant/time
index. Backdating a case cannot bypass the budget. Failed/invalid/rolled-back reports
consume nothing. Successful report and ledger writes, required audit evidence and
delivery tickets commit together. Decisions, dismissal, hiding, restoration and review
requests do not consume or refund submission slots. Exhausted reporters do not block
moderators from resolving existing reports. Duplicate reports still consume slots;
no new deduplication rule is introduced. Separate accounts have separate budgets.

Report creation retains the existing `ModerationChanged` audit evidence, with no raw
report title/explanation logged. Quota denials use the existing `Denied` / `QuotaExceeded`
diagnostic path; no successful report audit is added on rejection. No new automatic
alert or production dashboard is configured. Failed-request floods and coordinated
multiple-account abuse still require API/identity protections. Non-SQL adapters must
implement `IReportSubmissionQuota` for equivalent coordinated accounting.

[ReportSubmissionQuotaTests](../../../tests/Atlas.Moderation.Tests/ReportSubmissionQuotaTests.cs)
shows the accepted third report, blocked fourth, exact expiry, configurable window
and full support/retry wording.
[SqlReportSubmissionQuotaTests](../../../tests/Atlas.Persistence.Tests/SqlReportSubmissionQuotaTests.cs)
checks persisted participant budgets, unchanged queue/audit/outbox on rejection,
moderator access while exhausted, rollback, server accounting and competing final slots.
Fresh-model SQL tests require a configured connection and do not verify upgrades.

The new ledger requires a developer-generated migration. If no quota migrations
exist yet, use `AddAbusePreventionQuotas` in the commands above to capture all three
ledgers. If earlier quota migrations exist, retain them and add the remaining change:

```powershell
dotnet tool restore
dotnet ef migrations add AddReportSubmissionQuota --project src/Atlas.Persistence
dotnet ef migrations has-pending-model-changes --project src/Atlas.Persistence
dotnet ef database update --project src/Atlas.Persistence
dotnet test tests/Atlas.Moderation.Tests/Atlas.Moderation.Tests.csproj
dotnet test tests/Atlas.Persistence.Tests/Atlas.Persistence.Tests.csproj --filter FullyQualifiedName~SqlReportSubmissionQuotaTests
```

Per `src/Atlas.Persistence/AGENTS.md`, the developer must generate, review and commit
the migration, designer and snapshot before migration-dependent CI passes. No such
files are generated/edited by the assistant and no developer database is updated.
Existing cases are not backfilled into the ledger; retention/account-deletion policy
for this ledger belongs with the other quota ledgers in PTT-139.

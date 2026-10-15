# Implementation, verification and follow-ups

Status: **local runtime implementation and developer-generated migration added**. See [the running guide](LOCAL-DEVELOPMENT.md) for current coverage, commands and tests. The staged plan and future deployment criteria remain below. [Requirements](README.md) and [event catalogue](EVENTS.md).

## Terms used in this plan

- **Sink** means output destination. A **test sink** is a small in-memory recorder used by automated tests instead of a real file or cloud service. Tests inspect the captured events to check their fields and outcomes, and confirm fake passwords/tokens never appear. It is “sink,” not “sync”; it does not mean synchronization.

- **Serialization** converts the approved event record into JSON. Tests check both the record and serialized output; checking only an object before serialization is insufficient.

- **Duplicate ownership** means deciding which layer emits a particular event. For example, a failed sign-in should not be reported as three separate attempts by the session, menu and exception handler. This differs from deduplicating retried cloud deliveries by event ID.

- **Rotating files** means starting a new diagnostic file at a size/time boundary. For example, `diagnostics-001.jsonl` fills, then new records go to `diagnostics-002.jsonl`. Older files can be retained or removed under a configured policy. Pending audit records are never deleted through diagnostic-file rotation.

- **Audit contract** means the typed record and interface that workflows use to request required audit persistence. Atlas.Contracts is a candidate for a genuinely shared boundary contract; the word “contract” alone does not determine project ownership. Keep SQL implementation in persistence and provider wiring in the host. Review dependencies before choosing the final location.

- **Durable outbox** means pending-delivery rows in SQL, saved with the audit record and business action. It is not the diagnostic output file. A later background delivery loop reads these rows, sends them and records confirmed acceptance. Process restarts do not erase pending rows.

- **Worker** means that background delivery loop, not a person or a current separate service. It might eventually run inside the host or as a separate process. Its implementation/deployment is still future work.

## Suggested implementation sequence

1. **Local structured logging and event context.**

   - Introduce stable event names, bounded outcome/reason categories and an operation context.
   - Wire a JSON file provider in Console startup, writing to a configured gitignored local folder. Keep menus separate. Use explicit environment/configuration to select local files versus a future deployment exporter; no live cloud connection is needed now. Review size/time rotation and retention settings rather than building a domain file writer.
   - Instrument final account/session outcomes, authorization denials and outer operation failures first.
   - Verify approved fields/secret exclusion, JSON encoding, one responsible emitter per event and actual output capture with an in-memory recording test sink. Add temporary-file tests for real file output/rotation; these are separate from SQL audit persistence tests. No cloud account is needed.

2. **Durable audit for important actions.**

   - Agree on the initial required actions: moderation decisions/visibility and privileged role/account-security changes are the proposed starting set.
   - Define the application audit contract and persist records atomically with those changes. A logger call after commit is insufficient for guaranteed audit recording.
   - Required audit records must have a durable outbox and reliable cloud delivery. Do not silently assume the existing in-memory event publisher survives process termination.
   - Review retention/access policy and privileged provisioning before claiming production audit coverage.
   - Any schema change requires the developer-generated EF migration workflow; this implementation adds audit/outbox mappings; the developer-generated AddSecurityAuditOutbox migration is now committed. The implementation PR summary must include exact tool-restore, migration-creation, pending-model verification, database-update and relevant test commands with `--project src/Atlas.Persistence`. The developer generates, reviews and commits the migration/designer/snapshot; do not generate or apply them on their behalf.

3. **API and cloud integration.**

   - Add API middleware for correlation, authenticated context and operation metrics/traces when the host exists.
   - Configure host/exporter destinations, secure collector transport, retention, operator access and pipeline health checks.
   - Measure baseline behavior before selecting targets, sampling and alert thresholds. Cloud dashboards/SIEM selection are deployment work.

The local logging, persistence and local delivery portions are now implemented; the API/cloud slice remains deferred. The parent ticket remains incomplete until its agreed acceptance evidence and follow-ups are reviewed.

## Host and domain responsibilities

| Responsibility | Owner |
|---|---|
| Establish authenticated actor and server operation ID | Console session/application boundary; future API authentication/middleware |
| Decide whether an operation is allowed | Existing application/domain policies; observability does not grant permission |
| Describe semantic action and final result | Application workflow at the enforcement/coordinator boundary |
| Persist required audit intent with business changes | SQL adapter using the same transaction/outer operation |
| Format/filter/export diagnostic events | Host logging providers; no cloud SDK in domain projects |
| Collect spans and metrics | Host instrumentation and configured exporter |
| Store/query/retain telemetry; evaluate alerts | Deployment/operations configuration |

For workflows still owned by Console, put the semantic event into reusable coordination as that code moves under [PTT-131](https://thepublicthinktank.atlassian.net/browse/PTT-131), rather than coupling future API behavior to menu code.

## Correlation through Console, API and workers

- The field is named `observability_id` to make its purpose explicit: linking audit, log and trace records for one workflow. It is not a participant ID or proof of permission, and it does not measure duration on its own; trace spans and elapsed-time measurements do that.

- Console starts a new observability operation ID for each command. Related repository calls and events share it. Do not reuse a lifetime ID for every command in one session.

- The future API creates a server operation ID per request and accepts valid trace propagation context. A caller-supplied request/correlation header must be bounded and validated; it is not an actor identity or trusted audit event ID.

- Use request route templates, not raw URLs/query strings. Do not add request bodies or headers to trace tags.

- Resolve actor identity after authentication and authorization context is available. Do not accept actor IDs from request bodies or inherited logging scopes alone.

- A future outbox delivery worker uses `actor_kind=system` for its own delivery-attempt events: software initiated that attempt. It must preserve the original audit record’s participant actor, event ID and observability operation ID, rather than relabeling the human action as a system action. Each delivery attempt can have its own trace span linked back to the original operation.

- Async scopes must be disposed after each operation. Tests must show that parallel requests cannot inherit one another's actor, operation ID or fields.

- Future API abuse defense should include controlled IP/user-agent collection. Obtain the peer address from the server and honor forwarded addresses only through configured trusted proxies. Treat user-agent text as untrusted, bounded input, never as verified identity. Define approved security-event fields, restricted access, short purpose-specific retention and minimization before enabling collection. Do not add these fields to every audit record, or use IP/user-agent values as metric labels. Console has no HTTP client IP/user-agent to collect.

## Verification without a cloud account

| Test scenario | Required evidence |
|---|---|
| Sign-in success, wrong password, unknown account, locked/unconfirmed/inactive account and unsupported 2FA | Correct final outcomes; exactly one owned event; no false successful authentication |
| Anonymous or unauthorized mutation | Denied result recorded; no mutation and no durable success event; supplied IDs do not become trusted actors |
| Successful moderation change | Business change and one semantic durable audit action commit together; case IDs/count correlate |
| Business write failure / outer rollback | Neither business state nor successful audit persists; a sanitized failure signal may remain |
| Required audit insert failure | Entire audited mutation rolls back |
| Diagnostic/file/cloud sink unavailable | Defined bounded behavior; no credential leak or recursive logging; no retry of an already committed business action |
| Outbox retry / process restart | Pending events recover; event ID stays stable; duplicates are handled; exhausted retries retain records for replay; no exactly-once assertion |
| Secret canaries in password, token, cookie, content, SQL/exception text | Captured serialized output contains none of them, including framework logs and exporter failures |
| Unapproved fields and future model properties | Adding secret-bearing properties to request/domain objects never adds them to serialized events; only approved fields and value categories are emitted |
| Malicious newlines/control characters and huge inputs | One safely encoded bounded record; no injected entries or unexpected fields |
| Concurrent operations | Correct distinct actors/operation IDs; scopes do not leak |
| Cache metric export and restart | No lifetime-counter double counting; page/detail outcomes are distinguished; reset behavior is explicit |
| Metric labels and trace tags | No identifiers, search terms, raw routes or unbounded categories in metrics; no sensitive payload tags |
| Graceful shutdown and full buffers | Bounded flush/drain policy tested; diagnostic loss is disclosed; persisted audit remains recoverable |

Use an in-memory recording sink for schema/privacy/correlation tests and an isolated SQL fixture for atomic audit/outbox tests. A live cloud account should not be needed to validate the application contract. Later staging checks must still verify the actual transport, ingestion, permissions and alert delivery.

## Required cloud delivery acceptance

- Persist audited changes, audit records and pending delivery atomically; rollback tests cover all three.

- Simulate an unavailable cloud endpoint and restart the worker. Pending records must survive and later reach the destination with the same event IDs.

- Simulate acknowledgement loss after ingestion. Retrying may duplicate delivery but must not lose the record; reconcile by event ID.

- Exhaust retries and fill buffers. Retain pending records for operator replay and expose backlog/failure signals. If durable capacity is exhausted, reject new required audited mutations instead of dropping audit data.

- Specify and test the selected provider's durable-acceptance acknowledgement. Do not treat an in-memory exporter queue as successful delivery. Verify eventual queryability in staging and document recovery/backup limits.

## Protecting audit data

- SQL permissions can separate audit access from ordinary application data access. Production design should give the application/worker only needed insert/delivery-update permissions, with a separate restricted operator role for querying/export. Public application users must not receive database credentials or an audit browsing endpoint by default.

- Prefer append-only audit records; delivery status belongs in outbox state. Restrict alteration/deletion of audit history. This does not make records immune to a database administrator; stronger tamper evidence and retention controls need a separate review.

- Local developers can inspect their own development SQL audit records and gitignored diagnostic files. Use development data and credentials. `.gitignore` prevents routine Git inclusion; it does not encrypt files or restrict operating-system access.

- Production permission provisioning and retention remain deployment work. The local implementation must still avoid public audit access and document which identities write, deliver and query records.

## Deployment decisions still open

- **Retention:** choose periods separately for diagnostics, security signals and authoritative audit records. No retention duration is mandated by this PR.

- **Access:** define which operator roles can read/export records, how access is itself audited, and how production is separated from development.

- **Availability:** define bounded buffering, overflow policy, recovery and backlog alarms. Required audit data must not be silently dropped when a queue fills.

- **Privacy/deletion:** participant/resource IDs can be identifying data. Define account-deletion handling, justified audit retention and export policies rather than assuming all records live forever.

- **Costs:** decide log volume/sampling/retention budgets before choosing infrastructure. Do not collect full payloads just because storage is available.

- **Incident workflow:** decide who investigates repeated denials, how records are queried by operation/target IDs, and which notifications require action. No alert recipient or threshold is selected yet.

## PTT-126 acceptance traceability

| Ticket expectation | This PR's evidence / remaining work |
|---|---|
| Inventory MVP security/operational events | [EVENTS.md](EVENTS.md), with proposed emitters and current enforcement evidence |
| Structured strategy and common context | [README.md](README.md#common-event-fields); illustrative envelope, actor/outcome/correlation rules |
| Secret/sensitive-data handling | [Allowlist and serialization requirements](README.md#sensitive-data-and-safe-serialization); future canary tests above |
| Distinguish diagnostics, audit, metrics and alerts | [Definitions](README.md#start-here), separate durability and export rules |
| Vendor-independent application code | Proposed host/application/persistence responsibilities; no package/vendor coupling added |
| Testable important events without live cloud | Verification matrix above; local contract/file tests and fresh-model SQL tests added; see the running guide for validation limits |
| Identity/API correlation | Final sign-in outcomes, per-operation context and future middleware contract |
| Defer cloud retention/dashboards/SIEM | Explicit deployment decisions and provider examples, no provisioning |
| Track larger implementation gaps | Linked follow-up tasks below |

## Follow-up work

- [PTT-134: structured local security logging and operation context](https://thepublicthinktank.atlassian.net/browse/PTT-134).

- [PTT-135: transactionally durable audit for moderation and privileged security actions](https://thepublicthinktank.atlassian.net/browse/PTT-135).

- [PTT-136: API correlation and cloud observability with reviewed deployment policies](https://thepublicthinktank.atlassian.net/browse/PTT-136).

These tasks implement the reviewed contract; this proposal does not silently choose the remaining production policies.

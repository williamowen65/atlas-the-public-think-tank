# Security logging, audit and observability

Status: **Proposed requirements for review**, not implemented instrumentation. [PTT-126](https://thepublicthinktank.atlassian.net/browse/PTT-126). Source baseline: main after PR #345, reviewed 2026-10-13.

## Start here

- **Logging** records individual events: an operation failed, sign-in was denied, or a database conflict occurred.

- **Audit** records important actions with enough context to reconstruct who did what, to which resource, and with what result. A diagnostic message is not automatically a durable audit record.

- **Observability** means being able to answer questions about a running application from its signals. For Atlas: why is discovery slow, are sign-in failures increasing, did a moderation action commit, and is notification delivery failing?

- **Metrics** summarize activity over time: request counts, durations, failure counts and cache hits. They show a pattern, but do not tell the whole story of one operation.

- **Traces** follow one request/workflow through its steps. A slow discovery request could contain separate spans for the revision check, page query and card loading.

- **Alerts** are rules evaluated by monitoring infrastructure, not a separate kind of application log. For example, sustained delivery failures could notify an operator. Thresholds and recipients remain deployment decisions.

This first PR defines the events and integration contract. It adds no logger provider, file writer, database audit table, exporter, dashboard or cloud dependency. Follow the [event inventory](EVENTS.md) and [implementation and verification plan](IMPLEMENTATION.md) for proposed work.

## What exists today

| Current evidence | What it establishes | What it does not establish |
|---|---|---|
| [IdentityServices.AddAtlasIdentity](../../../src/Atlas.Identity/IdentityServices.cs) registers `AddLogging`; [AtlasSignInManager](../../../src/Atlas.Identity/AtlasSignInManager.cs) receives the framework logger | Framework logging services are available | Console has no configured logging output provider; this is not an Atlas-wide security event catalogue |
| [Console Program](../../../src/Atlas.Console/Program.cs) and [account commands](../../../src/Atlas.Console/Identity/ConsoleAccountCommands.cs) write user-facing output | Interactive feedback and development setup | A structured log or durable audit destination |
| [ModerationCase](../../../src/Atlas.Moderation/ModerationCase.cs) and [NodeReaction](../../../src/Atlas.Graph/Reactions/NodeReaction.cs) retain domain history | Some actions already have persisted business records | A complete security history, including denied attempts or every privileged action |
| [DiscoveryCache](../../../src/Atlas.Persistence/Discovery/DiscoveryCache.cs) maintains counters | In-process diagnostic measurements, used by tests/debugging | An exported metric stream, dashboard or alert |
| [SqlOperationBoundary](../../../src/Atlas.Console/Storage/SqlOperationBoundary.cs) coordinates commits/rollbacks | A place to integrate transaction-aware audit persistence | A durable audit outbox or guaranteed delivery today |

Existing tests verify enforcement and persistence behavior. They do not verify the proposed telemetry because it has not been implemented.

## Proposed direction: stable events, configurable destinations

- Application workflows describe meaningful events using stable names and allowlisted fields. Domains remain independent of cloud SDKs.

- Hosts create operation context and select output destinations. Console does this in startup wiring; the future API does it in startup and request middleware.

- Use .NET `ILogger<T>` for diagnostics and structured security signals. Preserve named properties instead of constructing a prose string containing all values.

- Use .NET `ActivitySource` and `Meter` for workflow traces and metrics, with host-configured OpenTelemetry collection/export as a later implementation choice.

- Keep durable audit intent distinct from ordinary logging. A proposed application-owned audit contract accepts a bounded typed record; its SQL adapter participates in the business transaction. The exact interface and project placement need implementation review, especially while [PTT-131](https://thepublicthinktank.atlassian.net/browse/PTT-131) moves Console-owned workflows into reusable services.

- Do not install a logging hook on every EF row save and call that an audit trail. Rows do not always identify the semantic action, authenticated actor, denied attempt or complete operation.

### Local output first

- **Local development destination:** structured JSON diagnostic files in a configured local directory excluded by `.gitignore`, separate from interactive Console output. Select destinations through explicit host environment/configuration, not by guessing which computer is running.

- **File management:** choose a reviewed file provider for local files. Rotation starts a new file after a size/time threshold; it does not repeatedly overwrite the active file. Retention can remove older diagnostic files under a separately configured policy. .NET's general-purpose built-in providers do not include a generic rotating file sink. Configure size/age limits, permissions, shutdown flushing and a location outside the repository; do not write a new unbounded file writer inside domains.

- Redirecting a stream is useful for development, but does not by itself supply rotation, access controls or delivery guarantees. No capture command is presented as working until the provider is wired.

- Local files remain diagnostic output. They are not the authoritative record for actions that require transactionally durable audit evidence.

### Cloud output later

- Your expectation is correct: a cloud logging service can ingest events and provide a searchable interface. The application emits records; a configured provider/exporter or platform collector sends them to storage; a query interface reads that storage.

- For an Azure Container Apps deployment, Azure Monitor Log Analytics is one possible destination for collected application/system logs. Application Insights/OpenTelemetry can add request traces and performance telemetry. These are examples, not a provider selection in this PR.

- Changing destinations should preserve Atlas event names, field meanings and privacy rules. Host configuration and infrastructure wiring change; business rules do not.

- Ordinary console logs or sampled traces cannot substitute for a durable audit ledger. An authoritative audit record should persist with the state change; a cloud copy must be delivered reliably after commit for required audit records.

## Common event fields

| Field | Requirement and meaning |
|---|---|
| `schema_version` | Start at 1; change when field meanings/contracts become incompatible |
| `event_id` | Unique occurrence ID; preserve it when delivery retries the same record |
| `event_name` | Stable action, such as `moderation.visibility_changed`; not user-provided text |
| `occurred_at_utc` | Server-assigned UTC event time; distinguish it from later ingestion/export time |
| `observability_operation_id` | Server-generated observability correlation ID shared by records from one operation; not an authentication identity. Works even without tracing |
| `actor_kind` | `participant`, `anonymous` or `system` |
| `actor_id` | Verified participant ID for an established actor; otherwise absent. A submitted email/user ID is not an authenticated actor |
| `target_kind`, `target_id` | Resource type and ID when available. Avoid logging complete objects |
| `outcome` | `succeeded`, `denied`, `failed` or `conflict`; an attempted write is not a succeeded write |
| `reason_code` | Bounded allowlisted category, such as `not_authorized`, `invalid_input` or `storage_conflict`; no arbitrary exception text |
| `service`, `environment`, `app_version` | Host-provided context for identifying a deployment; exclude machine paths and secrets |
| `trace_id`, `span_id` | Optional validated trace context, present when a span is active |
| `duration_ms` | Optional elapsed duration for completed operations; measure elapsed time rather than subtracting wall-clock timestamps |

The schema is a proposal, not an existing C# type. Event-specific additions must be allowlisted in the inventory. Identifiers are sensitive operational data even when they are not secrets.

### Example: a committed visibility change

```json
{
  "schema_version": 1,
  "event_id": "8c4b817d-6520-41d4-9c30-eec174449abb",
  "event_name": "moderation.visibility_changed",
  "occurred_at_utc": "2026-10-13T21:00:00Z",
  "observability_operation_id": "62b438d1-e332-481f-8d23-b0e5ab4781f4",
  "actor_kind": "participant",
  "actor_id": "8593b626-5d31-4945-a401-f237c48e07cb",
  "target_kind": "node",
  "target_id": "b1000000-0000-4000-8000-000000000023",
  "outcome": "succeeded",
  "reason_code": "moderator_exclusion",
  "visibility": "hidden",
  "report_count": 2,
  "service": "atlas-console",
  "environment": "development",
  "app_version": "example"
}
```

All values above are illustrative. Report bodies and hidden node content are deliberately absent.

## Commit, failure and delivery rules

- **Diagnostics/security attempts:** record denials and failures at the application/authentication boundary where the outcome is known. Avoid duplicate records from every catch/rethrow layer. A failure record does not depend on committing the rejected business write.

- **Durable successful actions:** write the audit record with the business mutation in the same SQL transaction. If the business write rolls back, its success record must roll back too. Nested operations use the outer transaction, rather than publishing success after an inner `SaveChanges`.

- **Cloud delivery is required for audit records.** Persist the audit record and a durable outbox entry in the same transaction as the business change. A worker delivers it after commit. A successful business response means the action and pending audit delivery are durably stored; it does not mean cloud ingestion has already completed.

- **During a cloud outage:** keep undelivered records in durable storage across process restarts. Retry with backoff and bounded individual attempts. Exhausting automatic attempts must retain the record for operator recovery/replay, not delete it. Monitor backlog count/age and delivery failures so a stalled pipeline cannot silently go unnoticed.

- **Confirm delivery:** keep `event_id` stable across retries. Mark an entry delivered only after the chosen destination's documented durable-acceptance acknowledgement. A collector accepting data into memory is not sufficient evidence. Define that acknowledgement boundary and test reconciliation/queryability with the selected provider. Retries can produce duplicate copies; deduplicate by event ID rather than risk dropping the original.

- **No silent loss:** required audit records are not sampled, dropped when a buffer fills, or expired while pending delivery. If durable local audit/outbox storage cannot accept a required record, reject/roll back the associated mutation. Define recovery, capacity limits and backup/restore procedures before production. This is an eventual-delivery requirement under recoverable infrastructure, not a claim that software can prevent loss after every possible storage disaster. Ordinary diagnostics and sampled traces retain their separate policies.

- **Failures:** ordinary diagnostic sink failure must not silently change a business result. Durable audited writes must fail/roll back when their required local audit persistence fails. An unavailable cloud exporter should not undo an already committed local action; it leaves recoverable pending delivery.

- **Unknown commit outcome:** a connection failure during commit can leave the application uncertain whether SQL committed. Record `failed` with a bounded `commit_outcome_unknown` reason for the attempt; reconcile by operation/event ID before retrying. Do not claim the database definitely rolled back.

- Do not hold SQL transactions open while calling a cloud logging service. Do not retry business mutations simply because exporting their diagnostic message failed.

## Sensitive data and safe serialization

- Never record passwords/hashes, confirmation/reset/2FA tokens, recovery codes, cookies, raw authorization headers, API keys, connection strings or Data Protection material.

- Exclude submitted email addresses, raw search text, document/comment/report bodies, upload content and complete request/response objects by default. Use lengths, counts and stable IDs only when needed.

- Resolve actor IDs from trusted authenticated context. An unsuccessful sign-in can have an anonymous actor; finding an account from submitted input does not authenticate the caller. Account correlation, if needed for abuse analysis, requires a separately reviewed pseudonymous subject policy.

- **Construct events from an explicit allowlist before serialization.** Each event type defines approved fields and permitted value categories. Build a typed record by selecting those values deliberately; never serialize a request, domain object, dictionary of arbitrary properties or exception and then try to remove sensitive fields afterward.

- **New fields require deliberate review.** Adding a property to a request/domain model must not automatically add it to logs or audit records. An event-schema addition must document its purpose, privacy impact, bounds and tests before it is permitted.

- **Redaction is defense in depth.** It can catch accidental leakage but is not the primary protection. Framework/provider logs need their own reviewed category/level configuration and safe field handling; Atlas's event allowlist does not automatically govern third-party messages. Do not enable a framework category that emits forbidden data and rely on downstream cleanup.

- Keep EF sensitive-data logging disabled outside deliberately controlled development. Do not log SQL parameters, request headers, URLs with query strings or exception `Data` indiscriminately.

- Structured serialization must encode newlines/control characters, bound string lengths and reject unexpected properties. User content must not create extra log records or field names.

- Access to audit/security records is an operator permission, not implied by public node access or the Global Moderator role. Proposed access must be explicit and auditable; exports, retention and deletion require deployment/privacy review.

## Useful first metrics and traces

| Signal | What it helps answer | Allowed dimensions |
|---|---|---|
| Operation count and duration histogram | Which workflows fail or slow down? | Stable operation name, outcome, environment |
| Sign-in denial/lockout counts | Are failures increasing? | Bounded outcome/reason; no email, participant ID or IP label |
| Database conflict count | Are constraints/locks causing retries? | Operation and bounded conflict category |
| Discovery page/detail hits, misses and oversized rejections | Is caching avoiding reads? | Cache layer and outcome; no page keys, query text or node IDs |
| Notification/export failure and backlog age/count | Is work being delivered? | Stable channel and outcome |
| Workflow/database spans | Where does one request spend time? | Operation names and sanitized database metadata; no SQL parameters/content |

- Existing cache counters are lifetime values, not already exported OpenTelemetry counters. A future adapter must define reset/delta semantics to avoid double-counting when sampled, or increment instruments at the lookup points.

- Proposed metrics have bounded labels. Per-user/resource identifiers belong only in appropriately protected event records, not unbounded metric labels.

- Diagnostic logs/traces may be filtered or sampled. Required authoritative audit actions are not sampled. Anonymous read successes need aggregate metrics rather than a per-resource audit record for every browse action.

## Decisions for this review

| Decision | Proposed starting point | Still to decide |
|---|---|---|
| Local diagnostics | Structured JSON files in a gitignored local folder, separate from menus | File size/time rotation thresholds and diagnostic retention |
| First durable audit actions | Moderation visibility/decisions and privileged role/account security changes | Retention and exact privileged provisioning workflows |
| Other mutations | Structured outcomes; preserve existing business history | Which require durable retention beyond existing domain records |
| Observability | Operation metrics and correlated workflow traces | Sampling, targets and alert thresholds after measurement |
| API context | Verified actor, generated operation ID and trace context | Transport middleware, trusted proxy/IP rules and security access policy |
| Production destination | Configurable host/infrastructure adapters | Cloud provider, region, budget, retention, dashboards and alert recipients |

No production release should claim PTT-126's instrumentation is complete based on this documentation PR. The implementation tests and deployment policies below remain work to do.

## References

- [.NET logging providers](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging-providers): output destinations are provider responsibilities; a generic rotating file sink needs a separate choice.
- [OpenTelemetry signals](https://opentelemetry.io/docs/concepts/signals/): logs, metrics and traces provide different forms of evidence.
- [Azure Container Apps observability](https://learn.microsoft.com/en-us/azure/container-apps/observability) and [Log Analytics integration](https://learn.microsoft.com/en-us/azure/container-apps/log-monitoring): examples of future searchable cloud destinations, not selected infrastructure.
- [Authorization evidence](../authorization/README.md), [validation evidence](../validation/IMPLEMENTATION.md), [Identity architecture](../../architecture/IDENTITY.md) and [data ownership](../../architecture/DATA-OWNERSHIP.md) supply the existing application boundaries.

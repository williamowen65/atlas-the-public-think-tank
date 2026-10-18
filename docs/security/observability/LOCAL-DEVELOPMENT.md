# Run and inspect local observability

This is the implemented local-development slice of PTT-126. API middleware, cloud adapters, production permissions and dashboards remain deployment follow-ups. No cloud account is needed.

## First: apply the developer-generated migration

The developer-generated `20261014195401_AddSecurityAuditOutbox` migration, designer and snapshot are now included. Do not scaffold the same migration again. Run from the solution root against your intended local development database:

```powershell
dotnet tool restore
# Review the committed migration before applying it.
dotnet ef migrations has-pending-model-changes --project src/Atlas.Persistence
dotnet ef database update --project src/Atlas.Persistence
dotnet test tests/Atlas.Console.Tests/Atlas.Console.Tests.csproj --filter FullyQualifiedName~ObservabilityTests
dotnet test tests/Atlas.Persistence.Tests/Atlas.Persistence.Tests.csproj --filter FullyQualifiedName~SqlAuditTests
```

SQL tests use `ATLAS_SQL_TEST_CONNECTION_STRING` in the shared user secrets or environment. The developer generated and committed the migration/designer/snapshot. Model consistency and migration-dependent CI now validate those committed files. Do not reset the database merely to add these tables; the migration should preserve existing content.

## Launch

### Visual Studio Run button

1. Pull the latest PR branch changes.
2. Set **Atlas.Console** as the startup project in Solution Explorer.
3. Select the **Atlas.Console** launch profile beside the green Run button, then press Run or F5.

The committed [launchSettings.json](../../../src/hosts/Atlas.Console/Properties/launchSettings.json) sets `DOTNET_ENVIRONMENT=Development` automatically for that profile. No PowerShell command is needed. This is local launch configuration, not a production environment setting; do not put SQL credentials or other secrets in it.

To inspect or change it in Visual Studio, open the project's **Properties → Debug → Open debug launch profiles UI**, select the profile, and inspect its environment variables. The JSON file is also directly editable if your Visual Studio version presents different menu labels.

### Read logs in Solution Explorer

- Expand **Atlas.Console → Logs → Observability**. The project links to the generated `.jsonl` files in the solution-root `logs/observability/` directory; it does not copy them into the project or build output.

- The folder is present before the first run. Files appear after they are created and the project is reevaluated. The wildcard also includes newly rotated files. If Visual Studio does not automatically discover a new file, unload and reload **Atlas.Console** to refresh the project items.

- Double-click a file to read it while the app runs. When the writer appends more records, Visual Studio can prompt to reload the open file (depending on your external-file-change settings). This is a normal editor view, not a continuously scrolling log viewer. Avoid editing these generated files while the app writes them.

- `.gitignore` still excludes the actual log files. Showing a linked file in Solution Explorer does not commit it. The project also excludes these files from build/publish copying and `dotnet watch` restarts.

- These links cover the default solution-root destination. If you override `ATLAS_TELEMETRY_DIRECTORY`, open that destination separately or update the project's log include path to match.

### Command line

```powershell
$env:DOTNET_ENVIRONMENT = "Development"
dotnet run --project src/hosts/Atlas.Console
```

The default directory is **`logs/observability/` under the solution root**, covered by `.gitignore`. The Console prints its resolved directory at startup. It also works when launched by Visual Studio: startup searches upward from the working directory and binary directory for `Atlas.sln`. Outside a source checkout, the fallback is a local application-data directory.

- `diagnostics-*.jsonl`: allowlisted business/security events and failures.

- `audit-*.jsonl`: delivered copies of committed SQL audit records. The same event ID connects a line to its SQL row. Delivery is asynchronous, so allow a moment after an action.

- `metrics-*.jsonl`: local count deltas and duration observations. These are readable samples, not OTLP wire-format or a complete monitoring dashboard.

- `traces-*.jsonl`: command, discovery and storage spans with trace/parent IDs and elapsed time. Interactive command spans include time waiting for input; use discovery/storage measurements for backend performance.

Read the latest files with an editor, or use PowerShell:

```powershell
Get-ChildItem logs/observability/*.jsonl | Sort-Object LastWriteTime -Descending
Get-Content (Get-ChildItem logs/observability/diagnostics-*.jsonl | Sort-Object LastWriteTime -Descending | Select-Object -First 1).FullName -Tail 10 -Wait
```

The prototype only enables this file host in `Development`. A different `DOTNET_ENVIRONMENT` fails startup explicitly rather than silently pretending that a local file satisfies production cloud delivery.

## Optional local settings

| Environment variable | Default | Meaning |
|---|---|---|
| `ATLAS_TELEMETRY_DIRECTORY` | `logs/observability/` | Absolute or working-directory-relative destination; custom paths need their own ignore/access policy |
| `ATLAS_TELEMETRY_FILE_BYTES` | 5242880 (5 MiB) | Start a new file when this size would be exceeded; minimum 1024 bytes |
| `ATLAS_TELEMETRY_RETAINED_FILES` | 5 per stream | Keep newest files across restarts; SQL audit history is unaffected |

Each process uses unique filenames to avoid competing appends. Diagnostic retention manages older files across process restarts. The default produces a small bounded collection per stream under normal single-process development. One event can exceed a configured size threshold; events have a closed bounded schema rather than user content. Deleting local files does not delete SQL audit records, but delivered records are not automatically replayed just because their inspection file was removed.

## What is saved where?

| Activity | Diagnostic file | Durable SQL audit and outbox |
|---|---|---|
| Final sign-in/registration/sign-out/session rejection | Yes; no submitted email/password/token | Account/role persistence changes are audited separately; a denied password check alone is a diagnostic event |
| Node/type/content/vote/reaction/community/membership/link/comment/profile/notification tracked changes | Bounded changed-record events, published after transaction commit | Only participant active-state changes in this group currently require additional audit records; existing business history remains intact |
| Moderation case transitions | Yes, after commit | Yes, including case ID, related node ID and bounded created/hidden/dismissed/restored/review-requested action |
| Identity account changes, role assignments/removals and role-definition changes | Yes, after commit | Yes; no credential values. Role membership identifies subject user and related role ID |
| Discovery execution and cache reuse | Duration/count/cache metrics and spans; no search text or selected node list | No read-history audit trail |
| Rejected/failed coordinated operations | Bounded reason/outcome, never exception messages | No successful-action audit survives a rollback |
| Audit delivery attempts | Outcome/failure diagnostic | Outbox lease, attempt count, retry time and acknowledgement state |

The first slice records **persisted transitions**, not a claim of a complete semantic command journal. Several transitions may share one `observability_id` (for example grouped moderation cases); one command can legitimately produce multiple records. Adding fields to EF rows does not add those values to telemetry. [AuditCapture](../../../src/Atlas.Persistence/Observability/AuditCapture.cs) is an explicit closed mapping.

Direct SQL, `ExecuteUpdate` and `ExecuteDelete` bypass `SaveChanges` capture. The outbox intentionally uses conditional `ExecuteUpdate` for its own lease/status metadata; it does not generate another audit action. Future privileged bulk operations must supply explicit audit intent rather than assuming this hook covers them. Domain histories and authorization remain their owning domains' responsibility.

Existing serialized SQL audit records and older files may still contain `observability_operation_id`. Newly serialized events and traces use `observability_id`; this naming change does not rewrite historical evidence or require a database migration.

## Node-view diagnostics and privacy

- Opening or redisplaying node details emits `node.view_completed` in diagnostics, a `NodeView` trace span, and count/duration metrics. Both authenticated and anonymous views use the shared [detail renderer](../../../src/hosts/Atlas.Console/NodeDisplay.cs). This measures detail rendering and its referenced-data reads, not the entire navigation request or time waiting for menu input. Redisplaying after an action counts again; it is not a unique-view or popularity statistic.

- The event explicitly says `privacy_policy: "read_no_identity_or_target"`. `actor_id` and `target_id` are intentionally null; `target_kind` is `node`. `actor_kind: "system"` describes the measurement context, not whether the viewer is signed in. `action` remains `none`: this is an observation, not a business mutation or SQL audit record.

- A fresh observability ID and root trace prevent joining the view to an actor-bearing parent command. Match its diagnostic and trace using `observability_id` or `trace_id`. This intentionally gives up reconstructing which person read which node. Timestamps and local access still make these files sensitive; this is data minimization, not a guarantee of anonymity.

- The [serializer](../../../src/Atlas.Observability/EventJson.cs) rejects node-view envelopes that contain actor/target/related-target IDs or omit the required policy. Do not add IDs as a debugging convenience. The [tests](../../../tests/Atlas.Console.Tests/ObservabilityTests.cs) cover successful and failed views, correlation isolation, restored caller identity and rejection of accidental enrichment.

- For a slow or failed view, use its duration, bounded outcome/reason and trace to identify the instrumented operation. This first span does not expose every internal method or SQL statement. Reproduce the issue locally for content-specific investigation; do not add titles, node IDs, user input or exception messages to the telemetry envelope.

## Three actual local shapes

Illustrative identifiers/times below; actual generated records use the same fields. The event serializer also includes service/environment/version, actor/target metadata and optional trace/span IDs.

```json
{"schema_version":1,"event_name":"moderation.changed","event_id":"730f52ac-a13a-40fe-a817-a81bccd5de7c","observability_id":"6bf6a849-a291-4527-bd01-16b8094ed1ea","action":"hidden","outcome":"succeeded","target_kind":"moderation_case"}
```

```json
{"signal":"metric","name":"atlas.discovery.cache.lookups","unit":"{lookup}","value":1,"temporality":"delta","layer":"detail","outcome":"hit"}
```

```json
{"signal":"trace","operation":"storage.transaction","observability_id":"6bf6a849-a291-4527-bd01-16b8094ed1ea","trace_id":"9638bb3f8ade4ae7864746418442ca77","span_id":"8a223597ac14f08a","parent_span_id":"425e1b24d9da67ae","duration_ms":12.4}
```

These are abbreviated examples for reading, not universal cloud JSON schemas. .NET `Meter` instruments and `ActivitySource` sources are available for future OpenTelemetry subscribers. Counters emit lookup deltas at actual lookup sites, avoiding double-counting lifetime snapshots. There are no per-participant/node/search metric labels.

## Audit delivery and recovery

- `AuditRecords` holds append-only serialized evidence; `AuditOutbox` holds independent destination/lease/attempt state. Normal context writes reject edits/deletes to audit records. SQL administrator access is still privileged; production SQL permission provisioning remains required.

- Required records and their outbox entries are inserted in the same transaction as business data. Failure rolls back that transaction. Outer repository/registration transactions publish success diagnostics only after commit.

- The local worker leases a due entry for two minutes, writes/flushed JSON to the local file and then acknowledges delivery in SQL. No database transaction spans file I/O. Competing workers use a conditional lease update; an expired lease can be recovered after a crash.

- A write or acknowledgement failure leaves the entry pending, with exponentially increasing delay capped at five minutes. No retry limit deletes it. If the process dies after writing but before acknowledgement, replay can duplicate a line. Use `event_id` to identify duplicates.

- A diagnostic file failure does not reverse a committed action. The in-process output failure count and shutdown warning expose failed diagnostic writes. Audit delivery failures are retried independently. No diagnostic text contains exception/connection details. The worker also emits pending-count/oldest-age gauges every ten seconds and reports file-output failures to standard error.

- Local and cloud destinations have separate acknowledgements. Future deployed contexts must enable `RequireCloudAuditDelivery` and register an `IAuditDestination` named `cloud`, whose success means the selected provider's documented durable acceptance. Cloud entries remain pending until that adapter acknowledges them. The current development host does not connect to a provider or create a pretend cloud acknowledgement.

- Pending rows are recoverable SQL state, not files subject to log rotation. Production backlog monitoring, storage capacity limits, operator replay tools, backup/restore guarantees and retention policies remain explicit deployment work. Existing SQL audit rows can be queried by authorized local developers; no public audit browsing API is added.

## Verification and current limits

- [ObservabilityTests](../../../tests/Atlas.Console.Tests/ObservabilityTests.cs): field allowlist, bounded values, exception-secret exclusion, duplicate failure ownership, concurrent actor/correlation scopes, diagnostic sink failure and actual file output/rotation.

- [SqlAuditTests](../../../tests/Atlas.Persistence.Tests/SqlAuditTests.cs): commit/rollback, failed audit insertion, retry identity, local-versus-cloud acknowledgements and append-only enforcement. These isolated fresh-model tests do not prove a migration upgrade; CI runs them in a separate SQL job while migration generation remains yours.

- The public observer port accepts only typed events. Arbitrary framework `ILogger` messages are not exported to these files; this avoids bypassing the allowlist through EF/Identity message formatting. Reviewed framework diagnostics may be added later with their own privacy policy.

- The new infrastructure is reusable; it is not a new business bounded context. [Telemetry contracts](../../../src/Atlas.Contracts/Observability/Telemetry.cs), [local infrastructure](../../../src/Atlas.Observability/LocalTelemetry.cs) and [host wiring](../../../src/Atlas.Application/Observability/LocalDevelopmentTelemetry.cs) have distinct ownership.

- No API middleware, IP/user-agent collection, generic privileged provisioning API, live cloud adapter, SIEM or production dashboard is implemented in this slice.

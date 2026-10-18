# PTT-126 observability and audit review map

Open [the native blackboard](ObservabilityAndAudit.excalidraw) in Excalidraw. Review regions 1–9 from left to right, then move down. Each card has its own link; the links target specific source lines rather than only a file. Read one card and follow its link before moving to the next. The bullets below provide the same review route in a searchable format.

Code links are pinned to commit `0716efbc670fc202b9dfc5a81095cf09623ffbd4` so line ranges stay stable during this review. Future implementation changes should update the board and its pinned links together. The [local-development guide](../../../security/observability/LOCAL-DEVELOPMENT.md) is the operational reference.

The board distinguishes implemented local behavior from deferred deployment work. It does not imply every mutation has a durable audit record or that traces instrument every internal method. Node-view timing covers detail rendering and referenced-data reads; it does not cover all navigation work before rendering.

## 1  START HERE: host wiring and operation context

- [Run button → development host](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/hosts/Atlas.Console/Program.cs#L45-L66)

  • Console startup installs telemetry.
  • Launch profile selects Development.
  • No cloud account is needed.

- [Host owns destinations](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/hosts/Atlas.Console/Observability/ConsoleTelemetry.cs#L19-L40)

  • Resolve logs/observability at solution root.
  • Install observer; start SQL delivery worker.
  • Non-development startup currently refuses.

- [One command gets one context](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/Atlas.Contracts/Observability/Telemetry.cs#L74-L95)

  • BeginOperation creates a correlation ID.
  • Only verified actor IDs enter this scope.
  • Disposal restores the previous context.

- [After authentication succeeds](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/hosts/Atlas.Console/Identity/ConsoleIdentitySession.cs#L57-L78)

  • UseVerifiedActor preserves operation ID.
  • A rejected login has no verified actor ID.
  • No submitted email or password is logged.

## 2  FOUR FILE STREAMS: what question does each answer?

- [Diagnostics: what happened?](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/Atlas.Observability/LocalTelemetry.cs#L64-L85)

  • Event name, outcome, bounded reason.
  • Ordinary changes publish after commit.
  • A failed sink does not undo business work.

- [Audit: what important change committed?](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/Atlas.Persistence/Observability/AuditDelivery.cs#L15-L36)

  • SQL is the authoritative evidence.
  • audit JSONL is a delivered inspection copy.
  • Payload is stored JSON, not rebuilt later.

- [Metrics: how often / how long / how much?](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/Atlas.Observability/LocalTelemetry.cs#L69-L90)

  • Counter observations are deltas to sum.
  • Duration observations support averages.
  • Dimensions are bounded, not node/user IDs.

- [Traces: which timed steps ran?](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/Atlas.Observability/LocalTelemetry.cs#L40-L61)

  • ActivitySource supplies parent/child spans.
  • Export only approved operation names.
  • A trace does not record every code line.

## 3  SQL WRITE PATH: business + required audit + outbox

- [A. Classify persisted transitions](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/Atlas.Persistence/Observability/AuditCapture.cs#L12-L33)

  • Stage inspects changed EF rows explicitly.
  • Select IDs and bounded actions only.
  • Do not serialize row or request objects.

- [B. Decide which changes require audit](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/Atlas.Persistence/Observability/AuditCapture.cs#L86-L104)

  • Moderation; Identity accounts and roles.
  • Participant active-state transitions.
  • Other mapped changes get diagnostics.

- [C. Save in the SQL transaction](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/Atlas.Persistence/AtlasDataContext.cs#L81-L113)

  • Audit/outbox rows join business writes.
  • The context joins an outer transaction.
  • Required audit insert failure rolls back.

- [D. Outer owner commits first](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/Atlas.Application/Storage/SqlOperationBoundary.cs#L42-L66)

  • A coordinated operation owns its commit.
  • Publish staged success events afterward.
  • No file/cloud I/O inside this SQL commit.

- [E. Publish the matching transaction](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/Atlas.Persistence/AtlasDataContext.cs#L41-L62)

  • Events retain their transaction ID.
  • Success diagnostics follow confirmed commit.
  • Keep unrelated retry evidence separate.

- [Failure and uncertainty](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/Atlas.Application/Storage/SqlOperationBoundary.cs#L63-L77)

  • SQL failure: no committed success audit.
  • Connection loss during commit: outcome
    can be unknown; do not assume rollback.

## 4  OUTBOX DELIVERY: commit now, deliver recoverably

- [A. Two different SQL responsibilities](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/Atlas.Persistence/Observability/AuditRows.cs#L4-L26)

  • AuditRecords: append-only event payload.
  • AuditOutbox: mutable delivery status.
  • File retention never deletes pending rows.

- [B. Claim a due entry](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/Atlas.Persistence/Observability/AuditDelivery.cs#L34-L56)

  • Destination-specific pending selection.
  • Conditional update elects one lease owner.
  • A lease expires after two minutes.

- [C. Write, then acknowledge](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/Atlas.Persistence/Observability/AuditDelivery.cs#L53-L64)

  • Flush the audit JSONL copy first.
  • Mark delivered only after acceptance.
  • No SQL transaction spans file I/O.

- [D. Retry and recover](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/Atlas.Persistence/Observability/AuditDelivery.cs#L68-L78)

  • Retry delay grows, capped at five minutes.
  • No retry limit discards the audit record.
  • Crash after write can duplicate delivery.

- [Same event; separate delivery operation](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/Atlas.Persistence/Observability/AuditDelivery.cs#L31-L40)

  • event_id identifies the original record.
  • Delivery has a new system operation ID.
  • Deduplicate repeated copies by event_id.

- [Cloud is a future adapter](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/Atlas.Persistence/Observability/AuditCapture.cs#L99-L102)

  • IAuditDestination defines the seam.
  • Local and cloud have separate outbox rows.
  • Local acknowledgement is not cloud proof.

## 5  NODE VIEWS: explicit privacy, useful timing

- [A. One renderer for both audiences](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/hosts/Atlas.Console/NodeDisplay.cs#L162-L190)

  • WriteDetails wraps referenced-data reads
    and rendering for signed-in/anonymous use.
  • The interactive menu is outside the span.

- [B. Separate root and correlation](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/Atlas.Contracts/Observability/Telemetry.cs#L107-L142)

  • Save caller context; remove parent Activity.
  • Use fresh observability and trace IDs.
  • Restore caller context even on failure.

- [C. Policy is visible on the record](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/Atlas.Observability/EventJson.cs#L44-L60)

  • node.view_completed; target_kind: node.
  • privacy_policy: read_no_identity_or_target.
  • actor_id and target_id intentionally null.

- [D. Enforce the policy at serialization](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/Atlas.Observability/EventJson.cs#L24-L32)

  • Reject actor, target and related-target IDs.
  • Reject missing/wrong node-view policy.
  • Nulls are policy, not incomplete logging.

- [What this does and does not measure](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/Atlas.Contracts/Observability/Telemetry.cs#L107-L138)

  • Redisplay creates another observation.
  • This is not a unique-view popularity count.
  • No SQL audit or browsing-history journal.

- [Tradeoff: less content-specific evidence](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/docs/security/observability/LOCAL-DEVELOPMENT.md#L98-L110)

  • Outcome, reason and duration remain useful.
  • No person/node lookup from this record.
  • Reproduce content-specific issues locally.
  • Timestamps still make logs sensitive.

## 6  FOLLOW EVIDENCE: identifiers and metric arithmetic

- [observability_id: one logical operation](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/Atlas.Contracts/Observability/Telemetry.cs#L158-L171)

  • Joins diagnostics to instrumented trace.
  • It is not an identity or authorization token.
  • Node views intentionally start a new ID.

- [trace_id / span_id / parent_span_id](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/Atlas.Observability/LocalTelemetry.cs#L49-L60)

  • trace_id groups the execution trace.
  • span_id identifies one timed step.
  • parent_span_id describes nesting.

- [event_id: one event, including replay](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/Atlas.Persistence/Observability/AuditCapture.cs#L89-L101)

  • SQL audit ID equals the event ID.
  • The worker copies the saved payload.
  • Delivery retries preserve event identity.

- [Counters: sum deltas over a window](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/Atlas.Observability/LocalTelemetry.cs#L77-L82)

  • Example: 3 observations of value 1 = 3.
  • Filter operation and outcome first.
  • Metrics omit per-request correlation IDs.

- [Durations: calculate a distribution](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/Atlas.Observability/LocalTelemetry.cs#L70-L94)

  • Use observations within a time window.
  • Average describes typical overall timing;
    percentiles reveal the slower tail.
  • Local files do not compute dashboards.

- [Gauges: latest value, not a running sum](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/hosts/Atlas.Console/Observability/ConsoleTelemetry.cs#L39-L57)

  • audit.pending is backlog at sample time.
  • 2, 2, 2 means still 2 pending, not 6.
  • Host samples health about every 10 seconds.

## 7  LOCAL INSPECTION: files, rotation and deletion

- [JSONL: one complete JSON object per line](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/Atlas.Observability/JsonLineFile.cs#L28-L52)

  • Append one event without array editing.
  • FileShare.Read lets an editor read live.
  • Writes flush before returning.

- [Rotation bounds local file growth](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/Atlas.Observability/JsonLineFile.cs#L48-L55)

  • Default: about 5 MiB per file.
  • Keep five files per stream across restarts.
  • Stop the app before manual test cleanup.

- [Find files in Solution Explorer](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/hosts/Atlas.Console/Atlas.Console.csproj#L36-L46)

  • Atlas.Console → Logs → Observability.
  • Linked wildcard uses the default log path.
  • Reload project if new files are not listed.
  • Generated files remain gitignored.

- [Deleting a copy does not delete history](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/Atlas.Persistence/Observability/AuditDelivery.cs#L55-L62)

  • SQL retains audit evidence and delivery state.
  • A delivered record is not replayed merely
    because its local file was deleted.
  • SQL retention remains separate policy.

## 8  REVIEW / VERIFY: where to start and what is deferred

- [First: privacy envelope and isolation tests](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/tests/Atlas.Console.Tests/ObservabilityTests.cs#L103-L162)

  • Exact field allowlist; no secret text.
  • Success/failure node views get root traces.
  • Reject accidental ID enrichment.
  • Verify caller context is restored.

- [Then: output and rotation tests](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/tests/Atlas.Console.Tests/ObservabilityTests.cs#L73-L103)

  • Real diagnostics, metrics and trace files.
  • JSON parses; arbitrary trace labels excluded.
  • Rotation retains a bounded set of files.

- [SQL: atomicity and recovery tests](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/tests/Atlas.Persistence.Tests/SqlAuditTests.cs#L15-L86)

  • Commit/retry keep event identity.
  • Rollback removes audit and delivery intent.
  • Required audit failure prevents commit.

- [SQL: history guards and worker leases](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/tests/Atlas.Persistence.Tests/SqlAuditTests.cs#L89-L188)

  • Local acknowledgement is not cloud ack.
  • Normal context rejects audit history edits.
  • Competing leases and expired-lease recovery.

- [Manual walkthrough after pulling](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/docs/security/observability/LOCAL-DEVELOPMENT.md#L80-L95)

  • Open a node: policy-marked event + trace.
  • Deny login: bounded denied event.
  • Make an audited change: SQL + local copy.
  • Match IDs; no cloud account needed.

- [Deferred deployment work](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/docs/security/observability/LOCAL-DEVELOPMENT.md#L144-L154)

  • Cloud adapter and durable cloud acceptance.
  • API middleware and abuse-defense context.
  • Production SQL access / retention / alerts.
  • No API or cloud integration in this slice.

## 9  DISCOVERY AND SAFETY: other instrumentation paths

- [Discovery execution: bounded observation](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/Atlas.Discovery/DiscoveryService.cs#L30-L36)

  • Validate query, then observe its execution.
  • No search terms or result-node lists.
  • Feed reads differ from node detail views.

- [Reusable duration and outcome wrapper](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/Atlas.Contracts/Observability/Telemetry.cs#L85-L104)

  • Observe starts a named Activity span.
  • Success records elapsed execution time.
  • Failure records bounded reason; rethrows.

- [Cache lookup observations](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/Atlas.Observability/LocalTelemetry.cs#L90-L102)

  • Record detail/page layer and hit/miss.
  • Count actual lookup deltas once.
  • No cache keys or node/user IDs in metrics.

- [Failures have one diagnostic owner](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/Atlas.Contracts/Observability/Telemetry.cs#L180-L201)

  • Deduplicate the same exception per scope.
  • Emit a bounded category, not its text.
  • A broken observer cannot change results.

- [Closed event shape is the boundary](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/Atlas.Observability/EventJson.cs#L18-L40)

  • Typed vocabulary; explicit JSON projection.
  • Domain property additions do not extend logs.
  • No arbitrary properties or request dumps.

- [Standard instrumentation, local view](https://github.com/williamowen65/atlas-the-public-think-tank/blob/0716efbc670fc202b9dfc5a81095cf09623ffbd4/src/Atlas.Observability/LocalTelemetry.cs#L26-L40)

  • .NET Meter exposes counters/histogram.
  • ActivitySource supports future subscribers.
  • JSONL examples are not universal OTLP.

## Suggested first local walkthrough

1. Pull this PR, use the Development launch profile and run the Console. Open the linked logs under **Atlas.Console → Logs → Observability**. See [launch and inspection instructions](../../../security/observability/LOCAL-DEVELOPMENT.md#visual-studio-run-button).

2. Open a node. Find `node.view_completed` in diagnostics. Confirm that `privacy_policy` is `read_no_identity_or_target`, the actor/target IDs are null, and the duration is present. Redisplaying the details can add another observation.

3. Copy that record's `observability_id`. Find the `NodeView` trace with the same ID; compare its `trace_id` and `span_id` with the diagnostic. Its parent span ID is zero because this read deliberately starts a separate trace. No actor-bearing parent link is retained.

4. Find `atlas.events.count` observations for `node.view_completed` in metrics. Sum the delta values for your chosen time window; do not add duration and count values together. Duration observations can be analyzed separately. The file contains observations, not precomputed rolling averages or percentiles.

5. Make a deliberately denied sign-in attempt. Find `identity.sign_in_completed` with outcome `denied`. Submitted credentials should not appear. A rejected attempt is a diagnostic event; it is not by itself a durable successful business audit.

6. Perform an important persisted change, such as a moderation transition using an authorized account. Find the diagnostic, the SQL `AuditRecords` row and the local audit copy. Match the original `event_id`; delivery can lag briefly. The delivery worker's own diagnostic uses a separate system operation and references that audit record as its target.

7. Inspect `atlas.audit.pending` over successive samples. Repeated zeroes mean the backlog is empty at those sample times; they are not delivery counts. A nonzero value that persists or grows merits investigation.

8. Use the linked tests in region 8 to review rollback and retry behavior without manually breaking your development database. Stop the app before deleting disposable local files. SQL audit history and delivered outbox state remain intact.

Older stored audit JSON and older log files may retain `observability_operation_id`. Newly serialized events use `observability_id`; historical payloads are deliberately not rewritten.

# Observability: choose a view

The original [linked review board](ObservabilityAndAudit.excalidraw) and [code walkthrough](ObservabilityAndAudit.md) remain unchanged.

## Follow the running system

Open [ObservabilityProcessFlow.excalidraw](ObservabilityProcessFlow.excalidraw). This is a process/activity view with decision branches, rather than a hierarchy of code-review cards.

Start with panel **0**, the simple model. Then zoom into:

1. **Host startup**: where writers, Meter instruments and ActivityListener are instantiated; how shutdown cleans them up.
2. **Measured workflow**: where operation context and spans begin; success versus failure; when spans stop and the previous context returns.
3. **SQL write**: which changes require audit; atomic save; confirmed commit versus failure or uncertainty; after-commit diagnostics.
4. **Audit delivery**: independent system operation; due-entry selection, conditional lease, delivery, acknowledgement, retry and cancellation.
5. **Signal fan-out and privacy**: where event counts, durations, cache observations and trace output are produced; the isolated NodeView scope.

Green marks a beginning/success, red an ending/failure, blue evidence or signal output, and amber diamonds decisions. Arrows show process/data flow; independent tasks are not a single chronological chain. Node titles link to implementation pinned to the source revision used to build these boards.

A useful exercise: follow a successful measured command through panel 2, then its event through panel 5. Next follow a moderation write through panel 3 and its later audit copy through panel 4. Finally compare the separate NodeView trace with the ordinary command scope.

## Understand responsibilities and patterns

Open [ObservabilityUml.excalidraw](ObservabilityUml.excalidraw). This is a focused UML class view, split into four regions:

- **Contracts and adapter**: Telemetry, TelemetryEvent, the two observer ports, and LocalTelemetry implementing both.
- **Scopes and instrumentation**: Context, Restore, ActivitySource/Activity, ActivityListener, Meter and numeric instruments.
- **Transactional outbox**: AtlasDataContext, AuditCapture, audit/outbox rows, AuditDelivery and its destination port/adapter.
- **Host composition and files**: ConsoleTelemetry ownership/lifetime responsibilities and JsonLineFile behavior.

Dashed open arrows indicate dependencies, dashed triangles interface realization, and solid arrows associations. Class boxes separate the type name from selected members. Ownership/lifetime details appear in the boxes; composition diamonds are deliberately not used for objects whose lifetimes are not uniformly owned. This is a focused explanation, not a generated inventory of every member or EF relationship.

The patterns to recognize are **ports and adapters**, an **ambient context with a disposable scope guard**, and the **transactional outbox**. One observer is installed in a flow; this is not a multicast event bus. ActivitySource and Meter are standard .NET instrumentation APIs.

## Accuracy boundaries

- Instruments are created at startup; observations are recorded later. Defining a counter does not increment it.
- Meter instruments and explicit local metric JSONL writes are separate outputs. Observable gauges expose values when a collector polls.
- Spans can stop without an event being recorded. With no interested listener, StartActivity can return null.
- Required audit rows join the business transaction; diagnostic/file output is best effort. Direct SQL business mutations bypass EF transition capture.
- NodeView clears identity, target and parent-trace linkage, then restores the enclosing context.
- SQL remains authoritative. Audit files are inspection copies; local acknowledgement is separate from cloud acceptance. A cloud destination adapter remains deferred.
- Duplicate delivery is possible after a crash or lost acknowledgement; the event ID remains stable. No exactly-once guarantee is implied.

All diagram elements remain editable in Excalidraw. Exported previews are not committed.

# Architecture blackboards

These blackboards are exploratory visual companions to Atlas's authoritative
Markdown documentation. We try to update them alongside behavior changes; they can still drift, so check their linked code and context map when reviewing.

The [Node creation board](workflows/Nodes/NodeCreation.excalidraw) restores the custom 213-element layout from before the PTT-87 replacement (commit `0fa7943`). PTT-121 updates its labels and links for SQL, pre-write construction, topology coordination, rollback and post-commit events while preserving the original geometry and arrow bindings.

- [Blackboard conventions](CONVENTIONS.md) describe the shared layout,
  editability, connection, traceability, and visual-language rules to apply to
  future workflow boards.

- [PTT-130 transaction flow](workflows/Persistence/PTT-130-TRANSACTION-FLOW.excalidraw) shows how reference checks and Content/Graph saves share one EF context and SQL transaction, then commit or roll back together. See the [code walkthrough](workflows/Persistence/PTT-130-TRANSACTION-FLOW.md).
- [System Blackboard](SYSTEM-BLACKBOARD.excalidraw) is the single root-level system overview: hosts, domains, Identity, shared infrastructure, persistence, events and remaining boundaries. Detailed workflows live under their subject folders.
- [Reusable Node Tags](workflows/Node%20Tags/NodeTags.excalidraw) maps tag
  definition reuse, node-specific application, mutation authority, author
  disposition, lifecycle audit history, persistence, display, and the future
  Voting handoff.
- [Content Block Composition](workflows/Content/ContentBlocks.excalidraw) is a
  dashboard of Content ownership, block creation and validation, ordered document
  composition, type-specific payloads, SQL Server persistence, and reconstitution.
- [Node Tag Voting](workflows/Voting/NodeTagVoting.excalidraw) maps PTT-84's
  console voting interaction, NodeTag target and -1/+1 value types, cumulative
  score aggregation, cross-domain eligibility checks, SQL Server persistence, and
  verification tests.
- [Comments Domain](workflows/Comments/CommentsDomain.excalidraw) maps comment
  ownership, top-level creation and replies, edit and soft-removal permissions,
  threaded reads, Console navigation, target availability, and deferred
  integrations.
- [Moderation Domain](workflows/Moderation/ModerationDomain.excalidraw) maps
  Node reporting, grouped moderator decisions, hidden visibility, author edits,
  review requests, restoration, stored history, and future integration points.
- [In-Memory Event Bus](workflows/System/InMemoryEventBus.excalidraw) shows the
  console host subscriptions, publication paths, exact-type lookup, synchronous
  handler calls, and failure behavior.
- [Notifications Domain](workflows/Notifications/NotificationsDomain.excalidraw) traces
  completed comment and moderation actions through the event bus, `Handle`,
  recipient preferences, SQL Server persistence, simulated delivery, and feed actions.
- [The current-state context map](../architecture/CONTEXT-MAP.md) remains the
  authoritative inventory when the blackboard and written documentation differ.

- [Discovery reads, pagination and caching](workflows/Discovery/DiscoveryReadFlow.excalidraw) walks through EF queries, remaining SQL helpers, page keys and transaction-backed revisions. See the [code walkthrough](workflows/Discovery/DiscoveryReadFlow.md).

- [Observability and durable audit](workflows/Observability/ObservabilityAndAudit.excalidraw) provides a numbered review route through host wiring, four JSONL streams, SQL atomicity, outbox recovery, node-view privacy, correlation, metrics, local retention, tests and discovery instrumentation. Each card links to specific code lines. See the [linked review map](workflows/Observability/ObservabilityAndAudit.md).

- [Observability process flow](workflows/Observability/ObservabilityProcessFlow.excalidraw) follows startup, scope boundaries, decisions, signal creation and audit delivery. [Observability UML](workflows/Observability/ObservabilityUml.excalidraw) explains responsibilities and patterns. See the [diagram guide](workflows/Observability/DIAGRAMS.md).

## Legend

- Solid borders and arrows represent implemented components or interactions.
- Dashed borders and arrows represent candidates, gaps, or unresolved questions.
- Candidate-domain notes record discoveries only; they do not design or approve
  future boundaries.

Open the `.excalidraw` files in Excalidraw to pan, zoom, and edit each board.
Keep related areas on the same canvas unless the board becomes too crowded to
review.

System Blackboard is intentionally high-level. Use the subject workflow boards for operation details, transaction mechanics and test evidence.

## Viewing and reviewing

The native `.excalidraw` file is the diagram deliverable. Download it and open it in Excalidraw (or an Excalidraw editor extension). Review edits in Excalidraw; static previews and PDF exports do not replace that review. Exports are provided only when explicitly requested. See [conventions](CONVENTIONS.md).

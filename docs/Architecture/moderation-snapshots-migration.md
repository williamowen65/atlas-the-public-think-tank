# Moderation snapshots and audit events — replacement for temporal history

## Scope
Replace the moderation dependency on SQL Server system-versioned temporal tables with a bounded, case-linked reported-content snapshot and a separate append-only audit event log. Preserve the existing moderation UI and `GET /api/moderation/nodes/{id}/revisions` response shape.

## Invariants
- A report captures **the content at report submission**, not the content at moderator decision or the last edit. Store title, short description, and the complete ordered content-block payload needed to reconstruct the report. Do not reconstruct from a node's current document or temporal periods.
- The reported snapshot is immutable and associated with its report/case ID. A later report on the same node must not overwrite earlier evidence.
- The revised side of the comparison is the **current** node/document content. Routine author saves do not create moderation snapshots or audit events.
- Report submission and snapshot persistence are atomic. If capture fails, the report must not be accepted without evidence.
- Moderator-only comparison and history endpoints must not expose reporter identities through author-facing APIs.
- Audit events are append-only, recording actor, action, time, case/node reference, rationale when relevant. Capture report, decision, review request, and restoration events, including repeated review requests. Never rewrite prior events.
- Use a storage limit for snapshot payloads and existing report submission quotas; do not store a new snapshot for each author edit.
- Avoid synthesizing evidence for historical cases whose exact reported content is unavailable.

## Schema / migration
1. Add `ModerationReportedSnapshotRows` with unique `ModerationCaseId`, `NodeId`, `CapturedAt`, `Title`, `ShortDescription`, and a versioned JSON payload of ordered content blocks. Foreign-key to the case and node.
2. Add `ModerationAuditEventRows` with immutable event ID, case/node ID, actor participant ID, event kind, occurred-at, and optional reason/detail; index by node and occurrence.
3. Remove `IsTemporal()` from `NodeRows`, `DocumentRows`, `BlockRows`, and `DocumentBlocks` **only after checking all remaining usages**. EF migration must disable SQL Server system versioning safely, remove generated period columns/history tables where appropriate, and preserve current rows. Update the EF model snapshot.
4. Migrate existing cases conservatively: retain case metadata, mark any missing reported snapshot as unavailable; never use the latest prior revision as purported evidence. Disposable demo data can be reseeded.

## Integration
- Report endpoint: capture the full content and create the report and snapshot within the same database transaction / operation boundary; preserve report quota enforcement.
- Revisions endpoint: resolve the relevant hidden case's snapshot and current content. If multiple cases exist, select the case being investigated explicitly or deterministically expose the relevant cases; never silently merge their reported versions.
- Existing author revision and request-review behavior remains unchanged.
- Seed both review-request fixtures with snapshots, plus Demo User 04's other hidden cases. No temporal seeding edits are needed after migration.

## Regression tests
- Report snapshot matches submission-time content, including ordered blocks, after multiple author saves.
- Two reports on one node retain independent snapshots.
- Failed snapshot persistence rolls back the report.
- Moderator revisions endpoint returns immutable reported content + current content, with missing-snapshot behavior explicit.
- Author/nonmoderator cannot read moderator-only snapshots or reporter identities.
- Repeated author edits do not grow snapshot count; review requests and restoration produce audit events.
- Existing queue, hidden visibility, authored listing, and search security tests remain passing.
- Migration upgrades a populated database without losing current node/document data; fresh database seed succeeds.

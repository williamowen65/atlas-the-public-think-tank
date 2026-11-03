# Moderation snapshots and audit events

> **Proposed migration — not yet implemented.** This plan would replace moderation's dependence on SQL Server temporal history with immutable, report-specific content snapshots and append-only audit events. It is not an instruction to drop existing history tables today.

## Why change the storage model?

A moderator needs to compare **what was reported** with **what the author has written now**. Historical row versions alone do not reliably identify the exact content visible when a report was submitted.

The proposed design keeps two different kinds of records:

| Record | Purpose | When it changes |
|---|---|---|
| Reported-content snapshot | Preserves the node as it appeared when a specific report was submitted | Never; each report gets its own snapshot |
| Audit event | Records who performed a moderation action, what happened, and when | Never; new actions append new events |
| Current node and document | Shows the author's latest content for comparison | Changes with normal author edits |

The existing moderation UI and `GET /api/moderation/nodes/{id}/revisions` response shape must remain compatible.

## Rules that must remain true

### Report-time evidence

- Capture the title, short description, and **all ordered content blocks at report submission**—not at the moderator's decision or the most recent edit.
- Associate the immutable snapshot with its own report/case ID. Two reports about one node retain separate evidence.
- Save the report and snapshot atomically. If evidence capture fails, reject and roll back the report.
- Enforce a snapshot-payload size limit and existing report-submission quotas.

### Comparisons and privacy

- Compare the frozen reported snapshot with the **current** node/document.
- Normal author edits do not create additional moderation snapshots or audit events.
- Do not expose reporter identities through author-facing APIs. Snapshot and comparison endpoints intended for moderators remain restricted.
- If a historical case lacks its actual submission-time content, mark its snapshot **unavailable**. Never present an inferred earlier revision as exact evidence.

### Audit trail

Append events for report submission, decisions, author review requests (including repeated requests), and restoration. Each event records:

- Event ID, action type, and occurrence time
- Actor's Participant ID and case/node reference
- Rationale or other details when relevant

Prior events must never be rewritten.

## Proposed SQL model

| Table | Required data | Constraints |
|---|---|---|
| `ModerationReportedSnapshotRows` | `ModerationCaseId`, `NodeId`, `CapturedAt`, `Title`, `ShortDescription`, versioned JSON of ordered content blocks | Unique case ID; foreign keys to case and node |
| `ModerationAuditEventRows` | Immutable event ID, case/node reference, actor Participant ID, event kind, occurred-at, optional reason/detail | Index by node and occurrence |

## Migration sequence

1. **Add the new tables.** Generate and review the EF migration and updated model snapshot.
2. **Wire report submission.** Capture all reported content and save it with the case in the same transaction/operation boundary.
3. **Update comparisons.** Resolve the relevant hidden case's snapshot alongside current content. For multiple cases, select the case explicitly or present the relevant cases deterministically; never merge their reported versions silently.
4. **Handle existing cases.** Preserve case metadata. Flag unavailable snapshots rather than manufacturing evidence. Disposable demo data may be reseeded.
5. **Remove temporal history only after a usage audit.** Check every remaining dependency on `IsTemporal()` for `NodeRows`, `DocumentRows`, `BlockRows`, and `DocumentBlocks`. Safely disable SQL Server versioning and remove period columns/history tables where appropriate **without losing current rows**.
6. **Refresh seed fixtures.** Supply snapshots for both review-request fixtures and Demo User 04's other hidden cases. Temporal seeding edits will no longer be needed after migration.

Keep existing author-edit and request-review behavior intact throughout the change.

## Verification checklist

- [ ] Submission-time title, short description, and ordered blocks remain unchanged after several author edits.
- [ ] Two reports about one node retain separate snapshots.
- [ ] A failed snapshot write rolls back report submission.
- [ ] The revisions endpoint returns reported and current content, with a clear unavailable-snapshot response where needed.
- [ ] Authors and nonmoderators cannot read restricted snapshots or reporter identities.
- [ ] Author saves do not increase snapshot counts; review requests and restorations append audit events.
- [ ] Queue, hidden-content visibility, authored listings, and search-security tests still pass.
- [ ] Migration preserves current data in an existing populated database.
- [ ] Fresh setup and demo seeding succeed.

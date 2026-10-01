# Atlas test strategy

## Evidence by layer

| Layer | Evidence |
|---|---|
| Domain unit | Invariants, values, lifecycle, authorization, and deterministic summaries |
| Repository integration | SQL Server save/reload/change/delete, ordered children, and domain reconstitution |
| Database integrity | Unique keys, missing-reference rejection, restricted deletion, owned-child cascade |
| Migration | Apply checked-in migrations, upgrade existing relational rows, check pending model changes |
| Boundary/host | Creation, moderation visibility, recipient notifications, discovery, and Console navigation |
| Contract | Versioned event payload compatibility; broader compatibility coverage remains future work |
| Performance | Realistic query/load targets once a network API exists |

Atlas.Persistence.Tests uses isolated SQL Server databases through SqlTestDatabase. GitHub Actions starts SQL Server and supplies ATLAS_SQL_TEST_CONNECTION_STRING. Read the helper/workflow for actual configuration; do not substitute EF InMemory or SQLite when verifying SQL Server constraints.

## Verification rules

A requirement is Verified only when linked automated evidence supports its criteria. Manual Console demonstrations support Implemented status. Do not infer workflow atomicity or conflict recovery from unique-index coverage.

## Useful follow-up coverage

- Conflicting updates to the same record and losing first-insert recovery.
- Multi-save failures and orphan cleanup.
- Reusable graph-wide cycle policy.
- Durable event publication and external delivery recovery.
- Production identity and protected lifecycle operations.
- Query performance for materialized Discovery/ancestry reads.

Run the affected projects and SQL integration suite when behavior changes. Documentation-only updates require source-link, stale-reference, diagram-structure, and layout verification rather than new behavior tests.

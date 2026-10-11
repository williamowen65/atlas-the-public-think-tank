# ADR-0005: Enforce concurrency invariants at persistence boundaries

Status: Accepted for uniqueness, coordinated SQL operations and topology locking; general stale-edit/reliable-delivery policies remain open
Updated: 2026-10-11

## Context and decision

Process-local locks cannot enforce invariants across app instances. Domain-owned repository contracts describe required behavior; SQL Server constraints enforce configured uniqueness across processes.

Atlas currently shares one AtlasDataContext and migration set in Atlas.Persistence. This is infrastructure reuse, not a transfer of domain behavior or a promise of separate databases per domain.

## Current enforcement

- Votes: unique (ParticipantId, TargetType, TargetId).
- Notifications: unique (OccurrenceId, RecipientParticipantId).
- Preferences: primary key ParticipantId.
- Communities: unique Name under SQL collation.
- Memberships and community-node links: composite ID-pair primary keys.
- Ordered children: composite owner-ID/Position primary keys.

Standalone saves are transactional. Calls within [OperationBoundary](../../../src/Atlas.Contracts/Operations/OperationBoundary.cs) share the SQL adapter's operation context and serializable transaction. Node creation, community/owner creation, reaction replacement and grouped moderation commit or roll back together.

Node creation and parent changes use [NodeTopologyBoundary](../../../src/Atlas.Graph/Nodes/NodeTopologyBoundary.cs) and the database-scoped AtlasGraphTopology lock. A per-node lock is insufficient for cycles formed by changes across several nodes. Metadata edits use SaveMetadata, preserve stored parents and do not request the graph lock. Other domains rely on transactions and constraints unless a specific invariant requires named coordination.

SQL tests cover constraints, upgrades, real document-insert failure rollback, competing topology writes and independent metadata/Content/vote writes while the graph lock is held. See [implementation evidence](../../security/validation/IMPLEMENTATION.md#workflows-and-evidence).

## Remaining limits

- Expected SQL deadlocks, uniqueness/constraint failures and lock timeouts become OperationConflictException. They are not automatically retried or converted into a successful upsert.
- No universal rowversion check rejects every stale edit. Some relationship workflows check current state; broader version policy remains separate work.
- The boundary covers sequential cooperating calls in one database. Direct SQL/DbContext writes can skip application validation and topology coordination.
- Rollback restores SQL data, not caller objects; reload before retrying.
- Event publication and external delivery remain outside the SQL transaction. Durable outbox/reliable delivery is deferred.

See [Voting](../VOTING.md), [Data ownership](../DATA-OWNERSHIP.md), and [SQL setup](../../PTT-87-SQL-Server.md).

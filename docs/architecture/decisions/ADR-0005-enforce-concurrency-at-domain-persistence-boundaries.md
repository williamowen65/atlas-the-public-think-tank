# ADR-0005: Enforce concurrency invariants at persistence boundaries

Status: Accepted for implemented uniqueness; broader conflict handling remains open  
Updated: 2026-09-30

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

Each repository save and owned-child replacement is transactional. SQL integration tests verify uniqueness, reference constraints, deletion behavior, and upgrades.

## Remaining limits

A uniqueness error is not automatically converted to a successful create-or-update retry. Existing-record conflicts use last-write behavior; there is no rowversion. Multi-save domain workflows and event publication do not share one transaction. Future policies must address recovery, conflict detection, and reliable delivery explicitly.

See [Voting](../VOTING.md), [Data ownership](../DATA-OWNERSHIP.md), and [SQL setup](../../PTT-87-SQL-Server.md).

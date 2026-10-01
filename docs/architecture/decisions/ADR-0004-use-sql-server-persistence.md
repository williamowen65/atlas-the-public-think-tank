# ADR-0004: Use SQL Server persistence through EF Core row adapters

Status: Accepted  
Updated: 2026-09-30  
Related requirements: [CON-002](../../requirements/REQUIREMENTS.md#con-002), [PER-001](../../requirements/REQUIREMENTS.md#per-001), [PER-002](../../requirements/REQUIREMENTS.md#per-002)

## Context

Atlas needs durable relational integrity and cross-process uniqueness while keeping its domain models independent of the database provider. This record describes the current PTT-87 implementation and replaces the earlier prototype storage decision.

## Decision

Use SQL Server with EF Core. Atlas.Persistence owns a shared AtlasDataContext, explicit row mappings, and versioned migrations. Console supplies Sql*Repository adapters implementing domain-owned repository interfaces. Domains generate aggregate IDs and enforce business behavior.

Normalize ordered references, audits, and delivery attempts into child tables. Use database primary keys, configured foreign keys, and unique vote/notification/community constraints as the final integrity boundary.

## Consequences

- Domain objects remain independent of EF Core and are explicitly reconstituted from rows.
- Each repository save updates one addressed record plus owned children in a transaction.
- Multi-save workflows and event publication need separate consistency design.
- Polymorphic targets are outside ordinary foreign-key enforcement.
- SQL integration tests exercise actual SQL Server constraints and checked-in migrations.
- Splitting contexts or deploying independent services would require a deliberate later migration.

See [SQL setup](../../PTT-87-SQL-Server.md) and [data ownership](../DATA-OWNERSHIP.md).

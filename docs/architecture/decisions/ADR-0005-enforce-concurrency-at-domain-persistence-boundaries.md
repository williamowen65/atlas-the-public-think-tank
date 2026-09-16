# ADR-0005: Enforce concurrency invariants at each bounded context's persistence boundary

Status: Proposed  
Date: 2026-09-16  
Related requirements: [VOT-004](../../requirements/REQUIREMENTS.md#vot-004), [VOT-010](../../requirements/REQUIREMENTS.md#vot-010), [PER-001](../../requirements/REQUIREMENTS.md#per-001), [PER-002](../../requirements/REQUIREMENTS.md#per-002)  
Related work: [PTT-83](https://thepublicthinktank.atlassian.net/browse/PTT-83), [PTT-86](https://thepublicthinktank.atlassian.net/browse/PTT-86), [PTT-87](https://thepublicthinktank.atlassian.net/browse/PTT-87)

## Context

Concurrent requests can read the same state and produce a result that violates a domain rule. An in-process `lock` cannot prevent this when requests run in different processes or on different servers.

## Decision

Each bounded context owns its concurrency rules. Its production persistence adapter must enforce each complete state change atomically.

Each bounded context also owns its persistence model and migrations. Sharing a physical database does not mean sharing a central `DbContext`.

The database provider and exact enforcement mechanism will be decided in PTT-87, using Voting as the first implementation.

## Consequences

- Concurrency rules remain owned by the relevant domain.
- Production adapters must enforce those rules across processes.
- Provider-backed integration tests must verify concurrent behavior.
- JSON adapters cannot demonstrate cross-process concurrency safety.

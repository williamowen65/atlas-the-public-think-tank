# ADR-0005: Enforce concurrency invariants at each bounded context's persistence boundary

Status: Proposed  
Date: 2026-09-16  
Related requirements: [VOT-004](../../requirements/REQUIREMENTS.md#vot-004), [VOT-010](../../requirements/REQUIREMENTS.md#vot-010), [PER-001](../../requirements/REQUIREMENTS.md#per-001), [PER-002](../../requirements/REQUIREMENTS.md#per-002)  
Related work: [PTT-83](https://thepublicthinktank.atlassian.net/browse/PTT-83), [PTT-86](https://thepublicthinktank.atlassian.net/browse/PTT-86), [PTT-87](https://thepublicthinktank.atlassian.net/browse/PTT-87)

## Context

A domain operation such as casting a vote is a read-decide-write operation. Two requests can read the same state, make individually valid decisions, and then write a combined result that violates a domain invariant.

A process-local `lock` can serialize callers only when they share the same lock object in the same process. It cannot coordinate horizontally scaled service instances, independently hosted processes, or other writers that reach the same durable state.

The domain still owns the invariant: for example, Voting decides what constitutes one current vote and how a recast behaves. Infrastructure must enforce that rule atomically for every writer that shares the state.

## Decision

Each bounded context owns its concurrency invariants and the persistence boundary that enforces them.

- The domain defines the required behavior through its model and ports. Callers should invoke an intent-level operation rather than reproduce a multi-step read/write protocol.
- A production persistence adapter performs the complete state transition atomically using transactions, uniqueness constraints, optimistic concurrency, atomic upsert, or an equivalent provider capability.
- Each bounded context owns its persistence model, mappings, schema or other namespace, migrations, and migration history. A shared physical database does not create a shared cross-domain data model or a central application `DbContext`.
- Persistence conflicts are translated into outcomes meaningful to the owning domain rather than leaking provider-specific exceptions through the application.
- In-process coordination may be used as a local optimization or to make a non-production adapter internally safe, but it is not evidence of cross-process concurrency correctness.
- Concurrency requirements that depend on shared durable enforcement remain partial until tested against the production-class persistence adapter.

The relational provider and exact concurrency mechanism are deliberately deferred to PTT-87. Voting will be the first implementation used to validate the pattern before it is repeated in other bounded contexts.

## Consequences

- Domain ownership remains explicit while enforcement occurs at the only boundary shared by all service instances.
- Horizontal and vertical scaling do not rely on singleton application objects or static locks.
- Every production adapter must provide an atomic implementation of its domain-owned state transitions.
- Integration tests must exercise separate application/repository instances against the same durable store and verify both the final state and the reported outcomes.
- JSON adapters remain useful for prototyping and local inspection, as established by ADR-0004, but cannot by themselves satisfy cross-process concurrency requirements.
- Database-specific design and operational work increases: migrations, constraints, transaction behavior, conflict mapping, and provider-backed tests must be maintained per bounded context.
- Requirements documentation must distinguish locally demonstrated safety from durable, cross-instance enforcement.

## Alternatives considered

### Process-local locks

They can prevent races among callers that share a process and a gate, but cannot coordinate independent processes or servers. Keyed locks also introduce lifetime and cleanup concerns. They are not the system-wide correctness boundary.

### A central persistence domain

A single shared data model or application `DbContext` could centralize enforcement, but it would couple bounded contexts and move ownership away from the domain whose invariant is being protected.

### Caller-managed read/write sequences

Requiring every use case to remember a particular lock, transaction, or repository call order makes correctness optional and easy to bypass.

### Database enforcement without domain-owned contracts

Constraints are an essential final safeguard, but treating concurrency solely as a database concern would hide the business meaning of conflicts and weaken independent domain tests.

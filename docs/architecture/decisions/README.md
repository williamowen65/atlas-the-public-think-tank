# Architecture Decision Records

Architecture Decision Records explain why Atlas adopted a consequential design. They complement requirements: a requirement says what must be true, while an ADR explains why a design was chosen to satisfy constraints.

## Index

| ADR | Decision | Status |
|---|---|---|
| [ADR-0001](ADR-0001-use-bounded-contexts.md) | Use bounded contexts inside the solution | Accepted |
| [ADR-0002](ADR-0002-reference-boundaries-by-id.md) | Reference foreign resources by identifier | Accepted |
| [ADR-0003](ADR-0003-use-versioned-integration-contracts.md) | Use versioned integration contracts | Accepted |
| [ADR-0004](ADR-0004-start-with-json-persistence.md) | Start with JSON persistence adapters | Accepted |

## Identifier and status rules

ADR numbers are permanent and never reused. A superseded ADR remains in the repository and links to the decision that replaced it.

Statuses are:

- **Proposed** — under discussion.
- **Accepted** — the current architectural direction.
- **Superseded** — replaced by a later ADR.
- **Rejected** — considered but not selected.
- **Deprecated** — still present but scheduled for removal.

## Template

```markdown
# ADR-NNNN: Decision title

Status: Proposed
Date: YYYY-MM-DD
Related requirements: [ID](../../requirements/REQUIREMENTS.md#id)

## Context

What forces, constraints, and problem made a decision necessary?

## Decision

What direction will Atlas take?

## Consequences

What becomes easier, harder, required, or deliberately deferred?

## Alternatives considered

What credible alternatives were evaluated?
```

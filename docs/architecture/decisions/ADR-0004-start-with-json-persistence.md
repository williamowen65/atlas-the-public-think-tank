# ADR-0004: Start with JSON persistence adapters

Status: Accepted  
Date: 2026-09-07  
Related requirements: [CON-002](../../requirements/REQUIREMENTS.md#con-002), [PER-001](../../requirements/REQUIREMENTS.md#per-001), [PER-002](../../requirements/REQUIREMENTS.md#per-002)

## Context

Atlas needs persistent, inspectable state while its domain boundaries and workflows are still being learned. Introducing databases, migrations, distributed messaging, and service hosting at the same time would obscure the domain experiments.

## Decision

The Console prototype will use JSON repository adapters under `data/`. Each boundary's records remain in a separate file, and repository interfaces remain owned by the appropriate domain project.

JSON is prototype infrastructure, not part of the domain model.

## Consequences

- State can be inspected and edited during development.
- Boundary ownership is visible in separate files.
- Repositories can later be replaced without changing core entities.
- Concurrency, transactions, indexing, and high-volume performance are intentionally limited.
- Legacy-shape migration must be handled explicitly.

## Alternatives considered

An immediate relational database would offer transactions and querying but increase setup and migration cost. Pure in-memory storage would be simpler but would not demonstrate reconstitution or compatibility.

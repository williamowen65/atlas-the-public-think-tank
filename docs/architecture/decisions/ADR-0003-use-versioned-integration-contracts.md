# ADR-0003: Use versioned integration contracts

Status: Accepted  
Date: 2026-09-07  
Related requirements: [EVT-001](../../requirements/REQUIREMENTS.md#evt-001), [EVT-002](../../requirements/REQUIREMENTS.md#evt-002), [NFR-001](../../requirements/REQUIREMENTS.md#nfr-001)

## Context

Graph events may be consumed by Content and future Search, Activity, Notifications, or Analytics capabilities. Consumers need a stable payload definition without importing Graph entities.

## Decision

Published payloads will be defined once in Atlas.Contracts and versioned explicitly, beginning with names such as `NodeCreatedV1`. Producers and consumers reference the same contract definition in the modular application.

Contracts contain communication data and primitive wire values, not domain behavior. A breaking payload change creates a new version.

## Consequences

- Payload shape has a discoverable source of truth.
- Producer and consumer compatibility is visible at compile time today.
- Contract packages must be managed carefully if services deploy independently.
- Internal domain changes need not become contract changes.
- Supporting multiple versions may be necessary during migrations.

## Alternatives considered

Duplicated producer and consumer DTOs would reduce package coupling but introduce mapping and drift. Publishing Graph entities would expose internal design and create stronger coupling.

# Atlas architecture

Atlas is currently a modular application hosted by Atlas.Console. Its projects are treated as bounded contexts so they can evolve toward independently deployed services without sharing domain entities.

## Documents

- [Context map](CONTEXT-MAP.md) describes the boundaries and their relationships.
- [Data ownership](DATA-OWNERSHIP.md) identifies the authoritative owner of records and identifiers.
- [Architecture decisions](decisions/README.md) preserve the reasoning behind consequential choices.

## Architectural principles

- A boundary owns its model, invariants, lifecycle, and identifiers.
- Other boundaries refer to owned resources by stable identifiers.
- The host composes workflows and views; it does not become the owner of domain rules.
- Published contracts are versioned and contain communication data, not domain entities.
- Authorization is enforced at an application use-case boundary even when a UI also hides unavailable actions.
- The current JSON adapters and in-memory event publisher are prototype infrastructure, not permanent deployment constraints.

## Current versus future topology

Today, Atlas.Console references the boundary projects directly and provides persistence adapters and event dispatch. This makes boundary interactions observable without requiring distributed infrastructure.

A future web application may first retain the same modular-monolith shape. If a boundary later becomes an independent service, its public application API and integration contracts form the migration seam.

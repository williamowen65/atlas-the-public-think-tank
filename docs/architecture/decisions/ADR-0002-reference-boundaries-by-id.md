# ADR-0002: Reference foreign resources by identifier

Status: Accepted  
Date: 2026-09-07  
Related requirements: [GRA-004](../../requirements/REQUIREMENTS.md#gra-004), [GRA-005](../../requirements/REQUIREMENTS.md#gra-005), [CON-001](../../requirements/REQUIREMENTS.md#con-001)

## Context

A node needs an author and description, but Participant and Document lifecycles belong to other boundaries. Storing those entities inside Graph would transfer ownership accidentally.

## Decision

A boundary stores only an opaque identifier for a resource owned by another boundary. The owning boundary generates the identifier.

Graph stores `AuthorId` and `DescriptionId`; it does not store Participant or Document objects. The application host obtains and composes the records when presenting a complete view.

## Consequences

- Domain entities are not physically duplicated across boundaries.
- Graph can exist without loading profiles or documents.
- References may temporarily be unresolved.
- Cross-boundary existence checks and cleanup require explicit policies.
- Composition moves to the application/API layer.

## Alternatives considered

Sharing entity classes would be convenient in-process but couple lifecycles and deployments. Asking Graph to generate Content identifiers would violate Content ownership.

# ADR-0001: Use bounded contexts inside the solution

Status: Accepted  
Date: 2026-09-07  
Related requirement: [NFR-001](../../requirements/REQUIREMENTS.md#nfr-001)

## Context

Atlas contains graph structure, rich content, participants, authorization, and future voting behavior. Treating these as one shared domain model would make ownership unclear and make later service extraction expensive.

## Decision

Atlas will organize domain behavior into explicit bounded contexts. The current application may deploy as one process, but Graph, Content, Participants, Contracts, and future Voting code will retain separate ownership and public surfaces.

Atlas.Console is a composition host and learning harness. It may connect boundaries, but it does not become the owner of their rules.

## Consequences

- The model can use precise language within each context.
- Cross-boundary workflows require explicit coordination.
- Some concepts use different local types even when their wire value is the same GUID.
- UI read models may combine data from several boundaries.
- Independent deployment remains possible but is not mandatory.

## Alternatives considered

A single domain project would reduce early ceremony but encourage direct entity sharing. Creating independently deployed services immediately would add operational complexity before Atlas needs it.

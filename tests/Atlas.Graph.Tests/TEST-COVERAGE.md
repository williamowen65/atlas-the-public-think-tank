# Atlas.Graph test coverage

This document is a behavior-oriented coverage map for the Graph boundary. It complements the test runner: the runner shows whether tests pass, while this file records which domain rules are protected and which decisions remain open.

Status meanings:

- **Covered** — an automated test currently protects the behavior.
- **Partial** — related behavior is tested, but an important case remains.
- **Pending design** — the domain rule must be decided or requires a graph-aware collaborator.
- **Infrastructure** — belongs in repository or integration tests rather than the `Node` unit suite.

## Node aggregate

| Area | Behavior | Status | Test location or next action |
|---|---|---:|---|
| Creation | Generates a non-empty node ID | Covered | `NodeConstructionTests` |
| Creation | Stores title, description ID, type ID, and author ID | Covered | `NodeTests`, `NodeConstructionTests` |
| Creation | Begins active with matching creation/update timestamps | Covered | `NodeConstructionTests` |
| Creation | Begins without parents when none are supplied | Covered | `NodeConstructionTests` |
| Creation | Accepts and deduplicates requested sub-node types | Covered | `NodeTests`, `NodeConstructionTests` |
| Creation | Rejects a null requested-type collection | Covered | `NodeConstructionTests` |
| Creation event | Emits one complete `NodeCreatedV1` message | Covered | `NodeConstructionTests` |
| Reconstitution | Restores all persisted state | Covered | `NodeReconstitutionTests` |
| Reconstitution | Does not emit creation events | Covered | `NodeReconstitutionTests` |
| Reconstitution | Rejects updated time before created time | Covered | `NodeReconstitutionTests` |
| Reconstitution | Deduplicates requested types and parents | Covered | `NodeReconstitutionTests` |
| Reconstitution | Rejects null collections and invalid parent IDs | Covered | `NodeReconstitutionTests` |
| Rename | Changes title and update timestamp | Covered | `NodeTests`, `NodeMutationTests` |
| Rename | Same title is a no-op | Covered | `NodeMutationTests` |
| Type | Changes type and update timestamp | Covered | `NodeMutationTests` |
| Type | Same type is a no-op | Covered | `NodeMutationTests` |
| Description | Replaces the external description reference | Covered | `NodeTests`, `NodeMutationTests` |
| Description | Same reference is a no-op | Covered | `NodeMutationTests` |
| Requested types | Adds and removes requested types | Covered | `NodeTests`, `NodeMutationTests` |
| Requested types | Duplicate add and missing remove are no-ops | Covered | `NodeTests`, `NodeMutationTests` |
| Parents | Attaches multiple distinct parents | Covered | `NodeTests` |
| Parents | Rejects empty IDs and self-parenting | Covered | `NodeTests`, `NodeParentBehaviorTests` |
| Parents | Duplicate attach and missing detach are no-ops | Covered | `NodeTests`, `NodeParentBehaviorTests` |
| Parent events | Attach/detach messages contain all contract fields | Covered | `NodeParentBehaviorTests` |
| Archive | Changes status, timestamp, and emits one event | Covered | `NodeTests`, `NodeLifecycleBehaviorTests` |
| Archive | Repeated archive is a no-op | Covered | `NodeTests`, `NodeLifecycleBehaviorTests` |
| Restore | Changes status, timestamp, and emits one event | Covered | `NodeLifecycleBehaviorTests` |
| Restore | Restoring an active node is a no-op | Covered | `NodeLifecycleBehaviorTests` |
| Event buffer | Clearing removes recorded events | Covered | `NodeConstructionTests` |

## Value objects

| Type | Protected behavior | Status |
|---|---|---:|
| `NodeTitle` | Required, trimmed, maximum 200 characters, useful string representation | Covered |
| `NodeDescriptionId` | Empty GUID rejected | Covered |
| `NodeAuthorId` | Empty GUID rejected | Covered |
| `RequestedSubNodeType` | Empty type GUID rejected | Covered |
| `NodeId` | Empty GUID policy | Pending design |
| `NodeTypeId` | Empty GUID policy outside requested-type usage | Pending design |

## Rules that should not be forced into one Node

| Rule | Why it is not a Node-only test | Recommended owner |
|---|---|---|
| Prevent indirect cycles such as A → B → C → A | One node cannot inspect the rest of the graph | Graph application service or graph traversal policy |
| Confirm that a parent node exists | Requires repository knowledge | Graph application service |
| Confirm that a node type exists and is active | Requires the type repository | Graph application service |
| Require multiple parents only for Relationship nodes | Needs a finalized product rule and type semantics | Pending domain decision |
| Control who may attach or detach a parent | Requires participant and authorization information | Application policy |
| Prevent timestamps moving backward during mutations | Current aggregate permits it | Pending domain decision |

## Repository and workflow coverage

These should be tested separately from the aggregate:

- JSON round trips preserve requested type IDs, parent IDs, author IDs, and description IDs.
- Legacy records are migrated or defaulted intentionally.
- Missing referenced documents, participants, node types, and parents are handled predictably.
- Publishing occurs after a successful node save.
- Subscribers receive each integration event once.
- A failed subscriber has an explicit retry or failure policy.
- Concurrent writes do not silently overwrite changes.

## Maintaining this map

When a domain rule is introduced:

1. Add or update the rule in this document.
2. Put the test in the file matching the behavior: construction, mutation, lifecycle, parents, reconstitution, or value objects.
3. Prefer one observable behavior per test.
4. Keep test setup in `NodeTestFactory` when it is incidental to the behavior.
5. Move a row from **Pending design** only after the rule and its owner are explicit.

Line coverage can be added later with a coverage collector, but high line coverage is not a substitute for this behavior map.

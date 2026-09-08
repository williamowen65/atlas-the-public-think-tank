# Data ownership and consistency

This document is the detailed record-level ownership register. The [current-state context map](CONTEXT-MAP.md) is authoritative for boundary responsibilities, relationships, event flow, and boundary-level gaps. This register should be updated before adding a new cross-boundary write.

## Ownership table

| Data | Authority | ID generator | Current storage | Referenced by |
|---|---|---|---|---|
| Node | Graph | Graph | `data/nodes.json` | Console and future consumers |
| Node type | Graph | Graph | `data/node-types.json` | Nodes and Console presentation |
| Parent relationship | Graph, on the child node | Graph behavior validates attachment | `data/nodes.json` | Graph navigation |
| Description document | Content | Content | `data/documents.json` | Graph via `DescriptionId` |
| Participant profile | Participants | Participants | `data/participants.json` | Graph via `AuthorId` |
| Integration-event shape | Contracts | Producer supplies event data | In memory today | Registered consumers |
| Vote and summary | Voting — future | Voting | Not implemented | Node views by node ID |

## Rules

1. ID creation follows ownership. Graph creates Node IDs; Content creates Document IDs; Participants creates Participant IDs.
2. A foreign identifier is a reference, not ownership of the foreign entity.
3. A consumer must not update another boundary's storage directly.
4. The host may compose reads but must send writes through the owning boundary.
5. Missing referenced data must be handled explicitly; it must not silently cause a replacement identity to be generated.
6. Cross-boundary operations may require compensation or eventual consistency once services are distributed.

## Current creation consistency

The Console currently creates and saves a Content document before creating its Graph node. If node creation fails, an orphaned document can remain. That is acceptable for the prototype but should eventually be addressed by an application workflow, cleanup policy, or durable process manager.

## Questions to resolve before service extraction

- Which references require immediate validation?
- May consumers cache public data, and how is it invalidated?
- What is the retention policy for orphaned or archived resources?
- Which operation owns retries and compensation?
- How are event delivery and database updates made reliable together?
- What happens when the authoritative boundary is unavailable?

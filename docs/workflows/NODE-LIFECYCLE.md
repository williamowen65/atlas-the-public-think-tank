# Node lifecycle workflow

Related requirements: [GRA-001](../requirements/REQUIREMENTS.md#gra-001), [GRA-003](../requirements/REQUIREMENTS.md#gra-003), [GRA-005](../requirements/REQUIREMENTS.md#gra-005), [EVT-001](../requirements/REQUIREMENTS.md#evt-001)

## Create a node with a description

**Actor:** authenticated participant

**Goal:** publish a new node with separately owned descriptive content.

### Preconditions

- The authenticated participant exists and is active.
- The selected node type exists and is active.
- Any selected parents exist and are available for creation.
- The requested parent attachment would not create a graph cycle.

### Normal sequence

1. The host gathers title, description body, node type and requested sub-node types.
2. NodeCreationService enters the topology boundary: SQL starts a transaction and acquires the graph lock.
3. Check author/type/parent availability and construct a Markdown block, Document and Node proposal using one AtlasTime UTC timestamp. Content and Graph generate their IDs in memory.
4. Give Graph the DocumentId as DescriptionId; attach the optional parent after ancestry validation. Graph records its lifecycle events in memory.
5. Save the block, document and Node through repositories sharing the operation context and transaction.
6. Commit the outer transaction only after every save succeeds; the service returns the Node.
7. The host publishes recorded events outside the SQL transaction.
8. Registered subscribers handle or ignore each event synchronously.
9. The host clears successfully dispatched events and displays the new node.

### Persistent result

- `DocumentRows` contains the Content-owned identifier and timestamps; `DocumentBlocks` preserves block order and `BlockRows` contains typed payloads.
- `NodeRows` contains the Graph node and matching description reference.
- The Document does not contain the Node entity.
- The Node does not contain the Document entity.

### Transaction and failure behavior

[NodeCreationService](../../src/Atlas.Console/NodeCreationService.cs) constructs the proposed block, document and Node before persistence, then saves them in one shared topology-coordinated SQL transaction. Failure rolls back all related writes; caller objects must be reloaded before retrying. Event dispatch follows successful commit and remains outside the transaction. See [failure-path evidence](../security/validation/IMPLEMENTATION.md#workflows-and-evidence).

## Archive and restore

Graph owns the state transition and records the corresponding versioned event. The host saves the node before dispatching the event. Subscribers decide independently whether the change requires action in their own boundary.

Graph mutations enforce author permission; Console supplies the authenticated actor. Further domain-service organization is tracked separately; see [AUT-002](../requirements/REQUIREMENTS.md#aut-002).

## Moderation visibility

Moderation can hide a Node's public title and description without changing the
Graph Node's status or relationships. The Console preserves navigation and
shows a public reason placeholder; Discovery removes the hidden Node from its
main browsing results. The author can edit and request restoration review.
See the [Moderation Domain blackboard](../blackboards/workflows/Moderation/ModerationDomain.excalidraw)
and [boundary documentation](../architecture/MODERATION.md) for the full workflow.

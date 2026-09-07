# Node lifecycle workflow

Related requirements: [GRA-001](../requirements/REQUIREMENTS.md#gra-001), [GRA-003](../requirements/REQUIREMENTS.md#gra-003), [GRA-005](../requirements/REQUIREMENTS.md#gra-005), [EVT-001](../requirements/REQUIREMENTS.md#evt-001)

## Create a node with a description

**Actor:** selected participant  
**Goal:** publish a new node with separately owned descriptive content.

### Preconditions

- The participant exists.
- The selected node type exists and is active.
- Any selected parent identifiers exist.
- The requested parent attachment would not create a graph cycle.

### Normal sequence

1. The host gathers title, description body, node type, and requested sub-node types.
2. Content creates a Document and generates its DocumentId.
3. The Content repository saves the Document.
4. The host supplies that ID to Graph as the node's DescriptionId.
5. Graph creates the Node, its NodeId, and a `NodeCreatedV1` event.
6. The Graph repository saves the Node.
7. The host publishes recorded events.
8. Registered subscribers handle or ignore each event.
9. The host clears successfully dispatched events and displays the new node.

### Persistent result

- `data/documents.json` contains the document body and Content-owned identifier.
- `data/nodes.json` contains the Graph node and matching description reference.
- The Document does not contain the Node entity.
- The Node does not contain the Document entity.

### Current failure gap

If the document is saved and node creation then fails, the prototype may retain an orphaned document. Before distributed deployment, choose a compensation, cleanup, or process-management policy.

## Archive and restore

Graph owns the state transition and records the corresponding versioned event. The host saves the node before dispatching the event. Subscribers decide independently whether the change requires action in their own boundary.

Authorization for these node mutations is not yet fully enforced behind a Graph application use case; see [AUT-002](../requirements/REQUIREMENTS.md#aut-002).

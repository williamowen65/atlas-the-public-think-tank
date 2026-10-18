# Nodes and Content: UML and process trees

[Visual guide](README.md) · [Wiring](WIRING.md) · Next: [Reactions and notifications](REACTIONS-AND-NOTIFICATIONS.md) · [Transactions](TRANSACTIONS-AND-EVENTS.md)

## Two domains, one coordinator

```mermaid
classDiagram
    direction TB
    class NodeWorkflowService {
        Create()
        Rename()
        Archive()
        Restore()
        Attach()
        Detach()
    }
    class ApplicationNodeCreationService {
        Create()
    }
    class GraphNodeCreationService {
        Prepare()
        Save()
    }
    class DocumentService {
        PrepareMarkdown()
        Save()
        Add()
        Update()
        Move()
        Remove()
    }
    class NodeDescriptionService {
        Add()
        Update()
        Move()
        Remove()
    }
    NodeWorkflowService ..> ApplicationNodeCreationService : create
    ApplicationNodeCreationService ..> GraphNodeCreationService : node rules
    ApplicationNodeCreationService ..> DocumentService : description rules
    NodeDescriptionService ..> DocumentService : authorized edits
```

The two creation classes have the same short name in different namespaces. **Application** coordinates; **Graph** validates and persists the node. Graph holds a description ID without referencing Content's concrete model. Content owns document composition and block payloads.

Sources: [Application creation](../../../src/Atlas.Application/NodeCreationService.cs#L12), [Graph creation](../../../src/Atlas.Graph/Nodes/NodeCreationService.cs), [DocumentService](../../../src/Atlas.Content/Documents/DocumentService.cs), [NodeWorkflowService](../../../src/Atlas.Application/NodeWorkflowService.cs#L11).

## Creation call order

```mermaid
sequenceDiagram
    participant W as NodeWorkflowService
    participant A as Application creation
    participant D as Content and Graph services
    participant S as SQL boundary
    participant E as Event publisher
    W->>A: Create(inputs)
    A->>S: Enter topology boundary
    A->>D: Prepare description and node, one timestamp
    alt Preparation fails
        D-->>A: Validation error before writes
        A-->>W: Failure
    else Proposals valid
        A->>D: Save block/document, then node
        D->>S: Nested saves join current transaction
        alt Save or commit fails
            S-->>A: Failure, no success return
            A-->>W: Failure, no event dispatch
        else Commit confirmed
            S-->>A: Return created node
            A-->>W: Created node with recorded events
            W->>E: Publish each event
            W->>W: Clear events after successful dispatch
        end
    end
```

This preserves coordinated Graph/Content creation while moving the coordination out of the terminal. Graph validates proposed references/ancestry/quota during preparation and checks storage references when saving. The outer SQL topology boundary holds transaction-scoped topology coordination. [Transaction details](TRANSACTIONS-AND-EVENTS.md) explain nested saves and uncertain commit outcomes.

## Description mutation decisions

```mermaid
flowchart TD
    A["Add / Update / Move / Remove"] --> B["Enter operation boundary"]
    B --> C{"Participant available, node exists, actor is author?"}
    C -->|No| X["Reject; do not perform Content mutation"]
    C -->|Yes| D["Resolve description ID and operation timestamp"]
    D --> E{"Requested change"}
    E -->|Add| F["Create typed block; insert ID; save block and document"]
    E -->|Update| G["Check membership and kind; update and save block"]
    E -->|Move| H["Change block order; save document"]
    E -->|Remove| I["Detach block ID; save document; retain payload"]
    F --> J{"Content validation and SQL writes succeed?"}
    G --> J
    H --> J
    I --> J
    J -->|Yes| K["Commit outer SQL operation"]
    J -->|No| L["Throw; uncommitted SQL writes roll back"]
```

These are application authorization checks followed by Content invariants. Reference availability alone is not an ownership check. `NodeDescriptionService` reloads the node on every call and calls `EnsureAuthoredBy`; Content loads the description and validates the selected mutation. Update keeps block identity; Remove retains its stored payload. This path does not publish Graph lifecycle events.

Sources: [description entry points](../../../src/Atlas.Application/NodeDescriptionService.cs#L18), [typed BlockInput](../../../src/Atlas.Content/Blocks/BlockInput.cs), [Content mutation methods](../../../src/Atlas.Content/Documents/DocumentService.cs). Verification: [SQL application workflow tests](../../../tests/Atlas.Persistence.Tests/SqlApplicationWorkflowTests.cs).

## Other node mutations

Rename/archive/restore enter an ordinary operation boundary, check current participant/ownership, mutate the supplied node and save metadata. Attach delegates relationship rules to Graph; Detach uses the topology boundary. All dispatch recorded events after their save boundary returns. This reload checks ownership; it is not optimistic version checking of all fields on the supplied node. See [NodeWorkflowService](../../../src/Atlas.Application/NodeWorkflowService.cs#L11) and [workflow regression tests](../../../tests/Atlas.Application.Tests/WorkflowTests.cs).

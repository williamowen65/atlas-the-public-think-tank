# Transaction and event lifecycle

[Visual guide](README.md) · [Wiring](WIRING.md) · [Nodes](NODES-AND-CONTENT.md) · [Reactions and notifications](REACTIONS-AND-NOTIFICATIONS.md)

## Capabilities connect an interface to SQL behavior

These interfaces let domain/application code request a protected operation without embedding SQL transaction or locking code in the domain. With the current SQL adapters, the ordinary operation boundary groups work into one transaction; the topology boundary additionally coordinates graph structure changes with a named database lock.

**Transaction** means related writes commit together or uncommitted writes roll back. **Topology** means the node/parent structure. Its extra lock prevents competing topology operations from interleaving their ancestry checks and writes. Ordinary transactions also take SQL locks; the named topology lock is additional coordination.

```mermaid
classDiagram
    direction TB
    class IOperationBoundary["IOperationBoundary (Atlas.Contracts)"] {
        <<interface>>
        Execute(operation)
    }
    class INodeTopologyBoundary["INodeTopologyBoundary (Atlas.Graph)"] {
        <<interface>>
        ExecuteTopology(operation)
    }
    class SqlRepository["SqlRepository (Atlas.Application)"]
    class SqlNodeRepository["SqlNodeRepository (Atlas.Application)"]
    class SqlOperationBoundary["SqlOperationBoundary (Atlas.Application)"] {
        Execute(open, operation, lockResource)
        Open(open)
    }
    class AtlasDataContext["AtlasDataContext (Atlas.Persistence)"]
    IOperationBoundary <|.. SqlRepository : implements
    SqlRepository <|-- SqlNodeRepository : inherits
    INodeTopologyBoundary <|.. SqlNodeRepository : implements
    SqlNodeRepository ..> SqlOperationBoundary : topology coordination
    SqlRepository ..> SqlOperationBoundary : ordinary operations
    SqlOperationBoundary ..> AtlasDataContext : owns or lends context
```

Read a **dashed line with a hollow triangle** as “implements this interface” (UML realization). A **solid line with a hollow triangle** means class inheritance. A **dashed arrow** means one class uses another. These arrows show type relationships and dependencies, not the execution order of a transaction.

| Type | Defined in / responsibility |
|---|---|
| `IOperationBoundary` | **Atlas.Contracts** — general operation capability shared across domains |
| `INodeTopologyBoundary` | **Atlas.Graph** — Graph-specific capability for coordinated topology work |
| `SqlRepository` | **Atlas.Application.Storage** — base SQL adapter implementing the general operation capability |
| `SqlNodeRepository` | **Atlas.Application.Storage** — inherits the ordinary capability and implements the topology capability |
| `SqlOperationBoundary` | **Atlas.Application.Storage** — opens/joins the SQL transaction and acquires requested named locks |
| `AtlasDataContext` | **Atlas.Persistence** — EF context, tracked rows and database mappings; Application storage uses it |

Only the topology interface is defined by Graph; the ordinary operation interface belongs to Contracts. The implementations are in Application, while the EF context remains in the separate Persistence project. Application references Persistence rather than containing its context.

The Contracts `OperationBoundary` helper detects `IOperationBoundary`; Graph's `NodeTopologyBoundary` detects its additional capability. Without a supplied capability they execute the callback through the fallback path; that does not invent a transaction for an arbitrary repository. SQL repositories implement the capabilities and delegate to the shared SQL coordinator.

For example, node creation enters the topology boundary: its description and node writes share the transaction, while the graph lock protects the structural checks. Rename uses the ordinary operation boundary and does not request the named topology lock. See [node workflows](NODES-AND-CONTENT.md) for the callers and the next diagram for transaction ownership.

Sources: [Contracts interface and helper](../../../src/Atlas.Contracts/Operations/OperationBoundary.cs), [Graph topology interface and helper](../../../src/Atlas.Graph/Nodes/NodeTopologyBoundary.cs), [SQL base repository](../../../src/Atlas.Application/Storage/SqlRepository.cs), [SQL node repository](../../../src/Atlas.Application/Storage/SqlNodeRepository.cs), [SQL coordinator](../../../src/Atlas.Application/Storage/SqlOperationBoundary.cs#L23), [EF context](../../../src/Atlas.Persistence/AtlasDataContext.cs).

## Entering a SQL boundary

```mermaid
flowchart TD
    A["SqlOperationBoundary.Execute"] --> B{"Ambient EF context exists?"}
    B -->|Yes| C{"Exact configured connection string matches?"}
    C -->|No| D["Reject cross-database operation"]
    C -->|Yes| E["Acquire requested coordination lock; run nested callback"]
    E --> F["Return to outer caller; nested call does not commit"]
    B -->|No| G["Join telemetry; open context and serializable transaction"]
    G --> H["Acquire requested lock; expose ambient context; run callback"]
    H --> I{"Callback succeeds?"}
    I -->|No| J["Throw; dispose uncommitted transaction"]
    I -->|Yes| K["Commit transaction"]
    K --> L["Publish committed telemetry; record storage success"]
    L --> M["Return result"]
```

This is one synchronous execution flow. `AsyncLocal` carries the current context for nested calls; it does not make EF thread-safe. Do not start parallel repository work on that context. The outer owner clears the ambient slot in `finally` and disposes its context/transaction. Nested repository context leases borrow that context and do not dispose it.

Ordinary operations use serializable isolation. Topology operations additionally request the transaction-owned `AtlasGraphTopology` application lock. The lock coordinates topology work; transaction atomicity comes from the SQL transaction itself.

The happy-path tree omits exception edges from opening, locking, committing and post-commit telemetry. Failure before a confirmed commit leaves uncommitted work to roll back on disposal. A commit exception can leave the outcome uncertain; a failure after commit cannot roll back persisted data. Recognized SQL/EF conflicts are translated to `OperationConflictException`. The implementation emits telemetry for those distinctions; an exception alone is not proof that nothing was saved.

## Event delivery after saving

```mermaid
flowchart TD
    A{"Node mutation boundary returned successfully?"}
    A -->|No| B["Propagate failure; no success event dispatch"]
    A -->|Yes| C["Publish recorded events synchronously"]
    C --> D{"All handlers return?"}
    D -->|Yes| E["Clear node events; return success"]
    D -->|No| F["Propagate error; saved business state remains"]
    F --> G["Node event list is not cleared; earlier handlers may have run"]
```

For `NodeWorkflowService`, clearing happens after the entire dispatch loop. This is not automatic retry: retrying publication could call an earlier handler again, and losing the in-memory node loses its pending list. The publisher does not catch subscriber failures, persist messages or guarantee delivery. An event with no registered handlers returns normally. Comments and Moderation construct notification requests directly after saving rather than draining a node event list.

The durable **audit outbox** is a separate mechanism. It does not make these domain events or notification requests durable. Follow [Audit Delivery](../../security/observability/AUDIT-DELIVERY.md) only for that separate pipeline.

Sources: [NodeWorkflowService dispatch](../../../src/Atlas.Application/NodeWorkflowService.cs#L53), [InMemoryEventPublisher](../../../src/Atlas.Application/Eventing/InMemoryEventPublisher.cs). Verification: [workflow tests](../../../tests/Atlas.Application.Tests/WorkflowTests.cs), [SQL operation tests](../../../tests/Atlas.Persistence.Tests/SqlOperationValidationTests.cs), [SQL workflow tests](../../../tests/Atlas.Persistence.Tests/SqlThinHostWorkflowTests.cs).

## What parity means for this refactor

| Preserved mechanism | New organization or intentional change |
|---|---|
| Shared SQL transactions, reference validation and topology coordination | SQL adapters moved from Console to Application.Storage |
| Domain ownership, availability and validation rules | Reusable Application coordinates the cross-domain paths |
| Saving before synchronous event handling | Event publisher and recipient selection moved out of menus |
| Graph reaction and Voting policies | Application now wraps association plus vote in one shared boundary |

The extraction aims to preserve existing behavior while making the host replaceable. These diagrams expose the seams for review; the linked tests provide executable evidence of selected paths rather than proof of every possible interaction.

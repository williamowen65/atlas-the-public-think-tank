# Communities workflows

[Boundary](../architecture/COMMUNITIES.md) · [Requirements](../requirements/REQUIREMENTS.md#communities) · [Traceability](../requirements/TRACEABILITY.md#com-001)

## Community lifecycle

```mermaid
stateDiagram-v2
    [*] --> Active: eligible participant creates
    Active --> Active: owner edits name or description
    Active --> Archived: owner archives
    Archived --> Active: owner restores
```

Creation also establishes the creator as owner and first active member. Archived Communities remain readable but reject joins and new Node associations.

## Membership decision

```mermaid
flowchart TD
    A[Select public community] --> B{Archived?}
    B -->|Yes| C[Read only]
    B -->|No| D{Active member?}
    D -->|No| E[Join]
    D -->|Yes| F{Owner?}
    F -->|Yes| G[Remain member while owner]
    F -->|No| H[Leave]
```

## Node organization and navigation

```mermaid
flowchart TD
    N[One Graph Node] --> A[Community association]
    N --> B[Community association]
    A --> C[Community A content browser]
    B --> D[Community B content browser]
```

A Node may have no associations. Multiple associations still point to one Node and do not create, copy, attach, or detach Graph parent relationships. The Console shows Communities next to tags, lets a participant manage associations from a Node, and supports navigation in both directions.

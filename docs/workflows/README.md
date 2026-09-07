# Atlas workflows

Workflow documents describe behavior that crosses entities or bounded contexts. They identify sequencing, responsibility, authorization, events, persistence, and failure handling without tying the behavior to a particular UI.

## Catalog

- [Node lifecycle](NODE-LIFECYCLE.md) covers node creation, description ownership, event publication, archive, and restore.

Future useful workflow documents include:

- Add a new sub-node beneath a parent
- Attach an existing node to another parent
- Change requested sub-node types
- Update a participant profile
- Submit or change a vote
- Moderate a node or node type

## Workflow template

Each workflow should record:

1. Actor and goal
2. Preconditions
3. Authorization decision
4. Normal sequence
5. Boundary ownership at each step
6. Persistent state changes
7. Events produced or consumed
8. Failure and retry behavior
9. Related requirements, ADRs, and tests

A workflow describes coordination. It should not move domain invariants into the host.

# Atlas.Graph

`Atlas.Graph` owns the structure of the Public Think Tank: nodes, globally
recognized node types, and the directed links that connect nodes.

The current implementation is the core domain model used by the Console
prototype. Persistence and user interaction are supplied by adapters outside
this project.

## Boundary responsibilities

Graph owns:

- Node identity, title, status, and timestamps
- References to a node's author and Content description
- Global system-defined and participant-defined node types
- The node types a node requests from potential responders
- Parent links between nodes
- Root, child, and multiple-parent graph structures
- Local invariants for changing Graph state
- Graph domain events expressed through versioned public contracts

Graph does not own:

- Participant profiles or authentication
- Description documents or content blocks
- Voting and scoring
- Comments as a separate discussion entity

In Atlas, a comment is currently modeled as a Graph node. This allows comments
to participate in the same typed, navigable structure as questions, ideas,
evidence, and other responses.

## Node model

A `Node` currently contains references rather than objects owned by other
boundaries:

```text
Node
├── NodeId
├── NodeTitle
├── NodeTypeId
├── NodeAuthorId
├── NodeDescriptionId
├── RequestedSubNodeTypes
├── ParentNodeIds
├── NodeStatus
└── CreatedAt / UpdatedAt
```

`NodeAuthorId` references a participant owned by `Atlas.Participants`.
`NodeDescriptionId` references a document owned by `Atlas.Content`.
Graph does not share those boundaries' domain objects.

## Node types

`NodeTypeDefinition` represents a globally reusable type. Types can be
system-defined or participant-defined and are identified by GUID-backed
`NodeTypeId` values.

A type currently includes:

- Name and description
- Owner reference for participant-defined types
- System-defined and archived flags
- An `AutoPluralize` presentation setting
- Creation and update timestamps

Once a participant introduces a type such as `Counter Evidence`, it becomes
available globally. Other nodes can request it and other participants can use
it without creating a duplicate definition.

## Requested types and actual children

Requested sub-node types and actual child nodes are deliberately different.

A requested type means:

> This node invites responses of this type.

An actual child means:

> This node has been attached to the parent.

A responder may add a child using a globally known type that the parent did not
request. The parent's requested type collection remains unchanged. This
preserves the distinction between the author's invitation and the responder's
contribution.

Child counts are derived from actual parent links and child type IDs. They are
not stored on the parent, avoiding duplicated counts that could become stale.

## Parent links

Each node stores a collection of GUID-backed `ParentNodeIds`:

- No parents means the node is a root.
- One parent is the usual child-node case.
- Multiple parents support relationship nodes and converging branches.

Parent links are changed through intentional behavior:

```csharp
node.AttachToParent(parentId, attachedAt);
node.DetachFromParent(parentId, detachedAt);
```

The aggregate currently prevents empty IDs, self-parenting, and duplicate
parent links. It cannot detect longer circular chains by itself because doing
so requires loading other nodes. The Console prototype currently performs that
graph traversal before attaching an existing node.

## Domain events and contracts

Graph records domain events in `DomainEvents`. The current records are defined
in `Atlas.Contracts.Graph.V1`, which is the shared source of truth for message
payloads crossing boundaries.

Current lifecycle contracts include:

- `NodeCreatedV1`
- `NodeArchivedV1`
- `NodeRestoredV1`
- `NodeParentAttachedV1`
- `NodeParentDetachedV1`

The Graph model records events but does not publish them. The application host
saves the aggregate and then broadcasts its recorded events. Subscribers may
respond or ignore them without Graph knowing who is listening.

## Persistence

Graph defines repository interfaces, while persistence belongs to the hosting
application or infrastructure adapter.

The Console prototype currently stores:

- Node records and reference IDs in `data/nodes.json`
- Global type definitions in `data/node-types.json`

Content documents and participants remain in their own data files. Existing
JSON is migrated when newer optional reference collections are absent.

A relational implementation may eventually use a join table for parent links,
even though the domain continues to expose `ParentNodeIds` as a collection.

## Current local invariants

The model currently protects rules including:

- Required, bounded node titles
- Required GUID references
- No duplicate requested sub-node types
- No empty requested type IDs
- No empty parent IDs
- No self-parenting
- No duplicate parent links
- Updated timestamps cannot precede creation
- Ownership and moderator rules for editing node-type definitions

Rules involving multiple aggregates should be coordinated by a Graph
application service rather than by loading repositories from inside `Node`.

## Refinements to consider

The core model is intentionally usable before all policy decisions are final.
Likely refinements include:

### Parent-link policy

- Move circular-reference detection from Console into a Graph application
  service.
- Decide whether cycles are always forbidden or valid for particular graph
  relationships.
- Enforce when ordinary nodes may have multiple parents.
- Require a minimum number of parents for relationship nodes.
- Decide whether missing or archived parents affect link validity.

### Link metadata

If Atlas needs historical attribution beyond the current references, replace
or supplement raw parent IDs with a richer parent-link model containing:

- Parent and child IDs
- Who created the link
- When it was created
- Whether the child's type was requested by the parent
- Link status or removal history
- A semantic relationship kind

This information belongs to the contextual parent-child association, not to the
global `NodeTypeDefinition`.

### Requested-type policy

- Restrict changes to the node author, moderators, or another explicit policy.
- Record events when requested types are added or removed.
- Decide whether Comment must always remain requested.
- Support ordering, prompts, or display settings per requested type.
- Determine how archived global types affect existing requests.

### Type governance

- Enforce global name uniqueness below the Console layer.
- Add moderation and recommendation workflows.
- Consider explicit plural names for irregular nouns.
- Decide whether system types need stable semantic keys in addition to GUIDs.
- Define rules for merging duplicate participant-created types.

### Query and persistence maturity

- Add repository queries for roots, children, parents, and type-grouped counts.
- Avoid scanning every node as the graph grows.
- Add integration tests for JSON migration and complete creation/linking flows.
- Introduce concurrency handling when persistence moves beyond local files.
- Use transactional event publishing or an outbox when services are separated.

### Additional Graph behavior

- Define rules for Location nodes.
- Refine the semantics of Relationship nodes.
- Add events for title, type, description-reference, and requested-type
  changes.
- Decide how archiving a node affects its children and parent links.
- Add explicit traversal and navigation services as graph queries become more
  complex.

## Current status

The Graph boundary now supports the essential Atlas experiment: authors can
create typed nodes, declare the response types they seek, responders can add
requested or unexpected typed children, and relationship nodes can connect
multiple branches.

The remaining work is primarily policy, scale, authorization, and richer link
semantics rather than a missing core graph structure.

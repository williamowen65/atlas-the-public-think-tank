# Atlas requirements baseline

This document is the authoritative catalog of requirement statements and acceptance criteria. The [traceability matrix](TRACEABILITY.md) connects every record to current code, tests, demonstrations, and gaps.

<a id="gra-001"></a>
## GRA-001 — Nodes receive stable unique identifiers

**Statement:** The system shall assign every newly created node a non-empty, stable GUID identifier.

**Rationale:** Nodes must remain addressable across persistence, parent relationships, events, and future service boundaries.

**Priority:** Must  
**Status:** Verified

### Acceptance criteria

- Creating a node produces a non-empty GUID.
- Reconstitution preserves the persisted identifier.
- Editing a node does not replace its identifier.

[View traceability](TRACEABILITY.md#gra-001)

<a id="gra-002"></a>
## GRA-002 — Nodes have validated titles

**Statement:** The system shall require each node to have a trimmed title no longer than 200 characters.

**Rationale:** A concise, valid title is the primary human-readable identity of a node.

**Priority:** Must  
**Status:** Verified

### Acceptance criteria

- Null, empty, and whitespace-only titles are rejected.
- Leading and trailing whitespace is removed.
- Titles longer than the allowed maximum are rejected.

[View traceability](TRACEABILITY.md#gra-002)

<a id="gra-003"></a>
## GRA-003 — Nodes support archive and restore lifecycle states

**Statement:** The system shall create nodes as active and support intentional archive and restore transitions.

**Rationale:** Content should be removable from active use without destroying its identity or history.

**Priority:** Must  
**Status:** Verified

### Acceptance criteria

- A new node is active.
- Archiving an active node changes it to archived.
- Restoring an archived node changes it to active.
- Repeated no-op transitions do not alter the updated timestamp or add duplicate events.

[View traceability](TRACEABILITY.md#gra-003)

<a id="gra-004"></a>
## GRA-004 — Nodes retain their author reference

**Statement:** The system shall store the participant identifier of the node's author.

**Rationale:** Authorship supports attribution, profile navigation, contribution summaries, and future authorization.

**Priority:** Must  
**Status:** Verified

### Acceptance criteria

- Creation requires a non-empty author identifier.
- Reconstitution preserves the author identifier.
- The Graph model stores only the external participant reference, not a Participant object.

[View traceability](TRACEABILITY.md#gra-004)

<a id="gra-005"></a>
## GRA-005 — Nodes reference descriptions owned by Content

**Statement:** The Graph boundary shall associate a node with a description document by identifier without containing the Content document.

**Rationale:** Content owns document composition and lifecycle while Graph owns node topology.

**Priority:** Must  
**Status:** Verified

### Acceptance criteria

- A node stores a non-empty description identifier.
- The Graph project does not require a Document instance to construct or reconstitute a node.
- Reconstitution preserves the description identifier.

[View traceability](TRACEABILITY.md#gra-005)

<a id="typ-001"></a>
## TYP-001 — Node types use stable GUID identifiers

**Statement:** The system shall categorize each node with a non-empty GUID node-type identifier.

**Rationale:** GUID references allow system and user-defined types to share one global catalog without enum coupling.

**Priority:** Must  
**Status:** Verified

### Acceptance criteria

- A node requires a non-empty type identifier.
- Changing a node type preserves the node identifier.
- Reconstitution preserves the persisted type identifier.

[View traceability](TRACEABILITY.md#typ-001)

<a id="typ-002"></a>
## TYP-002 — Users can create globally reusable node types

**Statement:** The system shall allow a user-defined node type to be created once and subsequently selected by other node workflows.

**Rationale:** Shared terms such as Counter Evidence should become reusable vocabulary rather than duplicated per node.

**Priority:** Must  
**Status:** Implemented

### Acceptance criteria

- A custom type receives a GUID and owner reference.
- The type is persisted in the global node-type catalog.
- Later node and sub-node workflows can select the saved type.
- Duplicate active names are not offered as separate new definitions.

[View traceability](TRACEABILITY.md#typ-002)

<a id="typ-003"></a>
## TYP-003 — Node authors declare requested sub-node types

**Statement:** The system shall allow a node to declare a set of node types it is requesting as responses.

**Rationale:** Requested types communicate the kinds of contributions that would add value beneath a node.

**Priority:** Must  
**Status:** Verified

### Acceptance criteria

- A request references a non-empty type GUID.
- The same requested type cannot be added twice.
- A requested type can be removed intentionally.
- Reconstitution preserves the requested type set.

[View traceability](TRACEABILITY.md#typ-003)

<a id="typ-004"></a>
## TYP-004 — Comment is requested by default

**Statement:** The system shall include Comment as a default requested sub-node type when a user does not select another initial set.

**Rationale:** Any participant should be able to contribute a general comment throughout Atlas.

**Priority:** Should  
**Status:** Partial

### Acceptance criteria

- The creation workflow offers Comment as the default.
- The default is persisted with the node.
- The rule is eventually enforced outside any single UI host.

[View traceability](TRACEABILITY.md#typ-004)

<a id="typ-005"></a>
## TYP-005 — Type labels support controlled pluralization

**Statement:** The system shall display counted node-type labels in singular or plural form according to the type definition's pluralization setting.

**Rationale:** Labels such as Comments should read naturally while mass nouns such as Evidence remain unchanged.

**Priority:** Should  
**Status:** Verified

### Acceptance criteria

- A count of one uses the singular stored name.
- Other counts append s when automatic pluralization is enabled.
- The stored name remains unchanged when automatic pluralization is disabled.
- Type-selection prompts use singular names because the user is choosing one type.

[View traceability](TRACEABILITY.md#typ-005)

<a id="rel-001"></a>
## REL-001 — Nodes support zero or multiple parents

**Statement:** The system shall allow a node to be a root or to be linked beneath multiple parent nodes.

**Rationale:** Atlas is a graph: a contribution may be relevant to more than one existing node.

**Priority:** Must  
**Status:** Verified

### Acceptance criteria

- A new root can have no parents.
- A node can attach to more than one distinct parent.
- Reconstitution preserves all parent identifiers.

[View traceability](TRACEABILITY.md#rel-001)

<a id="rel-002"></a>
## REL-002 — Invalid direct parent links are rejected

**Statement:** The system shall reject empty parent identifiers, self-parenting, and duplicate direct parent attachments.

**Rationale:** These local invariants prevent malformed graph edges.

**Priority:** Must  
**Status:** Verified

### Acceptance criteria

- An empty parent GUID is rejected.
- A node cannot attach to itself.
- Attaching the same parent twice does not create duplicate links.

[View traceability](TRACEABILITY.md#rel-002)

<a id="rel-003"></a>
## REL-003 — Circular parent relationships are prevented

**Statement:** The system shall prevent an attachment that would introduce a cycle in the node graph.

**Rationale:** Cycle prevention keeps parent/child navigation meaningful and avoids unbounded traversal.

**Priority:** Must  
**Status:** Partial

### Acceptance criteria

- Before attachment, the reachable ancestry of the proposed parent is checked.
- An attachment that reaches the child is rejected.
- The rule is enforced independently of the Console UI.

[View traceability](TRACEABILITY.md#rel-003)

<a id="rel-004"></a>
## REL-004 — Parent attachments and detachments emit facts

**Statement:** The Graph boundary shall record versioned events when parent relationships are attached or detached.

**Rationale:** Other boundaries can react to topology changes without Graph knowing its subscribers.

**Priority:** Should  
**Status:** Verified

### Acceptance criteria

- A successful attachment records NodeParentAttachedV1.
- A successful detachment records NodeParentDetachedV1.
- No-op operations do not record misleading events.

[View traceability](TRACEABILITY.md#rel-004)

<a id="con-001"></a>
## CON-001 — Content owns documents and document identifiers

**Statement:** The Content boundary shall create and own description documents and their identifiers.

**Rationale:** Graph should reference rich content without owning its future block model.

**Priority:** Must  
**Status:** Implemented

### Acceptance criteria

- Content generates DocumentId values.
- A Document can exist without containing a Graph Node object or node identifier.
- Graph stores only its opaque description reference.

[View traceability](TRACEABILITY.md#con-001)

<a id="con-002"></a>
## CON-002 — Description documents persist separately from nodes

**Statement:** The Console prototype shall persist Content documents separately from Graph nodes.

**Rationale:** Separate persistence makes boundary ownership visible even while the prototype runs in one process.

**Priority:** Must  
**Status:** Implemented

### Acceptance criteria

- Documents are written to data/documents.json.
- Nodes contain DescriptionId rather than description text.
- Restarting the Console preserves document identifiers and bodies.

[View traceability](TRACEABILITY.md#con-002)

<a id="par-001"></a>
## PAR-001 — Participants have validated public profiles

**Statement:** The system shall represent each participant with a stable identifier, display name, optional biography, active state, and timestamps.

**Rationale:** Participants need lightweight public identities without prematurely implementing authentication.

**Priority:** Must  
**Status:** Verified

### Acceptance criteria

- Display name is required and length-limited.
- Biography is optional and length-limited.
- Reconstitution preserves identity and timestamps.

[View traceability](TRACEABILITY.md#par-001)

<a id="par-002"></a>
## PAR-002 — Users can browse participant profiles and contributions

**Statement:** The Console prototype shall allow users to browse profiles and see authored nodes summarized by node type.

**Rationale:** This demonstrates composition of Participant, Graph, and node-type data without merging their domain models.

**Priority:** Should  
**Status:** Implemented

### Acceptance criteria

- Participants are selectable from a browser.
- A profile can be reached from a node's author action.
- Authored nodes are summarized with singular/plural type labels.
- The nodes summary is the rightmost browser column.

[View traceability](TRACEABILITY.md#par-002)

<a id="aut-001"></a>
## AUT-001 — Participants may edit only their own profiles

**Statement:** The system shall permit a participant profile update only when the acting participant owns that profile.

**Rationale:** UI visibility is not a security boundary; ownership must be checked in the Participants application workflow.

**Priority:** Must  
**Status:** Verified

### Acceptance criteria

- The profile owner can update display name and biography.
- A different participant is denied.
- External hosts cannot bypass the use case by calling profile-content mutation methods directly.

[View traceability](TRACEABILITY.md#aut-001)

<a id="aut-002"></a>
## AUT-002 — Node management is limited to authorized actors

**Statement:** The system shall allow node mutation and requested-type management only for the node author or an actor granted an applicable moderation capability.

**Rationale:** Authorship is recorded, but recording an author is not itself authorization enforcement.

**Priority:** Must  
**Status:** Proposed

### Acceptance criteria

- The actor is checked before each protected node mutation.
- A node author can perform allowed management actions.
- An unrelated participant is denied.
- Moderator authority is expressed as a capability rather than a UI-only Boolean.

[View traceability](TRACEABILITY.md#aut-002)

<a id="evt-001"></a>
## EVT-001 — Node creation records a versioned integration fact

**Statement:** Creating a node shall record a versioned NodeCreated event containing the identifiers required by consumers.

**Rationale:** Versioned contracts provide a stable communication shape across boundaries.

**Priority:** Must  
**Status:** Verified

### Acceptance criteria

- Node creation records NodeCreatedV1.
- The event contains the node and description identifiers and occurrence time.
- Reconstitution does not replay a creation event.

[View traceability](TRACEABILITY.md#evt-001)

<a id="evt-002"></a>
## EVT-002 — The host broadcasts events to interested subscribers

**Statement:** The application host shall publish recorded integration events to zero or more registered subscribers without Graph knowing those subscribers.

**Rationale:** This demonstrates boundary decoupling and permits Content or future services to respond or ignore events.

**Priority:** Should  
**Status:** Implemented

### Acceptance criteria

- Subscribers are registered in the composition root.
- Publishing dispatches by event type.
- Publishing with no subscribers is safe.
- Delivery and subscriber observations are logged in the Console prototype.

[View traceability](TRACEABILITY.md#evt-002)

<a id="per-001"></a>
## PER-001 — Prototype boundary data is stored in separate files

**Statement:** The Console prototype shall persist nodes, node types, documents, and participants in separate JSON files under data.

**Rationale:** The file-system database keeps ownership visible and makes the prototype easy to inspect.

**Priority:** Must  
**Status:** Implemented

### Acceptance criteria

- Each aggregate category has its own repository adapter and data file.
- Repositories implement contracts owned by their respective domain boundaries.
- Restarting the Console reloads prior state.

[View traceability](TRACEABILITY.md#per-001)

<a id="per-002"></a>
## PER-002 — Legacy node records migrate without losing descriptions

**Statement:** The Console prototype shall migrate supported legacy node JSON into the current identifier-based representation.

**Rationale:** Existing prototype data should remain usable as boundaries and storage shapes evolve.

**Priority:** Should  
**Status:** Implemented

### Acceptance criteria

- Legacy description text becomes a Content document.
- The migrated node receives that document's identifier.
- Missing author and requested-type fields receive documented compatibility defaults.
- Migration does not regenerate identifiers on every load.

[View traceability](TRACEABILITY.md#per-002)

<a id="vot-001"></a>
## VOT-001 — Node views report vote totals and averages

**Statement:** The system shall display the number of current votes and the average rating for each node without placing voting behavior in the Graph domain.

**Rationale:** Voting is part of the product experience but has an independent lifecycle, load profile, and rule set.

**Priority:** Must  
**Status:** Approved

### Acceptance criteria

- Node voting accepts whole-number values from 1 through 10.
- Browse and detail views show the number of current votes.
- The arithmetic mean is displayed to two decimal places.
- A dedicated Voting boundary owns vote records and aggregation.
- The composition layer joins voting summaries to nodes by identifier.

[View traceability](TRACEABILITY.md#vot-001)

<a id="vot-002"></a>
## VOT-002 — Voting is the single authority for vote behavior

**Statement:** The Voting boundary shall own vote records, voting-policy validation, vote lifecycle rules, and aggregate calculations for every supported vote target.

**Rationale:** Central ownership gives developers one authoritative location for vote behavior and permits voting traffic to scale without scaling unrelated boundaries.

**Priority:** Must  
**Status:** Approved

### Acceptance criteria

- Graph, Participants, hosts, and target-specific features do not persist or enforce their own vote records.
- Voting identifies foreign participants and targets by stable identifiers rather than importing their domain entities.
- Voting can be separated behind an application or service API without moving vote rules from another boundary.
- Voting supports different target-specific value policies under the same ownership boundary.

[View traceability](TRACEABILITY.md#vot-002)

<a id="vot-003"></a>
## VOT-003 — Only authenticated participants may vote

**Statement:** The system shall accept a vote only for an authenticated, eligible Atlas participant account.

**Rationale:** Each vote must be attributable to one accountable online identity even when the participant's public persona differs from their legal identity.

**Priority:** Must  
**Status:** Approved

### Acceptance criteria

- An anonymous browser session cannot cast, change, or remove a vote.
- An accepted vote records the acting participant identifier.
- The initial eligibility policy allows every authenticated active participant to vote.
- The initial policy adds no reputation, contribution, or per-account voting quota beyond one vote per target.

[View traceability](TRACEABILITY.md#vot-003)

<a id="vot-004"></a>
## VOT-004 — A participant has at most one current vote per target

**Statement:** The Voting boundary shall maintain no more than one current vote for the same participant and target.

**Rationale:** Participant-to-target uniqueness prevents duplicate influence while allowing independent votes on different targets.

**Priority:** Must  
**Status:** Approved

### Acceptance criteria

- Casting an initial vote creates one current participant-target record.
- Repeating a create request cannot produce a second current record for the same participant and target.
- The uniqueness rule holds when concurrent requests target the same participant-target pair.
- The same participant may vote independently on other targets.

[View traceability](TRACEABILITY.md#vot-004)

<a id="vot-005"></a>
## VOT-005 — Participants can change or undo their votes

**Statement:** The system shall allow the participant who owns a current vote to replace its value or remove it from current voting results.

**Rationale:** Participants need to correct mistakes and revise or retract their judgment.

**Priority:** Must  
**Status:** Approved

### Acceptance criteria

- Changing a vote updates the existing current participant-target vote rather than adding another current vote.
- A changed vote preserves its creation time and records a later update time.
- Undoing a vote removes its contribution from all current aggregates.
- An undone vote no longer appears in the public current-vote list.
- Whether the underlying removed record is physically deleted or privately retained remains a separate retention decision.

[View traceability](TRACEABILITY.md#vot-005)

<a id="vot-006"></a>
## VOT-006 — Node votes use a 1–10 general rating

**Statement:** The system shall allow an eligible participant to assign a Node one whole-number general rating from 1 through 10.

**Rationale:** Atlas initially relies on the Node's human-readable context rather than prescribing that every rating means only agreement, truth, importance, or quality.

**Priority:** Must  
**Status:** Approved

### Acceptance criteria

- Values below 1, above 10, and non-whole-number values are rejected.
- A value from 1 through 10 is accepted for an eligible Node.
- Changing and undoing the rating follow the universal vote lifecycle.
- Any future narrowing or specialization of rating meaning is defined within Voting.

[View traceability](TRACEABILITY.md#vot-006)

<a id="vot-007"></a>
## VOT-007 — NodeTag votes measure node-specific applicability

**Statement:** The system shall represent a NodeTag vote as an upvote or downvote on whether one TagDefinition applies to one specific Node.

**Rationale:** A tag may be appropriate for one Node and inappropriate for another; the vote must not become a global rating of the TagDefinition.

**Priority:** Must  
**Status:** Approved

### Acceptance criteria

- The vote target identifies the NodeTag association, not only its TagDefinition.
- An upvote contributes +1 and a downvote contributes -1.
- The displayed aggregate is a signed whole-number sum that may be negative, zero, or positive.
- A participant may change direction or undo the vote.
- Participant and timestamp data are recorded even if the first NodeTag interface displays only the aggregate.

[View traceability](TRACEABILITY.md#vot-007)

<a id="vot-008"></a>
## VOT-008 — Current Node votes are publicly auditable

**Statement:** The system shall allow any viewer of a Node's voting details to inspect the participant account and current value associated with each current Node vote.

**Rationale:** Public vote attribution supports transparency and helps participants discover others who share interest in a contribution and may wish to collaborate.

**Priority:** Must  
**Status:** Approved

### Acceptance criteria

- A current Node vote can be traced to its public participant profile.
- The current value cast by that participant is visible.
- An undone vote is absent from the public current-vote list.
- Public attribution reveals the Atlas account identity and does not require disclosure of a participant's legal identity.

[View traceability](TRACEABILITY.md#vot-008)

<a id="vot-009"></a>
## VOT-009 — Archived or unavailable targets reject voting interaction

**Statement:** The system shall reject new and changed votes when the target-owning boundary reports that the target is archived or otherwise unavailable for interaction.

**Rationale:** Archiving a Node stops direct interaction beneath that Node without transferring lifecycle ownership to Voting.

**Priority:** Must  
**Status:** Approved

### Acceptance criteria

- Voting checks authoritative target availability before accepting a new or changed vote.
- A Node archived by Graph cannot receive a new or changed rating.
- A directly dependent NodeTag target beneath an archived Node cannot receive a new or changed vote.
- Existing vote-read and undo behavior for archived targets is explicitly resolved before this requirement is implemented.

[View traceability](TRACEABILITY.md#vot-009)

<a id="vot-010"></a>
## VOT-010 — Aggregates remain correct under concurrent voting

**Statement:** The Voting boundary shall produce current aggregates without losing accepted votes or violating uniqueness when participants vote concurrently.

**Rationale:** Voting may be one of Atlas's highest-volume activities and must remain correct when many participants act at the same time.

**Priority:** Must  
**Status:** Approved

### Acceptance criteria

- Concurrent votes from different participants are all reflected in the resulting aggregate.
- Concurrent requests for one participant-target pair do not create duplicate current votes.
- A Node summary reports the current vote count and arithmetic mean.
- A NodeTag summary reports the current signed whole-number total.
- Accepted changes are reflected to users in real time under the current product expectation.
- The allowed consistency window after independent service deployment is documented before implementation.

[View traceability](TRACEABILITY.md#vot-010)

<a id="vot-011"></a>
## VOT-011 — Votes retain participant, target, value, and timestamps

**Statement:** Each current vote shall retain the identifiers and timestamps needed to identify who voted, what received the vote, the current value, and when it was created or last changed.

**Rationale:** These fields support uniqueness, aggregation, public transparency, debugging, and future policy decisions.

**Priority:** Must  
**Status:** Approved

### Acceptance criteria

- Each current vote has a stable vote identifier.
- Each current vote records one participant identifier and one target identity and type.
- Each current vote records a value valid for its voting policy.
- Each current vote records creation and last-updated timestamps.
- NodeTag votes retain this information even when their initial UI exposes only the aggregate.

[View traceability](TRACEABILITY.md#vot-011)

<a id="nfr-001"></a>
## NFR-001 — Boundaries communicate through identifiers and contracts

**Statement:** Atlas boundaries shall exchange stable primitive identifiers and versioned contracts rather than sharing each other's domain entities.

**Rationale:** This preserves ownership and permits boundaries to become independently deployed services.

**Priority:** Must  
**Status:** Partial

### Acceptance criteria

- Graph does not contain Content Document or Participant entities.
- Public events are defined as versioned contracts.
- The host composes data returned by multiple boundaries.
- Remaining direct project references are reviewed before independent deployment.

[View traceability](TRACEABILITY.md#nfr-001)

<a id="nfr-002"></a>
## NFR-002 — Integration events survive process failure

**Statement:** When Atlas adopts durable service messaging, published integration events shall survive process failure and support safe redelivery.

**Rationale:** The current in-memory publisher is educational but cannot guarantee delivery across processes.

**Priority:** Should  
**Status:** Deferred

### Acceptance criteria

- Domain persistence and event enqueueing are coordinated through an outbox or equivalent.
- Consumers handle duplicate delivery idempotently.
- Failed deliveries can be retried and observed.

[View traceability](TRACEABILITY.md#nfr-002)


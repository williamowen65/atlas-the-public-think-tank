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
**Status:** Verified

### Acceptance criteria

- The actor is checked before each protected node mutation.
- A node author can perform allowed management actions.
- An unrelated participant is denied.
- Adding a sub-node remains available to a participant who is not the parent node's author.
- Any future moderator override must be introduced as an explicit domain capability, not as a UI-only bypass.

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

<a id="tag-001"></a>
## TAG-001 — Tag definitions form reusable vocabulary

**Statement:** The system shall represent tag wording as a reusable `TagDefinition` with a stable identifier, normalized text, creator identifier, and creation time.

**Rationale:** Nodes should reuse one vocabulary entry instead of storing duplicate copies of the same wording.

**Priority:** Must
**Status:** Approved

### Acceptance criteria

- A new definition receives a stable GUID.
- The original display text and normalized text are retained separately.
- Later node workflows can select an existing definition.
- Graph owns `TagDefinition` and its lifecycle.

[View traceability](TRACEABILITY.md#tag-001)

<a id="tag-002"></a>
## TAG-002 — Equivalent tag text resolves to one definition

**Statement:** The system shall normalize proposed tag text before duplicate detection.

**Rationale:** Differences in capitalization or spacing should not create duplicate vocabulary entries.

**Priority:** Must
**Status:** Approved

### Acceptance criteria

- Input is trimmed, repeated whitespace is collapsed, Unicode text is normalized, and comparison is case-insensitive.
- `Tunnel Vision`, `tunnel vision`, and `TUNNEL  VISION` resolve to the same definition.
- Empty normalized text is rejected.
- Punctuation remains meaningful unless a later policy explicitly changes that rule.

[View traceability](TRACEABILITY.md#tag-002)

<a id="tag-003"></a>
## TAG-003 — Tags are applied through node-specific associations

**Statement:** The system shall represent the application of a tag to a node as a distinct `NodeTag` identified by `NodeTagId`, `NodeId`, and `TagId`.

**Rationale:** Reusing a definition must not merge its meaning, lifecycle, or votes across nodes.

**Priority:** Must
**Status:** Approved

### Acceptance criteria

- Applying a tag creates a node-specific association with its actor and creation time.
- The same definition can be applied independently to multiple nodes.
- A node cannot contain two active associations to the same definition.
- Graph owns `NodeTag` and references the participant by identifier.

[View traceability](TRACEABILITY.md#tag-003)

<a id="tag-004"></a>
## TAG-004 — Shared tag definitions are not renamed through a node

**Statement:** Changing tag wording on one node shall select or create another `TagDefinition` and replace that node's association.

**Rationale:** A local edit must not unexpectedly rename the reusable definition everywhere.

**Priority:** Must
**Status:** Approved

### Acceptance criteria

- Node-level editing never mutates an existing definition's text.
- Replacement removes the old association and creates or selects the requested definition.
- Other nodes using the original definition are unchanged.
- Definition-level moderation is a separate privileged workflow.

[View traceability](TRACEABILITY.md#tag-004)

<a id="tag-005"></a>
## TAG-005 — Authorized actors manage node tags

**Statement:** Active participants shall be able to apply tags, while removal and replacement shall be limited to the applying participant, the node author, or an authorized moderator.

**Rationale:** Tagging should remain open to contribution without allowing unrelated participants to erase other contributions.

**Priority:** Must
**Status:** Approved

### Acceptance criteria

- Applying a tag requires an active participant and active node.
- The applying participant can remove or replace their association.
- The node author or a moderator with the applicable capability can remove any association on that node.
- An unrelated participant cannot remove or replace another participant's association.

[View traceability](TRACEABILITY.md#tag-005)

<a id="tag-006"></a>
## TAG-006 — Archived tag targets reject mutation

**Statement:** Archived nodes and moderated tag associations shall remain readable but reject tag application, replacement, removal, and voting.

**Rationale:** Historical context should remain visible while archived or moderated targets are frozen against new activity.

**Priority:** Must
**Status:** Approved

### Acceptance criteria

- Existing tags remain visible after a node is archived.
- Graph rejects tag mutations for an archived node.
- Voting rejects casting, changing, or undoing a vote on an unavailable `NodeTag` target.
- Existing vote summaries remain readable.

[View traceability](TRACEABILITY.md#tag-006)

<a id="tag-007"></a>
## TAG-007 — Tag suggestions reuse existing definitions

**Statement:** Tag entry shall suggest existing definitions by normalized prefix or text match before offering creation.

**Rationale:** Discoverable reuse limits vocabulary fragmentation.

**Priority:** Should
**Status:** Approved

### Acceptance criteria

- Suggestions display the stored presentation text.
- Selecting a suggestion applies its existing `TagId`.
- Creation is not offered when normalized text already exists.
- No-result and lookup-failure states are distinguishable.

[View traceability](TRACEABILITY.md#tag-007)

<a id="tag-008"></a>
## TAG-008 — NodeTag votes are node-specific

**Statement:** Voting shall own one current signed vote per participant and `NodeTag`, representing whether that tag appropriately characterizes that node.

**Rationale:** Approval of a reusable phrase in one context must not become global approval everywhere it appears.

**Priority:** Must
**Status:** Approved

### Acceptance criteria

- A ballot targets `NodeTagId`, not `TagId`.
- A participant can hold at most one current vote for that target.
- Casting, changing, and undoing a vote follow Voting-owned policy.
- Applying the same definition to another node creates a separate voting target and summary.

[View traceability](TRACEABILITY.md#tag-008)

<a id="tag-009"></a>
## TAG-009 — Node views show prominent and discoverable tags

**Statement:** Node presentation shall show up to three prominent tags and provide access to the remaining tags.

**Rationale:** Strongly supported characterizations should be visible without allowing a large tag set to overwhelm the node card.

**Priority:** Should
**Status:** Approved

### Acceptance criteria

- The composition layer joins Graph-owned `NodeTag` records with Voting-owned summaries by `NodeTagId`.
- Prominence sorts by net score, then total ballots, then oldest association, then `NodeTagId` for a stable tie-break.
- At most three tags appear on a compact node card.
- All remaining tags are reachable from the node detail view.
- Graph does not store or cache vote summaries as authoritative state.

[View traceability](TRACEABILITY.md#tag-009)

<a id="vot-001"></a>
## VOT-001 — Node views report vote totals and averages

**Statement:** The system shall display the number of votes and average rating for each node without placing voting behavior in the Graph domain.

**Rationale:** Voting is part of the product experience but has an independent lifecycle and rule set.

**Priority:** Should  
**Status:** Approved

### Acceptance criteria

- Browse and detail views show total votes and average rating.
- A dedicated voting boundary owns ballots and aggregation.
- The composition layer joins voting summaries to nodes by identifier.

[View traceability](TRACEABILITY.md#vot-001)

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

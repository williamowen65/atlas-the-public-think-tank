# Validation implementation in PR #337

[PTT-128](https://thepublicthinktank.atlassian.net/browse/PTT-128), [PTT-129](https://thepublicthinktank.atlassian.net/browse/PTT-129) and [PTT-130](https://thepublicthinktank.atlassian.net/browse/PTT-130) are implemented together on the PTT-121 branch. The cards remain separate traceability units; review the code and evidence in this PR.

## Local values and history — PTT-128

Consumers reject empty/default Graph, Comment, Community and Vote IDs and runtime null required objects. Graph, Participant, Community and node-type changes cannot move their update time backward. Equal timestamps remain allowed in these domains; existing Comment and Vote mutations still require a strictly later time. An intentional repeated-operation no-op preserves history without requiring a new timestamp. Membership rejoin cannot precede the last leave.

Comment removal status, removal time and update time must agree. Moderation decision, review and restoration fields must form a complete, chronological history. Reaction changes construct and validate their audit entry before applying lifecycle/disposition changes. Notification identities are constructor-validated and immutable; delivery history is read-only outside the aggregate, and read/dismiss/delivery operations validate chronology.

| Enforcement | Focused evidence |
|---|---|
| [Node](../../../src/Atlas.Graph/Node.cs), [node types](../../../src/Atlas.Graph/Nodes/NodeTypes/NodeTypeDefinition.cs), [reactions](../../../src/Atlas.Graph/Reactions/NodeReaction.cs) | [ValidationBoundaryTests](../../../tests/Atlas.Graph.Tests/ValidationBoundaryTests.cs): defaults/nulls, unchanged values/parents/events, audit rejection |
| [Participant](../../../src/Atlas.Participants/Participants/Participant.cs), [Community](../../../src/Atlas.Communities/Communities/Community.cs), [membership](../../../src/Atlas.Communities/Memberships/CommunityMembership.cs) | [Participant boundary tests](../../../tests/Atlas.Participants.Tests/ValidationBoundaryTests.cs), [Community validation tests](../../../tests/Atlas.Communities.Tests/ValidationTests.cs): earlier edits, deactivation, rejoin, deliberate no-ops |
| [Document](../../../src/Atlas.Content/Documents/Document.cs), [Comment](../../../src/Atlas.Comments/Comments/Comment.cs), [Vote](../../../src/Atlas.Voting/Model/Vote.cs) | [Content validation tests](../../../tests/Atlas.Console.Tests/ContentValidationTests.cs), [Comment validation tests](../../../tests/Atlas.Comments.Tests/ValidationTests.cs), [Vote boundary tests](../../../tests/Atlas.Voting.Tests/ValidationBoundaryTests.cs) |
| [ModerationCase](../../../src/Atlas.Moderation/ModerationCase.cs), [Notifications](../../../src/Atlas.Notifications/Notifications.cs) | [Moderation validation tests](../../../tests/Atlas.Moderation.Tests/ValidationTests.cs), [Notification domain tests](../../../tests/Atlas.Notifications.Tests/NotificationDomainTests.cs): restored state and direct entity construction |

### Shared time policy

[AtlasTime](../../../src/Atlas.Contracts/Operations/AtlasTime.cs) uses .NET `TimeProvider.System` in production and a scoped provider in controlled tests. Backend current-time calls use this source. [Node creation](../../../src/Atlas.Console/NodeCreationService.cs) captures UTC once and reuses it for the block, document, node and creation events. Aggregate properties normalize timestamp offsets to UTC without changing the represented instant; reconstitution preserves that instant and validates history.

The application supplies time to domain methods so tests and reconstitution can use known timestamps. Future ordinary API requests must not choose audit times. Display converts UTC to the viewer's time zone, using a time-zone identifier where daylight-saving rules matter. Clock synchronization across hosts and clock adjustments remain deployment concerns; a timestamp is not a concurrency version.

## References and relationships — PTT-129

A reference check asks: **“Does this ID identify a record of the expected kind, and is that record available for this operation?”** The application decides which references it needs; the host's lookup implementation answers using stored facts. This is one part of validation. Aggregate value/history checks, authorization and graph ancestry checks still have their own owners.

### How the pieces fit together

| Piece | Responsibility | Where to read it |
|---|---|---|
| `IReferenceLookup` | Defines the question an adapter must answer: `IsAvailable(kind, id, requireActive)` returns a boolean. The interface contains no database queries. | [Contracts: ReferenceValidation.cs](../../../src/Atlas.Contracts/Operations/ReferenceValidation.cs) |
| `ReferenceValidation.Require` | Rejects an empty ID, checks that the supplied adapter implements the contract, calls `IsAvailable`, and throws if the reference is unavailable. It turns a boolean answer into an enforced precondition. | [Contracts: ReferenceValidation.cs](../../../src/Atlas.Contracts/Operations/ReferenceValidation.cs) |
| Application service | Chooses the required references and whether availability or existence alone is needed, then calls `Require` before changing state or saving. | [NodeCreationService](../../../src/Atlas.Console/NodeCreationService.cs), [CommunityService](../../../src/Atlas.Communities/Communities/CommunityService.cs) |
| `SqlRepository` | Implements the lookup once in the host's storage layer. Its `kind` switch selects EF queries for Participants, Nodes, Documents, Blocks, NodeTypes, Communities or NodeReactions. | [Console storage: SqlRepository](../../../src/Atlas.Console/Storage/SqlRepository.cs) |
| Concrete SQL repository | Supplies its domain's persistence operations and inherits the shared lookup implementation. It does not need to repeat `IsAvailable`. | [SqlNodeRepository](../../../src/Atlas.Console/Storage/SqlNodeRepository.cs), [SqlCommunityRepository](../../../src/Atlas.Console/Storage/SqlCommunityRepository.cs) |
| Database context | Provides access to the mapped tables in the shared Atlas database. The SQL lookup uses these tables directly through EF. | [AtlasDataContext](../../../src/Atlas.Persistence/AtlasDataContext.cs) |

**The contract is a capability; inheritance is how the current SQL adapter provides it.** For example, `SqlNodeRepository` inherits from `SqlRepository`, which implements `IReferenceLookup`. A `SqlNodeRepository` instance therefore supports both `INodeRepository` and `IReferenceLookup`.

This code lives in the Console project because Console is the current host/composition layer. “Host” means the backend that wires services to adapters, not a browser or form. A future REST host can provide the same capability without putting SQL queries into domain aggregates.

### Follow one call: validate a Node's author

[NodeCreationService.Create](../../../src/Atlas.Console/NodeCreationService.cs) receives `nodes` as an `INodeRepository`. In the SQL-backed host, the actual object is a `SqlNodeRepository`. It calls:

```csharp
ReferenceValidation.Require(nodes, "Participant", authorId);
```

1. **The helper checks the input and capability.** An empty `authorId` is rejected immediately. Although the parameter's declared type is `INodeRepository`, the runtime object also implements `IReferenceLookup` through its base class. `Require` checks for that interface; an adapter without it fails explicitly.
2. **The same repository instance answers the lookup.** `Require` calls its inherited `IsAvailable("Participant", authorId, true)`. There is no separate lookup object created and no call to `SqlParticipantRepository` here.
3. **The shared implementation queries the appropriate table.** The `"Participant"` branch asks EF whether `ParticipantRows` contains that ID with `IsActive == true`. A Node repository can answer this cross-domain question because its shared base implementation has access to the common Atlas database context.
4. **The answer controls continuation.** `false` causes `Require` to throw before Node construction or writes. `true` lets the service proceed to its other reference checks and local aggregate validation. It does not establish that the caller is authenticated or authorized.

```mermaid
sequenceDiagram
    participant Service as NodeCreationService
    participant Guard as ReferenceValidation
    participant Repo as SqlNodeRepository
    participant Context as AtlasDataContext
    participant DB as SQL Server
    Service->>Guard: Require(nodes, Participant, authorId)
    Guard->>Guard: Check nonempty ID and IReferenceLookup
    Guard->>Repo: IsAvailable(Participant, authorId, true)
    Note over Repo: Inherited from SqlRepository
    Repo->>Context: Query ParticipantRows for active author
    Context->>DB: Execute parameterized existence query
    DB-->>Context: Match or no match
    Context-->>Repo: Boolean result
    Repo-->>Guard: Available or unavailable
    alt Available
        Guard-->>Service: Return; continue validation
    else Missing or inactive
        Guard-->>Service: Throw; stop before mutation or saves
    end
```

The caller chooses the policy; `SqlRepository.IsAvailable` is the central implementation of the **reference availability facts**, rather than all validation. With `requireActive: true` (the helper's default), Participants must be active, NodeTypes unarchived, Communities active, and Nodes active and not currently hidden by moderation. An active reaction also needs an available parent Node. Documents and Blocks currently have only an existence check. With `requireActive: false`, these branches check existence while allowing historical/inactive references. Unsupported kinds return `false`.

The current wiring passes the repository object into the helper and discovers its lookup capability at runtime; the individual domain repository interfaces do not themselves require `IReferenceLookup`. Test adapters that exercise these services must explicitly implement that capability too. If another backend replaces SQL, it must provide equivalent facts and policy behavior.

### How reference checks connect to saving

The [operation boundary](../../../src/Atlas.Contracts/Operations/OperationBoundary.cs) starts the SQL transaction before these service checks. [SqlOperationBoundary](../../../src/Atlas.Console/Storage/SqlOperationBoundary.cs) lets the lookup and subsequent repository saves reuse that operation's database context. Node creation validates the author/types/optional parent, constructs valid local objects, then saves the block, document and Node. [SqlDocumentRepository](../../../src/Atlas.Console/Storage/SqlDocumentRepository.cs) verifies block references, and [SqlNodeRepository.Save](../../../src/Atlas.Console/Storage/SqlNodeRepository.cs) repeats its reference and ancestry checks before the Node write. Those save-boundary checks also protect callers using the repository directly.

SQL foreign keys remain the final protection for mapped relationships. A successful lookup does not replace those constraints, authorize an actor, or detect a transitive graph cycle. The graph traversal described below handles ancestry; the transaction and competing-write protection are covered under [PTT-130](#complete-operations--ptt-130).

### Policies by operation

| Operation | Enforced policy |
|---|---|
| Create Node | Active author, existing description document and blocks, existing unarchived selected/requested types, active and publicly available parent |
| Change/request Node types | Resolve new types before changing the aggregate; preserve unchanged historical references |
| Attach parent | Active, publicly available child/parent and active actor; check full ancestry and detect stale loaded relationship state before mutation |
| Create/join/associate Community | Active owner/participant and active community; associations require an active, publicly available Node. Leaving/removing retains history. |
| Add/edit/reply Comment | Active actor and available Node; reject unsupported target kinds. Replies inherit their parent's target and cannot predate it. Removed ancestors may receive replies, preserving the existing thread policy. |
| Cast/change Vote | Active participant; target kind must resolve to an active, publicly available Node or an active reaction on such a Node |
| Notification request | Active actor/recipient and supported kind/Node subject; discussion requires an available Node, moderation may refer to a hidden Node. Validate before duplicate/self shortcuts. |
| Historical updates | Existing references may remain when their participant/type/parent later becomes inactive; creation and new relationships require availability. SQL FKs retain mapped existence protection. |

[NodeRelationshipService](../../../src/Atlas.Graph/Nodes/NodeRelationshipService.cs) validates relationships and type proposals before mutation. [GraphRelationshipValidation](../../../src/Atlas.Graph/Nodes/GraphRelationshipValidation.cs) traverses ancestry to reject direct/transitive cycles and missing parents, while allowing several parents and shared ancestors. [SqlNodeRepository](../../../src/Atlas.Console/Storage/SqlNodeRepository.cs) repeats graph/reference checks at the write boundary.

Evidence: [Graph boundary tests](../../../tests/Atlas.Graph.Tests/ValidationBoundaryTests.cs), [SQL operation tests](../../../tests/Atlas.Persistence.Tests/SqlOperationValidationTests.cs) for missing documents/types, inactive authors, missing/wrong-kind/hidden vote targets and competing parent links; [Comment tests](../../../tests/Atlas.Comments.Tests/CommentTests.cs) for unavailable targets; [Notification tests](../../../tests/Atlas.Notifications.Tests/NotificationDomainTests.cs) for shortcut validation. This is evidence for these named scenarios, not every possible reference combination.

## Complete operations — PTT-130

[IOperationBoundary](../../../src/Atlas.Contracts/Operations/OperationBoundary.cs) wraps the complete application use case. [SqlOperationBoundary](../../../src/Atlas.Console/Storage/SqlOperationBoundary.cs) shares one EF context and serializable transaction across nested repositories using the same connection configuration. A transaction-owned `sp_getapplock` serializes the current MVP write workflows. It covers both the precheck and related saves, including graph ancestry. Different database configurations cannot join the boundary.

| Workflow | Writes committed or rolled back together | Failure-injection evidence |
|---|---|---|
| [NodeCreationService](../../../src/Atlas.Console/NodeCreationService.cs) | Block, document, Node and parent relationship | `FailedNodeSaveRollsBackBlocksAndDocument` |
| [CommunityService.Create](../../../src/Atlas.Communities/Communities/CommunityService.cs) | Community and owner membership | `FailedOwnerMembershipRollsBackCommunity` |
| [Reaction replacement](../../../src/Atlas.Graph/Reactions/NodeReactionApplicationService.cs) | Replacement and old association's supersession/audit | `FailedSupersessionSaveRollsBackReplacement` |
| [Grouped moderation](../../../src/Atlas.Moderation/ModerationService.cs) | All decisions, review requests or restorations in the group | `FailedSecondModerationSaveRollsBackWholeGroup` and `FailedGroupedReviewAndRestorationPreservePersistedHistory` |

All named tests above are in [SqlOperationValidationTests](../../../tests/Atlas.Persistence.Tests/SqlOperationValidationTests.cs). Further tests cover competing community names/vote inserts, competing moderator decisions, opposite parent attachments and translation of a SQL unique violation into [OperationConflictException](../../../src/Atlas.Contracts/Operations/OperationBoundary.cs). Constraints remain final protection. No EF model or migration change is required.

### Limits to retain during review

The database-wide write lock is intentionally coarse and limits write throughput. Refine its granularity only with equivalent concurrency evidence. These boundaries are synchronous, use one database and cover cooperating repository operations. They do not make arbitrary direct `DbContext` writes follow application policy, provide universal optimistic version checks for every stale object, or coordinate external delivery/event publication with SQL.

Validation rejects local invalid proposals before mutation. A later persistence failure rolls back database writes; it does not rewind domain objects already changed in caller memory. Reload after failure before retrying. In-memory test adapters without an `IOperationBoundary` do not claim SQL rollback semantics.

Notifications persist before external delivery. An outbox/reliable delivery mechanism and general stale-object versioning are separate architectural work. Poll/chart existence remains deferred until those domains exist. Resource limits, wider failure paths and future browser/HTTP boundaries remain [PTT-122](https://thepublicthinktank.atlassian.net/browse/PTT-122), [PTT-123](https://thepublicthinktank.atlassian.net/browse/PTT-123) and [PTT-125](https://thepublicthinktank.atlassian.net/browse/PTT-125).

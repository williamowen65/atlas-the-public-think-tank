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

[IReferenceLookup](../../../src/Atlas.Contracts/Operations/ReferenceValidation.cs) supplies existence/activity facts through a backend port. Missing adapters fail explicitly. [SQL repositories](../../../src/Atlas.Console/Storage/SqlRepository.cs) implement the port; aggregates do not query another domain's database. These facts complement authentication and authorization and do not replace them.

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

# MVP domain validation inventory

Paths below are relative to the repository root. **Existing** means enforcement was inspected; named tests provide the stated evidence. **Fixed** refers to changes in PTT-121. **Gap** remains open with tracking. Coverage includes public aggregate operations, application services and current host adapters; no HTTP endpoint exists in this review.

## Graph and reactions

| Operation / expectation | Enforcement | Evidence | Result / follow-up |
|---|---|---|---|
| Construct/rename: nonblank trimmed title, at most 200 characters; nonempty author/description | `src/Atlas.Graph/Nodes/NodeTitle.cs`, `NodeAuthorId.cs`, `NodeDescriptionId.cs` | `tests/Atlas.Graph.Tests/NodeValueObjectTests.cs` | Existing value checks; null objects can still bypass intended usage: PTT-128 |
| Construct/change type: valid required type; persisted node identity | `NodeTypeId` and `NodeId` are positional record structs; `Node.ChangeType` assigns a supplied ID | Inspected `src/Atlas.Graph/Node.cs` and ID types | Gap: empty/default IDs and reference existence are not uniformly checked. PTT-128/129 |
| Request/stop requesting subtypes: nonempty type IDs; unique collection; collection required on construction | `RequestedSubNodeType`, `Node.CreateRequestedSubNodeTypes` | `NodeConstructionTests`, `NodeMutationTests`, `NodeValueObjectTests` | Existing local checks; catalog existence requires PTT-129 |
| Attach/detach/reconstitute parents: reject self/empty IDs; deduplicate | `Node.EnsureValidParentNodeId`, `CreateParentNodeIds` | `NodeParentBehaviorTests`, `NodeReconstitutionTests` | Existing local checks. Transitive cycles and missing parents are not rejected by the aggregate; FK is not a cycle check. PTT-129 |
| Archive/restore: defined lifecycle; repeated transition is a no-op | `Node.Archive`, `Restore`, private reconstitution constructor | `NodeLifecycleBehaviorTests`; `ValidationTests.NodeReconstitutionRejectsUndefinedStatus` | Fixed undefined restored status. Mutation times can regress: PTT-128. Archived-edit policy must be explicit rather than inferred from UI. |
| Custom/system node types: required trimmed name ≤50, description ≤500, custom owner required; governed edits | `NodeTypeDefinition` | `NodeTypePluralizationTests`; inspected naming validators | Existing. Reconstitution does not enforce custom owner consistency; IDs/timestamps: PTT-128; owner/reference policy: PTT-129 |
| Reaction definitions: required text ≤24, emoji, description ≤160; normalized text matches display text | `ReactionDefinition` | `ReusableReactionTests.Definition_RequiresEmojiAndDescription`; inspected normalization | Existing; storage uniqueness is a final check. |
| Apply curated reaction: normalized catalog text, not suppressed, active node/actor, reuse existing association | `NodeReactionApplicationService.Apply` / `ResolveDefinition` | `ReusableReactionTests` | Existing. Actor activity is a host fact, not a caller-selectable transport value. |
| Remove/replace/disposition: association belongs to supplied node, applicable lifecycle/authority, chronological audit | `NodeReactionApplicationService`, `NodeReaction.EnsureAuditTime` | `ReusableReactionTests` plus Graph `ValidationTests` for rejection without changing audit/state | Fixed undefined disposition/lifecycle/audit enums. Replacement is saved before old association's time validation; PTT-130. Direct `Remove`/`Supersede` can change lifecycle before invalid audit actor/reference rejection; PTT-128. |

## Content

| Operation / expectation | Enforcement | Evidence | Result / follow-up |
|---|---|---|---|
| Create/reconstitute/update block: payload checks independent of UI | `src/Atlas.Content/Blocks/ContentBlock.cs` and all six `Blocks/Types` classes | `tests/Atlas.Console.Tests/DocumentPersistenceTests.BlockTypesEnforceTheirOwnValidation` | Existing text/reference checks. All six updates now validate before assignment. |
| Image: required URL text ≤500, alt text ≤2,000, optional caption ≤2,000; Video: URL text ≤500, caption ≤2,000 | `ImageBlock`, `VideoBlock` | `DocumentPersistenceTests`, inspected validators | URL is a media reference string; existing tests deliberately accept `media/image-1`. Do not impose HTTP-only semantics without an asset policy. Future renderer/fetcher policy: PTT-125 |
| Link preview: absolute HTTP(S), title ≤500, description ≤2,000 | `LinkPreviewBlock`, `ContentBlock.HttpUrl` | `DocumentPersistenceTests.BlockTypesEnforceTheirOwnValidation`; `ContentValidationTests` | Existing scheme restriction; does not prevent SSRF if a future backend fetches the URL. PTT-125 |
| Markdown ≤100,000; optional empty Markdown allowed | `MarkdownTextBlock` / `Optional` | Inspected constructor/update helper; stale-time regression in `ContentValidationTests` | Existing bound; browser rendering remains PTT-125 |
| Poll/chart: nonempty reference, optional chart title ≤500 | `PollReferenceBlock`, `ChartReferenceBlock` | `DocumentPersistenceTests` | Existence is intentionally deferred while poll/chart domains do not exist; convenience constructors generate placeholders. |
| Document construction/add/move/remove: unique blocks, valid membership and index; block/document references exist | `src/Atlas.Content/Documents/Document.cs`; mapped document-block FK | `DocumentPersistenceTests`; `ContentValidationTests.StaleTimeDoesNotChangeDocumentComposition`; `SqlReferentialIntegrityTests` | Fixed stale-time partial list edits. Null block IDs/elements and missing blocks before SQL write: PTT-128/129 |
| Block/document time: restored update ≥creation; mutation ≥current update; rejection changes nothing | Content constructors, `ChangedAt`, local payload validation | `ContentValidationTests` checks all six types and all three list operations | Fixed; equal timestamps accepted. Persistence transaction coordination is separate: PTT-130 |

## Participants and Identity

| Operation / expectation | Enforcement | Evidence | Result / follow-up |
|---|---|---|---|
| Create/edit profile: trimmed required name ≤80, bio ≤500; validate all fields before profile update | `src/Atlas.Participants/Participants/Participant.cs`, `Profiles/UpdateParticipantProfile.cs` | `ParticipantTests.UpdateProfile_WithInvalidBio_DoesNotPartiallyRename`, `UpdateParticipantProfileTests` | Existing atomic payload update. Rename/update/deactivate times can regress; default IDs/null object handling: PTT-128 |
| Register: framework email/password validation; caller cannot choose account IDs or roles; duplicate profile/account rejected | `src/Atlas.Identity/AtlasAccounts.cs`, `IdentityServices.cs`; `UserManager`; SQL transaction | `tests/Atlas.Identity.Tests/IdentityServicesTests.cs`, `SqlIdentityTests.cs` | Existing. Do not duplicate Identity password/email validators in domain code. |
| Resolve/sign in: malformed/empty identity claim, missing/inactive participant, unconfirmed/locked account fail | `AtlasAccounts.ResolveParticipantAsync`, `AtlasSignInManager`, `AtlasAuthorization` | `IdentityServicesTests`, `SqlIdentityTests`, Console `ConsoleIdentitySessionTests` | Existing. Host derives actor; domain references alone do not establish authentication. |

## Voting

| Operation / expectation | Enforcement | Evidence | Result / follow-up |
|---|---|---|---|
| Cast/change: target and participant required, nonempty IDs; rating 0–10, reaction vote exactly ±1 | `VoteTarget`, `ParticipantId`, `NodeImportanceRating`, `NodeReactionVote`, `Vote.CreateValue` | `tests/Atlas.Voting.Tests/VotingTests.cs`, `NodeReactionVotingTests.cs` | Existing value checks. Direct `Vote` creation/reconstitution can receive null participant; `VoteId` permits empty/default: PTT-128 |
| Cast/change/undo: participant eligible and target available; one current vote per participant/target | `VoteMutationPolicy`, `CastVote`, `UndoVote`; `RepositoryVotingEligibility`; unique SQL index | Voting tests; `tests/Atlas.Persistence.Tests/SqlVoteRepositoryTests.cs` | Existing application rule and SQL uniqueness. Polymorphic target has no ordinary target FK; retain availability port: PTT-129 |
| Reconstitute/change time: restored update ≥creation; real value change has later time | `Vote` constructor / `ChangeValue` | `VotingTests` | Existing chronological checks; repository saves are not substitutes for policy checks. |

## Communities

| Operation / expectation | Enforcement | Evidence | Result / follow-up |
|---|---|---|---|
| Create/rename/change description: name required ≤100, description ≤1,000; unique normalized name | `Community`, `CommunityService.EnsureUniqueName`; `SqlCommunityRepository` and unique SQL index | `tests/Atlas.Communities.Tests/CommunityTests.cs`; `SqlCommunityRepositoryTests` | Existing checks. Creation saves community before owner membership: PTT-130 |
| Archive/restore/reconstitute: defined status, owner changes; repeat transition no-op | `Community` | `CommunityTests`; Communities `ValidationTests.ReconstitutionRejectsUndefinedStatus` | Fixed enum. Times/default IDs need PTT-128 |
| Join/leave: active community for joining, nonempty participant, owner cannot leave, leave ≥join | `CommunityService`, `CommunityMembership` | `CommunityTests.ParticipantCanLeaveAndRejoinButOwnerCannotLeave` | Existing local checks; participant existence/activity mostly SQL/host, rejoin can predate last leave: PTT-128/129 |
| Associate node: active community, nonempty node/participant, stable community/node pair | `CommunityNodeAssociation`, `CommunityService`, SQL repositories/FKs | `CommunityTests.NodeCanBelongToMultipleCommunitiesWithoutDuplication`; `SqlCommunityRepositoryTests` | Existing ID/pair checks; backend availability policy beyond existence: PTT-129 |

## Comments

| Operation / expectation | Enforcement | Evidence | Result / follow-up |
|---|---|---|---|
| Add/reply: body required ≤10,000, target kind required, target/author nonempty; parent exists and reply inherits target | `Comment`, `CommentTarget`, `CommentService.Reply` | `tests/Atlas.Comments.Tests/CommentTests.cs` | Existing. Participant existence/activity and default `CommentId` require PTT-128/129 |
| Add/edit/reply: target available; unsupported kind fails availability adapter | `CommentService.EnsureTargetAvailable`; `src/Atlas.Console/Comments/NodeCommentTargetAvailability.cs` | `CommentTests.Mutations_WhenTargetUnavailable_AreRejectedButThreadRemainsReadable` | Existing adapter checks Node existence/active status. Broader hidden-node availability policy: PTT-129 |
| Edit/remove: active comment, correct actor, increasing change time; removed ancestors may still receive replies | `Comment` / `CommentService` | `CommentTests` | Existing deliberate thread policy. Reconstituted status/removal-time consistency still needs PTT-128 |
| Reconstitution: defined status, chronological times, not self-parent | `Comment` private constructor | Comments `ValidationTests.ReconstitutionRejectsUndefinedStatus`; inspected constructor | Fixed enum; constructor-validated `CommentId` can be bypassed by default(struct): PTT-128 |

## Discovery

| Operation / expectation | Enforcement | Evidence | Result / follow-up |
|---|---|---|---|
| All four query methods: positive page, nonnegative/ordered vote-count bounds, finite/ordered rating bounds within 0–10, ordered dates | `src/Atlas.Discovery/DiscoveryService.cs` / `Validate` before source enumeration | `DiscoveryServiceTests`, especially `DiscoverRejectsNonFiniteBoundsBeforeReadingCandidates` | Fixed NaN escape; finite guard includes infinities. |
| Text/reaction/ID filters; author-scope collection | Trimmed search; reactions use AND matching; null reactions treated as none; null authored collection rejected | `DiscoveryServiceTests` | Empty/unknown filter GUIDs currently give no matches rather than a mutation error. Future transport should document this semantics. Search/collection size budgets: PTT-122 |
| Pagination arithmetic must not overflow for large positive pages | Long offset calculation; empty page beyond available results | Inspected `DiscoveryService.Page`; pagination tests | Existing. All candidates still load/rank before slicing; resource review: PTT-122 |

## Moderation

| Operation / expectation | Enforcement | Evidence | Result / follow-up |
|---|---|---|---|
| Report: nonempty case/node/reporter IDs; required reason ≤100; explanation ≤2,000; title required; hidden node cannot be reported again | `src/Atlas.Moderation/ModerationCase.cs`, `ModerationService.ReportNode` | `ModerationWorkflowTests` | Existing. Node/reference facts still host/database concerns: PTT-129 |
| Decide: defined enums, reviewer/rationale, valid time, pending cases only | `ModerationCase.Decide`; `ModerationService.DecideNode` prevalidates each case | `ModerationWorkflowTests` | Existing local validation; multi-save group transaction still PTT-130 |
| Restore/review: hidden case, pending review, edit after decision, ordered times and moderator rationale | `ModerationCase`, `ModerationService` | `ModerationWorkflowTests.Hidden_node_shows_public_reason_and_author_review_can_restore_visibility` | Existing workflow. Full restored-field consistency and request time relative to node edit need PTT-128; multiple saves: PTT-130 |
| Reconstitute decided case: reviewer, rationale and actual timestamp | `ModerationCase` constructor | Moderation `ValidationTests.DecidedCaseRequiresDecisionTimeAndBoundedRationale` | Fixed nullable timestamp escape and missing rationale bound |

## Notifications

| Operation / expectation | Enforcement | Evidence | Result / follow-up |
|---|---|---|---|
| Handle: occurrence/recipient/subject required, supported kind, no duplicate delivery for same occurrence/recipient | `src/Atlas.Notifications/Notifications.cs` / `NotificationService.Handle`; unique SQL index | `tests/Atlas.Notifications.Tests/NotificationDomainTests.cs`; persistence `NotificationFlowTests` | Existing. Actor and subject-kind policy not explicit; self-notification returns before supported-kind check. PTT-129 |
| Page: nonnegative offset, limit 1–100; preferences/read/dismiss addressed to owner | `NotificationService.Page`, `EnsureOwner`, `Own` | `NotificationDomainTests` for preferences/read ownership; inspected page bounds | Empty recipient/actor IDs not uniformly rejected; two empty IDs pass equality in preferences: PTT-128 |
| Notification entity/preferences/delivery state | Public init/set properties and mutable attempts list; service populates normal flow | Inspected `Notifications.cs` | Direct objects can bypass service validation; do not bind transport payloads directly to these entities. Full lifecycle/enum/default handling: PTT-128; HTTP DTOs: PTT-125 |

## Cross-boundary write behavior

Payload validation and authorization should complete before durable changes. PTT-121 fixes local Content mutation ordering, but does not make every multi-repository workflow atomic. `NodeCreationWorkflow` writes Content before all Node validation/persistence completes; community creation writes before membership; reaction replacement writes before supersession time checks; grouped moderation writes cases separately. Each SQL repository transaction protects its own save, not the entire use case. [PTT-130](https://thepublicthinktank.atlassian.net/browse/PTT-130) tracks application transaction coordination, failure injection and concurrency tests.

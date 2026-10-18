# Proposed event inventory

[Overview and field definitions](README.md). The semantic catalogue below remains the target design. [The running guide](LOCAL-DEVELOPMENT.md#what-is-saved-where) lists the actual changed-record events and implemented coverage; not every proposed semantic name below is emitted yet. The linked tests establish existing business/enforcement behavior; new event assertions belong to the implementation follow-ups.

## Event classes

- **Security signal:** an authentication/authorization outcome or suspicious failure. Useful for investigation and aggregation, but not automatically guaranteed durable delivery.

- **Audit action:** an important committed mutation requiring a durable record alongside its state change. Start with moderation and privileged security changes; review other retention needs separately.

- **Operational diagnostic:** a technical outcome such as a storage conflict or delivery failure. It must not leak the payload involved.

Every event uses the [common envelope](README.md#common-event-fields). The additions below are bounded categories, identifiers or counts; they never authorize arbitrary object serialization.

## MVP coverage and proposed emitters

| Boundary / proposed events | Proposed location and timing | Additional allowlisted fields | Current enforcement/history evidence | Future use |
|---|---|---|---|---|
| Identity: `identity.registration_completed`, `identity.sign_in_completed`, `identity.sign_out_completed`, `identity.session_rejected` | [AtlasAccounts](../../../src/Atlas.Identity/AtlasAccounts.cs), [ConsoleIdentitySession](../../../src/hosts/Atlas.Console/Identity/ConsoleIdentitySession.cs); record final coordinator outcome, not merely successful password verification | Flow kind, bounded result such as locked/unconfirmed/2FA-required; no submitted email/password/token | [SqlIdentityTests](../../../tests/Atlas.Identity.Tests/SqlIdentityTests.cs), [ConsoleIdentitySessionTests](../../../tests/Atlas.Console.Tests/ConsoleIdentitySessionTests.cs); event capture not tested yet | Security investigation; aggregate sign-in denial/lockout metrics |
| Privileged security: `identity.role_changed`, `identity.account_security_changed` | Future trusted administrator/account workflow around framework managers; successful durable record shares its transaction | Subject ID, role name or bounded security-action category; no old/new credential or token | [Identity authorization](../authorization/Identity.md); a complete privileged provisioning/audit workflow is still to be designed | Durable audit and later alerts; public registration cannot assign privileged roles |
| Authorization: `authorization.denied` | [AtlasRoleHandler](../../../src/Atlas.Identity/AtlasAuthorization.cs) and application command boundary after domain rejection; avoid double-emitting the same denial | Policy/operation name and bounded reason; do not reveal ownership/target existence to the caller | [Authorization review](../authorization/README.md) links per-domain tests | Detect denied mutation patterns; no per-read-success audit flood |
| Participants: `participant.profile_changed`, `participant.state_changed` | [UpdateParticipantProfile](../../../src/Atlas.Participants/Profiles/UpdateParticipantProfile.cs), appropriate state-change coordinator; success after commit | Changed field names or state transition, not profile text | [UpdateParticipantProfileTests](../../../tests/Atlas.Participants.Tests/UpdateParticipantProfileTests.cs), [Participants review](../authorization/Participants.md) | Mutation diagnosis; elevated account-state audit policy still open |
| Graph: `node.created`, `node.metadata_changed`, `node.parents_changed`, `node.lifecycle_changed`, `node_type.changed` | [NodeCreationService](../../../src/Atlas.Application/NodeCreationService.cs), [NodeCommands](../../../src/hosts/Atlas.Console/NodeCommands.cs) until reusable coordination exists; record once per complete workflow | Node/type IDs, action category, parent-count change; no title/body dumps | [NodeAuthorizationTests](../../../tests/Atlas.Graph.Tests/NodeAuthorizationTests.cs), [SQL operation tests](../../../tests/Atlas.Persistence.Tests/SqlOperationValidationTests.cs) | Investigate state changes; quota rejection is a security signal, not successful creation |
| Content: `content.changed` | Existing [Content authorization boundary](../authorization/Content.md), then reusable command coordinator under PTT-131 | Document/block IDs, content kind and changed-field names; no Markdown/captions/URLs/payload | [ContentValidationTests](../../../tests/Atlas.Console.Tests/ContentValidationTests.cs), [SQL rollback evidence](../validation/IMPLEMENTATION.md#workflows-and-evidence) | Diagnose rejected or committed edits; decide durable history separately |
| Voting: `vote.changed` | [CastVote](../../../src/Atlas.Voting/CastVote.cs) and undo coordinator, after operation result is established | Target kind/ID, cast/update/undo action; omit rating value by default | [VotingTests](../../../tests/Atlas.Voting.Tests/VotingTests.cs), [Voting architecture](../../architecture/VOTING.md) | Rejection/conflict investigation and aggregate activity; no promise to retain undone votes forever |
| Reactions: `reaction.changed` | [NodeReactionApplicationService](../../../src/Atlas.Graph/Reactions/NodeReactionApplicationService.cs); coordinate replacement/supersession as one workflow | Reaction/definition IDs, lifecycle/disposition category | [NodeReaction business history](../../../src/Atlas.Graph/Reactions/NodeReaction.cs), [Graph authorization](../authorization/Graph.md), [SQL operation tests](../../../tests/Atlas.Persistence.Tests/SqlOperationValidationTests.cs) | Existing history remains authoritative for its domain; telemetry is additional context |
| Communities: `community.changed`, `community.membership_changed`, `community.node_link_changed` | [CommunityService](../../../src/Atlas.Communities/Communities/CommunityService.cs), after complete transaction | Community/node IDs, join/leave/link/unlink category; no community description | [CommunityTests](../../../tests/Atlas.Communities.Tests/CommunityTests.cs), [Communities review](../authorization/Communities.md) | Investigate changes and denials; privileged membership actions need retention review |
| Comments: `comment.changed` | [CommentService](../../../src/Atlas.Comments/Comments/CommentService.cs), after committed add/edit/remove | Comment/parent/target IDs, action category; no comment text | [Comments review](../authorization/Comments.md), [validation evidence](../validation/IMPLEMENTATION.md) | Mutation outcome and notification correlation |
| Moderation: `moderation.report_created`, `moderation.decision_recorded`, `moderation.visibility_changed` | [ModerationService](../../../src/Atlas.Moderation/ModerationService.cs); durable decision/visibility audit in business transaction | Case/node IDs, decision/public reason codes, affected-report count; no report/private content | [ModerationCase](../../../src/Atlas.Moderation/ModerationCase.cs), [ModerationWorkflowTests](../../../tests/Atlas.Moderation.Tests/ModerationWorkflowTests.cs), [SqlModerationRepositoryTests](../../../tests/Atlas.Persistence.Tests/SqlModerationRepositoryTests.cs) | Reconstruct hide/restore and grouped decisions; action on two reports is one coordinated workflow |
| Notifications: `notification.delivery_completed`, `notification.preference_changed` | [Notifications](../../../src/Atlas.Notifications/Notifications.cs) coordinator; delivery outcome differs from notification creation | Notification/occurrence IDs, channel, bounded attempt/result category; no notification body or recipient address | [NotificationFlowTests](../../../tests/Atlas.Persistence.Tests/NotificationFlowTests.cs), [notification architecture](../../architecture/NOTIFICATIONS.md) | Failure/backlog monitoring; durable delivery reliability remains separate work |
| Discovery: `discovery.read_completed` diagnostics when useful | [SqlDiscoveryReadQuery](../../../src/Atlas.Persistence/Discovery/SqlDiscoveryReadQuery.cs); capture duration/outcome once | Page size, returned count, cache outcomes; omit filters, search terms and cache keys | [DiscoveryCacheTests](../../../tests/Atlas.Persistence.Tests/DiscoveryCacheTests.cs), [performance evidence](../../architecture/discovery-performance.md) | Prefer aggregate latency/cache metrics; no audit record for every anonymous browse |
| Storage/host: `operation.conflicted`, `operation.failed`, `host.started`, `host.stopped`, `audit.delivery_failed` | [SqlOperationBoundary](../../../src/Atlas.Application/Storage/SqlOperationBoundary.cs), outer host exception boundary and future exporter worker | Bounded error/conflict category, attempt count and pending age; no raw SQL/exception/connection details | [SQL operation tests](../../../tests/Atlas.Persistence.Tests/SqlOperationValidationTests.cs); exporter not implemented | Diagnose concurrency, startup and pipeline health |

Event names are suggested conventions, not a requirement to emit every listed event at Information level. Routine successes can use metrics and selectively enabled diagnostics. Required audit actions must have a documented durable record.

## Outcome examples

- **Wrong password:** `identity.sign_in_completed` with `outcome=denied`, anonymous actor and a generic credential-denial category. Do not attach a submitted account identifier as if it were the authenticated actor.

- **Password accepted but 2FA required:** no succeeded sign-in event yet. Record the incomplete/denied flow with a bounded reason. Console currently refuses the unsupported 2FA flow.

- **Node creation denied by quota:** no `node.created` success record. Record one denied command outcome with `reason_code=quota_exceeded`; retain no submitted description.

- **Grouped moderation succeeds:** the durable decision/visibility record includes the node and affected report count. A shared operation ID connects related case records without treating each inner save as a separate completed action.

- **Transaction rolls back:** no durable success audit remains. One failure/conflict signal can describe the attempted operation with a safe reason code.

- **Required cloud audit delivery:** preserve the event ID and original occurrence time across retries and restarts. Keep the pending record until durable acceptance is acknowledged; exhausted retries retain it for operator replay. Ingestion time/attempt metadata may change. Do not create a new business-action audit record for each export attempt.

## Deliberate exclusions

- Public reading of nodes/profiles, each breadcrumb hop and every cache lookup do not require individual durable audit records. Use aggregate metrics and correlated traces where useful.

- This proposal does not introduce analytics about what individual participants read or search. If product analytics is later wanted, it needs its own purpose, consent/privacy and retention decisions.

- Authentication secrets and user content remain excluded even when a validation error or exception occurs.

- Current domain history remains in its owning boundary. A future centralized audit ledger must not replace domain invariants or copy complete aggregates indiscriminately.

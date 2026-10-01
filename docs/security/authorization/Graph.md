# Graph Authorization Review

PTT-120 review of authorization at the `Atlas.Graph` boundary.

## Boundary summary

Graph owns Atlas nodes, including node identity, title, type, author, lifecycle status, requested sub-node types, parent relationships, and Graph domain events.

Current node mutation policy is author ownership: the participant recorded as `Node.AuthorId` is the actor permitted to modify the node.

## Authorization inventory

| Boundary / operation | Data/action | Required access | Current enforcement | Test/evidence | Result / follow-up |
|---|---|---|---|---|---|
| Read/browse node | Read public node state | Public | Current browsing/query paths treat nodes as public Atlas content | Existing Graph/Console behavior | **Covered conceptually**; future API public-read endpoint tests later |
| Create node | Create public Atlas content | Authenticated Member at host/application boundary | Node constructor records supplied author but does not authenticate that GUID | Current Console uses authenticated actor after Identity integration | **Current host covered; future API enforcement:** host must derive author from authenticated Participant rather than request body |
| Rename node | Modify title | Author | `Node.Rename` calls `EnsureAuthoredBy` before mutation | `NodeAuthorizationTests`, `SecurityAuthorizationTests` | **Covered** |
| Change node type | Modify type | Author | `Node.ChangeType` calls `EnsureAuthoredBy` | `NodeAuthorizationTests` | **Covered** |
| Request/stop requesting sub-node type | Modify requested response types | Author | Both mutations call `EnsureAuthoredBy` | `NodeAuthorizationTests` | **Covered** |
| Attach/detach parent | Modify graph relationships | Author | Both relationship mutations call `EnsureAuthoredBy` before changing state/events | `NodeAuthorizationTests` | **Covered** |
| Archive node | Change lifecycle state | Author | `Node.Archive` calls `EnsureAuthoredBy` before mutation/event | `NodeAuthorizationTests`, `SecurityAuthorizationTests` | **Covered** |
| Restore node | Change lifecycle state | Author | `Node.Restore` calls `EnsureAuthoredBy` before mutation/event | `NodeAuthorizationTests` | **Covered** |
| Explicit author check | Establish author-owned workflow permission | Author; non-empty actor required | `EnsureAuthoredBy` rejects missing actor and foreign actor | PTT-120 security regression tests | **Covered** |
| Moderator hide/exclude behavior | Moderation of node visibility | GlobalModerator/Administrator through Moderation boundary | Moderation is intentionally separate from author-owned Graph lifecycle behavior | Moderation authorization/tests | **Separate boundary**; review in `Moderation.md` |

## Findings

### Authorization lives inside the Graph aggregate

Graph's current mutation boundary is strong because authorization is not merely a Console convention. Every existing author-owned mutation calls `EnsureAuthoredBy` inside `Node` before changing state.

This includes scalar changes, collection/relationship changes, and archive/restore lifecycle changes. The tests also verify that a foreign actor is rejected before a no-op check, so an unauthorized caller does not gain a special path simply because the requested value happens to equal the current value.

### Missing and foreign actors are explicitly rejected

`EnsureAuthoredBy` distinguishes an absent actor from a foreign actor:

- `Guid.Empty` is invalid because an acting participant is required.
- A non-empty actor that differs from `AuthorId` receives an authorization failure.

PTT-120 security regression tests verify both cases and verify rejected operations do not mutate node state or add domain events.

### Creation is a host trust-boundary concern

A new `Node` accepts a `NodeAuthorId`; the aggregate cannot itself prove that the GUID represents the authenticated caller. That proof belongs at the authenticated application/host boundary.

The current Identity-backed Console establishes the actor before node workflows. A future REST API must derive the author from authenticated Identity and must not allow a request body to choose another Participant ID as the author.

This is a future endpoint-wiring requirement, not a reason to add HTTP concepts to Graph.

### Moderation is intentionally separate

Author archive/restore is Graph lifecycle behavior. Moderator visibility/exclusion is a different authority and belongs to the Moderation boundary. A GlobalModerator role should therefore not be added as a bypass to `Node.EnsureAuthoredBy` merely because moderators can moderate content.

Keeping these paths separate preserves a useful distinction between **editing somebody else's content** and **performing an auditable moderation action**.

## PTT-120 result

Existing Graph mutations have consistent author authorization and representative negative-path tests. No Graph-domain authorization fix was identified in this pass.

The important future-host requirement is that node creation and mutation actor IDs come from authenticated Identity rather than caller-controlled request data. Actual REST endpoint/policy tests remain deferred until the HTTP host exists.

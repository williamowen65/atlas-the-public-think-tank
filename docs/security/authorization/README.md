# Authorization Review

PTT-120 reviews authorization operation by operation before the REST API/frontend exist.

For every operation, answer only two security questions:

1. **How is the actor established?** Authentication/host responsibility. Never treat a caller-supplied ParticipantId as proof of identity.
2. **What enforces permission once the actor is known?** Application/domain authorization responsibility.

A disabled Console action is useful UI behavior, but is not by itself an authorization boundary.

## Review index

| Boundary | Review |
|---|---|
| Participants | [Participants.md](Participants.md) |
| Graph | [Graph.md](Graph.md) |
| Content | [Content.md](Content.md) |
| Voting | [Voting.md](Voting.md) |
| Communities | [Communities.md](Communities.md) |
| Comments | [Comments.md](Comments.md) |
| Discovery | [Discovery.md](Discovery.md) |
| Notifications | [Notifications.md](Notifications.md) |
| Moderation | [Moderation.md](Moderation.md) |
| Identity | [Identity.md](Identity.md) |

Each review is intentionally a compact operation table rather than a domain recap. Use the architecture/domain documentation for broader behavior.

## Result labels

- **Covered** — the current boundary enforces the rule, with evidence.
- **Host identity required** — the domain/application behavior relies on the host supplying the actor established by authentication.
- **Review / Policy needed** — the current code exposes a question that needs an explicit authorization decision or stronger workflow.
- **Deferred** — intentionally belongs to a future host/capability.

## Authenticated actor

Application/host workflows obtain the current actor through `IAuthenticatedActor` rather than treating a caller-supplied Participant ID as authentication evidence. The current Console implementation is `ConsoleIdentitySession`; a future ASP.NET host should provide a request-scoped implementation derived from the authenticated `ClaimsPrincipal`.

`IAuthenticatedActor` deliberately does **not** belong in the domain projects. Once the application has established the actor, domain operations continue to receive explicit actor IDs where authorization is part of their rule. This keeps authorization visible/testable without coupling Graph, Participants, Voting, Comments, Communities, Notifications, or Moderation to HTTP/session infrastructure.

This distinction also separates **actor** from **target**. A workflow may navigate or display another Participant, Node, notification, or other resource while the authenticated actor remains unchanged.

## Supporting boundaries

`Atlas.Console` is the current host/security harness. It establishes the current actor through the Identity-backed session and provides anonymous/authenticated UI behavior. Its disabled menu items are not substitutes for application/domain authorization.

`Atlas.Persistence` persists security-relevant state and constraints but is not treated as a user-operation authorization domain. `Atlas.Contracts` defines cross-boundary messages and likewise does not establish caller identity. Both may have security requirements under the other PTT-97 reviews.

## Future HTTP/API boundary

The future host must derive the actor from authenticated Identity, not request data. Endpoint tests should prove that unauthenticated callers are rejected where authentication is required, Member A cannot act as Member B by supplying B's ParticipantId, and privileged operations use the established global/contextual authorization source.

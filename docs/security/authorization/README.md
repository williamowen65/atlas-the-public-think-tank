# Authorization Review

PTT-120 reviews authorization boundaries across Atlas before the REST API and frontend exist.

ASP.NET Core Identity answers **who is the authenticated actor?** Atlas authorization answers **what may that actor read or change?** The current Console derives its actor through the Identity-backed session. A future REST API/web host must preserve that trust boundary rather than accepting a caller-supplied ParticipantId as proof of identity.

## Review method

Each MVP boundary is reviewed domain by domain. Important operations are recorded using this inventory:

| Boundary / operation | Data/action | Required access | Current enforcement | Test/evidence | Result / follow-up |
|---|---|---|---|---|---|
| Example: update Participant profile | Write own profile | Authenticated owner | Workflow compares actor and profile IDs | Ownership tests | Covered |
| Example: read public Node | Public read | Public/anonymous | Public query/domain path | Existing query evidence or future host test | Review |
| Example: moderation decision | Privileged write | GlobalModerator or Administrator | Moderation authorization backed by Identity roles | Moderation + Identity tests | Covered at application boundary; API wiring later |

Results should distinguish:

- **Covered** — existing implementation and tests already establish the rule.
- **Small PTT-120 fix** — a focused authorization gap that belongs in this review.
- **Follow-up** — a larger or domain-specific authorization change.
- **Future API enforcement** — the application rule is known, but endpoint/policy wiring requires the future HTTP host.

## Domain review order

The review starts with Participants so the distinction between public profile data, owner-controlled profile data, and Identity-owned private account/security data is explicit. The same method can then be applied to Graph, Content, Voting, Communities, Comments, Discovery, Notifications, Moderation, and other MVP boundaries.

Domain documents are added as their reviews are performed rather than created as empty placeholders.

## HTTP/API boundary

PTT-120 does not create speculative controllers, middleware, or page protection. When the REST API/web host exists, host-level authorization tests should prove that authenticated identity is used as the actor, caller-supplied participant identifiers cannot impersonate another user, and privileged endpoints enforce the appropriate global or contextual permissions.

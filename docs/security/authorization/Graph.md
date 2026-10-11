# Graph Authorization

Each row answers two security questions: **how is the actor established?** and **what enforces permission once the actor is known?**

| Operation | Access | How is the actor established? | What enforces permission? | Evidence | Result |
|---|---|---|---|---|---|
| [Read / browse node](../../../src/Atlas.Discovery/DiscoveryService.cs) | Public | No actor required | Public Discovery/Graph read path; anonymous Console has read-only node navigation | Anonymous Console + Discovery | Covered at current host |
| [Create node](../../../src/Atlas.Graph/Node.cs) | Authenticated Member | Host must derive Participant ID from authenticated Identity; author ID must not be trusted request input | Constructor requires an author ID, but Graph cannot authenticate that ID | Identity-backed Console creation path | Host boundary covered now; future API must preserve |
| [Rename node](../../../src/Atlas.Graph/Node.cs) | Author | Host supplies authenticated Participant ID | `Node.Rename` → `EnsureAuthoredBy` | Graph authorization tests | Covered |
| [Change node type](../../../src/Atlas.Graph/Node.cs) | Author | Host supplies authenticated Participant ID | `Node.ChangeType` → `EnsureAuthoredBy` | Graph authorization tests | Covered |
| [Request / stop requesting sub-node type](../../../src/Atlas.Graph/Node.cs) | Author | Host supplies authenticated Participant ID | Node mutation → `EnsureAuthoredBy` | Graph authorization tests | Covered |
| [Attach / detach parent](../../../src/Atlas.Graph/Node.cs) | Author | Host supplies authenticated Participant ID | Node relationship mutation → `EnsureAuthoredBy` | Graph authorization tests | Covered |
| [Archive node](../../../src/Atlas.Graph/Node.cs) | Author | Host supplies authenticated Participant ID | `Node.Archive` → `EnsureAuthoredBy` | Graph + PTT-120 security tests | Covered |
| [Restore node](../../../src/Atlas.Graph/Node.cs) | Author | Host supplies authenticated Participant ID | `Node.Restore` → `EnsureAuthoredBy` | Graph authorization tests | Covered |
| [Moderator hide / exclude](../../../src/Atlas.Moderation/ModerationService.cs) | GlobalModerator / Administrator | Host supplies authenticated actor; Identity-backed moderator authorization establishes global role | Moderation boundary, not Graph author-edit permission | Moderation + Identity tests | Separate Moderation operation |

## Review note

Graph does not authenticate Participant IDs and should not depend on ASP.NET Identity. The host establishes the actor. Graph then independently enforces author-owned mutations. Moderator actions remain separate from editing another author's node.

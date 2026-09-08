# Atlas domain glossary

This glossary defines shared product language. Code names may be more specific, but they should not contradict these meanings.

| Term | Meaning |
|---|---|
| Actor | The participant attempting an action. The actor may differ from the owner of the target resource. |
| Author | The participant credited with creating a node. Authorship is attribution and an input to authorization, not authorization by itself. |
| Boundary / bounded context | A model with explicit ownership, language, rules, and public interaction points. |
| Child / sub-node | A node that contains another node's ID in its parent collection. It remains a full node and may have multiple parents. |
| Content document | Content-owned descriptive material referenced by a node through a description identifier. It is plain text today and may become block-based later. |
| Contract | A versioned cross-boundary communication payload. It contains data, not domain behavior. |
| Domain event | A fact recorded by domain behavior about something that occurred. The current code keeps the name `DomainEvents` even when the public record is also used as an integration contract. |
| Integration event | A published, versioned fact that another boundary may consume or ignore. |
| Invariant | A rule that must always remain true for a domain object or boundary to be in a valid state, regardless of which screen, host, or workflow caused the change. For example, a node cannot be its own parent, and `UpdatedAt` cannot precede `CreatedAt`. A rule checked only by the Console is not yet a Graph invariant because another host could bypass it. |
| Node | The primary Graph entity representing a question, idea, issue, comment, or another typed contribution. |
| Node type | A globally reusable Graph definition identified by GUID. It controls classification and display behavior such as pluralization. |
| Parent | A node referenced by a child node's parent-ID collection. A node may have zero or multiple parents. |
| Participant | A person represented inside the Participants boundary by a public profile and stable ID. Authentication credentials are not currently part of this model. |
| Requested sub-node type | A type of response the node author invites. It does not prove that a child of that type exists and does not prevent other valid response types. |
| Relationship node | A node whose meaning connects multiple parents. Future rules may require at least two parents for particular relationship types. |
| Reconstitution | Rebuilding a domain object from persisted state without replaying creation behavior or generating new identities. |
| Subscriber | A handler registered to receive one event type. The publisher does not require every possible consumer to subscribe. |
| Type pluralization | Presentation behavior that uses a counted plural label when enabled while leaving mass nouns such as Evidence unchanged. |

## Vocabulary questions still open

- The preferred product-facing distinction between author-requested and responder-supplied sub-node types.
- Whether Comment remains a Graph node type or later gains additional Discussion-boundary behavior.
- The final name for relationship-specific node types and their minimum-parent rules.
- Whether user, participant, member, and account remain separate concepts once authentication is introduced.

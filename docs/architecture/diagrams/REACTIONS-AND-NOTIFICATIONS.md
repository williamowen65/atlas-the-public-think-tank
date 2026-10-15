# Reactions and notifications: decisions across domains

[Visual guide](README.md) · [Nodes and Content](NODES-AND-CONTENT.md) · Next: [Transactions and events](TRANSACTIONS-AND-EVENTS.md)

## A reaction definition is not its node association

```mermaid
classDiagram
    direction TB
    class ReactionVotingService {
        Vote()
        BuildChoices()
    }
    class ReactionDefinition {
        Id
        Text
    }
    class NodeReaction {
        Id
        NodeId
        ReactionDefinitionId
    }
    class CastVote {
        Execute(target, participant, value)
    }
    class NodeReactionVoteTarget
    ReactionVotingService ..> ReactionDefinition : resolves curated text
    ReactionVotingService ..> NodeReaction : finds or applies
    NodeReaction ..> ReactionDefinition : refers by ID
    ReactionVotingService ..> CastVote : submits vote
    CastVote ..> NodeReactionVoteTarget : targets association ID
```

Definitions describe the curated reaction catalog. An association applies a definition to one node. Votes target that association's ID. `BuildChoices` is read-only: it lists unsuppressed definitions with summaries, including definitions not yet applied to the node; those choices have zero totals and no participant vote.

## Vote process and rollback branch

```mermaid
flowchart TD
    A["Vote: node, reaction text, verified actor, value"] --> B["Enter shared SQL operation; load participant"]
    B --> C{"Participant found?"}
    C -->|No| X["Reject"]
    C -->|Yes| D{"Active association for this node and text?"}
    D -->|Yes| E["Reuse association"]
    D -->|No| F["Graph validates and applies reaction"]
    E --> G["Voting checks eligibility and value; saves current vote"]
    F --> G
    G --> H{"All work succeeds?"}
    H -->|Yes| I["Commit association and vote together"]
    H -->|No| J["Throw; roll back uncommitted SQL work, including new association"]
    F -->|Graph rejects| J
```

The shared boundary is an intentional improvement over independently saving the association before voting. A valid association can be reused; a newly created one must not survive a rejected vote. SQL adapters provide that rollback guarantee; arbitrary in-memory implementations need their own boundary support. See [ReactionVotingService](../../../src/Atlas.Application/ReactionVotingService.cs#L25), [Graph application service](../../../src/Atlas.Graph/Reactions/NodeReactionApplicationService.cs), [CastVote](../../../src/Atlas.Voting/CastVote.cs) and [SQL thin-host tests](../../../tests/Atlas.Persistence.Tests/SqlThinHostWorkflowTests.cs).

## Comment save and recipient selection

```mermaid
sequenceDiagram
    participant H as Host
    participant W as CommentWorkflowService
    participant C as CommentService
    participant E as Publisher
    participant N as NotificationService
    H->>W: AddTopLevel or Reply
    W->>C: Validate and save comment
    C-->>W: Saved comment
    W->>W: Load target node and optional parent
    alt Parent loaded
        W->>E: Request CommentReplied for parent author
    else No parent loaded
        W->>E: Request NodeCommented for node author
    end
    E->>N: Handle request synchronously
    N-->>E: Apply notification rules and persistence
    W-->>H: Return comment after handlers finish
```

Recipient selection is the application's job; Notifications owns its handling rules. The current implementation uses the node author if no parent was loaded, including its null fallback. Edit and Remove delegate to Comments without requesting these creation notifications. Comment save and notification handling are separate operations: a failure in lookup or publishing can be reported after the comment was saved.

Sources: [CommentWorkflowService](../../../src/Atlas.Application/Comments/CommentWorkflowService.cs#L22), [Notifications domain](../../../src/Atlas.Notifications), [notification workflow](../../workflows/NOTIFICATIONS.md), [SQL recipient tests](../../../tests/Atlas.Persistence.Tests/SqlThinHostWorkflowTests.cs).

## Moderation routes

```mermaid
flowchart TD
    A{"Moderation operation"} -->|Request review| B["Find latest node, document and block edit timestamp"]
    B --> C["Moderation validates review request"]
    A -->|Decide| D["Moderation saves decision and closes matching cases"]
    D --> E["Request ReportDecided for each reporter"]
    E --> F{"HideNode and node found?"}
    F -->|Yes| G["Request NodeHidden for node author"]
    F -->|No| H["Return decided cases"]
    G --> H
    A -->|Restore| I["Moderation saves restoration"]
    I --> J{"Node found?"}
    J -->|Yes| K["Request NodeRestored for node author"]
    J -->|No| L["Return without author notification"]
```

The decision tree shows orchestration, not every Moderation authorization branch. Domain rejection or notification-handler failure throws and stops the path. The saved decision/restoration precedes notification publication; requests to multiple recipients are not one all-or-nothing notification batch. See [ModerationWorkflowService](../../../src/Atlas.Application/ModerationWorkflowService.cs#L12), [Moderation architecture](../MODERATION.md) and [SQL thin-host tests](../../../tests/Atlas.Persistence.Tests/SqlThinHostWorkflowTests.cs).

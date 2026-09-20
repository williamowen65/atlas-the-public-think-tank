# Curated Node Reactions

Reactions are short expressions of how a participant responds to a Node in context. They are deliberately different from topics, categories, and the Node's 0–10 rating.

| Concept | Meaning |
|---|---|
| Node rating | A participant's 0–10 resonance with the Node itself |
| Reaction | A curated word and emoji applied to one Node |
| Reaction vote | +1 or −1 indicating whether that word resonates in the context of that Node |
| Author reaction | A reaction applied by the Node author |
| Community reaction | A reaction applied by another participant |

Examples include `🌱 Promising`, `❗ Important`, `🚨 Urgent`, `⚠️ Concerning`, and `🔎 Evidence-needed`. Topic phrases such as “Native plants,” “Pollinator habitat,” and “Safe routes” belong in Nodes or Communities rather than the reaction catalog.

## Add a reaction

```mermaid
flowchart TD
    A[Open Node reactions] --> B[Show curated catalog]
    B --> C[Choose word and emoji]
    C --> D{Already on Node?}
    D -->|Yes| E[Return existing association]
    D -->|No| F[Create NodeReaction]
    F --> G{Applied by author?}
    G -->|Yes| H[Show under Author reactions]
    G -->|No| I[Show under Community reactions]
```

Participants cannot create reaction wording through the Node workflow. Graph owns the catalog and the Node–Reaction association. Applying the same definition to different Nodes creates independent `NodeReactionId` targets.

## Vote on a reaction

```mermaid
sequenceDiagram
    participant P as Participant
    participant C as Console
    participant G as Graph
    participant V as Voting
    P->>C: Select reaction and +1 or -1
    C->>G: Confirm NodeReaction is active
    G-->>C: Available
    C->>V: Cast vote for NodeReactionId
    V-->>C: Contextual score
```

Voting owns one current signed vote per participant and `NodeReactionId`. A positive vote means the expression fits or resonates in context; a negative vote means it does not. It never changes the Node's separate 0–10 rating.

## Lifecycle and presentation

- Active participants may add a catalog reaction to an active Node.
- The same catalog entry can appear only once on a Node.
- The participant who applied a reaction may withdraw it; moderators retain administrative removal capability.
- Archived Nodes preserve reactions and votes for reading but reject mutation.
- Curated wording removes the normal need for typo replacement, author hiding, disputes, and global suppression in the participant-facing workflow.
- Compact and detail views show emoji, word, score, and whether the reaction came from the author or community.

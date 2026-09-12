# Voting boundary

This document defines the initial product and architectural decisions for the future Atlas Voting boundary. Voting is designed but not yet implemented.

## Purpose

Voting is the single authority for vote behavior throughout Atlas. A developer looking for vote rules, vote records, or vote aggregation should find them in this boundary rather than in Graph, Participants, a target-specific feature, or a user interface.

Voting is also a strong candidate for independent deployment. Atlas may receive substantially more vote traffic than content-creation traffic, so a Voting service should be capable of scaling without requiring unrelated boundaries to scale with it. Independent deployment is a future topology decision; the ownership boundary applies now.

## Boundary definition

| Attribute | Decision |
|---|---|
| **Responsibility** | Accept, change, remove, query, and aggregate votes under target-specific voting policies. |
| **Owns** | Vote IDs and records. • The participant-to-target uniqueness rule. • Vote value validation. • Vote timestamps. • Vote totals, counts, and averages. • Voting policy selection. |
| **References** | Participant IDs supplied by Participants. • Target IDs and target availability supplied by the target-owning boundary. |
| **Does not own** | Nodes, NodeTags, TagDefinitions, participant profiles, credentials, sessions, or target lifecycle. |
| **Possible deployment** | A separately deployable service that can scale independently when voting traffic is high. |
| **Current implementation** | None. The Console contains presentation placeholders only. |

A foreign target identifier tells Voting what received a vote; it does not transfer ownership of that target to Voting.

## Initial vote record

A persisted vote needs, at minimum:

- a stable vote identifier;
- the participant identifier;
- the target identifier and target type;
- the voting policy or value kind;
- the current value;
- creation and last-updated timestamps.

Voting records the same participant and timestamp information for NodeTag votes even if the first user interface does not expose the voter list for those votes.

## Universal invariants

- Only an authenticated Atlas account may vote. A participant may use a public persona that differs from their real-world identity, but an anonymous browser session cannot vote.
- Each participant has at most one current vote per target.
- A participant may change a current vote.
- A participant may undo a vote, which removes it from the current aggregate and public voter list.
- Voting validates values according to the target's voting policy.
- A target that is archived or otherwise unavailable for interaction rejects new and changed votes.
- Concurrent votes must not violate participant-to-target uniqueness or lose accepted updates.
- Current summaries should be refreshed in real time as accepted votes change.

## Node rating policy

A Node receives a general rating based on the human-readable context presented with that Node. Atlas initially relies on normal user interpretation rather than assigning a narrower universal meaning such as agreement, truth, importance, or quality. If ambiguity becomes harmful, Voting owns the future voting-policy change.

- Allowed values are whole numbers from 0 through 10.
- The summary contains the number of current votes and their arithmetic mean.
- The displayed average uses two decimal places.
- The public vote detail identifies which participant account cast each current value.

## NodeTag applicability policy

A NodeTag vote evaluates whether one TagDefinition applies to one particular Node. It does not vote globally on the TagDefinition.

- A participant may cast an upvote or downvote.
- Upvotes contribute +1 and downvotes contribute -1.
- The displayed result is the signed whole-number sum and may be negative, zero, or positive.
- A participant may change direction or undo the vote.
- Voting retains participant and timestamp information even if the initial NodeTag interface shows only the aggregate.

## Authorization and target availability

Authentication establishes the acting account. Participants owns the participant's identity and account standing; it does not own the vote. The target-owning boundary owns whether the target exists and whether its lifecycle permits interaction.

For the initial policy, every authenticated, eligible participant is entitled to one vote per target. Atlas does not initially impose contribution thresholds, reputation limits, or other voting quotas.

When Graph reports that a Node is archived, voting interaction on that Node and its directly dependent targets stops. The precise event, query, caching, and recovery mechanism remains an integration-design decision.

## Public transparency

Current Node votes are publicly auditable:

- viewers can inspect the participant account associated with each current vote;
- viewers can inspect the current value cast by that participant;
- participants can discover others who rated the same Node and may use public profile/contact features to collaborate.

This is public account attribution, not necessarily disclosure of a person's legal identity.

## Removal and history

Undoing a vote must remove it from all current aggregates and public vote listings.

Whether an undone vote is physically deleted or retained in a restricted operational history is unresolved. The user-facing behavior must be the same either way: it is no longer a current vote. Any later retention decision must account for transparency, privacy, moderation, abuse investigation, and data-retention policy.

## Cross-boundary collaboration

| Collaborator | Voting needs | Ownership retained by collaborator |
|---|---|---|
| **Graph** | Target identity, target kind, and whether a Node or NodeTag target permits interaction | Nodes, NodeTags, TagDefinitions, and lifecycle |
| **Participants / identity** | Authenticated participant ID and eligibility or active-account status | Profiles, credentials, sessions, and account lifecycle |
| **Console or future web host** | Commands and queries for voting plus summaries for composed views | Session/UI composition; no vote rules |
| **Contracts** | Versioned payloads if Voting communicates through events or service messages | Public communication shapes; no vote behavior |

## Console prototype slice

The Console should eventually demonstrate:

1. selecting the acting participant;
2. casting a 0–10 Node vote;
3. changing and undoing that vote;
4. displaying the current count and two-decimal average;
5. listing current Node voters and values;
6. casting, changing, and undoing a NodeTag upvote/downvote;
7. showing the signed NodeTag total;
8. rejecting voting against an archived or unavailable target.

The domain behavior belongs behind a Voting application API. Console commands should coordinate interaction and presentation without becoming the only enforcement point.

## Open decisions

- Whether an undone vote is physically deleted or retained outside the active/public record.
- Whether archived targets' existing vote details remain readable and whether votes can be undone while the target is archived.
- How Graph communicates target availability to an independently deployed Voting service.
- Whether summaries are calculated directly, maintained as projections, or cached by consumers.
- What consistency window qualifies as real time after service extraction.
- Whether NodeTag voter details will be visible in the first web interface.
- Whether future abuse controls add eligibility rules, quotas, rate limits, or moderation capabilities.
- Which versioned events and query contracts are required.

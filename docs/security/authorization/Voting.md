# Voting Authorization

| Operation | Access | How is the actor established? | What enforces permission? | Evidence | Result |
|---|---|---|---|---|---|
| View vote totals / averages | Public | No actor required | Read-only Voting summary | Voting tests | Covered |
| View current votes / voter attribution | Public under current product rules | No actor required | Voting read paths | Voting documentation/tests | Covered under current policy |
| Cast first vote | Authenticated eligible participant | Current host supplies authenticated Participant ID; future API must derive it from Identity | `CastVote` always calls `VoteMutationPolicy.EnsureAllowed`; eligibility port verifies participant and target availability | `VotingTests` | Covered after actor establishment |
| Change own current vote | Authenticated eligible participant | Same authenticated actor mapping | Vote lookup is by participant + target; mutation policy runs before change | Voting tests | Covered |
| Undo own vote | Authenticated eligible participant | Same authenticated actor mapping | `UndoVote` calls mutation policy and deletes only the vote for that participant + target | Voting tests | Covered |
| Vote as another participant | Not permitted | Host must not accept caller-selected Participant ID as actor | Voting can validate eligibility of the supplied ID, but cannot authenticate who supplied it | Current Identity-backed host + future API requirement | Host trust requirement |

## Review note

Voting is a good example of split responsibility. `VoteMutationPolicy` decides whether a **given participant** may vote, but it cannot prove that the caller really is that participant. Authentication must establish the actor before Voting receives the Participant ID.

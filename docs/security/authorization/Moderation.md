# Moderation Authorization

| Operation | Access | How is the actor established? | What enforces permission? | Evidence | Result |
|---|---|---|---|---|---|
| Report node | Authenticated participant under current host | Host supplies reporter Participant ID | Moderation prevents reporting an already-hidden node, but does not authenticate reporter ID | Current Console + Moderation service | Host identity required |
| Read public moderation visibility/placeholder | Public | No actor required | `Visibility(nodeId)` returns public hidden state/reason only | Moderation service/tests | Covered |
| Read moderation queue | GlobalModerator / Administrator | Host supplies authenticated Participant ID | `EnsureModerator` → `IModeratorAuthorization`; current Identity adapter checks persisted role + active/confirmed/unlocked account | PTT-120 security test + Identity adapter | Covered |
| Read grouped node queue | GlobalModerator / Administrator | Same | Same `EnsureModerator` | Moderation tests | Covered |
| Read node moderation history | GlobalModerator / Administrator | Same | Same `EnsureModerator` | PTT-120 security test | Covered |
| View hidden original | Node author or GlobalModerator/Admin | Host supplies authenticated actor | `CanViewHiddenOriginal` compares actor to author or calls moderator authorization | Moderation service/tests | Covered after actor establishment |
| Request review after editing hidden node | Node author | Host supplies authenticated actor | `RequestNodeReview` requires actor = author and verifies edit/review state | Moderation tests | Covered |
| Decide report(s) / hide node | GlobalModerator / Administrator | Host supplies authenticated actor | `EnsureModerator`; Identity-backed adapter checks persisted global role | PTT-120 + Identity tests | Covered |
| Restore hidden node after review | GlobalModerator / Administrator | Host supplies authenticated actor | `EnsureModerator` plus pending-review/rationale/time rules | Moderation tests | Covered |

## Review note

This is one of the stronger authorization boundaries: privileged operations call an authorization port inside Moderation rather than trusting a host-supplied boolean. The current Identity adapter backs that port with persisted global roles. The host still must supply the actual authenticated actor ID.

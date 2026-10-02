# Comments Authorization

| Operation | Access | How is the actor established? | What enforces permission? | Evidence | Result |
|---|---|---|---|---|---|
| Read comment thread | Public with target visibility | No actor required for current public targets | `GetThread` reads target comments | Comments tests | Covered under current public-read policy |
| Add top-level comment | Authenticated participant | Host must derive author ID from authenticated Identity | `CommentService` checks target availability; `Comment` requires non-empty author | Comments tests | Host identity required; domain does not authenticate author ID |
| Reply | Authenticated participant | Host must derive author ID from authenticated Identity | Service requires existing parent + available target; Comment records supplied author | Comments tests | Host identity required |
| Edit comment | Author | Host supplies authenticated Participant ID | `Comment.Edit` → `EnsureAuthor` | Comments tests | Covered |
| Remove own comment | Author | Host supplies authenticated Participant ID | `CommentService.Remove` selects author path; `RemoveByAuthor` checks author again | Comments tests | Covered |
| Remove another user's comment | Moderator | Host supplies authenticated actor | Service currently receives caller-provided `isModerator`; if true it invokes moderator removal | Comments tests | **Review:** moderator fact must come from authoritative authorization, not request input |

## Review note

Author-owned mutations have domain enforcement. Moderator removal is different: Comments currently accepts an `isModerator` fact from its caller. A future API must never populate that boolean from request data. PTT-120 should verify the host/application wiring derives it from Atlas moderator authorization.

# Comments Authorization

| Operation | Access | How is the actor established? | What enforces permission? | Evidence | Result |
|---|---|---|---|---|---|
| [Read comment thread](../../../src/Atlas.Comments/Comments/CommentService.cs) | Public with target visibility | No actor required for current public targets | `GetThread` reads target comments | Comments tests | Covered under current public-read policy |
| [Add top-level comment](../../../src/Atlas.Comments/Comments/CommentService.cs) | Authenticated participant | Host must derive author ID from authenticated Identity | `CommentService` checks target availability; `Comment` requires non-empty author | Comments tests | Host identity required; domain does not authenticate author ID |
| [Reply](../../../src/Atlas.Comments/Comments/CommentService.cs) | Authenticated participant | Host must derive author ID from authenticated Identity | Service requires existing parent + available target; Comment records supplied author | Comments tests | Host identity required |
| [Edit comment](../../../src/Atlas.Comments/Comments/Comment.cs) | Author | Host supplies authenticated Participant ID | `Comment.Edit` → `EnsureAuthor` | Comments tests | Covered |
| [Remove own comment](../../../src/Atlas.Comments/Comments/CommentService.cs) | Author | Host supplies authenticated Participant ID | `CommentService.Remove` selects author path; `RemoveByAuthor` checks author again | Comments tests | Covered |
| [Remove another user's comment](../../../src/Atlas.Comments/Comments/CommentService.cs) | Moderator | Host supplies authenticated actor | `CommentService` asks `ICommentModeratorAuthorization`; the Console adapter delegates to authoritative Moderation authorization | Comments tests | Covered — no caller-supplied moderator flag |

## Review note

Author-owned mutations remain enforced by Comments. Moderator removal now follows the same security shape: the caller supplies the actor ID, while Comments asks an authorization port whether that actor may moderate. The Console adapter obtains that answer from Atlas Moderation rather than accepting a caller-supplied boolean.

# Comments workflows

## Add top-level comment

1. UI/API supplies a `CommentTarget`, acting participant ID, text, and time.
2. `CommentService` asks the target-availability port whether the target accepts comment mutations.
3. A root `Comment` is created with `ParentCommentId = null`.
4. Comments persists it through `ICommentRepository`.

## Reply

1. Load the requested parent from Comments.
2. Reject a missing parent.
3. Check availability of the parent's target.
4. Create the reply with exactly one parent ID and the parent's target.
5. Persist the reply.

## Edit

The application service loads the comment and verifies target availability. The entity permits the change only for its author and only while active.

## Remove

The author may soft-remove their own comment. A moderator may soft-remove another participant's comment. The comment remains in the tree so descendants retain their path.

## Read thread

Comments loads records for one target and orders roots and descendants by creation time. Reading remains allowed even if the target later becomes unavailable.

## Deep thread reading

A deep thread is legitimate discussion structure. `CommentService.GetThread` uses an explicit stack to retain stable depth-first order without recursive stack growth. No reply-depth posting cap is introduced.

The orphan/cycle checks are **defensive guards for inconsistent stored data**, not conditions expected during correct operation of the current application. Reply creation requires an existing parent and creates a new comment beneath it. Author removal and moderator removal preserve the parent record as a placeholder, so neither should orphan replies or create cycles.

An orphan would reference a physically missing parent; a cycle would link comments back to one another. These could result from a faulty import, manual database changes or a future bug. If encountered, thread reading fails explicitly rather than silently omitting comments. These guards do not indicate a known defect in normal reply/removal behavior.

The method still materializes all matching comments; query paging, cancellation and work budgets remain PTT-138.

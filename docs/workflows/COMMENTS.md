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

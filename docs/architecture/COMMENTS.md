# Comments boundary

Comments is a dedicated Atlas bounded context for threaded discussion. A Comment is not a Graph Node.

## Ownership

Comments owns comment identity, body text, author reference, lifecycle state, parent relationship, and thread ordering. Other boundaries are referenced only by stable IDs through `CommentTarget`.

A top-level comment has no parent comment and belongs to exactly one target. A reply has exactly one `ParentCommentId` and inherits the same target as its parent. There is no collection of parent IDs, so Graph's multi-parent relationship model cannot leak into Comments.

## Lifecycle policy selected for PTT-99

Deletion is a soft removal. The record remains so replies do not lose their place in the thread. The lifecycle distinguishes author removal from moderator removal. Removed comments cannot be edited.

Replies to a removed ancestor remain visible and are allowed. This keeps an established discussion from becoming structurally inaccessible because an ancestor was removed. Presentation may replace a removed body's text with a tombstone later; the domain preserves the record and its body for now.

When the external target is unavailable or archived, existing threads remain readable but comment mutations are frozen. Comments asks target availability through the `ICommentTargetAvailability` port rather than depending on Graph.

Authors may edit their own active comments and remove their own comments. A moderator may remove another participant's comment. There is no edit-window restriction in this slice.

## Pattern recognition

**Aggregate/tree invariant:** each Comment stores one optional parent ID rather than Graph-style `ParentIds`. The shape of the model makes multi-parent comments unrepresentable.

**Ports and Adapters / Dependency Inversion:** `ICommentTargetAvailability` lets Comments enforce target lifecycle rules without referencing Graph. A Graph-backed adapter can be supplied by the host later.

**Repository:** `ICommentRepository` expresses the persistence operations Comments needs without choosing JSON or EF/SQL here.

**Application Service:** `CommentService` coordinates operations that need repository or external-target state while the Comment entity protects its own lifecycle and author invariants.

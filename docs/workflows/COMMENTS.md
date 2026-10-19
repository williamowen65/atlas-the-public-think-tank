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

## Read roots and expand replies

Interactive comment reads use `CommentService.GetPageAsync` /
`ICommentRepository.ReadPageAsync`. The Console's authenticated and anonymous browsers
load **20 direct siblings per page**, never the entire discussion. Select a comment
and choose **View direct replies** to load that branch. Next-page and back navigation
store cursor positions rather than loaded comment trees. Replying, editing and removal
retain the existing authorization and notification behavior. Removed comments remain
placeholders and can still be expanded or receive replies.

`CommentPageRequest` accepts 1–50 items. A null parent selects roots; a parent selects
only its direct children within the requested target. The service rejects missing
parents and parents from another target. Read availability is unchanged: hosts still
own hidden-node visibility checks, while the target-availability port governs mutations.

SQL applies the target/parent filter, orders by `(CreatedAt, Id)`, and retrieves only
`Size + 1` rows. The extra row determines whether a Next cursor is available. No
recursive descendants, total-count query or OFFSET is needed for page navigation.
Timestamp ties use SQL Server's uniqueidentifier order for both sorting and comparison.
The composite `IX_CommentRows_SiblingPage` supports this access path. Node-card comment
counts use SQL COUNT instead of loading bodies, though exact counts still require
index work proportional to the target's rows.

Cursors are exclusive positions, not a snapshot of a changing thread. Later insertions
can appear on later pages; backdated inserts before the cursor require refreshing an
earlier page. Hosts should reuse a cursor only with its original target/parent. A future
API can encode the structured cursor as an opaque value and validate its request fields.
Hosts must project removed comments as placeholders rather than serializing retained
domain Body values directly; both Console browsers preserve that behavior.

The read accepts a cancellation token and passes it to EF's async query. A future API
can pass `HttpContext.RequestAborted`, so work can stop when a request is cancelled.
The Console does not simulate browser navigation cancellation or launch background
loads. Cancellation is cooperative; it is not a guarantee of instant interruption.
Existing SQL command timeouts and the bounded query/index work together; no arbitrary
posting-frequency or reply-depth limit is introduced.

## Legacy full-thread traversal

`CommentService.GetThread` remains a full-materialization utility for existing traversal
tests and trusted diagnostic callers. It uses an explicit stack and preserves the
20,000-comment deep-thread regression. Interactive hosts no longer call it or
`GetByTarget`; future APIs must use the bounded page port.

Its orphan/cycle checks are defensive guards for inconsistent stored data, not conditions
expected during correct application behavior. Creation requires an existing parent;
soft removal retains placeholders. The paged read does not scan the full graph to
validate every unrelated descendant. Imports or repair tooling should separately
validate integrity rather than imposing unbounded work on each page.

## Tests and migration handoff

- [CommentPagingTests](../../tests/Atlas.Comments.Tests/CommentPagingTests.cs): input limits, parent scope, removed parents, cancellation, and no full-thread reads.
- [CommentPageQueryTests](../../tests/Atlas.Persistence.Tests/CommentPageQueryTests.cs): actual EF SQL translation, TOP/lookahead, no OFFSET/COUNT, and index shape without a database.
- [SqlCommentPagingTests](../../tests/Atlas.Persistence.Tests/SqlCommentPagingTests.cs): root/reply separation, placeholders, multiple pages, timestamp ties, inserts, target isolation and cancellation.
- [CommentPagingUiTests](../../tests/Atlas.Console.Tests/CommentPagingUiTests.cs): only viewed pages/branches are requested; navigation stays read-only for anonymous users.

The index change requires a developer-generated migration per
[src/Atlas.Persistence/AGENTS.md](../../src/Atlas.Persistence/AGENTS.md). The assistant
does not generate/edit migrations or the snapshot or update the developer database.
Review other pending model changes before generating; a migration includes all changes
since the last snapshot, not only this index. From the solution root:

```powershell
dotnet tool restore
dotnet ef migrations add AddCommentSiblingPagingIndex --project src/Atlas.Persistence
dotnet ef migrations has-pending-model-changes --project src/Atlas.Persistence
dotnet ef database update --project src/Atlas.Persistence
dotnet test tests/Atlas.Comments.Tests/Atlas.Comments.Tests.csproj
dotnet test tests/Atlas.Persistence.Tests/Atlas.Persistence.Tests.csproj
dotnet test tests/Atlas.Console.Tests/Atlas.Console.Tests.csproj
```

Review Up/Down for the composite index change and any intentionally included pending
ledger tables. Commit the migration, designer and snapshot before ordinary
migration-dependent SQL CI can pass. Fresh-model SQL tests verify queries/model shape,
not the upgrade path.

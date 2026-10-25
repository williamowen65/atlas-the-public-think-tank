# Editing an owned node

When signed in as a node's author, use the pencil **Edit** control in its card header or full node page. Shared card layouts carry this control into discovery, profile and community feeds and node search results. Other visitors do not see it. A shared session check supplies identity; the API still authorizes each write.

The editor reloads the current node and content before editing. Title, short description and each existing Markdown block have individual Save actions. Every successful Save is immediately public and updates the displayed node. A failed request retains editable values for retry. Close or Escape asks before discarding unsaved changes. Existing block IDs are retained; non-Markdown blocks remain untouched. Block addition, deletion and reordering are outside this pass.

**Save private revision draft** stores current editor values under the signed-in participant and node in this browser. Reopen Edit on the same node and select **Restore private revision draft** to resume, then review and save changes individually. Deleted or replaced blocks are not recreated by restoring a draft. **Delete private revision draft** removes the saved browser copy. Drafts are not synchronized across devices or listed among new-node drafts. A session change closes the editor and clears its in-memory values.

There is no public under-revision status or server-side revision history in this change. Existing content stays public while a private draft is prepared. Node and block saves use independent existing API operations, rather than one atomic document-wide save. Concurrent edits retain the backend's existing latest-save behavior. Full-text search indexing after content changes remains asynchronous.

No schema change or migration is needed. Restart the web/Storybook development server after pulling so its allowlisted session proxy accepts title, short-description and existing-block PUT requests. Backend owner checks, antiforgery protection and same-origin proxy checks remain in force.

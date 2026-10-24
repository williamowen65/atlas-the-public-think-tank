<!--
  @component
  LiveComments: Fetches and edits one level of a threaded discussion, recursively rendering replies.
  Each recursive instance owns its own cursor, retry state and composer; the API authorizes mutations.

  **Props**
  - `nodeId` (`string`; required): ID of the node this component reads, displays, or modifies.
  - `parentId` (`string`; optional): Comment ID whose direct replies this list loads; omit to load
    root comments for the node.
  - `apiBase` (`string`; optional; default `'/atlas-public-api'`): Public API base for root/reply
    comment pages, including each list’s cursor parameters. Profile lookups and mutations use
    sessionBase.
  - `sessionBase` (`string`; optional; default `'/atlas-session-api'`): API base for author-profile
    lookups and CSRF-protected comment/reply/edit/remove writes. Public comment pages use apiBase.
  - `participantId` (`string | null`; optional; default `null`): Current signed-in participant ID,
    supplied by the parent. Enables composing and author controls; the backend still authorizes
    writes.
  - `showComposer` (`boolean`; optional; default `true`): Allows the comment/reply editor to appear
    when a participant is signed in.
  - `depth` (`number`; optional; default `0`): Reply nesting depth used to limit increasing
    indentation.
  - `onAdded` (`() => void`; optional): Callback: Called after a comment or reply is successfully
    added; lets the parent refresh its displayed comment count.
-->
<script lang="ts">
  import { onMount, tick } from 'svelte';
  import LiveComments from './LiveComments.svelte';
  import { readJson, type Comment, type CommentPage } from '../api/nodePage';
  import { apiRequest } from '../api/apiRequest';
  let {
    nodeId,
    parentId,
    apiBase = '/atlas-public-api',
    sessionBase = '/atlas-session-api',
    participantId = null,
    showComposer = true,
    depth = 0,
    onAdded,
  }: {
    /** ID of the node this component reads, displays, or modifies. */
    nodeId: string;
    /** Comment ID whose direct replies this list loads; omit to load root comments for the node. */
    parentId?: string;
    /**
     * Public API base for root/reply comment pages, including each list’s cursor parameters.
     * Profile lookups and mutations use sessionBase.
     */
    apiBase?: string;
    /**
     * API base for author-profile lookups and CSRF-protected comment/reply/edit/remove writes.
     * Public comment pages use apiBase.
     */
    sessionBase?: string;
    /**
     * Current signed-in participant ID, supplied by the parent. Enables composing and author
     * controls; the backend still authorizes writes.
     */
    participantId?: string | null;
    /** Allows the comment/reply editor to appear when a participant is signed in. */
    showComposer?: boolean;
    /** Reply nesting depth used to limit increasing indentation. */
    depth?: number;
    /**
     * Callback: Called after a comment or reply is successfully added; lets the parent refresh its
     * displayed comment count.
     */
    onAdded?: () => void;
  } = $props();
  /**
   * Loaded comments for this thread level only; replies belong to recursive instances.
   */
  let items = $state<Comment[]>([]);
  /**
   * API keyset cursor for the next sibling page, or null when no continuation is
   * available.
   */
  let next = $state<CommentPage['next']>(null);
  /**
   * Whether a comment page has loaded successfully; distinguishes initial state from an empty
   * thread.
   */
  let started = $state(false);
  /**
   * Whether this level’s next comment page is loading.
   */
  let loading = $state(false);
  /**
   * Comment-list load failure; separate from posting/editing/removal failures.
   */
  let error = $state('');
  /**
   * Comment IDs whose reply threads are currently expanded.
   */
  let open = $state<string[]>([]);
  /**
   * Cached public author display names keyed by participant ID.
   */
  let names = $state<Record<string, string>>({});
  /**
   * New comment/reply composer text; retained when posting fails.
   */
  let text = $state('');
  /**
   * Whether posting, editing or removing a comment is underway.
   */
  let saving = $state(false);
  /**
   * Mutation failure displayed without replacing the loaded comment list.
   */
  let actionError = $state('');
  /**
   * Successful comment-action feedback announced to the visitor.
   */
  let notice = $state('');
  /**
   * ID of the comment whose edit form is open, or null when none is being edited.
   */
  let editing = $state<string | null>(null);
  /**
   * Working text for the open edit form; independent of the saved comment body.
   */
  let editText = $state('');
  /**
   * ID of the comment awaiting removal confirmation, or null.
   */
  let removing = $state<string | null>(null);
  /**
   * Comment ID whose expanded reply thread should show its composer.
   */
  let replyTarget = $state<string | null>(null);
  /**
   * Mounted new-comment textarea used for programmatic focus.
   */
  let composer = $state<HTMLTextAreaElement>();
  const instance = $props.id();
  const controller = new AbortController();
  const signal = () => AbortSignal.any([controller.signal, AbortSignal.timeout(10000)]);
  // Clear identity-sensitive drafts and edit state when the signed-in participant changes.
  $effect(() => {
    participantId;
    text = '';
    editing = null;
    removing = null;
    replyTarget = null;
    actionError = '';
  });
  // Each list has an independent keyset cursor; failures retain loaded items so retry can continue.
  async function load() {
    if (loading || (started && !next && !error)) return;
    loading = true;
    error = '';
    const query = new URLSearchParams({ size: '10' });
    if (parentId) query.set('parent', parentId);
    if (next) {
      query.set('afterAt', next.afterAt);
      query.set('afterId', next.afterId);
    }
    try {
      const page = await readJson<CommentPage>(
        apiBase,
        `nodes/${encodeURIComponent(nodeId)}/comments?${query}`,
        signal(),
      );
      if (!Array.isArray(page.items) || (page.next && (!page.next.afterAt || !page.next.afterId)))
        throw new Error('Invalid page');
      if (!controller.signal.aborted) {
        items = [
          ...items,
          ...page.items.filter((item) => !items.some((old) => old.id === item.id)),
        ].sort((a, b) => Date.parse(a.createdAt) - Date.parse(b.createdAt));
        next = page.next;
        started = true;
        void Promise.all(
          [...new Set(page.items.map((comment) => comment.authorId))]
            .filter((id) => !names[id])
            .map(async (id) => {
              try {
                const profile = await apiRequest<{ displayName: string }>(
                  sessionBase,
                  `participants/${encodeURIComponent(id)}`,
                  'GET',
                  undefined,
                  signal(),
                );
                if (!controller.signal.aborted) names[id] = profile.displayName;
              } catch {
                /* Keep a neutral fallback if the profile is unavailable. */
              }
            }),
        );
      }
    } catch {
      if (!controller.signal.aborted) error = 'Could not load comments.';
    } finally {
      loading = false;
    }
  }
  // Append only the comment returned by the API; preserve text if the write fails.
  async function post() {
    if (saving || !text.trim() || !participantId) return;
    saving = true;
    actionError = '';
    notice = '';
    try {
      const comment = await apiRequest<Comment>(
        sessionBase,
        parentId
          ? `comments/${encodeURIComponent(parentId)}/replies`
          : `nodes/${encodeURIComponent(nodeId)}/comments`,
        'POST',
        { body: text.trim() },
        signal(),
      );
      if (!controller.signal.aborted) {
        items = [...items.filter((item) => item.id !== comment.id), comment];
        text = '';
        notice = parentId ? 'Reply posted.' : 'Comment posted.';
        onAdded?.();
      }
    } catch (cause) {
      if (!controller.signal.aborted)
        actionError = cause instanceof Error ? cause.message : 'Could not post comment.';
    } finally {
      saving = false;
    }
  }
  async function saveEdit(id: string) {
    if (saving || !editText.trim()) return;
    saving = true;
    actionError = '';
    notice = '';
    try {
      await apiRequest(
        sessionBase,
        `comments/${encodeURIComponent(id)}`,
        'PUT',
        { body: editText.trim() },
        signal(),
      );
      if (!controller.signal.aborted) {
        items = items.map((item) =>
          item.id === id
            ? { ...item, body: editText.trim(), updatedAt: new Date().toISOString() }
            : item,
        );
        editing = null;
        notice = 'Comment updated.';
      }
    } catch (cause) {
      if (!controller.signal.aborted)
        actionError = cause instanceof Error ? cause.message : 'Could not update comment.';
    } finally {
      saving = false;
    }
  }
  // Replace the removed body locally while preserving its entry and any expanded reply thread.
  async function remove(id: string) {
    if (saving) return;
    saving = true;
    actionError = '';
    notice = '';
    try {
      await apiRequest(
        sessionBase,
        `comments/${encodeURIComponent(id)}`,
        'DELETE',
        undefined,
        signal(),
      );
      if (!controller.signal.aborted) {
        items = items.map((item) =>
          item.id === id ? { ...item, body: null, status: 'RemovedByAuthor' } : item,
        );
        removing = null;
        notice = 'Comment removed.';
      }
    } catch (cause) {
      if (!controller.signal.aborted)
        actionError = cause instanceof Error ? cause.message : 'Could not remove comment.';
    } finally {
      saving = false;
    }
  }
  // Wait for conditional reply markup to render before moving keyboard focus.
  async function focusComposer() {
    await tick();
    composer?.focus();
  }
  onMount(() => {
    void load();
    if (parentId && showComposer) void focusComposer();
    return () => controller.abort();
  });
  $effect(() => {
    if (parentId && showComposer && participantId) void focusComposer();
  });
</script>

<!--
  Layout: Each thread level has its own optional composer, comment list and paging status;
  expanded replies recursively render another thread level.
-->

<div
  class="comments"
  class:nested={!!parentId}
  class:deep={depth > 3}
  role="group"
  aria-label={parentId ? 'Replies' : 'Comments'}
  aria-busy={loading}
>
  {#if participantId && showComposer}<form
      class="composer"
      aria-label={parentId ? 'Write a reply' : 'Write a comment'}
      onsubmit={(event) => {
        event.preventDefault();
        void post();
      }}
    >
      <label for={`${instance}-body`}>{parentId ? 'Your reply' : 'Add a comment'}</label><textarea
        bind:this={composer}
        id={`${instance}-body`}
        bind:value={text}
        rows="3"
        required
        maxlength="10000"
        disabled={saving}></textarea>
      <div class="compose-actions">
        <small>{text.length.toLocaleString()}/10,000</small><button
          type="submit"
          disabled={saving || !text.trim()}
          >{saving ? 'Posting…' : parentId ? 'Post reply' : 'Post comment'}</button
        >
      </div>
    </form>{/if}
  {#if actionError}<p role="alert">{actionError}</p>{/if}{#if notice}<p role="status">
      {notice}
    </p>{/if}
  <!--
    Each item owns edit/removal controls; expanded replies below it form another independently
    paged level.
  -->
  <ol>
    {#each items as comment (comment.id)}<li>
        <article
          class="comment"
          data-comment-id={comment.id}
          aria-label={`Comment by ${names[comment.authorId] ?? 'Participant'}`}
        >
          <header>
            <span class="avatar" aria-hidden="true"
              >{(names[comment.authorId] ?? 'P').charAt(0).toUpperCase()}</span
            >
            <div>
              <strong
                >{names[comment.authorId] ?? 'Participant'}{#if participantId === comment.authorId}
                  <span class="you">(you)</span>{/if}</strong
              >
              <p class="meta">
                <time datetime={comment.createdAt}
                  >{new Date(comment.createdAt).toLocaleString()}</time
                >{#if comment.updatedAt && comment.updatedAt !== comment.createdAt && comment.body !== null}
                  · Edited{/if}
              </p>
            </div>
          </header>
          {#if editing === comment.id}<form
              aria-label="Edit your comment"
              onsubmit={(event) => {
                event.preventDefault();
                void saveEdit(comment.id);
              }}
            >
              <label for={`${instance}-edit`}>Edit comment</label><textarea
                id={`${instance}-edit`}
                bind:value={editText}
                maxlength="10000"
                rows="3"
                required
                disabled={saving}></textarea>
              <div class="controls">
                <button disabled={saving || !editText.trim()}>Save changes</button><button
                  type="button"
                  disabled={saving}
                  onclick={() => (editing = null)}>Cancel edit</button
                >
              </div>
            </form>
          {:else}<p class="body" class:removed={comment.body === null}>
              {comment.body ?? 'Comment removed.'}
            </p>{/if}
          <div class="controls">
            <button
              class="text-button"
              onclick={() =>
                (open = open.includes(comment.id)
                  ? open.filter((id) => id !== comment.id)
                  : [...open, comment.id])}
              aria-expanded={open.includes(comment.id)}
              >{open.includes(comment.id) ? 'Hide replies' : 'View replies'}</button
            >
            {#if participantId}<button
                class="text-button"
                onclick={() => {
                  replyTarget = comment.id;
                  if (!open.includes(comment.id)) open = [...open, comment.id];
                }}>Reply</button
              >{/if}
            {#if participantId === comment.authorId && comment.body !== null}<button
                class="text-button"
                disabled={saving}
                onclick={() => {
                  editing = comment.id;
                  editText = comment.body ?? '';
                }}>Edit</button
              ><button class="text-button" disabled={saving} onclick={() => (removing = comment.id)}
                >Remove</button
              >{/if}
          </div>
          {#if removing === comment.id}<div
              class="confirm"
              role="group"
              aria-label="Confirm comment removal"
            >
              <p>Remove this comment? Replies will remain in the discussion.</p>
              <button disabled={saving} onclick={() => void remove(comment.id)}
                >Remove comment</button
              ><button disabled={saving} onclick={() => (removing = null)}>Keep comment</button>
            </div>{/if}
          <!--
            Mount replies only while expanded; each recursive component owns its cursor and
            composer.
          -->
          {#if open.includes(comment.id)}<LiveComments
              {nodeId}
              parentId={comment.id}
              {apiBase}
              {sessionBase}
              {participantId}
              {onAdded}
              showComposer={replyTarget === comment.id}
              depth={depth + 1}
            />{/if}
        </article>
      </li>{/each}
  </ol>
  {#if error}<p role="alert">{error}</p>{/if}
  {#if loading}<p role="status">
      {parentId ? 'Loading replies…' : 'Loading comments…'}
    </p>{:else if started && !items.length}<p class="empty">
      {parentId ? 'No replies yet.' : 'No comments yet. Start the discussion.'}
    </p>{/if}
  {#if error || next}<button class="more" onclick={() => void load()} disabled={loading}
      >{error
        ? parentId
          ? 'Retry replies'
          : 'Retry comments'
        : parentId
          ? 'Load more replies'
          : 'Load more comments'}</button
    >{/if}
</div>

<style lang="scss">
  .comments {
    min-width: 0;
  }
  .nested {
    margin-top: 0.75rem;
    margin-left: 0.5rem;
    padding-left: 0.8rem;
    border-left: 2px solid var(--atlas-border);
  }
  .deep {
    margin-left: 0;
    padding-left: 0.35rem;
  }
  ol {
    list-style: none;
    margin: 0;
    padding: 0;
  }
  .comment {
    border-bottom: 1px solid var(--atlas-border);
    padding: 1.1rem 0;
  }
  header {
    display: flex;
    align-items: center;
    gap: 0.6rem;
  }
  .avatar {
    display: grid;
    place-items: center;
    flex: none;
    width: 2.2rem;
    height: 2.2rem;
    border-radius: 50%;
    background: var(--atlas-bg);
    color: var(--atlas-accent);
    font-weight: 600;
  }
  .meta {
    margin: 0.2rem 0 0;
    font-size: 0.8rem;
    color: var(--atlas-muted);
  }
  .you,
  small,
  .empty {
    color: var(--atlas-muted);
  }
  .body {
    white-space: pre-wrap;
    overflow-wrap: anywhere;
    line-height: var(--atlas-line-height, 1.65);
    margin: 0.65rem 0;
  }
  .removed {
    font-style: italic;
    color: var(--atlas-muted);
  }
  form {
    display: grid;
    gap: 0.5rem;
  }
  .composer {
    margin: 1rem 0;
  }
  textarea {
    box-sizing: border-box;
    width: 100%;
    min-width: 0;
    resize: vertical;
    font: inherit;
    padding: 0.65rem;
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    background: var(--atlas-surface);
    color: var(--atlas-text);
  }
  .compose-actions {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 0.5rem;
  }
  button {
    font: inherit;
    cursor: pointer;
    padding: 0.4rem 0.65rem;
    color: var(--atlas-accent);
    background: var(--atlas-surface);
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
  }
  button:disabled {
    cursor: default;
    opacity: 0.6;
  }
  .controls {
    display: flex;
    flex-wrap: wrap;
    gap: 0.35rem;
  }
  .text-button {
    border: 0;
    background: transparent;
    padding: 0.25rem 0.4rem;
    font-size: 0.85rem;
  }
  .text-button:hover {
    text-decoration: underline;
  }
  .confirm {
    margin: 0.5rem 0;
    padding: 0.75rem;
    background: var(--atlas-bg);
  }
  .more {
    margin-top: 1rem;
  }
  @media (max-width: 380px) {
    .nested {
      margin-left: 0;
      padding-left: 0.4rem;
    }
  }
</style>

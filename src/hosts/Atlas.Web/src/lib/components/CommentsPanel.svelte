<!--
  @component
  CommentsPanel: Composes session controls and the live threaded discussion for a node.
  Owns session identity for its discussion and forwards successful additions to the parent.

  **Props**
  - `nodeId` (`string`; required): ID of the node this component reads, displays, or modifies.
  - `apiBase` (`string`; optional; default `'/atlas-public-api'`): Public comment-read base
    forwarded to LiveComments. Authentication and mutations use sessionBase.
  - `sessionBase` (`string`; optional; default `'/atlas-session-api'`): Session-aware API base
    forwarded to SessionControl and LiveComments for identity checks and comment mutations.
  - `onAdded` (`() => void`; optional): Callback: Called after a comment or reply is successfully
    added; lets the parent refresh its displayed comment count.
-->
<script lang="ts">
  import LiveComments from './LiveComments.svelte';
  import SessionControl from './SessionControl.svelte';
  import { readJson, type Comment } from '../api/nodePage';
  let {
    nodeId,
    focusCommentId = '',
    apiBase = '/atlas-public-api',
    sessionBase = '/atlas-session-api',
    onAdded,
  }: {
    /** ID of the node this component reads, displays, or modifies. */
    nodeId: string;
    /** Comment selected in search; loads its bounded ancestor path. */
    focusCommentId?: string;
    /**
     * Public comment-read base forwarded to LiveComments. Authentication and mutations use
     * sessionBase.
     */
    apiBase?: string;
    /**
     * Session-aware API base forwarded to SessionControl and LiveComments for identity checks and
     * comment mutations.
     */
    sessionBase?: string;
    /**
     * Callback: Called after a comment or reply is successfully added; lets the parent refresh its
     * displayed comment count.
     */
    onAdded?: () => void;
  } = $props();
  /**
   * Current participant ID reported by the session API, or null when no usable identity is
   * known.
   */
  let participantId = $state<string | null>(null);
  let focusPath = $state<Comment[]>([]);
  let focusLoading = $state(false);
  let focusError = $state('');
  let focusRetry = $state(0);
  $effect(() => {
    const id = focusCommentId,
      node = nodeId;
    focusRetry;
    focusPath = [];
    focusError = '';
    if (!id) {
      focusLoading = false;
      return;
    }
    focusLoading = true;
    const controller = new AbortController();
    void readJson<{ items: Comment[] }>(
      apiBase,
      `nodes/${encodeURIComponent(node)}/comments/${encodeURIComponent(id)}/path`,
      AbortSignal.any([controller.signal, AbortSignal.timeout(10000)]),
    )
      .then((path) => {
        if (controller.signal.aborted) return;
        if (
          !Array.isArray(path.items) ||
          !path.items.length ||
          path.items.length > 128 ||
          path.items.at(-1)?.id !== id
        )
          throw new Error('Invalid comment path');
        focusPath = path.items;
      })
      .catch(() => {
        if (!controller.signal.aborted)
          focusError =
            'This comment could not be opened. It may have been removed or become unavailable.';
      })
      .finally(() => {
        if (!controller.signal.aborted) focusLoading = false;
      });
    return () => controller.abort();
  });
</script>

<!--
  Layout: Discussion heading, session prompt, then the live comment thread. Session identity
  connects the prompt to posting controls.
-->

<section class="discussion" aria-label="Comment discussion">
  <header>
    <h2>Comments</h2>
    <p>Join the discussion or build on someone’s contribution.</p>
  </header>
  <SessionControl
    {sessionBase}
    hideWhenSignedIn
    prompt="Sign in to comment"
    onSession={(id) => (participantId = id)}
  />
  {#if focusLoading}<p role="status">Opening comment thread…</p>
  {:else if focusError}<p role="alert">{focusError}</p>
    <button type="button" onclick={() => focusRetry++}>Retry comment</button><button
      type="button"
      onclick={() => {
        focusError = '';
        focusPath = [];
      }}>Show discussion</button
    >
  {:else}{#key nodeId + ':' + focusCommentId}<LiveComments
        {nodeId}
        {apiBase}
        {sessionBase}
        {participantId}
        {onAdded}
        {focusPath}
      />{/key}{/if}
</section>

<style>
  h2 {
    margin: 0;
  }
  header p {
    color: var(--atlas-muted);
    margin: 0.4rem 0 1rem;
  }
</style>

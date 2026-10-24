<!--
  CommentsPanel: Composes session controls and the live threaded discussion for a node.
  Owns session identity for its discussion and forwards successful additions to the parent.
-->
<script lang="ts">
  import LiveComments from './LiveComments.svelte';
  import SessionControl from './SessionControl.svelte';
  let {
    nodeId,
    apiBase = '/atlas-public-api',
    sessionBase = '/atlas-session-api',
    onAdded,
  }: {
    /** ID of the node this component reads, displays, or modifies. */
    nodeId: string;
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
  let participantId = $state<string | null>(null);
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
  <LiveComments {nodeId} {apiBase} {sessionBase} {participantId} {onAdded} />
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

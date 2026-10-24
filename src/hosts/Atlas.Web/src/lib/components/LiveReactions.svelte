<!--
  @component
  LiveReactions: Loads reaction choices/scores and handles session-aware reaction voting and undo.
  Uses API-returned scores and participant votes instead of optimistically calculating totals.
-->
<script lang="ts">
  import { onMount } from 'svelte';
  import { apiRequest, ApiRequestError } from '../api/apiRequest';
  import SessionControl from './SessionControl.svelte';

  interface Choice {
    id: string;
    associationId: string | null;
    text: string;
    emoji: string;
    score: number;
    totalVotes: number;
    currentParticipantVote: number | null;
  }
  let {
    nodeId,
    archived = false,
    apiBase = '/atlas-session-api',
  }: {
    /** ID of the node this component reads, displays, or modifies. */
    nodeId: string;
    /** Marks the node as archived and disables applicable voting/editing controls. */
    archived?: boolean;
    /**
     * Session-aware endpoint base for both reaction reads and writes, despite the generic apiBase
     * prop name.
     */
    apiBase?: string;
  } = $props();
  /**
   * API reaction choices with scores, counts and the current participant’s vote.
   */
  let choices = $state<Choice[]>([]);
  /**
   * Whether data is being loaded; drives loading feedback for this view.
   */
  let loading = $state(true);
  /**
   * Reaction-choice ID currently being changed, or null when no mutation is running.
   */
  let busy = $state<string | null>(null);
  /**
   * Whether the full reaction picker is visible.
   */
  let expanded = $state(false);
  /**
   * Whether an unauthorized vote has opened the session prompt.
   */
  let signIn = $state(false);
  /**
   * User-facing load or request failure; empty text hides the error message.
   */
  let error = $state('');
  let controller: AbortController;
  let requestGeneration = 0;
  /**
   * Choices with votes or a current participant selection; these appear in the compact
   * row.
   */
  const visibleReactions = $derived(
    choices.filter((c) => c.totalVotes > 0 || c.currentParticipantVote !== null),
  );
  // Only the latest request may publish scores, preventing slower earlier reads from replacing new data.
  async function refresh() {
    const generation = ++requestGeneration;
    const result = await apiRequest<Choice[]>(
      apiBase,
      `nodes/${encodeURIComponent(nodeId)}/reactions`,
      'GET',
      undefined,
      controller.signal,
    );
    if (!Array.isArray(result)) throw new Error('Invalid reaction response');
    if (!controller.signal.aborted && generation === requestGeneration) {
      choices = result;
      error = '';
    }
  }
  // Load choices in the browser and refresh them after a session change; release listeners on removal.
  onMount(() => {
    controller = new AbortController();
    void refresh()
      .catch(() => {
        if (!controller.signal.aborted) error = 'Could not load reactions.';
      })
      .finally(() => {
        if (!controller.signal.aborted) loading = false;
      });
    const changed = () => {
      if (!controller.signal.aborted) void refresh().catch(() => {});
    };
    window.addEventListener('atlas-session-changed', changed);
    return () => {
      controller.abort();
      window.removeEventListener('atlas-session-changed', changed);
    };
  });
  // Selecting an existing +1 undoes it; otherwise request +1, then reload server totals.
  async function toggle(choice: Choice) {
    if (archived || busy) return;
    busy = choice.id;
    error = '';
    try {
      if (choice.currentParticipantVote === 1 && choice.associationId) {
        await apiRequest(
          apiBase,
          `reactions/${encodeURIComponent(choice.associationId)}/vote`,
          'DELETE',
          undefined,
          controller.signal,
        );
      } else {
        await apiRequest(
          apiBase,
          `nodes/${encodeURIComponent(nodeId)}/reactions/vote`,
          'PUT',
          { text: choice.text, value: 1 },
          controller.signal,
        );
      }
      await refresh();
    } catch (cause) {
      if (cause instanceof ApiRequestError && cause.status === 401) signIn = true;
      else error = cause instanceof Error ? cause.message : 'Reaction could not be saved.';
      // Do not optimistically change totals on a failed request.
    } finally {
      busy = null;
    }
  }
</script>

<!--
  Layout: Current reactions form a compact row; an optional picker exposes all choices, with
  request feedback and a separate sign-in prompt below.
-->

<section class="node-reactions" aria-label="Community reactions">
  <div class="reaction-list">
    <!-- class:chosen adds the chosen CSS class only while this participant has a +1 vote. -->
    {#each visibleReactions as choice (choice.id)}
      <button
        type="button"
        class:chosen={choice.currentParticipantVote === 1}
        aria-pressed={choice.currentParticipantVote === 1}
        aria-label={`${choice.text}, score ${choice.score}. ${choice.currentParticipantVote === 1 ? 'Undo your vote' : 'Add your vote'}`}
        title={choice.currentParticipantVote === 1 ? 'Click to undo your vote' : 'Click to add +1'}
        disabled={archived || busy !== null || loading}
        onclick={() => void toggle(choice)}
      >
        <span aria-hidden="true">{choice.emoji}</span>
        {choice.text} <strong>{choice.score}</strong>
      </button>
    {/each}
    {#if !archived}
      <button
        type="button"
        class="add-reaction"
        aria-expanded={expanded}
        onclick={() => (expanded = !expanded)}
      >
        {expanded ? 'Close reactions' : '+ Add reaction'}
      </button>
    {/if}
  </div>
  {#if expanded}
    <div class="reaction-picker" aria-label="Available reactions">
      <p>Choose a reaction to add +1. Select it again to undo your vote.</p>
      {#each choices as choice (choice.id)}
        <button
          type="button"
          class:chosen={choice.currentParticipantVote === 1}
          aria-pressed={choice.currentParticipantVote === 1}
          disabled={archived || busy !== null || loading}
          onclick={() => void toggle(choice)}
        >
          {choice.emoji}
          {choice.text} <strong>{choice.score}</strong>
        </button>
      {/each}
    </div>
  {/if}
  {#if loading}<p role="status">Loading reactions…</p>{/if}
  {#if error}<p role="alert">
      {error}
      <button
        type="button"
        onclick={() => {
          error = '';
          void refresh().catch(() => (error = 'Could not load reactions.'));
        }}>Retry</button
      >
    </p>{/if}
  {#if signIn}
    <div class="sign-in-prompt">
      <p>Sign in to add or vote on reactions.</p>
      <SessionControl
        expanded
        hideWhenSignedIn
        onSession={(id) => {
          if (id) {
            signIn = false;
            void refresh();
          }
        }}
      />
      <button type="button" onclick={() => (signIn = false)}>Dismiss</button>
    </div>
  {/if}
</section>

<style>
  .node-reactions {
    padding: 0.5rem var(--atlas-card-padding, 1.25rem) 0;
  }
  .reaction-list,
  .reaction-picker {
    display: flex;
    flex-wrap: wrap;
    gap: 0.45rem;
    align-items: center;
  }
  button {
    font: inherit;
    font-size: 0.85rem;
    cursor: pointer;
    color: var(--atlas-text);
    background: var(--atlas-surface);
    border: 1px solid var(--atlas-border);
    border-radius: 999px;
    padding: 0.3rem 0.7rem;
  }
  button.chosen {
    background: var(--atlas-accent);
    color: var(--atlas-on-accent);
    border-color: var(--atlas-accent);
    font-weight: 600;
  }
  button:disabled {
    opacity: 0.55;
    cursor: default;
  }
  button:focus-visible {
    outline: 2px solid var(--atlas-accent);
    outline-offset: 2px;
  }
  .reaction-picker {
    margin-top: 0.75rem;
    padding: 0.75rem;
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
  }
  .reaction-picker p {
    flex-basis: 100%;
    margin: 0 0 0.3rem;
    color: var(--atlas-muted);
    font-size: 0.85rem;
  }
  .sign-in-prompt {
    margin-top: 0.75rem;
    padding: 0.75rem;
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
  }
</style>

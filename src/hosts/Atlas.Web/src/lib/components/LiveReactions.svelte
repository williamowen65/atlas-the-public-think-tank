<script lang="ts">
  import { onMount } from 'svelte';
  import { commentRequest, CommentRequestError } from '../api/comments';
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
  let { nodeId, archived = false, apiBase = '/atlas-session-api' }: {
    nodeId: string; archived?: boolean; apiBase?: string
  } = $props();
  let choices = $state<Choice[]>([]);
  let loading = $state(true);
  let busy = $state<string | null>(null);
  let expanded = $state(false);
  let signIn = $state(false);
  let error = $state('');
  let controller: AbortController;
  let requestGeneration = 0;
  const active = $derived(choices.filter(c => c.totalVotes > 0 || c.currentParticipantVote !== null));
  async function refresh() {
    const generation = ++requestGeneration;
    const result = await commentRequest<Choice[]>(apiBase, `nodes/${encodeURIComponent(nodeId)}/reactions`, 'GET', undefined, controller.signal);
    if (!Array.isArray(result)) throw new Error('Invalid reaction response');
    if (!controller.signal.aborted && generation === requestGeneration) { choices = result; error = ''; }
  }
  onMount(() => {
    controller = new AbortController();
    void refresh().catch(() => { if (!controller.signal.aborted) error = 'Could not load reactions.'; })
      .finally(() => { if (!controller.signal.aborted) loading = false; });
    const changed = () => { if (!controller.signal.aborted) void refresh().catch(() => {}); };
    window.addEventListener('atlas-session-changed', changed);
    return () => { controller.abort(); window.removeEventListener('atlas-session-changed', changed); };
  });
  async function toggle(choice: Choice) {
    if (archived || busy) return;
    busy = choice.id; error = '';
    try {
      if (choice.currentParticipantVote === 1 && choice.associationId) {
        await commentRequest(apiBase, `reactions/${encodeURIComponent(choice.associationId)}/vote`, 'DELETE', undefined, controller.signal);
      } else {
        await commentRequest(apiBase, `nodes/${encodeURIComponent(nodeId)}/reactions/vote`, 'PUT', { text: choice.text, value: 1 }, controller.signal);
      }
      await refresh();
    } catch (cause) {
      if (cause instanceof CommentRequestError && cause.status === 401) signIn = true;
      else error = cause instanceof Error ? cause.message : 'Reaction could not be saved.';
      // Do not optimistically change totals on a failed request.
    } finally { busy = null; }
  }
</script>

<section class="node-reactions" aria-label="Community reactions">
  <div class="reaction-list">
    {#each active as choice (choice.id)}
      <button type="button" class:chosen={choice.currentParticipantVote === 1}
        aria-pressed={choice.currentParticipantVote === 1}
        aria-label={`${choice.text}, score ${choice.score}. ${choice.currentParticipantVote === 1 ? 'Undo your vote' : 'Add your vote'}`}
        title={choice.currentParticipantVote === 1 ? 'Click to undo your vote' : 'Click to add +1'}
        disabled={archived || busy !== null || loading}
        onclick={() => void toggle(choice)}>
        <span aria-hidden="true">{choice.emoji}</span> {choice.text} <strong>{choice.score}</strong>
      </button>
    {/each}
    {#if !archived}
      <button type="button" class="add-reaction" aria-expanded={expanded} onclick={() => expanded = !expanded}>
        {expanded ? 'Close reactions' : '+ Add reaction'}
      </button>
    {/if}
  </div>
  {#if expanded}
    <div class="reaction-picker" aria-label="Available reactions">
      <p>Choose a reaction to add +1. Select it again to undo your vote.</p>
      {#each choices as choice (choice.id)}
        <button type="button" class:chosen={choice.currentParticipantVote === 1}
          aria-pressed={choice.currentParticipantVote === 1}
          disabled={archived || busy !== null || loading}
          onclick={() => void toggle(choice)}>
          {choice.emoji} {choice.text} <strong>{choice.score}</strong>
        </button>
      {/each}
    </div>
  {/if}
  {#if loading}<p role="status">Loading reactions…</p>{/if}
  {#if error}<p role="alert">{error} <button type="button" onclick={() => { error = ''; void refresh().catch(() => error = 'Could not load reactions.'); }}>Retry</button></p>{/if}
  {#if signIn}
    <div class="sign-in-prompt">
      <p>Sign in to add or vote on reactions.</p>
      <SessionControl expanded hideWhenSignedIn onSession={id => { if (id) { signIn = false; void refresh(); } }} />
      <button type="button" onclick={() => signIn = false}>Dismiss</button>
    </div>
  {/if}
</section>
<style>
  .node-reactions{padding:.5rem var(--atlas-card-padding,1.25rem) 0;}
  .reaction-list,.reaction-picker{display:flex;flex-wrap:wrap;gap:.45rem;align-items:center;}
  button{font:inherit;font-size:.85rem;cursor:pointer;color:var(--atlas-text);background:var(--atlas-surface);border:1px solid var(--atlas-border);border-radius:999px;padding:.3rem .7rem;}
  button.chosen{background:var(--atlas-accent);color:var(--atlas-on-accent);border-color:var(--atlas-accent);font-weight:600;}
  button:disabled{opacity:.55;cursor:default;}
  button:focus-visible{outline:2px solid var(--atlas-accent);outline-offset:2px;}
  .reaction-picker{margin-top:.75rem;padding:.75rem;border:1px solid var(--atlas-border);border-radius:var(--atlas-radius);}
  .reaction-picker p{flex-basis:100%;margin:0 0 .3rem;color:var(--atlas-muted);font-size:.85rem;}
  .sign-in-prompt{margin-top:.75rem;padding:.75rem;border:1px solid var(--atlas-border);border-radius:var(--atlas-radius);}
</style>

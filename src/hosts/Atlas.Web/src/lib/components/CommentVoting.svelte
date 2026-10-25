<script lang="ts">
  import { apiRequest } from '../api/apiRequest';
  let {
    commentId,
    participantId = null,
    sessionBase = '/atlas-session-api',
  }: {
    commentId: string;
    participantId?: string | null;
    sessionBase?: string;
  } = $props();
  type Summary = { score: number; totalVotes: number; currentParticipantVote: number | null };
  let summary = $state<Summary | null>(null);
  let busy = $state(false);
  let error = $state('');
  let signIn = $state(false);
  let retry = $state(0);
  let current: AbortController;
  $effect(() => {
    const id = commentId,
      base = sessionBase;
    participantId;
    retry;
    const controller = new AbortController();
    current = controller;
    summary = null;
    error = '';
    signIn = false;
    busy = true;
    void apiRequest<Summary>(
      base,
      `comments/${encodeURIComponent(id)}/votes`,
      'GET',
      undefined,
      AbortSignal.any([controller.signal, AbortSignal.timeout(10000)]),
    )
      .then((value) => {
        if (!controller.signal.aborted) summary = value;
      })
      .catch(() => {
        if (!controller.signal.aborted) error = 'Could not load comment votes.';
      })
      .finally(() => {
        if (!controller.signal.aborted) busy = false;
      });
    return () => controller.abort();
  });
  async function vote(value: number) {
    if (!participantId) {
      signIn = true;
      return;
    }
    if (busy || !summary) return;
    const controller = current;
    const base = sessionBase,
      id = commentId;
    const signal = AbortSignal.any([controller.signal, AbortSignal.timeout(10000)]);
    busy = true;
    error = '';
    let saved = false;
    try {
      await apiRequest(
        base,
        `comments/${encodeURIComponent(id)}/vote`,
        summary.currentParticipantVote === value ? 'DELETE' : 'PUT',
        summary.currentParticipantVote === value ? undefined : { value },
        signal,
      );
      saved = true;
      const fresh = await apiRequest<Summary>(
        base,
        `comments/${encodeURIComponent(id)}/votes`,
        'GET',
        undefined,
        signal,
      );
      if (!controller.signal.aborted) summary = fresh;
    } catch {
      if (!controller.signal.aborted) {
        error = saved
          ? 'Vote saved. Could not refresh totals.'
          : 'Could not confirm vote. Refresh totals before trying again.';
        summary = null;
      }
    } finally {
      if (!controller.signal.aborted) busy = false;
    }
  }
</script>

<div class="comment-voting" role="group" aria-label="Comment voting" aria-busy={busy}>
  <button
    type="button"
    aria-label="Upvote comment"
    aria-pressed={summary?.currentParticipantVote === 1}
    disabled={busy || (!!participantId && !summary)}
    onclick={() => void vote(1)}>↑</button
  >
  <span aria-label="Comment score">{summary?.score ?? '—'}</span>
  <button
    type="button"
    aria-label="Downvote comment"
    aria-pressed={summary?.currentParticipantVote === -1}
    disabled={busy || (!!participantId && !summary)}
    onclick={() => void vote(-1)}>↓</button
  >
  {#if error}<span role="alert">{error}</span><button
      type="button"
      disabled={busy}
      onclick={() => retry++}>Refresh votes</button
    >{/if}
  {#if signIn}<span role="status"><a href="/login">Sign in</a> to vote on comments.</span><button
      type="button"
      aria-label="Dismiss sign-in prompt"
      onclick={() => (signIn = false)}>×</button
    >{/if}
</div>

<style lang="scss">
  .comment-voting {
    display: inline-flex;
    align-items: center;
    gap: 0.35rem;
    flex-wrap: wrap;
    font-size: 0.85rem;
  }
  button {
    font: inherit;
    color: var(--atlas-accent);
    background: transparent;
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    padding: 0.2rem 0.5rem;
    cursor: pointer;
  }
  button[aria-pressed='true'] {
    background: var(--atlas-accent);
    color: var(--atlas-surface);
  }
  button:disabled {
    opacity: 0.6;
    cursor: default;
  }
</style>

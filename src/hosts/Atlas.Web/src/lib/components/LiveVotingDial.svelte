<script lang="ts">
  import { onMount, tick } from 'svelte';
  import VotingDial from './VotingDial.svelte';
  import SessionControl from './SessionControl.svelte';
  import { commentRequest, CommentRequestError } from '../api/comments';
  interface VoteSummary { voteCount: number; averageVote: number | null; currentParticipantVote: number | null }
  let { nodeId, initialAverage = null, initialTotalVotes = 0, archived = false, sessionBase = '/atlas-comments-api' }: { nodeId: string; initialAverage?: number | null; initialTotalVotes?: number; archived?: boolean; sessionBase?: string } = $props();
  let confirmed = $state<number | null>(null); let value = $state<number | null>(null);
  let average = $state<number | null>(null); let totalVotes = $state(0);
  let loading = $state(true); let busy = $state(false); let notice = $state(''); let error = $state(''); let refreshFailed = $state(false); let login = $state(false);
  let container = $state<HTMLDivElement>();
  let dialog = $state<HTMLDialogElement>();
  let confirmDialog = $state<HTMLDialogElement>();
  let pending = $state<number | null>(null);
  let pendingUndo = $state(false);
  let timer: ReturnType<typeof setTimeout> | undefined;
  const controller = new AbortController();
  const signal = () => AbortSignal.any([controller.signal, AbortSignal.timeout(10000)]);
  async function refresh() {
    const summary = await commentRequest<VoteSummary>(sessionBase, `nodes/${encodeURIComponent(nodeId)}/votes`, 'GET', undefined, signal());
    if (!Number.isInteger(summary.voteCount) || summary.voteCount < 0 || (summary.currentParticipantVote !== null && (!Number.isInteger(summary.currentParticipantVote) || summary.currentParticipantVote < 0 || summary.currentParticipantVote > 10))) throw new Error('Invalid vote summary');
    if (controller.signal.aborted) return;
    average = summary.averageVote; totalVotes = summary.voteCount; confirmed = summary.currentParticipantVote; value = confirmed; refreshFailed = false;
  }
  async function retry() {
    if (busy) return; busy = true; error = '';
    try { await refresh(); } catch { if (!controller.signal.aborted) { error = 'Could not refresh voting totals.'; refreshFailed = true; } } finally { busy = false; }
  }
  onMount(() => {
    average = initialAverage; totalVotes = initialTotalVotes;
    void refresh().catch(() => { if (!controller.signal.aborted) { error = 'Could not load your voting status.'; refreshFailed = true; } }).finally(() => loading = false);
    const sessionChanged = () => { clearTimeout(timer); cancelPending(); void retry(); };
    window.addEventListener('atlas-session-changed', sessionChanged);
    return () => { controller.abort(); clearTimeout(timer); window.removeEventListener('atlas-session-changed', sessionChanged); };
  });
  $effect(() => { if (login && dialog && !dialog.open) dialog.showModal(); });
  function cancelPending() {
    clearTimeout(timer);
    pending = null;
    pendingUndo = false;
    value = confirmed;
    confirmDialog?.close();
  }
  function choose(next: number) {
    if (busy || archived || loading || pending !== null) return;
    clearTimeout(timer); value = next; notice = ''; error = '';
    if (next === confirmed) return;
    // Wait for wheel/drag momentum to settle, then request explicit confirmation.
    timer = setTimeout(() => {
      if (controller.signal.aborted || busy) return;
      pending = next;
      void tick().then(() => { if (pending === next && confirmDialog && !confirmDialog.open) confirmDialog.showModal(); });
    }, 450);
  }
  function requestUndo() {
    if (busy || loading || archived || confirmed === null) return;
    clearTimeout(timer);
    pending = null;
    pendingUndo = true;
    void tick().then(() => { if (pendingUndo && confirmDialog && !confirmDialog.open) confirmDialog.showModal(); });
  }
  function approve() {
    if (!pendingUndo && pending === null) return;
    const next = pendingUndo ? null : pending;
    pending = null;
    pendingUndo = false;
    confirmDialog?.close();
    void save(next);
  }
  async function save(next: number | null) {
    clearTimeout(timer); if (busy || archived) return;
    busy = true; error = ''; notice = ''; const previous = confirmed;
    try {
      await commentRequest(sessionBase, `nodes/${encodeURIComponent(nodeId)}/vote`, next === null ? 'DELETE' : 'PUT', next === null ? undefined : { value: next }, signal());
      if (controller.signal.aborted) return;
      confirmed = next; value = next;
      notice = next === null ? 'Vote removed.' : `Rated ${next}/10.`;
      try { await refresh(); }
      catch { if (!controller.signal.aborted) { error = 'Your vote was saved, but totals could not refresh.'; refreshFailed = true; } }
    } catch (cause) {
      if (!controller.signal.aborted) {
        value = previous; confirmed = previous;
        if (cause instanceof CommentRequestError && cause.status === 401) { confirmed = null; value = null; login = true; }
        else error = cause instanceof Error ? cause.message : 'Vote could not be saved. Please retry.';
      }
    } finally { busy = false; }
    if (next === null && confirmed === null && !controller.signal.aborted) { await tick(); container?.querySelector<HTMLElement>('[role=spinbutton]')?.focus(); }
  }
  function close() { login = false; dialog?.close(); }
</script>
<div bind:this={container} class="live-voting" aria-busy={busy}>
  <VotingDial label="Node importance" {value} {average} {totalVotes} hasVoted={confirmed !== null} disabled={archived || busy || loading} onChange={choose} />
  {#if confirmed !== null && !archived}<button class="undo" type="button" disabled={busy || loading} onclick={requestUndo} aria-label="Undo your node importance vote">Undo vote</button>{/if}
  {#if busy}<small role="status">Saving…</small>{/if}
  {#if notice}<small role="status">{notice}</small>{/if}
  {#if error}<small role="alert">{error}</small>{/if}
  {#if refreshFailed}<button class="undo" onclick={() => void retry()} disabled={busy}>Retry totals</button>{/if}
</div>
<dialog bind:this={confirmDialog} class="vote-login" aria-labelledby="vote-confirm-title" oncancel={cancelPending} onclose={() => { if (pending !== null || pendingUndo) cancelPending(); }}>
  {#if pendingUndo}
    <h2 id="vote-confirm-title">Undo your importance rating?</h2>
    <p>Your current rating of <strong>{confirmed} out of 10</strong> will be removed from the community average and vote count.</p>
  {:else}
    <h2 id="vote-confirm-title">Confirm your importance rating</h2>
    <p>Does this contribution deserve attention? Rate how important it is to explore or consider, from <strong>0 (not important)</strong> to <strong>10 (very important)</strong>.</p>
    <p>Your proposed rating is <strong>{pending ?? '—'} out of 10</strong>. Your rating contributes to the community average.</p>
  {/if}
  <div class="vote-actions"><button type="button" onclick={cancelPending}>Cancel</button><button type="button" class="confirm" onclick={approve} disabled={!pendingUndo && pending === null}>{pendingUndo ? 'Confirm undo' : 'Confirm rating'}</button></div>
</dialog>
<dialog bind:this={dialog} class="vote-login" aria-label="Sign in to vote" oncancel={() => login = false} onclose={() => login = false}>
  {#if login}<h2>Sign in to vote</h2><p>Your rating wasn’t saved. Sign in, then choose your rating again.</p>
    <SessionControl {sessionBase} expanded prompt="Sign in to Atlas" onSession={id => { if (id) { close(); void retry(); } }} />
    <button class="dismiss" onclick={close}>Close</button>
  {/if}
</dialog>
<style lang="scss">
  .live-voting{width:min-content;min-width:4.3rem;text-align:center;}small{display:block;font-size:.75rem;line-height:1.3;overflow-wrap:anywhere;margin:.3rem 0;}.undo{display:block;margin:.25rem auto 0;padding:.15rem .25rem;border:0;background:transparent;color:var(--atlas-muted);font:inherit;font-size:.75rem;cursor:pointer;white-space:nowrap;}.undo:hover{text-decoration:underline;color:var(--atlas-accent);}.undo:disabled{cursor:default;opacity:.6;}.vote-login{box-sizing:border-box;width:min(28rem,calc(100vw - 2rem));max-height:calc(100dvh - 2rem);overflow:auto;padding:1.25rem;border:1px solid var(--atlas-border);border-radius:var(--atlas-radius);color:var(--atlas-text);background:var(--atlas-surface);text-align:left;}.vote-login::backdrop{background:#0007;}.vote-login h2{margin-top:0;}.vote-actions{display:flex;justify-content:flex-end;flex-wrap:wrap;gap:.75rem;margin-top:1rem;}.vote-actions button{font:inherit;padding:.5rem .75rem;cursor:pointer;}.vote-actions .confirm{background:var(--atlas-accent);color:var(--atlas-on-accent);border:0;border-radius:.35rem;}.dismiss{font:inherit;margin-top:1rem;padding:.4rem .75rem;cursor:pointer;}
</style>

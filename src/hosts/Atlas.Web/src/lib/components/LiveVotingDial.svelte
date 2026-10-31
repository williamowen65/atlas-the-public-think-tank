<!--
  @component
  LiveVotingDial: Connects VotingDial to API summaries, explicit confirmation, undo and sign-in prompts.
  Wraps the same VotingDial UI; it is not a separate dial implementation.
  Loads the participant rating and server totals, confirms selections, saves/undoes votes,
  and prompts for sign-in after an unauthorized write. It never computes community averages.
  Disables the underlying dial while archived, loading status or saving a vote.

  **Props**
  - `mobileCompact` (`boolean`; optional; default `false`): At phone widths, show Vote/Voted beside Read more and expand the dial into the description.
  - `nodeId` (`string`; required): ID of the node this component reads, displays, or modifies.
  - `initialAverage` (`number | null`; optional; default `null`): Initial average shown while the
    live voting summary is loading; the API summary is authoritative.
  - `initialTotalVotes` (`number`; optional; default `0`): Initial count shown while the live voting
    summary is loading; refreshed from the API.
  - `archived` (`boolean`; optional; default `false`): Marks the node as archived and disables
    applicable voting/editing controls.
  - `sessionBase` (`string`; optional; default `'/atlas-session-api'`): Base URL for vote-summary
    reads, CSRF-protected vote/undo writes, and the embedded sign-in control. The API returns the
    authoritative count and average.
-->
<script lang="ts">
  import { onMount, tick } from 'svelte';
  import { formatVoteCount } from '../format/formatVoteCount';
  import VotingDial from './VotingDial.svelte';
  import SessionControl from './SessionControl.svelte';
  import { apiRequest, ApiRequestError } from '../api/apiRequest';
  /** Whether the mobile summary has been expanded by the visitor. */
  let expanded = $state(false);
  /** Match the feed breakpoint without changing desktop dial behavior. */
  let narrow = $state(false);
  onMount(() => {
    const media = matchMedia('(max-width: 560px)');
    /** Keep the responsive mode current when rotating or resizing the viewport. */
    const resize = () => { narrow = media.matches; };
    resize();
    media.addEventListener('change', resize);
    return () => media.removeEventListener('change', resize);
  });
  interface VoteSummary {
    voteCount: number;
    averageVote: number | null;
    currentParticipantVote: number | null;
  }
  let {
    nodeId,
    initialAverage = null,
    initialTotalVotes = 0,
    archived = false,
    mobileCompact = false,
    sessionBase = '/atlas-session-api',
  }: {
    /** ID of the node this component reads, displays, or modifies. */
    nodeId: string;
    /**
     * Initial average shown while the live voting summary is loading; the API summary is
     * authoritative.
     */
    initialAverage?: number | null;
    /** Initial count shown while the live voting summary is loading; refreshed from the API. */
    initialTotalVotes?: number;
    /** Marks the node as archived and disables applicable voting/editing controls. */
    archived?: boolean;
    /** Collapse the mobile feed dial behind a rating summary; full node pages keep the dial. */
    mobileCompact?: boolean;
    /**
     * Base URL for vote-summary reads, CSRF-protected vote/undo writes, and the embedded sign-in
     * control. The API returns the authoritative count and average.
     */
    sessionBase?: string;
  } = $props();
  const collapsedMode = $derived(mobileCompact && narrow);
  /**
   * Last server-confirmed participant rating; null means no confirmed vote.
   */
  let confirmed = $state<number | null>(null);
  /**
   * Rating displayed by the dial, including a proposed selection awaiting confirmation.
   */
  let value = $state<number | null>(null);
  /**
   * Backend-calculated average importance rating; null means no average is available.
   */
  let average = $state<number | null>(null);
  /**
   * Backend-reported importance vote count; the frontend does not calculate it.
   */
  let totalVotes = $state(0);
  /**
   * Whether data is being loaded; drives loading feedback for this view.
   */
  let loading = $state(true);
  /**
   * Whether a mutation is underway; prevents overlapping actions.
   */
  let busy = $state(false);
  /**
   * Non-error feedback shown after an action completes.
   */
  let notice = $state('');
  /**
   * User-facing load or request failure; empty text hides the error message.
   */
  let error = $state('');
  /**
   * Whether a write succeeded but refreshing authoritative totals failed; exposes Retry
   * totals.
   */
  let refreshFailed = $state(false);
  /**
   * Whether the sign-in dialog’s content is mounted.
   */
  let login = $state(false);
  /**
   * Mounted inline dial wrapper used for interaction/focus handling.
   */
  let container = $state<HTMLDivElement>();
  /**
   * Mounted native sign-in dialog controlled by showModal/close.
   */
  let dialog = $state<HTMLDialogElement>();
  /**
   * Mounted native confirmation dialog for casting or undoing a vote.
   */
  let confirmDialog = $state<HTMLDialogElement>();
  /**
   * Proposed rating awaiting explicit approval; null means no rating is pending.
   */
  let pending = $state<number | null>(null);
  /**
   * Whether the confirmation dialog represents an undo rather than a new rating.
   */
  let pendingUndo = $state(false);
  const controller = new AbortController();
  const signal = () => AbortSignal.any([controller.signal, AbortSignal.timeout(10000)]);
  // Validate and adopt the API summary together: current rating, count and average are one confirmed snapshot.
  async function refresh() {
    const summary = await apiRequest<VoteSummary>(
      sessionBase,
      `nodes/${encodeURIComponent(nodeId)}/votes`,
      'GET',
      undefined,
      signal(),
    );
    if (
      !Number.isInteger(summary.voteCount) ||
      summary.voteCount < 0 ||
      (summary.currentParticipantVote !== null &&
        (!Number.isInteger(summary.currentParticipantVote) ||
          summary.currentParticipantVote < 0 ||
          summary.currentParticipantVote > 10))
    )
      throw new Error('Invalid vote summary');
    if (controller.signal.aborted) return;
    average = summary.averageVote;
    totalVotes = summary.voteCount;
    confirmed = summary.currentParticipantVote;
    value = confirmed;
    refreshFailed = false;
  }
  async function retry() {
    if (busy) return;
    busy = true;
    error = '';
    try {
      await refresh();
    } catch {
      if (!controller.signal.aborted) {
        error = 'Could not refresh voting totals.';
        refreshFailed = true;
      }
    } finally {
      busy = false;
    }
  }
  onMount(() => {
    average = initialAverage;
    totalVotes = initialTotalVotes;
    void refresh()
      .catch(() => {
        if (!controller.signal.aborted) {
          error = 'Could not load your voting status.';
          refreshFailed = true;
        }
      })
      .finally(() => (loading = false));
    const sessionChanged = () => {
      cancelPending();
      void retry();
    };
    window.addEventListener('atlas-session-changed', sessionChanged);
    return () => {
      controller.abort();
      window.removeEventListener('atlas-session-changed', sessionChanged);
    };
  });
  $effect(() => {
    if (login && dialog && !dialog.open) dialog.showModal();
  });
  // Cancel confirmation without saving and restore the last confirmed selection.
  function cancelPending() {
    pending = null;
    pendingUndo = false;
    value = confirmed;
    confirmDialog?.close();
  }
  function choose(next: number) {
    if (busy || archived || loading || pending !== null) return;
    value = next;
    notice = '';
    error = '';
  }
  /**
   * Request explicit review after the dial reports a completed gesture or Enter.
   * Preview updates alone never open a modal or send an API write.
   * @param next Final proposed whole-number rating, including zero.
   */
  function requestRating(next: number) {
    if (busy || archived || loading || pending !== null || next === confirmed) return;
    value = next;
    pending = next;
    void tick().then(() => {
      if (!controller.signal.aborted && pending === next && confirmDialog && !confirmDialog.open)
        confirmDialog.showModal();
    });
  }
  function requestUndo() {
    if (busy || loading || archived || confirmed === null) return;
    pending = null;
    pendingUndo = true;
    void tick().then(() => {
      if (pendingUndo && confirmDialog && !confirmDialog.open) confirmDialog.showModal();
    });
  }
  // Capture the proposed rating (or undo), close confirmation, then perform the API write.
  function approve() {
    if (!pendingUndo && pending === null) return;
    const next = pendingUndo ? null : pending;
    pending = null;
    pendingUndo = false;
    confirmDialog?.close();
    void save(next);
  }
  // A failed write restores the confirmed state; a failed totals refresh after a successful write is reported separately.
  async function save(next: number | null) {
    if (busy || archived) return;
    busy = true;
    error = '';
    notice = '';
    const previous = confirmed;
    try {
      await apiRequest(
        sessionBase,
        `nodes/${encodeURIComponent(nodeId)}/vote`,
        next === null ? 'DELETE' : 'PUT',
        next === null ? undefined : { value: next },
        signal(),
      );
      if (controller.signal.aborted) return;
      confirmed = next;
      value = next;
      // Collapse only after the API accepts the write; failed writes keep the
      // editor available. The summary uses confirmed state, including zero.
      if (collapsedMode) {
        expanded = false;
        await tick();
        container?.querySelector<HTMLButtonElement>('.rating-summary')?.focus({ preventScroll: true });
      }
      notice = next === null ? 'Vote removed.' : `Rated ${next}/10.`;
      try {
        await refresh();
      } catch {
        if (!controller.signal.aborted) {
          error = 'Your vote was saved, but totals could not refresh.';
          refreshFailed = true;
        }
      }
    } catch (cause) {
      if (!controller.signal.aborted) {
        value = previous;
        confirmed = previous;
        if (cause instanceof ApiRequestError && cause.status === 401) {
          confirmed = null;
          value = null;
          login = true;
        } else
          error = cause instanceof Error ? cause.message : 'Vote could not be saved. Please retry.';
      }
    } finally {
      busy = false;
    }
    if (!collapsedMode && next === null && confirmed === null && !controller.signal.aborted) {
      await tick();
      container?.querySelector<HTMLElement>('[role=spinbutton]')?.focus();
    }
  }
  function close() {
    login = false;
    dialog?.close();
  }
</script>

<!--
  Layout: The dial, undo and request feedback stay inline. Separate native dialogs handle vote
  confirmation and sign-in.
-->

<div bind:this={container} class="live-voting" class:collapsed-mode={collapsedMode} aria-busy={busy}>
  {#if collapsedMode}
    <button class="rating-summary" type="button" aria-expanded={expanded}
      disabled={archived || busy || loading}
      onclick={() => { expanded = !expanded; if (!expanded) cancelPending(); }}
      aria-label={`${confirmed === null ? 'Vote' : `Voted ${confirmed} out of 10. Change your vote`}. Rate node importance. Average ${average === null ? 'not rated' : average.toFixed(1)}. ${totalVotes} votes.`}>
      <strong class="summary-average">{average === null ? '—' : average.toFixed(1)}</strong>
      <span class="summary-voters" role="img" aria-label={`${totalVotes} ${totalVotes === 1 ? 'vote' : 'votes'}`} title={totalVotes.toLocaleString('en-US')}>
        <svg viewBox="0 0 56 44" aria-hidden="true"><circle cx="28" cy="7" r="6"/><circle cx="10" cy="12" r="5"/><circle cx="46" cy="12" r="5"/><path d="M1 41V26a9 9 0 0 1 18 0v15ZM37 41V26a9 9 0 0 1 18 0v15ZM12 44V30a16 16 0 0 1 32 0v14Z"/></svg>
        <span class="summary-count" aria-hidden="true">{formatVoteCount(totalVotes)}</span>
      </span>
      <span>{confirmed === null ? 'Vote' : `Voted ${confirmed}/10`}</span>
    </button>
  {/if}
  {#if !collapsedMode || expanded}
  <div class="dial-content">
  <!-- Live interaction lock: archived nodes, pending status reads and in-flight writes. -->
  <VotingDial
    label="Node importance"
    {value}
    {average}
    {totalVotes}
    hasVoted={confirmed !== null}
    disabled={archived || busy || loading}
    onChange={choose}
    onCommit={requestRating}
    onCancel={cancelPending}
  />
  {#if confirmed !== null && !archived}<button
      class="undo"
      type="button"
      disabled={busy || loading}
      onclick={requestUndo}
      aria-label="Undo your node importance vote">Undo vote</button
    >{/if}
  </div>
  {/if}
  {#if busy}<small role="status">Saving…</small>{/if}
  {#if notice}<small role="status">{notice}</small>{/if}
  {#if error}<small role="alert">{error}</small>{/if}
  {#if refreshFailed}<button class="undo" onclick={() => void retry()} disabled={busy}
      >Retry totals</button
    >{/if}
</div>
<!--
  Confirmation is a separate native dialog so its modal behavior is independent of the inline
  dial.
-->
<dialog
  bind:this={confirmDialog}
  class="vote-login"
  aria-labelledby="vote-confirm-title"
  oncancel={cancelPending}
  onclose={() => {
    if (pending !== null || pendingUndo) cancelPending();
  }}
>
  {#if pendingUndo}
    <h2 id="vote-confirm-title">Undo your importance rating?</h2>
    <p>
      Your current rating of <strong>{confirmed} out of 10</strong> will be removed from the community
      average and vote count.
    </p>
  {:else}
    <h2 id="vote-confirm-title">Confirm your importance rating</h2>
    <p>
      Does this contribution deserve attention? Rate how important it is to explore or consider,
      from <strong>0 (not important)</strong> to <strong>10 (very important)</strong>.
    </p>
    <p>
      Your proposed rating is <strong>{pending ?? '—'} out of 10</strong>. Your rating contributes
      to the community average.
    </p>
  {/if}
  <div class="vote-actions">
    <button type="button" onclick={cancelPending}>Cancel</button><button
      type="button"
      class="confirm"
      onclick={approve}
      disabled={!pendingUndo && pending === null}
      >{pendingUndo ? 'Confirm undo' : 'Confirm rating'}</button
    >
  </div>
</dialog>
<!--
  Sign-in is separate from confirmation; signing in asks the visitor to select their rating
  again.
-->
<dialog
  bind:this={dialog}
  class="vote-login"
  aria-label="Sign in to vote"
  oncancel={() => (login = false)}
  onclose={() => (login = false)}
>
  {#if login}<h2>Sign in to vote</h2>
    <p>Your rating wasn’t saved. Sign in, then choose your rating again.</p>
    <SessionControl
      {sessionBase}
      expanded
      prompt="Sign in to Atlas"
      onSession={(id) => {
        if (id) {
          close();
          void retry();
        }
      }}
    />
    <button class="dismiss" onclick={close}>Close</button>
  {/if}
</dialog>

<style lang="scss">
  .rating-summary {
    display: flex;
    align-items: center;
    gap: 0.35rem;
    min-height: 2.75rem;
    padding: 0.25rem 0.5rem 0.25rem 0.25rem;
    border: 1px solid var(--atlas-border);
    border-radius: 0.65rem;
    background: var(--atlas-surface);
    color: var(--atlas-accent);
    font: inherit;
    font-size: 0.8rem;
    cursor: pointer;
  }
  .summary-average {
    padding: 0.3rem 0.5rem;
    border-radius: 0.4rem;
    background: color-mix(in srgb, var(--atlas-accent) 12%, var(--atlas-surface));
    font-variant-numeric: tabular-nums;
  }
  // Match the full dial: the count sits inside the central person's body.
  // Keep the icon wide enough for abbreviated totals such as 4.5B.
  .summary-voters {
    position: relative;
    display: grid;
    place-items: center;
    width: 2.4rem;
    height: 1.9rem;
    flex: 0 0 2.4rem;
  }
  .summary-voters svg {
    position: absolute;
    inset: 0;
    width: 100%;
    height: 100%;
    fill: var(--atlas-accent);
  }
  .summary-count {
    position: relative;
    margin-top: 0.7rem;
    padding-inline: 0.1rem;
    border-radius: 0.1rem;
    background: var(--atlas-accent);
    color: var(--atlas-on-accent);
    font-size: 0.65rem;
    font-weight: 700;
    font-variant-numeric: tabular-nums;
    line-height: 1.15;
  }
  .dial-content { display: contents; }
  // The card body owns the action row. The expanded dial rejoins normal text flow
  // as a left float, so introductions can wrap beside and then below it.
  .live-voting.collapsed-mode { display: contents; }
  .collapsed-mode .rating-summary {
    position: absolute;
    inset-inline-start: var(--card-padding);
    bottom: 0.65rem;
  }
  .collapsed-mode .dial-content {
    display: block;
    float: left;
    width: min-content;
    margin: 0 var(--dial-text-gap) 0.6rem -0.4rem;
    text-align: center;
  }
  // Status is announced without becoming a clearing block between the floated
  // dial and description. The saved rating remains visible in the Vote button.
  .collapsed-mode small[role='status'] {
    position: absolute;
    width: 1px;
    height: 1px;
    overflow: hidden;
    clip-path: inset(50%);
    white-space: nowrap;
  }
  .collapsed-mode small[role='alert'] { clear: both; }

  .live-voting {
    width: min-content;
    min-width: 4.3rem;
    text-align: center;
  }
  small {
    display: block;
    font-size: 0.75rem;
    line-height: 1.3;
    overflow-wrap: anywhere;
    margin: 0.3rem 0;
  }
  .undo {
    display: block;
    margin: 0.25rem auto 0;
    padding: 0.15rem 0.25rem;
    border: 0;
    background: transparent;
    color: var(--atlas-muted);
    font: inherit;
    font-size: 0.75rem;
    cursor: pointer;
    white-space: nowrap;
  }
  .undo:hover {
    text-decoration: underline;
    color: var(--atlas-accent);
  }
  .undo:disabled {
    cursor: default;
    opacity: 0.6;
  }
  .vote-login {
    box-sizing: border-box;
    width: min(28rem, calc(100vw - 2rem));
    max-height: calc(100dvh - 2rem);
    overflow: auto;
    padding: 1.25rem;
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    color: var(--atlas-text);
    background: var(--atlas-surface);
    text-align: left;
  }
  .vote-login::backdrop {
    background: #0007;
  }
  .vote-login h2 {
    margin-top: 0;
  }
  .vote-actions {
    display: flex;
    justify-content: flex-end;
    flex-wrap: wrap;
    gap: 0.75rem;
    margin-top: 1rem;
  }
  .vote-actions button {
    font: inherit;
    padding: 0.5rem 0.75rem;
    cursor: pointer;
  }
  .vote-actions .confirm {
    background: var(--atlas-accent);
    color: var(--atlas-on-accent);
    border: 0;
    border-radius: 0.35rem;
  }
  .dismiss {
    font: inherit;
    margin-top: 1rem;
    padding: 0.4rem 0.75rem;
    cursor: pointer;
  }
</style>

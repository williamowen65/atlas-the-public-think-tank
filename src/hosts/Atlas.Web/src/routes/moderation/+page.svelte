<!-- Global moderators review reports; backend policies authorize every operation. -->
<script lang="ts">
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  import { apiRequest, ApiRequestError } from '$lib/api/apiRequest';

  type QueueItem = { nodeId: string; reportedTitle: string; count: number; reviewRequested: boolean };
  type Case = { id: string; reason: string; explanation: string | null; createdAt: string; status: string; reporter: string; reviewer: string | null; decisionReason: string | null; decidedAt: string | null; publicReason: string; reviewRequestedAt: string | null; visibilityRestoredAt: string | null; restoredBy: string | null; restorationReason: string | null };
  const base = '/atlas-session-api';
  const publicReasons = ['Other', 'Spam', 'Harassment', 'UnsafeContent', 'OffTopic'];
  let queue = $state<QueueItem[]>([]);
  let queuePage = $state(1);
  let queueTotal = $state(0);
  let queueHasMore = $state(false);
  let selected = $state<string | null>(null);
  let history = $state<Case[]>([]);
  let loading = $state(true);
  let busy = $state(false);
  let confirmation = $state<'dismiss' | 'hide' | 'restore' | null>(null);
  // Authorization is determined by the server, not by a client-side role claim.
  // This can later permit community moderators with scoped queue permissions.
  let access = $state<'checking' | 'allowed' | 'denied'>('checking');
  let error = $state('');
  let notice = $state('');
  let rationale = $state('');
  let publicReason = $state('Other');
  let search = $state('');
  let reviewOnly = $state(false);
  let sort = $state<'reports' | 'title'>('reports');

  async function refresh(page = queuePage) {
    loading = true;
    error = '';
    try {
      // Distinguish an anonymous visitor from a signed-in user lacking moderation rights.
      const session = await apiRequest<{ participantId: string | null }>(base, 'auth/session');
      if (!session.participantId) {
        access = 'checking';
        await goto('/login');
        return;
      }
      // The API remains the authority; never expose queue content on a denied response.
      const params = new URLSearchParams({ page: String(page), search, reviewOnly: String(reviewOnly), sort });
      const result = await apiRequest<{ items: QueueItem[]; totalCount: number; hasNextPage: boolean }>(base, 'moderation/queue?' + params);
      queue = result.items;
      queuePage = page;
      queueTotal = result.totalCount;
      queueHasMore = result.hasNextPage;
      access = 'allowed';
    } catch (e) {
      queue = [];
      queueTotal = 0;
      queueHasMore = false;
      selected = null;
      history = [];
      if (e instanceof ApiRequestError && e.status === 401) {
        access = 'checking';
        await goto('/login');
      } else if (e instanceof ApiRequestError && e.status === 403) {
        access = 'denied';
      } else {
        access = 'checking';
        error = e instanceof Error ? e.message : 'Unable to verify moderation access.';
      }
    } finally {
      loading = false;
    }
  }
  async function openItem(id: string) {
    selected = id;
    history = [];
    error = '';
    try {
      history = await apiRequest<Case[]>(base, 'moderation/nodes/' + encodeURIComponent(id) + '/history');
    } catch (e) {
      error = e instanceof Error ? e.message : 'Unable to load report history.';
    }
  }
  async function decide(decision: 0 | 1) {
    if (!selected || !rationale.trim() || busy) return;
    busy = true;
    error = '';
    notice = '';
    try {
      await apiRequest<void>(base, 'moderation/nodes/' + encodeURIComponent(selected) + '/decision',
        'POST', { decision, rationale: rationale.trim(), reason: publicReasons.indexOf(publicReason) });
      notice = decision === 0 ? 'Reports dismissed.' : 'Node hidden with public reason.';
      rationale = '';
      selected = null;
      await refresh(1);
    } catch (e) {
      error = e instanceof Error ? e.message : 'Could not save decision.';
    } finally {
      busy = false;
    }
  }
  async function restore() {
    if (!selected || !rationale.trim() || busy) return;
    busy = true;
    error = '';
    notice = '';
    try {
      await apiRequest<void>(base, 'moderation/nodes/' + encodeURIComponent(selected) + '/restore',
        'POST', { text: rationale.trim() });
      notice = 'Node visibility restored.';
      rationale = '';
      selected = null;
      await refresh();
    } catch (e) {
      error = e instanceof Error ? e.message : 'Could not restore visibility.';
    } finally {
      busy = false;
    }
  }
  async function confirmAction() {
    const action = confirmation;
    confirmation = null;
    if (action === 'restore') await restore();
    else if (action) await decide(action === 'hide' ? 1 : 0);
  }
  onMount(() => { void refresh(); });
  const current = $derived(queue.find((item) => item.nodeId === selected));
  // Filters only affect visibility/order; decisions always target the selected node ID.
  const visibleQueue = $derived(queue);
</script>

<svelte:head><title>Moderation · Atlas</title></svelte:head>
<section class="moderation">
  {#if loading}<p role="status">Checking access…</p>
  {:else if access === 'denied'}
    <h1>Access denied</h1>
    <p role="alert">You do not have permission to view moderation.</p>
  {:else if access === 'allowed'}
    <div class="page-heading"><div><h1>Moderation</h1><p>Review reports, investigate contributions, and record decisions.</p></div><span class="queue-total">{queueTotal} in queue</span></div>
    {#if error}<p role="alert">{error}</p>{/if}
    {#if notice}<p role="status">{notice}</p>{/if}
    {#if queueTotal === 0}<p>No pending reports or review requests matching these filters.</p>{/if}
    <div class="filters panel" role="search" aria-label="Filter moderation queue">
        <label for="moderation-search">Search node titles</label>
        <input id="moderation-search" type="search" bind:value={search} placeholder="Find a reported node…" />
        <label class="checkbox"><input type="checkbox" bind:checked={reviewOnly} /> Author review requests only</label>
        <label for="moderation-sort">Sort cases</label>
        <select id="moderation-sort" bind:value={sort}>
          <option value="reports">Most reports first</option>
          <option value="title">Node title A–Z</option>
        </select>
        <button class="primary" type="button" disabled={loading} onclick={() => void refresh(1)}>Apply filters</button>
        <p class="filter-count" role="status">Showing {visibleQueue.length} of {queueTotal} nodes · Page {queuePage}</p>
      </div>
      {#if visibleQueue.length === 0 && queueTotal > 0}<p>No nodes on this page.</p>{/if}
    <ul class="queue-list" aria-label="Moderation queue">
      {#each visibleQueue as item (item.nodeId)}
        <li>
          <button class="queue-card" type="button" onclick={() => void openItem(item.nodeId)} aria-pressed={selected === item.nodeId}>
            <span class="queue-title">{item.reportedTitle}</span>
            <span class="queue-meta"><span class="count-pill">{item.count} pending {item.count === 1 ? 'report' : 'reports'}</span>
              {#if item.reviewRequested}<span class="review-pill">Author requested review</span>{/if}</span>
          </button>
        </li>
      {/each}
    </ul>
    {#if queuePage > 1 || queueHasMore}<div class="actions"><button type="button" disabled={loading || queuePage === 1} onclick={() => void refresh(queuePage - 1)}>Previous</button><button type="button" disabled={loading || !queueHasMore} onclick={() => void refresh(queuePage + 1)}>Next</button></div>{/if}
    {#if current}
      <section class="case-detail panel" aria-label="Selected moderation case">
        <h2>{current.reportedTitle}</h2>
        <p><a href={'/nodes?id=' + encodeURIComponent(current.nodeId)}>View node for read-only investigation</a></p>
        <h3>Report history</h3>
        {#if history.length === 0}<p>No report details loaded.</p>{/if}
        <ol class="history-list">{#each history as report (report.id)}
          <li>
            <div class="history-event"><strong>Report submitted</strong><time datetime={report.createdAt}>{new Date(report.createdAt).toLocaleString()}</time></div>
            <p>Reported by {report.reporter} · {report.reason}</p>
            {#if report.explanation}<p>{report.explanation}</p>{/if}
            {#if report.decidedAt}
              <div class="history-event"><strong>Moderator decision: {report.status === 'Actioned' ? 'Hidden' : report.status}</strong><time datetime={report.decidedAt}>{new Date(report.decidedAt).toLocaleString()}</time></div>
              <p>Decided by {report.reviewer ?? 'Unknown moderator'} · {report.publicReason}</p>
              {#if report.decisionReason}<p>Rationale: {report.decisionReason}</p>{/if}
            {:else}<p>Awaiting moderator decision</p>{/if}
            {#if report.reviewRequestedAt}
              <div class="history-event"><strong>Author requested review</strong><time datetime={report.reviewRequestedAt}>{new Date(report.reviewRequestedAt).toLocaleString()}</time></div>
            {/if}
            {#if report.visibilityRestoredAt}
              <div class="history-event"><strong>Visibility restored by {report.restoredBy ?? 'Unknown moderator'}</strong><time datetime={report.visibilityRestoredAt}>{new Date(report.visibilityRestoredAt).toLocaleString()}</time></div>
              {#if report.restorationReason}<p>Rationale: {report.restorationReason}</p>{/if}
            {/if}
          </li>
        {/each}</ol>
        <div class="decision-area"><h3>Record a decision</h3><label for="rationale">Moderator rationale (required)</label>
        <textarea id="rationale" bind:value={rationale} maxlength="2000" rows="3"></textarea>
        {#if current.count > 0}
          <label for="public-reason">Public reason when hiding</label>
          <select id="public-reason" bind:value={publicReason}>
            {#each publicReasons as reason}<option value={reason}>{reason}</option>{/each}
          </select>
          <div class="actions">
            <button disabled={busy || !rationale.trim()} onclick={() => confirmation = 'dismiss' }>Dismiss reports</button>
            <button disabled={busy || !rationale.trim()} onclick={() => confirmation = 'hide' }>Hide node</button>
          </div>
        {/if}
        {#if current.reviewRequested}
          <button disabled={busy || !rationale.trim()} onclick={() => confirmation = 'restore' }>Restore after author review request</button>
        {/if}
        </div>
      </section>
    {/if}
  {:else if error}
    <p role="alert">{error}</p>
  {/if}
</section>
{#if confirmation}
  <div class="confirmation-backdrop">
    <section class="confirmation-dialog" role="alertdialog" aria-modal="true" aria-labelledby="confirm-title" aria-describedby="confirm-description">
      <h2 id="confirm-title">{confirmation === 'hide' ? 'Hide this node?' : confirmation === 'dismiss' ? 'Dismiss these reports?' : 'Restore this node?'}</h2>
      <p id="confirm-description">{confirmation === 'hide'
        ? 'The node will be excluded from public discovery and search. Its author can edit it and request review. Pending reports will be closed.'
        : confirmation === 'dismiss'
          ? 'The node will remain public and its pending reports will be closed.'
          : 'The original node will become publicly visible again, including in discovery and search.'}</p>
      <div class="actions">
        <button type="button" onclick={() => confirmation = null}>Cancel</button>
        <button type="button" onclick={() => void confirmAction()}>{confirmation === 'hide' ? 'Hide node' : confirmation === 'dismiss' ? 'Dismiss reports' : 'Restore node'}</button>
      </div>
    </section>
  </div>
{/if}
<style>
  .moderation { max-width: 66rem; margin: 2.5rem auto 4rem; padding: 0 1rem; }
  .page-heading { display: flex; align-items: center; justify-content: space-between; gap: 1rem; margin-bottom: 1.5rem; }
  .page-heading h1 { margin: 0 0 .3rem; font-size: clamp(1.8rem, 4vw, 2.4rem); }
  .page-heading p, .filter-count { color: var(--atlas-muted); margin: 0; }
  .queue-total, .count-pill, .review-pill { display: inline-flex; align-items: center; border-radius: 99px; padding: .3rem .7rem; font-size: .85rem; font-weight: 600; white-space: nowrap; }
  .queue-total, .count-pill { background: color-mix(in srgb, var(--atlas-accent) 10%, var(--atlas-surface)); color: var(--atlas-accent); }
  .review-pill { background: color-mix(in srgb, #bd8336 17%, var(--atlas-surface)); color: var(--atlas-text); }
  .panel { background: var(--atlas-surface); border: 1px solid color-mix(in srgb, var(--atlas-border) 45%, transparent); border-radius: 1rem; padding: 1.4rem; box-shadow: 0 .3rem 1.2rem rgb(0 0 0 / .035); }
  .filters { display: grid; grid-template-columns: minmax(0, 1fr) minmax(12rem, .65fr); gap: .6rem 1.2rem; align-items: end; margin-bottom: 1.6rem; }
  .filters > label { font-weight: 600; font-size: .9rem; }
  .filters > label[for='moderation-search'] { grid-column: 1; grid-row: 1; }
  .filters input[type='search'] { grid-column: 1; grid-row: 2; }
  .filters > label[for='moderation-sort'] { grid-column: 2; grid-row: 1; }
  .filters select { grid-column: 2; grid-row: 2; }
  .filters .checkbox { grid-column: 1 / -1; display: flex; align-items: center; gap: .55rem; font-weight: 400; margin: .4rem 0; }
  .filters .checkbox input { width: 1.1rem; height: 1.1rem; accent-color: var(--atlas-accent); }
  .filters .filter-count { grid-column: 2; text-align: right; font-size: .85rem; align-self: center; }
  input[type='search'], select, textarea { display: block; width: 100%; min-width: 0; border: 1px solid var(--atlas-border); border-radius: .5rem; padding: .65rem .8rem; background: var(--atlas-surface); color: var(--atlas-text); font: inherit; }
  textarea { resize: vertical; margin: .45rem 0 1rem; }
  button { cursor: pointer; padding: .65rem 1rem; border: 1px solid var(--atlas-border); border-radius: .55rem; background: var(--atlas-surface); color: var(--atlas-text); font: inherit; }
  button:hover:not(:disabled) { border-color: var(--atlas-accent); }
  button:disabled { cursor: not-allowed; opacity: .55; }
  button.primary { justify-self: start; background: var(--atlas-accent); color: var(--atlas-on-accent); border-color: var(--atlas-accent); font-weight: 600; }
  .queue-list { list-style: none; padding: 0; display: grid; gap: .7rem; margin: 0 0 1.5rem; }
  .queue-list li { margin: 0; }
  .queue-card { display: flex; width: 100%; flex-wrap: wrap; align-items: center; justify-content: space-between; text-align: left; gap: .7rem 1rem; padding: 1rem 1.2rem; background: var(--atlas-surface); border: 1px solid color-mix(in srgb, var(--atlas-border) 50%, transparent); border-radius: .8rem; transition: border-color .15s, box-shadow .15s; }
  .queue-card:hover, .queue-card[aria-pressed='true'] { border-color: var(--atlas-accent); box-shadow: 0 .2rem .8rem rgb(0 0 0 / .06); }
  .queue-card[aria-pressed='true'] { box-shadow: inset 4px 0 var(--atlas-accent); }
  .queue-title { font-weight: 650; line-height: 1.4; }
  .queue-meta { display: flex; flex-wrap: wrap; gap: .4rem; }
  .case-detail { margin-top: 1.5rem; }
  .case-detail h2 { margin: 0 0 .5rem; font-size: 1.45rem; }
  .case-detail h3 { margin: 1.5rem 0 .85rem; }
  .history-list { list-style: none; padding: 0; margin: 0; }
  .history-list > li { padding: 1rem 1.1rem; margin: .7rem 0; background: var(--atlas-bg); border-radius: .7rem; border-left: 3px solid var(--atlas-accent); }
  .history-list p { margin: .45rem 0; overflow-wrap: anywhere; }
  .history-event { display: flex; justify-content: space-between; flex-wrap: wrap; gap: .3rem 1rem; margin-top: .85rem; }
  .history-event:first-child { margin-top: 0; }
  .history-event time { color: var(--atlas-muted); font-size: .85rem; }
  .decision-area { border-top: 1px solid color-mix(in srgb, var(--atlas-border) 40%, transparent); margin-top: 1.5rem; padding-top: .25rem; }
  .decision-area label { display: block; font-weight: 600; margin-top: .8rem; }
  .decision-area select { margin: .45rem 0 1rem; }
  .actions { display: flex; gap: .65rem; flex-wrap: wrap; margin: 1rem 0; }
  .confirmation-backdrop { position: fixed; inset: 0; z-index: 1000; background: rgb(0 0 0 / .55); display: grid; place-items: center; padding: 1rem; }
  .confirmation-dialog { background: var(--atlas-surface); color: var(--atlas-text); border-radius: .9rem; padding: 1.5rem; max-width: 32rem; width: 100%; box-shadow: 0 1rem 3rem rgb(0 0 0 / .25); }
  .confirmation-dialog .actions { justify-content: flex-end; }
  @media (max-width: 640px) {
    .moderation { margin: 1.5rem auto 3rem; padding: 0 .35rem; }
    .page-heading { align-items: flex-start; }
    .panel { padding: 1rem; }
    .filters { grid-template-columns: minmax(0, 1fr); }
    .filters > label[for='moderation-sort'], .filters select, .filters .checkbox, .filters .filter-count { grid-column: 1; grid-row: auto; }
    .filters .filter-count { text-align: left; }
    .queue-card { align-items: flex-start; flex-direction: column; }
  }
</style>

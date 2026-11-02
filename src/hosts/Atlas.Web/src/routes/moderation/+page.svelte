<!-- Global moderators review reports; backend policies authorize every operation. -->
<script lang="ts">
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  import { apiRequest, ApiRequestError } from '$lib/api/apiRequest';

  type QueueItem = { nodeId: string; reportedTitle: string; count: number; reviewRequested: boolean };
  type Case = { id: string; reason: string; explanation: string | null; createdAt: string; status: string };
  const base = '/atlas-session-api';
  const publicReasons = ['Other', 'Spam', 'Harassment', 'UnsafeContent', 'OffTopic'];
  let queue = $state<QueueItem[]>([]);
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

  async function refresh() {
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
      queue = await apiRequest<QueueItem[]>(base, 'moderation/queue');
      access = 'allowed';
    } catch (e) {
      queue = [];
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
      await refresh();
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
  const visibleQueue = $derived(queue.filter((item) =>
    item.reportedTitle.toLocaleLowerCase().includes(search.trim().toLocaleLowerCase()) &&
    (!reviewOnly || item.reviewRequested)
  ).sort((a, b) => sort === 'reports'
    ? b.count - a.count || a.reportedTitle.localeCompare(b.reportedTitle)
    : a.reportedTitle.localeCompare(b.reportedTitle)));
</script>

<svelte:head><title>Moderation · Atlas</title></svelte:head>
<section class="moderation">
  {#if loading}<p role="status">Checking access…</p>
  {:else if access === 'denied'}
    <h1>Access denied</h1>
    <p role="alert">You do not have permission to view moderation.</p>
  {:else if access === 'allowed'}
    <h1>Moderation</h1>
    {#if error}<p role="alert">{error}</p>{/if}
    {#if notice}<p role="status">{notice}</p>{/if}
    {#if queue.length === 0}<p>No pending reports or review requests.</p>{/if}
    {#if queue.length > 0}
      <div class="filters" role="search" aria-label="Filter moderation queue">
        <label for="moderation-search">Search node titles</label>
        <input id="moderation-search" type="search" bind:value={search} placeholder="Find a reported node…" />
        <label class="checkbox"><input type="checkbox" bind:checked={reviewOnly} /> Author review requests only</label>
        <label for="moderation-sort">Sort cases</label>
        <select id="moderation-sort" bind:value={sort}>
          <option value="reports">Most reports first</option>
          <option value="title">Node title A–Z</option>
        </select>
        <p role="status">Showing {visibleQueue.length} of {queue.length} nodes</p>
      </div>
      {#if visibleQueue.length === 0}<p>No nodes match these filters.</p>{/if}
    {/if}
    <ul>
      {#each visibleQueue as item (item.nodeId)}
        <li>
          <button type="button" onclick={() => void openItem(item.nodeId)} aria-pressed={selected === item.nodeId}>
            {item.reportedTitle} — {item.count} pending {item.count === 1 ? 'report' : 'reports'}
            {item.reviewRequested ? ' · Author requested review' : ''}
          </button>
        </li>
      {/each}
    </ul>
    {#if current}
      <section aria-label="Selected moderation case">
        <h2>{current.reportedTitle}</h2>
        <p><a href={'/nodes?id=' + encodeURIComponent(current.nodeId)}>View node (read-only investigation)</a></p>
        <h3>Report history</h3>
        {#if history.length === 0}<p>No report details loaded.</p>{/if}
        <ul>{#each history as report (report.id)}
          <li><strong>{report.reason}</strong> · {report.status} · <time datetime={report.createdAt}>{new Date(report.createdAt).toLocaleString()}</time>
            {#if report.explanation}<p>{report.explanation}</p>{/if}
          </li>
        {/each}</ul>
        <label for="rationale">Moderator rationale (required)</label>
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
  .confirmation-backdrop { position: fixed; inset: 0; z-index: 1000; background: rgb(0 0 0 / .55); display: grid; place-items: center; padding: 1rem; }
  .confirmation-dialog { background: var(--atlas-surface, #fff); color: var(--atlas-text, #222); border-radius: .75rem; padding: 1.5rem; max-width: 32rem; width: 100%; box-shadow: 0 1rem 3rem rgb(0 0 0 / .25); }
  .confirmation-dialog .actions { justify-content: flex-end; margin-top: 1.5rem; }
  .moderation { max-width: 55rem; margin: 2rem auto; padding: 0 1rem; }
  ul { padding-left: 1.5rem; }
  li { margin: .65rem 0; }
  button { cursor: pointer; padding: .55rem .85rem; }
  button:disabled { cursor: not-allowed; opacity: .55; }
  textarea, select { display: block; width: 100%; max-width: 36rem; margin: .35rem 0 1rem; }
  .filters { padding: 1rem 0; }
  .filters > label { display: block; margin: .5rem 0 .25rem; }
  .filters input[type='search'] { width: 100%; max-width: 36rem; padding: .5rem; }
  .filters .checkbox { display: flex; align-items: center; gap: .5rem; }
  .actions { display: flex; gap: .65rem; flex-wrap: wrap; margin-bottom: 1rem; }
  section section { margin-top: 1.5rem; border-top: 1px solid var(--atlas-muted); padding-top: 1rem; }
</style>

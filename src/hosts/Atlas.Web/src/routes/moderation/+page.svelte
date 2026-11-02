<!-- Global moderators review reports; backend policies authorize every operation. -->
<script lang="ts">
  import { onMount } from 'svelte';
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
  let denied = $state(false);
  let error = $state('');
  let notice = $state('');
  let rationale = $state('');
  let publicReason = $state('Other');

  async function refresh() {
    loading = true;
    error = '';
    try {
      queue = await apiRequest<QueueItem[]>(base, 'moderation/queue');
      denied = false;
    } catch (e) {
      denied = e instanceof ApiRequestError && (e.status === 401 || e.status === 403);
      if (!denied) error = e instanceof Error ? e.message : 'Unable to load reports.';
      queue = [];
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
  onMount(() => { void refresh(); });
  const current = $derived(queue.find((item) => item.nodeId === selected));
</script>

<svelte:head><title>Moderation · Atlas</title></svelte:head>
<section class="moderation">
  <h1>Global moderation</h1>
  <p>Review reported nodes across all communities, including nodes without a community. Moderators cannot edit authors’ content.</p>
  {#if loading}<p role="status">Loading reports…</p>
  {:else if denied}<p role="alert">Global moderator access is required. Sign in with a provisioned account.</p>
  {:else}
    {#if error}<p role="alert">{error}</p>{/if}
    {#if notice}<p role="status">{notice}</p>{/if}
    {#if queue.length === 0}<p>No pending reports or review requests.</p>{/if}
    <ul>
      {#each queue as item (item.nodeId)}
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
            <button disabled={busy || !rationale.trim()} onclick={() => void decide(0)}>Dismiss reports</button>
            <button disabled={busy || !rationale.trim()} onclick={() => void decide(1)}>Hide node</button>
          </div>
        {/if}
        {#if current.reviewRequested}
          <button disabled={busy || !rationale.trim()} onclick={() => void restore()}>Restore after author review request</button>
        {/if}
      </section>
    {/if}
  {/if}
</section>
<style>
  .moderation { max-width: 55rem; margin: 2rem auto; padding: 0 1rem; }
  ul { padding-left: 1.5rem; }
  li { margin: .65rem 0; }
  button { cursor: pointer; padding: .55rem .85rem; }
  button:disabled { cursor: not-allowed; opacity: .55; }
  textarea, select { display: block; width: 100%; max-width: 36rem; margin: .35rem 0 1rem; }
  .actions { display: flex; gap: .65rem; flex-wrap: wrap; margin-bottom: 1rem; }
  section section { margin-top: 1.5rem; border-top: 1px solid var(--atlas-muted); padding-top: 1rem; }
</style>

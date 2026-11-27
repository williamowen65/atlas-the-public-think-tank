<!-- Loaded only when an ancestry disclosure opens. Do not recursively render the same path. -->
<script lang="ts">
  import { readDiscovery, discoveryCard } from '../api/discovery';
  import type { NodeCardData } from './node-card-versions/types';
  import LiveVotingDial from './LiveVotingDial.svelte';
  import LiveReactions from './LiveReactions.svelte';
  let { id, title, apiBase = '/atlas-public-api', onNavigate }: {
    id: string; title: string; apiBase?: string; onNavigate?: (id: string) => void;
  } = $props();
  let card = $state<NodeCardData | null>(null);
  let average = $state<number | null>(null);
  let totalVotes = $state(0);
  let loading = $state(true);
  let failed = $state(false);
  let retry = $state(0);
  const compactCard = import('./node-card-versions/NodeCardRibbonCorner.svelte');
  function open(nodeId: string) {
    if (onNavigate) onNavigate(nodeId);
    else window.location.assign('/nodes?id=' + encodeURIComponent(nodeId));
  }
  function discussion(nodeId: string, type: string) {
    window.location.assign('/nodes?id=' + encodeURIComponent(nodeId) + '&type=' + encodeURIComponent(type) + '#contributions');
  }
  $effect(() => {
    const nodeId = id, base = apiBase;
    retry;
    const controller = new AbortController();
    loading = true; failed = false; card = null;
    void readDiscovery(base, 1, '', AbortSignal.any([controller.signal, AbortSignal.timeout(10000)]), nodeId)
      .then(page => {
        if (controller.signal.aborted) return;
        const entry = page.items.find(item => item.nodeId === nodeId);
        const display = entry && discoveryCard(entry);
        if (!entry || !display || display.isHidden) throw new Error('Unavailable ancestor');
        card = { ...display, breadcrumb: [] };
        average = entry.averageVote;
        totalVotes = entry.voteCount;
      })
      .catch(() => { if (!controller.signal.aborted) failed = true; })
      .finally(() => { if (!controller.signal.aborted) loading = false; });
    return () => controller.abort();
  });
</script>

{#if card}
  {#await compactCard then { default: CompactCard }}
    <CompactCard node={card} mobileLayout {apiBase} {average} {totalVotes}
      onReadMore={open} onNavigate={open} onComments={(id) => discussion(id, 'comments')} onChildType={discussion}>
      {#snippet voting()}<LiveVotingDial nodeId={id} initialAverage={average} initialTotalVotes={totalVotes} forceCompact archived={card?.status === 'Archived'} />{/snippet}
      {#snippet reactionControls()}<LiveReactions nodeId={id} archived={card?.status === 'Archived'} />{/snippet}
    </CompactCard>
  {:catch}
    <div class="fallback"><a href={'/nodes?id=' + encodeURIComponent(id)}>{title}</a></div>
  {/await}
{:else}
  <div class="fallback">
    <a href={'/nodes?id=' + encodeURIComponent(id)} onclick={(event) => { if (onNavigate && !event.metaKey && !event.ctrlKey && !event.shiftKey && !event.altKey) { event.preventDefault(); open(id); } }}>{title}</a>
    {#if loading}<span role="status">Loading parent node…</span>{:else if failed}<button type="button" onclick={() => retry++}>Retry preview</button>{/if}
  </div>
{/if}

<style>
  .fallback { border: 1px solid var(--atlas-border); border-radius: var(--atlas-radius); padding: 1rem; background: var(--atlas-surface); color: var(--atlas-text); display: grid; gap: .4rem; }
  .fallback a { color: var(--atlas-accent); }
  .fallback span { color: var(--atlas-muted); }
  .fallback button { justify-self: start; }
</style>

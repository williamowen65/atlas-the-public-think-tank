<script lang="ts">
  import { onMount } from 'svelte';
  import { discoveryCard, readDiscovery } from '../api/discovery';
  import LiveVotingDial from './LiveVotingDial.svelte';
  import NodeFeed, { type FeedItem } from './NodeFeed.svelte';

  let { nodeId }: { nodeId: string } = $props();
  let items = $state<FeedItem[]>([]);
  let loading = $state(true);
  let error = $state('');

  onMount(() => {
    const request = new AbortController();
    async function load() {
      try {
        if (!nodeId) throw new Error('Missing node ID');
        const page = await readDiscovery('/atlas-public-api', 1, '', request.signal, nodeId);
        const entry = page.items.find((item) => item.nodeId === nodeId);
        const node = entry && discoveryCard(entry);
        if (!entry || !node || node.isHidden || node.status !== 'Active') throw new Error('Unavailable node');
        items = [{
          node: { ...node, breadcrumb: [] },
          average: entry.averageVote,
          totalVotes: entry.voteCount,
        }];
      } catch {
        if (!request.signal.aborted) error = 'This node is unavailable.';
      } finally {
        if (!request.signal.aborted) loading = false;
      }
    }
    void load();
    return () => request.abort();
  });
</script>

<div data-share-node>
  <NodeFeed
    {items}
    {loading}
    {error}
    presentation="mobile-experiment"
    onReadMore={(id) => window.location.assign(`/nodes?id=${encodeURIComponent(id)}`)}
  >
    {#snippet renderVoting(item, compact)}
      <LiveVotingDial
        nodeId={item.node.id}
        initialAverage={item.average}
        initialTotalVotes={item.totalVotes}
        forceCompact={compact}
        preview
      />
    {/snippet}
  </NodeFeed>
</div>

<style>
  [data-share-node] :global(.node-card > header) {
    padding-block: 0.65rem 0.2rem;
  }

  [data-share-node] :global(.node-card > header h2) {
    margin-block-end: 0.25rem;
  }

  [data-share-node] :global(.node-card > .body) {
    padding-block-start: 0.15rem;
  }
</style>

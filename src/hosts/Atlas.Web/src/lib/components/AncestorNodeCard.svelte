<!-- Lazy ancestry projection: use known title immediately, then add type and vote summary. -->
<script lang="ts">
  import { readDiscovery, discoveryCard } from '../api/discovery';
  import type { NodeCardData } from './node-card-versions/types';
  import NodeCardMini from './node-card-versions/NodeCardMini.svelte';
  let { id, title, apiBase = '/atlas-public-api', onNavigate }: {
    id: string; title: string; apiBase?: string; onNavigate?: (id: string) => void;
  } = $props();
  let card = $state<Pick<NodeCardData, 'id' | 'title' | 'typeName' | 'status'> | null>(null);
  let average = $state<number | null>(null);
  let totalVotes = $state<number | undefined>(undefined);
  let loading = $state(true);
  let failed = $state(false);
  let retry = $state(0);

  $effect(() => {
    const nodeId = id, base = apiBase;
    retry;
    const controller = new AbortController();
    loading = true; failed = false; card = null; average = null; totalVotes = undefined;
    void readDiscovery(base, 1, '', AbortSignal.any([controller.signal, AbortSignal.timeout(10000)]), nodeId)
      .then(page => {
        if (controller.signal.aborted) return;
        const entry = page.items.find(item => item.nodeId === nodeId);
        const display = entry && discoveryCard(entry);
        if (!entry || !display || display.isHidden) throw new Error('Unavailable ancestor');
        card = { id: display.id, title: display.title, typeName: display.typeName, status: display.status };
        average = entry.averageVote;
        totalVotes = entry.voteCount;
      })
      .catch(() => { if (!controller.signal.aborted) failed = true; })
      .finally(() => { if (!controller.signal.aborted) loading = false; });
    return () => controller.abort();
  });
</script>

<div class="ancestor-preview" aria-busy={loading}>
  <NodeCardMini node={card ?? { id, title }} {average} {totalVotes} {onNavigate} />
  {#if failed}
    <button class="retry" type="button" onclick={() => retry++}>Retry node details</button>
  {/if}
</div>

<style>
  .ancestor-preview { min-width: 0; }
  .retry { display: block; margin: .2rem auto 0; font: inherit; font-size: .75rem; color: var(--atlas-accent); background: transparent; border: 0; cursor: pointer; }
</style>

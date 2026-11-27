<!-- Lazy ancestry projection: mount the mini card only after its complete display model is ready. -->
<script lang="ts">
  import { readDiscovery, discoveryCard } from '../api/discovery';
  import type { NodeCardData } from './node-card-versions/types';
  import NodeCardMini from './node-card-versions/NodeCardMini.svelte';
  let { id, title, apiBase = '/atlas-public-api', onNavigate }: {
    id: string; title: string; apiBase?: string; onNavigate?: (id: string) => void;
  } = $props();

  type Preview = {
    node: Pick<NodeCardData, 'id' | 'title' | 'typeName' | 'status'>;
    average: number | null;
    totalVotes: number;
  };
  // A single snapshot prevents rendering a title-only card before its metadata arrives.
  let preview = $state<Preview | null>(null);
  let loading = $state(true);
  let failed = $state(false);
  let retry = $state(0);

  $effect(() => {
    const nodeId = id, base = apiBase;
    retry;
    const controller = new AbortController();
    loading = true; failed = false; preview = null;
    void readDiscovery(base, 1, '', AbortSignal.any([controller.signal, AbortSignal.timeout(10000)]), nodeId)
      .then(page => {
        if (controller.signal.aborted) return;
        const entry = page.items.find(item => item.nodeId === nodeId);
        const display = entry && discoveryCard(entry);
        if (!entry || !display || display.isHidden) throw new Error('Unavailable ancestor');
        preview = {
          node: { id: display.id, title: display.title, typeName: display.typeName, status: display.status },
          average: entry.averageVote,
          totalVotes: entry.voteCount,
        };
      })
      .catch(() => { if (!controller.signal.aborted) failed = true; })
      .finally(() => { if (!controller.signal.aborted) loading = false; });
    return () => controller.abort();
  });
</script>

<div class="ancestor-preview" aria-busy={loading}>
  {#if preview}
    <NodeCardMini node={preview.node} average={preview.average} totalVotes={preview.totalVotes} {onNavigate} />
  {:else if failed}
    <div class="preview-status" role="status">
      Could not load ancestor details.
      <a href={'/nodes?id=' + encodeURIComponent(id)}>{title}</a>
      <button class="retry" type="button" onclick={() => retry++}>Retry node details</button>
    </div>
  {:else}
    <div class="preview-status" role="status">Loading ancestor…</div>
  {/if}
</div>

<style>
  .ancestor-preview { min-width: 0; }
  /* Reserve roughly two lines of card space without inserting a partial mini card. */
  .preview-status {
    box-sizing: border-box;
    min-block-size: 3.45rem;
    display: grid;
    align-content: center;
    justify-items: center;
    gap: .2rem;
    color: var(--atlas-muted);
    font-size: .8rem;
    text-align: center;
    overflow-wrap: anywhere;
  }
  .preview-status a { color: var(--atlas-accent); }
  .retry { font: inherit; color: var(--atlas-accent); background: transparent; border: 0; cursor: pointer; }
</style>

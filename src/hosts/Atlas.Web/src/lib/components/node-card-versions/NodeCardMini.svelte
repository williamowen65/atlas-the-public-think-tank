<!-- Minimal read-only navigation card: no body, reactions, voting mutations or nested graph reads. -->
<script lang="ts">
  import type { NodeCardData } from './types';
  let { node, average = null, totalVotes, onNavigate }: {
    node: Pick<NodeCardData, 'id' | 'title'> & Partial<Pick<NodeCardData, 'typeName' | 'status'>>;
    average?: number | null;
    totalVotes?: number;
    onNavigate?: (id: string) => void;
  } = $props();
</script>

<article class="mini-node" data-presentation="mini">
  {#if node.typeName || totalVotes !== undefined || node.status === 'Archived'}
    <div class="mini-meta">
      {#if node.typeName}<span class="mini-type">{node.typeName}</span>{/if}
      {#if node.status === 'Archived'}<span class="mini-status">Archived</span>{/if}
      {#if totalVotes !== undefined}
        <span class="mini-score" aria-label={average == null ? `Not rated, ${totalVotes} votes` : `Average importance ${average.toFixed(1)} out of 10, ${totalVotes} votes`}>
          {average == null ? '—' : average.toFixed(1)} · {totalVotes} {totalVotes === 1 ? 'vote' : 'votes'}
        </span>
      {/if}
    </div>
  {/if}
  <a class="mini-title" href={`/nodes?id=${encodeURIComponent(node.id)}`}
    onclick={(event) => {
      if (!onNavigate || event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) return;
      event.preventDefault();
      onNavigate(node.id);
    }}>{node.title}</a>
</article>

<style>
  .mini-node {
    box-sizing: border-box;
    display: grid;
    gap: .2rem;
    width: 100%;
    min-width: 0;
    padding: .45rem .7rem .55rem;
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    background: var(--atlas-surface);
    color: var(--atlas-text);
    overflow-wrap: anywhere;
  }
  .mini-meta { display: flex; align-items: baseline; flex-wrap: wrap; gap: .2rem .55rem; min-width: 0; font-size: .75rem; line-height: 1.3; }
  .mini-type { font-weight: 700; color: var(--atlas-accent); min-width: 0; }
  .mini-score { margin-inline-start: auto; color: var(--atlas-muted); font-variant-numeric: tabular-nums; }
  .mini-status { color: var(--atlas-muted); }
  .mini-title { display: block; color: var(--atlas-text); font-size: .95rem; font-weight: 650; line-height: 1.35; text-decoration: none; }
  .mini-title:hover { color: var(--atlas-accent); text-decoration: underline; }
  .mini-title:focus-visible { outline: 2px solid var(--atlas-accent); outline-offset: 2px; border-radius: .15rem; }
</style>

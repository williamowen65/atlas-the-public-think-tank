<script lang="ts" module>
  import type { NodeCardData } from './NodeCard.svelte';
  export interface FeedItem { node: NodeCardData; average: number | null; totalVotes: number; reactionError?: boolean; hasMoreAncestors?: boolean }
</script>
<script lang="ts">
  import type { Snippet } from 'svelte';
  import NodeCard, { type NodeCardPresentation } from './NodeCard.svelte';
  import type { LoadOlderAncestors } from './Breadcrumb.svelte';
  let { renderVoting, renderReactions, items, loading = false, hasMore = false, error = '', presentation = 'ribbon-corner', onLoadMore, onNavigate, onOpenProfile, onReadMore, onComments, onChildType, loadOlderAncestors, emptyTypeName, onCreateFirst }: {
    renderVoting?: Snippet<[FeedItem]>; renderReactions?: Snippet<[FeedItem]>; items: FeedItem[]; loading?: boolean; hasMore?: boolean; error?: string;
    presentation?: NodeCardPresentation; onLoadMore?: () => void; onNavigate?: (id: string) => void; onOpenProfile?: (id: string) => void; onReadMore?: (id: string) => void; loadOlderAncestors?: LoadOlderAncestors; onChildType?: (id: string, typeId: string) => void; onComments?: (id: string) => void; emptyTypeName?: string; onCreateFirst?: () => void;
  } = $props();
  let tail: HTMLDivElement;
  const singularType = $derived((emptyTypeName ?? '').trim());
  const article = $derived(/^[aeiou]/i.test(singularType) ? 'an' : 'a');
  $effect(() => {
    items.length;
    if (typeof IntersectionObserver === 'undefined' || !tail || loading || error || !hasMore || !onLoadMore) return;
    const observer = new IntersectionObserver(entries => {
      if (entries.some(entry => entry.isIntersecting)) onLoadMore?.();
    }, { rootMargin: '0px 0px 240px 0px' });
    observer.observe(tail);
    return () => observer.disconnect();
  });
</script>
<section class="node-feed" aria-label="Node feed" aria-busy={loading}>
  <ul aria-label="Nodes">
    {#each items as item (item.node.id)}
      {#snippet voting()}{#if renderVoting}{@render renderVoting(item)}{/if}{/snippet}
      {#snippet reactions()}{#if renderReactions}{@render renderReactions(item)}{/if}{/snippet}
      <li><NodeCard reactionControls={renderReactions ? reactions : undefined} voting={renderVoting ? voting : undefined} node={item.node} {presentation} average={item.average} totalVotes={item.totalVotes} {onNavigate} {onOpenProfile} {onReadMore} {onComments} {onChildType} {loadOlderAncestors} hasMoreAncestors={item.hasMoreAncestors ?? false} />
        {#if item.reactionError}<p class="note">Reaction totals are unavailable for this node.</p>{/if}
      </li>
    {/each}
  </ul>
  <div class="feed-tail" bind:this={tail}>
    {#if error}<p role="alert">{error}</p>{/if}
    {#if !loading && !error && !items.length && !hasMore && emptyTypeName && onCreateFirst}
      <div class="empty-invitation"><h3>Be the first to create {article} {singularType.toLowerCase()}</h3><p>There aren't any {singularType.toLowerCase()} contributions here yet. Start the conversation by adding one.</p><button type="button" onclick={onCreateFirst}>Create {article} {singularType.toLowerCase()}</button></div>
    {:else}<p role="status">{loading ? 'Loading nodes…' : !items.length ? (error ? 'No nodes loaded.' : 'No nodes found.') : `${items.length} nodes shown${hasMore ? '.' : ' · You’re all caught up.'}`}</p>{/if}
    {#if hasMore || error}<button type="button" disabled={loading || !onLoadMore} onclick={onLoadMore}>{error ? 'Retry loading nodes' : 'Load more nodes'}</button>{/if}
  </div>
</section>
<style lang="scss">
  .node-feed {max-width:48rem;margin-inline:auto;min-width:0;}
  ul {list-style:none;padding:0;margin:0;display:grid;gap:1.25rem;}li{min-width:0;}
  .feed-tail {text-align:center;padding:1.5rem .5rem;color:var(--atlas-muted);}
  button {font:inherit;padding:.6rem 1rem;border:1px solid var(--atlas-border);border-radius:var(--atlas-radius);background:var(--atlas-surface);color:var(--atlas-accent);cursor:pointer;}
  .note {font-size:.85rem;color:var(--atlas-muted);}
  .empty-invitation{padding:2rem 1rem;border:1px dashed var(--atlas-border);border-radius:var(--atlas-radius);background:var(--atlas-surface);}
  .empty-invitation h3{margin:0 0 .5rem;color:var(--atlas-text);font-size:1.2rem;}
  .empty-invitation p{margin:.5rem 0 1rem;}
</style>

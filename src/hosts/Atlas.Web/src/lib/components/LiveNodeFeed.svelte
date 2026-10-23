<script lang="ts">
  import LiveVotingDial from './LiveVotingDial.svelte';
  import LiveReactions from './LiveReactions.svelte';
  import { onMount } from 'svelte';
  import NodeFeed, { type FeedItem } from './NodeFeed.svelte';
  import { readDiscovery, discoveryCard, readOlderAncestors } from '../api/discovery';
  import type { DiscoveryFilters } from '../api/discovery';
  import type { NodeCardPresentation } from './NodeCard.svelte';
  let { presentation = 'ribbon-corner', apiBase = '/atlas-preview-api', parentId, typeId, authorId, search = '', filters = {}, emptyTypeName, onCreateFirst, onOpenNode, onOpenProfile }: { presentation?: NodeCardPresentation; apiBase?: string; parentId?: string; typeId?: string; authorId?: string; search?: string; filters?: DiscoveryFilters; emptyTypeName?: string; onCreateFirst?: () => void; onOpenNode?: (id: string, typeId?: string) => void; onOpenProfile?: (id: string) => void } = $props();
  let items = $state<FeedItem[]>([]);
  let loading = $state(false);
  let error = $state('');
  let hasMore = $state(true);
  let nextPage = 1;
  let controller: AbortController | undefined;
  let disposed = false;
  async function load() {
    if (loading || disposed || !hasMore) return;
    loading = true; error = '';
    controller = new AbortController();
    const current = controller;
    const timeout = setTimeout(() => current.abort(), 10000);
    try {
      const page = await readDiscovery(apiBase, nextPage, search, current.signal, undefined, { ...filters, parentId: parentId ?? filters.parentId, typeId: typeId ?? filters.typeId, authorId: authorId ?? filters.authorId });
      if (page.page !== nextPage) throw new Error('Unexpected page');
      const additions: FeedItem[] = [];
      for (const entry of page.items) {
        const node = discoveryCard(entry);
        if (!node || items.some(item => item.node.id === node.id) || additions.some(item => item.node.id === node.id)) continue;
        additions.push({ node, average: entry.averageVote, totalVotes: entry.voteCount, hasMoreAncestors: !!entry.card?.breadcrumbSegment?.continuationNodeId });
      }
      // Publish cards promptly; optional reaction summaries never block pagination.
      if (disposed) return;
      items = [...items, ...additions];
      hasMore = page.hasNextPage;
      nextPage = page.page + 1;

    } catch {
      if (!disposed) error = 'Could not load more nodes. Please retry.';
    } finally {
      clearTimeout(timeout);
      if (!disposed) loading = false;
    }
  }
  // Parent views key the feed when their filters change. Avoid an effect that
  // reads and writes loading/items, which can repeatedly restart requests.
  onMount(() => { void load(); return () => { disposed = true; controller?.abort(); }; });
</script>
<NodeFeed {onOpenProfile} onComments={id => onOpenNode ? onOpenNode(id, 'comments') : window.location.assign(`/nodes?id=${encodeURIComponent(id)}&type=comments#contributions`)} onChildType={(id, childType) => onOpenNode ? onOpenNode(id, childType) : window.location.assign(`/nodes?id=${encodeURIComponent(id)}&type=${encodeURIComponent(childType)}#contributions`)} onReadMore={onOpenNode ?? (id => window.location.assign(`/nodes?id=${encodeURIComponent(id)}`))} onNavigate={onOpenNode} {items} {loading} {hasMore} {error} {presentation} {emptyTypeName} {onCreateFirst} onLoadMore={() => void load()} loadOlderAncestors={id => id ? readOlderAncestors(id, AbortSignal.timeout(10000), apiBase) : Promise.resolve({ items: [], hasMore: false })}>{#snippet renderVoting(item)}<LiveVotingDial nodeId={item.node.id} initialAverage={item.average} initialTotalVotes={item.totalVotes} archived={item.node.status === 'Archived'} />{/snippet}{#snippet renderReactions(item)}<LiveReactions nodeId={item.node.id} archived={item.node.status === 'Archived'} />{/snippet}</NodeFeed>

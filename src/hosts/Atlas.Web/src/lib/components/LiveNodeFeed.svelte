<script lang="ts">
  import { onMount } from 'svelte';
  import NodeFeed, { type FeedItem } from './NodeFeed.svelte';
  import { readDiscovery, discoveryCard, readReactions, readOlderAncestors } from '../api/discovery';
  import type { NodeCardPresentation } from './NodeCard.svelte';
  let { presentation = 'ribbon-corner', apiBase = '/atlas-preview-api', parentId, typeId, onOpenNode }: { presentation?: NodeCardPresentation; apiBase?: string; parentId?: string; typeId?: string; onOpenNode?: (id: string, typeId?: string) => void } = $props();
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
      const page = await readDiscovery(apiBase, nextPage, '', current.signal, undefined, { parentId, typeId });
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
      await Promise.all(additions.map(item =>
        readReactions(item.node.id, current.signal, apiBase).then(reactions => {
          if (!disposed) items = items.map(existing => existing.node.id === item.node.id ? { ...existing, node: { ...existing.node, reactions } } : existing);
        }).catch(() => { if (!disposed) items = items.map(existing => existing.node.id === item.node.id ? { ...existing, reactionError: true } : existing); })
      ));
    } catch {
      if (!disposed) error = 'Could not load more nodes. Please retry.';
    } finally {
      clearTimeout(timeout);
      if (!disposed) loading = false;
    }
  }
  onMount(() => { void load(); return () => { disposed = true; controller?.abort(); }; });
</script>
<NodeFeed onChildType={(id, childType) => onOpenNode ? onOpenNode(id, childType) : window.location.assign(`/nodes?id=${encodeURIComponent(id)}&type=${encodeURIComponent(childType)}#contributions`)} onReadMore={onOpenNode ?? (id => window.location.assign(`/nodes?id=${encodeURIComponent(id)}`))} onNavigate={onOpenNode} {items} {loading} {hasMore} {error} {presentation} onLoadMore={() => void load()} loadOlderAncestors={id => id ? readOlderAncestors(id, AbortSignal.timeout(10000), apiBase) : Promise.resolve({ items: [], hasMore: false })} />

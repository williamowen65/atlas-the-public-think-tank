<!--
  @component
  LiveNodeFeed: Loads discovery pages and composes NodeFeed with live voting/reaction controls.
  Owns loading and page accumulation; NodeFeed owns rendering and requests more pages through a callback.
-->
<script lang="ts">
  import LiveVotingDial from './LiveVotingDial.svelte';
  import LiveReactions from './LiveReactions.svelte';
  import { onMount } from 'svelte';
  import NodeFeed, { type FeedItem } from './NodeFeed.svelte';
  import { readDiscovery, discoveryCard, readOlderAncestors } from '../api/discovery';
  import type { DiscoveryFilters } from '../api/discovery';
  import type { NodeCardPresentation } from './NodeCard.svelte';
  let {
    presentation = 'ribbon-corner',
    apiBase = '/atlas-public-api',
    parentId,
    typeId,
    authorId,
    search = '',
    filters = {},
    emptyTypeName,
    onCreateFirst,
    onOpenNode,
    onOpenProfile,
  }: {
    /** Selects a NodeCardPresentation layout; it does not change the underlying node data. */
    presentation?: NodeCardPresentation;
    /**
     * Public API base for discovery pages and descendant breadcrumb reads. Live voting/reaction
     * wrappers use their own session-aware defaults.
     */
    apiBase?: string;
    /**
     * Restrict discovery to children of this parent node. When omitted, filters.parentId can still
     * supply the scope; otherwise the parent filter is unset.
     */
    parentId?: string;
    /** Node type ID used to restrict the feed. */
    typeId?: string;
    /** Participant ID used to restrict the feed to that author. */
    authorId?: string;
    /** Free-text discovery search, separate from structured filters. */
    search?: string;
    /** Additional DiscoveryFilters for the feed request; see the data-shape reference. */
    filters?: DiscoveryFilters;
    /** Singular contribution type name used in the empty-feed invitation. */
    emptyTypeName?: string;
    /**
     * Callback: Called by the empty-feed invitation to open creation for that contribution type.
     */
    onCreateFirst?: () => void;
    /**
     * Callback requesting navigation to a node ID. Optional typeId selects a contribution tab, or
     * comments for the comment tab. Without a handler, this component uses the app’s node URL.
     */
    onOpenNode?: (id: string, typeId?: string) => void;
    /** Callback: Receives the author/participant ID to open in a profile page. */
    onOpenProfile?: (id: string) => void;
  } = $props();
  /**
   * Accumulated discovery cards from loaded pages; duplicate node IDs are excluded.
   */
  let items = $state<FeedItem[]>([]);
  /**
   * Whether data is being loaded; drives loading feedback for this view.
   */
  let loading = $state(false);
  /**
   * User-facing load or request failure; empty text hides the error message.
   */
  let error = $state('');
  /**
   * Whether discovery reports another page to load.
   */
  let hasMore = $state(true);
  let nextPage = 1;
  let controller: AbortController | undefined;
  let disposed = false;
  // Fetch the next page, discard hidden/unavailable projections, and append each node ID only once.
  async function load() {
    if (loading || disposed || !hasMore) return;
    loading = true;
    error = '';
    controller = new AbortController();
    const current = controller;
    const timeout = setTimeout(() => current.abort(), 10000);
    try {
      const page = await readDiscovery(apiBase, nextPage, search, current.signal, undefined, {
        ...filters,
        parentId: parentId ?? filters.parentId,
        typeId: typeId ?? filters.typeId,
        authorId: authorId ?? filters.authorId,
      });
      if (page.page !== nextPage) throw new Error('Unexpected page');
      const additions: FeedItem[] = [];
      for (const entry of page.items) {
        const node = discoveryCard(entry);
        if (
          !node ||
          items.some((item) => item.node.id === node.id) ||
          additions.some((item) => item.node.id === node.id)
        )
          continue;
        additions.push({
          node,
          average: entry.averageVote,
          totalVotes: entry.voteCount,
          hasMoreAncestors: !!entry.card?.breadcrumbSegment?.continuationNodeId,
        });
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
  onMount(() => {
    void load();
    return () => {
      disposed = true;
      controller?.abort();
    };
  });
</script>

<!--
  Layout: NodeFeed supplies the list layout; per-item voting and reaction snippets inject live
  controls into each card.
-->

<NodeFeed
  {apiBase}
  {onOpenProfile}
  onComments={(id) =>
    onOpenNode
      ? onOpenNode(id, 'comments')
      : window.location.assign(`/nodes?id=${encodeURIComponent(id)}&type=comments#contributions`)}
  onChildType={(id, childType) =>
    onOpenNode
      ? onOpenNode(id, childType)
      : window.location.assign(
          `/nodes?id=${encodeURIComponent(id)}&type=${encodeURIComponent(childType)}#contributions`,
        )}
  onReadMore={onOpenNode ?? ((id) => window.location.assign(`/nodes?id=${encodeURIComponent(id)}`))}
  onNavigate={onOpenNode ?? ((id) => window.location.assign(`/nodes?id=${encodeURIComponent(id)}`))}
  {items}
  {loading}
  {hasMore}
  {error}
  {presentation}
  {emptyTypeName}
  {onCreateFirst}
  onLoadMore={() => void load()}
  loadOlderAncestors={(id) =>
    id
      ? readOlderAncestors(id, AbortSignal.timeout(10000), apiBase)
      : Promise.resolve({ items: [], hasMore: false })}
  >{#snippet renderVoting(item)}<LiveVotingDial
      nodeId={item.node.id}
      initialAverage={item.average}
      initialTotalVotes={item.totalVotes}
      archived={item.node.status === 'Archived'}
    />{/snippet}{#snippet renderReactions(item)}<LiveReactions
      nodeId={item.node.id}
      archived={item.node.status === 'Archived'}
    />{/snippet}</NodeFeed
>

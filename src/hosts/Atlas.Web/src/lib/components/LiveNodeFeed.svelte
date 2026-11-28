<!--
  @component
  LiveNodeFeed: Loads discovery pages and composes NodeFeed with live voting/reaction controls.
  Owns loading and page accumulation; NodeFeed owns rendering and requests more pages through a callback.

  **Props**
  - `presentation` (`NodeCardPresentation`; optional; default `'ribbon-corner'`): Selects a
    NodeCardPresentation layout; it does not change the underlying node data.
  - `apiBase` (`string`; optional; default `'/atlas-public-api'`): Public API base for discovery
    pages and descendant breadcrumb reads. Live voting/reaction wrappers use their own session-aware
    defaults.
  - `parentId` (`string`; optional): Restrict discovery to children of this parent node. When
    omitted, filters.parentId can still supply the scope; otherwise the parent filter is unset.
  - `typeId` (`string`; optional): Node type ID used to restrict the feed.
  - `authorId` (`string`; optional): Participant ID used to restrict the feed to that author.
  - `search` (`string`; optional; default `''`): Free-text discovery search, separate from
    structured filters.
  - `filters` (`DiscoveryFilters`; optional; default `{}`): Additional DiscoveryFilters for the feed
    request; see the data-shape reference.
  - `emptyTypeName` (`string`; optional): Singular contribution type name used in the empty-feed
    invitation.
  - `onCreateFirst` (`() => void`; optional): Callback: Called by the empty-feed invitation to open
    creation for that contribution type.
  - `onOpenNode` (`(id: string, typeId?: string) => void`; optional): Callback requesting navigation
    to a node ID. Optional typeId selects a contribution tab, or comments for the comment tab.
    Without a handler, this component uses the app’s node URL.
  - `onOpenProfile` (`(id: string) => void`; optional): Callback: Receives the author/participant ID
    to open in a profile page.
-->
<script lang="ts">
  import LiveVotingDial from './LiveVotingDial.svelte';
  import { moderatorVisibility } from '../state/moderatorVisibility.svelte';
  import LiveReactions from './LiveReactions.svelte';
  import { onMount, tick } from 'svelte';
  import { page as routePage } from '$app/state';
  import NodeFeed, { type FeedItem } from './NodeFeed.svelte';
  import { DatabaseResumingError, readDiscovery, discoveryCard, readOlderAncestors } from '../api/discovery';
  import type { DiscoveryFilters, DiscoverySubnodePreview } from '../api/discovery';
  import type { NodeCardPresentation } from './NodeCard.svelte';
  let {
    presentation = 'mobile-experiment',
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
    connected = false,
    inline = false,
    initialSubnodes,
  }: {
    /** Selects a NodeCardPresentation layout; it does not change the underlying node data. */
    presentation?: NodeCardPresentation;
    /** Parent-scoped graph rail; not used for general Discover. */
    connected?: boolean;
    /** Embedded relation: no route scroll restore, nested expansion or automatic pagination. */
    inline?: boolean;
    initialSubnodes?: DiscoverySubnodePreview;
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
  let databaseResuming = $state(false);
  let slowLoading = $state(false);
  /**
   * Whether discovery reports another page to load.
   */
  let hasMore = $state(true);
  /** Whether an earlier page exists before the current loaded window. */
  let hasPrevious = $state(false);
  // Moderator inspection uses authenticated navigation only while hidden cards are enabled.
  const breadcrumbBase = $derived(
    moderatorVisibility.canModerate && moderatorVisibility.includeHidden && apiBase === '/atlas-public-api'
      ? '/atlas-session-api'
      : apiBase,
  );
  let nextPage = 1;
  let previousPage = 0;
  let failedDirection: 'initial' | 'next' | 'previous' = 'initial';
  let failedPage = 1;
  let pendingRestoreAnchor: SavedFeedAnchor | undefined;
  let restoreLayoutObserver: ResizeObserver | undefined;
  let restoreLayoutTimeout: ReturnType<typeof setTimeout> | undefined;
  let restoreLayoutStartedAt = 0;
  let restoreLoadHandler: (() => void) | undefined;
  let controller: AbortController | undefined;
  let disposed = false;
  type SavedFeedAnchor = { kind: string; id: string; offset: number; page?: number };
  const savedHistoryKey = '__atlasScrollPosition';

  function captureViewportAnchor() {
    const candidates = [...document.querySelectorAll<HTMLElement>('[data-node-id]')].filter((element) => {
      const rect = element.getBoundingClientRect();
      return rect.bottom > 0 && rect.top < window.innerHeight;
    });
    const element = candidates.sort(
      (a, b) => a.getBoundingClientRect().top - b.getBoundingClientRect().top,
    )[0];
    return element
      ? { id: element.dataset.nodeId ?? '', top: element.getBoundingClientRect().top }
      : undefined;
  }

  function findNodeElement(id: string) {
    return [...document.querySelectorAll<HTMLElement>('[data-node-id]')].find(
      (element) => element.dataset.nodeId === id,
    );
  }

  /** Align the saved card with its saved viewport offset if the loaded window has enough height. */
  function restoreFeedAnchor(anchor: SavedFeedAnchor) {
    const element = findNodeElement(anchor.id);
    if (!element) {
      window.scrollTo(0, 0);
      return true;
    }
    const root = document.scrollingElement ?? document.documentElement;
    const targetY = Math.max(
      0,
      element.getBoundingClientRect().top + window.scrollY - anchor.offset,
    );
    const maxScroll = Math.max(0, root.scrollHeight - window.innerHeight);
    window.scrollTo(0, Math.min(targetY, maxScroll));
    return targetY <= maxScroll;
  }

  function finishFeedAnchorStabilization() {
    restoreLayoutObserver?.disconnect();
    restoreLayoutObserver = undefined;
    if (restoreLoadHandler) document.removeEventListener('load', restoreLoadHandler, true);
    restoreLoadHandler = undefined;
    if (restoreLayoutTimeout) clearTimeout(restoreLayoutTimeout);
    restoreLayoutTimeout = undefined;
    window.removeEventListener('wheel', cancelFeedAnchorStabilization);
    window.removeEventListener('touchstart', cancelFeedAnchorStabilization);
    window.removeEventListener('pointerdown', cancelFeedAnchorStabilization);
    window.removeEventListener('keydown', cancelFeedAnchorStabilization);
  }

  function cancelFeedAnchorStabilization() {
    finishFeedAnchorStabilization();
  }

  /** Keep the saved item at its recorded offset while responsive/lazy media settles. */
  function stabilizeFeedAnchor(anchor: SavedFeedAnchor) {
    finishFeedAnchorStabilization();
    restoreLayoutStartedAt = Date.now();
    const correctPosition = () => {
      if (Date.now() - restoreLayoutStartedAt >= 5000) {
        finishFeedAnchorStabilization();
        return;
      }
      const element = findNodeElement(anchor.id);
      if (!element) return;
      const delta = element.getBoundingClientRect().top - anchor.offset;
      if (Math.abs(delta) > 1) window.scrollBy(0, delta);
      if (restoreLayoutTimeout) clearTimeout(restoreLayoutTimeout);
      restoreLayoutTimeout = setTimeout(finishFeedAnchorStabilization, 800);
    };
    restoreLayoutObserver = new ResizeObserver(() => requestAnimationFrame(correctPosition));
    restoreLayoutObserver.observe(document.body);
    window.addEventListener('wheel', cancelFeedAnchorStabilization, { passive: true });
    window.addEventListener('touchstart', cancelFeedAnchorStabilization, { passive: true });
    window.addEventListener('pointerdown', cancelFeedAnchorStabilization, { passive: true });
    window.addEventListener('keydown', cancelFeedAnchorStabilization);
    // Lazy images can load without a timely body resize notification on some mobile browsers.
    restoreLoadHandler = correctPosition;
    document.addEventListener('load', restoreLoadHandler, true);
    restoreLayoutTimeout = setTimeout(finishFeedAnchorStabilization, 800);
  }

  // Page numbers address any ranked slice directly; adjacent slices are fetched in either
  // direction and the API remains the source of truth for each page.
  async function load(direction: 'initial' | 'next' | 'previous' = 'next', requestedPage?: number, restoreAnchor?: SavedFeedAnchor) {
    if (loading || disposed) return;
    if (direction === 'next' && !hasMore) return;
    if (direction === 'previous' && !hasPrevious) return;
    let pageNumber = requestedPage ?? (direction === 'previous' ? previousPage : nextPage);
    if (!Number.isInteger(pageNumber) || pageNumber < 1) return;
    const viewportAnchor = direction === 'previous' ? captureViewportAnchor() : undefined;
    loading = true;
    error = '';
    databaseResuming = false;
    slowLoading = false;
    controller = new AbortController();
    const current = controller;
    // Pending requests may succeed without ever returning a SQL resume error.
    // Explain the delay without claiming that its cause is known.
    const slowNotice = setTimeout(() => {
      if (!disposed && current === controller) slowLoading = true;
    }, 4000);
    const timeout = setTimeout(() => current.abort(), 60000);
    try {
      const moderatorRead = moderatorVisibility.canModerate && moderatorVisibility.includeHidden && apiBase === '/atlas-public-api';
      let page = await readDiscovery(apiBase, pageNumber, search, current.signal, undefined, {
        ...filters,
        parentId: parentId ?? filters.parentId,
        typeId: typeId ?? filters.typeId,
        authorId: authorId ?? filters.authorId,
      }, false, moderatorRead);
      // A saved page can fall beyond the current end after deletions or filtering changes.
      // Move to the latest available page rather than restoring into an empty feed.
      if (direction === 'initial' && !page.items.length && page.totalCount > 0) {
        const lastPage = Math.max(1, Math.ceil(page.totalCount / page.pageSize));
        if (lastPage !== pageNumber) {
          pageNumber = lastPage;
          page = await readDiscovery(apiBase, pageNumber, search, current.signal, undefined, {
            ...filters,
            parentId: parentId ?? filters.parentId,
            typeId: typeId ?? filters.typeId,
            authorId: authorId ?? filters.authorId,
          }, false, moderatorRead);
        }
      }
      if (page.page !== pageNumber) throw new Error('Unexpected page');
      const additions: FeedItem[] = [];
      for (const entry of page.items) {
        const node = discoveryCard(entry, moderatorRead);
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
          page: page.page,
        });
      }
      // Publish cards promptly; optional reaction summaries never block pagination.
      if (disposed || current !== controller) return;
      if (direction === 'initial') {
        items = additions;
        previousPage = page.page - 1;
        nextPage = page.page + 1;
        hasPrevious = page.page > 1;
        hasMore = page.hasNextPage;
        await tick();
        if (restoreAnchor?.kind === 'node') {
          const restored = restoreFeedAnchor(restoreAnchor);
          pendingRestoreAnchor = !restored && page.hasNextPage ? restoreAnchor : undefined;
          if (restored) stabilizeFeedAnchor(restoreAnchor);
        }
      } else if (direction === 'previous') {
        items = [...additions, ...items];
        previousPage = page.page - 1;
        hasPrevious = page.page > 1;
        await tick();
        if (viewportAnchor) {
          const anchor = findNodeElement(viewportAnchor.id);
          if (anchor) window.scrollBy(0, anchor.getBoundingClientRect().top - viewportAnchor.top);
        }
      } else {
        items = [...items, ...additions];
        nextPage = page.page + 1;
        hasMore = page.hasNextPage;
        if (pendingRestoreAnchor) {
          await tick();
          const restored = restoreFeedAnchor(pendingRestoreAnchor);
          if (restored || !page.hasNextPage) pendingRestoreAnchor = undefined;
        }
      }
    } catch (cause) {
      if (!disposed && current === controller) {
        failedDirection = direction;
        failedPage = pageNumber;
        databaseResuming = cause instanceof DatabaseResumingError;
        error = cause instanceof DatabaseResumingError
          ? cause.message
          : current.signal.aborted
            ? 'Loading nodes timed out. The server or database may still be waking up. Please retry.'
            : 'Could not load nodes. Please retry.';
      }
    } finally {
      clearTimeout(slowNotice);
      clearTimeout(timeout);
      if (!disposed && current === controller) {
        loading = false;
        slowLoading = false;
        // If the saved offset was beyond the scrollable height of the first page, fetch only
        // enough adjacent pages below it to make that offset reachable before finishing restore.
        if (pendingRestoreAnchor && hasMore && !error) void load('next');
      }
    }
  }
  // Parent views key the feed when their filters change. Avoid an effect that
  // reads and writes loading/items, which can repeatedly restart requests.
  onMount(() => {
    const historyState = routePage.state as Record<string, unknown>;
    type SavedPosition = { anchor?: SavedFeedAnchor; route?: string };
    let saved = inline ? undefined : historyState[savedHistoryKey] as SavedPosition | undefined;
    const navigation = performance.getEntriesByType('navigation')[0] as PerformanceNavigationTiming | undefined;
    if (!inline && !saved && routePage.url.pathname === '/' && navigation?.type === 'back_forward') {
      try {
        saved = JSON.parse(sessionStorage.getItem(savedHistoryKey) ?? 'null') as SavedPosition | undefined;
      } catch { /* Fall back to the first page if storage is unavailable. */ }
    }
    const savedAnchor = saved?.anchor;
    const sameRoute = !saved?.route || saved.route === routePage.url.pathname + routePage.url.search;
    const restorePage = sameRoute && savedAnchor?.kind === 'node' && Number.isInteger(savedAnchor.page) && savedAnchor.page! > 0
      ? savedAnchor.page!
      : 1;
    const anchor = sameRoute && savedAnchor?.kind === 'node' ? savedAnchor : undefined;
    nextPage = restorePage;
    previousPage = restorePage - 1;
    hasPrevious = restorePage > 1;
    if (inline && !typeId && initialSubnodes) {
      items = initialSubnodes.items.flatMap(entry => {
        const node = discoveryCard(entry);
        return node ? [{ node, average: entry.averageVote, totalVotes: entry.voteCount, page: 1 }] : [];
      });
      nextPage = 2;
      hasMore = initialSubnodes.hasNextPage;
    } else {
      void load('initial', restorePage, anchor);
    }
    const refresh = () => {
      controller?.abort();
      controller = undefined;
      loading = false;
      items = [];
      hasMore = true;
      hasPrevious = false;
      nextPage = 1;
      previousPage = 0;
      void load('initial', 1);
    };
    window.addEventListener('atlas-moderator-visibility-changed', refresh);
    return () => {
      disposed = true;
      finishFeedAnchorStabilization();
      controller?.abort();
      window.removeEventListener('atlas-moderator-visibility-changed', refresh);
    };
  });
</script>

<!--
  Layout: NodeFeed supplies the list layout; per-item voting and reaction snippets inject live
  controls into each card.
-->

<NodeFeed
  apiBase={breadcrumbBase}
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
  loadingNotice={slowLoading ? 'Loading is taking longer than usual. The server or database may be waking up. Please keep this page open.' : ''}
  {hasMore}
  {hasPrevious}
  {error}
  errorRole={databaseResuming ? 'status' : 'alert'}
  {presentation}
  {connected}
  {inline}
  inlineRelationships={!inline}
  {emptyTypeName}
  {onCreateFirst}
  onLoadMore={() => void load('next')}
  onLoadPrevious={() => void load('previous')}
  onRetry={() => void load(failedDirection, failedPage)}
  loadOlderAncestors={(id) =>
    id
      ? readOlderAncestors(id, AbortSignal.timeout(10000), breadcrumbBase)
      : Promise.resolve({ items: [], hasMore: false })}
  >{#snippet renderVoting(item, compact)}<LiveVotingDial
      forceCompact={compact}
      mobileCompact={presentation === 'mobile-experiment'}
      nodeId={item.node.id}
      initialAverage={item.average}
      initialTotalVotes={item.totalVotes}
      archived={item.node.status === 'Archived' || item.node.isHidden === true}
    />{/snippet}{#snippet renderReactions(item)}<LiveReactions
      nodeId={item.node.id}
      onTotalChange={(count) => {
        const id = item.node.id;
        items = items.map(current => current.node.id === id && current.node.reactionCount !== count
          ? { ...current, node: { ...current.node, reactionCount: count } } : current);
      }}
      archived={item.node.status === 'Archived' || item.node.isHidden === true}
    />{/snippet}</NodeFeed
>

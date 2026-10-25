<!--
  @component
  Compact global discovery results: linked titles, short introductions and discussion counts.
  Fetches anonymous discovery pages and cancels outdated searches; read-only importance totals without a voting dial or breadcrumb.

  **Props**
  - `search` (`string`; required): Accepted free-text search, separate from structured filters.
  - `filters` (`DiscoveryFilters`; required): Validated accepted discovery constraints.
  - `onSelect` (`() => void`; optional): Close the containing disclosure after opening a result.
-->
<script lang="ts">
  import {
    SearchUnavailableError,
    readDiscovery,
    discoveryCard,
    type DiscoveryFilters,
  } from '../api/discovery';
  import type { NodeCardData } from './node-card-versions/types';
  let {
    search,
    filters,
    onSelect,
  }: {
    /** Accepted free-text query; pending typing does not trigger requests. */
    search: string;
    /** Parsed filter constraints supplied by the shared query parser. */
    filters: DiscoveryFilters;
    /** Notify the containing search disclosure when a result is opened normally. */
    onSelect?: () => void;
  } = $props();
  /** Search-only projection retaining authoritative discovery aggregates without extra requests. */
  type SearchResult = NodeCardData & {
    /** Backend-calculated average; null represents an unrated node. */
    comment?: {
      nodeId: string;
      id: string;
      nodeTitle: string;
      authorId: string;
      authorName: string;
      createdAt: string;
      excerpt: string;
      score: number;
    } | null;
    average: number | null;
    /** Number of ratings included in the backend aggregate. */
    totalVotes: number;
  };
  /** Adapt visible discovery cards while keeping their existing voting aggregates. */
  function resultCard(item: Parameters<typeof discoveryCard>[0]): SearchResult | null {
    if (item.comment)
      return {
        id: 'comment:' + item.comment.id,
        title: item.comment.nodeTitle,
        description: item.comment.excerpt,
        typeName: 'Comment',
        authorName: item.comment.authorName,
        createdAt: item.comment.createdAt,
        status: 'Active',
        breadcrumb: [],
        communities: [],
        related: [],
        requestedTypes: [],
        reactions: [],
        childTypes: [],
        commentCount: 0,
        comment: { ...item.comment, nodeId: item.nodeId },
        average: null,
        totalVotes: 0,
      };
    const card = discoveryCard(item);
    return card ? { ...card, average: item.averageVote, totalVotes: item.voteCount } : null;
  }
  /** Compact display models accumulated from successful result pages. */
  let items = $state<SearchResult[]>([]);
  /** Whether the current page request is pending. */
  let loading = $state(true);
  /** Visible request error; existing results remain available on paging failure. */
  let error = $state('');
  /** Matching record count reported by the API. */
  let total = $state(0);
  /** Whether the API offers another result page. */
  let hasMore = $state(false);
  /** Next page number; advances only after a successful response. */
  let nextPage = $state(1);
  /** Sequence changed by Retry to repeat an initial failed search. */
  let retry = $state(0);
  let controller: AbortController;
  /** Scroll container used as the observer root, rather than the whole browser viewport. */
  let resultsElement = $state<HTMLElement>();

  /**
   * Svelte action: observe the trailing marker inside this dropdown and request the next page
   * shortly before it reaches view. Conditional mounting pauses observation while loading or
   * after an error; destroying the marker disconnects its browser observer.
   */
  function observeNextPage(marker: HTMLElement) {
    const observer = new IntersectionObserver(
      (entries) => {
        if (entries.some((entry) => entry.isIntersecting)) void loadMore();
      },
      { root: resultsElement, rootMargin: '0px 0px 80px 0px' },
    );
    observer.observe(marker);
    return { destroy: () => observer.disconnect() };
  }

  // An accepted query change resets results and cancels the previous request. The
  // async continuation is not a reactive dependency and cannot publish after abort.
  $effect(() => {
    const query = search;
    const constraints = filters;
    retry;
    const current = new AbortController();
    controller = current;
    items = [];
    total = 0;
    nextPage = 1;
    hasMore = false;
    error = '';
    loading = true;
    void readDiscovery(
      '/atlas-public-api',
      1,
      query,
      AbortSignal.any([current.signal, AbortSignal.timeout(10000)]),
      undefined,
      constraints,
      true,
    )
      .then((page) => {
        if (current.signal.aborted) return;
        items = page.items.map(resultCard).filter((item): item is SearchResult => item !== null);
        total = page.totalCount;
        hasMore = page.hasNextPage;
        nextPage = 2;
      })
      .catch((failure: unknown) => {
        if (!current.signal.aborted)
          error =
            failure instanceof SearchUnavailableError
              ? failure.message
              : 'Search results could not be loaded.';
      })
      .finally(() => {
        if (!current.signal.aborted) loading = false;
      });
    return () => current.abort();
  });

  /** Append the next API page; failures keep existing results and the same retry cursor. */
  async function loadMore() {
    if (loading || !hasMore) return;
    const current = controller;
    loading = true;
    error = '';
    try {
      const page = await readDiscovery(
        '/atlas-public-api',
        nextPage,
        search,
        AbortSignal.any([current.signal, AbortSignal.timeout(10000)]),
        undefined,
        filters,
        true,
      );
      if (current.signal.aborted) return;
      const added = page.items
        .map(resultCard)
        .filter(
          (item): item is SearchResult =>
            !!item && !items.some((existing) => existing.id === item.id),
        );
      items = [...items, ...added];
      total = page.totalCount;
      hasMore = page.hasNextPage;
      nextPage++;
    } catch (failure: unknown) {
      if (!current.signal.aborted)
        error =
          failure instanceof SearchUnavailableError
            ? failure.message
            : 'More results could not be loaded.';
    } finally {
      if (!current.signal.aborted) loading = false;
    }
  }
</script>

<!-- Layout: A bounded, scrollable result dropdown overlays the current page; links use normal app navigation. -->
<section
  class="search-results"
  aria-label="Search results"
  aria-busy={loading}
  bind:this={resultsElement}
>
  <h2>Search results</h2>
  {#if items.length}<p class="summary">{items.length} shown · {total} matches</p>{/if}
  <ul>
    {#each items as node (node.id)}
      <li>
        {#if node.comment}
          <div class="importance" aria-label={`Comment score: ${node.comment.score ?? 0}`}>
            <strong>{node.comment.score ?? 0}</strong><span>score</span>
          </div>
          <div class="result-content comment-result">
            <a
              class="result-title"
              href={`/nodes?id=${encodeURIComponent(node.comment.nodeId)}&type=comments&comment=${encodeURIComponent(node.comment.id)}`}
              onclick={(event) => {
                if (!event.ctrlKey && !event.metaKey && !event.shiftKey && !event.altKey)
                  onSelect?.();
              }}>Comment on {node.title}</a
            >
            <p class="metadata">
              {node.comment.authorName} ·
              <time datetime={node.comment.createdAt}
                >{new Date(node.comment.createdAt).toLocaleDateString()}</time
              >
            </p>
            <p class="intro">{node.comment.excerpt}</p>
          </div>
        {:else}
          <!-- Read-only totals use discovery data; they never mount voting controls or fetch per result. -->
          <div
            class="importance"
            aria-label={`Importance: ${node.average === null ? 'not rated' : node.average.toFixed(1) + ' out of 10'}, ${node.totalVotes} votes`}
          >
            <strong>{node.average === null ? '—' : node.average.toFixed(1)}</strong>
            <span class="voters"
              ><svg
                aria-hidden="true"
                viewBox="0 0 24 24"
                width="16"
                height="16"
                fill="none"
                stroke="currentColor"
                stroke-width="1.6"
                ><circle cx="9" cy="7" r="3" /><path
                  d="M3 20v-3a6 6 0 0 1 12 0v3M16 4a3 3 0 0 1 0 6M18 13a5 5 0 0 1 3 4v3"
                /></svg
              >{node.totalVotes}</span
            >
          </div>
          <div class="result-content">
            <a
              class="result-title"
              href={`/nodes?id=${encodeURIComponent(node.id)}`}
              onclick={(event) => {
                if (!event.ctrlKey && !event.metaKey && !event.shiftKey && !event.altKey)
                  onSelect?.();
              }}><span class="node-type">{node.typeName}:</span> {node.title}</a
            >
            <p class="intro">{node.description}</p>
            <p class="metadata">
              <span>{node.commentCount} comments</span>
              {#each node.childTypes ?? [] as type}
                <span>{type.label}</span>
              {/each}
            </p>
          </div>
        {/if}
      </li>
    {/each}
  </ul>
  {#if loading}<p role="status">Loading search results…</p>
  {:else if !error && !items.length}<p role="status">No results found.</p>{/if}
  {#if error}<p role="alert">{error}</p>
    <button
      type="button"
      onclick={() => {
        if (items.length) void loadMore();
        else retry++;
      }}>Retry search</button
    >
  {/if}
  <!-- Remount after each successful page so a short list fills the dropdown automatically.
       Errors leave an explicit Retry control instead of repeatedly retrying in the background. -->
  {#if hasMore && !loading && !error}
    <div class="next-page-marker" aria-hidden="true" use:observeNextPage></div>
  {/if}
</section>

<style>
  .search-results {
    position: absolute;
    inset-inline: 0;
    top: 0.4rem;
    z-index: 40;
    max-height: min(55svh, 32rem);
    overflow-y: auto;
    overscroll-behavior: contain;
    padding: 1rem;
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    background: var(--atlas-surface);
    box-shadow: 0 8px 24px #0002;
  }
  h2 {
    font-size: 1rem;
    margin: 0 0 0.5rem;
  }
  ul {
    list-style: none;
    padding: 0;
    margin: 0;
  }
  li {
    display: grid;
    grid-template-columns: 2.6rem minmax(0, 1fr);
    gap: 0.7rem;
    padding: 0.7rem 0;
    border-top: 1px solid var(--atlas-border);
  }
  .importance {
    align-self: center;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 0.2rem;
    color: var(--atlas-muted);
    font-size: 0.8rem;
  }
  .importance strong {
    font-size: 1rem;
    color: var(--atlas-text);
  }
  .voters {
    display: inline-flex;
    align-items: center;
    gap: 0.2rem;
  }
  .result-content {
    min-width: 0;
    overflow-wrap: anywhere;
  }
  .metadata {
    display: flex;
    flex-wrap: wrap;
    gap: 0.25rem 0.6rem;
  }
  .metadata span + span::before {
    content: '·';
    margin-inline-end: 0.6rem;
  }
  .result-title {
    font-weight: 700;
  }
  p {
    margin: 0.3rem 0;
  }
  .intro {
    display: -webkit-box;
    -webkit-line-clamp: 2;
    line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
  }
  .metadata,
  .summary {
    color: var(--atlas-muted);
    font-size: 0.8rem;
  }
  .next-page-marker {
    height: 1px;
  }
  button {
    font: inherit;
    padding: 0.5rem;
    color: var(--atlas-text);
    background: var(--atlas-surface);
    border: 1px solid var(--atlas-border);
    border-radius: 0.4rem;
    cursor: pointer;
  }
</style>

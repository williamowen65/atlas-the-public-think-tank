<!--
  @component
  Compact global discovery results: linked titles, short introductions and discussion counts.
  Fetches anonymous discovery pages and cancels outdated searches; no voting or breadcrumb UI.

  **Props**
  - `search` (`string`; required): Accepted free-text search, separate from structured filters.
  - `filters` (`DiscoveryFilters`; required): Validated accepted discovery constraints.
  - `onSelect` (`() => void`; optional): Close the containing disclosure after opening a result.
-->
<script lang="ts">
  import { readDiscovery, discoveryCard, type DiscoveryFilters } from '../api/discovery';
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
  /** Compact display models accumulated from successful result pages. */
  let items = $state<NodeCardData[]>([]);
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
    )
      .then((page) => {
        if (current.signal.aborted) return;
        items = page.items.map(discoveryCard).filter((item): item is NodeCardData => item !== null);
        total = page.totalCount;
        hasMore = page.hasNextPage;
        nextPage = 2;
      })
      .catch(() => {
        if (!current.signal.aborted) error = 'Search results could not be loaded.';
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
      );
      if (current.signal.aborted) return;
      const added = page.items
        .map(discoveryCard)
        .filter(
          (item): item is NodeCardData =>
            !!item && !items.some((existing) => existing.id === item.id),
        );
      items = [...items, ...added];
      total = page.totalCount;
      hasMore = page.hasNextPage;
      nextPage++;
    } catch {
      if (!current.signal.aborted) error = 'More results could not be loaded.';
    } finally {
      if (!current.signal.aborted) loading = false;
    }
  }
</script>

<!-- Layout: A bounded, scrollable result dropdown overlays the current page; links use normal app navigation. -->
<section class="search-results" aria-label="Search results" aria-busy={loading}>
  <h2>Search results</h2>
  {#if items.length}<p class="summary">{items.length} shown · {total} matches</p>{/if}
  <ul>
    {#each items as node (node.id)}
      <li>
        <a
          class="result-title"
          href={`/nodes?id=${encodeURIComponent(node.id)}`}
          onclick={(event) => {
            if (!event.ctrlKey && !event.metaKey && !event.shiftKey && !event.altKey) onSelect?.();
          }}>{node.title}</a
        >
        <p class="intro">{node.description}</p>
        <p class="metadata">
          {node.typeName} · {node.commentCount} comments{#if node.childTypes?.length}
            · {node.childTypes.map((type) => type.label).join(' · ')}{/if}
        </p>
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
  {:else if hasMore}<button type="button" disabled={loading} onclick={() => void loadMore()}
      >Load more results</button
    >{/if}
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
    padding: 0.7rem 0;
    border-top: 1px solid var(--atlas-border);
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

<!--
  @component
  DiscoverySearch: Shared discovery search and filter toolbox; communicates through discoveryQuery state.
  Parses search tokens and publishes the accepted query to shared discovery state.

  **Props**
  - `onResultSelected` (`() => void`; optional): Notify the parent after selecting a search result.
  - `onExpandedChange` (`(expanded: boolean) => void`; optional): Callback: Reports whether the
    search filter toolbox is expanded, so the header can adjust its layout.
-->
<script lang="ts">
  import { onMount } from 'svelte';
  import VotingWeightControl from './VotingWeightControl.svelte';
  import { searchPreferences, watchSearchPreferences } from '../state/searchPreferences.svelte';
  import {
    discoveryQuery,
    setDiscoveryQuery,
    restoreDiscoveryQuery,
  } from '$lib/state/discoveryQuery.svelte';
  import { readTypes, type NodeType } from '$lib/api/nodePage';
  import { parseDiscoveryQuery } from '../state/parseDiscoveryQuery';
  import SearchResults from './SearchResults.svelte';
  let {
    active = true,
    onActivate,
    placeholder = 'Search nodes and comments, or try type:Idea',
    onExpandedChange,
    onResultSelected,
  }: {
    /** Whether global result overlays should be shown for the current header interaction. */
    active?: boolean;
    /** Notify the header when its real search field receives focus. */
    onActivate?: () => void;
    /** Placeholder supplied by the containing header; search behavior stays shared. */
    placeholder?: string;
    /**
     * Callback: Reports whether the search filter toolbox is expanded, so the header can adjust
     * its layout.
     */
    onExpandedChange?: (expanded: boolean) => void;
    /** Notify the parent to close search after normal result navigation. */
    onResultSelected?: () => void;
  } = $props();
  /**
   * Available non-archived node types used by labels, filters or creation choices.
   */
  let types = $state<NodeType[]>([]);
  /** Wait for the type catalog before interpreting restored type-name filters. */
  let typesReady = $state(false);
  /** Accepted query used by results; pending input/toolbox edits do not change it. */
  const accepted = $derived(parse(discoveryQuery.value));
  /**
   * Whether the visual filter toolbox is open.
   */
  let expanded = $state(false);
  let searchRegion: HTMLElement;
  function dismissOutside(event: PointerEvent) {
    if (expanded && event.target instanceof Node && !searchRegion?.contains(event.target))
      expanded = false;
  }
  function dismissEscape(event: KeyboardEvent) {
    if (event.key === 'Escape') expanded = false;
  }
  $effect(() => onExpandedChange?.(expanded));
  /**
   * Editable query in the search input; may differ from the last accepted query.
   */
  let input = $state('');
  /**
   * Last accepted query reflected by the shared discovery state.
   */
  let applied = $state('');
  /**
   * Query being edited by the visual toolbox; Apply commits it to shared state.
   */
  let pending = $state('');
  /**
   * Parsed pending query, including filter values and validation errors for the toolbox.
   */
  const editing = $derived(parse(pending));
  onMount(() => {
    restoreDiscoveryQuery();
    return watchSearchPreferences();
  });
  /** Use one parser for pending edits and accepted global search requests. */
  function parse(value: string) {
    return parseDiscoveryQuery(value, types);
  }
  // Publish only valid queries; global header results consume the accepted search independently of the browsing feed.
  function apply(value: string) {
    const result = parse(value);
    if (result.errors.length) {
      input = value;
      return;
    }
    setDiscoveryQuery(value);
    applied = discoveryQuery.value;
    input = applied;
    pending = applied;
  }
  /** Mounted search input; receives focus again after clearing text and all filters. */
  let queryInput = $state<HTMLInputElement>();
  /** Clear accepted query and pending edits together; an empty query also removes saved storage. */
  function clearSearch() {
    apply('');
    queryInput?.focus({ preventScroll: true });
  }
  // Replace one field token without dropping unrelated terms in the draft query.
  function token(key: string, value: string) {
    const parts = (pending.match(/(?:[^\s"]+|"[^"]*")+/g) ?? []).filter(
      (p) => !p.toLowerCase().startsWith(key + ':'),
    );
    if (value) parts.push(key + ':' + (value.includes(' ') ? '"' + value + '"' : value));
    pending = parts.join(' ');
    input = pending;
  }
  $effect(() => {
    const q = discoveryQuery.value;
    if (q !== applied) {
      applied = q;
      input = q;
      pending = q;
    }
  });
  $effect(() => {
    const controller = new AbortController();
    void readTypes('/atlas-public-api', controller.signal)
      .then((t) => (types = t))
      .catch(() => {})
      .finally(() => {
        if (!controller.signal.aborted) typesReady = true;
      });
    return () => controller.abort();
  });
</script>

<!--
  Layout: The search row stays visible; an expandable toolbox edits filter tokens before Apply
  publishes them to shared discovery state.
-->

<svelte:window onpointerdown={dismissOutside} onkeydown={dismissEscape} />
<section bind:this={searchRegion} class="filter-bar" aria-label="Filter discovery" onfocusin={() => onActivate?.()}>
  <form
    class="search-line"
    onsubmit={(event) => {
      event.preventDefault();
      apply(input);
    }}
  >
    <label class="sr-only" for="discovery-query">Search nodes and filters</label>
    <!-- The clear button overlays the input area, rather than adding another pill segment. -->
    <div class="query-field" class:has-query={!!input}>
      <input
        id="discovery-query"
        bind:this={queryInput}
        bind:value={input}
        {placeholder}
      />
      {#if input || pending || applied}
        <button
          class="clear-search"
          type="button"
          aria-label="Clear search and filters"
          title="Clear search and filters"
          onclick={clearSearch}
        >
          <svg
            aria-hidden="true"
            viewBox="0 0 24 24"
            width="16"
            height="16"
            fill="none"
            stroke="currentColor"
            stroke-width="2"
            stroke-linecap="round"><path d="m6 6 12 12M18 6 6 18" /></svg
          >
        </button>
      {/if}
    </div>
    <!-- Icon segments share the input’s pill; accessible names keep their purpose explicit. -->
    <button class="search-action" type="submit" aria-label="Search" title="Search">
      <svg
        aria-hidden="true"
        viewBox="0 0 24 24"
        width="20"
        height="20"
        fill="none"
        stroke="currentColor"
        stroke-width="2"
        stroke-linecap="round"
      >
        <circle cx="10.5" cy="10.5" r="6.5" />
        <path d="m16 16 4.5 4.5" />
      </svg>
    </button>
    <button
      class="search-action filter-toggle"
      type="button"
      aria-label="Filters"
      title="Filters"
      aria-expanded={expanded}
      onclick={() => (expanded = !expanded)}
    >
      <svg
        aria-hidden="true"
        viewBox="0 0 24 24"
        width="20"
        height="20"
        fill="none"
        stroke="currentColor"
        stroke-width="2"
        stroke-linecap="round"
        stroke-linejoin="round"
      >
        <path d="M4 5h16l-6 7v6l-4 2v-8Z" />
      </svg>
    </button>
  </form>
  {#if parse(input).errors.length}<p class="validation" role="alert">
      {parse(input).errors.join(' · ')}
    </p>{/if}
  {#if expanded}
    <!--
      These controls edit pending tokens; Apply commits them, while the active feed keeps its
      previous query.
    -->
    <div class="toolbox">
      <VotingWeightControl />
      <label
        >Search in<select
          value={editing.filters.scope ?? 'all'}
          onchange={(e) =>
            token('in', e.currentTarget.value === 'all' ? '' : e.currentTarget.value)}
        >
          <option value="all">Nodes and comments</option><option value="nodes">Nodes</option><option
            value="comments">Comments</option
          >
        </select></label
      >
      <label
        >Node type<select
          value={editing.filters.typeId ?? ''}
          onchange={(e) =>
            token('type', types.find((type) => type.id === e.currentTarget.value)?.name ?? '')}
          ><option value="">All types</option>{#each types as type}<option value={type.id}
              >{type.name}</option
            >{/each}</select
        ></label
      >
      <label
        >Minimum votes<input
          type="number"
          min="0"
          value={editing.filters.minVotes ?? ''}
          onchange={(e) =>
            token(
              'votes',
              e.currentTarget.value || editing.filters.maxVotes !== undefined
                ? `${e.currentTarget.value}..${editing.filters.maxVotes ?? ''}`
                : '',
            )}
        /></label
      >
      <label
        >Maximum votes<input
          type="number"
          min="0"
          value={editing.filters.maxVotes ?? ''}
          onchange={(e) =>
            token(
              'votes',
              e.currentTarget.value || editing.filters.minVotes !== undefined
                ? `${editing.filters.minVotes ?? ''}..${e.currentTarget.value}`
                : '',
            )}
        /></label
      >
      <label
        >Minimum average (0–10)<input
          type="number"
          min="0"
          max="10"
          step=".1"
          value={editing.filters.minAverage ?? ''}
          onchange={(e) =>
            token(
              'average',
              e.currentTarget.value || editing.filters.maxAverage !== undefined
                ? `${e.currentTarget.value}..${editing.filters.maxAverage ?? ''}`
                : '',
            )}
        /></label
      >
      <label
        >Maximum average (0–10)<input
          type="number"
          min="0"
          max="10"
          step=".1"
          value={editing.filters.maxAverage ?? ''}
          onchange={(e) =>
            token(
              'average',
              e.currentTarget.value || editing.filters.minAverage !== undefined
                ? `${editing.filters.minAverage ?? ''}..${e.currentTarget.value}`
                : '',
            )}
        /></label
      >
      <label
        >Created after<input
          type="date"
          value={editing.filters.from ?? ''}
          onchange={(e) => token('from', e.currentTarget.value)}
        /></label
      >
      <label
        >Created before<input
          type="date"
          value={editing.filters.through ?? ''}
          onchange={(e) => token('through', e.currentTarget.value)}
        /></label
      >
      <label class="check"
        ><input
          type="checkbox"
          checked={editing.filters.archived ?? false}
          onchange={(e) => token('archived', e.currentTarget.checked ? 'true' : '')}
        /> Include archived nodes</label
      >
      <div class="apply-row">
        <button
          type="button"
          onclick={() => {
            clearSearch();
            expanded = false;
          }}>Clear filters</button
        ><button
          type="button"
          onclick={() => {
            if (!editing.errors.length) {
              apply(pending);
              expanded = false;
            }
          }}
          disabled={editing.errors.length > 0}>Apply filters</button
        >{#if pending !== applied}<span>Changes not yet applied</span>{/if}
      </div>
      {#if editing.errors.length}<p role="alert" class="validation">
          {editing.errors.join(' · ')}
        </p>{/if}
      <p class="hint">
        Visual filters update the search syntax above. You can also type filters directly, such as <code
          >type:Idea</code
        >, <code>votes:5..100</code>, <code>average:7..10</code>, <code>from:2026-01-01</code>,
        <code>author:ID</code>, or <code>community:ID</code>. Use <code>in:comments</code> or
        <code>in:nodes</code> to narrow search. Author and date filters apply to the result itself; type
        and community filters apply to its node.
      </p>
    </div>
  {/if}
  <!-- Results belong to the global search, not to the page currently behind the header. -->
  {#if active && !expanded && discoveryQuery.value.trim()}
    <div class="results-slot">
      {#if !typesReady}<p role="status">Preparing search…</p>
      {:else if accepted.errors.length}<p role="alert">{accepted.errors.join(' · ')}</p>
      {:else}<SearchResults
          search={accepted.text}
          filters={{ ...accepted.filters, votingWeight: searchPreferences.votingWeight }}
          onSelect={onResultSelected}
        />{/if}
    </div>
  {/if}
</section>

<style>
  .results-slot {
    position: relative;
  }
  p {
    color: var(--atlas-muted);
  }
  .filter-bar {
    position: sticky;
    top: 0;
    z-index: 12;
    max-width: 48rem;
    margin: 0 auto 1.25rem;
    padding: 0.75rem;
    background: var(--atlas-surface);
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    box-sizing: border-box;
  }
  /* One shrinkable input and two fixed icon segments, with a shared pill outline. */
  .search-line {
    display: flex;
    align-items: stretch;
    gap: 0;
    min-width: 0;
    border: 1px solid var(--atlas-border);
    border-radius: 999px;
    background: var(--atlas-surface);
  }
  .query-field {
    position: relative;
    display: flex;
    flex: 1 1 0;
    min-width: 0;
  }
  .search-line input {
    width: 100%;
    min-width: 0;
    border: 0;
    border-radius: 999px 0 0 999px;
    /* Reserve the overlaid clear target even when empty, so typed text never shifts. */
    padding-inline: 0.9rem 2.75rem;
    background: transparent;
  }
  .query-field .clear-search {
    position: absolute;
    inset-inline-end: 0;
    inset-block: 0;
    width: 2.75rem;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    padding: 0;
    border: 0;
    border-radius: 0;
    background: transparent;
  }
  .clear-search:hover {
    color: var(--atlas-accent);
  }
  .search-line .search-action {
    display: inline-flex;
    flex: 0 0 2.75rem;
    align-items: center;
    justify-content: center;
    min-height: 2.75rem;
    padding: 0;
    border: 0;
    border-inline-start: 1px solid var(--atlas-border);
    border-radius: 0;
    background: transparent;
  }
  .search-line .filter-toggle {
    border-radius: 0 999px 999px 0;
  }
  .search-action:hover,
  .filter-toggle[aria-expanded='true'] {
    color: var(--atlas-on-accent);
    background: var(--atlas-accent);
  }
  .search-line:focus-within {
    border-color: var(--atlas-accent);
    box-shadow: 0 0 0 1px color-mix(in srgb, var(--atlas-accent) 35%, transparent);
  }
  input,
  select,
  button {
    font: inherit;
    color: var(--atlas-text);
    background: var(--atlas-surface);
    border: 1px solid var(--atlas-border);
    border-radius: 0.4rem;
    padding: 0.5rem;
  }
  button {
    cursor: pointer;
  }
  .toolbox {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(145px, 1fr));
    gap: 0.75rem;
    margin-top: 0.85rem;
    padding-top: 0.85rem;
    border-top: 1px solid var(--atlas-border);
    max-height: 55vh;
    overflow-y: auto;
  }
  .toolbox label {
    display: flex;
    flex-direction: column;
    gap: 0.3rem;
    font-size: 0.85rem;
  }
  .toolbox input,
  .toolbox select {
    width: 100%;
    box-sizing: border-box;
  }
  .toolbox .check {
    flex-direction: row;
    align-items: center;
  }
  .toolbox .check input {
    width: auto;
  }
  .apply-row {
    grid-column: 1/-1;
    display: flex;
    align-items: center;
    gap: 0.75rem;
  }
  .apply-row span {
    font-size: 0.85rem;
    color: var(--atlas-muted);
  }
  .hint {
    grid-column: 1/-1;
    font-size: 0.8rem;
    margin: 0;
  }
  .validation {
    color: var(--atlas-text);
    font-size: 0.85rem;
    margin: 0.5rem 0;
  }
  .sr-only {
    position: absolute;
    width: 1px;
    height: 1px;
    padding: 0;
    margin: -1px;
    overflow: hidden;
    clip: rect(0, 0, 0, 0);
    white-space: nowrap;
    border: 0;
  }

  .filter-bar {
    position: static;
    max-width: none;
    margin: 0;
    padding: 0.75rem 0 1rem;
    border: 0;
    border-top: 1px solid var(--atlas-border);
    border-radius: 0;
  }
  /* Emphasize the invitation without enlarging the visitor's query text. */
  .search-line input::placeholder {
    font-size: 0.95rem;
    font-weight: 700;
    opacity: 1;
    color: var(--atlas-muted);
  }
  /* The pill supplies the focus treatment; avoid a second rectangular input ring. */
  .search-line input:focus-visible { outline: none; }
  .query-field { container-type: inline-size; }
  .search-line input { padding-inline: 0.65rem; }
  .query-field.has-query input { padding-inline-end: 2.75rem; }
  .search-line input::placeholder {
    font-size: clamp(0.65rem, 5cqi, 0.95rem);
    font-weight: 700;
  }
</style>

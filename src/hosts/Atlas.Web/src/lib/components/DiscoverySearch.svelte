<!--
  @component
  DiscoverySearch: Shared discovery search and filter toolbox; communicates through discoveryQuery state.
  Parses search tokens and publishes the accepted query to shared discovery state.
-->
<script lang="ts">
  import { onMount } from 'svelte';
  import {
    discoveryQuery,
    setDiscoveryQuery,
    restoreDiscoveryQuery,
  } from '$lib/state/discoveryQuery.svelte';
  import { readTypes, type NodeType } from '$lib/api/nodePage';
  import type { DiscoveryFilters } from '$lib/api/discovery';
  let {
    onExpandedChange,
  }: {
    /**
     * Callback: Reports whether the search filter toolbox is expanded, so the header can adjust
     * its layout.
     */
    onExpandedChange?: (expanded: boolean) => void;
  } = $props();
  /**
   * Available non-archived node types used by labels, filters or creation choices.
   */
  let types = $state<NodeType[]>([]);
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
  });
  const uuid = /^[0-9a-f]{8}-[0-9a-f-]{27,}$/i;
  // Separate ordinary search words from supported field tokens and collect validation messages.
  function parse(value: string) {
    const filters: DiscoveryFilters = {};
    const words: string[] = [];
    const errors: string[] = [];
    for (const part of value.match(/"[^"]*"|\S+/g) ?? []) {
      const match = /^(type|author|community|votes|average|from|through|archived):(.+)$/i.exec(
        part,
      );
      if (!match) {
        words.push(part);
        continue;
      }
      const [, rawKey, rawValue] = match;
      const key = rawKey.toLowerCase();
      const v = rawValue.replace(/^"|"$/g, '');
      if (key === 'type') {
        const found = types.find(
          (t) => t.id.toLowerCase() === v.toLowerCase() || t.name.toLowerCase() === v.toLowerCase(),
        );
        if (found) filters.typeId = found.id;
        else errors.push('Unknown node type: ' + v);
      } else if (key === 'author' || key === 'community') {
        if (uuid.test(v)) {
          if (key === 'author') filters.authorId = v;
          else filters.communityId = v;
        } else errors.push(key + ' requires an ID');
      } else if (key === 'votes' || key === 'average') {
        const m = /^(\d+(?:\.\d+)?)?\.\.(\d+(?:\.\d+)?)?$/.exec(v);
        if (!m || (!m[1] && !m[2])) {
          errors.push('Use ' + key + ':minimum..maximum');
          continue;
        }
        const low = m[1] ? Number(m[1]) : undefined,
          high = m[2] ? Number(m[2]) : undefined;
        if (
          (low !== undefined && high !== undefined && low > high) ||
          (key === 'votes' && [low, high].some((n) => n !== undefined && !Number.isInteger(n))) ||
          (key === 'average' && [low, high].some((n) => n !== undefined && n > 10))
        ) {
          errors.push('Invalid ' + key + ' range');
          continue;
        }
        if (key === 'votes') {
          filters.minVotes = low;
          filters.maxVotes = high;
        } else {
          filters.minAverage = low;
          filters.maxAverage = high;
        }
      } else if (key === 'from' || key === 'through') {
        if (/^\d{4}-\d{2}-\d{2}$/.test(v) && !Number.isNaN(Date.parse(v))) {
          if (key === 'from') filters.from = v;
          else filters.through = v;
        } else errors.push('Invalid date: ' + v);
      } else if (key === 'archived') {
        if (v === 'true') filters.archived = true;
        else if (v !== 'false') errors.push('Use archived:true or archived:false');
      }
    }
    return { text: words.join(' ').trim(), filters, errors };
  }
  // Publish only a valid query; the root page derives the actual discovery request filters.
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
  // Replace one field token without dropping unrelated terms in the draft query.
  function token(key: string, value: string) {
    const parts = (pending.match(/"[^"]*"|\S+/g) ?? []).filter(
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
      .catch(() => {});
    return () => controller.abort();
  });
</script>

<!--
  Layout: The search row stays visible; an expandable toolbox edits filter tokens before Apply
  publishes them to shared discovery state.
-->

<svelte:window onpointerdown={dismissOutside} onkeydown={dismissEscape} />
<section bind:this={searchRegion} class="filter-bar" aria-label="Filter discovery">
  <form
    class="search-line"
    onsubmit={(event) => {
      event.preventDefault();
      apply(input);
    }}
  >
    <label class="sr-only" for="discovery-query">Search nodes and filters</label>
    <input
      id="discovery-query"
      bind:value={input}
      placeholder="Search nodes or try type:Idea votes:5..100"
    />
    <button type="submit">Search</button>
    <button type="button" aria-expanded={expanded} onclick={() => (expanded = !expanded)}
      >Filters {expanded ? '▴' : '▾'}</button
    >
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
      <label
        >Node type<select
          value={editing.filters.typeId ?? ''}
          onchange={(e) => token('type', e.currentTarget.value)}
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
            apply('');
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
        <code>author:ID</code>, or <code>community:ID</code>. Text searches node content only for
        now.
      </p>
    </div>
  {/if}
</section>

<style>
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
  .search-line {
    display: flex;
    gap: 0.4rem;
    flex-wrap: wrap;
  }
  .search-line input {
    flex: 1 1 15rem;
    min-width: 0;
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
</style>

<script lang="ts">
  import { onMount } from 'svelte';
  import NodeCard from '../../src/lib/components/NodeCard.svelte';
  import { readDiscovery, discoveryCard, type DiscoveryPage } from '../../src/lib/api/discovery';

  let result = $state<DiscoveryPage | null>(null);
  let loading = $state(false);
  let error = $state('');
  let search = $state('');
  let appliedSearch = '';
  let selectedId = $state('');
  let ratings = $state<Record<string, number>>({});
  let request: AbortController | null = null;
  const items = $derived((result?.items ?? []).flatMap(item => {
    const node = discoveryCard(item);
    return node ? [{ node, average: item.averageVote, totalVotes: item.voteCount }] : [];
  }));
  const selected = $derived(items.find(item => item.node.id === selectedId) ?? items[0]);

  async function load(page = 1, query = appliedSearch) {
    request?.abort();
    const controller = new AbortController();
    request = controller;
    loading = true;
    error = '';
    const timeout = setTimeout(() => controller.abort(), 10000);
    try {
      const data = await readDiscovery('/atlas-preview-api', page, query, controller.signal);
      if (request !== controller) return;
      result = data;
      appliedSearch = query;
      if (!data.items.some(item => item.nodeId === selectedId)) selectedId = '';
    } catch (cause) {
      if (request !== controller) return;
      result = null;
      error = controller.signal.aborted ? 'The Atlas API request timed out.' : cause instanceof Error ? cause.message : 'Could not load Atlas nodes.';
    } finally {
      clearTimeout(timeout);
      if (request === controller) loading = false;
    }
  }

  onMount(() => {
    void load();
    return () => { request?.abort(); request = null; };
  });
</script>

<section class="live-preview" aria-label="Local API node preview" aria-busy={loading}>
  <p class="fixture-note">Local Atlas API · anonymous browsing. Rating changes stay in this preview and do not cast votes. Averages and totals come from Atlas.</p>
  <form onsubmit={(event) => { event.preventDefault(); void load(1, search); }}>
    <label>Search nodes <input bind:value={search} type="search" /></label>
    <button type="submit" disabled={loading}>Search</button>
    <button type="button" disabled={loading} onclick={() => void load(result?.page ?? 1)}>Refresh</button>
  </form>
  {#if loading}<p role="status">Loading Atlas nodes…</p>
  {:else if error}
    <div role="alert">
      <p>{error}</p>
      <p>Start Atlas.Api and use the Storybook development server. The default API address is https://localhost:7240. See the setup instructions in Atlas.Web/README.md.</p>
      <button type="button" onclick={() => void load()}>Retry</button>
    </div>
  {:else if result}
    <div class="paging">
      <button type="button" disabled={result.page <= 1} onclick={() => void load(result!.page - 1)}>Previous page</button>
      <span>Page {result.page} · {result.totalCount} matching nodes</span>
      <button type="button" disabled={!result.hasNextPage} onclick={() => void load(result!.page + 1)}>Next page</button>
    </div>
    {#if selected}
      <label class="node-picker">Node on this page
        <select value={selected.node.id} onchange={(event) => selectedId = event.currentTarget.value}>
          {#each items as item}<option value={item.node.id}>{item.node.typeName}: {item.node.title}</option>{/each}
        </select>
      </label>
      <NodeCard node={selected.node} average={selected.average} totalVotes={selected.totalVotes}
        value={ratings[selected.node.id] ?? null} onVote={(value) => ratings[selected.node.id] = value} />
      <p role="status">{ratings[selected.node.id] === undefined ? 'No local rating selected' : `Local preview selection: ${ratings[selected.node.id]} out of 10. Not submitted.`}</p>
    {:else}<p role="status">No displayable nodes on this page. Try another search or page.</p>{/if}
  {/if}
</section>

<style lang="scss">
  .live-preview {
    max-width: 48rem;
    form, .paging {
      display: flex;
      flex-wrap: wrap;
      align-items: end;
      gap: .75rem;
      margin-block: 1rem;
    }
    label {
      display: grid;
      gap: .4rem;
      min-width: 0;
    }
    input, select, button {
      font: inherit;
      padding: .5rem;
      max-width: 100%;
      color: var(--atlas-text);
      background: var(--atlas-surface);
      border: 1px solid var(--atlas-border);
      border-radius: var(--atlas-radius);
    }
    button {
      cursor: pointer;
    }
    button:disabled {
      cursor: default;
    }
    .node-picker {
      margin-block: 1rem;
    }
  }
</style>

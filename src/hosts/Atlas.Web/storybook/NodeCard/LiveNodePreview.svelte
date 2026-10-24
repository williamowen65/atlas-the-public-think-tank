<script lang="ts">
  import { openNodeStory } from '../shared/navigation';
  import LiveVotingDial from '../../src/lib/components/LiveVotingDial.svelte';
  import { onMount } from 'svelte';
  import type { NodeCardPresentation } from '../../src/lib/components/NodeCard.svelte';
  import NodeCard from '../../src/lib/components/NodeCard.svelte';
  import {
    readDiscovery,
    readOlderAncestors,
    readReactions,
    discoveryCard,
    type DiscoveryPage,
  } from '../../src/lib/api/discovery';

  let {
    presentation = 'ribbon',
  }: {
    /** Selects a NodeCardPresentation layout; it does not change the underlying node data. */
    presentation?: NodeCardPresentation;
  } = $props();
  async function loadOlderAncestors(id?: string) {
    if (!id) return { items: [], hasMore: false };
    return readOlderAncestors(id, AbortSignal.timeout(10000));
  }
  let result = $state<DiscoveryPage | null>(null);
  let loading = $state(false);
  let error = $state('');
  let search = $state('');
  let appliedSearch = '';
  let selectedId = $state('');
  let detailId = $state('');
  let reactions = $state<{ label: string; count: number }[] | null>(null);
  let reactionError = $state('');
  let request: AbortController | null = null;
  const items = $derived(
    (result?.items ?? []).flatMap((item) => {
      const node = discoveryCard(item);
      return node
        ? [
            {
              node,
              hasMoreAncestors: !!item.card?.breadcrumbSegment?.continuationNodeId,
              average: item.averageVote,
              totalVotes: item.voteCount,
            },
          ]
        : [];
    }),
  );
  const selected = $derived(items.find((item) => item.node.id === selectedId) ?? items[0]);

  $effect(() => {
    const id = selected?.node.id;
    result; // Refresh reaction totals when Discovery is refreshed too.
    reactions = null;
    reactionError = '';
    if (!id) return;
    const controller = new AbortController();
    const timeout = setTimeout(() => controller.abort(), 10000);
    void readReactions(id, controller.signal)
      .then((value) => {
        if (!controller.signal.aborted) reactions = value;
      })
      .catch(() => {
        if (!controller.signal.aborted)
          reactionError = 'Reaction totals could not be loaded. Refresh to retry.';
      })
      .finally(() => clearTimeout(timeout));
    return () => {
      controller.abort();
      clearTimeout(timeout);
    };
  });

  async function load(page = 1, query = appliedSearch, nodeId = '') {
    request?.abort();
    const controller = new AbortController();
    request = controller;
    loading = true;
    error = '';
    const timeout = setTimeout(() => controller.abort(), 10000);
    try {
      const data = await readDiscovery(
        '/atlas-public-api',
        page,
        query,
        controller.signal,
        nodeId || undefined,
      );
      if (request !== controller) return;
      result = data;
      if (
        nodeId &&
        !data.items.some((item) => item.nodeId === nodeId && item.card && !item.card.isHidden)
      ) {
        throw new Error(
          'This ancestor is unavailable or hidden. Return to Discovery to continue browsing.',
        );
      }
      detailId = nodeId;
      if (!nodeId) appliedSearch = query;
      if (!data.items.some((item) => item.nodeId === selectedId)) selectedId = '';
    } catch (cause) {
      if (request !== controller) return;
      result = null;
      error = controller.signal.aborted
        ? 'The Atlas API request timed out.'
        : cause instanceof Error
          ? cause.message
          : 'Could not load Atlas nodes.';
    } finally {
      clearTimeout(timeout);
      if (request === controller) loading = false;
    }
  }

  function navigate(id: string) {
    void load(1, '', id);
  }

  onMount(() => {
    void load();
    return () => {
      request?.abort();
      request = null;
    };
  });
</script>

<!--
  Layout: Live API explanation and search/paging controls precede one selected node card, with
  loading/error/empty alternatives.
-->

<section class="live-preview" aria-label="Local API node preview" aria-busy={loading}>
  <p class="preview-note">
    Live Atlas API · sign in when prompted to cast a rating. Averages and totals come from Atlas.
  </p>
  <form
    onsubmit={(event) => {
      event.preventDefault();
      void load(1, search);
    }}
  >
    <label>Search nodes <input bind:value={search} type="search" /></label>
    <button type="submit" disabled={loading}>Search</button>
    <button
      type="button"
      disabled={loading}
      onclick={() => void load(result?.page ?? 1, detailId ? '' : appliedSearch, detailId)}
      >Refresh</button
    >
    {#if detailId}<button
        type="button"
        disabled={loading}
        onclick={() => void load(1, appliedSearch)}>Back to Discovery</button
      >{/if}
  </form>
  {#if loading}<p role="status">Loading Atlas nodes…</p>
  {:else if error}
    <div role="alert">
      <p>{error}</p>
      <p>
        Start Atlas.Api and use the Storybook development server. The default API address is
        https://localhost:7240. See the setup instructions in Atlas.Web/README.md.
      </p>
      <button type="button" onclick={() => void load()}>Retry</button>
    </div>
  {:else if result}
    <div class="paging">
      <button type="button" disabled={result.page <= 1} onclick={() => void load(result!.page - 1)}
        >Previous page</button
      >
      <span>Page {result.page} · {result.totalCount} matching nodes</span>
      <button
        type="button"
        disabled={!result.hasNextPage}
        onclick={() => void load(result!.page + 1)}>Next page</button
      >
    </div>
    {#if selected}
      <label class="node-picker"
        >Node on this page
        <select
          value={selected.node.id}
          onchange={(event) => (selectedId = event.currentTarget.value)}
        >
          {#each items as item}<option value={item.node.id}
              >{item.node.typeName}: {item.node.title}</option
            >{/each}
        </select>
      </label>
      <NodeCard
        hasMoreAncestors={selected.hasMoreAncestors}
        {loadOlderAncestors}
        {presentation}
        node={{ ...selected.node, reactions: reactions ?? [], reactionSummary: undefined }}
        average={selected.average}
        totalVotes={selected.totalVotes}
        onNavigate={navigate}
        onReadMore={openNodeStory}
        onChildType={openNodeStory}
        onComments={(id) => openNodeStory(id, 'comments')}
        >{#snippet voting()}{#if selected}{#key selected.node.id}<LiveVotingDial
                nodeId={selected.node.id}
                initialAverage={selected.average}
                initialTotalVotes={selected.totalVotes}
                archived={selected.node.status === 'Archived'}
              />{/key}{/if}{/snippet}</NodeCard
      >
      {#if reactionError}<p role="note">{reactionError}</p>{/if}
    {:else}<p role="status">No displayable nodes on this page. Try another search or page.</p>{/if}
  {/if}
</section>

<style lang="scss">
  .live-preview {
    max-width: 48rem;
    form,
    .paging {
      display: flex;
      flex-wrap: wrap;
      align-items: end;
      gap: 0.75rem;
      margin-block: 1rem;
    }
    label {
      display: grid;
      gap: 0.4rem;
      min-width: 0;
    }
    input,
    select,
    button {
      font: inherit;
      padding: 0.5rem;
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

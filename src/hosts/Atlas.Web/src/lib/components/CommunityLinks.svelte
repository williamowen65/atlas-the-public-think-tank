<!--
  @component
  CommunityLinks: Reads node-community associations and displays links, with names as fallback.
  Uses supplied names until public community records are available.
-->
<script lang="ts">
  import { onMount } from 'svelte';
  import { apiRequest } from '../api/apiRequest';
  import type { Community } from '../api/communities';
  let {
    nodeId,
    names,
  }: {
    /** ID of the node this component reads, displays, or modifies. */
    nodeId: string;
    /** Fallback community display names while linked community records are being fetched. */
    names: string[];
  } = $props();
  /**
   * Public community associations loaded for this node.
   */
  let linked = $state<Community[]>([]);
  // Fetch linkable records once mounted; abort on removal and keep fallback names on failure.
  onMount(() => {
    const controller = new AbortController();
    void apiRequest<Community[]>(
      '/atlas-public-api',
      'communities/node/' + encodeURIComponent(nodeId),
      'GET',
      undefined,
      controller.signal,
    )
      .then((items) => {
        if (!controller.signal.aborted) linked = items;
      })
      .catch(() => {});
    return () => controller.abort();
  });
  /**
   * Linked community records when available, otherwise supplied display-only names.
   */
  const labels = $derived(
    linked.length
      ? linked.map((c) => ({ id: c.id, name: c.name }))
      : names.map((name) => ({ id: '', name })),
  );
</script>

<!--
  Layout: An inline comma-separated list: known community IDs become links, while fallback names
  remain plain text. No block wrapper is added.
-->

{#each labels as community, i (community.id || i)}
  {#if i > 0},
  {/if}
  {#if community.id}<a href={'/communities/' + encodeURIComponent(community.id)}>{community.name}</a
    >{:else}{community.name}{/if}
{/each}

<style>
  a {
    color: inherit;
    text-decoration: none;
  }
  a:hover,
  a:focus-visible {
    text-decoration: underline;
  }
</style>

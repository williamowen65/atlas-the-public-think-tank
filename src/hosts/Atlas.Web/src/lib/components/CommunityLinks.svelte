<script lang="ts">
  import { onMount } from 'svelte';
  import { commentRequest } from '../api/comments';
  import type { Community } from '../api/communities';
  let { nodeId, names }: { nodeId: string; names: string[] } = $props();
  let linked = $state<Community[]>([]);
  onMount(() => {
    const controller = new AbortController();
    void commentRequest<Community[]>(
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
  const labels = $derived(
    linked.length
      ? linked.map((c) => ({ id: c.id, name: c.name }))
      : names.map((name) => ({ id: '', name })),
  );
</script>

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

<!--
  @component
  Node-detail route adapting URL node/tab IDs and app navigation callbacks to LiveNodePage.

  **Props**
  No explicit component props. SvelteKit selects this route; URL parameters are read by the route itself.
-->
<script lang="ts">
  import { page } from '$app/state';
  import { goto } from '$app/navigation';
  import LiveNodePage from '$lib/components/LiveNodePage.svelte';
</script>

<!--
  Layout: The route supplies URL-selected node/tab IDs and navigation callbacks; LiveNodePage
  owns the visible node-page composition.
-->

<svelte:head><title>Node · Atlas</title></svelte:head>
<div class="node-route">
  <LiveNodePage
    onOpenProfile={(id) => void goto('/profiles?id=' + encodeURIComponent(id))}
    focusCommentId={page.url.searchParams.get('comment') ?? ''}
    initialTypeId={page.url.searchParams.get('type') ?? ''}
    nodeId={page.url.searchParams.get('id') ?? ''}
    onOpenNode={(id, typeId) =>
      void goto(
        `/nodes?id=${encodeURIComponent(id)}${typeId ? `&type=${encodeURIComponent(typeId)}#contributions` : ''}`,
      )}
  />
</div>

<style lang="scss">
  .node-route {
    padding-block-start: var(--atlas-space);
  }
</style>

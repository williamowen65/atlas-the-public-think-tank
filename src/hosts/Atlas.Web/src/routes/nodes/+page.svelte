<!--
  @component
  Node-detail route adapting URL node/tab IDs and app navigation callbacks to LiveNodePage.
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
<LiveNodePage
  onOpenProfile={(id) => void goto('/profiles?id=' + encodeURIComponent(id))}
  initialTypeId={page.url.searchParams.get('type') ?? ''}
  nodeId={page.url.searchParams.get('id') ?? ''}
  onOpenNode={(id, typeId) =>
    void goto(
      `/nodes?id=${encodeURIComponent(id)}${typeId ? `&type=${encodeURIComponent(typeId)}#contributions` : ''}`,
    )}
/>

<script lang="ts">
  import { page } from '$app/state';
  import { goto } from '$app/navigation';
  import LiveNodePage from '$lib/components/LiveNodePage.svelte';
</script>
<svelte:head><title>Node · Atlas</title></svelte:head>
<LiveNodePage initialTypeId={page.url.searchParams.get('type') ?? ''} nodeId={page.url.searchParams.get('id') ?? ''} onOpenNode={(id, typeId) => void goto(`/nodes?id=${encodeURIComponent(id)}${typeId ? `&type=${encodeURIComponent(typeId)}#contributions` : ''}`)} />

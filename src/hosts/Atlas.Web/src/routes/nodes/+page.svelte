<script lang="ts">
  import { page } from '$app/state';
  import { goto } from '$app/navigation';
  import LiveNodePage from '$lib/components/LiveNodePage.svelte';
</script>
<svelte:head><title>Node · Atlas</title></svelte:head>
<LiveNodePage nodeId={page.url.searchParams.get('id') ?? ''} onOpenNode={id => void goto(`/nodes?id=${encodeURIComponent(id)}`)} />

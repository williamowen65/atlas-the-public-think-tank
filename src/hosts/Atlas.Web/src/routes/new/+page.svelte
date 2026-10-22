<script lang="ts">
  import { page } from '$app/state';
  import { goto } from '$app/navigation';
  import LiveNodeForm from '$lib/components/LiveNodeForm.svelte';
</script>
<svelte:head><title>Create a node · Atlas</title></svelte:head>
<h1>Create a node</h1><LiveNodeForm draftId={page.url.searchParams.get('draft') ?? undefined} onCreated={id => void goto(`/nodes?id=${encodeURIComponent(id)}`)} />

<script lang="ts">
  import { page } from '$app/state';
  import { goto } from '$app/navigation';
  import LiveProfilePage from '$lib/components/LiveProfilePage.svelte';
</script>
<svelte:head><title>Public profiles · Atlas</title></svelte:head>
<LiveProfilePage participantId={page.url.searchParams.get('id') ?? ''} onOpenNode={id => void goto('/nodes?id=' + encodeURIComponent(id))} onOpenProfile={id => void goto('/profiles?id=' + encodeURIComponent(id))} />

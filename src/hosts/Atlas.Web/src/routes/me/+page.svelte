<script lang="ts">
  import { goto } from '$app/navigation';
  import LiveMyProfile from '$lib/components/LiveMyProfile.svelte';
</script>
<svelte:head><title>My profile · Atlas</title></svelte:head>
<LiveMyProfile onOpenNode={id => void goto('/nodes?id=' + encodeURIComponent(id))} onOpenProfile={id => void goto('/profiles?id=' + encodeURIComponent(id))} />

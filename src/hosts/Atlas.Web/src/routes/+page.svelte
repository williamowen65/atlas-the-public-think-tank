<script lang="ts">
  import { goto } from '$app/navigation';
  import LiveNodeFeed from '$lib/components/LiveNodeFeed.svelte';
</script>
<svelte:head><title>Discover · Atlas</title><meta name="description" content="Explore connected questions, evidence, and ideas in Atlas." /></svelte:head>
<div class="feed-intro"><h1>Discover</h1><p>Follow a question. Find a new perspective. Build on an idea.</p></div>
<LiveNodeFeed onOpenNode={id => void goto(`/nodes?id=${encodeURIComponent(id)}`)} />
<style>.feed-intro{max-width:48rem;margin:2rem auto;}h1{margin-bottom:.5rem;}p{color:var(--atlas-muted);}</style>

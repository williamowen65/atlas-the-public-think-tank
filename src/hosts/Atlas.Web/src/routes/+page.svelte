<!--
  @component
  Discover’s browsing feed, independent of the global header search results.

  **Props**
  No explicit component props. SvelteKit selects this route; URL parameters are read by the route itself.
-->
<script lang="ts">
  import { goto } from '$app/navigation';
  import LiveNodeFeed from '$lib/components/LiveNodeFeed.svelte';
</script>

<!--
  Layout: Page metadata and an introduction precede the browsing feed. Global search has its own compact results in the header.
-->

<svelte:head>
  <title>Discover · Atlas</title>
  <meta name="description" content="Explore connected questions, evidence, and ideas in Atlas." />
</svelte:head>
<div class="feed-intro">
  <h1>Discover</h1>
  <p>Follow a question. Find a new perspective. Build on an idea.</p>
</div>

<LiveNodeFeed
  onOpenProfile={(id) => void goto('/profiles?id=' + encodeURIComponent(id))}
  onOpenNode={(id, typeId) =>
    void goto(
      `/nodes?id=${encodeURIComponent(id)}${typeId ? `&type=${encodeURIComponent(typeId)}#contributions` : ''}`,
    )}
/>

<style>
  .feed-intro {
    max-width: 48rem;
    margin: 2rem auto;
  }
  h1 {
    margin-bottom: 0.5rem;
  }
  p {
    color: var(--atlas-muted);
  }
</style>

<script lang="ts">
  import { page } from '$app/state';
  import { goto } from '$app/navigation';
  import LiveNodeForm from '$lib/components/LiveNodeForm.svelte';
</script>

<!--
  Layout: Page heading followed by the live creation form; a URL draft ID can resume local edits
  and successful creation navigates to the node.
-->

<svelte:head><title>Create a node · Atlas</title></svelte:head>
<h1>Create a node</h1>
<LiveNodeForm
  draftId={page.url.searchParams.get('draft') ?? undefined}
  onCreated={(id) => void goto(`/nodes?id=${encodeURIComponent(id)}`)}
/>

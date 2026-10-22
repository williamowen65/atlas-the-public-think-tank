<script lang="ts">
  import LiveNodePage from '../../src/lib/components/LiveNodePage.svelte';
  import { openStory, openNodeStory } from '../shared/navigation';
  let { nodeId = '', initialTypeId = '' }: { nodeId?: string; initialTypeId?: string } = $props();
</script>
<button onclick={() => openStory('compositions-feed--playground')}>Back to feed</button>
<LiveNodePage {nodeId} {initialTypeId} onOpenNode={openNodeStory} />

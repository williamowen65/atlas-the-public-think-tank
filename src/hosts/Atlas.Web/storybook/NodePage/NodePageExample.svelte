<script lang="ts">
  import LiveNodePage from '../../src/lib/components/LiveNodePage.svelte';
  import { openStory, openNodeStory } from '../shared/navigation';
  let { nodeId = '' }: { nodeId?: string } = $props();
</script>
<button onclick={() => openStory('compositions-feed--playground')}>Back to feed</button>
<LiveNodePage {nodeId} onOpenNode={openNodeStory} />

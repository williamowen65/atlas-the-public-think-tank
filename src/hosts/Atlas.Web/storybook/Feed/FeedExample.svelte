<script lang="ts">
  import LiveNodeFeed from '../../src/lib/components/LiveNodeFeed.svelte';
  import type { NodeCardPresentation } from '../../src/lib/components/NodeCard.svelte';
  import { openNodeStory, openProfileStory } from '../shared/navigation';
  let { presentation = 'ribbon-corner' }: { presentation?: NodeCardPresentation } = $props();
</script>

<LiveNodeFeed {presentation} onOpenNode={openNodeStory} onOpenProfile={openProfileStory} />

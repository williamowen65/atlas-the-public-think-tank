<script lang="ts">
  import LiveNodeFeed from '../../src/lib/components/LiveNodeFeed.svelte';
  import type { NodeCardPresentation } from '../../src/lib/components/NodeCard.svelte';
  import { openNodeStory, openProfileStory } from '../shared/navigation';
  let {
    presentation = 'ribbon-corner',
  }: {
    /** Selects a NodeCardPresentation layout; it does not change the underlying node data. */
    presentation?: NodeCardPresentation;
  } = $props();
</script>

<LiveNodeFeed {presentation} onOpenNode={openNodeStory} onOpenProfile={openProfileStory} />

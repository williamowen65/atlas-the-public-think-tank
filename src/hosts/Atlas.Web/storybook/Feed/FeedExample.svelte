<!--
  @component
  Live feed workbench wrapper adapting node/profile navigation to Storybook stories.

  **Props**
  - `presentation` (`NodeCardPresentation`; optional; default `'ribbon-corner'`): Selects a
    NodeCardPresentation layout; it does not change the underlying node data.
-->
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

<!--
  Layout: The live feed is rendered directly; navigation callbacks keep node/profile links
  inside Storybook.
-->

<LiveNodeFeed {presentation} onOpenNode={openNodeStory} onOpenProfile={openProfileStory} />

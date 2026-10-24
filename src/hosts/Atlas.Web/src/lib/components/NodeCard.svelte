<!--
  NodeCard: Selects a card presentation and forwards the common card contract.
  Only selects a visual variant; shared props and optional voting/reaction snippets pass through unchanged.
-->
<script lang="ts" module>
  export type { NodeCardData, NodeCardPresentation } from './node-card-versions/types';
</script>

<script lang="ts">
  import NodeCardContextBelow from './node-card-versions/NodeCardContextBelow.svelte';
  import NodeCardRibbon from './node-card-versions/NodeCardRibbon.svelte';
  import NodeCardRibbonCorner from './node-card-versions/NodeCardRibbonCorner.svelte';
  import NodeCardRibbonSplit from './node-card-versions/NodeCardRibbonSplit.svelte';
  import NodeCardRibbonResponsive from './node-card-versions/NodeCardRibbonResponsive.svelte';
  import type { NodeCardProps, NodeCardPresentation } from './node-card-versions/types';
  // A supplied presentation overrides the default; the remaining props are forwarded to the selected variant.
  let {
    presentation = 'ribbon',
    ...props
  }: NodeCardProps & {
    /** Selects a NodeCardPresentation layout; it does not change the underlying node data. */
    presentation?: NodeCardPresentation;
  } = $props();
</script>

<!--
  Layout: Exactly one presentation variant is rendered; this component adds no layout wrapper
  and forwards the common card props.
-->

{#if presentation === 'context-below'}
  <NodeCardContextBelow {...props} />
{:else if presentation === 'ribbon-corner'}
  <NodeCardRibbonCorner {...props} />
{:else if presentation === 'ribbon-split'}
  <NodeCardRibbonSplit {...props} />
{:else if presentation === 'ribbon-responsive'}
  <NodeCardRibbonResponsive {...props} />
{:else}
  <NodeCardRibbon {...props} />
{/if}

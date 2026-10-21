<script lang="ts" module>
  export type { NodeCardData, NodeCardPresentation } from './node-card/types';
</script>

<script lang="ts">
  import NodeCardCurrent from './node-card/NodeCardCurrent.svelte';
  import NodeCardRibbon from './node-card/NodeCardRibbon.svelte';
  import type { NodeCardProps, NodeCardPresentation } from './node-card/types';
  let { presentation = 'ribbon', ...props }: NodeCardProps & { presentation?: NodeCardPresentation } = $props();
</script>

{#if presentation === 'current'}
  <NodeCardCurrent {...props} />
{:else}
  <NodeCardRibbon {...props} />
{/if}

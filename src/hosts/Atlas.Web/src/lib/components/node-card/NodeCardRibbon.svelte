<script lang="ts">
  import NodeCardLayout from './NodeCardLayout.svelte';
  import type { NodeCardProps } from './types';
  let props: NodeCardProps = $props();
</script>

<NodeCardLayout {...props} presentation="ribbon" />

<script lang="ts">
  import type { Snippet } from 'svelte';
  let { side, width = 100, children }: { side: 'left' | 'right'; width?: number; children: Snippet } = $props();
</script>

<div class="section-wrapped-image" class:right={side === 'right'} style:--image-scale={width / 100}>
  {@render children()}
</div>

<style>
  .section-wrapped-image {
    float: left;
    clear: both;
    width: calc((45% - .5rem) * var(--image-scale));
    margin: 0 1rem .55rem 0;
  }
  .right { float: right; margin-inline: 1rem 0; }
  @container (max-width: 37rem) {
    .section-wrapped-image { float: none; width: calc(100% * var(--image-scale)); margin-inline: 0; }
  }
</style>

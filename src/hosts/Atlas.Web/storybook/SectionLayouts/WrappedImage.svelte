<script lang="ts">
  import type { Snippet } from 'svelte';
  let { side, children }: { side: 'left' | 'right'; children: Snippet } = $props();
</script>

<div class="section-wrapped-image" class:right={side === 'right'}>
  {@render children()}
</div>

<style>
  .section-wrapped-image {
    float: left;
    clear: both;
    width: calc(45% - .5rem);
    margin: 0 1rem .55rem 0;
  }
  .right { float: right; margin-inline: 1rem 0; }
  @container (max-width: 37rem) {
    .section-wrapped-image { float: none; width: 100%; margin-inline: 0; }
  }
</style>

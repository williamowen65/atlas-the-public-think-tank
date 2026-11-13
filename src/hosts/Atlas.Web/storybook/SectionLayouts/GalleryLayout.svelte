<script lang="ts">
  import type { Snippet } from 'svelte';
  import type { GalleryStyle, Photo } from './sectionLayouts';

  let { photos, layout = 'grid', selected, onNavigate, photo }: {
    photos: Photo[];
    layout?: GalleryStyle;
    selected?: string | null;
    onNavigate?: () => void;
    photo: Snippet<[Photo]>;
  } = $props();

  let index = $state(0);
  const selectedIndex = $derived(photos.findIndex(({ id }) => id === selected));
  const current = $derived(selectedIndex >= 0 ? selectedIndex : Math.min(index, photos.length - 1));

  $effect(() => {
    if (selectedIndex >= 0) index = selectedIndex;
  });

  function navigate(direction: number) {
    index = (current + direction + photos.length) % photos.length;
    onNavigate?.();
  }
</script>

{#if layout === 'slideshow' && photos.length}
  <div class="slideshow">
    {#key photos[current].id}
      {@render photo(photos[current])}
    {/key}
    {#if photos.length > 1}
      <div class="slide-controls">
        <button type="button" aria-label="Previous gallery image" onclick={() => navigate(-1)}>‹</button>
        <span aria-live="polite">{current + 1} of {photos.length}</span>
        <button type="button" aria-label="Next gallery image" onclick={() => navigate(1)}>›</button>
      </div>
    {/if}
  </div>
{:else}
  <div class="tiles" class:mosaic={layout === 'mosaic'} class:single={photos.length === 1}>
    {#each photos as item (item.id)}
      <div class="tile">{@render photo(item)}</div>
    {/each}
  </div>
{/if}

<style>
  .tiles { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: .6rem; align-items: start; }
  .mosaic { grid-template-columns: repeat(3, minmax(0, 1fr)); }
  .mosaic .tile:is(:nth-child(4n + 1), :nth-child(4n + 4)) { grid-column: span 2; }
  .single .tile, .tiles .tile:last-child:nth-child(odd) { grid-column: 1 / -1; width: min(100%, 32rem); justify-self: center; }
  .slideshow { width: min(100%, 40rem); margin-inline: auto; }
  .slide-controls { display: flex; align-items: center; justify-content: center; gap: .75rem; margin-block-start: .4rem; }
  .slide-controls button { min-width: 2.75rem; min-height: 2.75rem; border: 1px solid var(--atlas-border); border-radius: var(--atlas-radius); background: var(--atlas-surface); color: inherit; cursor: pointer; }
  @container (max-width: 24rem) {
    .mosaic { grid-template-columns: repeat(2, minmax(0, 1fr)); }
    .mosaic .tile:not(:last-child):is(:nth-child(4n + 1), :nth-child(4n + 4)) { grid-column: auto; }
    .mosaic .tile:last-child:nth-child(even) { grid-column: auto; }
  }
</style>

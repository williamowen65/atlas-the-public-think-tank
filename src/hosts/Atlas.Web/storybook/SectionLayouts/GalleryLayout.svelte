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

  const mosaicRows = $derived(photos.flatMap((_, index) =>
    index % 3 === 0 ? [photos.slice(index, index + 3)] : []));

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
{:else if layout === 'mosaic' && photos.length > 1}
  <div class="mosaic">
    {#each mosaicRows as [feature, ...small], row (feature.id)}
      <div class="mosaic-row" class:reverse={row % 2 === 1} class:single={!small.length} class:paired={small.length === 2}>
        <div class="feature">{@render photo(feature)}</div>
        {#if small.length}
          <div class="mosaic-stack">
            {#each small as item (item.id)}
              {@render photo(item)}
            {/each}
          </div>
        {/if}
      </div>
    {/each}
  </div>
{:else if layout === 'masonry' && photos.length > 1}
  <div class="masonry">
    {#each photos as item (item.id)}
      <div class="masonry-tile">{@render photo(item)}</div>
    {/each}
  </div>
{:else}
  <div class="tiles">
    {#each photos as item (item.id)}
      <div class="tile">{@render photo(item)}</div>
    {/each}
  </div>
{/if}

<style>
  .tiles { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: .6rem; align-items: start; }
  .tiles .tile:last-child:nth-child(odd), .mosaic-row.single .feature {
    grid-column: 1 / -1; width: min(100%, 32rem); justify-self: center;
  }
  .mosaic, .mosaic-stack { display: grid; gap: .6rem; align-content: start; }
  .mosaic-row { display: grid; grid-template-columns: minmax(0, 2fr) minmax(0, 1fr); gap: .6rem; align-items: start; }
  .mosaic-row.reverse { grid-template-columns: minmax(0, 1fr) minmax(0, 2fr); }
  .reverse:not(.single) .feature { grid-area: 1 / 2; }
  .reverse .mosaic-stack { grid-area: 1 / 1; }
  .mosaic-row:not(.single) :global(figure) { position: relative; width: 100% !important; margin: 0; }
  .mosaic-row:not(.single) :global(figure > button) { display: block; width: 100%; }
  .mosaic-row:not(.single) :global(figure > button > .frame) { width: 100%; aspect-ratio: 1 !important; }
  .mosaic-row:not(.single) :global(figure > button > .frame > img) {
    width: 100%; height: 100%; object-fit: cover;
    object-position: var(--focus-x) var(--focus-y);
    transform: scale(var(--zoom)); transform-origin: var(--focus-x) var(--focus-y);
  }
  .paired .feature { align-self: stretch; position: relative; }
  .paired .feature :global(figure) {
    position: absolute; inset: 0; display: grid; grid-template-rows: minmax(0, 1fr) auto;
  }
  .paired .feature :global(figure > button),
  .paired .feature :global(figure > button > .frame) { height: 100%; }
  .masonry { columns: 3; column-gap: .6rem; }
  .masonry-tile { break-inside: avoid; display: inline-block; vertical-align: top; width: 100%; padding-block-end: .6rem; }
  .slideshow { width: min(100%, 40rem); margin-inline: auto; }
  .slide-controls { display: flex; align-items: center; justify-content: center; gap: .75rem; margin-block-start: .4rem; }
  .slide-controls button { min-width: 2.75rem; min-height: 2.75rem; border: 1px solid var(--atlas-border); border-radius: var(--atlas-radius); background: var(--atlas-surface); color: inherit; cursor: pointer; }
  @container (max-width: 24rem) {
    .mosaic-row, .mosaic-row.reverse { grid-template-columns: repeat(2, minmax(0, 1fr)); }
    .masonry { columns: 2; }
  }
  @container (max-width: 17rem) {
    .masonry { columns: 1; }
  }
</style>

<script lang="ts">
  import type { Snippet } from 'svelte';
  import type { GalleryStyle, Photo, SlideshowSettings } from './sectionLayouts';

  let { photos, layout = 'grid', width = 'full', masonryAlignment = 'top', slideshow = {}, selected, onNavigate, photo }: {
    photos: Photo[];
    layout?: GalleryStyle;
    width?: 'full' | 'centered';
    masonryAlignment?: 'top' | 'center';
    slideshow?: SlideshowSettings;
    selected?: string | null;
    onNavigate?: () => void;
    photo: Snippet<[Photo, boolean?]>;
  } = $props();

  const mosaicRows = $derived(photos.flatMap((_, index) =>
    index % 3 === 0 ? [photos.slice(index, index + 3)] : []));

  let masonryWidth = $state(0);
  const columnCount = $derived(Math.min(photos.length, masonryWidth < 272 ? 1 : masonryWidth < 384 ? 2 : 3));
  const masonryColumns = $derived(Array.from({ length: columnCount }, (_, column) => {
    const size = Math.floor(photos.length / columnCount);
    const remainder = photos.length % columnCount;
    const start = column * size + Math.min(column, remainder);
    return photos.slice(start, start + size + Number(column < remainder));
  }));

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

<div class:centered={width === 'centered'}>
  {#if layout === 'slideshow' && photos.length}
    <div class="slideshow">
      <div class="slide-stage" class:peek={slideshow.neighbors && photos.length > 1}
        class:fill={slideshow.fit === 'cover'} data-height={slideshow.height ?? 'standard'}
        class:has-captions={photos.some((item) => item.caption && item.captionPlacement !== 'overlay')}>
        {#if slideshow.neighbors && photos.length > 1}
          <div class="neighbor previous" inert aria-hidden="true">
            {@render photo(photos[(current + photos.length - 1) % photos.length], true)}
          </div>
        {/if}
        <div class="current-slide">
          {#key photos[current].id}
            {@render photo(photos[current])}
          {/key}
        </div>
        {#if slideshow.neighbors && photos.length > 1}
          <div class="neighbor next" inert aria-hidden="true">
            {@render photo(photos[(current + 1) % photos.length], true)}
          </div>
        {/if}
      </div>
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
    <div class="masonry" class:middle={masonryAlignment === 'center'}
      bind:clientWidth={masonryWidth} style:--column-count={columnCount}>
      {#each masonryColumns as items}
        <div class="masonry-column">
          {#each items as item (item.id)}
            {@render photo(item)}
          {/each}
        </div>
      {/each}
    </div>
  {:else}
    <div class="tiles">
      {#each photos as item (item.id)}
        <div class="tile">{@render photo(item)}</div>
      {/each}
    </div>
  {/if}

</div>

<style>
  .centered { width: min(84%, 48rem); margin-inline: auto; }
  @container (max-width: 37rem) { .centered { width: 100%; } }
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
    position: absolute; inset: 0; display: grid; grid-template: minmax(0, 1fr) auto / minmax(0, 1fr);
  }
  .paired .feature :global(figure > button) { min-width: 0; min-height: 0; height: 100%; }
  .paired .feature :global(figure > button > .frame) { height: 100%; aspect-ratio: auto !important; }
  .paired .feature :global(figcaption) { overflow-wrap: anywhere; }
  .masonry { display: grid; grid-template-columns: repeat(var(--column-count), minmax(0, 1fr)); gap: .6rem; align-items: start; }
  .masonry.middle { align-items: center; }
  .masonry-column { display: grid; gap: .6rem; min-width: 0; }
  .slideshow { container-type: inline-size; width: min(100%, 40rem); margin-inline: auto; }
  .slide-stage { --slide-height: min(28rem, 57svh); position: relative; isolation: isolate; }
  .slide-stage[data-height='compact'] { --slide-height: min(18rem, 45svh); }
  .slide-stage[data-height='tall'] { --slide-height: min(36rem, 65svh); }
  .peek {
    --peek-width: clamp(.75rem, 7cqi, 3rem);
    --slide-gap: clamp(.375rem, 1.5cqi, .75rem);
    display: grid;
    grid-template-columns: var(--peek-width) minmax(0, 1fr) var(--peek-width);
    gap: var(--slide-gap);
    align-items: start;
  }
  .neighbor { position: relative; height: var(--slide-height); overflow: hidden; }
  .neighbor :global(figure) {
    position: absolute; inset-block-start: 0;
    width: calc(100cqi - 2 * (var(--peek-width) + var(--slide-gap))) !important;
  }
  .previous :global(figure) { inset-inline-end: 0; }
  .next :global(figure) { inset-inline-start: 0; }
  .current-slide { position: relative; z-index: 1; }
  .slide-stage :global(figure) { width: 100% !important; margin: 0; min-width: 0; }
  .slide-stage :global(figure > :is(button, .neighbor-frame)) { display: block; width: 100%; height: var(--slide-height); }
  .slide-stage :global(figure > :is(button, .neighbor-frame)) { background-color: var(--atlas-bg); }
  .slide-stage :global(figure > :is(button, .neighbor-frame) > .frame) {
    width: 100%; height: 100%; aspect-ratio: auto !important; background: var(--atlas-bg);
  }
  .slide-stage :global(figure > :is(button, .neighbor-frame) > .frame > img) {
    width: 100%; height: 100%; object-fit: contain; transform: none;
  }
  .fill :global(figure > :is(button, .neighbor-frame) > .frame > img) {
    object-fit: cover; object-position: var(--focus-x) var(--focus-y);
    transform: scale(var(--zoom)); transform-origin: var(--focus-x) var(--focus-y);
  }
  .has-captions .current-slide { padding-block-end: 2.75rem; }
  .current-slide :global(figcaption) {
    position: absolute; inset: auto 0 0; height: 2.5rem; overflow: auto;
    width: 100%; margin: 0; overflow-wrap: anywhere;
  }
  .neighbor :global(:is(figcaption, .caption-overlay)) { display: none; }
  .slide-controls { display: flex; align-items: center; justify-content: center; gap: .75rem; margin-block-start: .4rem; }
  .slide-controls button { min-width: 2.75rem; min-height: 2.75rem; border: 1px solid var(--atlas-border); border-radius: var(--atlas-radius); background: var(--atlas-surface); color: inherit; cursor: pointer; }
  @container (max-width: 24rem) {
    .mosaic-row, .mosaic-row.reverse { grid-template-columns: repeat(2, minmax(0, 1fr)); }
  }
</style>

<!--
  Storybook-only FEED presentation of the node's EXISTING top-level content
  blocks. This is not an authoring block kind and has no independent slides,
  nested content, storage, voting or API calls.
-->
<script lang="ts">
  import ContentBlocks from '../../src/lib/components/ContentBlocks.svelte';
  import type { ContentBlock } from '../../src/lib/api/nodePage';

  let {
    blocks, index = 0, frame = 'landscape', onIndexChange, onOpenFull,
  }: {
    blocks: ContentBlock[];
    index?: number;
    frame?: 'portrait' | 'landscape';
    onIndexChange?: (next: number) => void;
    onOpenFull?: () => void;
  } = $props();

  let startX: number | null = null;
  const safeIndex = $derived(Math.max(0, Math.min(index, blocks.length - 1)));
  const active = $derived(blocks[safeIndex]);
  const hasMore = $derived(blocks.length > 1);

  function move(direction: number) {
    if (!blocks.length) return;
    onIndexChange?.((safeIndex + direction + blocks.length) % blocks.length);
  }

  function onTouchStart(event: TouchEvent) {
    // Don't turn child interactions into navigation gestures.
    const target = event.target;
    startX = target instanceof Element && target.closest('button, a, input, video, textarea')
      ? null
      : (event.touches[0]?.clientX ?? null);
  }

  function onTouchEnd(event: TouchEvent) {
    if (startX === null) return;
    const end = event.changedTouches[0]?.clientX;
    const distance = end === undefined ? 0 : end - startX;
    startX = null;
    if (Math.abs(distance) >= 45) move(distance < 0 ? 1 : -1);
  }
</script>

<section class="feed-block-carousel" aria-label="Feed preview of node content blocks">
  <div class="summary-row">
    <span>Explore this node</span>
    <span class="progress" aria-live="polite">
      {blocks.length ? safeIndex + 1 : 0} of {blocks.length} blocks
    </span>
  </div>

  <div class="viewport" class:portrait={frame === 'portrait'} class:landscape={frame === 'landscape'}
    role="region" aria-label="Node content carousel, swipe or use arrows"
    aria-roledescription="carousel" tabindex="0"
    onkeydown={(event) => {
      if (event.target !== event.currentTarget) return;
      if (event.key === 'ArrowRight') { event.preventDefault(); move(1); }
      if (event.key === 'ArrowLeft') { event.preventDefault(); move(-1); }
    }}
    ontouchstart={onTouchStart} ontouchend={onTouchEnd}
    ontouchcancel={() => { startX = null; }}>

    {#if active}
      <div class="feed-block-slide" role="group" aria-roledescription="slide"
        aria-label={'Block ' + (safeIndex + 1) + ' of ' + blocks.length}
        data-block-id={active.id} data-block-kind={active.kind}>
        <div class="block-kind">{active.kind.toLowerCase().includes('markdown') ? 'Text' :
          active.kind.toLowerCase().includes('image') ? 'Image' : active.kind} block</div>
        <div class="block-preview">
          {#key active.id}
            <ContentBlocks blocks={[active]} />
          {/key}
        </div>
      </div>
    {:else}
      <div class="empty">This node has no content blocks yet.</div>
    {/if}

    {#if hasMore}
      <button type="button" class="nav previous" aria-label="Previous node content block"
        onclick={() => move(-1)}>‹</button>
      <button type="button" class="nav next" aria-label="Next node content block"
        onclick={() => move(1)}>›</button>
    {/if}
    <div class="fade" aria-hidden="true"></div>
  </div>

  <div class="carousel-bottom">
    {#if hasMore}
      <div class="dots" role="group" aria-label="Content block pages">
        {#each blocks as block, i (block.id)}
          <button type="button" aria-label={'Show node block ' + (i + 1)}
            aria-current={safeIndex === i ? 'true' : undefined}
            class:active={safeIndex === i}
            onclick={() => onIndexChange?.(i)}></button>
        {/each}
      </div>
      <span class="hint">Swipe to explore this node</span>
    {:else}
      <span class="hint">Preview of this node</span>
    {/if}
    <button type="button" class="read-more" onclick={onOpenFull}>Read more</button>
  </div>
</section>

<style>
  .feed-block-carousel { width: 100%; min-width: 0; color: var(--atlas-text); }
  .summary-row { display: flex; align-items: center; justify-content: space-between;
    gap: .5rem; padding: 0 0 .4rem; font-size: .75rem; color: var(--atlas-muted); }
  .summary-row > span:first-child { font-weight: 600; }
  .viewport { position: relative; width: 100%; box-sizing: border-box;
    overflow: hidden; border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius); background: var(--atlas-surface);
    touch-action: pan-y;
  }
  .viewport.landscape { height: min(12rem, 50vw); min-height: 8rem; }
  .viewport.portrait { height: min(17rem, 77vw); min-height: 11rem; }
  .feed-block-slide { box-sizing: border-box; display: flex; flex-direction: column;
    width: 100%; height: 100%; padding: .65rem 2.75rem; overflow: hidden; }
  .block-kind { font-size: .69rem; font-weight: 700; text-transform: uppercase;
    letter-spacing: .025em; color: var(--atlas-muted); flex: 0 0 auto; margin-bottom: .35rem; }
  .block-preview { min-height: 0; flex: 1 1 auto; overflow: hidden; position: relative; }
  /* Use the existing Atlas block renderer; only constrain its FEED viewport.
     A long Markdown section is still one slide and one original content block. */
  :global(.feed-block-slide .content-blocks) {
    max-width: 100%; min-width: 0; width: 100%; height: 100%; overflow: hidden;
  }
  :global(.feed-block-slide .content-blocks figure) {
    margin: 0; height: 100%; min-height: 0;
    display: flex; flex-direction: column; gap: .25rem;
  }
  :global(.feed-block-slide .content-blocks img) {
    display: block; flex: 1 1 auto; min-height: 0;
    max-width: 100%; width: 100%; height: 100%;
    object-fit: contain;
  }
  :global(.feed-block-slide .content-blocks figcaption) {
    font-size: .76rem; color: var(--atlas-muted);
    overflow: hidden; white-space: nowrap; text-overflow: ellipsis;
  }
  :global(.feed-block-slide .content-blocks p) { margin-top: 0; }
  .fade { position: absolute; bottom: 0; left: 0; right: 0; height: 1.7rem;
    background: linear-gradient(transparent, var(--atlas-surface));
    pointer-events: none; }
  .nav { position: absolute; z-index: 3; top: 50%; transform: translateY(-50%);
    display: grid; place-items: center; width: 2rem; height: 2.5rem;
    padding: 0; border: 0; border-radius: .3rem;
    background: color-mix(in srgb, var(--atlas-surface) 92%, transparent);
    color: var(--atlas-text); cursor: pointer; font: 1.6rem/1 system-ui, sans-serif;
  }
  .previous { left: .25rem; } .next { right: .25rem; }
  .carousel-bottom { display: flex; align-items: center; justify-content: space-between;
    flex-wrap: wrap; gap: .35rem .6rem; padding: .5rem 0 .05rem; }
  .dots { display: flex; gap: .33rem; flex-wrap: wrap; align-items: center; }
  .dots button { width: .65rem; height: .65rem; border: 1px solid var(--atlas-muted);
    padding: 0; border-radius: 50%; background: transparent; cursor: pointer; }
  .dots button.active { background: var(--atlas-accent); border-color: var(--atlas-accent); }
  .hint { color: var(--atlas-muted); font-size: .73rem; margin-right: auto; }
  .read-more { margin-left: auto; border: 0; padding: .2rem 0;
    font: inherit; font-size: .85rem; font-weight: 600;
    background: transparent; color: var(--atlas-accent); cursor: pointer; }
  .empty { padding: 1.5rem 2.75rem; color: var(--atlas-muted); font-size: .83rem; }
  button:focus-visible, .viewport:focus-visible {
    outline: 2px solid var(--atlas-focus); outline-offset: 2px;
  }
</style>

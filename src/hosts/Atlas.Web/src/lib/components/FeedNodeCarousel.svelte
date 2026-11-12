<script lang="ts">
  import type { Snippet } from 'svelte';
  import ContentBlocks from './ContentBlocks.svelte';
  import ActionButton from './ActionButton.svelte';
  import type { ContentBlock } from '../api/nodePage';

  let {
    blocks, index, frame, onIndexChange, onOpenFull, renderBlock,
  }: {
    blocks: ContentBlock[];
    index?: number;
    frame?: 'portrait' | 'landscape';
    onIndexChange?: (next: number) => void;
    onOpenFull?: () => void;
    renderBlock?: Snippet<[ContentBlock]>;
  } = $props();

  let start: { x: number; y: number } | null = null;
  let current = $state(0);
  const safeIndex = $derived(Math.max(0, Math.min(index ?? current, blocks.length - 1)));
  const active = $derived(blocks[safeIndex]);
  const hasMore = $derived(blocks.length > 1);

  function select(next: number) {
    current = next;
    onIndexChange?.(next);
  }

  function move(direction: number) {
    if (!blocks.length) return;
    select((safeIndex + direction + blocks.length) % blocks.length);
  }

  function onTouchStart(event: TouchEvent) {
    // Don't turn child interactions into navigation gestures.
    const target = event.target;
    const touch = event.touches[0];
    start = touch && !(target instanceof Element && target.closest('button, a, input, video, textarea'))
      ? { x: touch.clientX, y: touch.clientY }
      : null;
  }

  function onTouchEnd(event: TouchEvent) {
    const touch = event.changedTouches[0];
    if (!start || !touch) { start = null; return; }
    const dx = touch.clientX - start.x;
    const dy = touch.clientY - start.y;
    start = null;
    if (Math.abs(dx) >= 45 && Math.abs(dx) > Math.abs(dy)) move(dx < 0 ? 1 : -1);
  }
</script>

<section class="feed-block-carousel" aria-label="Feed preview of node content blocks">
  <!-- svelte-ignore a11y_no_noninteractive_tabindex, a11y_no_noninteractive_element_interactions (The carousel region supports keyboard slide navigation.) -->
  <div class="viewport" class:compact={!frame} class:portrait={frame === 'portrait'} class:landscape={frame === 'landscape'}
    role="region" aria-label="Node content carousel, swipe or use arrows"
    aria-roledescription="carousel" tabindex={hasMore ? 0 : undefined}
    onkeydown={(event) => {
      if (event.target !== event.currentTarget) return;
      if (event.key === 'ArrowRight') { event.preventDefault(); move(1); }
      if (event.key === 'ArrowLeft') { event.preventDefault(); move(-1); }
    }}
    ontouchstart={onTouchStart} ontouchend={onTouchEnd}
    ontouchcancel={() => { start = null; }}>

    {#if active}
      <div class="feed-block-slide" role="group" aria-roledescription="slide"
        aria-label={'Block ' + (safeIndex + 1) + ' of ' + blocks.length}
        data-block-id={active.id} data-block-kind={active.kind}>
        <div class="block-preview">
          {#key active.id}
            {#if renderBlock}{@render renderBlock(active)}{:else}<ContentBlocks blocks={[active]} preview />{/if}
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
    <span class="progress" aria-live="polite">{blocks.length ? safeIndex + 1 : 0} of {blocks.length}</span>
    {#if hasMore}
      <div class="dots" class:scroll={!frame} role="group" aria-label="Content block pages">
        {#each blocks as block, i (block.id)}
          <button type="button" aria-label={'Show node block ' + (i + 1)}
            aria-current={safeIndex === i ? 'true' : undefined}
            class:active={safeIndex === i}
            onclick={() => select(i)}></button>
        {/each}
      </div>
    {/if}
    <div class="carousel-read-more read-more"><ActionButton label="Read more" disabled={!onOpenFull} onActivate={onOpenFull} /></div>
  </div>
</section>

<style>
  .feed-block-carousel { width: 100%; min-width: 0; color: var(--atlas-text); }
  /* No nested card/border: the slide IS the old short-description area
     of the Atlas node card. Only the outer node border should remain visible. */
  .viewport { position: relative; width: 100%; box-sizing: border-box;
    overflow: hidden; border: 0; border-radius: 0; background: transparent;
    touch-action: pan-y;
  }
  .viewport.compact { height: var(--atlas-feed-preview-height, 8.5rem); }
  .viewport.landscape { height: min(12rem, 50vw); min-height: 8rem; }
  .viewport.portrait { height: min(17rem, 77vw); min-height: 11rem; }
  .feed-block-slide { box-sizing: border-box; display: flex; flex-direction: column;
    width: 100%; height: 100%; padding: .25rem 1.7rem; overflow: hidden; }
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
  :global(.feed-block-slide .content-blocks figure img) {
    display: block; flex: 1 1 auto; min-height: 0;
    max-width: 100%; width: 100%; height: 100%;
    object-fit: contain;
  }
  /* Embedded Markdown images are part of the text slide, not a separate
     full-height image block. Text remains visible in the same card. */
  :global(.feed-block-slide :is(.story-markdown, .markdown) img) {
    max-height: 8rem;
    width: auto;
    object-fit: contain;
  }
  .compact :global(.markdown img) { width: min(42%, 7.25rem); }
  :global(.feed-block-slide .content-blocks figcaption) {
    font-size: .76rem; color: var(--atlas-muted);
    overflow: hidden; white-space: nowrap; text-overflow: ellipsis;
  }
  :global(.feed-block-slide .content-blocks p) { margin-top: 0; }
  .fade { position: absolute; bottom: 0; left: 0; right: 0; height: 1.7rem;
    background: linear-gradient(transparent, var(--atlas-surface));
    pointer-events: none; }
  /* Slim arrows live in the pale node-card gutters, not on the writing. */
  .nav { position: absolute; z-index: 3; top: 50%; transform: translateY(-50%);
    display: grid; place-items: center; width: 1.45rem; height: 2.5rem;
    padding: 0; border: 0; border-radius: .25rem;
    background: color-mix(in srgb, var(--atlas-bg) 55%, var(--atlas-surface));
    color: var(--atlas-text); cursor: pointer; font: 1.65rem/1 system-ui, sans-serif;
  }
  .viewport:not(:has(.nav)) .feed-block-slide { padding-inline: 0; }
  .previous { left: 0; } .next { right: 0; }
  .carousel-bottom { display: flex; align-items: center; justify-content: space-between;
    flex-wrap: wrap; gap: .35rem .6rem; padding: .5rem 0 .05rem; }
  .progress { font-size: .73rem; color: var(--atlas-muted); font-variant-numeric: tabular-nums; }
  .dots { display: flex; gap: .33rem; flex-wrap: wrap; align-items: center; }
  .dots button { width: .65rem; height: .65rem; border: 1px solid var(--atlas-muted);
    padding: 0; border-radius: 50%; background: transparent; cursor: pointer; }
  .dots.scroll { flex-wrap: nowrap; overflow-x: auto; flex: 0 1 auto; max-width: 45%; padding: 3px; }
  .dots button { flex-shrink: 0; }
  .dots button.active { background: var(--atlas-accent); border-color: var(--atlas-accent); }
  .carousel-read-more { margin-left: auto; --atlas-action-padding: .4rem .7rem; }
  .empty { padding: 1.5rem 1.7rem; color: var(--atlas-muted); font-size: .83rem; }
  button:focus-visible, .viewport:focus-visible {
    outline: 2px solid var(--atlas-focus); outline-offset: 2px;
  }
</style>

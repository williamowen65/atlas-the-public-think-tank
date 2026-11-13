<script lang="ts">
  import PhotoFrame from './PhotoFrame.svelte';
  import SectionBreak from './SectionBreak.svelte';
  import MarkdownContent from '../../src/lib/components/MarkdownContent.svelte';
  import NodeMediaGallery, { type GalleryMedia } from '../../src/lib/components/NodeMediaGallery.svelte';
  import { publicUrl } from '../../src/lib/api/nodePage';
  import { blockAt, columns, splitText, type Draft } from './sectionLayouts';

  let { draft, feed = false, onReadMore }: {
    draft: Draft;
    feed?: boolean;
    onReadMore: () => void;
  } = $props();

  let expanded = $state(false);
  let viewportHeight = $state(0);
  let contentHeight = $state(0);
  let gallery = $state<ReturnType<typeof NodeMediaGallery>>();
  const instance = $props.id();
  const truncated = $derived(feed && contentHeight > viewportHeight + 1);
  const photos = $derived(draft.sections.flatMap((section) => {
    const layout = draft.layouts.find(({ id }) => id === section.layoutId)!;
    return layout.slots.flatMap((slot) => {
      const block = blockAt(draft, section.id, slot.id);
      return block && block.kind !== 'text' ? block.photos.flatMap((photo) => {
        const src = publicUrl(photo.src);
        return src ? [{ ...photo, src }] : [];
      }) : [];
    });
  }));
  const media = $derived<GalleryMedia[]>(photos.map((photo) => ({ kind: 'image', ...photo })));
</script>

<div class="viewport" class:feed class:expanded class:truncated id={instance} bind:clientHeight={viewportHeight}>
  <div class="sections" bind:offsetHeight={contentHeight}>
    {#each draft.sections as section (section.id)}
      {@const layout = draft.layouts.find(({ id }) => id === section.layoutId)!}
      {#if section.breakBefore}<SectionBreak />{/if}
      {#if layout.slots.some((slot) => blockAt(draft, section.id, slot.id))}
        <section class="section" style:--columns={columns(layout)}>
          {#each layout.slots as slot (slot.id)}
            {@const block = blockAt(draft, section.id, slot.id)}
            {@const vertical = draft.assignments.find((entry) => entry.sectionId === section.id && entry.slotId === slot.id)?.vertical}
            <div class="slot" style:--area={slot.area} style:align-self={vertical ?? 'start'}
              style:text-align={block?.kind === 'text' ? block.alignment : undefined}>
              {#if block?.kind === 'text'}
                <MarkdownContent preserveHeadingLevels text={slot.continuations ? splitText(block.text)[0] : block.text} />
              {:else if block}
                <div class="photos" class:photo-grid={block.kind === 'gallery'}>
                  {#each block.photos as photo (photo.id)}
                    {@const src = publicUrl(photo.src)}
                    {#if src}
                      <figure style:width={(photo.frame?.width ?? 100) + '%'}>
                        <button type="button" aria-label={`View ${photo.alt || 'photo'}`}
                          onclick={(event) => void gallery?.open(photos.findIndex(({ id }) => id === photo.id), event.currentTarget)}>
                          <PhotoFrame {photo} {src} gallery={block.kind === 'gallery'} />
                        </button>
                        {#if photo.caption}<figcaption>{photo.caption}</figcaption>{/if}
                      </figure>
                    {/if}
                  {/each}
                </div>
              {/if}
            </div>
            {#if slot.continuations && block?.kind === 'text'}
              {#each slot.continuations as area, part (area)}
                {@const content = splitText(block.text, slot.continuations.length + 1)[part + 1]}
                {#if content}
                  <div class="slot continuation" style:--area={area} style:text-align={block.alignment}>
                    <MarkdownContent preserveHeadingLevels text={content} />
                  </div>
                {/if}
              {/each}
            {/if}
          {/each}
        </section>
      {/if}
    {/each}
  </div>
</div>
{#if truncated}<div class="cutoff" aria-label="More content follows">…</div>{/if}
{#if feed}
  <div class="feed-actions">
    {#if truncated || expanded}
      <button type="button" aria-expanded={expanded} aria-controls={instance}
        onclick={() => expanded = !expanded}>{expanded ? 'Collapse' : 'Expand'}</button>
    {/if}
    <button type="button" class="primary" onclick={onReadMore}>Read More</button>
  </div>
{/if}
{#if media.length}<NodeMediaGallery bind:this={gallery} items={media} />{/if}

<style>
  .viewport.feed { max-height: 19rem; overflow: clip; }
  .viewport.feed.expanded { max-height: 38rem; }
  .viewport.truncated { mask-image: linear-gradient(black calc(100% - 1.6rem), transparent); }
  .sections { display: grid; gap: clamp(1.2rem, 3cqi, 2.75rem); }
  .section { display: grid; grid-template-columns: var(--columns); gap: clamp(1rem, 3cqi, 2.25rem); align-items: start; }
  .slot { min-width: 0; grid-area: var(--area, auto); }
  .slot :global(.markdown > :first-child) { margin-block-start: 0; }
  .slot :global(.markdown > :last-child) { margin-block-end: 0; }
  .photos { display: grid; gap: .55rem; }
  .photo-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); }
  figure { margin: 0 auto; }
  figure button { display: block; width: 100%; padding: 0; border: 0; background: transparent; cursor: zoom-in; }
  figcaption, .cutoff { color: var(--atlas-muted); }
  figcaption { text-align: center; font-size: .85rem; margin-top: .25rem; }
  .cutoff { margin-block: -.4rem .35rem; position: relative; }
  .feed-actions { display: flex; flex-wrap: wrap; justify-content: flex-end; gap: .5rem; margin-top: .5rem; }
  .feed-actions button {
    font: inherit; padding: .5rem .85rem; border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius, .5rem); color: var(--atlas-text); background: var(--atlas-surface); cursor: pointer;
  }
  .feed-actions .primary { background: var(--atlas-accent); color: var(--atlas-on-accent, white); border-color: var(--atlas-accent); }
  @container (max-width: 37rem) {
    .section { grid-template-columns: minmax(0, 1fr); }
    .slot { grid-area: auto; }
    .slot:empty { display: none; }
    .viewport.feed { max-height: 15rem; }
    .viewport.feed.expanded { max-height: 30rem; }
  }
</style>

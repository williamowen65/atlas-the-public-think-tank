<script lang="ts">
  import GalleryLayout from './GalleryLayout.svelte';
  import WrappedImage from './WrappedImage.svelte';
  import PhotoFrame from './PhotoFrame.svelte';
  import SectionBreak from './SectionBreak.svelte';
  import MarkdownContent from '../../src/lib/components/MarkdownContent.svelte';
  import NodeMediaGallery, { type GalleryMedia } from '../../src/lib/components/NodeMediaGallery.svelte';
  import { publicUrl } from '../../src/lib/api/nodePage';
  import { blockAt, columns, sectionRows, rowWrap, type Draft, type Photo } from './sectionLayouts';

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
    return sectionRows(section, layout).flatMap(({ slots }) => slots).flatMap((slot) => {
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
      {#each sectionRows(section, layout) as row (row.id)}
        {@const wrap = rowWrap(draft, section, row.slots)}
        {#if row.slots.some((slot) => blockAt(draft, section.id, slot.id))}
          <section class="section" style:--columns={wrap ? "minmax(0, 1fr)" : columns(row.slots)}>
            {#each row.slots as slot (slot.id)}
              {@const block = blockAt(draft, section.id, slot.id)}
              {@const vertical = draft.assignments.find((entry) => entry.sectionId === section.id && entry.slotId === slot.id)?.vertical}
              {#if !wrap || block?.id !== wrap.imageId}
                <div class="slot" style:align-self={vertical ?? 'start'}
                  style:text-align={block?.kind === 'text' ? block.alignment : undefined}>
                  {#if block?.kind === 'text'}
                    {#snippet inset()}
                      {#if wrap}
                        <WrappedImage side={wrap.side} width={wrap.image.photos[0]?.frame?.width}>
                          {#each wrap.image.photos as photo (photo.id)}
                            {@render photoView(photo, false, 100)}
                          {/each}
                        </WrappedImage>
                      {/if}
                    {/snippet}
                    <MarkdownContent preserveHeadingLevels text={block.text} inset={wrap ? inset : undefined} insetAnchor={wrap?.anchor} />
                  {:else if block?.kind === 'gallery'}
                    {#snippet tile(photo: Photo)}{@render photoView(photo, true)}{/snippet}
                    <GalleryLayout photos={block.photos.filter((photo) => publicUrl(photo.src))} layout={block.galleryStyle} photo={tile} />
                  {:else if block}
                    {#each block.photos as photo (photo.id)}{@render photoView(photo, false)}{/each}
                  {/if}
                </div>
              {/if}
            {/each}
          </section>
        {/if}
      {/each}
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

{#snippet photoView(photo: Photo, inGallery: boolean, width = photo.frame?.width ?? 100)}
  {@const src = publicUrl(photo.src)}
  {#if src}
    <figure style:width={width + '%'}>
      <button type="button" aria-label={`View ${photo.alt || 'photo'}`}
        onclick={(event) => void gallery?.open(photos.findIndex(({ id }) => id === photo.id), event.currentTarget)}>
        <PhotoFrame {photo} {src} gallery={inGallery} />
      </button>
      {#if photo.caption}<figcaption>{photo.caption}</figcaption>{/if}
    </figure>
  {/if}
{/snippet}

<style>
  .viewport.feed { max-height: 19rem; overflow: clip; }
  .viewport.feed.expanded { max-height: 38rem; }
  .viewport.truncated { mask-image: linear-gradient(black calc(100% - 1.6rem), transparent); }
  .sections { display: grid; gap: clamp(1.2rem, 3cqi, 2.75rem); }
  .section { display: grid; grid-template-columns: var(--columns); gap: clamp(1rem, 3cqi, 2.25rem); align-items: start; }
  .slot { min-width: 0; }
  .slot :global(.markdown > :first-child) { margin-block-start: 0; }
  .slot :global(.markdown > :last-child) { margin-block-end: 0; }
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
    .slot:empty { display: none; }
    .viewport.feed { max-height: 15rem; }
    .viewport.feed.expanded { max-height: 30rem; }
  }
</style>

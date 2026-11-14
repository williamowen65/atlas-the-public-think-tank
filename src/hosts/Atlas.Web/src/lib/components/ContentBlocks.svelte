<!--
  @component
  ContentBlocks: Renders an ordered list of supported content blocks.
  Handles images/links with URL validation and delegates Markdown to MarkdownContent.

  **Props**
  - `blocks` (`ContentBlock[]`; required): Ordered content blocks to render; follow the ContentBlock
    type definition for its fields.
-->
<script lang="ts">
  import NodeMediaGallery, { type GalleryMedia } from './NodeMediaGallery.svelte';
  import { markdownImages } from '../markdown/renderMarkdown';
  import { standaloneImageLayout } from '../api/contentAuthoring';
  import { imageLayoutStyle } from '../markdown/imageLayout';
  import MarkdownContent from './MarkdownContent.svelte';
  import { publicUrl, type ContentBlock } from '../api/nodePage';
  let {
    blocks,
    galleryBlocks = blocks,
    preview = false,
  }: {
    /** Ordered content blocks to render; follow the ContentBlock type definition for its fields. */
    blocks: ContentBlock[];
    galleryBlocks?: ContentBlock[];
    preview?: boolean;
  } = $props();

  let gallery = $state<ReturnType<typeof NodeMediaGallery>>();
  const media = $derived(galleryBlocks.flatMap((block) => {
    const kind = block.kind.toLowerCase();
    const src = publicUrl(block.content.url);
    const items: GalleryMedia[] = kind.includes('markdown') ? markdownImages(block.content.markdown ?? '').map((image) => ({ ...image, kind: 'image' }))
      : src && (kind.includes('image') || kind.includes('video'))
        ? [{ kind: kind.includes('video') ? 'video' : 'image', src,
          alt: block.content.altText ?? '', caption: block.content.caption ?? '' }]
        : [];
    return items.map((image, index) => ({ ...image, blockId: block.id, index }));
  }));

  function showMedia(id: string, index: number, trigger: HTMLButtonElement) {
    const position = media.findIndex((image) => image.blockId === id && image.index === index);
    if (position >= 0) void gallery?.open(position, trigger);
  }
</script>

<!--
  Layout: An ordered content region chooses a renderer for each block: Markdown, captioned
  image, link or plain fallback.
-->

<section aria-label="Node content" class="content-blocks">
  <!-- Match supported block kinds; unsupported interactive kinds retain a plain fallback. -->
  {#each blocks as block (block.id)}
    {#if block.kind.toLowerCase().includes('markdown')}<MarkdownContent
        images
        {preview}
        onImage={(index, trigger) => showMedia(block.id, index, trigger)}
        text={block.content.markdown ?? ''}
      />
    {:else if block.kind.toLowerCase().includes('image') && publicUrl(block.content.url)}
      {@const layout = standaloneImageLayout(block.content.layout)}
      <figure data-image-placement={layout.placement} style={imageLayoutStyle(layout)}>
        <button type="button" class="standalone-image" aria-label={`View ${block.content.altText || 'image'}`}
          onclick={(event) => showMedia(block.id, 0, event.currentTarget)}>
        <img
          src={publicUrl(block.content.url)}
          alt={block.content.altText ?? ''}
          loading="lazy"
          referrerpolicy="no-referrer"
        /></button>{#if block.content.caption}<figcaption>{block.content.caption}</figcaption>{/if}
      </figure>
    {:else if block.kind.toLowerCase().includes('video') && publicUrl(block.content.url)}
      <p><button type="button" onclick={(event) => showMedia(block.id, 0, event.currentTarget)}>
        {block.content.caption || 'View video'}
      </button></p>
    {:else if publicUrl(block.content.url)}<p>
        {#if preview}{block.content.title ?? block.content.caption ?? 'Linked content'}
        {:else}<a href={publicUrl(block.content.url)} rel="noopener noreferrer"
          >{block.content.title ?? block.content.caption ?? 'Open linked content'}</a>{/if}
        {block.content.description ?? ''}
      </p>
    {:else}<p>{block.content.title ?? block.kind} content</p>{/if}
  {:else}<p>No content blocks yet.</p>{/each}
</section>

{#if media.length}<NodeMediaGallery bind:this={gallery} items={media} />{/if}

<style>
  .content-blocks {
    min-width: 0;
  }
  figure {
    margin: 1rem 0;
  }
  .standalone-image {
    display: block;
    width: var(--atlas-image-width, 50%);
    padding: 0;
    border: 0;
    background: transparent;
    cursor: zoom-in;
  }
  figure[data-image-placement='center'] .standalone-image { margin-inline: auto; }
  figure[data-image-placement='right'] .standalone-image { margin-inline-start: auto; }
  .standalone-image img {
    display: block;
    width: 100%;
    height: auto;
    border-radius: var(--atlas-radius);
  }
</style>

<!--
  ContentBlocks: Renders an ordered list of supported content blocks.
  Handles images/links with URL validation and delegates Markdown to MarkdownContent.
-->
<script lang="ts">
  import MarkdownContent from './MarkdownContent.svelte';
  import { publicUrl, type ContentBlock } from '../api/nodePage';
  let { blocks }: { blocks: ContentBlock[] } = $props();
</script>

<section aria-label="Node content" class="content-blocks">
  <!-- Match supported block kinds; unsupported interactive kinds retain a plain fallback. -->
  {#each blocks as block (block.id)}
    {#if block.kind.toLowerCase().includes('markdown')}<MarkdownContent
        text={block.content.markdown ?? ''}
      />
    {:else if block.kind.toLowerCase().includes('image') && publicUrl(block.content.url)}<figure>
        <img
          src={publicUrl(block.content.url)}
          alt={block.content.altText ?? ''}
          loading="lazy"
        />{#if block.content.caption}<figcaption>{block.content.caption}</figcaption>{/if}
      </figure>
    {:else if publicUrl(block.content.url)}<p>
        <a href={publicUrl(block.content.url)} rel="noopener noreferrer"
          >{block.content.title ?? block.content.caption ?? 'Open linked content'}</a
        >
        {block.content.description ?? ''}
      </p>
    {:else}<p>{block.content.title ?? block.kind} content</p>{/if}
  {:else}<p>No content blocks yet.</p>{/each}
</section>

<style>
  .content-blocks {
    min-width: 0;
  }
  figure {
    margin: 1rem 0;
  }
  img {
    max-width: 100%;
    height: auto;
  }
</style>

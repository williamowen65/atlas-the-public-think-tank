<!-- Lazy, bounded feed excerpt from the actual ordered Content document.
     A discovery summary remains a fallback while a document loads or is unavailable. -->
<script lang="ts">
  import { onMount } from 'svelte';
  import { publicUrl, readJson, type ContentBlock } from '../api/nodePage';

  let { nodeId, fallback = '', apiBase = '/atlas-public-api', hidden = false }: {
    nodeId: string;
    fallback?: string;
    apiBase?: string;
    hidden?: boolean;
  } = $props();

  let host: HTMLSpanElement;
  let blocks = $state<ContentBlock[]>([]);
  let loaded = $state(false);
  const plainText = (markdown: string) =>
    markdown.replace(/!\[[^\]]*\]\([^)]*\)/g, '')
      .replace(/\[([^\]]+)\]\([^)]*\)/g, '$1')
      .replace(/[#*_~\x60>]/g, '').replace(/\s+/g, ' ').trim();

  const display = $derived.by(() => {
    const eligible = blocks.filter((block) => block.kind === 'markdown'
      ? !!block.content.markdown?.trim()
      : block.kind === 'image' && !!publicUrl(block.content.url));
    const first = eligible[0];
    if (!first) return { text: fallback, image: undefined as ContentBlock | undefined, kind: 'text' };
    const second = eligible[1];
    // When the node starts with an image, present it first, with a short optional follow-on line.
    if (first.kind === 'image') return {
      text: second?.kind === 'markdown' ? plainText(second.content.markdown ?? '') : (first.content.caption ?? ''),
      image: first,
      kind: 'image',
    };
    return {
      text: plainText(first.content.markdown ?? ''),
      image: second?.kind === 'image' ? second : undefined,
      kind: 'text',
    };
  });

  onMount(() => {
    if (hidden) return;
    const controller = new AbortController();
    let started = false;
    async function load() {
      if (started) return;
      started = true;
      try {
        const result = await readJson<ContentBlock[]> (apiBase, `nodes/${nodeId}/description`, controller.signal);
        if (!controller.signal.aborted && Array.isArray(result)) {
          blocks = result;
          loaded = true;
        }
      } catch { /* Existing discovery excerpt remains visible. */ }
    }
    if (typeof IntersectionObserver === 'undefined') void load();
    else {
      const observer = new IntersectionObserver((entries) => {
        if (entries.some((entry) => entry.isIntersecting)) {
          observer.disconnect();
          void load();
        }
      }, { rootMargin: '120px' });
      observer.observe(host);
      return () => { observer.disconnect(); controller.abort(); };
    }
    return () => controller.abort();
  });
</script>

<span class="feed-block-preview" class:image-first={display.kind === 'image'} bind:this={host}
  aria-label="Content preview">
  {#if display.image && publicUrl(display.image.content.url)}
    <img class="thumbnail" src={publicUrl(display.image.content.url)}
      alt={display.image.content.altText ?? ''} loading="lazy"/>
  {/if}
  {#if display.text}
    <span class="excerpt">{display.text}</span>
  {:else if !display.image}
    <span class="excerpt">{loaded ? 'Open node to view its content.' : fallback}</span>
  {/if}
</span>

<style>
  .feed-block-preview { display: block; max-height: var(--atlas-feed-preview-height, 8rem); overflow: hidden; line-height: var(--atlas-line-height, 1.65); }
  .excerpt { display: -webkit-box; -webkit-box-orient: vertical; -webkit-line-clamp: 4; line-clamp: 4; overflow: hidden; white-space: normal; overflow-wrap: anywhere; }
  .thumbnail { display: block; max-width: 100%; width: min(100%, 13rem); height: min(7rem, 32vw); object-fit: cover; border-radius: var(--atlas-radius); margin: .2rem 0; }
  .feed-block-preview:not(.image-first) .thumbnail { float: right; width: min(34%, 7rem); height: 5rem; margin: .15rem 0 .3rem .7rem; }
  .image-first .thumbnail { width: 100%; max-width: 15rem; }
  .image-first .excerpt { -webkit-line-clamp: 1; line-clamp: 1; }
  @media (max-width: 560px) {
    .feed-block-preview { max-height: 7rem; }
    .thumbnail { height: 5rem; }
  }
</style>
<script lang="ts">
  import type { Snippet } from 'svelte';
  import { readJson, publicUrl, type ContentBlock } from '../api/nodePage';
  import FeedNodeCarousel from './FeedNodeCarousel.svelte';

  let { nodeId, fallback, renderFallback, apiBase = '/atlas-public-api', blocks, onReadMore }: {
    nodeId: string;
    fallback: string;
    renderFallback: Snippet<[string]>;
    apiBase?: string;
    blocks?: ContentBlock[];
    onReadMore?: () => void;
  } = $props();

  let host = $state<HTMLDivElement>();
  let loaded = $state<ContentBlock[]>([]);
  const pages = $derived((blocks ?? loaded).filter((block) => {
    if (block.kind === 'markdown') return !!block.content.markdown?.trim();
    if (block.kind === 'image') return !!publicUrl(block.content.url);
    return true;
  }));

  $effect(() => {
    const id = nodeId;
    const base = apiBase;
    const element = host;
    if (blocks !== undefined || !element) return;
    loaded = [];
    const controller = new AbortController();
    let started = false;
    async function load() {
      if (started) return;
      started = true;
      try {
        const result = await readJson<ContentBlock[]>(base, `nodes/${id}/description`, controller.signal);
        if (!controller.signal.aborted && Array.isArray(result)) loaded = result;
      } catch { /* The discovery excerpt remains visible. */ }
    }

    if (typeof IntersectionObserver === 'undefined') {
      void load();
      return () => controller.abort();
    }

    const observer = new IntersectionObserver((entries) => {
      if (!entries.some((entry) => entry.isIntersecting)) return;
      observer.disconnect();
      void load();
    }, { rootMargin: '120px' });
    observer.observe(element);
    return () => { observer.disconnect(); controller.abort(); };
  });
</script>

<div class="feed-block-preview" class:has-slides={pages.length > 0} bind:this={host}>
  {#if pages.length}
    {#key nodeId}
      <FeedNodeCarousel blocks={pages} onOpenFull={onReadMore} />
    {/key}
  {:else}
    {@render renderFallback(fallback)}
  {/if}
</div>

<style>
  .feed-block-preview { display: block; min-width: 0; }
</style>

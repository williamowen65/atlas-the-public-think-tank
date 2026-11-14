<script lang="ts">
  import type { Snippet } from 'svelte';
  import { readJson, publicUrl, type ContentBlock } from '../api/nodePage';
  import SectionNodeView from '../sections/SectionNodeView.svelte';
  import { readComposition } from '../sections/composition';
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
  let settled = $state(false);
  const content = $derived(blocks ?? loaded);
  const composition = $derived(readComposition(content.find(({ kind }) => kind === 'sectioned')?.content.composition));
  const pages = $derived(content.filter((block) => {
    if (block.kind === 'markdown') return !!block.content.markdown?.trim();
    if (block.kind === 'image') return !!publicUrl(block.content.url);
    return block.kind !== 'sectioned';
  }));

  $effect(() => {
    const id = nodeId;
    const base = apiBase;
    const element = host;
    if (blocks !== undefined || !element) return;
    loaded = [];
    settled = false;
    const controller = new AbortController();
    let started = false;
    async function load() {
      if (started) return;
      started = true;
      try {
        const result = await readJson<ContentBlock[]>(base, `nodes/${id}/description`, controller.signal);
        if (!controller.signal.aborted && Array.isArray(result)) loaded = result;
      } catch { /* Use the discovery excerpt if content is unavailable. */ }
      finally { if (!controller.signal.aborted) settled = true; }
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

<div class="feed-block-preview" class:has-sections={!!composition} class:has-slides={!composition && pages.length > 0} bind:this={host}>
  {#if composition}
    {#key nodeId}
      <SectionNodeView draft={{ ...composition, title: '' }} feed {onReadMore} />
    {/key}
  {:else if pages.length}
    {#key nodeId}
      <FeedNodeCarousel blocks={pages} onOpenFull={onReadMore} />
    {/key}
  {:else if blocks !== undefined || settled}
    {@render renderFallback(fallback)}
  {:else}
    <div class="loading" aria-busy="true"></div>
  {/if}
</div>

<style>
  .loading { min-block-size: 5rem; }
  .feed-block-preview { display: block; min-width: 0; }
</style>

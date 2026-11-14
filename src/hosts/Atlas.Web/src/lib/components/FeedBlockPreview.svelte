<script lang="ts">
  import type { Snippet } from 'svelte';
  import { readJson, publicUrl, type ContentBlock } from '../api/nodePage';
  import { readComposition, type Composition } from '../sections/composition';
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
  function sectionPages(block: ContentBlock, composition: Composition): ContentBlock[] {
    return composition.sections.flatMap((section) => {
      const assignments = composition.assignments.filter(({ sectionId }) => sectionId === section.id);
      if (!assignments.length) return [];
      const ids = new Set(assignments.map(({ blockId }) => blockId));
      return [{
        ...block, id: `${block.id}/${section.id}`,
        content: { composition: JSON.stringify({
          ...composition, sections: [section], assignments, blocks: composition.blocks.filter(({ id }) => ids.has(id)),
        }) },
      }];
    });
  }

  const pages = $derived((blocks ?? loaded).flatMap((block) => {
    if (block.kind === 'markdown' && !block.content.markdown?.trim()) return [];
    if (block.kind === 'image' && !publicUrl(block.content.url)) return [];
    if (block.kind === 'sectioned') {
      const composition = readComposition(block.content.composition);
      return composition ? sectionPages(block, composition) : [];
    }
    return [block];
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

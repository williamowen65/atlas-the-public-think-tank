<script lang="ts">
  import BlockComposer from '../../src/lib/components/BlockComposer.svelte';
  import ContentBlocks from '../../src/lib/components/ContentBlocks.svelte';
  import { contentForPreview, derivedExcerpt, type AuthorBlock } from '../../src/lib/api/contentAuthoring';

  let { initialBlocks, title = 'A better public square' }: { initialBlocks: AuthorBlock[]; title?: string } = $props();
  let blocks = $state<AuthorBlock[]>(initialBlocks.map((block) => ({ ...block })));
  let showFull = $state(true);
</script>

<div class="demo">
  <h2>{title}</h2>
  <p>Change, add, remove, or reorder blocks. The full-node preview below uses the same ordered content.</p>
  <BlockComposer {blocks} onChange={(next) => blocks = next}/>
  <div class="preview">
    <h3>Compact text fallback (legacy API projection)</h3>
    <p class="excerpt">{derivedExcerpt(blocks, title)}</p>
    <button type="button" onclick={() => showFull = !showFull}>{showFull ? 'Hide' : 'Show'} full-node preview</button>
    {#if showFull}<ContentBlocks blocks={contentForPreview(blocks)}/>{/if}
  </div>
</div>
<style>
  .demo { max-width: 48rem; margin-inline: auto; padding: 1rem; }
  .preview { border: 1px solid var(--atlas-border); border-radius: var(--atlas-radius); padding: 1rem; margin-top: 1.5rem; }
  .excerpt { line-height: 1.5; display: -webkit-box; -webkit-box-orient: vertical; -webkit-line-clamp: 4; overflow: hidden; max-height: 6rem; }
  button { font: inherit; padding: .4rem .7rem; }
</style>

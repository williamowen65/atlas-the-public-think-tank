<script lang="ts">
  import BlockComposer from '../../src/lib/components/BlockComposer.svelte';
  import ContentBlocks from '../../src/lib/components/ContentBlocks.svelte';
  import { contentForPreview, type AuthorBlock } from '../../src/lib/api/contentAuthoring';
  import TiptapCanvasDemo from './TiptapCanvasDemo.svelte';
  import FeedNodeCarousel from './FeedNodeCarousel.svelte';

  let {
    initialBlocks,
    title = 'A better public square',
    initialMode = 'tiptap',
    initialStyle = 'atlas',
    initialNarrow = false,
    initialBreadcrumb = true,
  }: {
    initialBlocks: AuthorBlock[];
    title?: string;
    initialMode?: 'blocks' | 'tiptap';
    initialStyle?: 'atlas' | 'editorial';
    initialNarrow?: boolean;
    initialBreadcrumb?: boolean;
  } = $props();

  // The SAME Atlas block array is read and written by both editors.
  // Switching editors remounts Tiptap from this canonical source of truth.
  // We are testing whether keys, order and content survive the round trip.
  let blocks = $state<AuthorBlock[]>(initialBlocks.map(block => ({ ...block })));
  let titleDraft = $state(title);
  let nodeType = $state('Idea');
  let community = $state('Urban Design');
  let showBreadcrumb = $state(initialBreadcrumb);
  let mode = $state<'blocks' | 'tiptap'>(initialMode);
  let styleMode = $state<'atlas' | 'editorial'>(initialStyle);
  let view = $state<'edit' | 'feed' | 'full'>('edit');
  let narrow = $state(initialNarrow);
  let previewBlocks = $derived(contentForPreview(blocks));

  // Feed is only a VIEW over the SAME flat Atlas node content blocks.
  // It does not add a carousel kind, nested blocks, media data or API writes.
  // Track selected feed slide by block ID, so reordering preserves the target.
  let feedBlockKey = $state<string | null>(null);
  let feedIndex = $derived(Math.max(0, previewBlocks.findIndex(
    block => block.id === feedBlockKey
  )));
  let feedFrame = $state<'portrait' | 'landscape'>('landscape');

</script>

<div class="workbench" class:phone-width={narrow}>
  <div class="workbench-header">
    <div>
      <p class="eyebrow">Atlas · Storybook-only workspace</p>
      <h1>Node authoring workbench</h1>
      <p>One shared block document. Change either editor, then switch to see the result.</p>
    </div>
    <button type="button" class="width-toggle" aria-pressed={narrow} onclick={() => narrow = !narrow}>
      {narrow ? 'Expand to desktop' : 'Simulate phone width'}
    </button>
  </div>

  <div class="workbench-controls" aria-label="Prototype controls">
    <div class="control-group">
      <strong>Editor</strong>
      <div class="switcher" role="group" aria-label="Editor approach">
        <button type="button" aria-pressed={mode === 'blocks'} class:chosen={mode === 'blocks'} onclick={() => mode = 'blocks'}>Existing block forms</button>
        <button type="button" aria-pressed={mode === 'tiptap'} class:chosen={mode === 'tiptap'} onclick={() => mode = 'tiptap'}>Tiptap canvas</button>
      </div>
    </div>
    <div class="control-group">
      <strong>View</strong>
      <div class="switcher" role="group" aria-label="Editor and previews">
        <button type="button" aria-pressed={view === 'edit'} class:chosen={view === 'edit'} onclick={() => view = 'edit'}>Edit</button>
        <button type="button" aria-pressed={view === 'feed'} class:chosen={view === 'feed'} onclick={() => view = 'feed'}>Feed</button>
        <button type="button" aria-pressed={view === 'full'} class:chosen={view === 'full'} onclick={() => view = 'full'}>Full node</button>
      </div>
    </div>
    {#if view === 'feed'}
      <div class="control-group">
        <strong>Feed preview shape · research only</strong>
        <div class="switcher" role="group" aria-label="Feed slide framing">
          <button type="button" class:chosen={feedFrame === 'landscape'}
            aria-pressed={feedFrame === 'landscape'}
            onclick={() => feedFrame = 'landscape'}>Landscape</button>
          <button type="button" class:chosen={feedFrame === 'portrait'}
            aria-pressed={feedFrame === 'portrait'}
            onclick={() => feedFrame = 'portrait'}>Portrait</button>
        </div>
      </div>
    {/if}
    {#if mode === 'tiptap'}
      <div class="control-group">
        <strong>Text style</strong>
        <div class="switcher" role="group" aria-label="Tiptap visual design">
          <button type="button" aria-pressed={styleMode === 'atlas'} class:chosen={styleMode === 'atlas'} onclick={() => styleMode = 'atlas'}>Atlas</button>
          <button type="button" aria-pressed={styleMode === 'editorial'} class:chosen={styleMode === 'editorial'} onclick={() => styleMode = 'editorial'}>Editorial</button>
        </div>
      </div>
    {/if}
  </div>

  <div class="node-area">
    <article class="node-frame" class:feed-frame={view === 'feed'} aria-label={view === 'edit' ? 'Node editing card' : view === 'feed' ? 'Compact feed preview' : 'Full node preview'}>
      {#if showBreadcrumb}
        <nav class="breadcrumb" aria-label="Node ancestry">
          <span>Neighborhood ideas</span>
          <span aria-hidden="true">›</span>
          <span>Better streets</span>
        </nav>
      {/if}

      <header class="node-header">
        {#if view === 'edit'}
          <h2 class="editable-title">
            <select id="workbench-type" class="node-type-select" aria-label="Node type" bind:value={nodeType}>
              <option>Idea</option>
              <option>Question</option>
              <option>Evidence</option>
            </select>
            <input aria-label="Node title" bind:value={titleDraft} placeholder="Give your node a title" maxlength="200"/>
          </h2>
        {:else}
          <h2><span class="type-prefix">{nodeType}:</span> {titleDraft || 'Untitled node'}</h2>
        {/if}

        <div class="byline">
          <span>By You</span>
          <span aria-hidden="true">·</span>
          <span>Draft</span>
          {#if view === 'edit'}
            <span aria-hidden="true">·</span>
            <label for="workbench-community">Community:</label>
            <select id="workbench-community" aria-label="Community" bind:value={community}>
              <option value="">No community selected</option>
              <option>Urban Design</option>
              <option>Neighborhood Planning</option>
              <option>Public Spaces</option>
            </select>
          {:else if community}
            <span aria-hidden="true">·</span>
            <span>Community: {community}</span>
          {/if}
        </div>

        {#if view === 'edit'}
          <div class="card-fields">
            <label class="breadcrumb-toggle">
              <input type="checkbox" bind:checked={showBreadcrumb}/>
              Show parent breadcrumb
            </label>
          </div>
        {/if}
      </header>

      <div class="node-content">
        {#if view === 'edit'}
          {#if mode === 'blocks'}
            <div class="forms-area"><BlockComposer {blocks} onChange={(next) => blocks = next}/></div>
          {:else}
            <TiptapCanvasDemo initialBlocks={blocks} {styleMode} onChange={(next) => blocks = next}/>
          {/if}
        {:else if view === 'feed'}
          <!-- Feed slides are derived DIRECTLY from the editable node blocks.
               No separately authored gallery and no inner carousel in Full Node. -->
          <FeedNodeCarousel blocks={previewBlocks} index={feedIndex} frame={feedFrame}
            onIndexChange={(next) => feedBlockKey = previewBlocks[next]?.id ?? null}
            onOpenFull={() => view = 'full'} />
        {:else}
          <ContentBlocks blocks={previewBlocks}/>
        {/if}
      </div>
    </article>
    {#if view === 'feed'}
      <p class="preview-note">Feed research: one bounded slide per real node block. Read more opens the normal full-node preview; this is not the production feed layout.</p>
    {/if}
  </div>

  <details class="model-inspector">
    <summary>Inspect Atlas blocks ({blocks.length}) · shared by both editors</summary>
    <p>This is the ordered AuthorBlock array. Each item retains its key and any existing server block ID, including when you move an image above text in the Tiptap canvas.</p>
    <p><strong>Feed carousel:</strong> {blocks.length} slides, exactly one per existing Atlas content block. The feed previews blocks without creating a carousel block or modifying their storage. Potential multi-image blocks remain a future model/design idea.</p>
    <ol>
      {#each blocks as block (block.key)}
        <li>
          <div class="model-entry">
            <strong>{block.kind === 'markdown' ? 'Text' : block.kind === 'image' ? 'Image' : block.kind}</strong>
            <code>{block.id || block.key}</code>
            <span>{block.kind === 'image' ? (block.caption || block.text || 'Untitled image') : block.text.slice(0, 150) || '(empty text)'}</span>
          </div>
        </li>
      {/each}
    </ol>
    <p>Research note: this round trip covers ordered text and image blocks. It does not yet preserve all possible Markdown syntax or implement production persistence.</p>
  </details>

  <footer class="workbench-foot">
    <strong>Try this:</strong> move an image or change a paragraph in Tiptap, switch to Existing block forms, and inspect the same content and order. Then change it in the forms and switch back.
    <span>Preview-only: no database writes, uploads, publishing, or voting.</span>
  </footer>
</div>

<style>
  .workbench { max-width: 65rem; margin: 0 auto; padding: 1.5rem; box-sizing: border-box; color: var(--atlas-text); }
  .workbench-header { display: flex; align-items: flex-start; justify-content: space-between; flex-wrap: wrap; gap: 1rem; margin-bottom: 1rem; }
  .workbench-header h1 { margin: .15rem 0 .3rem; font-size: clamp(1.4rem, 3vw, 1.95rem); line-height: 1.2; }
  .workbench-header p { margin: 0; color: var(--atlas-muted); }
  .workbench-header .eyebrow { text-transform: uppercase; letter-spacing: .07em; font-size: .72rem; color: var(--atlas-accent); font-weight: 700; }
  button, select, input { font: inherit; }
  button { cursor: pointer; }
  .width-toggle, .switcher button { border: 1px solid var(--atlas-border); border-radius: .4rem; color: var(--atlas-text); background: var(--atlas-surface); padding: .45rem .7rem; font-size: .85rem; }
  .workbench-controls { display: flex; flex-wrap: wrap; gap: .85rem 1.35rem; padding: .85rem 0; border-top: 1px solid var(--atlas-border); border-bottom: 1px solid var(--atlas-border); }
  .control-group { display: grid; gap: .35rem; }
  .control-group > strong { font-size: .72rem; color: var(--atlas-muted); text-transform: uppercase; letter-spacing: .04em; }
  .switcher { display: flex; flex-wrap: wrap; gap: .25rem; }
  .switcher .chosen { border-color: var(--atlas-accent); box-shadow: inset 0 0 0 1px var(--atlas-accent); font-weight: 700; }

  .node-area { max-width: 48rem; margin: 1.3rem auto; }
  .node-frame { border: 1px solid var(--atlas-border); border-radius: var(--atlas-radius); background: var(--atlas-surface); color: var(--atlas-text); overflow-wrap: anywhere; min-width: 0; }
  .node-frame .breadcrumb { display: flex; align-items: center; gap: .5rem; padding: .7rem 1.25rem; border-bottom: 1px solid var(--atlas-border); color: var(--atlas-muted); font-size: .83rem; }
  .node-header { padding: 1.1rem 1.25rem .3rem; }
  .node-header h2 { display: flex; flex-wrap: wrap; align-items: baseline; gap: .3rem; margin: .35rem 0 .4rem; font-size: clamp(1.25rem, 3.5vw, 1.75rem); line-height: 1.25; }
  .type-prefix { color: var(--atlas-accent); }
  .editable-title .node-type-select {
    flex: 0 0 auto; max-width: 100%;
    border: 1px solid var(--atlas-border); border-radius: .35rem;
    padding: .15rem .35rem; background: var(--atlas-surface);
    color: var(--atlas-accent); font: inherit; font-weight: 700;
    cursor: pointer;
  }
  .editable-title input { display: block; flex: 1 1 15rem; min-width: 0; width: 100%; border: 0; border-bottom: 1px solid transparent; padding: .05rem 0; color: var(--atlas-text); background: transparent; font: inherit; font-weight: inherit; line-height: inherit; }
  .editable-title input:hover, .editable-title input:focus { border-color: var(--atlas-border); }
  .byline { display: flex; align-items: center; flex-wrap: wrap; gap: .25rem .4rem; margin: .3rem 0 .75rem; color: var(--atlas-muted); font-size: .81rem; line-height: 1.5; }
  .byline select { min-width: 10rem; max-width: 100%; padding: .25rem .4rem; border: 1px solid var(--atlas-border); border-radius: .35rem; color: var(--atlas-text); background: var(--atlas-surface); }
  .byline label { white-space: nowrap; }
  .card-fields { display: flex; align-items: center; flex-wrap: wrap; gap: .5rem .6rem; color: var(--atlas-muted); font-size: .81rem; padding-bottom: .65rem; }
  .breadcrumb-toggle { display: flex; align-items: center; gap: .2rem; cursor: pointer; margin-left: auto; }
  .node-content { --editor-card-inset: 1.25rem; padding: .4rem 1.25rem 1.2rem; min-height: 10rem; }
  .feed-frame .node-content { min-height: 0; }
  /* Storybook-only WYSIWYG size parity: the existing single-image renderer
     otherwise expands portraits to a different size in Full Node. */
  :global(.node-frame:not(.feed-frame) .node-content .content-blocks figure img) {
    max-width: 100%; max-height: 24rem; width: auto; height: auto;
    object-fit: contain; border-radius: var(--atlas-radius);
  }
  .preview-note { color: var(--atlas-muted); font-size: .8rem; margin-top: .5rem; }
  .model-inspector { max-width: 48rem; margin: .7rem auto 1.2rem; border: 1px dashed var(--atlas-border); border-radius: .6rem; padding: .75rem 1rem; font-size: .82rem; }
  .model-inspector summary { cursor: pointer; font-weight: 700; }
  .model-inspector p { color: var(--atlas-muted); line-height: 1.5; }
  .model-inspector ol { display: grid; gap: .4rem; padding-left: 1.45rem; }
  .model-entry { display: grid; gap: .12rem; }
  .model-entry code { overflow-wrap: anywhere; font-size: .77rem; color: var(--atlas-muted); }
  .model-entry span { overflow-wrap: anywhere; }
  .workbench-foot { display: grid; gap: .35rem; padding-top: 1rem; border-top: 1px solid var(--atlas-border); font-size: .83rem; color: var(--atlas-muted); }
  .workbench-foot strong { color: var(--atlas-text); }
  button:focus-visible, select:focus-visible, input:focus-visible, summary:focus-visible { outline: 2px solid var(--atlas-focus); outline-offset: 2px; }
  .phone-width { max-width: 390px; padding: .7rem; border-inline: 1px dashed var(--atlas-border); }
  .phone-width .workbench-controls { display: grid; }
  .phone-width .node-area { margin: .8rem auto; }
  .phone-width .node-header { padding: .7rem .7rem .2rem; }
  .phone-width .node-frame .breadcrumb { padding: .55rem .7rem; }
  .phone-width .node-content { --editor-card-inset: .7rem; padding: .35rem .7rem .8rem; }
  .phone-width .editable-title { flex-direction: column; align-items: stretch; }
  .phone-width .editable-title .node-type-select { align-self: flex-start; }
  .phone-width .editable-title input { flex: 0 1 auto; width: 100%; }
  .phone-width .breadcrumb-toggle { margin-left: 0; }
  @media (max-width: 650px) {
    .workbench { padding: .8rem; }
    .node-area { margin: .8rem auto; }
    .node-header { padding: .8rem .75rem .25rem; }
    .node-frame .breadcrumb { padding: .6rem .75rem; }
    .node-content { --editor-card-inset: .75rem; padding: .35rem .75rem 1rem; }
    .editable-title { flex-direction: column; align-items: stretch; }
    .editable-title .node-type-select { align-self: flex-start; }
    .editable-title input { flex: 0 1 auto; width: 100%; }
  }
</style>

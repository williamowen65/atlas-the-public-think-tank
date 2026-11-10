<script lang="ts">
  import BlockComposer from '../../src/lib/components/BlockComposer.svelte';
  import ContentBlocks from '../../src/lib/components/ContentBlocks.svelte';
  import { contentForPreview, derivedExcerpt, type AuthorBlock } from '../../src/lib/api/contentAuthoring';
  import TiptapCanvasDemo from './TiptapCanvasDemo.svelte';

  let {
    initialBlocks,
    title = 'A better public square',
    initialMode = 'tiptap',
    initialStyle = 'atlas',
    initialNarrow = false,
  }: {
    initialBlocks: AuthorBlock[];
    title?: string;
    initialMode?: 'blocks' | 'tiptap';
    initialStyle?: 'atlas' | 'editorial';
    initialNarrow?: boolean;
  } = $props();

  // Separate scratchpads are intentional: this story compares authoring patterns.
  // Switching between them does not imply that a live Tiptap JSON document can
  // be converted losslessly to Atlas's current Markdown/image block records.
  const copyBlocks = () => initialBlocks.map(block => ({ ...block }));
  let titleDraft = $state(title);
  let mode = $state<'blocks' | 'tiptap'>(initialMode);
  let styleMode = $state<'atlas' | 'editorial'>(initialStyle);
  let view = $state<'edit' | 'feed' | 'full'>('edit');
  let narrow = $state(initialNarrow);
  let formBlocks = $state<AuthorBlock[]>(copyBlocks());
  let richBlocks = $state<AuthorBlock[]>(copyBlocks());
  let activeBlocks = $derived(mode === 'blocks' ? formBlocks : richBlocks);
  let previewBlocks = $derived(contentForPreview(activeBlocks));
  let fallback = $derived(derivedExcerpt(activeBlocks, titleDraft));
</script>

<div class="workbench" class:phone-width={narrow}>
  <header class="workbench-header">
    <div>
      <p class="eyebrow">Atlas · Storybook research</p>
      <h1>Node authoring workbench</h1>
      <p>Try the same starting content in two editors. Nothing here is saved to an API.</p>
    </div>
    <button type="button" class="width-toggle" aria-pressed={narrow} onclick={() => narrow = !narrow}>{narrow ? 'Expand to desktop' : 'Simulate phone width'}</button>
  </header>

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
    {#if mode === 'tiptap'}
      <div class="control-group">
        <strong>Canvas style</strong>
        <div class="switcher" role="group" aria-label="Tiptap visual design">
          <button type="button" aria-pressed={styleMode === 'atlas'} class:chosen={styleMode === 'atlas'} onclick={() => styleMode = 'atlas'}>Atlas</button>
          <button type="button" aria-pressed={styleMode === 'editorial'} class:chosen={styleMode === 'editorial'} onclick={() => styleMode = 'editorial'}>Editorial</button>
        </div>
      </div>
    {/if}
  </div>

  <div class="node-area">
    {#if view === 'edit'}
      <div class="title-control">
        <label for="workbench-node-title">Node title</label>
        <input id="workbench-node-title" bind:value={titleDraft} maxlength="200"/>
      </div>
    {/if}

    {#if mode === 'blocks'}
      {#if view === 'edit'}
        <div class="forms-area">
          <BlockComposer blocks={formBlocks} onChange={(next) => formBlocks = next}/>
        </div>
      {/if}
    {:else}
      <!-- Keep the editor mounted across preview toggles to preserve cursor and undo. -->
      <div class:hidden={view !== 'edit'}>
        <TiptapCanvasDemo {initialBlocks} {styleMode} onChange={(next) => richBlocks = next}/>
      </div>
    {/if}

    {#if view === 'feed'}
      <div class="preview-label">Compact feed-content concept · height constrained, not a pixel-exact live Atlas card</div>
      <article class="feed-preview" aria-label="Compact feed preview">
        <p class="node-type">Idea</p>
        <h2>{titleDraft || 'Untitled node'}</h2>
        <p class="byline">By Example Author · A community node</p>
        <div class="feed-content"><ContentBlocks blocks={previewBlocks}/></div>
        <p class="preview-foot">Read more</p>
      </article>
      <p class="preview-note">Legacy short-description fallback: “{fallback}”</p>
    {:else if view === 'full'}
      <div class="preview-label">Full node content concept · editing controls hidden</div>
      <article class="full-preview" aria-label="Full node preview">
        <p class="node-type">Idea</p>
        <h2>{titleDraft || 'Untitled node'}</h2>
        <p class="byline">By Example Author</p>
        <ContentBlocks blocks={previewBlocks}/>
      </article>
    {/if}
  </div>

  <footer>
    <strong>Compare the experience:</strong> Select text for formatting, press Enter and find the + menu on a blank line, insert an image, then try phone width and both previews.
    <span>Research only: independent editor drafts, approximate feed preview, no uploads or publishing, and no lossless conversion guarantee.</span>
  </footer>
</div>

<style>
  .workbench { max-width: 65rem; margin: 0 auto; padding: 1.5rem; color: var(--atlas-text); box-sizing: border-box; }
  .workbench-header { display: flex; gap: .8rem 1.5rem; flex-wrap: wrap; align-items: flex-start; justify-content: space-between; margin-bottom: 1rem; }
  .workbench-header h1 { margin: .15rem 0 .3rem; line-height: 1.2; font-size: clamp(1.45rem, 3vw, 2rem); }
  .workbench-header p { margin: 0; color: var(--atlas-muted); }
  .workbench-header .eyebrow { text-transform: uppercase; letter-spacing: .07em; font-size: .7rem; color: var(--atlas-accent); font-weight: 700; }
  .width-toggle, .switcher button { border: 1px solid var(--atlas-border); border-radius: .45rem; color: var(--atlas-text); background: var(--atlas-surface); padding: .45rem .7rem; font: inherit; font-size: .85rem; cursor: pointer; }
  .width-toggle { margin-top: .4rem; }
  .workbench-controls { display: flex; gap: .85rem 1.3rem; flex-wrap: wrap; padding: .9rem 0; border-top: 1px solid var(--atlas-border); border-bottom: 1px solid var(--atlas-border); }
  .control-group { display: grid; gap: .35rem; }
  .control-group > strong { font-size: .72rem; color: var(--atlas-muted); text-transform: uppercase; letter-spacing: .04em; }
  .switcher { display: flex; flex-wrap: wrap; gap: .25rem; }
  .switcher .chosen { border-color: var(--atlas-accent); box-shadow: 0 0 0 1px var(--atlas-accent); font-weight: 700; }
  .node-area { max-width: 48rem; margin: 1.5rem auto; min-height: 14rem; }
  .title-control { display: grid; gap: .35rem; margin-bottom: .75rem; }
  .title-control label { color: var(--atlas-muted); font-size: .8rem; }
  .title-control input { box-sizing: border-box; width: 100%; padding: .65rem .5rem; color: var(--atlas-text); background: var(--atlas-surface); border: 1px solid var(--atlas-border); border-radius: .35rem; font: inherit; font-size: 1.4rem; font-weight: 700; }
  .preview-label { margin: .65rem 0; color: var(--atlas-muted); font-size: .75rem; font-weight: 700; }
  .feed-preview, .full-preview { padding: 1.25rem; border: 1px solid var(--atlas-border); border-radius: var(--atlas-radius); background: var(--atlas-surface); overflow-wrap: anywhere; }
  .node-type { color: var(--atlas-accent); font-weight: 700; font-size: .8rem; margin: 0 0 .25rem; }
  .feed-preview h2, .full-preview h2 { margin: .2rem 0 .35rem; line-height: 1.2; }
  .byline { color: var(--atlas-muted); font-size: .82rem; margin: .25rem 0 1rem; }
  .feed-content { max-height: 12rem; overflow: hidden; position: relative; }
  .feed-content::after { content: ''; pointer-events: none; position: absolute; bottom: 0; left: 0; right: 0; height: 1.5rem; background: linear-gradient(transparent, var(--atlas-surface)); }
  .preview-foot { color: var(--atlas-accent); text-align: right; font-size: .82rem; margin: 0; }
  .preview-note { font-size: .8rem; color: var(--atlas-muted); overflow-wrap: anywhere; }
  footer { display: grid; gap: .35rem; padding-top: 1rem; border-top: 1px solid var(--atlas-border); font-size: .83rem; color: var(--atlas-muted); }
  footer strong { color: var(--atlas-text); }
  button:focus-visible, input:focus-visible { outline: 2px solid var(--atlas-focus); outline-offset: 2px; }
  .hidden { display: none; }
  .phone-width { max-width: 390px; padding: .75rem; border-left: 1px dashed var(--atlas-border); border-right: 1px dashed var(--atlas-border); }
  .phone-width .workbench-header { display: grid; }
  .phone-width .workbench-controls { display: grid; }
  .phone-width .node-area { margin: .7rem auto; }
  .phone-width .feed-preview, .phone-width .full-preview { padding: .75rem; }
  @media (max-width: 650px) {
    .workbench { padding: .8rem; }
    .node-area { margin: .85rem auto; }
  }
</style>

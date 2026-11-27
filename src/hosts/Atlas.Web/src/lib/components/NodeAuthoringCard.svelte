<script lang="ts">
  import type { Snippet } from 'svelte';
  import { contentForPreview, type AuthorBlock } from '../api/contentAuthoring';
  import { untrack } from 'svelte';
  import SectionLayoutWorkbench from '../sections/SectionLayoutWorkbench.svelte';
  import { fromMarkdown, readComposition, writeComposition } from '../sections/composition';
  import type { Draft, FeedPresentation } from '../sections/sectionLayouts';
  import BlockComposer from './BlockComposer.svelte';
  import LiveVotingDial from './LiveVotingDial.svelte';
  import ContentBlocks from './ContentBlocks.svelte';
  import NodeCardRibbonCorner from './node-card-versions/NodeCardRibbonCorner.svelte';
  import type { NodeCardData } from './node-card-versions/types';
  import TiptapCanvas from './TiptapCanvas.svelte';

  let { node, blocks, onChange, fields, actions, closeControl, disabled = false, average, totalVotes }: {
    node: NodeCardData;
    blocks: AuthorBlock[];
    onChange: (blocks: AuthorBlock[]) => void;
    fields: Snippet;
    actions?: Snippet;
    closeControl?: Snippet;
    disabled?: boolean;
    average?: number | null;
    totalVotes?: number;
  } = $props();

  let view = $state<'edit' | 'feed' | 'full'>('edit');
  let markdown = $state(false);
  let requiresMarkdown = $state(false);
  let sectionDraft = $state.raw<Draft | undefined>(untrack(() => {
    const saved = blocks.find(({ kind }) => kind === 'sectioned');
    const composition = readComposition(saved?.text);
    return composition ? { ...composition, title: '' } : undefined;
  }));
  let feedPresentation = $state<FeedPresentation>(untrack(() =>
    sectionDraft?.feedPresentation === 'title-only' ? 'title-only' : 'opening-content'));
  let feedHelp = $state(false);
  const feedHelpId = $props.id();
  let currentSectionDraft = untrack(() => sectionDraft);
  const canConvert = $derived(!sectionDraft && blocks.every(({ kind }) => kind === 'markdown'));
  let lastComposition = untrack(() => sectionDraft ? writeComposition(sectionDraft) : undefined);

  function useSections() {
    sectionDraft = fromMarkdown(blocks.map(({ text }) => text));
  }

  function updateSections(draft: Draft) {
    currentSectionDraft = draft;
    const text = writeComposition({ ...draft, feedPresentation });
    if (text === lastComposition) return;
    lastComposition = text;
    const original = blocks.find(({ kind }) => kind === 'sectioned');
    const sectioned: AuthorBlock = { key: original?.key ?? crypto.randomUUID(), id: original?.id, kind: 'sectioned', text, url: '', caption: '' };
    onChange(original ? blocks.map((block) => block === original ? sectioned : block) : [sectioned]);
  }

  const content = $derived(contentForPreview(blocks));
</script>

<div class="authoring-card" class:sectioned={!!sectionDraft && view === 'edit'}>
  <div class="view-controls" class:inline-controls={!!closeControl} role="group" aria-label="Node draft view">
    <button type="button" aria-pressed={view === 'edit'} onclick={() => view = 'edit'}>Edit</button>
    <button type="button" aria-pressed={view === 'feed'} onclick={() => view = 'feed'}>Preview in feed</button>
    {#if sectionDraft}
      <div class="feed-setting">
        <label>Show in feed
          <select value={feedPresentation} {disabled} onchange={(event) => {
            feedPresentation = event.currentTarget.value === 'title-only' ? 'title-only' : 'opening-content';
            if (currentSectionDraft) updateSections(currentSectionDraft);
          }}>
            <option value="opening-content">Title and opening content</option>
            <option value="title-only">Title only</option>
          </select>
        </label>
        <button class="feed-help-button" type="button" aria-label="About what appears in the feed"
          aria-expanded={feedHelp} aria-controls={feedHelpId} onclick={() => feedHelp = !feedHelp}>i</button>
      </div>
    {/if}
    {#if view === 'full'}
      <span class="full-label">Full node preview</span>
    {/if}
    {#if closeControl}<div class="close-control">{@render closeControl()}</div>{/if}
  </div>

  {#if sectionDraft}
    <p class="feed-help" id={feedHelpId} hidden={!feedHelp}>
      Opening content is the first section with text, an image, or a link. Title only hides the body in the feed. Read More opens the full node with either option.
    </p>
  {/if}

  {#if view === 'feed'}
    <NodeCardRibbonCorner {node} blocks={content} draft mobileLayout {average} {totalVotes}
      onReadMore={() => view = 'full'}>
      {#snippet voting(compact?: boolean)}
        <LiveVotingDial nodeId={node.id} initialAverage={average} initialTotalVotes={totalVotes}
          mobileCompact forceCompact={compact} preview />
      {/snippet}
    </NodeCardRibbonCorner>
  {:else if view === 'full'}
    <NodeCardRibbonCorner {node} draft mobileLayout authoringBody={fullContent} />
  {/if}

  <!-- Preserve the document, selection, and undo history while previewing. -->
  <fieldset class="editor" hidden={view !== 'edit'} disabled={disabled || view !== 'edit'}>
    <NodeCardRibbonCorner {node} draft mobileLayout headerFields={fields}
      authoringBody={editorContent} authoringFooter={actions} />
  </fieldset>
</div>

{#snippet editorContent()}
  {#if sectionDraft}
    <SectionLayoutWorkbench initialDraft={sectionDraft} onDraftChange={updateSections} embedded />
  {:else}
  {#if canConvert}<button type="button" onclick={useSections}>Use section editor</button>{/if}
  <label class="markdown-toggle">
    <input type="checkbox" checked={markdown} onchange={(event) => {
      requiresMarkdown = false;
      markdown = event.currentTarget.checked;
    }} /> Edit Markdown
  </label>
  {#if requiresMarkdown}
    <p class="format-note">This draft uses Markdown formatting that needs the Markdown editor.</p>
  {/if}
  {#if markdown}
    <BlockComposer {blocks} {onChange} {disabled} />
  {:else}
    <TiptapCanvas initialBlocks={blocks} {onChange} {disabled}
      onUnsupportedMarkdown={() => { requiresMarkdown = true; markdown = true; }} />
  {/if}
  {/if}
{/snippet}

{#snippet fullContent()}<ContentBlocks blocks={content} />{/snippet}

<style>
  .authoring-card { max-width: 48rem; margin-inline: auto; min-width: 0; }
  .authoring-card.sectioned { max-width: 78rem; }
  .sectioned .editor :global(.node-card) { max-width: none; }
  .editor { min-width: 0; border: 0; margin: 0; padding: 0; }
  .view-controls { display: flex; align-items: center; flex-wrap: wrap; gap: .4rem; margin-bottom: .6rem; }
  .inline-controls { font-size: .85rem; gap: .35rem; }
  .inline-controls > button { padding: .35rem .5rem; white-space: nowrap; }
  .inline-controls .feed-setting { min-width: 0; }
  .inline-controls .feed-setting select { max-width: 100%; }
  .close-control { margin-inline-start: auto; }
  .feed-setting, .feed-setting label { display: flex; align-items: center; flex-wrap: wrap; gap: .4rem; }
  .feed-setting select {
    max-width: 100%; padding: .35rem; border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius); background: var(--atlas-surface); color: inherit; font: inherit;
  }
  .feed-setting .feed-help-button {
    display: inline-grid; place-items: center; width: 1.5rem; aspect-ratio: 1;
    border-radius: 50%; padding: 0; color: var(--atlas-accent); font-weight: 600;
  }
  .feed-help { color: var(--atlas-muted); font-size: .875rem; margin: 0 0 .6rem; }
  button {
    font: inherit;
    background: var(--atlas-surface);
    color: var(--atlas-text);
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    padding: .35rem .7rem;
    cursor: pointer;
  }
  button[aria-pressed='true'] { border-color: var(--atlas-accent); color: var(--atlas-accent); }
  .full-label { padding: .35rem .25rem; }
  .markdown-toggle { display: flex; align-items: center; gap: .35rem; color: var(--atlas-muted); font-size: .85rem; margin-bottom: .65rem; }
  .format-note { font-size: .85rem; }
</style>

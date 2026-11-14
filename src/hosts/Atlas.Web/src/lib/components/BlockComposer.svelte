<!-- A single-column editor, not a free-form page grid. Every block has stable identity. -->
<script lang="ts">
  import MarkdownContent from './MarkdownContent.svelte';
  import { publicUrl } from '../api/nodePage';
  import { newAuthorBlock, supportedBlock, type AuthorBlock } from '../api/contentAuthoring';

  let {
    blocks, onChange, disabled = false,
  }: {
    blocks: AuthorBlock[];
    onChange: (blocks: AuthorBlock[]) => void;
    disabled?: boolean;
  } = $props();

  function modify(key: string, change: Partial<AuthorBlock>) {
    onChange(blocks.map((block) => block.key === key ? { ...block, ...change } : block));
  }
  function move(index: number, direction: -1 | 1) {
    const destination = index + direction;
    if (destination < 0 || destination >= blocks.length) return;
    const updated = [...blocks];
    [updated[index], updated[destination]] = [updated[destination], updated[index]];
    onChange(updated);
  }
  function add(kind: 'markdown' | 'image') {
    onChange([...blocks, newAuthorBlock(kind)]);
  }
</script>

<div class="block-composer" aria-label="Node content blocks">
  {#each blocks as block, index (block.key)}
    <section class="content-block" aria-label={`Block ${index + 1}: ${block.kind}`}>
      <div class="block-toolbar">
        <strong>Block {index + 1} · {block.kind === 'markdown' ? 'Text' : block.kind === 'image' ? 'Image' : block.kind}</strong>
        <div class="block-actions" aria-label={`Reorder block ${index + 1}`}>
          <button type="button" disabled={disabled || index === 0} onclick={() => move(index, -1)} aria-label={`Move block ${index + 1} up`}>↑</button>
          <button type="button" disabled={disabled || index === blocks.length - 1} onclick={() => move(index, 1)} aria-label={`Move block ${index + 1} down`}>↓</button>
          {#if supportedBlock(block)}
            <button type="button" disabled={disabled || blocks.length === 1} onclick={() => onChange(blocks.filter((entry) => entry.key !== block.key))} aria-label={`Remove block ${index + 1}`}>Remove</button>
          {/if}
        </div>
      </div>
      {#if block.kind === 'markdown'}
        <label for={`${block.key}-text`}>Text (Markdown)</label>
        <textarea id={`${block.key}-text`} value={block.text} disabled={disabled} rows="5" maxlength="32000"
          placeholder="Write your idea…" oninput={(event) => modify(block.key, { text: event.currentTarget.value })}></textarea>
        <details><summary>Preview text</summary><MarkdownContent images text={block.text}/></details>
      {:else if block.kind === 'image'}
        <label for={`${block.key}-url`}>Image URL (HTTP or HTTPS)</label>
        <input id={`${block.key}-url`} type="url" value={block.url} disabled={disabled} placeholder="https://…"
          oninput={(event) => modify(block.key, { url: event.currentTarget.value })}/>
        <label for={`${block.key}-alt`}>Alternative text (required)</label>
        <textarea id={`${block.key}-alt`} rows="2" value={block.text} maxlength="2000" disabled={disabled}
          placeholder="Describe what the image communicates"
          oninput={(event) => modify(block.key, { text: event.currentTarget.value })}></textarea>
        <label for={`${block.key}-caption`}>Caption (optional)</label>
        <input id={`${block.key}-caption`} value={block.caption} maxlength="2000" disabled={disabled}
          oninput={(event) => modify(block.key, { caption: event.currentTarget.value })}/>
        {#if publicUrl(block.url)}
          <figure><img src={publicUrl(block.url)} alt={block.text || 'Unsaved image preview'} loading="lazy"/>
            {#if block.caption}<figcaption>{block.caption}</figcaption>{/if}
          </figure>
        {/if}
      {:else}
        <p class="note">This existing {block.kind} block is preserved in its original form. Editing this type is not available yet.</p>
      {/if}
    </section>
  {/each}
  <div class="add-actions" aria-label="Add content block">
    <span>Add a block</span>
    <button type="button" disabled={disabled} onclick={() => add('markdown')}>+ Text</button>
    <button type="button" disabled={disabled} onclick={() => add('image')}>+ Image</button>
  </div>
  <p class="note">Blocks appear in this order on the full node. Atlas keeps feed previews compact; more block types will be added later.</p>
</div>

<style>
  .block-composer { display: grid; gap: .85rem; min-width: 0; }
  .content-block { border: 1px solid var(--atlas-border); border-radius: var(--atlas-radius); padding: .85rem; min-width: 0; }
  .block-toolbar { display: flex; justify-content: space-between; align-items: center; gap: .5rem; flex-wrap: wrap; margin-bottom: .65rem; }
  .block-actions, .add-actions { display: flex; align-items: center; gap: .4rem; flex-wrap: wrap; }
  .add-actions { border: 1px dashed var(--atlas-border); border-radius: var(--atlas-radius); padding: .75rem; }
  label { display: block; margin: .65rem 0 .2rem; font-size: .87rem; }
  textarea, input { box-sizing: border-box; width: 100%; max-width: 100%; padding: .6rem; border: 1px solid var(--atlas-border); border-radius: var(--atlas-radius); background: var(--atlas-surface); color: var(--atlas-text); font: inherit; }
  textarea { resize: vertical; }
  button { font: inherit; cursor: pointer; border-radius: var(--atlas-radius); padding: .35rem .65rem; border: 1px solid var(--atlas-border); color: var(--atlas-text); background: var(--atlas-surface); }
  button:disabled { opacity: .5; cursor: default; }
  button:focus-visible, input:focus-visible, textarea:focus-visible { outline: 2px solid var(--atlas-focus); outline-offset: 2px; }
  figure { margin: .75rem 0; }
  img { display: block; max-width: 100%; max-height: 16rem; width: auto; height: auto; border-radius: var(--atlas-radius); }
  figcaption { color: var(--atlas-muted); margin-top: .3rem; }
  .note { color: var(--atlas-muted); font-size: .85rem; }
  @media (max-width: 450px) { .content-block { padding: .6rem; } .block-toolbar { align-items: flex-start; } }
</style>

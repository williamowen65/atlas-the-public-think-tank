<script lang="ts">
  import { tick, untrack } from 'svelte';
  import { publicUrl } from '../../src/lib/api/nodePage';
  import SectionTextEditor from './SectionTextEditor.svelte';
  import type TiptapCanvas from '../../src/lib/components/TiptapCanvas.svelte';
  import type { AuthorBlock } from '../../src/lib/api/contentAuthoring';
  import SectionBreak from './SectionBreak.svelte';
  import SectionNodeView from './SectionNodeView.svelte';
  import {
    applyLayout, blockAt, columns, duplicateSection, example, moveBlock, newId,
    type Block, type Layout, type Photo, type Draft, type VerticalAlignment,
  } from './sectionLayouts';

  let { start = 'editorial' }: { start?: 'blank' | 'editorial' | 'photoEssay' } = $props();
  let draft = $state(untrack(() => example(start)));
  let view = $state<'edit' | 'feed' | 'node'>('edit');
  let narrow = $state(false);
  let layoutSection = $state<string | null>(null);
  let layoutChoice = $state('full');
  let customName = $state('');
  let customColumns = $state(2);
  let leftShare = $state(60);
  let message = $state('');
  let activeBlock = $state<string | null>(null);
  let movingBlock = $state<string | null>(null);
  let picker = $state<string | null>(null);
  let photoDetails = $state<string | null>(null);
  let activeEditor = $state<ReturnType<typeof TiptapCanvas>>();
  let workbench = $state<HTMLDivElement>();
  let moveButton: HTMLButtonElement | undefined;
  const formats = [
    ['bold', 'Bold', 'B'], ['italic', 'Italic', 'I'], ['link', 'Link', 'Link'],
    ['bulletList', 'Bulleted list', 'List'],
  ] as const;

  let history = $state.raw<Draft[]>([]);
  let editorVersion = $state(0);
  const selectedText = $derived(draft.blocks.find((block) => block.id === activeBlock && block.kind === 'text'));
  const selectedAssignment = $derived(draft.assignments.find(({ blockId }) => blockId === activeBlock));

  function remember() {
    history = [...history, structuredClone($state.snapshot(draft))];
  }

  function deleteEmptySection(sectionId: string) {
    if (draft.sections.length < 2 || draft.assignments.some((entry) => entry.sectionId === sectionId)) return;
    remember();
    draft.sections = draft.sections.filter(({ id }) => id !== sectionId);
    if (layoutSection === sectionId) layoutSection = null;
    message = 'Empty section deleted.';
  }

  function undo() {
    const previous = history.at(-1);
    if (!previous) return;
    history = history.slice(0, -1);
    draft = previous;
    editorVersion++;
    movingBlock = null;
    activeBlock = null;
    activeEditor = undefined;
    layoutSection = null;
    picker = null;
    photoDetails = null;
    message = 'Change undone.';
  }

  function authorText(block: Extract<Block, { kind: 'text' }>): AuthorBlock[] {
    return [{ key: block.id, kind: 'markdown', text: block.text, url: '', caption: '' }];
  }

  function selectView(next: typeof view) {
    view = next;
    movingBlock = null;
    activeBlock = null;
    activeEditor = undefined;
    picker = null;
    photoDetails = null;
  }

  async function startMove(id: string, button: HTMLButtonElement) {
    moveButton = button;
    activeBlock = id;
    movingBlock = id;
    picker = null;
    photoDetails = null;
    activeEditor = undefined;
    layoutSection = null;
    await tick();
    workbench?.querySelector<HTMLButtonElement>('.destination')?.focus();
  }

  function cancelMove() {
    movingBlock = null;
    moveButton?.focus();
  }

  async function placeBlock(sectionId: string, slotId: string) {
    const id = movingBlock;
    if (!id) return;
    const target = blockAt(draft, sectionId, slotId);
    remember();
    draft = moveBlock(draft, id, sectionId, slotId);
    movingBlock = null;
    activeBlock = id;
    message = target ? 'Blocks swapped.' : 'Block moved.';
    await tick();
    workbench?.querySelector<HTMLButtonElement>(`[data-block="${id}"] .move-block`)?.focus();
  }

  function openLayout(sectionId: string) {
    const section = draft.sections.find(({ id }) => id === sectionId);
    layoutSection = sectionId;
    activeEditor = undefined;
    layoutChoice = section?.layoutId ?? 'full';
    seedCustomLayout(layoutChoice);
    message = '';
  }

  function seedCustomLayout(layoutId: string) {
    const selected = draft.layouts.find(({ id }) => id === layoutId)!;
    customName = '';
    customColumns = selected.slots.length;
    leftShare = selected.slots.length === 2
      ? Math.round(selected.slots[0].weight / (selected.slots[0].weight + selected.slots[1].weight) * 100)
      : 60;
  }

  function applyChoice(layoutId: string, record = true) {
    if (record && (layoutSection === 'new' || draft.sections.some((item) => item.id === layoutSection && item.layoutId !== layoutId))) remember();
    if (layoutSection === 'new') {
      draft.sections.push({ id: newId(), layoutId });
    } else if (layoutSection) {
      const count = draft.sections.length;
      draft = applyLayout(draft, layoutSection, layoutId);
      if (draft.sections.length > count) message = 'Extra blocks moved to full-width sections immediately below.';
    }
    layoutSection = null;
  }

  function saveLayout(reusable: boolean) {
    const name = customName.trim();
    if (reusable && (!name || draft.layouts.some((layout) => layout.name.toLowerCase() === name.toLowerCase()))) return;
    const id = newId();
    const layout: Layout = {
      id, name: reusable ? name : 'Custom section layout',
      slots: customColumns === 1 ? [{ id: 'left', weight: 1 }]
        : [{ id: 'left', weight: leftShare }, { id: 'right', weight: 100 - leftShare }],
    };
    remember();
    draft.layouts.push({ ...layout, ...(reusable ? {} : { sectionOnly: true }) });
    applyChoice(layout.id, false);
  }

  function addBlock(sectionId: string, slotId: string, kind: Block['kind']) {
    if (blockAt(draft, sectionId, slotId)) return;
    const id = newId();
    remember();
    const photo = kind === 'image' ? newPhoto() : undefined;
    draft.blocks.push(kind === 'text' ? { id, kind, text: '' }
      : { id, kind, photos: photo ? [photo] : [] });
    draft.assignments.push({ sectionId, slotId, blockId: id });
    activeBlock = id;
    activeEditor = undefined;
    picker = null;
    photoDetails = photo?.id ?? null;
  }

  function newPhoto(): Photo {
    return { id: newId(), src: '', alt: '', caption: '' };
  }

  function removeBlock(blockId: string) {
    if (!window.confirm('Remove this block?')) return;
    remember();
    draft.assignments = draft.assignments.filter((entry) => entry.blockId !== blockId);
    draft.blocks = draft.blocks.filter(({ id }) => id !== blockId);
    message = 'Block removed.';
    activeBlock = null;
    activeEditor = undefined;
  }

  function moveSection(sectionId: string, direction: number) {
    const from = draft.sections.findIndex(({ id }) => id === sectionId);
    const to = from + direction;
    if (to < 0 || to >= draft.sections.length) return;
    activeEditor = undefined;
    remember();
    const sections = [...draft.sections];
    [sections[from], sections[to]] = [sections[to], sections[from]];
    draft.sections = sections;
  }
</script>

<svelte:window onpointerdowncapture={(event) => {
  if (view !== 'edit' || !activeBlock || !(event.target instanceof Element)) return;
  if (workbench?.contains(event.target) && (
    event.target.closest('.editing-tools') || event.target.closest('[data-block]')?.getAttribute('data-block') === activeBlock
  )) return;
  activeEditor?.dismissControls();
  activeEditor = undefined;
  activeBlock = null;
  photoDetails = null;
}} onkeydowncapture={(event) => {
  if (event.key === 'Escape' && movingBlock) cancelMove();
  if (view === 'edit' && history.length && !movingBlock && !event.defaultPrevented
    && (event.ctrlKey || event.metaKey) && !event.shiftKey && !event.altKey && event.key.toLowerCase() === 'z'
    && !(event.target instanceof Element && event.target.closest('.layout-panel'))) {
    event.preventDefault();
    event.stopPropagation();
    undo();
  }
}} />
<div class="workbench" class:narrow bind:this={workbench}>
  <div class="toolbar" role="group" aria-label="Workbench view">
    <button type="button" aria-pressed={view === 'edit'} onclick={() => selectView('edit')}>Edit</button>
    <button type="button" aria-pressed={view === 'feed'} onclick={() => selectView('feed')}>Feed preview</button>
    <button type="button" aria-pressed={view === 'node'} onclick={() => selectView('node')}>Node page</button>
    <label class="phone"><input type="checkbox" bind:checked={narrow} /> Phone width</label>
  </div>

  <article class="node">
    <header>
      {#if view === 'edit'}
        <input class="title" aria-label="Node title" placeholder="Give your node a title" value={draft.title} oninput={(event) => { remember(); draft.title = event.currentTarget.value; }} />
      {:else}
        <h1>{draft.title || 'Untitled node'}</h1>
      {/if}
    </header>

    {#if view === 'edit'}
      <div class="editing-tools">
        {#if movingBlock}
          <span role="status">Choose a slot to move or swap.</span>
          <button type="button" onclick={cancelMove}>Cancel move</button>
          <button type="button" disabled={!history.length} onclick={undo}>Undo</button>
        {:else}
          <div class="formatting" role="group" aria-label="Text formatting">
            {#each formats as [command, label, short]}
              <button type="button" aria-label={label} title={label} disabled={!activeEditor}
                onpointerdown={(event) => event.preventDefault()}
                onclick={() => activeEditor?.formatText(command)}>{short}</button>
            {/each}
            <label>Heading
              <select aria-label="Heading level" disabled={!activeEditor} onchange={(event) => {
                const level = Number(event.currentTarget.value) as 2 | 3 | 4 | 5 | 6;
                activeEditor?.formatText('heading', level);
                event.currentTarget.value = '';
              }}>
                <option value="" disabled selected>Choose heading</option>
                {#each [2, 3, 4, 5, 6] as level}<option value={level}>H{level}</option>{/each}
              </select>
            </label>
            <label>Text alignment
              <select disabled={selectedText?.kind !== 'text'} value={selectedText?.kind === 'text' ? selectedText.alignment ?? 'left' : 'left'}
                onchange={(event) => {
                  if (selectedText?.kind === 'text') { remember(); selectedText.alignment = event.currentTarget.value as 'left' | 'center' | 'right'; }
                }}>
                <option value="left">Left</option><option value="center">Center</option><option value="right">Right</option>
              </select>
            </label>
            <label>Vertical alignment
              <select disabled={!selectedAssignment} value={selectedAssignment?.vertical ?? 'start'}
                onchange={(event) => {
                  if (selectedAssignment) { remember(); selectedAssignment.vertical = event.currentTarget.value as VerticalAlignment; }
                }}>
                <option value="start">Top</option><option value="center">Middle</option><option value="end">Bottom</option>
              </select>
            </label>
            <button type="button" disabled={!history.length} onclick={undo}>Undo</button>
          </div>
        {/if}
      </div>
      <div class="editor">
        {#each draft.sections as section, index (section.id)}
          {@const layout = draft.layouts.find(({ id }) => id === section.layoutId)!}
          <section class="edit-section" aria-label={`Section ${index + 1}`}>
            {#if section.breakBefore}<SectionBreak />{/if}
            <div class="section-tools" inert={!!movingBlock}>
              <button type="button" onclick={() => {
                remember();
                section.breakBefore = !section.breakBefore;
              }}>{section.breakBefore ? 'Remove section break' : 'Add section break'}</button>
              <button type="button" aria-label={`Move section ${index + 1} up`} disabled={index === 0}
                onclick={() => moveSection(section.id, -1)}>↑</button>
              <button type="button" aria-label={`Move section ${index + 1} down`} disabled={index === draft.sections.length - 1}
                onclick={() => moveSection(section.id, 1)}>↓</button>
              <button type="button" onclick={() => { remember(); draft = duplicateSection(draft, section.id); }}>Duplicate section</button>
              {#if draft.sections.length > 1 && !draft.assignments.some((entry) => entry.sectionId === section.id)}
                <button type="button" onclick={() => deleteEmptySection(section.id)}>Delete empty section</button>
              {/if}
              <button type="button" aria-expanded={layoutSection === section.id} onclick={() => openLayout(section.id)}>Change layout</button>
            </div>
            {#if layoutSection === section.id}{@render layoutEditor()}{/if}
            <div class="grid" style:--columns={columns(layout)}>
              {#each layout.slots as slot, slotIndex (slot.id)}
                {@const block = blockAt(draft, section.id, slot.id)}
                {@const slotName = layout.slots.length === 1 ? 'Full width' : slotIndex === 0 ? 'Left' : 'Right'}
                {@const slotKey = `${section.id}/${slot.id}`}
                {@const vertical = draft.assignments.find((entry) => entry.sectionId === section.id && entry.slotId === slot.id)?.vertical}
                <div class="edit-slot" style:align-self={vertical ?? 'start'} class:active={!!block && activeBlock === block.id} data-block={block?.id}>
                  {#if movingBlock && block?.id !== movingBlock}
                    <button class="destination" type="button"
                      aria-label={`${block ? 'Swap' : 'Move'} here · Section ${index + 1} · ${slotName}`}
                      onclick={() => void placeBlock(section.id, slot.id)}>{block ? 'Swap here' : 'Move here'}</button>
                  {/if}
                  <div class="slot-content" style:text-align={block?.kind === 'text' ? block.alignment : undefined} inert={!!movingBlock}>
                    {#if block}
                      {#key `${block.id}/${editorVersion}`}
                        {#if block.kind === 'text'}
                          <SectionTextEditor initialBlocks={authorText(block)} onChange={(text) => { if (text !== block.text) { remember(); block.text = text; } }}
                            onActivate={(editor) => {
                              activeBlock = block.id;
                              activeEditor = editor;
                              photoDetails = null;
                            }} />
                        {:else}
                          <div class="edit-photos" class:photo-grid={block.kind === 'gallery'}>
                            {#each block.photos as photo (photo.id)}
                              <figure class="edit-photo">
                                <button class="photo-button" type="button" aria-label={`Edit ${photo.alt || 'image'}`}
                                  aria-expanded={photoDetails === photo.id}
                                  onclick={() => {
                                    activeBlock = block.id;
                                    activeEditor = undefined;
                                    photoDetails = photoDetails === photo.id ? null : photo.id;
                                  }}>
                                  {#if publicUrl(photo.src)}
                                    <img src={publicUrl(photo.src)} alt={photo.alt} loading="lazy" referrerpolicy="no-referrer" />
                                  {:else}
                                    Add image details
                                  {/if}
                                </button>
                                {#if photo.caption}<figcaption>{photo.caption}</figcaption>{/if}
                                {#if photoDetails === photo.id}
                                  <div class="photo-details">
                                    <label>Image URL<input type="url" value={photo.src} oninput={(event) => { remember(); photo.src = event.currentTarget.value; }} /></label>
                                    <label>Alternative text<input value={photo.alt} oninput={(event) => { remember(); photo.alt = event.currentTarget.value; }} /></label>
                                    <label>Caption<input value={photo.caption} oninput={(event) => { remember(); photo.caption = event.currentTarget.value; }} /></label>
                                    <button type="button" onclick={() => photoDetails = null}>Done</button>
                                    {#if block.kind === 'gallery'}
                                      <button type="button" onclick={() => {
                                        remember();
                                        block.photos = block.photos.filter(({ id }) => id !== photo.id);
                                        photoDetails = null;
                                      }}>Remove image</button>
                                    {/if}
                                  </div>
                                {/if}
                              </figure>
                            {/each}
                          </div>
                          {#if block.kind === 'gallery' && (activeBlock === block.id || !block.photos.length)}
                            <button type="button" onclick={() => {
                              const photo = newPhoto();
                              remember();
                              block.photos.push(photo);
                              activeBlock = block.id;
                              activeEditor = undefined;
                              photoDetails = photo.id;
                            }}>Add gallery image</button>
                          {/if}
                        {/if}
                      {/key}
                      <div class="block-tools">
                        <button class="move-block" type="button" aria-label="Move block" title="Move block"
                          disabled={draft.sections.length === 1 && layout.slots.length === 1}
                          onclick={(event) => void startMove(block.id, event.currentTarget)}>↔</button>
                        {#if activeBlock === block.id}
                          <button type="button" aria-label="Remove block" title="Remove block"
                            onclick={() => removeBlock(block.id)}>×</button>
                        {/if}
                      </div>
                    {:else}
                      <div class="empty-slot">
                        <button type="button" aria-label={`Add block · Section ${index + 1} · ${slotName}`}
                          aria-expanded={picker === slotKey}
                          onclick={() => picker = picker === slotKey ? null : slotKey}>+</button>
                        {#if picker === slotKey}
                          <button type="button" onclick={() => addBlock(section.id, slot.id, 'text')}>Text</button>
                          <button type="button" onclick={() => addBlock(section.id, slot.id, 'image')}>Image</button>
                          <button type="button" onclick={() => addBlock(section.id, slot.id, 'gallery')}>Gallery</button>
                        {/if}
                      </div>
                    {/if}
                  </div>
                </div>
              {/each}
            </div>
          </section>
        {/each}
        {#if message}<p role="status">{message}</p>{/if}
        <button type="button" disabled={!!movingBlock} aria-expanded={layoutSection === 'new'} onclick={() => openLayout('new')}>Add section</button>
        {#if layoutSection === 'new'}{@render layoutEditor()}{/if}
      </div>
    {:else}
      {#key view}
        <SectionNodeView {draft} feed={view === 'feed'} onReadMore={() => view = 'node'} />
      {/key}
    {/if}
  </article>

  <details class="inspector">
    <summary>Backend data preview</summary>
    <pre>{JSON.stringify(draft, null, 2)}</pre>
  </details>
</div>

{#snippet layoutEditor()}
  <div class="layout-panel" aria-label="Section layout controls">
    <label>Layout
      <select value={layoutChoice} onchange={(event) => {
        layoutChoice = event.currentTarget.value;
        seedCustomLayout(layoutChoice);
      }}>
        {#each draft.layouts.filter((item) => !item.sectionOnly || item.id === layoutChoice) as layout (layout.id)}
          <option value={layout.id}>{layout.name}</option>
        {/each}
      </select>
    </label>
    {#if layoutSection !== 'new' && draft.layouts.find(({ id }) => id === layoutChoice)?.slots.length === 1
      && draft.assignments.filter((entry) => entry.sectionId === layoutSection).length > 1}
      <p>The second block will move to a new full-width section below.</p>
    {/if}
    <div class="buttons">
      <button type="button" onclick={() => applyChoice(layoutChoice)}>{layoutSection === 'new' ? 'Add this layout' : 'Apply layout'}</button>
      <button type="button" onclick={() => layoutSection = null}>Cancel</button>
    </div>
    <details>
      <summary>Customize layout</summary>
      <div class="custom">
        <label>Layout name<input bind:value={customName} placeholder="Name this layout" /></label>
        <label>Columns<select bind:value={customColumns}><option value={1}>One column</option><option value={2}>Two columns</option></select></label>
        {#if customColumns === 2}
          <label>Left column width · {leftShare}%
            <input type="range" min="30" max="70" step="1" bind:value={leftShare} />
          </label>
        {/if}
      </div>
      {#if layoutSection !== 'new' && customColumns === 1
        && draft.assignments.filter((entry) => entry.sectionId === layoutSection).length > 1}
        <p>The second block will move to a new full-width section below.</p>
      {/if}
      <button type="button" onclick={() => saveLayout(false)}>Apply to this section</button>
      <button type="button" disabled={!customName.trim() || draft.layouts.some(({ name }) => name.toLowerCase() === customName.trim().toLowerCase())}
        onclick={() => saveLayout(true)}>Save as a new preset</button>
    </details>
  </div>
{/snippet}

<style>
  .workbench { max-width: 72rem; margin: 0 auto; padding: clamp(.65rem, 2vw, 1.5rem); color: var(--atlas-text); }
  .workbench.narrow { max-width: 25rem; padding-inline: 0; }
  .narrow .node { padding-inline: .65rem; }
  .narrow .toolbar, .narrow .inspector { padding-inline: .65rem; }
  @media (max-width: 40rem) {
    .workbench { padding-inline: 0; }
    .node, .toolbar, .inspector { padding-inline: .65rem; }
  }
  .toolbar, .buttons, .section-tools, .empty-slot, .block-tools { display: flex; flex-wrap: wrap; gap: .45rem; align-items: center; }
  .toolbar { margin-bottom: .75rem; }
  .phone { margin-left: auto; display: flex; gap: .35rem; align-items: center; }
  .node { container-type: inline-size; border: 1px solid var(--atlas-border); border-radius: var(--atlas-radius, .75rem); background: var(--atlas-surface); padding: clamp(.8rem, 3vw, 2.5rem); }
  header { margin-bottom: clamp(.8rem, 3vw, 1.7rem); }
  h1, .title { font-size: clamp(1.5rem, 5cqi, 2.75rem); font-weight: 650; margin: 0; line-height: 1.16; overflow-wrap: anywhere; }
  .title { width: 100%; padding: .35rem 0; border: 0; border-bottom: 1px solid var(--atlas-border); border-radius: 0; }
  .editor, .edit-section, .edit-slot, .layout-panel { display: grid; gap: .75rem; min-width: 0; }
  .editor > button { justify-self: start; }
  .editing-tools { position: sticky; top: 0; z-index: 5; background: var(--atlas-surface); padding-block: .4rem; }
  .formatting { display: flex; flex-wrap: wrap; align-items: end; gap: .4rem; }
  .formatting label { font-size: .8rem; }
  .formatting button, .section-tools button, .block-tools button { min-width: 2.1rem; min-height: 2.1rem; }
  .edit-section { padding-block: .5rem 1rem; }
  .section-tools { justify-content: flex-end; color: var(--atlas-muted); font-size: .82rem; }
  .grid { display: grid; grid-template-columns: var(--columns); gap: .75rem; align-items: start; }
  .edit-slot { position: relative; min-width: 0; border-radius: .25rem; }
  .edit-slot.active, .edit-slot:focus-within { outline: 1px solid var(--atlas-border); outline-offset: .2rem; }
  .slot-content { min-width: 0; }
  .edit-slot[data-block] .slot-content { padding-block-end: 2.2rem; }
  .destination { position: absolute; inset-block-start: 0; inset-inline-end: 0; z-index: 4; border-color: var(--atlas-accent); }
  .block-tools { position: absolute; bottom: 0; right: 0; }
  .photo-button { display: block; width: 100%; padding: 0; border: 0; text-align: start; }
  figure { margin: 0; }
  figcaption { font-size: .85rem; color: var(--atlas-muted); }
  .photo-details { display: grid; gap: .4rem; margin-block-start: .5rem; }
  .edit-slot label, .layout-panel label { display: grid; gap: .25rem; }
  .block-tools { align-items: end; font-size: .85rem; }
  .empty-slot { min-height: 4rem; justify-content: center; }
  .edit-photos, .custom { display: grid; gap: .6rem; }
  .photo-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); }
  .edit-photo { min-width: 0; }
  .edit-photo img { display: block; width: 100%; max-height: 23rem; object-fit: contain; border-radius: var(--atlas-radius, .5rem); }
  .photo-grid img { aspect-ratio: 1; object-fit: cover; }
  .layout-panel { padding: .65rem; border: 1px solid var(--atlas-border); border-radius: var(--atlas-radius, .5rem); }
  .custom { margin: .55rem 0; }
  .layout-panel p { margin: 0; }
  button, input, select {
    box-sizing: border-box; font: inherit; color: inherit; background: var(--atlas-surface);
    padding: .4rem .55rem; border: 1px solid var(--atlas-border); border-radius: var(--atlas-radius, .45rem);
  }
  button, summary { cursor: pointer; }
  button:disabled { opacity: .5; cursor: default; }
  button[aria-pressed="true"] { border-color: var(--atlas-accent); color: var(--atlas-accent); }
  label input:not([type="checkbox"]), select { width: 100%; min-width: 0; }
  .inspector { margin-top: 1rem; }
  pre { overflow: auto; max-height: 28rem; padding: .75rem; background: var(--atlas-surface); border: 1px solid var(--atlas-border); }
  @container (max-width: 37rem) {
    .grid { grid-template-columns: minmax(0, 1fr); }
    .formatting button, .section-tools button, .block-tools button, .empty-slot button { min-height: 2.75rem; min-width: 2.75rem; }
    .edit-slot[data-block] .slot-content { padding-block-end: 2.8rem; }
  }
  @container (max-width: 24rem) { .edit-photos { grid-template-columns: minmax(0, 1fr); } }
</style>

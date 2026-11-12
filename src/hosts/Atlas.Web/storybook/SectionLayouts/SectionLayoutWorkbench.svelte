<script lang="ts">
  import { untrack } from 'svelte';
  import { publicUrl } from '../../src/lib/api/nodePage';
  import SectionNodeView from './SectionNodeView.svelte';
  import {
    applyLayout, blockAt, columns, example, moveBlock, newId,
    type Block, type Layout, type Photo,
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
  const destinations = $derived(draft.sections.flatMap((section, sectionIndex) => {
    const layout = draft.layouts.find(({ id }) => id === section.layoutId)!;
    return layout.slots.map((slot, slotIndex) => ({
      sectionId: section.id, slotId: slot.id,
      label: `Section ${sectionIndex + 1} · ${layout.slots.length === 1 ? 'Full width' : slotIndex === 0 ? 'Left' : 'Right'}`,
    }));
  }));

  function openLayout(sectionId: string) {
    const section = draft.sections.find(({ id }) => id === sectionId);
    layoutSection = sectionId;
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

  function applyChoice(layoutId: string) {
    if (layoutSection === 'new') {
      draft.sections.push({ id: newId(), layoutId });
    } else if (layoutSection) {
      const count = draft.sections.length;
      draft = applyLayout(draft, layoutSection, layoutId);
      if (draft.sections.length > count) message = 'Extra blocks moved to full-width sections immediately below.';
    }
    layoutSection = null;
  }

  function saveLayout() {
    const name = customName.trim();
    if (!name || draft.layouts.some((layout) => layout.name.toLowerCase() === name.toLowerCase())) return;
    const layout: Layout = {
      id: newId(), name,
      slots: customColumns === 1 ? [{ id: 'left', weight: 1 }]
        : [{ id: 'left', weight: leftShare }, { id: 'right', weight: 100 - leftShare }],
    };
    draft.layouts.push(layout);
    applyChoice(layout.id);
  }

  function addBlock(sectionId: string, slotId: string, kind: Block['kind']) {
    if (blockAt(draft, sectionId, slotId)) return;
    const id = newId();
    draft.blocks.push(kind === 'text' ? { id, kind, text: '' }
      : { id, kind, photos: kind === 'image' ? [newPhoto()] : [] });
    draft.assignments.push({ sectionId, slotId, blockId: id });
  }

  function newPhoto(): Photo {
    return { id: newId(), src: '', alt: '', caption: '' };
  }

  function removeBlock(blockId: string) {
    draft.assignments = draft.assignments.filter((entry) => entry.blockId !== blockId);
    draft.blocks = draft.blocks.filter(({ id }) => id !== blockId);
  }

  function moveSection(sectionId: string, direction: number) {
    const from = draft.sections.findIndex(({ id }) => id === sectionId);
    const to = from + direction;
    if (to < 0 || to >= draft.sections.length) return;
    const sections = [...draft.sections];
    [sections[from], sections[to]] = [sections[to], sections[from]];
    draft.sections = sections;
  }
</script>

<div class="workbench" class:narrow>
  <div class="toolbar" role="group" aria-label="Workbench view">
    <button type="button" aria-pressed={view === 'edit'} onclick={() => view = 'edit'}>Edit</button>
    <button type="button" aria-pressed={view === 'feed'} onclick={() => view = 'feed'}>Feed preview</button>
    <button type="button" aria-pressed={view === 'node'} onclick={() => view = 'node'}>Node page</button>
    <label class="phone"><input type="checkbox" bind:checked={narrow} /> Phone width</label>
  </div>

  <article class="node">
    <header>
      {#if view === 'edit'}
        <input class="title" aria-label="Node title" placeholder="Give your node a title" bind:value={draft.title} />
      {:else}
        <h1>{draft.title || 'Untitled node'}</h1>
      {/if}
    </header>

    {#if view === 'edit'}
      <div class="editor">
        {#each draft.sections as section, index (section.id)}
          {@const layout = draft.layouts.find(({ id }) => id === section.layoutId)!}
          <section class="edit-section" aria-label={`Section ${index + 1}`}>
            <div class="section-tools">
              <strong>{layout.name}</strong>
              <button type="button" aria-label={`Move section ${index + 1} up`} disabled={index === 0}
                onclick={() => moveSection(section.id, -1)}>↑</button>
              <button type="button" aria-label={`Move section ${index + 1} down`} disabled={index === draft.sections.length - 1}
                onclick={() => moveSection(section.id, 1)}>↓</button>
              {#if draft.sections.length > 1 && !draft.assignments.some((entry) => entry.sectionId === section.id)}
                <button type="button" onclick={() => {
                  draft.sections = draft.sections.filter(({ id }) => id !== section.id);
                  if (layoutSection === section.id) layoutSection = null;
                }}>Remove empty section</button>
              {/if}
              <button type="button" aria-expanded={layoutSection === section.id} onclick={() => openLayout(section.id)}>Change layout</button>
            </div>
            {#if layoutSection === section.id}{@render layoutEditor()}{/if}
            <div class="grid" style:--columns={columns(layout)}>
              {#each layout.slots as slot, slotIndex (slot.id)}
                {@const block = blockAt(draft, section.id, slot.id)}
                <div class="edit-slot" aria-label={layout.slots.length === 1 ? 'Full-width slot' : slotIndex === 0 ? 'Left slot' : 'Right slot'}>
                  {#if block}
                    {#if block.kind === 'text'}
                      <textarea aria-label="Text block" placeholder="Start writing…" rows="4" bind:value={block.text}></textarea>
                    {:else}
                      <div class="edit-photos" class:photo-grid={block.kind === 'gallery'}>
                        {#each block.photos as photo (photo.id)}
                          <div class="edit-photo">
                            {#if publicUrl(photo.src)}<img src={publicUrl(photo.src)} alt={photo.alt} loading="lazy" referrerpolicy="no-referrer" />{/if}
                            <details open={!publicUrl(photo.src)}>
                              <summary>Image details</summary>
                              <label>Image URL<input type="url" bind:value={photo.src} /></label>
                              <label>Alternative text<input bind:value={photo.alt} /></label>
                              <label>Caption<input bind:value={photo.caption} /></label>
                              {#if block.kind === 'gallery'}
                                <button type="button" onclick={() => block.photos = block.photos.filter(({ id }) => id !== photo.id)}>Remove image</button>
                              {/if}
                            </details>
                          </div>
                        {/each}
                      </div>
                      {#if block.kind === 'gallery'}<button type="button" onclick={() => block.photos.push(newPhoto())}>Add gallery image</button>{/if}
                    {/if}
                    <div class="block-tools">
                      <label>Move or swap
                        <select aria-label="Move or swap block" value={`${section.id}/${slot.id}`}
                          onchange={(event) => {
                            const [sectionId, slotId] = event.currentTarget.value.split('/');
                            draft = moveBlock(draft, block.id, sectionId, slotId);
                          }}>
                          {#each destinations as destination}
                            <option value={`${destination.sectionId}/${destination.slotId}`}>{destination.label}</option>
                          {/each}
                        </select>
                      </label>
                      <button type="button" onclick={() => removeBlock(block.id)}>Remove block</button>
                    </div>
                  {:else}
                    <div class="empty-slot">
                      <button type="button" onclick={() => addBlock(section.id, slot.id, 'text')}>Add text</button>
                      <button type="button" onclick={() => addBlock(section.id, slot.id, 'image')}>Add image</button>
                      <button type="button" onclick={() => addBlock(section.id, slot.id, 'gallery')}>Add gallery</button>
                    </div>
                  {/if}
                </div>
              {/each}
            </div>
          </section>
        {/each}
        {#if message}<p role="status">{message}</p>{/if}
        <button type="button" aria-expanded={layoutSection === 'new'} onclick={() => openLayout('new')}>Add section</button>
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
        {#each draft.layouts as layout (layout.id)}
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
      <summary>Customize and save a layout</summary>
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
      <button type="button" disabled={!customName.trim() || draft.layouts.some(({ name }) => name.toLowerCase() === customName.trim().toLowerCase())}
        onclick={saveLayout}>Save and apply layout</button>
    </details>
  </div>
{/snippet}

<style>
  .workbench { max-width: 72rem; margin: 0 auto; padding: clamp(.65rem, 2vw, 1.5rem); color: var(--atlas-text); }
  .workbench.narrow { max-width: 25rem; }
  .toolbar, .buttons, .section-tools, .empty-slot, .block-tools { display: flex; flex-wrap: wrap; gap: .45rem; align-items: center; }
  .toolbar { margin-bottom: .75rem; }
  .phone { margin-left: auto; display: flex; gap: .35rem; align-items: center; }
  .node { container-type: inline-size; border: 1px solid var(--atlas-border); border-radius: var(--atlas-radius, .75rem); background: var(--atlas-surface); padding: clamp(.8rem, 3vw, 2.5rem); }
  header { margin-bottom: clamp(.8rem, 3vw, 1.7rem); }
  h1, .title { font-size: clamp(1.5rem, 5cqi, 2.75rem); font-weight: 650; margin: 0; line-height: 1.16; overflow-wrap: anywhere; }
  .title { width: 100%; padding: .35rem 0; border: 0; border-bottom: 1px solid var(--atlas-border); border-radius: 0; }
  .editor, .edit-section, .edit-slot, .layout-panel { display: grid; gap: .75rem; min-width: 0; }
  .editor > button { justify-self: start; }
  .edit-section { border: 1px solid var(--atlas-border); border-radius: var(--atlas-radius, .5rem); padding: .75rem; }
  .section-tools strong { margin-right: auto; }
  .grid { display: grid; grid-template-columns: var(--columns); gap: .75rem; align-items: start; }
  .edit-slot { background: var(--atlas-bg); padding: .55rem; border-radius: var(--atlas-radius, .5rem); }
  .edit-slot label, .layout-panel label { display: grid; gap: .25rem; }
  .block-tools { align-items: end; font-size: .85rem; }
  .block-tools label { flex: 1 1 9rem; min-width: 0; }
  .empty-slot { min-height: 4rem; justify-content: center; }
  .edit-photos, .custom { display: grid; gap: .6rem; }
  .photo-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); }
  .edit-photo { min-width: 0; }
  .edit-photo img { display: block; width: 100%; max-height: 23rem; object-fit: contain; border-radius: var(--atlas-radius, .5rem); }
  .photo-grid img { aspect-ratio: 1; object-fit: cover; }
  .edit-photo details label { margin: .35rem 0; }
  .layout-panel { padding: .65rem; border: 1px solid var(--atlas-border); border-radius: var(--atlas-radius, .5rem); }
  .custom { margin: .55rem 0; }
  .layout-panel p { margin: 0; }
  button, input, textarea, select {
    box-sizing: border-box; font: inherit; color: inherit; background: var(--atlas-surface);
    padding: .4rem .55rem; border: 1px solid var(--atlas-border); border-radius: var(--atlas-radius, .45rem);
  }
  button, summary { cursor: pointer; }
  button:disabled { opacity: .5; cursor: default; }
  button[aria-pressed="true"] { border-color: var(--atlas-accent); color: var(--atlas-accent); }
  textarea { width: 100%; resize: vertical; min-height: 6rem; line-height: 1.5; }
  label input:not([type="checkbox"]), select { width: 100%; min-width: 0; }
  .inspector { margin-top: 1rem; }
  pre { overflow: auto; max-height: 28rem; padding: .75rem; background: var(--atlas-surface); border: 1px solid var(--atlas-border); }
  @container (max-width: 37rem) { .grid { grid-template-columns: minmax(0, 1fr); } }
  @container (max-width: 24rem) { .edit-photos { grid-template-columns: minmax(0, 1fr); } }
</style>

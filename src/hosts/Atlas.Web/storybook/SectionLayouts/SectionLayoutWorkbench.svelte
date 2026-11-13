<script lang="ts">
  import PhotoFramingControls from './PhotoFramingControls.svelte';
  import GalleryLayout from './GalleryLayout.svelte';
  import WrappedImage from './WrappedImage.svelte';
  import PhotoFrame from './PhotoFrame.svelte';
  import { tick, untrack } from 'svelte';
  import { publicUrl } from '../../src/lib/api/nodePage';
  import SectionTextEditor from './SectionTextEditor.svelte';
  import type TiptapCanvas from '../../src/lib/components/TiptapCanvas.svelte';
  import type { AuthorBlock } from '../../src/lib/api/contentAuthoring';
  import SectionBreak from './SectionBreak.svelte';
  import SectionNodeView from './SectionNodeView.svelte';
  import {
    applyLayout, blockAt, columns, duplicateSection, example,  moveBlock, newId, sectionRows, rowWrap,
    type Block, type Example, type GalleryStyle, type Section, type Slot, type Layout, type Photo, type Draft, type VerticalAlignment,
  } from './sectionLayouts';

  let { start = 'editorial' }: { start?: Example } = $props();
  let draft = $state(untrack(() => example(start)));
  let view = $state<'edit' | 'feed' | 'node'>('edit');
  let narrow = $state(false);
  let layoutSection = $state<string | null>(null);
  let layoutBefore = $state.raw<Draft | null>(null);
  let layoutChoice = $state('full');
  let customName = $state('');
  let customColumns = $state(2);
  let leftShare = $state(60);
  let message = $state('');
  let activeSection = $state<string | null>(null);
  let activeBlock = $state<string | null>(null);
  let movingBlock = $state<string | null>(null);
  let picker = $state<string | null>(null);
  let draggedSection = $state<string | null>(null);
  let dropParagraph = $state<HTMLElement | null>(null);
  let photoDetails = $state<string | null>(null);
  let activeEditor = $state<ReturnType<typeof TiptapCanvas>>();
  let workbench = $state<HTMLDivElement>();
  const formats = [
    ['bold', 'Bold', 'B'], ['italic', 'Italic', 'I'], ['underline', 'Underline', 'U'], ['link', 'Link', 'Link'],
    ['bulletList', 'Bulleted list', 'List'],
  ] as const;

  let history = $state.raw<Draft[]>([]);
  let editorVersion = $state(0);
  const selectedText = $derived(draft.blocks.find((block) => block.id === activeBlock && block.kind === 'text'));
  const selectedAssignment = $derived(draft.assignments.find(({ blockId }) => blockId === activeBlock));

  function remember() {
    history = [...history, structuredClone($state.snapshot(draft))];
  }

  function setWrap(section: Section, slots: Slot[], imageId: string, side: string) {
    const text = slots.map(({ id }) => blockAt(draft, section.id, id)).find((block) => block?.kind === 'text');
    if (!text || (side !== 'left' && side !== 'right' && !section.wrap)) return;
    remember();
    section.wrap = side === 'left' || side === 'right'
      ? { textId: text.id, imageId, side, anchor: section.wrap?.anchor ?? 0 }
      : undefined;
    activeBlock = imageId;
  }

  function moveWrappedImageHere() {
    const section = draft.sections.find(({ wrap }) => wrap && wrap.textId === activeBlock);
    if (!section?.wrap || !activeEditor) return;
    moveImageAnchor(section, activeEditor.selectedTextBlockIndex());
  }

  function moveImageAnchor(section: Section, anchor: number) {
    if (!section.wrap || section.wrap.anchor === anchor) return;
    remember();
    section.wrap.anchor = anchor;
  }

  function clearImageDrag() {
    dropParagraph?.classList.remove('image-drop-target');
    dropParagraph = null;
    draggedSection = null;
  }

  function startImageDrag(event: DragEvent, sectionId: string) {
    if (!event.dataTransfer) return;
    draggedSection = sectionId;
    event.dataTransfer.effectAllowed = 'move';
    event.dataTransfer.setData('application/x-atlas-image-anchor', sectionId);
  }

  function imageDropTarget(event: DragEvent) {
    if (!draggedSection || !(event.target instanceof Element)) return;
    const section = draft.sections.find(({ id }) => id === draggedSection);
    const editor = event.target.closest('.atlas-story-text');
    if (!section?.wrap || !editor || !workbench?.contains(editor)
      || editor.closest<HTMLElement>('.edit-slot')?.dataset.block !== section.wrap.textId) return;
    const blocks = Array.from(editor.children).filter((child) => !child.classList.contains('ProseMirror-widget'));
    const anchor = blocks.findIndex((child) => child.contains(event.target as Element));
    const paragraph = blocks[anchor];
    if (paragraph instanceof HTMLElement) return { section, anchor, paragraph };
  }

  function previewImageDrop(event: DragEvent) {
    if (!draggedSection) return;
    const target = imageDropTarget(event);
    dropParagraph?.classList.remove('image-drop-target');
    dropParagraph = target?.paragraph ?? null;
    if (!target) return;
    event.preventDefault();
    if (event.dataTransfer) event.dataTransfer.dropEffect = 'move';
    target.paragraph.classList.add('image-drop-target');
  }

  function dropWrappedImage(event: DragEvent) {
    if (!draggedSection) return;
    const target = imageDropTarget(event);
    event.preventDefault();
    event.stopPropagation();
    clearImageDrag();
    if (target) moveImageAnchor(target.section, target.anchor);
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
    layoutBefore = null;
    history = history.slice(0, -1);
    draft = previous;
    editorVersion++;
    movingBlock = null;
    activeSection = null;
    activeBlock = null;
    activeEditor = undefined;
    layoutSection = null;
    picker = null;
    photoDetails = null;
    message = 'Change undone.';
  }

  function updateFocus(target: EventTarget | null, keyboard = false) {
    if (view !== 'edit' || movingBlock || !(target instanceof Element)) return;
    const inside = workbench?.contains(target);
    if (!keyboard && layoutBefore && !target.closest('.layout-panel')) finishLayout();
    if (inside && target.closest('.editing-tools')) return;
    const section = inside && target.closest('.edit-slot, .section-tools, .layout-panel')
      ? target.closest<HTMLElement>('[data-section]') : null;
    const block = inside ? target.closest<HTMLElement>('[data-block]') : null;
    activeSection = section?.dataset.section ?? null;
    if (layoutSection && layoutSection !== 'new' && layoutSection !== activeSection) finishLayout();
    if (block?.dataset.block === activeBlock) return;
    activeEditor?.dismissControls();
    activeEditor = undefined;
    activeBlock = keyboard ? block?.dataset.block ?? null : null;
    photoDetails = null;
  }

  function authorText(block: Extract<Block, { kind: 'text' }>): AuthorBlock[] {
    return [{ key: block.id, kind: 'markdown', text: block.text, url: '', caption: '' }];
  }

  function selectView(next: typeof view) {
    view = next;
    movingBlock = null;
    activeSection = null;
    activeBlock = null;
    activeEditor = undefined;
    picker = null;
    photoDetails = null;
  }

  async function startMove(id: string) {
    activeBlock = id;
    movingBlock = id;
    picker = null;
    photoDetails = null;
    activeEditor = undefined;
    layoutSection = null;
    await tick();
    workbench?.querySelector<HTMLButtonElement>('.destination')?.focus({ preventScroll: true });
  }

  async function cancelMove() {
    const id = movingBlock;
    movingBlock = null;
    await tick();
    workbench?.querySelector<HTMLButtonElement>(`[data-block="${id}"] .move-block`)?.focus();
  }

  async function placeBlock(sectionId: string, slotId: string) {
    const id = movingBlock;
    if (!id) return;
    const target = blockAt(draft, sectionId, slotId);
    remember();
    draft = moveBlock(draft, id, sectionId, slotId);
    movingBlock = null;
    activeSection = sectionId;
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

  function previewLayout(layoutId: string) {
    if (!layoutSection || layoutSection === 'new') return;
    layoutBefore ??= structuredClone($state.snapshot(draft));
    const original = layoutBefore.sections.find(({ id }) => id === layoutSection);
    if (original?.layoutId === layoutId) {
      draft = { ...draft, sections: layoutBefore.sections, assignments: layoutBefore.assignments };
    } else {
      draft = applyLayout({ ...draft, sections: layoutBefore.sections, assignments: layoutBefore.assignments }, layoutSection, layoutId);
    }
  }

  function finishLayout(cancel = false) {
    const before = layoutBefore;
    if (before) {
      if (cancel) draft = { ...draft, sections: before.sections, assignments: before.assignments };
      else if (before.sections.find(({ id }) => id === layoutSection)?.layoutId !== layoutChoice)
        history = [...history, structuredClone(before)];
    }
    layoutBefore = null;
    layoutSection = null;
  }

  function applyChoice(layoutId: string, record = true) {
    if (record && layoutBefore) { finishLayout(); return; }
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
    if (layoutBefore) {
      draft = { ...draft, sections: layoutBefore.sections, assignments: layoutBefore.assignments };
      layoutBefore = null;
    }
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

  function reorderPhoto(block: Extract<Block, { photos: Photo[] }>, index: number, direction: number) {
    remember();
    const photos = [...block.photos];
    const [photo] = photos.splice(index, 1);
    photos.splice(index + direction, 0, photo);
    block.photos = photos;
  }

  function newPhoto(): Photo {
    return { id: newId(), src: '', alt: '', caption: '' };
  }

  function removeBlock(blockId: string) {
    if (!window.confirm('Remove this block?')) return;
    remember();
    for (const section of draft.sections) {
      if (section.wrap?.textId === blockId || section.wrap?.imageId === blockId) section.wrap = undefined;
    }
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

<svelte:window ondragovercapture={previewImageDrop} ondropcapture={dropWrappedImage} ondragend={clearImageDrag}
  onpointerdowncapture={(event) => updateFocus(event.target)}
  onfocusin={(event) => updateFocus(event.target, true)} onkeydowncapture={(event) => {
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
            {#if activeEditor && draft.sections.some(({ wrap }) => wrap && wrap.textId === activeBlock)}
              <button type="button" onpointerdown={(event) => event.preventDefault()} onclick={moveWrappedImageHere}>Move image here</button>
            {/if}
            <button type="button" disabled={!history.length} onclick={undo}>Undo</button>
          </div>
        {/if}
      </div>
      <div class="editor">
        {#each draft.sections as section, index (section.id)}
          {@const layout = draft.layouts.find(({ id }) => id === section.layoutId)!}
          {@const rows = sectionRows(section, layout)}
          <section class="edit-section" data-section={section.id} aria-label={`Section ${index + 1}`}>
            {#if section.breakBefore}<SectionBreak />{/if}
            <div class="section-tools" class:inactive={activeSection !== section.id || !!movingBlock}
              inert={activeSection !== section.id || !!movingBlock}>
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
            {#if !movingBlock && activeSection === section.id && layoutSection === section.id}{@render layoutEditor()}{/if}
            {#each rows as row, rowIndex (row.id)}
              {@const wrap = rowWrap(draft, section, row.slots)}
              <div class="grid" style:--columns={wrap ? "minmax(0, 1fr)" : columns(row.slots)}>
                {#each row.slots as slot, slotIndex (slot.id)}
                  {@const block = blockAt(draft, section.id, slot.id)}
                  {@const slotName = row.id !== null ? `Row ${rowIndex + 1} · Full width` : row.slots.length === 1 ? 'Full width' : slotIndex === 0 ? 'Left' : 'Right'}
                  {@const slotKey = `${section.id}/${slot.id}`}
                  {@const vertical = draft.assignments.find((entry) => entry.sectionId === section.id && entry.slotId === slot.id)?.vertical}
                  {#if !wrap || block?.id !== wrap.imageId}
                    <div class="edit-slot" style:--slot-alignment={vertical ?? 'start'} class:active={!!block && activeBlock === block.id} data-block={block?.id}>
                      {#if movingBlock && block?.id !== movingBlock}
                        <button class="destination" type="button"
                          aria-label={`${block ? 'Swap' : 'Move'} here · Section ${index + 1} · ${slotName}`}
                          onclick={() => void placeBlock(section.id, slot.id)}>{block ? 'Swap here' : 'Move here'}</button>
                      {/if}
                      <div class="slot-content" style:text-align={block?.kind === 'text' ? block.alignment : undefined} inert={!!movingBlock}>
                        {#if block}
                          {#key `${block.id}/${editorVersion}`}
                            {#if block.kind === 'text'}
                              {#snippet inset()}
                                {#if wrap}
                                  <WrappedImage side={wrap.side} width={wrap.image.photos[0]?.frame?.width}>
                                    <div data-block={wrap.imageId}>
                                      {@render editPhotos(wrap.image, section, row.slots, true)}
                                    </div>
                                  </WrappedImage>
                                {/if}
                              {/snippet}
                              <SectionTextEditor inset={wrap ? inset : undefined} anchor={wrap?.anchor} initialBlocks={authorText(block)} onChange={(text, anchor) => {
                                  if (text === block.text) return;
                                  remember();
                                  block.text = text;
                                  if (anchor !== undefined && section.wrap?.textId === block.id) section.wrap.anchor = anchor;
                                }}
                                onActivate={(editor) => {
                                  activeBlock = block.id;
                                  activeEditor = editor;
                                  photoDetails = null;
                                }} />
                            {:else}
                              {@render editPhotos(block, section, row.slots)}
                            {/if}
                          {/key}
                          {#if !movingBlock && activeBlock === block.id}<div class="block-tools">
                            <button class="move-block" type="button" aria-label="Move block" title="Move block"
                              disabled={!!wrap || (draft.sections.length === 1 && rows.length === 1 && row.slots.length === 1)}
                              onclick={() => void startMove(block.id)}>↔</button>
                            <button type="button" aria-label="Remove block" title="Remove block"
                              onclick={() => removeBlock(block.id)}>×</button>
                          </div>{/if}
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
                  {/if}
                {/each}
              </div>
            {/each}
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

{#snippet editPhotos(block: Extract<Block, { photos: Photo[] }>, section: Section, slots: Slot[], wrapped = false)}
  {#snippet editPhoto(photo: Photo)}
      <figure class="edit-photo" class:photo-open={photoDetails === photo.id} style:--photo-width={(wrapped ? 100 : photo.frame?.width ?? 100) + '%'}>
        <button class="photo-button"
          class:draggable-photo={wrapped} draggable={wrapped ? 'true' : 'false'}
          title={wrapped ? 'Drag to another paragraph, or click to edit image' : undefined}
          ondragstart={(event) => { if (wrapped) startImageDrag(event, section.id); }} type="button" aria-label={`Edit ${photo.alt || 'image'}`}
          aria-expanded={photoDetails === photo.id}
          onclick={() => {
            activeBlock = block.id;
            activeEditor = undefined;
            photoDetails = photoDetails === photo.id ? null : photo.id;
          }}>
          {#if publicUrl(photo.src)}
            <PhotoFrame {photo} src={publicUrl(photo.src)!} gallery={block.kind === 'gallery'} />
          {:else}
            Add image details
          {/if}
        </button>
        {#if photo.caption}<figcaption>{photo.caption}</figcaption>{/if}
        {#if photoDetails === photo.id}
          <div class="photo-details" role="group" aria-label="Image settings"
            class:align-end={wrapped ? section.wrap?.side === 'right' : block.kind === 'gallery' ? block.photos.indexOf(photo) % 2 === 1 : !!slots[1] && blockAt(draft, section.id, slots[1].id)?.id === block.id}>
            <label>Image URL<input type="url" value={photo.src} oninput={(event) => { remember(); photo.src = event.currentTarget.value; }} /></label>
            <label>Alternative text<input value={photo.alt} oninput={(event) => { remember(); photo.alt = event.currentTarget.value; }} /></label>
            <label>Caption<input value={photo.caption} oninput={(event) => { remember(); photo.caption = event.currentTarget.value; }} /></label>
            <PhotoFramingControls {photo} src={publicUrl(photo.src)} gallery={block.kind === 'gallery'}
              onChange={(frame) => { remember(); photo.frame = frame; }} />
            {#if block.kind === 'image' && slots.some(({ id }) => blockAt(draft, section.id, id)?.kind === 'text')}
              <label>Wrap text
                <select value={section.wrap?.imageId === block.id ? section.wrap.side : 'none'}
                  onchange={(event) => setWrap(section, slots, block.id, event.currentTarget.value)}>
                  <option value="none">Off</option>
                  <option value="left">Image on left</option>
                  <option value="right">Image on right</option>
                </select>
              </label>
            {/if}
            <button type="button" onclick={() => photoDetails = null}>Done</button>
            {#if wrapped}
              <button type="button" onclick={() => removeBlock(block.id)}>Remove wrapped image</button>
            {/if}
            {#if block.kind === 'gallery'}
              {@const index = block.photos.findIndex(({ id }) => id === photo.id)}
              <button type="button" disabled={index === 0} onclick={() => reorderPhoto(block, index, -1)}>Move image earlier</button>
              <button type="button" disabled={index === block.photos.length - 1} onclick={() => reorderPhoto(block, index, 1)}>Move image later</button>
              <button type="button" onclick={() => {
                remember();
                block.photos = block.photos.filter(({ id }) => id !== photo.id);
                photoDetails = null;
              }}>Remove image</button>
            {/if}
          </div>
        {/if}
      </figure>
  {/snippet}
  {#if block.kind === 'gallery'}
    <GalleryLayout photos={block.photos} layout={block.galleryStyle} selected={photoDetails} onNavigate={() => photoDetails = null} photo={editPhoto} />
  {:else}
    {#each block.photos as photo (photo.id)}{@render editPhoto(photo)}{/each}
  {/if}
  {#if block.kind === 'gallery' && (activeBlock === block.id || !block.photos.length)}
    <label>Gallery layout
      <select value={block.galleryStyle ?? 'grid'} onchange={(event) => {
        remember();
        block.galleryStyle = event.currentTarget.value as GalleryStyle;
      }}>
        <option value="grid">Grid</option>
        <option value="mosaic">Mosaic</option>
        <option value="slideshow">Slideshow</option>
      </select>
    </label>
    <button type="button" onclick={() => {
      const photo = newPhoto();
      remember();
      block.photos.push(photo);
      activeBlock = block.id;
      activeEditor = undefined;
      photoDetails = photo.id;
    }}>Add gallery image</button>
  {/if}
{/snippet}

{#snippet layoutEditor()}
  <div class="layout-panel" aria-label="Section layout controls">
    <label>Layout
      <select value={layoutChoice} onchange={(event) => {
        layoutChoice = event.currentTarget.value;
        seedCustomLayout(layoutChoice);
        previewLayout(layoutChoice);
      }}>
        {#each draft.layouts.filter((item) => !item.sectionOnly || item.id === layoutChoice) as layout (layout.id)}
          <option value={layout.id}>{layout.name}</option>
        {/each}
      </select>
    </label>
    {#if layoutSection !== 'new' && draft.layouts.find(({ id }) => id === layoutChoice)?.slots.length === 1
      && (layoutBefore?.assignments ?? draft.assignments).filter((entry) => entry.sectionId === layoutSection).length > 1}
      <p>The second block will move to a new full-width section below.</p>
    {/if}
    <div class="buttons">
      <button type="button" onclick={() => applyChoice(layoutChoice)}>{layoutSection === 'new' ? 'Add this layout' : 'Apply layout'}</button>
      <button type="button" onclick={() => finishLayout(true)}>Cancel</button>
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
        && (layoutBefore?.assignments ?? draft.assignments).filter((entry) => entry.sectionId === layoutSection).length > 1}
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
  .section-tools.inactive { display: none; }
  .section-tools { justify-content: flex-end; color: var(--atlas-muted); font-size: .82rem; }
  .grid { display: grid; grid-template-columns: var(--columns); gap: .75rem; align-items: stretch; }
  .edit-slot { position: relative; min-width: 0; border-radius: .25rem; }
  .edit-slot.active, .edit-slot:focus-within { outline: 1px solid var(--atlas-border); outline-offset: .2rem; }
  .edit-slot { display: grid; align-items: var(--slot-alignment); }
  .slot-content { min-width: 0; }
  .edit-slot.active .slot-content { padding-block-end: 2.2rem; }
  .destination { position: absolute; inset-block-start: 0; inset-inline-end: 0; z-index: 4; border-color: var(--atlas-accent); }
  .block-tools { position: absolute; bottom: 0; right: 0; }
  .photo-button { display: block; width: var(--photo-width, 100%); margin-inline: auto; padding: 0; border: 0; text-align: start; }
  .draggable-photo { cursor: grab; }
  .edit-slot :global(.image-drop-target) { outline: 2px solid var(--atlas-accent); outline-offset: .15rem; }
  figure { margin: 0; }
  figcaption { text-align: center; font-size: .85rem; color: var(--atlas-muted); width: var(--photo-width); margin: .25rem auto 0; }
  .photo-details {
    position: absolute;
    z-index: 4;
    inset-block-start: calc(100% + .35rem);
    inset-inline-start: calc((100% - var(--photo-width)) / 2);
    box-sizing: border-box;
    width: min(21rem, 90cqi);
    max-height: min(32rem, 65dvh);
    overflow: auto;
    display: grid;
    gap: .4rem;
    padding: .65rem;
    background: var(--atlas-surface);
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius, .5rem);
    box-shadow: 0 .35rem 1rem rgb(0 0 0 / 15%);
  }
  .photo-details.align-end {
    inset-inline-start: auto;
    inset-inline-end: calc((100% - var(--photo-width)) / 2);
  }
  .photo-open { z-index: 4; }
  .edit-slot label, .layout-panel label { display: grid; gap: .25rem; }
  .block-tools { align-items: end; font-size: .85rem; }
  .empty-slot { min-height: 4rem; justify-content: center; }
  .custom { display: grid; gap: .6rem; }
  .edit-photo { position: relative; min-width: 0; }
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
    .photo-details, .photo-details.align-end { inset-inline: 0 auto; }
    .formatting button, .section-tools button, .block-tools button, .empty-slot button { min-height: 2.75rem; min-width: 2.75rem; }
    .edit-slot.active .slot-content { padding-block-end: 2.8rem; }
  }
</style>

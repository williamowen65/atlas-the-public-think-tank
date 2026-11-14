<script lang="ts">
  import PhotoFramingControls from './PhotoFramingControls.svelte';
  import LinkPreviewCard from './LinkPreviewCard.svelte';
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
    type Block, linkPreviewSamples, type Example, type GalleryStyle, type SlideshowSettings, type Section, type Slot, type Layout, type Photo, type Draft,
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
  let sidebarOpen = $state(true);
  let photoDetails = $state<string | null>(null);
  let activeEditor = $state<ReturnType<typeof TiptapCanvas>>();
  let workbench = $state<HTMLDivElement>();
  let alignmentMenu = $state<HTMLDivElement>();
  const alignmentMenuId = $props.id();
  const textAlignments = [['left', 'Left'], ['center', 'Center'], ['right', 'Right']] as const;
  const verticalAlignments = [['start', 'Top'], ['center', 'Middle'], ['end', 'Bottom']] as const;
  const formats = [
    ['bold', 'Bold', 'B'], ['italic', 'Italic', 'I'], ['underline', 'Underline', 'U'], ['link', 'Link', ''],
  ] as const;

  let history = $state.raw<Draft[]>([]);
  let editorVersion = $state(0);
  const selectedText = $derived(draft.blocks.find((block): block is Extract<Block, { kind: 'text' }> => block.id === activeBlock && block.kind === 'text'));
  const selectedAssignment = $derived(draft.assignments.find(({ blockId }) => blockId === activeBlock));

  const mediaBlock = $derived(draft.blocks.find((block) => block.id === activeBlock && 'photos' in block));
  const mediaSection = $derived(draft.sections.find(({ id }) => id === selectedAssignment?.sectionId));
  const mediaLayout = $derived(draft.layouts.find(({ id }) => id === mediaSection?.layoutId));
  const mediaRow = $derived(mediaSection && mediaLayout
    ? sectionRows(mediaSection, mediaLayout).find(({ slots }) => slots.some(({ id }) => id === selectedAssignment?.slotId))
    : undefined);
  const selectedPhoto = $derived(mediaBlock && 'photos' in mediaBlock
    ? mediaBlock.photos.find(({ id }) => id === photoDetails) : undefined);

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

  function moveImageAnchor(section: Section, anchor: number, side?: 'left' | 'right') {
    if (!section.wrap || (section.wrap.anchor === anchor && (!side || section.wrap.side === side))) return;
    remember();
    section.wrap.anchor = anchor;
    if (side) section.wrap.side = side;
  }

  function clearImageDrag() {
    dropParagraph?.classList.remove('image-drop-left', 'image-drop-right');
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
    if (!(paragraph instanceof HTMLElement)) return;
    const bounds = editor.getBoundingClientRect();
    const side = event.clientX < bounds.left + bounds.width / 2 ? 'left' : 'right';
    return { section, anchor, paragraph, side } as const;
  }

  function previewImageDrop(event: DragEvent) {
    if (!draggedSection) return;
    const target = imageDropTarget(event);
    dropParagraph?.classList.remove('image-drop-left', 'image-drop-right');
    dropParagraph = target?.paragraph ?? null;
    if (!target) return;
    event.preventDefault();
    if (event.dataTransfer) event.dataTransfer.dropEffect = 'move';
    target.paragraph.classList.add(`image-drop-${target.side}`);
  }

  function dropWrappedImage(event: DragEvent) {
    if (!draggedSection) return;
    const target = imageDropTarget(event);
    event.preventDefault();
    event.stopPropagation();
    clearImageDrag();
    if (target) moveImageAnchor(target.section, target.anchor, target.side);
  }

  function deleteEmptySection(sectionId: string) {
    if (draft.sections.length < 2 || draft.assignments.some((entry) => entry.sectionId === sectionId)) return;
    remember();
    draft.sections = draft.sections.filter(({ id }) => id !== sectionId);
    if (draft.sections[0].breakBefore) draft.sections[0].breakBefore = false;
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
    if (inside && target.closest('.editor-sidebar, .sidebar-toggle, .editing-tools')) return;
    if (!keyboard && photoDetails && !target.closest('.photo-button')) photoDetails = null;
    if (!keyboard && layoutBefore && !target.closest('.layout-panel')) finishLayout();
    const section = inside && target.closest('.edit-slot, .layout-panel')
      ? target.closest<HTMLElement>('[data-section]') : null;
    const block = inside ? target.closest<HTMLElement>('[data-block]') : null;
    activeSection = section?.dataset.section ?? null;
    if (layoutSection && layoutSection !== 'new' && layoutSection !== activeSection) finishLayout();
    if (block?.dataset.block === activeBlock) return;
    activeEditor?.dismissControls();
    activeEditor = undefined;
    activeBlock = keyboard || block?.matches('.gallery-slot, .link-slot') ? block?.dataset.block ?? null : null;
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
    movingBlock = null;
    await tick();
    workbench?.querySelector<HTMLButtonElement>('.editor-sidebar .move-block')?.focus({ preventScroll: true });
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
    workbench?.querySelector<HTMLButtonElement>('.editor-sidebar .move-block')?.focus({ preventScroll: true });
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
      : kind === 'link' ? { id, kind, url: '' }
      : { id, kind, photos: photo ? [photo] : [] });
    draft.assignments.push({ sectionId, slotId, blockId: id });
    activeBlock = id;
    activeEditor = undefined;
    picker = null;
    photoDetails = photo?.id ?? null;
  }

  function updateSlideshow(block: Extract<Block, { photos: Photo[] }>, settings: SlideshowSettings) {
    remember();
    block.slideshow = { ...block.slideshow, ...settings };
  }

  function randomizeMasonryHeights(photos: Photo[]) {
    const ratios = ['1 / 1', '4 / 5', '2 / 3', '4 / 3'] as const;
    let previous: (typeof ratios)[number] | undefined;
    remember();
    for (const photo of photos) {
      const choices = ratios.filter((ratio) => ratio !== previous && ratio !== photo.frame?.ratio);
      const ratio = choices[Math.floor(Math.random() * choices.length)];
      photo.frame = { ...photo.frame, ratio };
      previous = ratio;
    }
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
    if (sections[0].breakBefore) sections[0].breakBefore = false;
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
    {#if view === 'edit'}
      <button class="sidebar-toggle" type="button" aria-expanded={sidebarOpen}
        onclick={() => sidebarOpen = !sidebarOpen}>{sidebarOpen ? 'Hide editor controls' : 'Show editor controls'}</button>
    {/if}
    <label class="phone"><input type="checkbox" bind:checked={narrow} /> Phone width</label>
  </div>

  <div class="editor-frame" class:with-sidebar={view === 'edit' && sidebarOpen}>
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
            <select class="heading-select" aria-label="Heading level" title="Heading level" disabled={!activeEditor} onchange={(event) => {
              const level = Number(event.currentTarget.value) as 2 | 3 | 4 | 5 | 6;
              activeEditor?.formatText('heading', level);
              event.currentTarget.value = '';
            }}>
              <option value="" disabled selected>Heading</option>
              {#each [2, 3, 4, 5, 6] as level}<option value={level}>H{level}</option>{/each}
            </select>
            <div class="format-group" role="group" aria-label="Inline formatting">
              {#each formats as [command, label, short]}
                <button class:italic={command === 'italic'} class:underline={command === 'underline'}
                  type="button" aria-label={label} title={label} disabled={!activeEditor}
                  onpointerdown={(event) => event.preventDefault()}
                  onclick={() => activeEditor?.formatText(command)}>
                  {#if command === 'link'}
                    <svg viewBox="0 0 24 24" aria-hidden="true">
                      <path d="m10 13 4-4m-6 6-1 1a3.5 3.5 0 0 1-5-5l4-4a3.5 3.5 0 0 1 5 0m2 2 1-1a3.5 3.5 0 0 1 5 5l-4 4a3.5 3.5 0 0 1-5 0" transform="translate(2 1)" />
                    </svg>
                  {:else}{short}{/if}
                </button>
              {/each}
            </div>
            <div class="format-group" role="group" aria-label="Paragraph and block formatting">
              <button type="button" aria-label="Alignment" title="Alignment"
                disabled={!selectedAssignment} popovertarget={alignmentMenuId}>
                <svg viewBox="0 0 24 24" aria-hidden="true">
                  <path d={selectedText?.alignment === 'center' ? 'M4 5h16M8 10h8M4 15h16M8 20h8'
                    : selectedText?.alignment === 'right' ? 'M4 5h16m-8 5h8M4 15h16m-8 5h8' : 'M4 5h16M4 10h8m-8 5h16M4 20h8'} />
                </svg>
              </button>
              <button type="button" aria-label="Bulleted list" title="Bulleted list" disabled={!activeEditor}
                onpointerdown={(event) => event.preventDefault()}
                onclick={() => activeEditor?.formatText('bulletList')}>
                <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M9 6h11M9 12h11M9 18h11M4 6h.01M4 12h.01M4 18h.01" /></svg>
              </button>
            </div>
            <button type="button" aria-label="Undo" title="Undo" disabled={!history.length} onclick={undo}>
              <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M9 5 4 10l5 5m-5-5h10a5 5 0 0 1 0 10" /></svg>
            </button>
            <div id={alignmentMenuId} class="alignment-menu" popover bind:this={alignmentMenu}
              role="group" aria-label="Alignment options">
              {#if selectedText}
                <div class="alignment-options" role="group" aria-label="Text alignment">
                  <span>Text alignment</span>
                  {#each textAlignments as [value, label]}
                    <button type="button" aria-pressed={(selectedText.alignment ?? 'left') === value}
                      onclick={() => { remember(); selectedText.alignment = value; alignmentMenu?.hidePopover(); }}>{label}</button>
                  {/each}
                </div>
              {/if}
              {#if selectedAssignment}
                <div class="alignment-options" role="group" aria-label="Vertical alignment">
                  <span>Vertical alignment</span>
                  {#each verticalAlignments as [value, label]}
                    <button type="button" aria-pressed={(selectedAssignment.vertical ?? 'start') === value}
                      onclick={() => { remember(); selectedAssignment.vertical = value; alignmentMenu?.hidePopover(); }}>{label}</button>
                  {/each}
                </div>
              {/if}
            </div>
            {#if activeEditor && draft.sections.some(({ wrap }) => wrap && wrap.textId === activeBlock)}
              <button type="button" onpointerdown={(event) => event.preventDefault()} onclick={moveWrappedImageHere}>Move image here</button>
            {/if}
          </div>
        {/if}
      </div>
      <div class="editor">
        {#each draft.sections as section, index (section.id)}
          {@const layout = draft.layouts.find(({ id }) => id === section.layoutId)!}
          {@const rows = sectionRows(section, layout)}
          <section class="edit-section" data-section={section.id} aria-label={`Section ${index + 1}`}>
            {#if index > 0 && section.breakBefore}<SectionBreak />{/if}
            {#each rows as row, rowIndex (row.id)}
              {@const wrap = rowWrap(draft, section, row.slots)}
              <div class="grid" style:--columns={wrap ? "minmax(0, 1fr)" : columns(row.slots)}>
                {#each row.slots as slot, slotIndex (slot.id)}
                  {@const block = blockAt(draft, section.id, slot.id)}
                  {@const slotName = row.id !== null ? `Row ${rowIndex + 1} · Full width` : row.slots.length === 1 ? 'Full width' : slotIndex === 0 ? 'Left' : 'Right'}
                  {@const slotKey = `${section.id}/${slot.id}`}
                  {@const vertical = draft.assignments.find((entry) => entry.sectionId === section.id && entry.slotId === slot.id)?.vertical}
                  {#if !wrap || block?.id !== wrap.imageId}
                    <div class="edit-slot" class:link-slot={block?.kind === 'link'} class:gallery-slot={block?.kind === 'gallery'}
                      role={block?.kind === 'gallery' ? 'group' : undefined}
                      aria-label={block?.kind === 'gallery' ? 'Gallery block' : undefined} style:--slot-alignment={vertical ?? 'start'} class:active={!!block && activeBlock === block.id} data-block={block?.id}>
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
                                      {@render editPhotos(wrap.image, section.id)}
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
                            {:else if block.kind === 'link'}
                              <div class="link-preview-editor">
                                <LinkPreviewCard url={block.url} editable />
                                {#if activeBlock === block.id || !publicUrl(block.url.trim())}
                                  <label>Link URL
                                    <input type="url" value={block.url} placeholder="https://example.org/article"
                                      oninput={(event) => { remember(); block.url = event.currentTarget.value; }} />
                                  </label>
                                  {#if block.url && !publicUrl(block.url.trim())}
                                    <small>Enter a full HTTP or HTTPS URL.</small>
                                  {/if}
                                  <div class="buttons">
                                    {#each linkPreviewSamples as sample (sample.url)}
                                      <button type="button" onclick={() => { remember(); block.url = sample.url; }}>
                                        {sample.label}
                                      </button>
                                    {/each}
                                  </div>
                                {/if}
                              </div>
                            {:else}
                              {@render editPhotos(block)}
                            {/if}
                          {/key}
                        {:else}
                          <div class="empty-slot">
                            <button type="button" aria-label={`Add block · Section ${index + 1} · ${slotName}`}
                              aria-expanded={picker === slotKey}
                              onclick={() => picker = picker === slotKey ? null : slotKey}>+</button>
                            {#if picker === slotKey}
                              <button type="button" onclick={() => addBlock(section.id, slot.id, 'text')}>Text</button>
                              <button type="button" onclick={() => addBlock(section.id, slot.id, 'image')}>Image</button>
                              <button type="button" onclick={() => addBlock(section.id, slot.id, 'gallery')}>Gallery</button>
                              <button type="button" onclick={() => addBlock(section.id, slot.id, 'link')}>Link preview</button>
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
        <button class="add-section" type="button" disabled={!!movingBlock} aria-expanded={layoutSection === 'new'} onclick={() => openLayout('new')}>Add section</button>
        {#if layoutSection === 'new'}{@render layoutEditor()}{/if}
      </div>
    {:else}
      {#key view}
        <SectionNodeView {draft} feed={view === 'feed'} onReadMore={() => view = 'node'} />
      {/key}
    {/if}
  </article>
  {#if view === 'edit' && sidebarOpen}
    {@const index = draft.sections.findIndex(({ id }) => id === activeSection)}
    {@const section = draft.sections[index]}
    <aside class="editor-sidebar" aria-label="Editor controls">
      <div class="sidebar-heading">
        <h2>Editor controls</h2>
        <button type="button" aria-label="Collapse editor controls" onclick={() => sidebarOpen = false}>×</button>
      </div>
      {#if !movingBlock && section}
        <h3>Section {index + 1}</h3>
        <div class="section-tools">
          {#if index > 0}
            <button type="button" onclick={() => {
              remember();
              section.breakBefore = !section.breakBefore;
            }}>{section.breakBefore ? 'Remove section break' : 'Add section break'}</button>
          {/if}
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
      {/if}
      {#if !movingBlock && activeBlock && selectedAssignment}
        <div class="block-tools">
          <button class="move-block" type="button"
            disabled={!!(mediaSection && mediaRow && rowWrap(draft, mediaSection, mediaRow.slots)) ||
              (draft.sections.length === 1 && !!mediaSection && !!mediaLayout &&
                sectionRows(mediaSection, mediaLayout).reduce((count, row) => count + row.slots.length, 0) < 2)}
            onclick={() => { if (activeBlock) void startMove(activeBlock); }}>Move block</button>
          <button type="button" onclick={() => { if (activeBlock) removeBlock(activeBlock); }}>Remove block</button>
        </div>
      {/if}
      {#if !movingBlock && mediaBlock && 'photos' in mediaBlock}
        {#if mediaBlock.kind === 'gallery'}{@render gallerySettings(mediaBlock)}{/if}
        {#if selectedPhoto && mediaSection && mediaRow}
          {@render photoSettings(selectedPhoto, mediaBlock, mediaSection, mediaRow.slots)}
        {/if}
      {:else if !section && !movingBlock}
        <p>Select a section or block to edit its settings.</p>
      {/if}
    </aside>
  {/if}
  </div>

  <details class="inspector">
    <summary>Backend data preview</summary>
    <pre>{JSON.stringify(draft, null, 2)}</pre>
  </details>
</div>

{#snippet editPhotos(block: Extract<Block, { photos: Photo[] }>, wrappedSection?: string)}
  {#snippet editPhoto(photo: Photo, neighbor = false)}
    {#if neighbor}
      {@const src = publicUrl(photo.src)}
      <figure><div class="neighbor-frame">
        {#if src}<PhotoFrame {photo} {src} gallery />{/if}
      </div></figure>
    {:else}
      <figure class="edit-photo" style:--photo-width={(wrappedSection ? 100 : photo.frame?.width ?? 100) + '%'}>
        <button class="photo-button"
          class:draggable-photo={!!wrappedSection} draggable={wrappedSection ? 'true' : 'false'}
          title={wrappedSection ? 'Drag to another paragraph, or click to edit image' : undefined}
          ondragstart={(event) => { if (wrappedSection) startImageDrag(event, wrappedSection); }} type="button" aria-label={`Edit ${photo.alt || 'image'}`}
          aria-pressed={photoDetails === photo.id}
          onclick={() => {
            activeBlock = block.id;
            activeEditor = undefined;
            photoDetails = photo.id;
          }}>
          {#if publicUrl(photo.src)}
            <PhotoFrame {photo} src={publicUrl(photo.src)!} gallery={block.kind === 'gallery' && block.galleryStyle !== 'masonry'} />
          {:else}
            Add image details
          {/if}
        </button>
        {#if photo.caption && photo.captionPlacement !== 'overlay'}<figcaption>{photo.caption}</figcaption>{/if}
      </figure>
    {/if}
  {/snippet}
  {#if block.kind === 'gallery'}
    <GalleryLayout photos={block.photos} layout={block.galleryStyle} width={block.galleryWidth} masonryAlignment={block.masonryAlignment} slideshow={block.slideshow} selected={photoDetails} onNavigate={() => photoDetails = null} photo={editPhoto} />
  {:else}
    {#each block.photos as photo (photo.id)}{@render editPhoto(photo)}{/each}
  {/if}
{/snippet}

{#snippet photoSettings(photo: Photo, block: Extract<Block, { photos: Photo[] }>, section: Section, slots: Slot[])}
  <div class="photo-details" role="group" aria-labelledby={`${alignmentMenuId}-image`}>
    <h3 id={`${alignmentMenuId}-image`}>Image settings</h3>
    <label>Image URL<input type="url" value={photo.src} oninput={(event) => { remember(); photo.src = event.currentTarget.value; }} /></label>
    <label>Alternative text<input value={photo.alt} oninput={(event) => { remember(); photo.alt = event.currentTarget.value; }} /></label>
    <label>Caption<input value={photo.caption} oninput={(event) => { remember(); photo.caption = event.currentTarget.value; }} /></label>
    <label>Caption placement
      <select value={photo.captionPlacement ?? 'below'} onchange={(event) => {
        remember();
        photo.captionPlacement = event.currentTarget.value === 'overlay' ? 'overlay' : 'below';
      }}>
        <option value="below">Centered below image</option>
        <option value="overlay">Over the image</option>
      </select>
    </label>
    <PhotoFramingControls {photo} src={publicUrl(photo.src)} gallery={block.kind === 'gallery' && block.galleryStyle !== 'masonry'} fitTile={block.kind === 'gallery' && block.galleryStyle === 'mosaic' && block.photos.length > 1}
      slideshowFit={block.galleryStyle === 'slideshow' ? block.slideshow?.fit ?? 'contain' : undefined}
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
    <div class="buttons photo-actions">
      <button type="button" onclick={() => photoDetails = null}>Done</button>
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
  </div>
{/snippet}

{#snippet gallerySettings(block: Extract<Block, { photos: Photo[] }>)}
    <div class="gallery-tools" role="group" aria-labelledby={`${alignmentMenuId}-gallery`}>
      <h3 id={`${alignmentMenuId}-gallery`}>Gallery settings</h3>
      <label>Gallery layout
        <select value={block.galleryStyle ?? 'grid'} onchange={(event) => {
          remember();
          block.galleryStyle = event.currentTarget.value as GalleryStyle;
        }}>
          <option value="grid">Grid</option>
          <option value="mosaic">Mosaic</option>
          <option value="masonry">Masonry</option>
          <option value="slideshow">Slideshow</option>
        </select>
      </label>
      <label>Gallery width
        <select value={block.galleryWidth ?? 'full'} onchange={(event) => {
          remember();
          block.galleryWidth = event.currentTarget.value === 'centered' ? 'centered' : 'full';
        }}>
          <option value="full">Full width</option>
          <option value="centered">Centered</option>
        </select>
      </label>
      {#if block.galleryStyle === 'slideshow'}
        <label>Slideshow height
          <select value={block.slideshow?.height ?? 'standard'} onchange={(event) =>
            updateSlideshow(block, { height: event.currentTarget.value as SlideshowSettings['height'] })}>
            <option value="compact">Compact</option>
            <option value="standard">Standard</option>
            <option value="tall">Tall</option>
          </select>
        </label>
        <label>Image fit
          <select value={block.slideshow?.fit ?? 'contain'} onchange={(event) =>
            updateSlideshow(block, { fit: event.currentTarget.value === 'cover' ? 'cover' : 'contain' })}>
            <option value="contain">Show whole image</option>
            <option value="cover">Fill frame</option>
          </select>
        </label>
        <label>Slideshow presentation
          <select value={block.slideshow?.neighbors ? 'peek' : 'single'} onchange={(event) =>
            updateSlideshow(block, { neighbors: event.currentTarget.value === 'peek' })}>
            <option value="single">One image</option>
            <option value="peek">Show neighboring edges</option>
          </select>
        </label>
      {/if}
      {#if block.galleryStyle === 'masonry'}
        <label>Masonry vertical alignment
          <select value={block.masonryAlignment ?? 'top'} onchange={(event) => {
            remember();
            block.masonryAlignment = event.currentTarget.value === 'center' ? 'center' : 'top';
          }}>
            <option value="top">Top</option>
            <option value="center">Center</option>
          </select>
        </label>
        <button type="button" disabled={block.photos.length < 2}
          onclick={() => randomizeMasonryHeights(block.photos)}>Randomize image heights</button>
      {/if}
      <button class="add-gallery-image" type="button" onclick={() => {
        const photo = newPhoto();
        remember();
        block.photos.push(photo);
        activeBlock = block.id;
        activeEditor = undefined;
        photoDetails = photo.id;
      }}>Add gallery image</button>
    </div>
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
  @media (min-width: 65.001rem) {
    .workbench:not(.narrow):has(.with-sidebar) { max-width: none; padding-inline-end: 0; }
  }
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
  .node { --node-title-size: clamp(1.5rem, 5cqi, 2.75rem); }
  .node :global(:is(.atlas-story-text, .markdown) :is(h2, h3, h4, h5, h6)) {
    font-size: max(var(--heading-min), calc(var(--node-title-size) * var(--heading-ratio)));
    font-weight: 650;
    line-height: 1.25;
    margin-block: 1.25rem .5rem;
  }
  .node :global(:is(.atlas-story-text, .markdown) h2) { --heading-ratio: .8; --heading-min: 1.375rem; }
  .node :global(:is(.atlas-story-text, .markdown) h3) { --heading-ratio: .68; --heading-min: 1.25rem; }
  .node :global(:is(.atlas-story-text, .markdown) h4) { --heading-ratio: .57; --heading-min: 1.125rem; }
  .node :global(:is(.atlas-story-text, .markdown) h5) { --heading-ratio: .48; --heading-min: 1.0625rem; }
  .node :global(:is(.atlas-story-text, .markdown) h6) { --heading-ratio: .42; --heading-min: 1rem; }
  .node :global(:is(.atlas-story-text, .markdown) > :is(h2, h3, h4, h5, h6):first-child),
  .node :global(.atlas-story-text > .ProseMirror-widget:first-child + :is(h2, h3, h4, h5, h6)) {
    margin-block-start: 0;
  }
  .node :global(:is(.atlas-story-text, .markdown) > :is(h2, h3, h4, h5, h6):last-child) {
    margin-block-end: 0;
  }
  header { margin-bottom: clamp(.8rem, 3vw, 1.7rem); }
  h1, .title { font-size: var(--node-title-size); font-weight: 650; margin: 0; line-height: 1.16; overflow-wrap: anywhere; }
  .title { width: 100%; padding: .35rem 0; border: 0; border-bottom: 1px solid var(--atlas-border); border-radius: 0; }
  .editor, .edit-section, .edit-slot, .layout-panel { display: grid; gap: .75rem; min-width: 0; }
  button.add-section {
    justify-self: center;
    min-height: 2.75rem;
    padding: .65rem 1.25rem;
    font-weight: 650;
    color: var(--atlas-on-accent, white);
    background: var(--atlas-accent);
    border-color: var(--atlas-accent);
  }
  .editing-tools { position: sticky; top: 0; z-index: 5; background: var(--atlas-surface); padding-block: .4rem; }
  .formatting {
    display: flex; align-items: center; gap: .25rem; overflow-x: auto;
    padding: .25rem; white-space: nowrap;
    border-block: 1px solid color-mix(in srgb, var(--atlas-border) 35%, transparent);
  }
  .formatting > *, .format-group > * { flex: none; }
  .format-group {
    display: flex; align-items: center; padding-inline: .2rem;
    border-inline-start: 1px solid color-mix(in srgb, var(--atlas-border) 45%, transparent);
  }
  .heading-select { width: auto; font-size: .875rem; }
  .formatting button { display: grid; place-items: center; padding: .25rem; border-color: transparent; font-weight: 650; }
  .formatting button:hover:not(:disabled), .formatting button:focus-visible {
    background: color-mix(in srgb, var(--atlas-accent) 12%, var(--atlas-surface));
  }
  .formatting button, .heading-select, .section-tools button, .block-tools button { min-width: 2rem; min-height: 2.5rem; }
  .formatting .italic { font-style: italic; }
  .formatting .underline { text-decoration: underline; }
  .formatting svg { width: 1.1rem; height: 1.1rem; fill: none; stroke: currentColor; stroke-width: 1.8; stroke-linecap: round; stroke-linejoin: round; }
  .alignment-menu {
    box-sizing: border-box; width: min(15rem, calc(100vw - 1rem)); padding: .65rem;
    border: 1px solid var(--atlas-border); border-radius: var(--atlas-radius, .5rem);
    color: var(--atlas-text); background: var(--atlas-surface); box-shadow: 0 .25rem 1rem #0003;
  }
  @supports (position-area: block-end) {
    .alignment-menu { inset: auto; margin: .25rem 0; position-area: block-end span-inline-start; position-try-fallbacks: flip-inline, flip-block; }
  }
  .alignment-options { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: .25rem; }
  .alignment-options + .alignment-options { margin-top: .65rem; }
  .alignment-options span { grid-column: 1 / -1; font-size: .8rem; font-weight: 650; }
  .alignment-options button { font-size: .875rem; font-weight: 500; border-color: var(--atlas-border); }
  .alignment-options button[aria-pressed="true"] {
    border-color: var(--atlas-accent); background: color-mix(in srgb, var(--atlas-accent) 12%, var(--atlas-surface));
  }
  .edit-section { padding-block: .5rem 1rem; }
  .link-preview-editor { display: grid; gap: .6rem; }
  .gallery-tools, .photo-details {
    display: grid; gap: .65rem; padding-block: .75rem;
    border-top: 1px solid color-mix(in srgb, var(--atlas-border) 40%, transparent);
  }
  .gallery-tools .add-gallery-image, .photo-actions { justify-self: start; }
  .section-tools { margin-block-end: .75rem; }
  .grid { display: grid; grid-template-columns: var(--columns); gap: .75rem; align-items: stretch; }
  .edit-slot { position: relative; min-width: 0; border-radius: .25rem; }
  .edit-slot.active, .edit-slot:focus-within { outline: 1px solid var(--atlas-border); outline-offset: .2rem; }
  .edit-slot { display: grid; align-items: var(--slot-alignment); }
  .slot-content { min-width: 0; }
  .destination { position: absolute; inset-block-start: 0; inset-inline-end: 0; z-index: 4; border-color: var(--atlas-accent); }
  .block-tools { margin-block-end: .75rem; }
  .photo-button { display: block; width: var(--photo-width, 100%); margin-inline: auto; padding: 0; border: 0; text-align: start; }
  .draggable-photo { cursor: grab; }
  .edit-slot :global(.image-drop-left) { box-shadow: inset 3px 0 var(--atlas-accent); }
  .edit-slot :global(.image-drop-right) { box-shadow: inset -3px 0 var(--atlas-accent); }
  figure { margin: 0; }
  figcaption { text-align: center; font-size: .85rem; color: var(--atlas-muted); width: var(--photo-width); margin: .25rem auto 0; }
  .editor-frame.with-sidebar { display: grid; grid-template-columns: minmax(0, 1fr) clamp(17rem, 24vw, 24rem); gap: 1rem; align-items: start; }
  .editor-sidebar {
    position: sticky; top: .5rem; max-height: calc(100dvh - 1rem); overflow: auto;
    padding: .75rem; background: var(--atlas-surface);
    border: 1px solid var(--atlas-border); border-radius: var(--atlas-radius, .5rem);
  }
  .sidebar-heading { display: flex; justify-content: space-between; align-items: center; gap: .5rem; margin-bottom: .65rem; }
  .editor-sidebar h2 { font-size: 1rem; margin: 0; }
  .editor-sidebar h3 { font-size: .9rem; margin: 0; }
  .editor-sidebar > h3 { margin-bottom: .45rem; }
  .editor-sidebar p { margin: 0; }
  .editor-sidebar :global(label) { font-size: .875rem; font-weight: 600; }
  .editor-sidebar :global(input:not([type="range"])),
  .editor-sidebar :global(select) {
    box-sizing: border-box; width: 100%; min-width: 0; min-height: 2.5rem;
    padding: .4rem .55rem; font: inherit; font-weight: 400; color: var(--atlas-text);
    border: 1px solid var(--atlas-border); border-radius: var(--atlas-radius, .45rem);
    background: color-mix(in srgb, var(--atlas-accent) 4%, var(--atlas-surface));
  }
  .editor-sidebar :global(:is(button, select, input):focus-visible),
  .formatting :is(button, select):focus-visible { outline: 2px solid var(--atlas-accent); outline-offset: 1px; }
  .editor-sidebar :global(input[type="range"]) {
    display: block; width: 100%; min-width: 0; height: 1.75rem; margin: 0; padding: 0;
    border: 0; background: transparent; accent-color: var(--atlas-accent); cursor: pointer;
  }
  @media (max-width: 65rem) {
    .editor-frame.with-sidebar { display: block; }
    .editor-sidebar { position: fixed; inset: auto .5rem .5rem; max-height: 38dvh; z-index: 10; }
    .editor-frame.with-sidebar { padding-bottom: 39dvh; }
  }
  .narrow .editor-frame.with-sidebar { display: block; padding-bottom: 39dvh; }
  .narrow .editor-sidebar { position: fixed; inset: auto .5rem .5rem; max-height: 38dvh; z-index: 10; }
  .edit-slot label, .editor-sidebar label, .layout-panel label { display: grid; gap: .25rem; }
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
    .formatting button, .section-tools button, .block-tools button, .empty-slot button { min-height: 2.75rem; min-width: 2.75rem; }
  }
</style>

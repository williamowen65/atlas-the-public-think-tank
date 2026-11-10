<!--
  Storybook-only research prototype. This uses the real Tiptap editing engine,
  but is not wired to creation, API writes, or persisted Atlas block identity.
-->
<script lang="ts">
  import { onMount } from 'svelte';
  import { Editor, Extension, Node } from '@tiptap/core';
  import StarterKit from '@tiptap/starter-kit';
  import { NodeSelection, Plugin, TextSelection } from '@tiptap/pm/state';
  import { Decoration, DecorationSet } from '@tiptap/pm/view';
  import { publicUrl } from '../../src/lib/api/nodePage';
  import type { AuthorBlock } from '../../src/lib/api/contentAuthoring';
  import { blocksToTiptapHtml, tiptapDocumentToBlocks } from './tiptapBlocks';

  let {
    initialBlocks,
    onChange,
    styleMode = 'atlas',
  }: {
    initialBlocks: AuthorBlock[];
    onChange?: (blocks: AuthorBlock[]) => void;
    styleMode?: 'atlas' | 'editorial';
  } = $props();

  // Focus is separate from ProseMirror's retained selection: a click elsewhere
  // in the node or on the page removes BOTH the gutter and block indentation.
  let editorFocused = false;
  let controlsVisible = $state(false);

  // The document may only hold real Atlas blocks. StarterKit's generic doc
  // otherwise lets Backspace create anonymous top-level paragraphs (phantom IDs).
  const AtlasDocument = Node.create({
    name: 'doc',
    topNode: true,
    content: '(atlasText | atlasImage | atlasUnsupported)+',
  });

  // Render the active block class through ProseMirror's decoration system.
  // DOM classList edits done in onTransaction are overwritten when a text node
  // rerenders, which made the rail overlap active Markdown while images worked.
  const ActiveBlockDecoration = Extension.create({
    name: 'atlasActiveBlockDecoration',
    addProseMirrorPlugins() {
      return [new Plugin({
        props: {
          decorations(state) {
            if (!editorFocused && !deleteTarget) return DecorationSet.empty;
            // Keep the exact block highlighted while its delete confirmation
            // owns keyboard focus. ProseMirror retains the block's stable key.
            let index = state.selection.$from.index(0);
            if (deleteTarget) {
              index = -1;
              for (let i = 0; i < state.doc.childCount; i++) {
                if (String(state.doc.child(i).attrs.key) === deleteTarget.key) {
                  index = i;
                  break;
                }
              }
            }
            if (index < 0 || index >= state.doc.childCount) return DecorationSet.empty;
            let from = 0;
            for (let i = 0; i < index; i++) from += state.doc.child(i).nodeSize;
            return DecorationSet.create(state.doc, [
              Decoration.node(from, from + state.doc.child(index).nodeSize, {
                class: deleteTarget
                  ? 'atlas-story-active-block atlas-story-pending-delete'
                  : 'atlas-story-active-block',
              }),
            ]);
          },
        },
      })];
    },
  });

  // A Markdown block stays one Atlas block even when it contains multiple
  // paragraphs/headings. This wrapper carries its stable key through editing,
  // reordering and conversion back into the existing AuthorBlock[] model.
  const AtlasText = Node.create({
    name: 'atlasText',
    group: 'block',
    // Do not allow an image to be dropped INSIDE Markdown; images are sibling Atlas blocks.
    content: '(paragraph | heading | bulletList | orderedList | blockquote | codeBlock | horizontalRule)+',
    defining: true,
    addAttributes() {
      return { key: { default: '' } };
    },
    parseHTML() {
      return [{
        tag: 'div[data-atlas-text]',
        getAttrs: element => typeof element === 'string' ? false :
          { key: element.getAttribute('data-atlas-key') || '' },
      }];
    },
    renderHTML({ node }) {
      return ['div', {
        'data-atlas-text': 'true',
        'data-atlas-key': node.attrs.key,
        class: 'atlas-story-text',
      }, 0];
    },
  });

  // Preserve future/unsupported block kinds without pretending to edit them.
  const AtlasUnsupported = Node.create({
    name: 'atlasUnsupported',
    group: 'block',
    atom: true,
    selectable: true,
    addAttributes() {
      return { key: { default: '' }, kind: { default: 'unknown' } };
    },
    parseHTML() {
      return [{
        tag: 'div[data-atlas-unsupported]',
        getAttrs: element => typeof element === 'string' ? false : {
          key: element.getAttribute('data-atlas-key') || '',
          kind: element.getAttribute('data-atlas-kind') || 'unknown',
        },
      }];
    },
    renderHTML({ node }) {
      return ['div', {
        'data-atlas-unsupported': 'true',
        'data-atlas-key': node.attrs.key,
        'data-atlas-kind': node.attrs.kind,
        class: 'atlas-story-unsupported',
        contenteditable: 'false',
      }, 'Unsupported ' + node.attrs.kind + ' block (preserved)'];
    },
  });

  // A purpose-built image atom is closer to our server-side image block than
  // Tiptap's default inline <img>. It also keeps alt text and caption together.
  const AtlasImage = Node.create({
    name: 'atlasImage',
    group: 'block',
    atom: true,
    draggable: true,
    selectable: true,
    addAttributes() {
      return {
        src: { default: '' },
        alt: { default: '' },
        caption: { default: '' },
        key: { default: '' },
      };
    },
    parseHTML() {
      return [{
        tag: 'figure[data-atlas-image]',
        getAttrs: element => {
          if (typeof element === 'string') return false;
          return {
            src: element.getAttribute('data-src') || element.querySelector('img')?.getAttribute('src') || '',
            alt: element.getAttribute('data-alt') || element.querySelector('img')?.getAttribute('alt') || '',
            caption: element.getAttribute('data-caption') || element.querySelector('figcaption')?.textContent || '',
            key: element.getAttribute('data-atlas-key') || '',
          };
        },
      }];
    },
    renderHTML({ node }) {
      const src = publicUrl(String(node.attrs.src ?? '')) || '';
      const figureAttrs = {
        'data-atlas-image': 'true',
        'data-atlas-key': node.attrs.key,
        'data-src': node.attrs.src,
        'data-alt': node.attrs.alt,
        'data-caption': node.attrs.caption,
        class: 'atlas-story-figure',
      };
      const editButton = ['button', {
        type: 'button', contenteditable: 'false', draggable: 'false',
        'data-atlas-edit-image': 'true', class: 'atlas-story-image-action',
        'aria-label': src ? 'Edit image details' : 'Add image details',
      }, src ? 'Edit image' : 'Add image details'];
      // A new image has a real, in-flow placeholder at its intended position.
      // Its editor panel is anchored inside this reserved image slot.
      if (!src) return ['figure', figureAttrs,
        ['div', { class: 'atlas-story-image-placeholder', 'aria-label': 'Unfinished image block' },
          'Image details will appear here'],
        editButton,
      ];
      return ['figure', figureAttrs,
        ['img', { src, alt: node.attrs.alt }],
        ['figcaption', {}, node.attrs.caption || ''],
        editButton,
      ];
    },
  });

  let canvasEl: HTMLDivElement;
  let shellEl: HTMLDivElement;
  let blockLayout = $state({ top: 0, bottom: 0, right: 0, center: 0 });
  let insertMenuLayout = $state({ top: 0, left: 0 });
  let activeIndex = $state(0);
  let blockCount = $state(initialBlocks.length);
  let deleteTarget = $state<{ key: string; kind: string; summary: string } | null>(null);
  let deleteConfirmPosition = $state({ top: 0, left: 0 });
  let railFrame = 0;
  let linkFormOpen = $state(false);
  let linkHref = $state('');
  let linkText = $state('');
  let linkError = $state('');
  let linkSelection = $state<{ from: number; to: number } | null>(null);
  let linkFormPosition = $state({ top: 0, left: 0 });
  let editorState = $state<{ editor: Editor | null }>({ editor: null });
  let floatingOptionsOpen = $state(false);
  let insertSide = $state<'before' | 'after'>('after');
  let insertAnchorKey = $state('');
  let selectionPopup = $state<{ left: number; top: number } | null>(null);
  let imageFormOpen = $state(false);
  let imageError = $state('');
  let editingExisting = $state(false);
  let imageDraft = $state({ url: '', alt: '', caption: '' });
  let imageFormAnchorKey = $state<string | null>(null);
  let provisionalImageKey: string | null = null;
  let imageFormPosition = $state({ top: 0, left: 0 });
  let latestBlocks: AuthorBlock[] = initialBlocks;

  // The forms are positioned against the document, not the full node footer.
  // The node and forms therefore scroll together, including on long nodes.
  function findBlock(key: string): { from: number; nodeSize: number; attrs: Record<string, unknown> } | null {
    const editor = currentEditor();
    if (!editor) return null;
    let from = 0;
    for (let i = 0; i < editor.state.doc.childCount; i++) {
      const node = editor.state.doc.child(i);
      if (String(node.attrs.key) === key)
        return { from, nodeSize: node.nodeSize, attrs: node.attrs };
      from += node.nodeSize;
    }
    return null;
  }

  function positionAnchoredForms() {
    const editor = currentEditor();
    if (!shellEl || !editor || editor.isDestroyed) return;
    const shell = shellEl.getBoundingClientRect();
    const leftBound = Math.max(0, shellEl.clientWidth - 350);

    if (linkFormOpen && linkSelection) {
      try {
        const start = editor.view.coordsAtPos(linkSelection.from);
        const end = editor.view.coordsAtPos(linkSelection.to);
        linkFormPosition = {
          top: Math.max(0, Math.round(Math.max(start.bottom, end.bottom) - shell.top + 9)),
          left: Math.max(0, Math.min(leftBound,
            Math.round((start.left + end.right) / 2 - shell.left - 160))),
        };
      } catch { /* Keep last valid position during a document remount. */ }
    }

    if (deleteTarget) {
      const block = Array.from(editor.view.dom.children)
        .find(element => element.getAttribute('data-atlas-key') === deleteTarget?.key);
      if (block) {
        const rect = block.getBoundingClientRect();
        // Align to the selected block (not to the entire document's bottom).
        // Clamp horizontally so the confirmation also fits on phone width.
        const maxLeft = Math.max(0, shellEl.clientWidth - 310);
        deleteConfirmPosition = {
          top: Math.max(0, Math.round(rect.top - shell.top + 5)),
          left: Math.max(0, Math.min(maxLeft, Math.round(rect.left - shell.left + 10))),
        };
      }
    }

    if (imageFormOpen && imageFormAnchorKey) {
      const figure = Array.from(shellEl.querySelectorAll('figure[data-atlas-image]'))
        .find(element => element.getAttribute('data-atlas-key') === imageFormAnchorKey);
      if (figure) {
        const rect = figure.getBoundingClientRect();
        imageFormPosition = {
          top: Math.max(0, Math.round(rect.top - shell.top + 2)),
          left: Math.max(0, Math.min(leftBound, Math.round(rect.left - shell.left + 4))),
        };
      }
    }
  }

  // The popover remembers the selected Atlas block's key. Changing the
  // Before/After choice or focusing the image form must not move the anchor.
  function insertionPoint(editor: Editor): number {
    let from = 0;
    const children = editor.state.doc;
    for (let i = 0; i < children.childCount; i++) {
      const node = children.child(i);
      if (String(node.attrs.key) === insertAnchorKey)
        return insertSide === 'before' ? from : from + node.nodeSize;
      from += node.nodeSize;
    }
    // Selection is the fallback when a block has been removed while a menu is open.
    const index = Math.min(editor.state.selection.$from.index(0), children.childCount - 1);
    from = 0;
    for (let i = 0; i < index; i++) from += children.child(i).nodeSize;
    return insertSide === 'before' ? from : from + children.child(index).nodeSize;
  }

  function openInsertChoices(side: 'before' | 'after') {
    const editor = currentEditor();
    if (!editor) return;
    const anchorKey = String(editor.state.doc.child(activeIndex)?.attrs.key || '');
    if (floatingOptionsOpen && insertSide === side && insertAnchorKey === anchorKey) {
      floatingOptionsOpen = false;
      return;
    }
    insertAnchorKey = anchorKey;
    insertSide = side;
    // The menu opens from the chosen plus and can cover the document by design.
    const offset = side === 'before' ? blockLayout.top : blockLayout.bottom;
    insertMenuLayout = {
      top: offset + 8,
      left: Math.max(0, Math.min(shellEl.clientWidth - 282, blockLayout.center - 18)),
    };
    selectionPopup = null;
    floatingOptionsOpen = true;
  }

  function insertTextBlock() {
    const editor = currentEditor();
    if (!editor) return;
    const position = insertionPoint(editor);
    const key = crypto.randomUUID();
    editor.chain().focus().insertContentAt(position, {
      type: 'atlasText',
      attrs: { key },
      content: [{ type: 'paragraph' }],
    }).setTextSelection(position + 2).run();
    floatingOptionsOpen = false;
  }


  function currentEditor(): Editor | null { return editorState.editor; }

  function format(kind: 'bold' | 'italic' | 'heading' | 'bulletList') {
    const editor = currentEditor();
    if (!editor) return;
    if (kind === 'bold') editor.chain().focus().toggleBold().run();
    else if (kind === 'italic') editor.chain().focus().toggleItalic().run();
    else if (kind === 'heading') editor.chain().focus().toggleHeading({ level: 2 }).run();
    else editor.chain().focus().toggleBulletList().run();
  }

  // Controls are anchored to the actual top/bottom/right of the selected
  // Atlas block. No gutter, caret tracking, or scroll chasing. Both images and
  // Markdown receive the identical selected decoration and actions.
  function repositionGutter(editor: Editor) {
    if (!shellEl || !editor.view?.dom?.isConnected) return;
    const index = Math.min(editor.state.selection.$from.index(0), editor.state.doc.childCount - 1);
    activeIndex = Math.max(0, index);
    blockCount = editor.state.doc.childCount;

    cancelAnimationFrame(railFrame);
    if (!editorFocused) {
      selectionPopup = null;
      return;
    }

    railFrame = requestAnimationFrame(() => {
      if (!shellEl || editor.isDestroyed || !editor.view.dom.isConnected || !editorFocused) return;
      const active = editor.view.dom.children.item(editor.state.selection.$from.index(0));
      if (!active) return;
      const bounds = active.getBoundingClientRect();
      const shell = shellEl.getBoundingClientRect();
      blockLayout = {
        top: Math.round(bounds.top - shell.top),
        bottom: Math.round(bounds.bottom - shell.top),
        right: Math.round(bounds.right - shell.left),
        center: Math.round((bounds.left + bounds.right) / 2 - shell.left),
      };

      const selection = editor.state.selection;
      if (selection instanceof TextSelection && !selection.empty && !linkFormOpen) {
        try {
          const start = editor.view.coordsAtPos(selection.from);
          const end = editor.view.coordsAtPos(selection.to);
          selectionPopup = {
            left: Math.max(0, Math.min(shellEl.clientWidth - 285,
              (start.left + end.right) / 2 - shell.left - 137)),
            top: Math.max(-42, Math.min(start.top, end.top) - shell.top - 48),
          };
        } catch {
          selectionPopup = null;
        }
      } else {
        selectionPopup = null;
      }
    });
  }

  // Reorder an entire top-level Atlas section (not one paragraph inside it).
  // The selection follows the moved section so the rail stays beside it.
  function moveCurrentBlock(direction: -1 | 1) {
    const editor = currentEditor();
    if (!editor) return;
    const { state } = editor;
    const index = state.selection.$from.index(0);
    const other = index + direction;
    if (other < 0 || other >= state.doc.childCount) return;
    const node = state.doc.child(index);
    const neighbor = state.doc.child(other);
    let from = 0;
    for (let i = 0; i < index; i++) from += state.doc.child(i).nodeSize;
    const destination = direction === -1 ? from - neighbor.nodeSize : from + neighbor.nodeSize;
    const transaction = state.tr.delete(from, from + node.nodeSize).insert(destination, node);
    if (node.type.name === 'atlasImage') {
      transaction.setSelection(NodeSelection.create(transaction.doc, destination));
    } else {
      transaction.setSelection(TextSelection.near(transaction.doc.resolve(destination + 1)));
    }
    editor.view.dispatch(transaction);
    editor.commands.focus();
    repositionGutter(editor);
  }

  // An in-page link form is safer than window.prompt in the Storybook iframe.
  // Capture the document selection BEFORE the input field takes focus.
  function openLinkForm() {
    const editor = currentEditor();
    if (!editor) return;
    const { from, to } = editor.state.selection;
    linkSelection = { from, to };
    linkText = editor.state.doc.textBetween(from, to, ' ');
    linkHref = String(editor.getAttributes('link').href ?? '');
    linkError = '';
    linkFormOpen = true;
    selectionPopup = null;
    positionAnchoredForms();
  }

  function saveLink() {
    const editor = currentEditor();
    if (!editor || !linkSelection) return;
    const url = linkHref.trim();
    let valid = false;
    try { valid = ['http:', 'https:'].includes(new URL(url).protocol); } catch { /* Invalid URL. */ }
    if (!valid) {
      linkError = 'Enter a full HTTP(S) URL.';
      return;
    }
    if (linkSelection.from === linkSelection.to) {
      editor.chain().focus().setTextSelection(linkSelection).insertContent({
        type: 'text', text: linkText.trim() || url,
        marks: [{ type: 'link', attrs: { href: url } }],
      }).run();
    } else {
      editor.chain().focus().setTextSelection(linkSelection).setLink({ href: url }).run();
    }
    linkFormOpen = false;
    linkSelection = null;
  }

  function unlink() {
    const editor = currentEditor();
    if (editor && linkSelection)
      editor.chain().focus().setTextSelection(linkSelection).unsetLink().run();
    linkFormOpen = false;
    linkSelection = null;
  }

  // Capture the selected block's stable key before the confirmation takes
  // keyboard focus. Confirmation is required for text AND images.
  function requestDeleteBlock() {
    const editor = currentEditor();
    if (!editor) return;
    const index = editor.state.selection.$from.index(0);
    const node = editor.state.doc.maybeChild(index);
    if (!node) return;
    const kind = node.type.name === 'atlasImage' ? 'image' :
      node.type.name === 'atlasText' ? 'text' : 'unsupported';
    deleteTarget = {
      key: String(node.attrs.key),
      kind,
      summary: kind === 'image' ? String(node.attrs.caption || node.attrs.alt || 'image') :
        node.textContent.slice(0, 90) || '(empty block)',
    };
    floatingOptionsOpen = false;
    // Refresh decorations even if the selection itself did not change.
    editor.view.dispatch(editor.state.tr.setMeta('atlasDeleteDialog', 'open'));
    requestAnimationFrame(positionAnchoredForms);
  }

  function cancelDeleteBlock() {
    deleteTarget = null;
    const editor = currentEditor();
    if (editor && !editor.isDestroyed) {
      editor.chain().focus().run();
      editor.view.dispatch(editor.state.tr.setMeta('atlasDeleteDialog', 'cancel'));
    }
  }

  function confirmDeleteBlock() {
    const editor = currentEditor();
    const target = deleteTarget;
    if (!editor || !target) return;
    const { state } = editor;
    let from = 0;
    for (let index = 0; index < state.doc.childCount; index++) {
      const node = state.doc.child(index);
      if (String(node.attrs.key) === target.key) {
        const transaction = state.tr;
        if (state.doc.childCount === 1) {
          // The custom document schema requires at least one Atlas block.
          // Replace the final section with a new empty text landing place.
          const empty = state.schema.nodes.atlasText.create(
            { key: crypto.randomUUID() }, state.schema.nodes.paragraph.create(),
          );
          transaction.replaceWith(from, from + node.nodeSize, empty);
        } else {
          transaction.delete(from, from + node.nodeSize);
        }
        transaction.setSelection(TextSelection.near(transaction.doc.resolve(
          Math.min(from, transaction.doc.content.size),
        )));
        editor.view.dispatch(transaction);
        editor.commands.focus();
        break;
      }
      from += node.nodeSize;
    }
    deleteTarget = null;
  }

  function insertDivider() {
    const editor = currentEditor();
    if (!editor) return;
    const position = insertionPoint(editor);
    editor.chain().focus().insertContentAt(position, {
      type: 'atlasText',
      attrs: { key: crypto.randomUUID() },
      content: [{ type: 'horizontalRule' }, { type: 'paragraph' }],
    }).run();
    floatingOptionsOpen = false;
  }

  // A new image is staged as an actual Atlas image node at the chosen
  // Before/After position. The placeholder reserves space for its editor.
  // Cancel removes this temporary node; Apply fills its metadata by stable key.
  function openImageForm(mode: 'new' | 'existing' = 'new') {
    const editor = currentEditor();
    if (!editor) return;
    if (imageFormOpen) cancelImageForm();
    editingExisting = mode === 'existing';
    imageError = '';
    const existing = editingExisting
      ? editor.state.doc.maybeChild(editor.state.selection.$from.index(0))
      : null;
    if (editingExisting && existing?.type.name !== 'atlasImage') return;

    if (existing) {
      imageFormAnchorKey = String(existing.attrs.key);
      imageDraft = {
        url: String(existing.attrs.src ?? ''),
        alt: String(existing.attrs.alt ?? ''),
        caption: String(existing.attrs.caption ?? ''),
      };
    } else {
      const key = crypto.randomUUID();
      const position = insertionPoint(editor);
      provisionalImageKey = key;
      imageFormAnchorKey = key;
      imageDraft = { url: '', alt: '', caption: '' };
      editor.chain().focus().insertContentAt(position, {
        type: 'atlasImage',
        attrs: { key, src: '', alt: '', caption: '' },
      }).setNodeSelection(position).run();
    }
    imageFormOpen = true;
    floatingOptionsOpen = false;
    requestAnimationFrame(positionAnchoredForms);
  }

  function cancelImageForm() {
    const editor = currentEditor();
    const key = provisionalImageKey;
    provisionalImageKey = null;
    imageFormOpen = false;
    imageFormAnchorKey = null;
    imageError = '';
    if (!editor || !key) return;
    const block = findBlock(key);
    if (!block) return;
    const { state } = editor;
    if (state.doc.childCount > 1) {
      editor.view.dispatch(state.tr.delete(block.from, block.from + block.nodeSize));
    } else {
      editor.view.dispatch(state.tr.replaceWith(block.from, block.from + block.nodeSize,
        state.schema.nodes.atlasText.create({ key: crypto.randomUUID() },
          state.schema.nodes.paragraph.create())));
    }
  }

  function saveImage() {
    const editor = currentEditor();
    if (!editor || !imageFormAnchorKey) return;
    if (!publicUrl(imageDraft.url)) {
      imageError = 'Enter a complete HTTP(S) image URL.';
      return;
    }
    if (!imageDraft.alt.trim()) {
      imageError = 'Alternative text is required.';
      return;
    }
    const block = findBlock(imageFormAnchorKey);
    if (!block) {
      imageError = 'This image block is no longer present.';
      return;
    }
    editor.chain().focus().setNodeSelection(block.from).updateAttributes('atlasImage', {
      src: imageDraft.url.trim(), alt: imageDraft.alt.trim(), caption: imageDraft.caption.trim(),
    }).run();
    provisionalImageKey = null;
    imageFormOpen = false;
    imageFormAnchorKey = null;
    imageError = '';
  }

  onMount(() => {
    const editor = new Editor({
      element: canvasEl,
      extensions: [
        StarterKit.configure({ document: false }),
        AtlasDocument,
        AtlasText,
        AtlasImage,
        AtlasUnsupported,
        ActiveBlockDecoration,
      ],
      content: blocksToTiptapHtml(initialBlocks),
      editorProps: {
        attributes: {
          class: 'atlas-story-rich-document',
          'aria-label': 'Node content: editable rich-text document',
        },
        handleDOMEvents: {
          pointerup: () => {
            // The browser updates ProseMirror's caret as the click finishes.
            // Pin the rail to that location; future scrolling never moves it.
            requestAnimationFrame(() => {
              if (!editor.isDestroyed && editorFocused) repositionGutter(editor);
            });
            return false;
          },
          pointerdown: (view, event) => {
            const target = event.target;
            if (!(target instanceof Element)) return false;
            const action = target.closest('[data-atlas-edit-image]');
            const figure = action?.closest('figure[data-atlas-key]');
            const key = figure?.getAttribute('data-atlas-key');
            if (!key) return false;
            event.preventDefault();
            const block = findBlock(key);
            if (!block) return true;
            view.dispatch(view.state.tr.setSelection(NodeSelection.create(view.state.doc, block.from)));
            openImageForm('existing');
            return true;
          },
        },
      },
      onFocus: ({ editor }) => {
        editorFocused = true;
        controlsVisible = true;
        editor.view.dispatch(editor.state.tr.setMeta('atlasEditorFocus', true));
        repositionGutter(editor);
      },
      onBlur: ({ editor }) => {
        editorFocused = false;
        controlsVisible = false;
        floatingOptionsOpen = false;
        selectionPopup = null;
        editor.view.dispatch(editor.state.tr.setMeta('atlasEditorFocus', false));
      },
      onTransaction: ({ editor }) => {
        editorState = { editor };
        repositionGutter(editor);
        requestAnimationFrame(positionAnchoredForms);
      },
      onUpdate: ({ editor }) => {
        const next = tiptapDocumentToBlocks(editor.getJSON(), latestBlocks);
        latestBlocks = next;
        onChange?.(next);
      },
    });
    editorState = { editor };
    repositionGutter(editor);
    const reposition = () => {
      // Link/image/delete forms remain anchored to their source content.
      // Do not reposition the section rail on scroll or resize.
      requestAnimationFrame(positionAnchoredForms);
    };
    window.addEventListener('resize', reposition);
    window.addEventListener('scroll', reposition, true);
    return () => {
      // Switching editor modes must not leak a staged, invalid image into the
      // canonical block-form draft when its inline settings were never applied.
      if (provisionalImageKey) cancelImageForm();
      window.removeEventListener('resize', reposition);
      window.removeEventListener('scroll', reposition, true);
      cancelAnimationFrame(railFrame);
      editor.destroy();
    };
  });
</script>


<div class="tiptap-prototype" class:editorial={styleMode === 'editorial'}>
  <div class="editor-hint">
    <span>Click into the node and write. Press Enter to start another paragraph.</span>
  </div>

  <!-- Persistent toolbar: never moves, expands, or disappears as the selection changes. -->
  <div class="format-toolbar" role="toolbar" aria-label="Text formatting">
    <button type="button" aria-label="Bold selected text" title="Bold"
      onpointerdown={(event) => event.preventDefault()} onclick={() => format('bold')}>
      <strong>B</strong>
    </button>
    <button type="button" aria-label="Italicize selected text" title="Italic"
      onpointerdown={(event) => event.preventDefault()} onclick={() => format('italic')}>
      <em>I</em>
    </button>
    <button type="button" aria-label="Add or edit link"
      onpointerdown={(event) => event.preventDefault()} onclick={openLinkForm}>Link</button>
    <span class="toolbar-separator" aria-hidden="true"></span>
    <button type="button" aria-label="Toggle heading" onpointerdown={(event) => event.preventDefault()}
      onclick={() => format('heading')}>H2</button>
    <button type="button" aria-label="Toggle bulleted list" onpointerdown={(event) => event.preventDefault()}
      onclick={() => format('bulletList')}>List</button>
  </div>


  <!-- Controls are overlays tied to the whole selected block. They never
       indent, resize, or change the text/image content layout. -->
  <div class="document-shell" bind:this={shellEl}>
    {#if deleteTarget}
      <div class="delete-confirm" role="alertdialog" aria-label="Confirm delete block"
        aria-describedby="story-delete-description"
        style:top={deleteConfirmPosition.top + 'px'}
        style:left={deleteConfirmPosition.left + 'px'}>
        <strong>Delete this {deleteTarget.kind} block?</strong>
        <p id="story-delete-description">
          This removes the entire block from this Storybook draft: {deleteTarget.summary}
        </p>
        <div class="form-actions">
          <button type="button" onclick={cancelDeleteBlock}>Cancel</button>
          <button type="button" class="destructive" onclick={confirmDeleteBlock}>Delete block</button>
        </div>
      </div>
    {/if}
    {#if linkFormOpen}
      <form class="link-form" aria-label="Insert or edit link"
        style:top={linkFormPosition.top + 'px'} style:left={linkFormPosition.left + 'px'}
        onsubmit={(event) => { event.preventDefault(); saveLink(); }}>
        <label for="story-link-url">Link URL</label>
        <input id="story-link-url" type="url" required bind:value={linkHref}
          placeholder="https://example.org/article" />
        {#if linkSelection?.from === linkSelection?.to}
          <label for="story-link-text">Link text (optional)</label>
          <input id="story-link-text" bind:value={linkText} placeholder="Text to insert" />
        {/if}
        {#if linkError}<p class="input-error" role="alert">{linkError}</p>{/if}
        <div class="form-actions">
          <button type="button" onclick={() => { linkFormOpen = false; linkSelection = null; }}>Cancel</button>
          <button type="button" onclick={unlink}>Remove link</button>
          <button type="submit" class="primary">Apply link</button>
        </div>
      </form>
    {/if}

    {#if selectionPopup && controlsVisible}
      <div class="selection-bubble" role="toolbar" aria-label="Selected text formatting"
        style:left={selectionPopup.left + 'px'} style:top={selectionPopup.top + 'px'}>
        <button type="button" aria-label="Bold highlighted text" title="Bold"
          onpointerdown={(event) => event.preventDefault()} onclick={() => format('bold')}><strong>B</strong></button>
        <button type="button" aria-label="Italicize highlighted text" title="Italic"
          onpointerdown={(event) => event.preventDefault()} onclick={() => format('italic')}><em>I</em></button>
        <button type="button" aria-label="Link highlighted text" title="Link"
          onpointerdown={(event) => event.preventDefault()} onclick={openLinkForm}>Link</button>
        <span class="toolbar-separator" aria-hidden="true"></span>
        <button type="button" aria-label="Heading for highlighted text" title="Heading 2"
          onpointerdown={(event) => event.preventDefault()} onclick={() => format('heading')}>H2</button>
        <button type="button" aria-label="List for highlighted text" title="Bulleted list"
          onpointerdown={(event) => event.preventDefault()} onclick={() => format('bulletList')}>List</button>
      </div>
    {/if}

    {#if controlsVisible}
      <button type="button" class="boundary-add before" aria-label="Insert block before"
        style:top={blockLayout.top + 'px'} style:left={blockLayout.center + 'px'}
        aria-expanded={floatingOptionsOpen && insertSide === 'before'}
        onpointerdown={(event) => event.preventDefault()}
        onclick={() => openInsertChoices('before')}>+</button>

      <div class="block-corner-controls" role="group" aria-label="Selected block actions"
        style:top={blockLayout.top + 'px'} style:right={Math.max(0, shellEl.clientWidth - blockLayout.right) + 'px'}>
        <button type="button" aria-label="Move current block up" title="Move block up"
          disabled={activeIndex === 0}
          onpointerdown={(event) => event.preventDefault()}
          onclick={() => moveCurrentBlock(-1)}>↑</button>
        <button type="button" aria-label="Move current block down" title="Move block down"
          disabled={activeIndex >= blockCount - 1}
          onpointerdown={(event) => event.preventDefault()}
          onclick={() => moveCurrentBlock(1)}>↓</button>
        <button type="button" class="corner-delete" aria-label="Delete current block" title="Delete block"
          onpointerdown={(event) => event.preventDefault()}
          onclick={requestDeleteBlock}>×</button>
      </div>

      <button type="button" class="boundary-add after" aria-label="Insert block after"
        style:top={blockLayout.bottom + 'px'} style:left={blockLayout.center + 'px'}
        aria-expanded={floatingOptionsOpen && insertSide === 'after'}
        onpointerdown={(event) => event.preventDefault()}
        onclick={() => openInsertChoices('after')}>+</button>

      {#if floatingOptionsOpen}
        <div class="insert-choices" role="group" aria-label="Insert a content block"
          style:top={insertMenuLayout.top + 'px'} style:left={insertMenuLayout.left + 'px'}>
          <span class="insert-label">Insert {insertSide} this block</span>
          <div class="insert-types">
            <button type="button" onpointerdown={(event) => event.preventDefault()}
              onclick={insertTextBlock}>Text block</button>
            <button type="button" onpointerdown={(event) => event.preventDefault()}
              onclick={() => openImageForm('new')}>Image</button>
            <button type="button" onpointerdown={(event) => event.preventDefault()}
              onclick={insertDivider}>Section break</button>
          </div>
          <button type="button" class="close-insert" aria-label="Close insert choices"
            onpointerdown={(event) => event.preventDefault()}
            onclick={() => floatingOptionsOpen = false}>×</button>
        </div>
      {/if}
    {/if}
    <div class="editor-canvas" bind:this={canvasEl}></div>
  {#if imageFormOpen}
    <form class="image-form" aria-label={editingExisting ? 'Edit image' : 'Insert image'}
      style:top={imageFormPosition.top + 'px'} style:left={imageFormPosition.left + 'px'}
      onsubmit={(event) => { event.preventDefault(); saveImage(); }}>
      <h3>{editingExisting ? 'Edit image' : 'Insert an image'}</h3>
      <label for="story-image-url">Image URL</label>
      <input id="story-image-url" type="url" required bind:value={imageDraft.url}
        placeholder="https://example.org/photo.jpg"/>
      <label for="story-image-alt">Alternative text <span>(required)</span></label>
      <textarea id="story-image-alt" rows="2" required bind:value={imageDraft.alt}
        placeholder="Describe the photograph for someone who cannot see it"></textarea>
      <label for="story-image-caption">Caption <span>(optional)</span></label>
      <input id="story-image-caption" bind:value={imageDraft.caption} placeholder="Optional caption"/>
      {#if imageError}<p role="alert" class="input-error">{imageError}</p>{/if}
      <div class="form-actions">
        <button type="button" onclick={cancelImageForm}>Cancel</button>
        <button type="submit" class="primary">Apply image</button>
      </div>
    </form>
  {/if}

  </div>




  <p class="editor-note">Select a text or image block to reveal its top-right move/delete controls and + buttons before and after it. Enter adds a paragraph inside the same text block. Editing stays inside this Storybook draft.</p>
</div>

<style>
  .tiptap-prototype {
    color: var(--atlas-text);
    container-type: inline-size;
    min-width: 0;
  }
  .editor-hint { color: var(--atlas-muted); font-size: .84rem; padding: .4rem 0 .7rem; }
  .format-toolbar {
    display: flex; align-items: center; gap: .3rem; flex-wrap: wrap;
    padding: .3rem .45rem; border: 1px solid var(--atlas-border);
    border-radius: .6rem; background: var(--atlas-surface);
    margin: 0 0 1rem; min-height: 2.65rem;
  }
  .format-toolbar button {
    color: var(--atlas-text); font: inherit; cursor: pointer; padding: .3rem .65rem;
    border: 0; border-radius: .4rem; background: transparent; min-height: 2rem;
  }
  .format-toolbar button:hover { background: var(--atlas-bg); }
  .toolbar-separator { height: 1.35rem; border-left: 1px solid var(--atlas-border); margin-inline: .2rem; }
  .document-shell {
    position: relative; min-width: 0; background: transparent; border: 0;
  }
  /* Overlaid at the *selected block's boundaries*, never a permanent gutter.
     The content retains the exact same width whether focused or not. */
  .boundary-add {
    position: absolute; z-index: 18; transform: translate(-50%, -50%);
    display: grid; place-items: center; box-sizing: border-box;
    width: 1.85rem; height: 1.85rem; padding: 0; border-radius: 50%;
    border: 1px solid var(--atlas-border);
    background: var(--atlas-surface); color: var(--atlas-text);
    font: 600 1.2rem/1 system-ui, sans-serif; cursor: pointer;
    box-shadow: 0 2px 8px #0001;
  }
  .boundary-add:hover { color: var(--atlas-accent); border-color: var(--atlas-accent); }
  .block-corner-controls {
    position: absolute; z-index: 20; transform: translateY(-50%);
    display: flex; align-items: center; gap: .12rem;
    padding: .2rem; border: 1px solid var(--atlas-border);
    border-radius: .55rem; background: var(--atlas-surface);
    color: var(--atlas-text); box-shadow: 0 2px 9px #0002;
  }
  .block-corner-controls button {
    display: grid; place-items: center; box-sizing: border-box;
    width: 1.75rem; height: 1.75rem; padding: 0;
    border: 0; border-radius: .35rem; background: transparent;
    color: inherit; cursor: pointer; font: 600 1.05rem/1 system-ui, sans-serif;
  }
  .block-corner-controls button:hover { background: var(--atlas-bg); }
  .block-corner-controls button:disabled { opacity: .3; cursor: default; }
  .block-corner-controls .corner-delete:hover { color: #b32318; }
  /* Insert menu opens directly from the clicked above/below button. */
  .insert-choices {
    position: absolute; z-index: 30;
    width: min(18rem, 95%); max-width: 95%; box-sizing: border-box;
    display: grid; gap: .55rem; padding: .75rem;
    border: 1px solid var(--atlas-border); border-radius: .6rem;
    background: var(--atlas-surface); color: var(--atlas-text);
    box-shadow: 0 8px 26px #0002; font-size: .86rem;
  }
  .insert-label { font-weight: 600; color: var(--atlas-muted); padding-right: 1.5rem; }
  .insert-types { display: flex; flex-wrap: wrap; gap: .3rem; }
  .insert-choices button, .block-context button, .form-actions button {
    cursor: pointer; border: 1px solid var(--atlas-border); border-radius: .4rem;
    background: var(--atlas-surface); color: var(--atlas-text);
    padding: .45rem .65rem; font: inherit; font-size: .85rem;
  }
  .insert-choices .close-insert {
    position: absolute; top: .22rem; right: .25rem;
    padding: .15rem .42rem; border: 0;
  }
  .selection-bubble {
    position: absolute; z-index: 25; display: flex; align-items: center;
    width: max-content; max-width: 100%; gap: .15rem; padding: .25rem;
    border: 1px solid var(--atlas-border); border-radius: .6rem;
    background: var(--atlas-surface); box-shadow: 0 7px 26px #0002;
  }
  .selection-bubble button {
    cursor: pointer; background: transparent; border: 0;
    color: var(--atlas-text); border-radius: .3rem;
    padding: .38rem .55rem; font: inherit; font-size: .85rem;
  }
  .selection-bubble button:hover { background: var(--atlas-bg); }
  .editor-canvas { padding: 1.3rem 0 1.5rem; min-height: 11rem; }
  .editorial .editor-canvas { font-family: Georgia, 'Times New Roman', serif; font-size: 1.14rem; line-height: 1.8; }
  :global(.atlas-story-rich-document) {
    min-height: 10rem; outline: none; overflow-wrap: anywhere; line-height: 1.7;
  }
  :global(.atlas-story-rich-document:focus-visible) { outline: none; }
  :global(.atlas-story-rich-document > [data-atlas-key]) {
    margin-block: 1.2rem;
  }
  :global(.atlas-story-rich-document > .atlas-story-active-block) {
    box-sizing: border-box; position: relative; isolation: isolate;
    /* Identical styling for Markdown and image blocks, with no indentation
       or change to text position. The image itself keeps its original size. */
    margin-left: 0;
  }
  :global(.atlas-story-rich-document > .atlas-story-active-block::before) {
    content: ''; position: absolute; pointer-events: none; z-index: -1;
    inset: -.28rem -.28rem;
    background: color-mix(in srgb, var(--atlas-accent) 5%, var(--atlas-surface));
    border: 1px solid color-mix(in srgb, var(--atlas-accent) 36%, var(--atlas-border));
    border-left-width: 3px;
    border-radius: .45rem;
  }
  :global(.atlas-story-text) { min-width: 0; }
  :global(.atlas-story-rich-document p) { margin: 0 0 .85rem; }
  :global(.atlas-story-rich-document h2) { font-size: 1.4em; line-height: 1.3; margin: 1.25rem 0 .5rem; }
  :global(.atlas-story-rich-document ul), :global(.atlas-story-rich-document ol) { padding-left: 1.5rem; }
  :global(.atlas-story-rich-document blockquote) { padding-left: 1rem; border-left: 3px solid var(--atlas-border); }
  :global(.atlas-story-rich-document a) { color: var(--atlas-accent); text-decoration: underline; }
  :global(.atlas-story-rich-document hr) { border: 0; border-top: 1px solid var(--atlas-border); margin: 1.5rem 0; }
  :global(.atlas-story-figure) {
    position: relative; margin: 1rem 0; padding: .15rem;
    box-sizing: border-box; width: 100%; max-width: 100%;
  }
  :global(.atlas-story-image-placeholder) {
    width: min(28rem, 100%); min-height: 27rem; box-sizing: border-box;
    display: flex; align-items: flex-start; justify-content: center;
    padding: 1rem; border: 1px dashed var(--atlas-border);
    border-radius: var(--atlas-radius); color: var(--atlas-muted);
    font-size: .85rem;
  }
  :global(.atlas-story-image-action) {
    position: absolute; bottom: .5rem; right: .5rem; z-index: 2;
    border: 1px solid var(--atlas-border); border-radius: .45rem;
    background: var(--atlas-surface); color: var(--atlas-text);
    padding: .35rem .6rem; cursor: pointer; font: 500 .8rem/1.3 system-ui, sans-serif;
    box-shadow: 0 2px 9px #0002;
  }
  :global(.atlas-story-image-action:hover) { border-color: var(--atlas-accent); }
  :global(.atlas-story-figure.ProseMirror-selectednode) {
    /* Avoid default ProseMirror outline competing with the Atlas highlight. */
    outline: none;
  }
  :global(.atlas-story-figure img) {
    display: block; max-width: 100%; max-height: 24rem; border-radius: var(--atlas-radius);
  }
  :global(.atlas-story-figure figcaption) {
    color: var(--atlas-muted); margin-top: .5rem; font: .85rem/1.5 system-ui, sans-serif;
  }
  :global(.atlas-story-unsupported) {
    border: 1px dashed var(--atlas-border); border-radius: .4rem;
    color: var(--atlas-muted); padding: .65rem; margin-block: .7rem; font-size: .85rem;
  }
  .block-context {
    margin: .6rem 0 0 3.15rem; display: flex; flex-wrap: wrap;
    align-items: center; gap: .45rem; color: var(--atlas-muted); font-size: .82rem;
  }
  .image-form, .link-form, .delete-confirm {
    display: grid; gap: .45rem; padding: .8rem; margin: .5rem 0 1rem;
    border: 1px solid var(--atlas-border); border-radius: .6rem;
    background: var(--atlas-surface);
  }
  .link-form, .image-form {
    position: absolute; z-index: 35; width: min(21rem, 100%);
    max-width: 100%; box-sizing: border-box; margin: 0;
    box-shadow: 0 9px 30px #0003;
  }
  .image-form { width: min(22rem, 100%); }
  .delete-confirm {
    position: absolute; z-index: 45;
    width: min(19rem, 100%); max-width: 100%;
    box-sizing: border-box; margin: 0;
    box-shadow: 0 9px 30px #0003;
    border-color: var(--atlas-focus);
  }
  :global(.atlas-story-rich-document > .atlas-story-pending-delete) {
    outline: 2px solid var(--atlas-focus);
    outline-offset: 2px;
    border-radius: .3rem;
  }
  .delete-confirm p { margin: .1rem 0; color: var(--atlas-muted); overflow-wrap: anywhere; }
  .form-actions .destructive { color: #b32318; border-color: #b32318; }
  .image-form h3 { margin: 0 0 .25rem; }
  .image-form label, .link-form label { font-size: .85rem; font-weight: 600; }
  .image-form label span { font-weight: 400; color: var(--atlas-muted); }
  .image-form input, .image-form textarea, .link-form input {
    box-sizing: border-box; width: 100%; padding: .6rem;
    border-radius: .4rem; border: 1px solid var(--atlas-border);
    background: var(--atlas-bg); color: var(--atlas-text); font: inherit;
  }
  .input-error { margin: 0; color: #b32318; font-size: .85rem; }
  .form-actions { display: flex; justify-content: flex-end; gap: .4rem; flex-wrap: wrap; }
  .form-actions .primary { background: var(--atlas-accent); color: var(--atlas-on-accent); }
  .editor-note { margin: .6rem 0 0; color: var(--atlas-muted); font-size: .75rem; line-height: 1.5; }
  button:focus-visible, input:focus-visible, textarea:focus-visible {
    outline: 2px solid var(--atlas-focus); outline-offset: 2px;
  }
  @container (max-width: 430px) {
    .boundary-add { width: 1.85rem; height: 1.85rem; }
    .block-corner-controls { gap: 0; padding: .1rem; }
    .block-corner-controls button { width: 1.75rem; height: 1.9rem; }
    .format-toolbar { gap: .1rem; }
    .format-toolbar button { min-height: 2.5rem; padding: .4rem .65rem; }
  }
</style>

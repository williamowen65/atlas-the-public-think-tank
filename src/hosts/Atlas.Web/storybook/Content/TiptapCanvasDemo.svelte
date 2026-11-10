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
            if (!editorFocused) return DecorationSet.empty;
            const index = state.selection.$from.index(0);
            if (index < 0 || index >= state.doc.childCount) return DecorationSet.empty;
            let from = 0;
            for (let i = 0; i < index; i++) from += state.doc.child(i).nodeSize;
            return DecorationSet.create(state.doc, [
              Decoration.node(from, from + state.doc.child(index).nodeSize, {
                class: 'atlas-story-active-block',
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
      return ['figure', {
        'data-atlas-image': 'true',
        'data-atlas-key': node.attrs.key,
        'data-src': node.attrs.src,
        'data-alt': node.attrs.alt,
        'data-caption': node.attrs.caption,
        class: 'atlas-story-figure',
      }, ['img', { src: publicUrl(String(node.attrs.src ?? '')) || '', alt: node.attrs.alt }],
      ['figcaption', {}, node.attrs.caption || '']];
    },
  });

  let canvasEl: HTMLDivElement;
  let shellEl: HTMLDivElement;
  let gutterTop = $state(8);
  let activeIndex = $state(0);
  let blockCount = $state(initialBlocks.length);
  let deleteTarget = $state<{ key: string; kind: string; summary: string } | null>(null);
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
  let insertionPosition = $state<number | null>(null);
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

  function openInsertChoices() {
    const editor = currentEditor();
    if (!editor) return;
    if (floatingOptionsOpen) {
      floatingOptionsOpen = false;
      return;
    }
    insertAnchorKey = String(editor.state.doc.child(activeIndex)?.attrs.key || '');
    insertSide = 'after';
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

  // Keep the editor at full width. Only the currently selected top-level
  // Atlas block receives a left margin to clear the compact control rail.
  // Position the rail at that block's top rather than tracking individual
  // paragraph lines (Enter must not change the indentation of other blocks).
  function repositionGutter(editor: Editor) {
    if (!shellEl || !editor.view?.dom?.isConnected) return;
    const index = Math.min(editor.state.selection.$from.index(0), editor.state.doc.childCount - 1);
    activeIndex = Math.max(0, index);
    blockCount = editor.state.doc.childCount;

    // ProseMirror may replace a text-block DOM node after onTransaction. Measure
    // on the next animation frame, after its decorations and layout have updated.
    cancelAnimationFrame(railFrame);
    if (!editorFocused) {
      selectionPopup = null;
      return;
    }
    railFrame = requestAnimationFrame(() => {
      if (!shellEl || editor.isDestroyed || !editor.view.dom.isConnected || !editorFocused) return;
      const current = editor.state.selection.$from.index(0);
      const block = editor.view.dom.children.item(current);
      if (block) {
        const top = block.getBoundingClientRect().top - shellEl.getBoundingClientRect().top;
        if (Number.isFinite(top)) gutterTop = Math.max(0, Math.round(top));
      }
      const selection = editor.state.selection;
      if (selection instanceof TextSelection && !selection.empty && !linkFormOpen) {
        try {
          const start = editor.view.coordsAtPos(selection.from);
          const end = editor.view.coordsAtPos(selection.to);
          const shell = shellEl.getBoundingClientRect();
          selectionPopup = {
            left: Math.max(0, Math.min(shellEl.clientWidth - 285, (start.left + end.right) / 2 - shell.left - 137)),
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

  function openImageForm() {
    const editor = currentEditor();
    if (!editor) return;
    editingExisting = editor.isActive('atlasImage');
    const attrs = editingExisting ? editor.getAttributes('atlasImage') : {};
    imageDraft = {
      url: String(attrs.src ?? ''),
      alt: String(attrs.alt ?? ''),
      caption: String(attrs.caption ?? ''),
    };
    insertionPosition = editingExisting ? null : insertionPoint(editor);
    imageError = '';
    imageFormOpen = true;
    floatingOptionsOpen = false;
  }

  function saveImage() {
    const editor = currentEditor();
    if (!editor) return;
    if (!publicUrl(imageDraft.url)) {
      imageError = 'Enter a complete HTTP(S) image URL.';
      return;
    }
    if (!imageDraft.alt.trim()) {
      imageError = 'Alternative text is required.';
      return;
    }
    const attrs = {
      src: imageDraft.url.trim(),
      alt: imageDraft.alt.trim(),
      caption: imageDraft.caption.trim(),
    };
    if (editingExisting) {
      editor.chain().focus().updateAttributes('atlasImage', attrs).run();
    } else {
      const position = insertionPosition ?? insertionPoint(editor);
      editor.chain().focus().insertContentAt(position, {
        type: 'atlasImage', attrs: { ...attrs, key: crypto.randomUUID() },
      }).setNodeSelection(position).run();
    }
    imageFormOpen = false;
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
    const reposition = () => requestAnimationFrame(positionAnchoredForms);
    window.addEventListener('resize', reposition);
    window.addEventListener('scroll', reposition, true);
    return () => {
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

  {#if linkFormOpen}
    <form class="link-form" aria-label="Insert or edit link"
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


  <!-- The editor remains full width. Only the currently active top-level Atlas
       block is indented, leaving room for plus and reordering controls. -->
  <div class="document-shell" bind:this={shellEl}>
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
      <div class="gutter-position" style:top={gutterTop + 'px'} role="group" aria-label="Current block controls">
      <button type="button" class="insert-trigger" aria-label="Add a content block"
        aria-expanded={floatingOptionsOpen} onpointerdown={(event) => event.preventDefault()}
        onclick={openInsertChoices}>+</button>
      {#if floatingOptionsOpen}
        <div class="insert-choices" role="group" aria-label="Insert a content block">
          <div class="insert-position">
            <span>Insert</span>
            <button type="button" aria-pressed={insertSide === 'before'}
              class:selected={insertSide === 'before'}
              onpointerdown={(event) => event.preventDefault()} onclick={() => insertSide = 'before'}>Before</button>
            <button type="button" aria-pressed={insertSide === 'after'}
              class:selected={insertSide === 'after'}
              onpointerdown={(event) => event.preventDefault()} onclick={() => insertSide = 'after'}>After</button>
          </div>
          <div class="insert-types">
            <button type="button" onpointerdown={(event) => event.preventDefault()}
              onclick={insertTextBlock}>Text block</button>
            <button type="button" onpointerdown={(event) => event.preventDefault()}
              onclick={openImageForm}>Image</button>
            <button type="button" onpointerdown={(event) => event.preventDefault()}
              onclick={insertDivider}>Section break</button>
          </div>
          <button type="button" class="close-insert" aria-label="Close insert choices"
            onpointerdown={(event) => event.preventDefault()}
            onclick={() => floatingOptionsOpen = false}>×</button>
        </div>
      {/if}
      <div class="move-controls">
        <button type="button" aria-label="Move current block up" title="Move section up"
          disabled={activeIndex === 0} onpointerdown={(event) => event.preventDefault()}
          onclick={() => moveCurrentBlock(-1)}>↑</button>
        <button type="button" aria-label="Move current block down" title="Move section down"
          disabled={activeIndex >= blockCount - 1} onpointerdown={(event) => event.preventDefault()}
          onclick={() => moveCurrentBlock(1)}>↓</button>
      </div>
      <button type="button" class="delete-trigger" aria-label="Delete current block"
        title="Delete section" onpointerdown={(event) => event.preventDefault()}
        onclick={requestDeleteBlock}>×</button>
      </div>
    {/if}
    <div class="editor-canvas" bind:this={canvasEl}></div>
  </div>

  {#if editorState.editor?.isActive('atlasImage')}
    <div class="block-context">
      <span>Image selected</span>
      <button type="button" onclick={openImageForm}>Edit image, caption &amp; alt text</button>
    </div>
  {/if}

  {#if deleteTarget}
    <div class="delete-confirm" role="alertdialog" aria-label="Confirm delete block"
      aria-describedby="story-delete-description">
      <strong>Delete this {deleteTarget.kind} block?</strong>
      <p id="story-delete-description">This removes the entire block from this Storybook draft: {deleteTarget.summary}</p>
      <div class="form-actions">
        <button type="button" onclick={() => deleteTarget = null}>Cancel</button>
        <button type="button" class="destructive" onclick={confirmDeleteBlock}>Delete block</button>
      </div>
    </div>
  {/if}

  {#if imageFormOpen}
    <form class="image-form" aria-label={editingExisting ? 'Edit image' : 'Insert image'}
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
        <button type="button" onclick={() => { imageFormOpen = false; imageError = ''; }}>Cancel</button>
        <button type="submit" class="primary">Apply image</button>
      </div>
    </form>
  {/if}

  <p class="editor-note">Only the selected section makes space for the +, ↑ ↓, and delete controls. Enter creates a paragraph inside that section; + → Text creates a separate Atlas block. The final empty text block remains as a place to type.</p>
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
  .gutter-position {
    position: absolute; left: calc(.2rem - var(--editor-card-inset, 1.25rem));
    width: 2.8rem; z-index: 2;
    display: flex; flex-direction: column; align-items: center; gap: .2rem;
  }
  .move-controls { display: flex; align-items: center; gap: .12rem; }
  .move-controls button {
    width: 1.25rem; height: 1.55rem; padding: 0;
    display: grid; place-items: center;
    color: var(--atlas-text); background: var(--atlas-surface);
    border: 1px solid var(--atlas-border); border-radius: .35rem;
    cursor: pointer; font: 600 .95rem/1 system-ui, sans-serif;
  }
  .move-controls button:disabled { opacity: .3; cursor: default; }
  .delete-trigger {
    border: 1px solid var(--atlas-border); border-radius: .35rem;
    background: var(--atlas-surface); color: var(--atlas-text);
    width: 1.7rem; height: 1.6rem; display: grid; place-items: center;
    padding: 0; font: 600 1.1rem/1 system-ui, sans-serif; cursor: pointer;
  }
  .delete-trigger:hover { color: #b32318; border-color: #b32318; }
  .insert-trigger {
    width: 2.15rem; height: 2.15rem; border: 1px solid var(--atlas-border);
    border-radius: 50%; background: var(--atlas-surface); color: var(--atlas-text);
    display: grid; place-items: center; box-sizing: border-box;
    padding: 0; line-height: 1; font: 600 1.4rem/1 system-ui, sans-serif;
    cursor: pointer;
  }
  .insert-trigger:hover { color: var(--atlas-accent); border-color: var(--atlas-accent); }
  /* The insert popover grows directly from the + control; unlike the rail,
     this user-opened panel is allowed to cover the document temporarily. */
  .insert-choices {
    position: absolute; left: calc(100% + .25rem); top: 0;
    z-index: 15; width: max-content; max-width: min(21rem, 85vw);
    display: grid; gap: .5rem; align-items: start;
    padding: .65rem; border: 1px solid var(--atlas-border);
    border-radius: .65rem; background: var(--atlas-surface);
    color: var(--atlas-text); box-shadow: 0 7px 28px #0002;
    font-size: .86rem; text-align: left;
  }
  .insert-position, .insert-types { display: flex; align-items: center; flex-wrap: wrap; gap: .3rem; }
  .insert-position span { color: var(--atlas-muted); margin-right: .25rem; }
  .insert-choices button, .block-context button, .form-actions button {
    cursor: pointer; border: 1px solid var(--atlas-border); border-radius: .4rem;
    background: var(--atlas-surface); color: var(--atlas-text);
    padding: .45rem .65rem; font: inherit; font-size: .85rem;
  }
  .insert-choices button.selected { border-color: var(--atlas-accent); font-weight: 700; }
  .insert-choices .close-insert { position: absolute; top: .15rem; right: .25rem; padding: .1rem .4rem; border: 0; }
  .insert-choices .insert-position { padding-right: 1.6rem; }
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
  .editor-canvas { padding: .3rem 0 1rem; min-height: 11rem; }
  .editorial .editor-canvas { font-family: Georgia, 'Times New Roman', serif; font-size: 1.14rem; line-height: 1.8; }
  :global(.atlas-story-rich-document) {
    min-height: 10rem; outline: none; overflow-wrap: anywhere; line-height: 1.7;
  }
  :global(.atlas-story-rich-document:focus-visible) { outline: none; }
  :global(.atlas-story-rich-document > *:first-child) { margin-top: 0; }
  :global(.atlas-story-rich-document > .atlas-story-active-block) {
    box-sizing: border-box;
    margin-left: calc(3.45rem - var(--editor-card-inset, 1.25rem));
  }
  :global(.atlas-story-text) { min-width: 0; }
  :global(.atlas-story-rich-document p) { margin: 0 0 .85rem; }
  :global(.atlas-story-rich-document h2) { font-size: 1.4em; line-height: 1.3; margin: 1.25rem 0 .5rem; }
  :global(.atlas-story-rich-document ul), :global(.atlas-story-rich-document ol) { padding-left: 1.5rem; }
  :global(.atlas-story-rich-document blockquote) { padding-left: 1rem; border-left: 3px solid var(--atlas-border); }
  :global(.atlas-story-rich-document a) { color: var(--atlas-accent); text-decoration: underline; }
  :global(.atlas-story-rich-document hr) { border: 0; border-top: 1px solid var(--atlas-border); margin: 1.5rem 0; }
  :global(.atlas-story-figure) { margin: 1rem 0; padding: .15rem; width: fit-content; max-width: 100%; }
  :global(.atlas-story-figure.ProseMirror-selectednode) {
    outline: 2px solid var(--atlas-focus); outline-offset: 2px;
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
  .delete-confirm { margin: 1rem 0; }
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
    .gutter-position { left: calc(.2rem - var(--editor-card-inset, .7rem)); }
    .insert-trigger { width: 2rem; height: 2rem; }
    .move-controls button { width: 1.2rem; }
    .block-context { margin-left: 2.7rem; }
    .format-toolbar { gap: .1rem; }
    .format-toolbar button { min-height: 2.5rem; padding: .4rem .65rem; }
  }
</style>

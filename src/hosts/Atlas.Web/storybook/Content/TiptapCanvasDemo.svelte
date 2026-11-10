<!--
  Storybook-only research prototype. This uses the real Tiptap editing engine,
  but is not wired to creation, API writes, or persisted Atlas block identity.
-->
<script lang="ts">
  import { onMount } from 'svelte';
  import { Editor, Node } from '@tiptap/core';
  import StarterKit from '@tiptap/starter-kit';
  import BubbleMenu from '@tiptap/extension-bubble-menu';
  import FloatingMenu from '@tiptap/extension-floating-menu';
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
      }, ['img', { src: node.attrs.src, alt: node.attrs.alt }],
      ['figcaption', {}, node.attrs.caption || '']];
    },
  });

  let canvasEl: HTMLDivElement;
  let bubbleEl: HTMLDivElement;
  let floatingEl: HTMLDivElement;
  let editorState = $state<{ editor: Editor | null }>({ editor: null });
  let floatingOptionsOpen = $state(false);
  let mobileOptionsOpen = $state(false);
  let imageFormOpen = $state(false);
  let imageError = $state('');
  let editingExisting = $state(false);
  let insertionPosition = $state<number | null>(null);
  let imageDraft = $state({ url: '', alt: '', caption: '' });

  function currentEditor(): Editor | null { return editorState.editor; }

  function format(kind: 'bold' | 'italic' | 'heading' | 'bulletList') {
    const editor = currentEditor();
    if (!editor) return;
    if (kind === 'bold') editor.chain().focus().toggleBold().run();
    else if (kind === 'italic') editor.chain().focus().toggleItalic().run();
    else if (kind === 'heading') editor.chain().focus().toggleHeading({ level: 2 }).run();
    else editor.chain().focus().toggleBulletList().run();
  }

  function addLink() {
    const editor = currentEditor();
    if (!editor) return;
    const existing = editor.getAttributes('link').href || '';
    const url = window.prompt('Link URL (http:// or https://)', String(existing));
    if (url === null) return;
    if (!url.trim()) {
      editor.chain().focus().unsetLink().run();
    } else if (/^https?:\/\//i.test(url.trim())) {
      editor.chain().focus().setLink({ href: url.trim() }).run();
    } else {
      window.alert('Use a full http:// or https:// link.');
    }
  }

  function insertDivider() {
    currentEditor()?.chain().focus().setHorizontalRule().run();
    floatingOptionsOpen = false;
    mobileOptionsOpen = false;
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
    insertionPosition = editor.state.selection.from;
    imageError = '';
    imageFormOpen = true;
    floatingOptionsOpen = false;
    mobileOptionsOpen = false;
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
      editor.chain().focus().insertContentAt(insertionPosition ?? editor.state.selection.from, [
        { type: 'atlasImage', attrs: { ...attrs, key: crypto.randomUUID() } },
        { type: 'paragraph' },
      ]).run();
    }
    imageFormOpen = false;
    imageError = '';
  }

  onMount(() => {
    const editor = new Editor({
      element: canvasEl,
      extensions: [
        StarterKit,
        AtlasImage,
        BubbleMenu.configure({ element: bubbleEl }),
        FloatingMenu.configure({ element: floatingEl }),
      ],
      content: blocksToTiptapHtml(initialBlocks),
      editorProps: {
        attributes: {
          class: 'atlas-story-rich-document',
          'aria-label': 'Node content: editable rich-text document',
        },
      },
      onTransaction: ({ editor }) => { editorState = { editor }; },
      onUpdate: ({ editor }) => onChange?.(tiptapDocumentToBlocks(editor.getJSON())),
    });
    editorState = { editor };
    return () => { editor.destroy(); };
  });
</script>

<div class="tiptap-prototype" class:editorial={styleMode === 'editorial'}>
  <div class="editor-hint">
    <span>Click into the document and type.</span>
    <span>Select text for formatting · Empty line for <strong>+</strong></span>
  </div>

  <!-- Both menu elements must exist before the editor is created on mount.
       Tiptap positions them; Atlas owns their contents and styles. -->
  <div bind:this={bubbleEl} class="atlas-story-bubble" aria-label="Selected text formatting">
    <button type="button" onclick={() => format('bold')} aria-label="Bold selected text" title="Bold"><strong>B</strong></button>
    <button type="button" onclick={() => format('italic')} aria-label="Italicize selected text" title="Italic"><em>I</em></button>
    <button type="button" onclick={addLink} aria-label="Add or edit link">Link</button>
  </div>
  <div bind:this={floatingEl} class="atlas-story-floating" aria-label="Insert content on empty line">
    <button type="button" class="insert-trigger" aria-label="Add a content block" aria-expanded={floatingOptionsOpen} onclick={() => floatingOptionsOpen = !floatingOptionsOpen}>+</button>
    {#if floatingOptionsOpen}
      <div class="insert-choices">
        <button type="button" onclick={openImageForm}>Image</button>
        <button type="button" onclick={() => { format('heading'); floatingOptionsOpen = false; }}>Heading</button>
        <button type="button" onclick={insertDivider}>Section break</button>
      </div>
    {/if}
  </div>

  <div class="document-shell">
    <div class="editor-canvas" bind:this={canvasEl}></div>
  </div>

  {#if editorState.editor?.isActive('atlasImage')}
    <div class="image-context">
      <span>Image block selected</span>
      <button type="button" onclick={openImageForm}>Edit image, caption &amp; alt text</button>
    </div>
  {/if}

  {#if imageFormOpen}
    <form class="image-form" aria-label={editingExisting ? 'Edit image' : 'Insert image'} onsubmit={(event) => { event.preventDefault(); saveImage(); }}>
      <h3>{editingExisting ? 'Edit image' : 'Insert an image'}</h3>
      <label for="story-image-url">Image URL</label>
      <input id="story-image-url" type="url" required bind:value={imageDraft.url} placeholder="https://example.com/photo.jpg"/>
      <label for="story-image-alt">Alternative text <span>(required)</span></label>
      <textarea id="story-image-alt" rows="2" required bind:value={imageDraft.alt} placeholder="Describe the photograph for someone who can't see it"></textarea>
      <label for="story-image-caption">Caption <span>(optional)</span></label>
      <input id="story-image-caption" bind:value={imageDraft.caption} placeholder="An optional explanation"/>
      {#if imageError}<p role="alert" class="input-error">{imageError}</p>{/if}
      <div class="form-actions">
        <button type="button" onclick={() => { imageFormOpen = false; imageError = ''; }}>Cancel</button>
        <button type="submit" class="primary">Apply image</button>
      </div>
    </form>
  {/if}

  <!-- Large touch targets instead of relying solely on hovering beside a line. -->
  <div class="touch-toolbar" aria-label="Mobile editing controls">
    <button type="button" onclick={() => format('bold')} aria-label="Bold selected text"><strong>B</strong></button>
    <button type="button" onclick={() => format('italic')} aria-label="Italic selected text"><em>I</em></button>
    <button type="button" onclick={addLink}>Link</button>
    <button type="button" onclick={() => format('heading')}>H2</button>
    <button type="button" onclick={() => mobileOptionsOpen = !mobileOptionsOpen} aria-expanded={mobileOptionsOpen}>+ Insert</button>
  </div>
  {#if mobileOptionsOpen}
    <div class="touch-insert-options" aria-label="Mobile insert options">
      <button type="button" onclick={openImageForm}>Image</button>
      <button type="button" onclick={insertDivider}>Section break</button>
      <button type="button" onclick={() => { format('bulletList'); mobileOptionsOpen = false; }}>Bulleted list</button>
    </div>
  {/if}
</div>

<style>
  .tiptap-prototype { color: var(--atlas-text); container-type: inline-size; }
  .editor-hint { display: flex; justify-content: space-between; gap: .4rem 1rem; flex-wrap: wrap; padding: .55rem 0 .8rem; font-size: .82rem; color: var(--atlas-muted); }
  .document-shell { border: 1px solid var(--atlas-border); border-radius: var(--atlas-radius); background: var(--atlas-surface); overflow: visible; }
  .editor-canvas { min-height: 18rem; padding: 1.2rem 1.5rem 2.5rem; }
  .editorial .document-shell { border-color: transparent; border-bottom-color: var(--atlas-border); border-radius: 0; }
  .editorial .editor-canvas { font-family: Georgia, 'Times New Roman', serif; font-size: 1.14rem; line-height: 1.8; }
  .editorial .editor-hint { font-family: system-ui, sans-serif; }
  :global(.atlas-story-rich-document) { min-height: 12rem; outline: none; overflow-wrap: anywhere; line-height: 1.7; }
  :global(.atlas-story-rich-document:focus-visible) { outline: 2px solid var(--atlas-focus); outline-offset: .5rem; border-radius: 2px; }
  :global(.atlas-story-rich-document > *:first-child) { margin-top: 0; }
  :global(.atlas-story-rich-document p) { margin: 0 0 .9rem; }
  :global(.atlas-story-rich-document h2) { font-size: 1.4em; margin: 1.35rem 0 .5rem; line-height: 1.3; }
  :global(.atlas-story-rich-document ul), :global(.atlas-story-rich-document ol) { padding-left: 1.6rem; }
  :global(.atlas-story-rich-document blockquote) { padding-left: 1rem; border-left: 3px solid var(--atlas-border); }
  :global(.atlas-story-rich-document a) { color: var(--atlas-accent); }
  :global(.atlas-story-rich-document hr) { border: 0; border-top: 1px solid var(--atlas-border); margin: 1.5rem 0; }
  :global(.atlas-story-figure) { margin: 1rem 0; padding: .15rem; max-width: 100%; }
  :global(.atlas-story-figure.ProseMirror-selectednode) { outline: 2px solid var(--atlas-focus); }
  :global(.atlas-story-figure img) { display: block; max-width: 100%; max-height: 24rem; border-radius: var(--atlas-radius); }
  :global(.atlas-story-figure figcaption) { color: var(--atlas-muted); margin-top: .5rem; font: .87rem/1.5 system-ui, sans-serif; }
  :global(.atlas-story-bubble), :global(.atlas-story-floating) { display: flex; gap: .25rem; align-items: center; padding: .3rem; border: 1px solid var(--atlas-border); border-radius: .65rem; box-shadow: 0 8px 30px #0002; background: var(--atlas-surface); color: var(--atlas-text); z-index: 2000; }
  :global(.atlas-story-floating) { border-radius: 999px; }
  :global(.atlas-story-bubble button), :global(.atlas-story-floating button) { cursor: pointer; border: 0; background: none; border-radius: .4rem; color: inherit; padding: .4rem .6rem; font: inherit; }
  :global(.atlas-story-bubble button:hover), :global(.atlas-story-floating button:hover) { background: var(--atlas-bg); }
  :global(.atlas-story-floating .insert-trigger) { width: 2rem; height: 2rem; padding: 0; font-size: 1.4rem; }
  :global(.atlas-story-floating .insert-choices) { display: flex; gap: .15rem; }
  .image-context { display: flex; align-items: center; flex-wrap: wrap; gap: .6rem; margin-top: .5rem; font-size: .85rem; color: var(--atlas-muted); }
  .image-context button, .form-actions button, .touch-toolbar button, .touch-insert-options button { cursor: pointer; border: 1px solid var(--atlas-border); border-radius: .5rem; background: var(--atlas-surface); color: var(--atlas-text); padding: .5rem .7rem; font: inherit; }
  .image-form { border: 1px solid var(--atlas-border); border-radius: .65rem; background: var(--atlas-surface); padding: 1rem; margin-top: 1rem; display: grid; gap: .45rem; }
  .image-form h3 { margin: 0 0 .3rem; }
  .image-form label { font-weight: 600; font-size: .85rem; }
  .image-form label span { font-weight: normal; color: var(--atlas-muted); }
  .image-form input, .image-form textarea { box-sizing: border-box; width: 100%; max-width: 100%; border: 1px solid var(--atlas-border); border-radius: .5rem; background: var(--atlas-bg); color: var(--atlas-text); padding: .65rem; font: inherit; }
  .image-form .input-error { color: #b32318; font-weight: 600; margin: 0; }
  .form-actions { display: flex; gap: .5rem; justify-content: flex-end; margin-top: .5rem; }
  .form-actions .primary { background: var(--atlas-accent); color: var(--atlas-on-accent); }
  .touch-toolbar, .touch-insert-options { display: none; }
  button:focus-visible, input:focus-visible, textarea:focus-visible { outline: 2px solid var(--atlas-focus); outline-offset: 2px; }
  @container (max-width: 650px) {
    .editor-canvas { padding: 1rem .75rem 1.7rem; }
    .editor-hint { font-size: .75rem; }
    .touch-toolbar { display: flex; flex-wrap: wrap; gap: .35rem; padding: .5rem 0; }
    .touch-toolbar button { min-height: 44px; min-width: 44px; }
    .touch-insert-options { display: flex; gap: .4rem; flex-wrap: wrap; padding: .4rem 0; }
  }
</style>

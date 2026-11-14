<script lang="ts">
  import type { Snippet } from 'svelte';
  import EditorImageInset from './EditorImageInset.svelte';
  import TiptapCanvas from '../components/TiptapCanvas.svelte';
  import type { AuthorBlock } from '../api/contentAuthoring';

  let { initialBlocks, onChange, onActivate, inset, anchor = 0 }: {
    initialBlocks: AuthorBlock[];
    inset?: Snippet;
    anchor?: number;
    onChange: (text: string, anchor?: number) => void;
    onActivate: (editor: ReturnType<typeof TiptapCanvas> | undefined) => void;
  } = $props();

  let editMarkdown = $state(false);
  let editor = $state<ReturnType<typeof TiptapCanvas>>();
</script>

{#if editMarkdown}
  <label>Section Markdown
    <textarea value={initialBlocks[0].text} oninput={(event) => onChange(event.currentTarget.value)}></textarea>
  </label>
{:else}
  <TiptapCanvas bind:this={editor} singleText {initialBlocks} inset={inset ? { component: EditorImageInset, content: inset, anchor } : undefined}
    onChange={(blocks, anchor) => onChange(blocks[0].text, anchor)} onActivate={() => onActivate(editor)}
    onUnsupportedMarkdown={() => { onActivate(undefined); editMarkdown = true; }} />
{/if}

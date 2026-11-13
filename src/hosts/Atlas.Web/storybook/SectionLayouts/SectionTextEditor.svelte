<script lang="ts">
  import type { Snippet } from 'svelte';
  import EditorImageInset from './EditorImageInset.svelte';
  import TiptapCanvas from '../../src/lib/components/TiptapCanvas.svelte';
  import type { AuthorBlock } from '../../src/lib/api/contentAuthoring';

  let { initialBlocks, onChange, onActivate, inset, anchor = 0 }: {
    initialBlocks: AuthorBlock[];
    inset?: Snippet;
    anchor?: number;
    onChange: (text: string, anchor?: number) => void;
    onActivate: (editor: ReturnType<typeof TiptapCanvas> | undefined) => void;
  } = $props();

  let editor = $state<ReturnType<typeof TiptapCanvas>>();
</script>

<TiptapCanvas bind:this={editor} singleText {initialBlocks} inset={inset ? { component: EditorImageInset, content: inset, anchor } : undefined}
  onChange={(blocks, anchor) => onChange(blocks[0].text, anchor)} onActivate={() => onActivate(editor)} />

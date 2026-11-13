<script lang="ts">
  import TiptapCanvas from '../../src/lib/components/TiptapCanvas.svelte';
  import type { AuthorBlock } from '../../src/lib/api/contentAuthoring';

  let { initialBlocks, onChange, onActivate }: {
    initialBlocks: AuthorBlock[];
    onChange: (text: string) => void;
    onActivate: (editor: ReturnType<typeof TiptapCanvas> | undefined) => void;
  } = $props();

  let editor = $state<ReturnType<typeof TiptapCanvas>>();
</script>

<TiptapCanvas bind:this={editor} singleText {initialBlocks}
  onChange={(blocks) => onChange(blocks[0].text)} onActivate={() => onActivate(editor)} />

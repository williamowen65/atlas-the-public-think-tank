<!--
  MarkdownContent: Shared Markdown rendering for published blocks and editor previews.
  The renderer disables raw HTML/images and validates link protocols before producing HTML.
-->
<script lang="ts">
  import { renderMarkdown } from '../markdown/renderMarkdown';

  let {
    text,
  }: {
    /** Markdown block source; short descriptions remain plain text elsewhere. */
    text: string;
  } = $props();
  const html = $derived(renderMarkdown(text));
</script>

<!-- Layout: One typography wrapper contains HTML produced only by the shared Markdown policy. -->
<div class="markdown">
  <!-- {@html} inserts HTML without sanitizing it. Only use renderMarkdown output here, never raw user input. -->
  {@html html}
</div>

<style>
  .markdown {
    overflow-wrap: anywhere;
    line-height: var(--atlas-line-height, 1.65);
  }
  /* Injected elements need descendant global selectors; scope them to this wrapper. */
  .markdown :global(pre) {
    overflow-x: auto;
    padding: 1rem;
    background: var(--atlas-bg);
  }
  .markdown :global(blockquote) {
    margin-inline: 0;
    padding-inline-start: 1rem;
    border-inline-start: 3px solid var(--atlas-border);
  }
  .markdown :global(a) {
    text-decoration: underline;
  }
</style>

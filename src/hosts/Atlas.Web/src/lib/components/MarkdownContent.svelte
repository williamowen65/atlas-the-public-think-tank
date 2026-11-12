<!--
  @component
  MarkdownContent: Shared Markdown rendering for published blocks and editor previews.
  The renderer disables raw HTML and validates link protocols before producing HTML.

  **Props**
  - `text` (`string`; required): Markdown block source; short descriptions remain plain text
    elsewhere.
-->
<script lang="ts">
  import { renderMarkdown } from '../markdown/renderMarkdown';

  let {
    text,
    images = false,
    preview = false,
  }: {
    /** Markdown block source; short descriptions remain plain text elsewhere. */
    text: string;
    images?: boolean;
    preview?: boolean;
  } = $props();
  /**
   * HTML derived only from the shared Markdown renderer; never raw user text for the HTML
   * insertion.
   */
  const html = $derived(renderMarkdown(text, images, preview));
</script>

<!-- Layout: One typography wrapper contains HTML produced only by the shared Markdown policy. -->
<div class="markdown">
  <!-- {@html} inserts HTML without sanitizing it. Only use renderMarkdown output here, never raw user input. -->
  {@html html}
</div>

<style>
  .markdown {
    display: flow-root;
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
  .markdown :global(img) {
    float: inline-start;
    width: min(42%, 12rem);
    aspect-ratio: 4 / 3;
    margin: 0.2rem 0.85rem 0.45rem 0;
    object-fit: contain;
    background: var(--atlas-bg);
    border-radius: var(--atlas-radius);
  }
  .markdown :global(a) {
    text-decoration: underline;
  }
</style>

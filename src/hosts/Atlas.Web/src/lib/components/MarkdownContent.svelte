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
    onImage,
  }: {
    /** Markdown block source; short descriptions remain plain text elsewhere. */
    text: string;
    images?: boolean;
    preview?: boolean;
    onImage?: (index: number, trigger: HTMLButtonElement) => void;
  } = $props();
  /**
   * HTML derived only from the shared Markdown renderer; never raw user text for the HTML
   * insertion.
   */
  const html = $derived(renderMarkdown(text, images, preview, !!onImage));
</script>

<!-- Layout: One typography wrapper contains HTML produced only by the shared Markdown policy. -->
<!-- svelte-ignore a11y_click_events_have_key_events, a11y_no_static_element_interactions (Events are delegated from native image buttons.) -->
<div class="markdown" onclick={(event) => {
  const button = event.target instanceof Element ? event.target.closest('button[data-node-image]') : null;
  if (button instanceof HTMLButtonElement) onImage?.(Number(button.dataset.nodeImage), button);
}}>
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
  .markdown :global(:is(.markdown-image, img)) {
    float: inline-start;
    width: calc(var(--atlas-image-width, 50%) - .5rem);
    margin: 0.2rem .5rem 0.45rem 0;
  }
  .markdown :global(.markdown-image) {
    display: block;
    padding: 0;
    border: 0;
    background: transparent;
    cursor: zoom-in;
  }
  .markdown :global(img) {
    display: block;
    height: auto;
    border-radius: var(--atlas-radius);
  }
  .markdown :global([data-image-placement='right']) { float: right; margin-inline: .5rem 0; }
  .markdown :global([data-image-placement='center']) {
    display: block; float: none; clear: both; margin-inline: auto;
  }
  .markdown :global(.markdown-image img) {
    float: none;
    width: 100%;
    margin: 0;
  }
  .markdown :global(a) {
    text-decoration: underline;
  }
</style>

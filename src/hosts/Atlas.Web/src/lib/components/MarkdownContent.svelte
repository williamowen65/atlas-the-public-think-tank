<!--
  MarkdownContent: Renders a small escaped Markdown subset: paragraphs, headings, lists and fenced code.
  Svelte escapes source text; this component does not inject arbitrary HTML or implement full Markdown.
-->
<script lang="ts">
  let {
    text,
  }: {
    /** Markdown source to render through the component’s safe Markdown subset. */
    text: string;
  } = $props();
  // Escape all source text through Svelte. Support a small, safe Markdown subset.
  const parts = $derived(text.split(/(```[\s\S]*?```)/g).filter(Boolean));
</script>

<!--
  Layout: A shared typography wrapper renders text sections as headings, paragraphs, lists or
  fenced code; these are elements, not injected raw HTML.
-->

<div class="markdown">
  {#each parts as part}
    {#if part.startsWith('```')}<pre><code
          >{part.replace(/^```[^\n]*\n?/, '').replace(/```$/, '')}</code
        ></pre>
    {:else}{#each part.split(/\n\s*\n/) as paragraph}
        {#if paragraph.startsWith('### ')}<h4>{paragraph.slice(4)}</h4>
        {:else if paragraph.startsWith('## ')}<h3>{paragraph.slice(3)}</h3>
        {:else if paragraph.startsWith('# ')}<h2>{paragraph.slice(2)}</h2>
        {:else if paragraph
          .trim()
          .split('\n')
          .every((line) => /^[-*] /.test(line))}<ul>
            {#each paragraph.trim().split('\n') as line}<li>{line.slice(2)}</li>{/each}
          </ul>
        {:else if paragraph.trim()}<p>{paragraph}</p>{/if}
      {/each}{/if}
  {/each}
</div>

<style>
  .markdown {
    overflow-wrap: anywhere;
    line-height: var(--atlas-line-height, 1.65);
  }
  p {
    white-space: pre-line;
  }
  pre {
    overflow-x: auto;
    padding: 1rem;
    background: var(--atlas-bg);
  }
</style>

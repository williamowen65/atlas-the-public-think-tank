<!--
  Explicit research-only Markdown preview: allow safe HTTP(S) inline images.
  The production MarkdownContent renderer intentionally disables images.
  Do NOT replace its security policy without a separate architecture review.
-->
<script lang="ts">
  import MarkdownIt from 'markdown-it';
  import { publicUrl } from '../../src/lib/api/nodePage';

  let { text }: { text: string } = $props();

  const markdown = new MarkdownIt({
    html: false,
    linkify: false,
    typographer: false,
  });

  const originalLinkValidator = markdown.validateLink.bind(markdown);
  markdown.validateLink = (value: string) => {
    if (!originalLinkValidator(value)) return false;
    try {
      return ['http:', 'https:', 'mailto:'].includes(new URL(value, 'https://atlas.invalid/').protocol);
    } catch { return false; }
  };

  // MarkdownIt escapes the image's src/alt when rendering. This additional
  // absolute-URL allowlist prevents unsafe protocols and local/data references.
  const imageRenderer = markdown.renderer.rules.image;
  markdown.renderer.rules.image = (tokens, index, options, env, renderer) => {
    const token = tokens[index];
    const safe = publicUrl(token.attrGet('src') || '');
    if (!safe) return '<span class="unsupported-image">[Image URL must be HTTP(S)]</span>';
    token.attrSet('src', safe);
    token.attrSet('loading', 'lazy');
    return imageRenderer?.(tokens, index, options, env, renderer)
      ?? renderer.renderToken(tokens, index, options);
  };

  const html = $derived(markdown.render(text));
</script>

<div class="story-markdown">
  {@html html}
</div>

<style>
  .story-markdown { min-width: 0; line-height: var(--atlas-line-height, 1.65); overflow-wrap: anywhere; }
  .story-markdown :global(img) {
    display: block; width: auto; height: auto;
    max-width: 100%; max-height: 17rem; object-fit: contain;
    margin: .45rem 0 .7rem; border-radius: .35rem;
  }
  .story-markdown :global(p) { margin: .35rem 0 .7rem; }
  .story-markdown :global(pre) { max-width: 100%; overflow-x: auto; }
  .story-markdown :global(a) { color: var(--atlas-accent); text-decoration: underline; }
  .story-markdown :global(blockquote) { margin-left: 0; padding-left: .9rem; border-left: 2px solid var(--atlas-border); }
  .story-markdown :global(.unsupported-image) { color: var(--atlas-muted); font-size: .82rem; }
</style>

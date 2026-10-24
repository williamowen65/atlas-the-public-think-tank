import MarkdownIt from 'markdown-it';

/**
 * Shared Atlas policy for published content and form previews. Raw HTML and images
 * stay disabled; enabling plugins or HTML later requires reviewing this trust boundary.
 * No plugin may inject untrusted HTML into the output consumed by {@html}.
 */
const markdown = new MarkdownIt({ html: false, linkify: false, typographer: false }).disable(
  'image',
);
const defaultValidateLink = markdown.validateLink.bind(markdown);

// Keep markdown-it’s checks, then restrict protocols explicitly. Relative links and
// fragments resolve against a dummy HTTPS origin solely for protocol validation.
markdown.validateLink = (url: string) => {
  if (!defaultValidateLink(url)) return false;
  try {
    return ['https:', 'http:', 'mailto:'].includes(new URL(url, 'https://atlas.invalid/').protocol);
  } catch {
    return false;
  }
};

// A node/page already owns its heading. Markdown starts at h2 and never exceeds h6.
for (const rule of ['heading_open', 'heading_close']) {
  markdown.renderer.rules[rule] = (tokens, index, options, _env, renderer) => {
    const token = tokens[index];
    token.tag = `h${Math.min(6, Number(token.tag.slice(1)) + 1)}`;
    return renderer.renderToken(tokens, index, options);
  };
}

/** Render user Markdown using the fixed policy above; never pass arbitrary HTML through. */
export function renderMarkdown(source: string): string {
  return markdown.render(source);
}

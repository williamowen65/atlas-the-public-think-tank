import MarkdownIt from 'markdown-it';

/**
 * Shared Atlas policy for published content and form previews. Raw HTML and images
 * stay disabled; enabling plugins or HTML later requires reviewing this trust boundary.
 * No plugin may inject untrusted HTML into the output consumed by {@html}.
 */
const markdown = new MarkdownIt({
  // Escape user-written HTML as text instead of allowing actual HTML elements.
  html: false,
  // Do not automatically turn bare URLs into links; explicit [label](URL) links still work.
  linkify: false,
  // Preserve punctuation rather than applying smart quotes and other typography substitutions.
  typographer: false,
})
  // Keep images in dedicated content blocks; Markdown must not produce image elements.
  .disable('image');

// Save the library’s original validator before replacing it below. bind(markdown)
// preserves its `this` context when called separately; it does not run validation yet.
const defaultValidateLink = markdown.validateLink.bind(markdown);

// Install a callback that markdown-it calls when checking link destinations.
// Require BOTH the library’s built-in checks and our explicit protocol allowlist.
markdown.validateLink = (url: string) => {
  // Preserve markdown-it’s existing rejection of dangerous link destinations.
  if (!defaultValidateLink(url)) return false;
  try {
    // This dummy HTTPS address is ONLY a parsing base for relative links/fragments.
    // It is not a deployment address, makes no network request, and never replaces
    // the rendered href. For example, /nodes?id=123 stays relative and will work on
    // thepublicthinktank.com. Only the temporary URL’s protocol is inspected here.
    return ['https:', 'http:', 'mailto:'].includes(new URL(url, 'https://atlas.invalid/').protocol);
  } catch {
    // A destination that cannot be parsed is not accepted as a clickable link.
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

import MarkdownIt from 'markdown-it';
import { imageLayoutStyle, readImageTitle } from './imageLayout';
import { publicUrl } from '../api/nodePage';

// Raw HTML stays disabled in both renderers. Node content can opt into
// external images using the same URL policy as standalone image blocks.
function createRenderer(images: boolean, shiftHeadings: boolean | 'preserve' = true, links = true, gallery = false) {
  const markdown = new MarkdownIt({ html: false, linkify: false, typographer: false });
  if (!images) markdown.disable('image');
  const validate = markdown.validateLink.bind(markdown);
  markdown.validateLink = (url) => {
    if (!validate(url)) return false;
    try {
      return ['https:', 'http:', 'mailto:'].includes(new URL(url, 'https://atlas.invalid/').protocol);
    } catch {
      return false;
    }
  };

  const renderImage = markdown.renderer.rules.image;
  if (images && renderImage) {
    markdown.renderer.rules.image = (tokens, index, options, env, renderer) => {
      const token = tokens[index];
      const src = publicUrl(String(token.attrGet('src') ?? ''));
      if (!src) return '<span>[Image URL must be HTTP(S)]</span>';
      const layout = readImageTitle(String(token.attrGet('title') ?? ''));
      token.attrs = token.attrs?.filter(([name]) => name !== 'title') ?? null;
      if (layout.title) token.attrSet('title', layout.title);
      const style = imageLayoutStyle(layout);
      if (!gallery) {
        token.attrSet('data-image-width', layout.width);
        token.attrSet('data-image-placement', layout.placement);
        token.attrSet('style', style);
      }
      token.attrSet('src', src);
      token.attrSet('loading', 'lazy');
      token.attrSet('referrerpolicy', 'no-referrer');
      const image = renderImage(tokens, index, options, env, renderer);
      if (!gallery) return image;
      const position = Number(env?.imageIndex ?? 0);
      if (env) env.imageIndex = position + 1;
      const label = markdown.utils.escapeHtml(String(token.attrGet('alt') || 'image'));
      return `<button type="button" class="markdown-image" data-image-placement="${layout.placement}" style="${style}" data-node-image="${position}" aria-label="View ${label}">${image}</button>`;
    };
  }

  if (!links) {
    markdown.renderer.rules.link_open = () => '<span>';
    markdown.renderer.rules.link_close = () => '</span>';
  }

  if (gallery && links) {
    // An image button cannot sit inside a link. In gallery mode, image
    // links open the gallery while ordinary Markdown links stay clickable.
    markdown.core.ruler.after('inline', 'gallery_image_links', (state) => {
      for (const token of state.tokens) {
        let open: NonNullable<typeof token.children>[number] | undefined;
        let containsImage = false;
        for (const child of token.children ?? []) {
          if (child.type === 'link_open') {
            open = child;
            containsImage = false;
          } else if (child.type === 'image' && publicUrl(String(child.attrGet('src') ?? ''))) {
            containsImage = true;
          } else if (child.type === 'link_close' && open && containsImage) {
            open.tag = child.tag = 'span';
            open.attrs = null;
          }
        }
      }
    });
  }

  if (shiftHeadings) {
    for (const rule of ['heading_open', 'heading_close']) {
      markdown.renderer.rules[rule] = (tokens, index, options, _env, renderer) => {
        const token = tokens[index];
        token.tag = `h${Math.min(6, Math.max(2, Number(token.tag.slice(1)) + (shiftHeadings === true ? 1 : 0)))}`;
        return renderer.renderToken(tokens, index, options);
      };
    }
  }
  return markdown;
}

const markdown = createRenderer(false);
const nodeMarkdown = createRenderer(true);
const feedMarkdown = createRenderer(true, true, false);
const galleryMarkdown = createRenderer(true, true, true, true);
const galleryFeedMarkdown = createRenderer(true, true, false, true);
const sectionMarkdown = createRenderer(false, 'preserve');
const editorMarkdown = createRenderer(true, false);
// Accept only literal underline tags. Markdown's general HTML rule stays disabled.
for (const renderer of [sectionMarkdown, editorMarkdown]) {
  renderer.inline.ruler.before('html_inline', 'atlas_underline', (state, silent) => {
    const tag = state.src.startsWith('<u>', state.pos) ? '<u>'
      : state.src.startsWith('</u>', state.pos) ? '</u>' : null;
    if (!tag || state.pos + tag.length > state.posMax) return false;
    if (!silent) state.push('atlas_underline', 'u', 0).content = tag;
    state.pos += tag.length;
    return true;
  });
  renderer.renderer.rules.atlas_underline = (tokens, index) => tokens[index].content;
}
// ProseMirror collapses soft breaks to spaces when it loads HTML. Render
// them the same way for the round-trip check while preserving hard breaks.
editorMarkdown.renderer.rules.softbreak = () => ' ';

/** Render user Markdown using the fixed policy above; never pass arbitrary HTML through. */
export function renderMarkdown(source: string, images = false, preview = false, gallery = false): string {
  const renderer = gallery ? (preview ? galleryFeedMarkdown : galleryMarkdown)
    : preview ? feedMarkdown : images ? nodeMarkdown : markdown;
  return renderer.render(source, { imageIndex: 0 });
}

export function renderSectionMarkdown(source: string): string {
  return sectionMarkdown.render(source);
}

/** Keep Markdown heading levels when loading a document into the editor. */
export function renderEditorMarkdown(source: string): string {
  return editorMarkdown.render(source);
}

/** Image order matches the gallery buttons emitted by the Markdown renderer. */
export function markdownImages(source: string) {
  return nodeMarkdown.parse(source, {}).flatMap((token) => (token.children ?? []).flatMap((child) => {
    if (child.type !== 'image') return [];
    const src = publicUrl(String(child.attrGet('src') ?? ''));
    if (!src) return [];
    const alt = nodeMarkdown.renderer.renderInlineAsText(child.children ?? [], nodeMarkdown.options, {});
    return [{ src, alt, caption: readImageTitle(String(child.attrGet('title') ?? '')).title }];
  }));
}

import MarkdownIt from 'markdown-it';
import { publicUrl } from '../api/nodePage';

// Raw HTML stays disabled in both renderers. Node content can opt into
// external images using the same URL policy as standalone image blocks.
function createRenderer(images: boolean, shiftHeadings = true, links = true) {
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
      token.attrSet('src', src);
      token.attrSet('loading', 'lazy');
      token.attrSet('referrerpolicy', 'no-referrer');
      return renderImage(tokens, index, options, env, renderer);
    };
  }

  if (!links) {
    markdown.renderer.rules.link_open = () => '<span>';
    markdown.renderer.rules.link_close = () => '</span>';
  }

  if (shiftHeadings) {
    for (const rule of ['heading_open', 'heading_close']) {
      markdown.renderer.rules[rule] = (tokens, index, options, _env, renderer) => {
        const token = tokens[index];
        token.tag = `h${Math.min(6, Number(token.tag.slice(1)) + 1)}`;
        return renderer.renderToken(tokens, index, options);
      };
    }
  }
  return markdown;
}

const markdown = createRenderer(false);
const nodeMarkdown = createRenderer(true);
const feedMarkdown = createRenderer(true, true, false);
const editorMarkdown = createRenderer(true, false);

/** Render user Markdown using the fixed policy above; never pass arbitrary HTML through. */
export function renderMarkdown(source: string, images = false, preview = false): string {
  return (preview ? feedMarkdown : images ? nodeMarkdown : markdown).render(source);
}

/** Keep Markdown heading levels when loading a document into the editor. */
export function renderEditorMarkdown(source: string): string {
  return editorMarkdown.render(source);
}

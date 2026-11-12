import { imageLayout, type ImageLayout } from '../markdown/imageLayout';
import { publicUrl, type ContentBlock } from './nodePage';

/** UI-only key distinguishes unsaved blocks; id is the stable server BlockId once saved. */
export interface AuthorBlock extends Partial<ImageLayout> {
  key: string;
  id?: string;
  kind: string;
  text: string;
  url: string;
  caption: string;
  /** Preserve the payload of unimplemented block kinds without interpreting it. */
  original?: ContentBlock;
}

export function newAuthorBlock(kind: 'markdown' | 'image' = 'markdown', text = ''): AuthorBlock {
  return { key: crypto.randomUUID(), kind, text, url: '', caption: '' };
}

export function authorBlock(block: ContentBlock): AuthorBlock {
  return {
    key: block.id,
    id: block.id,
    kind: block.kind,
    text: block.kind === 'image' ? (block.content.altText ?? '') : (block.content.markdown ?? ''),
    url: block.content.url ?? '',
    caption: block.content.caption ?? '',
    ...(block.kind === 'image' ? standaloneImageLayout(block.content.layout) : {}),
    original: block,
  };
}

export function standaloneImageLayout(value?: string): ImageLayout {
  const [width, placement] = (value ?? '').split(':');
  return imageLayout(width, placement);
}

function serializedImageLayout(block: AuthorBlock) {
  const { width, placement } = imageLayout(block.width, block.placement);
  return `${width}:${placement}`;
}

export function supportedBlock(block: AuthorBlock): boolean {
  return block.kind === 'markdown' || block.kind === 'image';
}

/** Server-compatible block mutation payload; image Text is accessible alternative text. */
export function blockRequest(block: AuthorBlock, index?: number) {
  if (!supportedBlock(block)) throw new Error('This block type is not editable yet.');
  return {
    kind: block.kind,
    text: block.text,
    url: block.kind === 'image' ? block.url : null,
    caption: block.kind === 'image' ? block.caption : null,
    ...(block.kind === 'image' ? { layout: serializedImageLayout(block) } : {}),
    ...(index === undefined ? {} : { index }),
  };
}

export function validateAuthorBlocks(blocks: AuthorBlock[]): string | null {
  if (!blocks.length) return 'Add at least one content block.';
  for (const [i, block] of blocks.entries()) {
    if (block.kind === 'image') {
      if (!publicUrl(block.url)) return `Block ${i + 1}: use an absolute HTTP(S) image URL.`;
      if (!block.text.trim()) return `Block ${i + 1}: describe the image for screen readers.`;
      if (block.text.length > 2000 || block.caption.length > 2000)
        return `Block ${i + 1}: image alternative text and caption must be under 2000 characters.`;
    }
    if (block.kind === 'markdown' && block.text.length > 32000)
      return `Block ${i + 1}: Markdown must be under 32000 characters.`;
  }
  return null;
}

/** The legacy API still requires shortDescription; authors never manage a duplicate field. */
export function derivedExcerpt(blocks: AuthorBlock[], title = ''): string {
  for (const block of blocks) {
    let value = block.kind === 'markdown'
      ? block.text
          .replace(/!\[[^\]]*\]\([^)]*\)/g, '')
          .replace(/\[([^\]]+)\]\([^)]*\)/g, '$1')
          .replace(/[#*_~\x60>]/g, '')
          .replace(/\s+/g, ' ')
          .trim()
      : block.kind === 'image'
        ? (block.caption.trim() || block.text.trim())
        : '';
    if (value) return value.slice(0, 280);
  }
  return title.trim().slice(0, 280);
}

export function contentForPreview(blocks: AuthorBlock[]): ContentBlock[] {
  return blocks.map((block) => block.original && !supportedBlock(block)
    ? block.original
    : {
      id: block.id ?? block.key,
      kind: block.kind,
      content: block.kind === 'image'
        ? { url: block.url, altText: block.text, caption: block.caption, layout: serializedImageLayout(block) }
        : { markdown: block.text },
    });
}

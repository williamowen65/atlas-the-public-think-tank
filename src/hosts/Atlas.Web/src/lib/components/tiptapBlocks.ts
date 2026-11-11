import type { JSONContent } from '@tiptap/core';
import { renderEditorMarkdown } from '../markdown/renderMarkdown';
import { publicUrl } from '../api/nodePage';
import type { AuthorBlock } from '../api/contentAuthoring';

type MarkdownBaseline = ReadonlyMap<string, { serialized: string; source: string }>;

export function equivalentMarkdown(source: string, serialized: string): boolean {
  return renderEditorMarkdown(source) === renderEditorMarkdown(serialized);
}

function escapeAttribute(value: string): string {
  return value.replace(/&/g, '&amp;').replace(/"/g, '&quot;')
    .replace(/</g, '&lt;').replace(/>/g, '&gt;');
}

/** Initial HTML is parsed into custom Tiptap block nodes, not flat paragraphs. */
export function blocksToTiptapHtml(blocks: AuthorBlock[]): string {
  return blocks.map(block => {
    const key = escapeAttribute(block.key);
    if (block.kind === 'markdown') {
      return '<div data-atlas-text="true" data-atlas-key="' + key + '">' +
        (renderEditorMarkdown(block.text) || '<p></p>') + '</div>';
    }
    if (block.kind === 'image') {
      const src = publicUrl(block.url) || '';
      return '<figure data-atlas-image="true" data-atlas-key="' + key +
        '" data-src="' + escapeAttribute(block.url) +
        '" data-alt="' + escapeAttribute(block.text) +
        '" data-caption="' + escapeAttribute(block.caption) +
        '"><img src="' + escapeAttribute(src) + '" alt="' + escapeAttribute(block.text) +
        '"><figcaption>' + escapeAttribute(block.caption) + '</figcaption></figure>';
    }
    // Unimplemented kinds stay in the ordered document as protected placeholders.
    return '<div data-atlas-unsupported="true" data-atlas-key="' + key +
      '" data-atlas-kind="' + escapeAttribute(block.kind) + '">Unsupported ' +
      escapeAttribute(block.kind) + ' block — retained in source data</div>';
  }).join('') || '<div data-atlas-text="true" data-atlas-key="new-empty"><p></p></div>';
}

function inlineMarkdown(node: JSONContent): string {
  if (node.type === 'atlasInlineImage') {
    const url = publicUrl(String(node.attrs?.src ?? ''));
    if (!url) return '';
    const alt = String(node.attrs?.alt ?? '').replace(/([\\\[\]])/g, '\\$1');
    return '![' + alt + '](' + url.replace(/\(/g, '%28').replace(/\)/g, '%29') + ')';
  }
  if (node.type === 'hardBreak') return '  \n';
  if (node.type === 'text') {
    const text = node.text ?? '';
    const code = node.marks?.some((mark) => mark.type === 'code');
    const leading = text.match(/^\s*/)?.[0] ?? '';
    const trailing = text.trim() ? (text.match(/\s*$/)?.[0] ?? '') : '';
    const body = text.slice(leading.length, text.length - trailing.length);
    if (!body) return text;
    let value = code ? body : body.replace(/([\\`*_~\[\]<>])/g, '\\$1');
    for (const mark of node.marks ?? []) {
      if (mark.type === 'bold') value = '**' + value + '**';
      else if (mark.type === 'italic') value = '*' + value + '*';
      else if (mark.type === 'strike') value = '~~' + value + '~~';
      else if (mark.type === 'code') value = String.fromCharCode(96) + value + String.fromCharCode(96);
      else if (mark.type === 'link' && mark.attrs?.href) {
        const href = String(mark.attrs.href).replace(/\(/g, '%28').replace(/\)/g, '%29');
        const title = String(mark.attrs.title ?? '').replace(/([\\"])/g, '\\$1');
        value = '[' + value + '](' + href + (title ? ' "' + title + '"' : '') + ')';
      }
    }
    return leading + value + trailing;
  }
  return (node.content ?? []).map(inlineMarkdown).join('');
}

function blockMarkdown(node: JSONContent): string {
  const text = (node.content ?? []).map(inlineMarkdown).join('');
  switch (node.type) {
    case 'heading':
      return '#'.repeat(Math.max(1, Math.min(6, Number(node.attrs?.level ?? 2)))) + ' ' + text;
    case 'horizontalRule': return '---';
    case 'codeBlock': {
      const source = (node.content ?? []).map((child) => child.text ?? '').join('');
      const fence = '`'.repeat(Math.max(3, ...(source.match(/`+/g) ?? []).map((run) => run.length + 1)));
      return fence + String(node.attrs?.language ?? '') + '\n' + source + '\n' + fence;
    }
    case 'blockquote':
      return (node.content ?? []).map(blockMarkdown).join('\n').split('\n')
        .map(line => '> ' + line).join('\n');
    case 'bulletList':
    case 'orderedList':
      return (node.content ?? []).map((item, index) => {
        const marker = node.type === 'bulletList' ? '- ' : `${Number(node.attrs?.start ?? 1) + index}. `;
        const [first, ...rest] = (item.content ?? []).map(blockMarkdown);
        return marker + (first ?? '') + rest.map((part, i) =>
          (item.content?.[i + 1]?.type === 'paragraph' ? '\n\n' : '\n')
          + part.split('\n').map((line) => ' '.repeat(marker.length) + line).join('\n')
        ).join('');
      }).join('\n');
    case 'paragraph':
    default: return text;
  }
}

/**
 * Keep IDs and original payloads from the previous authoring model. This
 * lets reordering/editing the Tiptap view compare against real Atlas blocks.
 */
export function tiptapDocumentToBlocks(
  doc: JSONContent, previous: AuthorBlock[] = [], baseline?: MarkdownBaseline,
): AuthorBlock[] {
  const byKey = new Map(previous.map(block => [block.key, block]));
  const output: AuthorBlock[] = [];
  const seen = new Set<string>();

  for (const node of doc.content ?? []) {
    const candidate = String(node.attrs?.key || '');
    const key = candidate && !seen.has(candidate) ? candidate : crypto.randomUUID();
    seen.add(key);
    const original = byKey.get(key);

    if (node.type === 'atlasImage') {
      const url = String(node.attrs?.src ?? '');
      output.push({
        ...(original?.kind === 'image' ? original : {}),
        key, kind: 'image', url,
        text: String(node.attrs?.alt ?? ''),
        caption: String(node.attrs?.caption ?? ''),
      });
    } else if (node.type === 'atlasUnsupported') {
      if (original && original.kind !== 'markdown' && original.kind !== 'image')
        output.push(original);
    } else {
      const serialized = node.type === 'atlasText'
        ? (node.content ?? []).map(blockMarkdown).join('\n\n')
        : blockMarkdown(node);
      const initial = baseline?.get(key);
      output.push({
        ...(original?.kind === 'markdown' ? original : {}),
        key, kind: 'markdown',
        text: initial?.serialized === serialized ? initial.source : serialized,
        url: '', caption: '',
      });
    }
  }

  return output.length ? output : [{
    key: crypto.randomUUID(), kind: 'markdown', text: '', url: '', caption: '',
  }];
}

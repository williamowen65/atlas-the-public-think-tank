/**
 * Storybook-only mapping between Atlas AuthorBlock[] and a Tiptap document.
 *
 * Every Atlas text block becomes a containing atlasText node (with its key).
 * Paragraphs, headings and lists inside that node remain ONE Markdown block.
 * Images remain atomic atlasImage nodes. The key maps back to id/original data.
 *
 * This proves basic block storage compatibility; it is NOT a production or
 * complete Markdown serializer. Unsupported marks/embeds require more work.
 */
import type { JSONContent } from '@tiptap/core';
import MarkdownIt from 'markdown-it';
import { publicUrl } from '../../src/lib/api/nodePage';
import type { AuthorBlock } from '../../src/lib/api/contentAuthoring';

const markdown = new MarkdownIt({ html: false, linkify: true });

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
        (markdown.render(block.text) || '<p></p>') + '</div>';
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
  // Storybook-only Markdown images remain inside ONE Atlas text block and
  // round-trip through Tiptap as inline atoms. Production policy still
  // disables Markdown images until an explicit security/product decision.
  if (node.type === 'atlasInlineImage') {
    const url = publicUrl(String(node.attrs?.src ?? ''));
    if (!url) return '';
    const alt = String(node.attrs?.alt ?? '').replace(/([\\\[\]])/g, '\\$1');
    return '![' + alt + '](' + url.replace(/\)/g, '%29') + ')';
  }
  if (node.type === 'hardBreak') return '  \n';
  if (node.type === 'text') {
    let value = node.text ?? '';
    for (const mark of node.marks ?? []) {
      if (mark.type === 'bold') value = '**' + value + '**';
      else if (mark.type === 'italic') value = '*' + value + '*';
      else if (mark.type === 'strike') value = '~~' + value + '~~';
      else if (mark.type === 'code') value = String.fromCharCode(96) + value + String.fromCharCode(96);
      else if (mark.type === 'link' && /^https?:\/\//.test(String(mark.attrs?.href ?? '')))
        value = '[' + value + '](' + String(mark.attrs?.href) + ')';
    }
    return value;
  }
  return (node.content ?? []).map(inlineMarkdown).join('');
}

function blockMarkdown(node: JSONContent): string {
  const text = (node.content ?? []).map(inlineMarkdown).join('');
  switch (node.type) {
    case 'heading':
      return '#'.repeat(Math.max(1, Math.min(3, Number(node.attrs?.level ?? 2)))) + ' ' + text;
    case 'horizontalRule': return '---';
    case 'codeBlock': return String.fromCharCode(96).repeat(3) + '\n' + text + '\n' + String.fromCharCode(96).repeat(3);
    case 'blockquote':
      return (node.content ?? []).map(blockMarkdown).join('\n').split('\n')
        .map(line => '> ' + line).join('\n');
    case 'bulletList':
      return (node.content ?? []).map(item =>
        '- ' + (item.content ?? []).map(blockMarkdown).join('\n')).join('\n');
    case 'orderedList':
      return (node.content ?? []).map((item, i) =>
        String(i + 1) + '. ' + (item.content ?? []).map(blockMarkdown).join('\n')).join('\n');
    case 'paragraph':
    default: return text;
  }
}

/**
 * Keep IDs and original payloads from the previous authoring model. This
 * lets reordering/editing the Tiptap view compare against real Atlas blocks.
 */
export function tiptapDocumentToBlocks(doc: JSONContent, previous: AuthorBlock[] = []): AuthorBlock[] {
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
      output.push({
        ...(original?.kind === 'markdown' ? original : {}),
        key, kind: 'markdown',
        text: node.type === 'atlasText'
          ? (node.content ?? []).map(blockMarkdown).join('\n\n')
          : blockMarkdown(node),
        url: '', caption: '',
      });
    }
  }

  return output.length ? output : [{
    key: crypto.randomUUID(), kind: 'markdown', text: '', url: '', caption: '',
  }];
}

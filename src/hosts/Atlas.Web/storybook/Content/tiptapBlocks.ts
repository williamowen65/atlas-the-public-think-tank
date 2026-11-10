/**
 * Storybook-only bridge between the existing Atlas author blocks and Tiptap.
 *
 * This is deliberately a lossy exploration, NOT an API persistence adapter.
 * New paragraphs may create new block keys and Markdown features outside this
 * small serializer will not round-trip. Production would need a designed
 * document/block identity migration and tests.
 */
import type { JSONContent } from '@tiptap/core';
import MarkdownIt from 'markdown-it';
import { publicUrl } from '../../src/lib/api/nodePage';
import type { AuthorBlock } from '../../src/lib/api/contentAuthoring';

const markdown = new MarkdownIt({ html: false, linkify: true });
const escapeAttribute = (value: string): string =>
  value.replace(/&/g, '&amp;').replace(/"/g, '&quot;').replace(/</g, '&lt;').replace(/>/g, '&gt;');

export function blocksToTiptapHtml(blocks: AuthorBlock[]): string {
  const html = blocks.map(block => {
    if (block.kind === 'markdown') return markdown.render(block.text || '');
    if (block.kind === 'image' && publicUrl(block.url)) {
      return `<figure data-atlas-image="true" data-atlas-key="${escapeAttribute(block.key)}" data-src="${escapeAttribute(block.url)}" data-alt="${escapeAttribute(block.text)}" data-caption="${escapeAttribute(block.caption)}"><img src="${escapeAttribute(block.url)}" alt="${escapeAttribute(block.text)}"><figcaption>${escapeAttribute(block.caption)}</figcaption></figure>`;
    }
    // Unknown node kinds are not editable in the prototype.
    return '';
  }).join('');
  return html || '<p></p>';
}

function inlineMarkdown(node: JSONContent): string {
  if (node.type === 'hardBreak') return '  \n';
  if (node.type === 'text') {
    let value = node.text ?? '';
    for (const mark of node.marks ?? []) {
      if (mark.type === 'bold') value = `**${value}**`;
      else if (mark.type === 'italic') value = `*${value}*`;
      else if (mark.type === 'strike') value = `~~${value}~~`;
      else if (mark.type === 'code') value = '`' + value + '`';
      else if (mark.type === 'link' && /^https?:\/\//.test(String(mark.attrs?.href ?? '')))
        value = `[${value}](${mark.attrs?.href})`;
    }
    return value;
  }
  return (node.content ?? []).map(inlineMarkdown).join('');
}

function blockMarkdown(node: JSONContent): string {
  const text = (node.content ?? []).map(inlineMarkdown).join('');
  switch (node.type) {
    case 'heading': return '#'.repeat(Math.max(1, Math.min(3, Number(node.attrs?.level ?? 2)))) + ' ' + text;
    case 'horizontalRule': return '---';
    case 'codeBlock': return '```\n' + text + '\n```';
    case 'blockquote': return (node.content ?? []).map(blockMarkdown).join('\n').split('\n').map(line => '> ' + line).join('\n');
    case 'bulletList': return (node.content ?? []).map(item => '- ' + (item.content ?? []).map(blockMarkdown).join(' ')).join('\n');
    case 'orderedList': return (node.content ?? []).map((item, index) => `${index + 1}. ${(item.content ?? []).map(blockMarkdown).join(' ')}`).join('\n');
    case 'paragraph':
    default: return text;
  }
}

export function tiptapDocumentToBlocks(doc: JSONContent): AuthorBlock[] {
  const blocks: AuthorBlock[] = [];
  for (const [index, node] of (doc.content ?? []).entries()) {
    if (node.type === 'atlasImage') {
      const url = String(node.attrs?.src ?? '');
      blocks.push({
        key: String(node.attrs?.key || `storybook-image-${index}`),
        kind: 'image',
        url: publicUrl(url) ? url : '',
        text: String(node.attrs?.alt ?? ''),
        caption: String(node.attrs?.caption ?? ''),
      });
    } else {
      blocks.push({
        key: `storybook-text-${index}`,
        kind: 'markdown',
        text: blockMarkdown(node),
        url: '',
        caption: '',
      });
    }
  }
  return blocks.length ? blocks : [{ key: 'storybook-empty', kind: 'markdown', text: '', url: '', caption: '' }];
}

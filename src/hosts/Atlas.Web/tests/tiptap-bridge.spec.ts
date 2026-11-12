import { expect, test } from '@playwright/test';
import { blocksToTiptapHtml, tiptapDocumentToBlocks } from '../src/lib/components/tiptapBlocks';
import { authorBlock, blockRequest, contentForPreview, type AuthorBlock } from '../src/lib/api/contentAuthoring';
import { markdownImages, renderMarkdown } from '../src/lib/markdown/renderMarkdown';

test('markdown and image blocks preserve their keys, saved IDs and reordering', () => {
  const starting: AuthorBlock[] = [
    { key: 'intro', id: '00000000-0000-4000-8000-000000000101',
      kind: 'markdown', text: 'First paragraph.\n\nSecond paragraph.', url: '', caption: '' },
    { key: 'photo', id: '00000000-0000-4000-8000-000000000102',
      kind: 'image', text: 'Green trees', url: 'https://example.org/tree.jpg', caption: 'Our square' },
  ];
  const html = blocksToTiptapHtml(starting);
  expect(html).toContain('data-atlas-text="true" data-atlas-key="intro"');
  expect(html).toContain('data-atlas-image="true" data-atlas-key="photo"');

  const moved = tiptapDocumentToBlocks({
    type: 'doc',
    content: [
      { type: 'atlasImage', attrs: { key: 'photo',
        src: 'https://example.org/tree.jpg', alt: 'Green trees', caption: 'Our square' } },
      { type: 'atlasText', attrs: { key: 'intro' }, content: [
        { type: 'paragraph', content: [{ type: 'text', text: 'First paragraph.' }] },
        { type: 'paragraph', content: [{ type: 'text', text: 'Second paragraph.' }] },
      ] },
    ],
  }, starting);
  expect(moved.map(block => block.key)).toEqual(['photo', 'intro']);
  expect(moved.map(block => block.id)).toEqual([
    '00000000-0000-4000-8000-000000000102',
    '00000000-0000-4000-8000-000000000101',
  ]);
  expect(moved[1].text).toBe('First paragraph.\n\nSecond paragraph.');
  expect(moved[0].text).toBe('Green trees');
  expect(moved[0].caption).toBe('Our square');
});

test('editing common Markdown marks remains representable in Atlas text blocks', () => {
  const output = tiptapDocumentToBlocks({
    type: 'doc',
    content: [{ type: 'atlasText', attrs: { key: 'body' }, content: [{
      type: 'paragraph', content: [
        { type: 'text', text: 'Make ' },
        { type: 'text', text: 'streets', marks: [{ type: 'bold' }] },
        { type: 'text', text: ' greener.' },
      ],
    }] }],
  });
  expect(output).toHaveLength(1);
  expect(output[0]).toMatchObject({ key: 'body', kind: 'markdown', text: 'Make **streets** greener.' });
});

test('unknown future block kinds survive an unchanged Tiptap document pass', () => {
  const original: AuthorBlock = {
    key: 'poll', id: '00000000-0000-4000-8000-000000000103',
    kind: 'poll-reference', text: '', url: '', caption: '',
    original: {
      id: '00000000-0000-4000-8000-000000000103',
      kind: 'poll-reference',
      content: { pollId: '00000000-0000-4000-8000-000000000104' },
    },
  };
  const html = blocksToTiptapHtml([original]);
  expect(html).toContain('data-atlas-unsupported="true"');
  const result = tiptapDocumentToBlocks({
    type: 'doc',
    content: [{ type: 'atlasUnsupported', attrs: { key: 'poll', kind: 'poll-reference' } }],
  }, [original]);
  expect(result).toEqual([original]);
});

test('an unfinished image URL is retained for the form to validate', () => {
  const input: AuthorBlock = { key: 'image-pending', kind: 'image', url: 'not-a-url',
    text: 'Alt text', caption: '' };
  expect(blocksToTiptapHtml([input])).toContain('data-src="not-a-url"');
  const output = tiptapDocumentToBlocks({
    type: 'doc',
    content: [{ type: 'atlasImage', attrs: {
      key: 'image-pending', src: 'not-a-url', alt: 'Alt text', caption: '',
    } }],
  }, [input]);
  expect(output[0].url).toBe('not-a-url');
});

test('image presets round-trip through the editor, publication payload and gallery', () => {
  const [markdown] = tiptapDocumentToBlocks({
    type: 'doc',
    content: [{ type: 'atlasText', attrs: { key: 'intro' }, content: [{
      type: 'paragraph', content: [{ type: 'atlasInlineImage', attrs: {
        src: 'https://example.org/photo.jpg', alt: 'Street', width: 'quarter', placement: 'right',
        title: 'An evening view',
      } }],
    }] }],
  });
  expect(markdown.text).toContain('"atlas-image:quarter:right An evening view"');
  const html = renderMarkdown(markdown.text, true, false, true);
  expect(html).toContain('data-image-placement="right"');
  expect(html).toContain('--atlas-image-width:25%');
  expect(markdownImages(markdown.text)[0].caption).toBe('An evening view');
  expect(blocksToTiptapHtml([markdown])).toContain('data-image-width="quarter"');

  const standalone = authorBlock({
    id: 'image', kind: 'image',
    content: { url: 'https://example.org/photo.jpg', altText: 'Street', layout: 'third:center' },
  });
  expect(blockRequest(standalone).layout).toBe('third:center');
  expect(contentForPreview([standalone])[0].content.layout).toBe('third:center');
  expect(blocksToTiptapHtml([standalone])).toContain('data-image-placement="center"');
});

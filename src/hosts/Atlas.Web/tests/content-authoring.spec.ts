import { expect, test } from '@playwright/test';
import {
  authorBlock, blockRequest, contentForPreview, derivedExcerpt, validateAuthorBlocks,
  type AuthorBlock,
} from '../src/lib/api/contentAuthoring';

const text = (key: string, value: string): AuthorBlock => ({
  key, kind: 'markdown', text: value, url: '', caption: '',
});
const image = (key: string, url = 'https://example.org/idea.png', alt = 'An example'): AuthorBlock => ({
  key, kind: 'image', text: alt, url, caption: '',
});

test('first meaningful content derives a bounded legacy excerpt without separate author copy', () => {
  expect(derivedExcerpt([text('one', '## **Hello** [world](https://example.org)')])).toBe('Hello world');
  expect(derivedExcerpt([image('first', undefined, 'An image'), text('second', 'Body')])).toBe('An image');
  expect(derivedExcerpt([text('empty', '')], 'Node title')).toBe('Node title');
  expect(derivedExcerpt([text('long', 'a'.repeat(500))])).toHaveLength(280);
});

test('image authoring requires an absolute safe URL and alternative text', () => {
  expect(validateAuthorBlocks([image('x', 'javascript:alert(1)')])).toContain('HTTP(S)');
  expect(validateAuthorBlocks([image('x', 'https://example.org/photo.png', '')])).toContain('screen readers');
  expect(validateAuthorBlocks([image('x')])).toBeNull();
  expect(blockRequest(image('x')).kind).toBe('image');
  expect(blockRequest(image('x')).text).toBe('An example');
});

test('existing unsupported blocks retain original identity and payload in preview', () => {
  const original = {
    id: 'd1000000-0000-4000-8000-000000000001',
    kind: 'poll-reference',
    content: { pollId: 'p1000000-0000-4000-8000-000000000001' },
  };
  const model = authorBlock(original);
  expect(model.id).toBe(original.id);
  expect(contentForPreview([model])[0]).toEqual(original);
  expect(() => blockRequest(model)).toThrow(/not editable/);
});

test('user-controlled order is retained by the preview projection', () => {
  const blocks = [text('intro', 'First'), image('photo'), text('last', 'Last')];
  expect(contentForPreview(blocks).map((block) => block.kind)).toEqual(['markdown', 'image', 'markdown']);
  expect(validateAuthorBlocks([])).toContain('at least one');
});

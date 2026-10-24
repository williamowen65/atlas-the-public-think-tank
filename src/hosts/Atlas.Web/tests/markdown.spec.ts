import { test, expect } from '@playwright/test';
import { renderMarkdown } from '../src/lib/markdown/renderMarkdown';

// Pure rendering checks need no browser or running API, but execute in the normal CI suite.
test('Markdown supports basic formatting and preserves page heading hierarchy', () => {
  const html = renderMarkdown(
    '# Heading\n\n**Bold** and *italic* with `code`.\n\n- First\n- Second\n\n> Quote\n\n```js\nconst x = 1;\n```\n\n[Source](https://example.org)',
  );
  expect(html).toContain('<h2>Heading</h2>');
  for (const tag of ['strong', 'em', 'code', 'ul', 'blockquote', 'pre'])
    expect(html).toContain(`<${tag}`);
  expect(html).toContain('href="https://example.org"');
  expect(renderMarkdown('###### Deep')).toContain('<h6>Deep</h6>');
});

test('Markdown rejects executable HTML, unsafe URLs and embedded images', () => {
  const html = renderMarkdown(
    '<script>alert(1)</script>\n\n<img src=x onerror=alert(1)>\n\n![image](https://example.org/image.png)',
  );
  expect(html).not.toMatch(/<(script|img)\b/i);
  expect(html).toContain('&lt;script&gt;');
  for (const url of [
    'javascript:alert(1)',
    'JaVaScRiPt:alert(1)',
    'jav&#x61;script:alert(1)',
    'data:text/html,test',
    'vbscript:test',
    'file:///tmp/test',
  ]) {
    expect(renderMarkdown(`[click](${url})`)).not.toContain('href=');
  }
  expect(renderMarkdown('[internal](/nodes?id=123)')).toContain('href="/nodes?id=123"');
  expect(renderMarkdown('[anchor](#details)')).toContain('href="#details"');
});

test('Markdown escapes link titles and keeps image references from loading', () => {
  const html = renderMarkdown('[source](https://example.org "\\" onmouseover=\\"alert(1)")');
  expect(html).not.toMatch(/title="[^"]*"\s+onmouseover=/i);
  expect(renderMarkdown('![photo][ref]\n\n[ref]: https://example.org/photo.png')).not.toContain(
    '<img',
  );
  expect(renderMarkdown('`<script>alert(1)</script>`')).toContain('&lt;script&gt;');
});

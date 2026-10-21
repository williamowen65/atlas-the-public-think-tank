import { expect, test, type Page } from '@playwright/test';
const root = 'b1000000-0000-4000-8000-000000000001';
const child = 'b1000000-0000-4000-8000-000000000002';
const idea = 'a1000000-0000-4000-8000-000000000001';
const evidence = 'a1000000-0000-4000-8000-000000000002';
const types = [{ id: idea, name: 'Idea' }, { id: evidence, name: 'Evidence' }];
const entry = (id = root) => ({ nodeId: id, averageVote: 7, voteCount: 2, card: { title: id === root ? 'A live question' : 'A child idea', shortDescription: 'A concise introduction.', typeName: 'Question', authorName: 'Author', status: 'Active', createdAt: '2026-10-21T12:00:00Z', breadcrumb: '', subNodes: '1 Idea', communities: 'Atlas', commentCount: 1, isHidden: false } });
async function mock(page: Page) {
  const urls: URL[] = [];
  await page.route('**/atlas-preview-api/**', route => {
    const url = new URL(route.request().url()); urls.push(url);
    if (url.pathname.endsWith('/catalog/node-types')) return route.fulfill({ json: types });
    if (url.pathname.endsWith('/reactions')) return route.fulfill({ json: [] });
    if (url.pathname.endsWith('/description')) return route.fulfill({ json: [{ id: 'block', kind: 'markdown', content: { markdown: '## Full explanation\n\nThe complete content belongs here.\n\n<script>alert(1)</script>' } }] });
    if (url.pathname.endsWith('/comments')) return route.fulfill({ json: { items: [{ id: 'comment', authorId: 'participant', body: 'A public comment', createdAt: '2026-10-21T12:00:00Z', status: 'Active' }], next: null } });
    if (/\/nodes\/[^/]+$/.test(url.pathname)) return route.fulfill({ json: { id: root, requestedTypeIds: [evidence], status: 'Active' } });
    return route.fulfill({ json: { items: [entry(url.searchParams.has('ParentNodeId') ? child : url.searchParams.get('NodeId') ?? root)], totalCount: 1, page: Number(url.searchParams.get('Page')), pageSize: 3, hasNextPage: false } });
  });
  return urls;
}

test('Read more opens the matching node story; tabs load child feeds and a parented form', async ({ page }) => {
  const urls = await mock(page);
  await page.goto('http://127.0.0.1:6006/iframe.html?id=compositions-feed--playground&viewMode=story');
  await page.getByRole('button', { name: 'Read more', exact: true }).click();
  await expect(page).toHaveURL(/compositions-node-page/);
  await expect(page.getByRole('heading', { name: 'Question: A live question' })).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Full explanation' })).toBeVisible();
  await expect(page.getByText('A public comment', { exact: true })).toBeVisible();
  await expect(page.locator('.content-blocks script')).toHaveCount(0);
  expect(urls.some(url => url.searchParams.get('NodeId') === root)).toBe(true);
  await page.getByRole('tab', { name: 'Idea', exact: true }).click();
  await expect(page.getByRole('heading', { name: 'Question: A child idea' })).toBeVisible();
  expect(urls.some(url => url.searchParams.get('ParentNodeId') === root && url.searchParams.get('NodeTypeId') === idea)).toBe(true);
  await page.getByRole('button', { name: 'Add Idea', exact: true }).click();
  await expect(page.getByRole('form', { name: 'Create child node' })).toBeVisible();
  await expect(page.getByLabel('Node type', { exact: true })).toHaveValue(idea);
  await expect(page.getByRole('button', { name: 'Create node', exact: true })).toBeDisabled();
});

test('root form previews a new type and never writes to the API', async ({ page }) => {
  const writes: string[] = []; page.on('request', request => { if (request.method() !== 'GET') writes.push(request.url()); });
  await mock(page);
  await page.goto('http://127.0.0.1:6006/iframe.html?id=components-node-form--playground&viewMode=story');
  await page.getByLabel('Title', { exact: true }).fill('A root idea');
  await page.getByLabel('Short description', { exact: true }).fill('A short authored summary.');
  await page.getByLabel('Node type', { exact: true }).selectOption('new');
  await page.getByLabel('Type name', { exact: true }).fill('Observation');
  await page.getByLabel('Content (Markdown)', { exact: true }).fill('## Supporting content\n\nA longer explanation.');
  await page.getByRole('button', { name: 'Preview draft' }).click();
  await expect(page.getByRole('region', { name: 'Draft preview' })).toContainText('Observation');
  await expect(page.getByRole('status')).toHaveText('Draft preview only. Nothing has been saved.');
  expect(writes).toEqual([]);
});

test('SPA Read more opens a node page and child tabs support keyboard navigation', async ({ page }) => {
  await mock(page);
  await page.setViewportSize({ width: 320, height: 720 });
  await page.goto('http://127.0.0.1:4173');
  await page.getByRole('button', { name: 'Read more', exact: true }).click();
  await expect(page).toHaveURL(new RegExp(`/nodes\\?id=${root}`));
  const comments = page.getByRole('tab', { name: 'Comments', exact: true });
  await comments.focus(); await page.keyboard.press('ArrowRight');
  await expect(page.getByRole('tab', { name: 'Evidence', exact: true })).toHaveAttribute('aria-selected', 'true');
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(320);
});

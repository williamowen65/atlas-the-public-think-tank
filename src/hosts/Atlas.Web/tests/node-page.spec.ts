import { expect, test, type Page } from '@playwright/test';
const root = 'b1000000-0000-4000-8000-000000000001';
const child = 'b1000000-0000-4000-8000-000000000002';
const idea = 'a1000000-0000-4000-8000-000000000001';
const evidence = 'a1000000-0000-4000-8000-000000000002';
const types = [{ id: idea, name: 'Idea', autoPluralize: true, pluralName: 'Ideas' }, { id: evidence, name: 'Evidence', autoPluralize: false, pluralName: 'Evidence' }];
const entry = (id = root) => ({ nodeId: id, averageVote: 7, voteCount: 2, card: { title: id === root ? 'A live question' : 'A child idea', shortDescription: 'A concise introduction.', typeName: 'Question', authorName: 'Author', status: 'Active', createdAt: '2026-10-21T12:00:00Z', breadcrumb: '', subNodes: '1 Idea · 0 Evidence', childTypes: [{ typeId: idea, label: '1 Idea', count: 1 }, { typeId: evidence, label: '0 Evidence', count: 0 }], communities: 'Atlas', commentCount: 1, isHidden: false } });
async function mock(page: Page) {
  await page.route('**/atlas-comments-api/**', route => {
    if (route.request().url().endsWith('/auth/session')) return route.fulfill({ json: { participantId: 'participant' } });
    return route.fulfill({ json: { displayName: 'Author' } });
  });
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
  await page.getByRole('tab', { name: '1 Idea', exact: true }).click();
  await expect(page.getByRole('heading', { name: 'Question: A child idea' })).toBeVisible();
  expect(urls.some(url => url.searchParams.get('ParentNodeId') === root && url.searchParams.get('NodeTypeId') === idea)).toBe(true);
  await page.getByRole('button', { name: 'Add Idea', exact: true }).click();
  await expect(page.getByRole('form', { name: 'Create child node' })).toBeVisible();
  await expect(page.getByLabel('Node type', { exact: true })).toHaveValue(idea);
  await expect(page.getByLabel('Node type', { exact: true })).toBeFocused();
  const form = page.getByRole('form', { name: 'Create child node' });
  await expect(page.getByRole('tabpanel').getByRole('form')).toHaveCount(1);
  await expect(form.getByRole('navigation')).toContainText('A live question');
  await expect.poll(async () => (await form.boundingBox())?.y).toBeGreaterThanOrEqual(0);
  await expect.poll(async () => (await form.boundingBox())?.y ?? Infinity).toBeLessThan(150);
  await expect(page.getByRole('tabpanel').locator('form')).toHaveCount(1);
  await expect(page.getByRole('button', { name: 'Create node', exact: true })).toBeEnabled();
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
  await expect(page.getByRole('region', { name: 'Content preview' })).toContainText('Supporting content');
  await page.getByLabel('Content (Markdown)', { exact: true }).fill('## Revised content');
  await expect(page.getByRole('region', { name: 'Content preview' })).toContainText('Revised content');
  await expect(page.getByRole('button', { name: 'Preview draft' })).toHaveCount(0);
  await expect(page.getByRole('spinbutton')).toHaveAttribute('aria-disabled', 'true');
  await expect(page.getByText('0 Comments', { exact: true })).toHaveCount(0);
  const contributions = page.getByLabel('Requested contributions', { exact: true });
  await expect(contributions.getByRole('option', { name: 'Ideas', exact: true })).toHaveCount(1);
  await expect(contributions.getByRole('option', { name: 'Evidence', exact: true })).toHaveCount(1);
  await contributions.selectOption(idea);
  await expect(page.getByRole('list', { name: 'Requested contribution types' })).toContainText('Ideas');
  await page.getByRole('button', { name: 'Remove Ideas contribution' }).click();
  await contributions.selectOption(evidence);
  await expect(page.getByRole('list', { name: 'Requested contribution types' })).toContainText('Evidence');
  await page.getByRole('button', { name: 'Remove Evidence contribution' }).click();
  await expect(page.getByRole('list', { name: 'Requested contribution types' })).toHaveCount(0);
  await page.setViewportSize({ width: 320, height: 720 });
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(320);
  expect(writes).toEqual([]);
});

test('SPA Read more opens a node page and child tabs support keyboard navigation', async ({ page }) => {
  await mock(page);
  await page.setViewportSize({ width: 320, height: 720 });
  await page.goto('http://127.0.0.1:4173');
  await page.getByRole('button', { name: 'Read more', exact: true }).click();
  await expect(page).toHaveURL(new RegExp(`/nodes\\?id=${root}`));
  const comments = page.getByRole('tab', { name: '1 Comment', exact: true });
  await comments.focus(); await page.keyboard.press('ArrowRight');
  await expect(page.getByRole('tab', { name: '1 Idea', exact: true })).toHaveAttribute('aria-selected', 'true');
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(320);
});


test('node page keeps a full-width ribbon and wraps post content around the dial', async ({ page }) => {
  await mock(page);
  await page.route('**/atlas-preview-api/nodes/*/reactions', route => route.fulfill({ json: [{ associationId: 'helpful', text: 'Helpful', emoji: '👍', totalVotes: 2 }] }));
  await page.route('**/atlas-preview-api/nodes/*/description', route => route.fulfill({ json: [{ id: 'body', kind: 'markdown', content: { markdown: 'Supporting detail. '.repeat(150) } }] }));
  await page.route('**/atlas-preview-api/nodes?**', route => route.fulfill({ json: { items: [{ ...entry(), card: { ...entry().card, breadcrumb: 'A parent question' } }], totalCount: 1, page: 1, pageSize: 3, hasNextPage: false } }));
  await page.goto('http://127.0.0.1:6006/iframe.html?id=compositions-node-page--playground&viewMode=story');
  const card = page.locator('.node-page');
  await expect(card.getByRole('heading', { name: 'Question: A live question' })).toBeVisible();
  for (const width of [800, 320]) {
    await page.setViewportSize({ width, height: 900 });
    const layout = await card.evaluate(el => {
      const bounds = el.getBoundingClientRect();
      const ribbon = el.querySelector('nav')!.getBoundingClientRect();
      const dial = el.querySelector('.dial')!.getBoundingClientRect();
      const text = el.querySelector('.content-blocks p')!.firstChild!;
      const range = document.createRange();
      range.setStart(text, 0); range.setEnd(text, 1);
      const first = range.getBoundingClientRect();
      let wrapsBelow = false;
      for (let index = 0; index < text.textContent!.length; index++) {
        range.setStart(text, index); range.setEnd(text, index + 1);
        const rect = range.getBoundingClientRect();
        if (rect.top > dial.bottom && rect.left < dial.right) { wrapsBelow = true; break; }
      }
      const content = el.querySelector('.content-blocks')!.getBoundingClientRect();
      const reactions = el.querySelector('.reactions')!.getBoundingClientRect();
      const actions = el.querySelector('.add')!.getBoundingClientRect();
      return { left: ribbon.left - bounds.left, right: bounds.right - ribbon.right, startsBesideDial: first.top < dial.bottom && first.left >= dial.right, wrapsBelow, reactionsAfterContent: reactions.top >= content.bottom, actionsAfterReactions: actions.top >= reactions.bottom };
    });
    expect(layout.left).toBeCloseTo(1, 0);
    expect(layout.right).toBeCloseTo(1, 0);
    expect(layout.startsBesideDial).toBe(true);
    expect(layout.wrapsBelow).toBe(true);
    expect(layout.reactionsAfterContent).toBe(true);
    expect(layout.actionsAfterReactions).toBe(true);
    expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(width);
  }
});


for (const destination of ['storybook', 'spa']) {
  for (const contribution of [{ typeId: idea, label: '1 Idea', tab: '1 Idea' }, { typeId: evidence, label: '0 Evidence', tab: '0 Evidence' }]) {
    test(`${destination} contribution button opens and reveals ${contribution.label}`, async ({ page }) => {
      const urls = await mock(page);
      if (contribution.typeId === evidence) {
        await page.route('**/atlas-preview-api/nodes?**', async route => {
          const url = new URL(route.request().url());
          if (!url.searchParams.has('ParentNodeId')) return route.fallback();
          urls.push(url);
          return route.fulfill({ json: { items: [], totalCount: 0, page: 1, pageSize: 3, hasNextPage: false } });
        });
      }
      await page.goto(destination === 'storybook'
        ? 'http://127.0.0.1:6006/iframe.html?id=compositions-feed--playground&viewMode=story'
        : 'http://127.0.0.1:4173');
      const button = page.getByRole('button', { name: contribution.label, exact: true });
      await expect(button).toHaveCSS('cursor', 'pointer');
      await button.focus();
      await page.keyboard.press('Enter');
      await expect(page).toHaveURL(destination === 'storybook' ? /compositions-node-page/ : /nodes\?id=/);
      const tab = page.getByRole('tab', { name: contribution.tab, exact: true });
      await expect(tab).toHaveAttribute('aria-selected', 'true');
      await expect(tab).toBeFocused();
      const tabs = page.getByRole('tablist', { name: 'Node discussions and child types' });
      await expect.poll(async () => Math.abs(((await tabs.boundingBox())?.y ?? Infinity) - 20)).toBeLessThanOrEqual(2);
      await expect.poll(() => urls.some(url => url.searchParams.get('ParentNodeId') === root && url.searchParams.get('NodeTypeId') === contribution.typeId)).toBe(true);
      if (contribution.typeId === evidence) await expect(page.getByText('No nodes found.', { exact: true })).toBeVisible();
      else await expect(page.getByRole('heading', { name: 'Question: A child idea' })).toBeVisible();
      await expect.poll(async () => Math.abs(((await tabs.boundingBox())?.y ?? Infinity) - 20)).toBeLessThanOrEqual(2);
    });
  }
}


test('node-page tabs use count-aware labels for only requested and populated contribution types', async ({ page }) => {
  await mock(page);
  const question = 'a1000000-0000-4000-8000-000000000003';
  const goal = 'a1000000-0000-4000-8000-000000000004';
  await page.route('**/atlas-preview-api/catalog/node-types', route => route.fulfill({ json: [...types, { id: question, name: 'Question', pluralName: 'Questions', autoPluralize: true }, { id: goal, name: 'Goal', pluralName: 'Goals', autoPluralize: true }] }));
  await page.route(`**/atlas-preview-api/nodes/${root}`, route => route.fulfill({ json: { id: root, requestedTypeIds: [question, evidence], status: 'Active' } }));
  await page.route('**/atlas-preview-api/nodes?**', route => route.fulfill({ json: { items: [{ ...entry(), card: { ...entry().card, childTypes: [{ typeId: evidence, label: '0 Evidence', count: 0 }, { typeId: idea, label: '2 Ideas', count: 2 }, { typeId: question, label: '0 Questions', count: 0 }] } }], totalCount: 1, page: 1, pageSize: 3, hasNextPage: false } }));
  await page.goto('http://127.0.0.1:6006/iframe.html?id=compositions-node-page--playground&viewMode=story');
  await expect(page.getByRole('tab')).toHaveText(['1 Comment', '0 Evidence', '2 Ideas', '0 Questions']);
  await expect(page.getByRole('tab', { name: /Goals?/ })).toHaveCount(0);
  await page.getByRole('tab', { name: '0 Questions', exact: true }).click();
  await expect(page.getByRole('tab', { name: '0 Questions', exact: true })).toHaveAttribute('aria-selected', 'true');
});

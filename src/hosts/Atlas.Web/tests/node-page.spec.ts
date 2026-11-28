import { expect, test, type Page } from '@playwright/test';
import { example } from '../src/lib/sections/sectionLayouts';
import { writeComposition } from '../src/lib/sections/composition';
const root = 'b1000000-0000-4000-8000-000000000001';
const child = 'b1000000-0000-4000-8000-000000000002';
const idea = 'a1000000-0000-4000-8000-000000000001';
const evidence = 'a1000000-0000-4000-8000-000000000002';
const types = [{ id: idea, name: 'Idea', autoPluralize: true, pluralName: 'Ideas' }, { id: evidence, name: 'Evidence', autoPluralize: false, pluralName: 'Evidence' }];
const entry = (id = root) => ({ nodeId: id, averageVote: 7, voteCount: 2, card: { title: id === root ? 'A live question' : 'A child idea', description: 'A concise introduction.', typeName: id === root ? 'Question' : 'Idea', authorName: 'Author', status: 'Active', createdAt: '2026-10-21T12:00:00Z', breadcrumb: '', subNodes: '1 Idea · 0 Evidence', childTypes: id === root ? [{ typeId: idea, label: '1 Idea', count: 1 }, { typeId: evidence, label: '0 Evidence', count: 0 }] : [], communities: 'Atlas', commentCount: 1, isHidden: false } });
async function mock(page: Page) {
  await page.route('**/atlas-session-api/**', route => {
    if (route.request().method() === 'GET' && new URL(route.request().url()).pathname.endsWith('/moderation/queue')) return route.fulfill({ status: 403 });
    if (route.request().method() === 'GET' && /^\/atlas-session-api\/nodes\/[^/]+$/.test(new URL(route.request().url()).pathname)) return route.fulfill({ json: { hidden: false, requestedTypeIds: [] } });
    if (new URL(route.request().url()).pathname.endsWith('/votes')) return route.fulfill({ json: { voteCount: 0, averageVote: null, currentParticipantVote: null } });
    if (route.request().url().endsWith('/reactions')) return route.fulfill({ json: [] });
    if (route.request().url().endsWith('/auth/session')) return route.fulfill({ json: { participantId: 'participant' } });
    return route.fulfill({ json: { displayName: 'Author' } });
  });
  const urls: URL[] = [];
  await page.route('**/atlas-public-api/**', route => {
    const url = new URL(route.request().url()); urls.push(url);
    if (url.pathname.endsWith('/catalog/node-types')) return route.fulfill({ json: types });
    if (url.pathname.endsWith('/reactions')) return route.fulfill({ json: [] });
    if (url.pathname.endsWith(`/nodes/${child}/description`)) return route.fulfill({ json: [] });
    if (url.pathname.endsWith('/description')) return route.fulfill({ json: [{ id: 'block', kind: 'markdown', content: { markdown: '## Full explanation\n\nThe complete content belongs here.\n\n<script>alert(1)</script>' } }] });
    if (url.pathname.endsWith('/comments')) return route.fulfill({ json: { items: [{ id: 'comment', authorId: 'participant', body: 'A public comment', createdAt: '2026-10-21T12:00:00Z', status: 'Active' }], next: null } });
    if (/\/nodes\/[^/]+$/.test(url.pathname)) return route.fulfill({ json: { id: root, requestedTypeIds: [evidence], status: 'Active' } });
    if (url.searchParams.has('ParentNodeId') && url.searchParams.get('NodeTypeId') === evidence) return route.fulfill({ json: { items: [], totalCount: 0, page: Number(url.searchParams.get('Page')), pageSize: 3, hasNextPage: false } });
    return route.fulfill({ json: { items: [entry(url.searchParams.has('ParentNodeId') ? child : url.searchParams.get('NodeId') ?? root)], totalCount: 1, page: Number(url.searchParams.get('Page')), pageSize: 3, hasNextPage: false } });
  });
  return urls;
}

for (const preview of ['excerpt', 'carousel', 'sections'] as const) {
  test(`compact feed actions stay on one row at phone width with a ${preview} preview`, async ({ page }) => {
    await page.setViewportSize({ width: 320, height: 800 });
    await mock(page);
    await page.route(`**/atlas-public-api/nodes/${root}/description`, route => route.fulfill({
      json: preview === 'excerpt' ? [] : preview === 'carousel'
        ? ['Opening', 'Continuation'].map((markdown, i) => ({ id: `block-${i}`, kind: 'markdown', content: { markdown } }))
        : [{ id: 'composition', kind: 'sectioned', content: { composition: writeComposition(example('editorial')) } }],
    }));
    await page.route('https://images.unsplash.com/**', route => route.abort());
    await page.goto('/');
    const card = page.locator('article.node-card').first();
    const toggle = card.getByRole('button', { name: 'Show subnodes' });
    const reactions = card.getByRole('button', { name: /^Reactions/ });
    const comments = card.getByRole('button', { name: 'Comments (1)', exact: true });
    const readMore = card.getByRole('button', { name: 'Read more', exact: true });
    await expect(comments.locator('svg')).toBeVisible();
    await expect(readMore.locator('svg')).toBeVisible();
    await expect(reactions).toContainText('Reactions');
    const row = await Promise.all([toggle, reactions, comments, readMore].map(action => action.boundingBox()));
    const bounds = (await card.boundingBox())!;
    for (const box of row) {
      expect(box).not.toBeNull();
      expect(box!.y + box!.height / 2).toBeCloseTo(row[0]!.y + row[0]!.height / 2, 0);
      expect(box!.x + box!.width).toBeLessThanOrEqual(bounds.x + bounds.width);
    }
    await toggle.click();
    await expect(card.getByRole('region', { name: 'Subnodes' })).toBeVisible();
    await readMore.click();
    await expect(page).toHaveURL(/\/nodes\?id=/);
  });
}

test('Read more opens the matching node story; filters load child feeds and a parented form', async ({ page }) => {
  const urls = await mock(page);
  await page.goto('http://127.0.0.1:6006/iframe.html?id=compositions-feed--playground&viewMode=story');
  await page.getByRole('button', { name: 'Read more', exact: true }).click();
  await expect(page).toHaveURL(/compositions-node-page/);
  await expect(page.getByRole('heading', { name: 'A live question' })).toBeVisible();
  await expect(page.locator('.node-page').getByRole('heading', { name: 'Full explanation' })).toBeVisible();
  await page.locator('.node-page').getByRole('button', { name: 'Comments (1)', exact: true }).click();
  await expect(page.getByText('A public comment', { exact: true })).toBeVisible();
  await expect(page.locator('.content-blocks script')).toHaveCount(0);
  expect(urls.some(url => url.searchParams.get('NodeId') === root)).toBe(true);
  await page.locator('.subnode-toolbar select').selectOption(idea);
  await expect(page.getByRole('heading', { name: 'A child idea' })).toBeVisible();
  expect(urls.some(url => url.searchParams.get('ParentNodeId') === root && url.searchParams.get('NodeTypeId') === idea)).toBe(true);
  await page.getByRole('button', { name: 'Add Idea', exact: true }).click();
  await expect(page.getByRole('form', { name: 'Create child node' })).toBeVisible();
  await expect(page.locator('select[data-node-type-select][aria-label="Node type"]')).toHaveValue(idea);
  await expect(page.getByRole('combobox', { name: 'Node type', exact: true })).toBeFocused();
  const form = page.getByRole('form', { name: 'Create child node' });
  await expect(page.locator('.external-subnodes').getByRole('form')).toHaveCount(1);
  await expect(form.getByRole('navigation')).toHaveCount(0);
  await expect.poll(async () => (await form.boundingBox())?.y).toBeGreaterThanOrEqual(0);
  await expect.poll(async () => (await form.boundingBox())?.y ?? Infinity).toBeLessThan(150);
  await expect(page.locator('.external-subnodes').locator('form')).toHaveCount(1);
  await expect(page.getByRole('button', { name: 'Create node', exact: true })).toBeEnabled();
});

test('root form previews a new type and never writes to the API', async ({ page }) => {
  const writes: string[] = []; page.on('request', request => { if (request.method() !== 'GET') writes.push(request.url()); });
  await mock(page);
  await page.goto('http://127.0.0.1:6006/iframe.html?id=components-node-form--playground&viewMode=story');
  await page.getByLabel('Title', { exact: true }).fill('A root idea');
  await page.locator('select[data-node-type-select][aria-label="Node type"]').selectOption('new', { force: true });
  await page.getByLabel('Type name', { exact: true }).fill('Observation');
  const content = page.getByRole('textbox', { name: /Node content/ }).first();
  await content.fill('Supporting content');
  await page.getByRole('button', { name: 'Preview in feed' }).click();
  await expect(page.getByRole('article', { name: 'A root idea' }).getByText('Supporting content', { exact: true })).toBeVisible();
  await page.getByRole('button', { name: 'Edit', exact: true }).click();
  await expect(content).toContainText('Supporting content');
  await expect(page.getByRole('button', { name: 'Preview draft' })).toHaveCount(0);
  // Creation drafts have no voting interaction.
  await expect(page.getByRole('spinbutton')).toHaveCount(0);
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

test('a published node stacks section columns in slot order on narrow screens', async ({ page }) => {
  await mock(page);
  await page.route('**/atlas-public-api/nodes/*/description', route => route.fulfill({
    json: [{ id: 'composition', kind: 'sectioned', content: { composition: writeComposition(example('editorial')) } }],
  }));
  await page.route('https://images.unsplash.com/**', route => route.abort());
  await page.setViewportSize({ width: 390, height: 844 });
  await page.goto(`http://127.0.0.1:4173/nodes?id=${root}`);
  const slots = page.locator('.post-body .sections > .section').first().locator(':scope > .slot');
  await expect(slots).toHaveCount(2);
  const first = (await slots.first().boundingBox())!;
  const second = (await slots.last().boundingBox())!;
  expect(second.x).toBeCloseTo(first.x);
  expect(second.y).toBeGreaterThanOrEqual(first.y + first.height);

  await page.setViewportSize({ width: 1280, height: 900 });
  const desktopFirst = (await slots.first().boundingBox())!;
  const desktopSecond = (await slots.last().boundingBox())!;
  expect(desktopSecond.x).toBeGreaterThan(desktopFirst.x);
  expect(desktopSecond.y).toBeCloseTo(desktopFirst.y);
});

for (const layout of ['grid', 'masonry'] as const) {
  test(`a published ${layout} fits its columns to the available width`, async ({ page }) => {
    const draft = example(layout);
    const gallery = draft.blocks.find(block => block.kind === 'gallery');
    if (gallery?.kind !== 'gallery') throw new Error('Expected a gallery in the demo draft');
    gallery.gridColumns = 4;
    gallery.masonryColumns = 4;
    await mock(page);
    await page.route('**/atlas-public-api/nodes/*/description', route => route.fulfill({
      json: [{ id: 'composition', kind: 'sectioned', content: { composition: writeComposition(draft) } }],
    }));
    await page.route('https://images.unsplash.com/**', route => route.abort());
    await page.setViewportSize({ width: 390, height: 844 });
    await page.goto(`http://127.0.0.1:4173/nodes?id=${root}`);
    const galleryLayout = page.locator(layout === 'grid' ? '.post-body .tiles' : '.post-body .masonry');
    const columnCount = () => galleryLayout.evaluate(element =>
      getComputedStyle(element).gridTemplateColumns.split(' ').length);
    await expect.poll(columnCount).toBe(2);
    await page.setViewportSize({ width: 1280, height: 900 });
    await expect.poll(columnCount).toBe(Math.min(4, gallery.photos.length));
  });
}

test('SPA Read more opens a node page and comments and child filters support navigation', async ({ page }) => {
  await mock(page);
  await page.setViewportSize({ width: 320, height: 720 });
  await page.goto('http://127.0.0.1:4173');
  await page.getByRole('button', { name: 'Read more', exact: true }).click();
  await expect(page).toHaveURL(new RegExp(`/nodes\\?id=${root}`));
  await expect(page.locator('.node-page').getByText('The complete content belongs here.', { exact: true })).toHaveCSS('white-space', 'normal');
  const comments = page.locator('.node-page').getByRole('button', { name: 'Comments (1)', exact: true });
  await comments.focus(); await page.keyboard.press('Enter');
  await expect(comments).toHaveAttribute('aria-expanded', 'true');
  const filter = page.locator('.subnode-toolbar select');
  await filter.selectOption(idea);
  await expect(filter).toHaveValue(idea);
  await expect(page.getByRole('heading', { name: 'A child idea' })).toBeVisible();
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(320);
});


test('visible nodes preload ancestry and reactions and reuse them when panels reopen', async ({ page }) => {
  const urls = await mock(page);
  let reactionReads = 0;
  await page.route('**/atlas-session-api/nodes/*/reactions', route => {
    reactionReads++;
    return route.fulfill({ json: [{ id: 'helpful', associationId: 'helpful', text: 'Helpful', emoji: '👍', score: 2, totalVotes: 2, currentParticipantVote: null }] });
  });
  await page.route('**/atlas-public-api/nodes?**', route => {
    const url = new URL(route.request().url());
    if (url.searchParams.get('NodeId') !== root) return route.fallback();
    const item = entry();
    return route.fulfill({ json: { items: [{ ...item, card: { ...item.card, breadcrumbSegment: {
      items: [{ nodeId: child, label: 'A child idea' }], continuationNodeId: null, hasAlternativeRoutes: false,
    } } }], totalCount: 1, page: 1, pageSize: 3, hasNextPage: false } });
  });
  await page.goto(`/nodes?id=${root}`);
  const card = page.locator('.node-page');
  const ancestry = card.getByRole('navigation', { name: 'Node ancestry' });
  const reactions = card.getByRole('button', { name: /^Reactions/ });
  const ancestorReads = () => urls.filter(url => url.searchParams.get('NodeId') === child).length;

  await expect(reactions).toBeVisible();
  await expect(ancestry).toBeHidden();
  await expect(card.locator('.reaction-controls')).toBeHidden();
  await expect.poll(ancestorReads).toBe(1);
  await expect.poll(() => reactionReads).toBeGreaterThan(0);

  await card.getByRole('button', { name: 'Show ancestry' }).click();
  await expect(ancestry.locator('.mini-node')).toContainText('A child idea');
  await card.getByRole('button', { name: 'Hide ancestry' }).click();
  await expect(ancestry).toBeHidden();
  await card.getByRole('button', { name: 'Show ancestry' }).click();
  await expect(ancestry.locator('.mini-node')).toContainText('A child idea');
  expect(ancestorReads()).toBe(1);

  await reactions.click();
  const panel = card.locator('.reaction-controls');
  await expect(panel.getByRole('button', { name: /Helpful/ })).toBeVisible();
  await reactions.click();
  await expect(panel).toBeHidden();
  const readsBeforeReopen = reactionReads;
  await reactions.click();
  await expect(panel.getByRole('button', { name: /Helpful/ })).toBeVisible();
  expect(reactionReads).toBe(readsBeforeReopen);
});

test('node page shows a sole parent in expandable ancestry and voting leaves content in place', async ({ page }) => {
  await mock(page);
  await page.route('**/atlas-session-api/nodes/*/reactions', route => route.fulfill({ json: [{ id: 'helpful', associationId: 'helpful', text: 'Helpful', emoji: '👍', score: 2, totalVotes: 2, currentParticipantVote: null }] }));
  await page.route('**/atlas-public-api/nodes/*/description', route => route.fulfill({ json: [{ id: 'body', kind: 'markdown', content: { markdown: 'Supporting detail. '.repeat(150) } }] }));
  await page.route('**/atlas-public-api/nodes?**', route => new URL(route.request().url()).searchParams.has('ParentNodeId') ? route.fallback() : route.fulfill({ json: { items: [{ ...entry(), card: { ...entry().card, breadcrumb: 'A parent question' } }], totalCount: 1, page: 1, pageSize: 3, hasNextPage: false } }));
  await page.goto('http://127.0.0.1:6006/iframe.html?id=compositions-node-page--playground&viewMode=story');
  const card = page.locator('.node-page');
  await expect(card.getByRole('heading', { name: 'A live question' })).toBeVisible();
  const ancestry = card.getByRole('navigation', { name: 'Node ancestry' });
  await expect(card.locator('.connected-ancestry')).toBeHidden();
  await card.getByRole('button', { name: 'Show ancestry' }).click();
  await expect(ancestry.getByRole('listitem')).toHaveText('A parent question');
  await card.getByRole('button', { name: 'Hide ancestry' }).click();
  for (const width of [800, 320]) {
    await page.setViewportSize({ width, height: 900 });
    const body = card.locator('.post-body');
    await expect(body).toContainText('Supporting detail.');
    const before = await body.boundingBox();
    await card.getByRole('button', { name: /Rate node importance/ }).click();
    const picker = card.getByRole('dialog', { name: 'Rate node importance', exact: true });
    await expect(picker.getByRole('spinbutton')).toBeVisible();
    expect(await body.boundingBox()).toEqual(before);
    await picker.getByRole('button', { name: 'Close vote dial' }).click();
    await expect(picker).toBeHidden();
    const geometry = await card.evaluate(element => ({
      contentBottom: element.querySelector('.post-body')!.getBoundingClientRect().bottom,
      actionsTop: element.querySelector('.node-actions')!.getBoundingClientRect().top,
      cardBottom: element.getBoundingClientRect().bottom,
      discussionTop: element.parentElement!.querySelector('.external-subnodes')!.getBoundingClientRect().top,
    }));
    expect(geometry.actionsTop).toBeGreaterThanOrEqual(geometry.contentBottom);
    expect(geometry.discussionTop).toBeGreaterThanOrEqual(geometry.cardBottom - 1);
    expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(width);
  }
});


for (const destination of ['storybook', 'spa']) {
  for (const contribution of [{ typeId: idea, label: '1 Idea', tab: '1 Idea' }, { typeId: evidence, label: '0 Evidence', tab: '0 Evidence' }]) {
    test(`${destination} contribution filter opens and reveals ${contribution.label}`, async ({ page }) => {
      const urls = await mock(page);
      if (contribution.typeId === evidence) {
        await page.route('**/atlas-public-api/nodes?**', async route => {
          const url = new URL(route.request().url());
          if (!url.searchParams.has('ParentNodeId')) return route.fallback();
          urls.push(url);
          return route.fulfill({ json: { items: [], totalCount: 0, page: 1, pageSize: 3, hasNextPage: false } });
        });
      }
      await page.goto(destination === 'storybook'
        ? 'http://127.0.0.1:6006/iframe.html?id=compositions-feed--playground&viewMode=story'
        : 'http://127.0.0.1:4173');
      const parentCard = page.locator('article.node-card').first();
      const toggle = parentCard.getByRole('button', { name: 'Show subnodes' });
      await toggle.click();
      const inline = parentCard.getByRole('region', { name: 'Subnodes' });
      await expect(inline).toBeVisible();
      await inline.getByRole('combobox').selectOption(contribution.typeId);
      if (contribution.typeId === idea) {
        await expect(inline.locator('.mini-node')).toHaveCount(1);
        await expect(inline.getByRole('link', { name: 'A child idea' })).toBeVisible();
      } else {
        await expect(inline.getByRole('status')).toContainText('No nodes found.');
        await expect(inline.locator('.mini-node')).toHaveCount(0);
      }
      await parentCard.getByRole('button', { name: 'Hide subnodes' }).click();
      await expect(inline).toBeHidden();
      await parentCard.getByRole('button', { name: 'Read More', exact: false }).click();
      await expect(page).toHaveURL(destination === 'storybook' ? /compositions-node-page/ : /nodes\?id=/);
      const tabs = page.locator('.subnode-toolbar');
      const filter = tabs.locator('select');
      await filter.selectOption(contribution.typeId);
      await expect(filter).toHaveValue(contribution.typeId);
      await expect.poll(() => urls.some(url => url.searchParams.get('ParentNodeId') === root && url.searchParams.get('NodeTypeId') === contribution.typeId)).toBe(true);
      if (contribution.typeId === evidence) {
        // Empty feeds keep natural height rather than reserving a viewport of blank space.
        await expect(page.locator('.external-subnodes section.node-feed')).toHaveCSS('min-height', '0px');
        await expect(page.getByRole('heading', { name: 'Be the first to contribute', exact: true })).toBeVisible();
        await expect(page.locator('.empty-invitation').getByRole('button', { name: 'Add Evidence', exact: true })).toBeEnabled();
      }
      else await expect(page.getByRole('heading', { name: 'A child idea' })).toBeVisible();
    });
  }
}


test('node-page filters use count-aware labels for only requested and populated contribution types', async ({ page }) => {
  await mock(page);
  const question = 'a1000000-0000-4000-8000-000000000003';
  const goal = 'a1000000-0000-4000-8000-000000000004';
  await page.route('**/atlas-public-api/catalog/node-types', route => route.fulfill({ json: [...types, { id: question, name: 'Question', pluralName: 'Questions', autoPluralize: true }, { id: goal, name: 'Goal', pluralName: 'Goals', autoPluralize: true }] }));
  await page.route(`**/atlas-public-api/nodes/${root}`, route => route.fulfill({ json: { id: root, requestedTypeIds: [question, evidence], status: 'Active' } }));
  await page.route('**/atlas-public-api/nodes?**', route => new URL(route.request().url()).searchParams.has('ParentNodeId') ? route.fallback() : route.fulfill({ json: { items: [{ ...entry(), card: { ...entry().card, childTypes: [{ typeId: evidence, label: '0 Evidence', count: 0 }, { typeId: idea, label: '2 Ideas', count: 2 }, { typeId: question, label: '0 Questions', count: 0 }] } }], totalCount: 1, page: 1, pageSize: 3, hasNextPage: false } }));
  await page.goto('http://127.0.0.1:6006/iframe.html?id=compositions-node-page--playground&viewMode=story');
  const filter = page.locator('.subnode-toolbar select');
  await expect(filter.getByRole('option')).toHaveText(['All types', '0 Evidence', '2 Ideas', '0 Questions']);
  await expect(filter.getByRole('option', { name: /Goals?/ })).toHaveCount(0);
  await filter.selectOption(question);
  await expect(filter).toHaveValue(question);
});

import AxeBuilder from '@axe-core/playwright';
import { expect, test } from '@playwright/test';

const preview = 'http://127.0.0.1:6006/iframe.html?id=compositions-atlas-node--playground&viewMode=story';
const item = (id: string, title: string, hidden = false) => ({
  nodeId: id, rank: 1, rankingScore: 1, voteCount: 12, averageVote: 7.5,
  card: { nodeId: id, title, typeName: 'Question', authorName: 'API author', status: 'Active', createdAt: '2026-09-28T15:20:00Z', description: 'Content from the API response.', breadcrumb: 'Root question', subNodes: 'Ideas: 2', reactions: 'Useful: 3', communities: 'Local community', commentCount: 4, isHidden: hidden },
});

test('live nodes support selection, search, paging and refresh without submitting votes', async ({ page }) => {
  const requests: { page: string | null; search: string | null; method: string }[] = [];
  await page.route('**/atlas-preview-api/**', async route => {
    const request = route.request();
    const url = new URL(request.url());
    requests.push({ page: url.searchParams.get('Page'), search: url.searchParams.get('SearchText'), method: request.method() });
    const second = url.searchParams.get('Page') === '2';
    await route.fulfill({ json: { items: second ? [item('new', 'A newly entered node')] : [item('one', 'Database question'), item('two', 'Database second node'), item('hidden', 'Hidden private title', true)], totalCount: 4, page: second ? 2 : 1, pageSize: 3, hasNextPage: !second } });
  });
  await page.route('**/atlas-preview-api/nodes/*/reactions', route => route.fulfill({ json: [
    { associationId: 'applied', text: 'Useful', emoji: '💡', totalVotes: 3 },
    { associationId: null, text: 'Unused', emoji: '', totalVotes: 0 },
  ] }));
  await page.goto(preview);
  await expect(page.getByRole('heading', { name: 'Question: Database question', exact: true })).toBeVisible();
  await expect(page.locator('.average')).toHaveText('7.5');
  await expect(page.locator('.vote-count')).toHaveAccessibleName('12 votes');
  await expect(page.locator('.node-card')).toContainText('Local community');
  await expect(page.getByRole('option', { name: /Hidden private title/ })).toHaveCount(0);
  await page.getByLabel('Node on this page').selectOption('two');
  await expect(page.getByRole('heading', { name: 'Question: Database second node' })).toBeVisible();
  await page.getByRole('spinbutton').focus();
  await page.keyboard.press('End');
  await expect(page.getByRole('status')).toHaveText('Local preview selection: 10 out of 10. Not submitted.');
  await expect(page.getByRole('status')).toHaveText('No local rating selected');
  await expect(page.getByRole('spinbutton')).toHaveAttribute('aria-valuetext', /Unrated/);
  await expect.poll(() => page.getByRole('spinbutton').evaluate(el => el.scrollTop)).toBe(220);
  await page.getByRole('button', { name: 'Next page' }).click();
  await expect(page.getByRole('heading', { name: 'Question: A newly entered node' })).toBeVisible();
  await page.getByRole('button', { name: 'Previous page' }).click();
  await page.getByLabel('Search nodes').fill('housing');
  await page.getByRole('button', { name: 'Search', exact: true }).click();
  await expect.poll(() => requests.at(-1)?.search).toBe('housing');
  await expect(page.getByRole('button', { name: 'Refresh' })).toBeEnabled();
  const count = requests.length;
  await page.getByRole('button', { name: 'Refresh' }).click();
  await expect.poll(() => requests.length).toBe(count + 1);
  expect(requests.every(request => request.method === 'GET')).toBe(true);
});

test('live preview handles empty results and API failure with retry', async ({ page }) => {
  let fail = true;
  await page.route('**/atlas-preview-api/**', route => fail
    ? route.fulfill({ status: 503, body: 'Unavailable' })
    : route.fulfill({ json: { items: [], totalCount: 0, page: 1, pageSize: 3, hasNextPage: false } }));
  await page.route('**/atlas-preview-api/nodes/*/reactions', route => route.fulfill({ json: [
    { associationId: 'applied', text: 'Useful', emoji: '💡', totalVotes: 3 },
    { associationId: null, text: 'Unused', emoji: '', totalVotes: 0 },
  ] }));
  await page.goto(preview);
  await expect(page.getByRole('alert')).toContainText('HTTP 503');
  fail = false;
  await page.getByRole('button', { name: 'Retry' }).click();
  await expect(page.getByRole('status')).toHaveText('No displayable nodes on this page. Try another search or page.');
});

test('fixture stories never contact API and dev proxy rejects writes', async ({ page, request }) => {
  let contacted = false;
  await page.route('**/atlas-preview-api/**', route => { contacted = true; return route.abort(); });
  await page.goto('http://127.0.0.1:6006/iframe.html?id=components-votingdial--playground&viewMode=story');
  await page.getByRole('spinbutton').focus();
  await page.keyboard.press('End');
  await expect(page.getByRole('status')).toHaveText('Preview selection: 10 out of 10');
  expect(contacted).toBe(false);
  const response = await request.post('http://127.0.0.1:6007/atlas-preview-api/nodes');
  expect(response.status()).toBe(405);
});


test('uncast fixture selections reset to neutral and accepted simulations stay selected', async ({ page }) => {
  for (const outcome of ['anonymous', 'failed']) {
    await page.goto(`http://127.0.0.1:6006/iframe.html?id=components-votingdial--playground&viewMode=story&args=initialValue:!null;voteOutcome:${outcome}`);
    const dial = page.getByRole('spinbutton');
    await dial.focus();
    await page.keyboard.press('End');
    await expect(dial).toHaveAttribute('aria-valuenow', '10');
    await expect(page.getByRole('status')).toHaveText('No rating selected');
    await expect(dial).toHaveAttribute('aria-valuetext', /Unrated/);
    await expect.poll(() => dial.evaluate(el => el.scrollTop)).toBe(220);
    await expect(page.locator('.selector')).toHaveClass(/unrated/);
    await expect(page.getByRole('note')).toContainText(outcome === 'anonymous' ? 'Sign in' : 'could not be saved');
  }
  await page.goto('http://127.0.0.1:6006/iframe.html?id=components-votingdial--playground&viewMode=story&args=voteOutcome:accepted');
  await page.getByRole('spinbutton').focus();
  await page.keyboard.press('Home');
  await page.waitForTimeout(800);
  await expect(page.getByRole('spinbutton')).toHaveAttribute('aria-valuenow', '0');
});

test('API breadcrumb prepends older pages, retries failures and stops at root', async ({ page, request }) => {
  const ids = Array.from({ length: 5 }, (_, i) => `b1000000-0000-4000-8000-${String(i + 1).padStart(12, '0')}`);
  let failed = false;
  let calls = 0;
  await page.route('**/atlas-preview-api/**', async route => {
    if (route.request().url().includes('/ancestors')) {
      calls++;
      if (!failed) { failed = true; return route.fulfill({ status: 503 }); }
      return route.fulfill({ json: { items: [{ id: ids[0], title: 'Oldest ancestor' }], hasMore: false } });
    }
    const entry = item(ids[4], 'Paged breadcrumb node');
    await route.fulfill({ json: { items: [{ ...entry, card: { ...entry.card, breadcrumbSegment: {
      items: [{ nodeId: ids[3], label: 'Nearest ancestor' }, { nodeId: ids[2], label: 'Middle ancestor' }, { nodeId: ids[1], label: 'Older ancestor' }],
      continuationNodeId: ids[1], hasAlternativeRoutes: false, cycleDetected: false,
    } } }], totalCount: 1, page: 1, pageSize: 3, hasNextPage: false } });
  });
  await page.route('**/atlas-preview-api/nodes/*/reactions', route => route.fulfill({ json: [
    { associationId: 'applied', text: 'Useful', emoji: '💡', totalVotes: 3 },
    { associationId: null, text: 'Unused', emoji: '', totalVotes: 0 },
  ] }));
  await page.goto(preview);
  const nav = page.getByRole('navigation', { name: 'Node breadcrumb' });
  await nav.getByRole('button', { name: 'Load older ancestors' }).click();
  await expect(nav.getByRole('alert')).toContainText('HTTP 503');
  await nav.getByRole('button', { name: 'Retry' }).click();
  await expect(nav).toContainText('Oldest ancestor');
  await expect(nav.getByRole('button', { name: 'Load older ancestors' })).toHaveCount(0);
  expect(calls).toBe(2);
  expect(await nav.locator('li').allTextContents()).toEqual(['Oldest ancestor', 'Older ancestor', 'Middle ancestor', 'Nearest ancestor']);
  const blocked = await request.get('http://127.0.0.1:6007/atlas-preview-api/nodes/' + ids[0] + '/votes');
  expect(blocked.status()).toBe(404);
  const write = await request.post('http://127.0.0.1:6007/atlas-preview-api/nodes/' + ids[0] + '/ancestors');
  expect(write.status()).toBe(405);
});


test('live-only card navigates an off-page ancestor by ID and keeps both presentations', async ({ page }) => {
  const nodeId = 'b1000000-0000-4000-8000-000000000010';
  const ancestor = 'b1000000-0000-4000-8000-000000000001';
  const requested: URL[] = [];
  await page.route('**/atlas-preview-api/**', async route => {
    const url = new URL(route.request().url());
    requested.push(url);
    const detail = url.searchParams.get('NodeId') === ancestor;
    const entry = item(detail ? ancestor : nodeId, detail ? 'Ancestor outside search' : 'Current child');
    await route.fulfill({ json: { items: [{ ...entry, card: { ...entry.card,
      description: 'Actual API description. '.repeat(80),
      breadcrumbSegment: { items: detail ? [] : [{ nodeId: ancestor, label: 'Ancestor outside search' }], continuationNodeId: null, hasAlternativeRoutes: false, cycleDetected: false },
    } }], totalCount: 1, page: 1, pageSize: 3, hasNextPage: false } });
  });
  await page.setViewportSize({ width: 320, height: 900 });
  await page.route('**/atlas-preview-api/nodes/*/reactions', route => route.fulfill({ json: [
    { associationId: 'applied', text: 'Useful', emoji: '💡', totalVotes: 3 },
    { associationId: null, text: 'Unused', emoji: '', totalVotes: 0 },
  ] }));
  await page.goto(preview);
  const nav = page.getByRole('navigation', { name: 'Node breadcrumb' });
  await expect(nav).toBeVisible();
  const positions = await page.locator('article').evaluate(el => ({
    left: el.getBoundingClientRect().left + el.clientLeft,
    ribbon: el.querySelector('nav')!.getBoundingClientRect().left,
    scrollbar: getComputedStyle(el.querySelector('.viewport')!).scrollbarWidth,
  }));
  expect(Math.abs(positions.left - positions.ribbon)).toBeLessThan(1);
  expect(positions.scrollbar).toBe('none');
  await expect(page.locator('article footer')).toHaveCount(0);
  await expect(page.locator('article .body .reactions strong')).toHaveText('3');
  await expect(page.locator('article .body .reactions span')).toHaveCSS('border-radius', '999px');
  await expect(page.locator('article .body .navigation-details')).toContainText('Ideas: 2');
  await expect(page.locator('article .body .navigation-details')).toContainText('4 Comments');
  await expect(page.locator('.navigation-details .navigation-separator')).toHaveText('·');
  await expect(page.locator('article .body .navigation-details')).not.toContainText('Connected nodes:');
  const navigationStyle = await page.locator('.navigation-details').evaluate(el => {
    const comment = el.querySelector('button')!; const connection = el.querySelector('.connection-summary')!;
    return { sameLine: Math.abs(comment.getBoundingClientRect().top - connection.getBoundingClientRect().top) < 4, commentFont: getComputedStyle(comment).fontSize, connectionFont: getComputedStyle(connection).fontSize };
  });
  expect(navigationStyle.commentFont).toBe(navigationStyle.connectionFont);
  await expect(page.locator('article h2')).toHaveText('Question: Current child');
  await expect(page.locator('article header')).not.toContainText('Local community');
  await expect(page.getByRole('group', { name: 'Communities', exact: true })).toHaveText('Community: Local community');
  await page.getByRole('button', { name: 'Read more' }).click();
  await expect(page.getByRole('button', { name: 'Show less' })).toHaveAttribute('aria-expanded', 'true');
  const results = await new AxeBuilder({ page }).include('#storybook-root').analyze();
  expect(results.violations).toEqual([]);
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(320);
  await nav.getByRole('button', { name: 'Ancestor outside search', exact: true }).click();
  await expect(page.getByRole('heading', { name: 'Question: Ancestor outside search', exact: true })).toBeVisible();
  expect(requested.at(-1)?.searchParams.get('NodeId')).toBe(ancestor);
  expect(requested.at(-1)?.searchParams.get('SearchText')).toBeNull();
  expect(requested.at(-1)?.searchParams.get('IncludeArchived')).toBe('true');
  await expect(page.getByRole('button', { name: 'Read more' })).toHaveAttribute('aria-expanded', 'false');
  await expect(nav).toHaveCount(0);
  await page.getByRole('button', { name: 'Back to Discovery' }).click();
  await expect(page.getByRole('heading', { name: 'Question: Current child', exact: true })).toBeVisible();
  await page.goto(`${preview}&args=presentation:context-below`);
  await expect(page.locator('article')).toHaveAttribute('data-presentation', 'context-below');
  await expect(page.getByRole('heading', { name: 'Current child', exact: true })).toBeVisible();
  await page.getByRole('button', { name: 'Read more' }).click();
  await expect(page.getByRole('button', { name: 'Show less' })).toBeVisible();
});

test('unavailable ancestor shows an error and can return to Discovery', async ({ page }) => {
  let fail = false;
  await page.route('**/atlas-preview-api/**', route => {
    const entry = item('child', 'Visible child');
    return route.fulfill({ json: { items: fail ? [] : [{ ...entry, card: { ...entry.card, breadcrumbSegment: {
      items: [{ nodeId: 'b1000000-0000-4000-8000-000000000001', label: 'Parent' }], continuationNodeId: null,
    } } }], totalCount: fail ? 0 : 1, page: 1, pageSize: 3, hasNextPage: false } });
  });
  await page.route('**/atlas-preview-api/nodes/*/reactions', route => route.fulfill({ json: [
    { associationId: 'applied', text: 'Useful', emoji: '💡', totalVotes: 3 },
    { associationId: null, text: 'Unused', emoji: '', totalVotes: 0 },
  ] }));
  await page.goto(preview);
  await expect(page.getByRole('heading', { name: 'Question: Visible child' })).toBeVisible();
  fail = true;
  await page.getByRole('navigation').getByRole('button', { name: 'Parent', exact: true }).click();
  await expect(page.getByRole('alert')).toContainText('unavailable or hidden');
  fail = false;
  await page.getByRole('button', { name: 'Retry' }).click();
  await expect(page.getByRole('heading', { name: 'Question: Visible child' })).toBeVisible();
});


test('reaction pills are separate and absent reactions leave no container or gap', async ({ page }) => {
  let empty = false;
  await page.route('**/atlas-preview-api/**', route => {
    if (route.request().url().endsWith('/reactions')) return route.fulfill({ json: empty ? [] : [
      { associationId: 'one', text: 'Useful', emoji: '💡', totalVotes: 3 },
      { associationId: 'two', text: 'Worth exploring', emoji: '🔎', totalVotes: 2 },
    ] });
    return route.fulfill({ json: { items: [item('b1000000-0000-4000-8000-000000000001', 'Compact node')], totalCount: 1, page: 1, pageSize: 3, hasNextPage: false } });
  });
  await page.goto(preview);
  const pills = page.locator('article .reaction-pill');
  await expect(pills).toHaveCount(2);
  await expect(pills).toHaveText(['💡 Useful 3', '🔎 Worth exploring 2']);
  const navigation = page.locator('.navigation-details');
  const sameLine = await navigation.evaluate(el => {
    const comment = el.querySelector('button')!.getBoundingClientRect();
    const connection = el.querySelector('.connection-summary')!.getBoundingClientRect();
    return Math.abs(comment.top - connection.top) < 4;
  });
  expect(sameLine).toBe(true);
  empty = true;
  await page.getByRole('button', { name: 'Refresh', exact: true }).click();
  await expect(page.getByRole('group', { name: 'Community reactions' })).toHaveCount(0);
  await expect(pills).toHaveCount(0);
  // The community is the first detail, with no reaction wrapper to reserve a row.
  await expect(page.locator('.card-details > :first-child')).toHaveClass(/\bcommunities\b/);
  await expect(page.locator('article')).not.toContainText('Useful: 3');
});

test('standalone card versions preserve mobile layout and offer a raised desktop dial', async ({ page }) => {
  await page.route('**/atlas-preview-api/**', route => {
    if (route.request().url().endsWith('/reactions')) return route.fulfill({ json: [] });
    const entry = item('b1000000-0000-4000-8000-000000000001', 'Layout comparison');
    return route.fulfill({ json: { items: [{ ...entry, card: { ...entry.card, description: 'A concise introduction for comparing card arrangements.' } }], totalCount: 1, page: 1, pageSize: 3, hasNextPage: false } });
  });
  await page.setViewportSize({ width: 1024, height: 1000 });
  await page.goto(`${preview}&args=presentation:ribbon`);
  await expect(page.locator('article')).toHaveAttribute('data-presentation', 'ribbon');
  const ribbonHeight = await page.locator('article').evaluate(el => el.getBoundingClientRect().height);
  await page.goto(`${preview}&args=presentation:ribbon-responsive`);
  await expect(page.locator('article')).toHaveAttribute('data-presentation', 'ribbon-responsive');
  const desktop = await page.locator('article').evaluate(el => ({
    header: el.querySelector('header')!.getBoundingClientRect().toJSON(),
    dial: el.querySelector('.voting-dial')!.getBoundingClientRect().toJSON(),
    selector: el.querySelector('.selector')!.getBoundingClientRect().toJSON(),
    title: el.querySelector('h2')!.getBoundingClientRect().toJSON(),
    paragraph: el.querySelector('.description')!.getBoundingClientRect().toJSON(),
    breadcrumbBottom: el.querySelector('nav li')!.getBoundingClientRect().bottom,
    padding: parseFloat(getComputedStyle(el.querySelector('header')!).paddingRight),
    height: el.getBoundingClientRect().height,
  }));
  expect(desktop.dial.top).toBeLessThan(desktop.header.bottom);
  expect(desktop.header.left).toBeGreaterThan(desktop.dial.left);
  // Preserve the float layout's 16px margin + 6.4px inner dial padding.
  expect(desktop.title.left - desktop.selector.right).toBeCloseTo(22.4, 0);
  expect(desktop.paragraph.left - desktop.selector.right).toBeCloseTo(22.4, 0);
  expect(desktop.title.top - desktop.breadcrumbBottom).toBeCloseTo(desktop.padding, 0);
  expect(desktop.height).toBeLessThan(ribbonHeight);
  await page.setViewportSize({ width: 320, height: 1000 });
  const mobile = await page.locator('article').evaluate(el => ({
    headerBottom: el.querySelector('header')!.getBoundingClientRect().bottom,
    dialTop: el.querySelector('.dial')!.getBoundingClientRect().top,
  }));
  expect(mobile.dialTop).toBeGreaterThanOrEqual(mobile.headerBottom);
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(320);
  await page.getByRole('spinbutton').focus();
  await page.keyboard.press('End');
  await expect(page.getByRole('spinbutton')).toHaveAttribute('aria-valuenow', '10');
  await expect(page.getByRole('status')).toHaveText('No local rating selected');
  await page.goto(`${preview}&args=presentation:context-below`);
  await expect(page.locator('article')).toHaveAttribute('data-presentation', 'context-below');
  await expect(page.getByRole('heading', { name: 'Layout comparison', exact: true })).toBeVisible();
});

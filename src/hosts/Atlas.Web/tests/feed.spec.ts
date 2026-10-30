import { expect, test, type Page } from '@playwright/test';
const feedStory = 'http://127.0.0.1:6006/iframe.html?id=compositions-feed--playground&viewMode=story';
const entry = (number: number) => ({ nodeId: `b1000000-0000-4000-8000-${String(number).padStart(12, '0')}`, averageVote: null, voteCount: 0, card: {
  title: `Feed question ${number}`, shortDescription: 'A concise introduction to this connected question.', typeName: 'Question', authorName: 'Reader', status: 'Active', createdAt: '2026-10-21T12:00:00Z', breadcrumb: '', subNodes: '2 Ideas', communities: 'Building Atlas', commentCount: 0, reactions: '', isHidden: false,
} });
async function mockFeed(page: Page, failSecond = false) {
  const requested: number[] = [];
  let fail = failSecond;
  await page.route('**/atlas-public-api/**', route => {
    const url = new URL(route.request().url());
    if (url.pathname.includes('/communities/') || url.pathname.endsWith('/parents')) return route.fulfill({ json: [] });
    if (url.pathname.endsWith('/reactions')) return route.fulfill({ json: [] });
    const current = Number(url.searchParams.get('Page'));
    requested.push(current);
    if (current === 2 && fail) { fail = false; return route.fulfill({ status: 503 }); }
    const items = current === 1 ? [entry(1), entry(2), entry(3)] : [entry(3), entry(4), { ...entry(5), card: { ...entry(5).card, isHidden: true, title: '[Hidden by moderator: Spam]' } }, entry(6)];
    return route.fulfill({ json: { items, totalCount: 6, page: current, pageSize: 3, hasNextPage: current === 1 } });
  });
  return requested;
}

test('isolated feed appends on scrolling, deduplicates, hides excluded cards and stops at the end', async ({ page }) => {
  await page.setViewportSize({ width: 700, height: 400 });
  const requests = await mockFeed(page);
  await page.goto(feedStory);
  await expect(page.locator('article')).toHaveCount(3);
  await page.locator('.feed-tail').scrollIntoViewIfNeeded();
  await expect(page.locator('article')).toHaveCount(5);
  await expect(page.getByRole('status')).toContainText('caught up');
  expect(requests).toEqual([1, 2]);
  await expect(page.locator('article').first()).toContainText('Feed question 1');
  await expect(page.locator('article').last()).toContainText('Feed question 6');
  await expect(page.getByText('[Hidden by moderator: Spam]')).toHaveCount(0);
  await expect(page.getByRole('button', { name: 'Load more nodes' })).toHaveCount(0);
});

test('failed paging retains cards and keyboard retry requests the same page', async ({ page }) => {
  const requests = await mockFeed(page, true);
  await page.goto(feedStory);
  await expect(page.locator('article')).toHaveCount(3);
  await page.locator('.feed-tail').scrollIntoViewIfNeeded();
  await expect(page.getByRole('alert')).toContainText('Could not load more nodes');
  await expect(page.locator('article')).toHaveCount(3);
  const retry = page.getByRole('button', { name: 'Retry loading nodes' });
  await retry.focus(); await page.keyboard.press('Enter');
  await expect(page.locator('article')).toHaveCount(5);
  expect(requests).toEqual([1, 2, 2]);
});

test('SPA composes the shared shell and live feed at narrow widths', async ({ page }) => {
  await mockFeed(page);
  await page.setViewportSize({ width: 320, height: 640 });
  await page.goto('http://127.0.0.1:4173');
  await expect(page.getByRole('heading', { name: 'Discover', exact: true })).toBeVisible();
  await expect(page.getByRole('banner')).toContainText('Atlas');
  await expect(page.locator('article')).toHaveCount(3);
  await expect(page.getByRole('contentinfo')).toContainText('The Public Think Tank');
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(320);
  await page.getByRole('link', { name: 'Skip to content' }).focus();
  await page.keyboard.press('Enter');
  await expect(page).toHaveURL(/#main-content$/);
});

test('header and footer each have a separate isolated Playground', async ({ page }) => {
  await page.setViewportSize({ width: 320, height: 640 });
  await page.goto('http://127.0.0.1:6006/iframe.html?id=layout-header--playground&viewMode=story');
  await page.getByLabel('Open navigation menu').click();
  await expect(page.getByRole('navigation', { name: 'Mobile navigation' })).toBeVisible();
  await expect(page.getByRole('navigation', { name: 'Mobile navigation' }).getByRole('link', { name: 'Discover', exact: true })).toHaveAttribute('href', '/');
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(320);
  await page.goto('http://127.0.0.1:6006/iframe.html?id=layout-footer--playground&viewMode=story');
  await expect(page.getByRole('navigation', { name: 'Explore' })).toBeVisible();
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(320);
});

test('manual pagination works without IntersectionObserver', async ({ page }) => {
  await page.addInitScript(() => { Object.defineProperty(window, 'IntersectionObserver', { value: undefined }); });
  const requests = await mockFeed(page);
  await page.goto(feedStory);
  await expect(page.locator('article')).toHaveCount(3);
  const more = page.getByRole('button', { name: 'Load more nodes' });
  await expect(more).toBeEnabled();
  expect(requests).toEqual([1]);
  await more.focus(); await page.keyboard.press('Enter');
  await expect(page.locator('article')).toHaveCount(5);
});

test('empty feed stops pagination and exposes its empty state', async ({ page }) => {
  let requests = 0;
  await page.route('**/atlas-public-api/**', route => { requests++; return route.fulfill({ json: { items: [], totalCount: 0, page: 1, pageSize: 3, hasNextPage: false } }); });
  await page.goto(feedStory);
  await expect(page.getByRole('status')).toHaveText('No nodes found.');
  await expect(page.locator('article')).toHaveCount(0);
  await expect(page.getByRole('button', { name: 'Load more nodes' })).toHaveCount(0);
  expect(requests).toBe(1);
});


test.beforeEach(async ({ page }) => {
  await page.route('**/atlas-session-api/**', route => {
    const path = new URL(route.request().url()).pathname;
    if (path.endsWith('/reactions')) return route.fulfill({ json: [] });
    if (path.endsWith('/votes')) return route.fulfill({ json: { voteCount: 0, averageVote: null, currentParticipantVote: null } });
    if (path.endsWith('/auth/csrf')) return route.fulfill({ json: { token: 'fixture-token' } });
    return route.fulfill({ status: 401, json: {} });
  });
});

/** Parent/type filters must survive all five pages; each response adds exactly three cards. */
test('node contribution tab pages fifteen children in scoped groups of three', async ({ page }) => {
  // Manual loading makes page boundaries deterministic instead of viewport-dependent.
  await page.addInitScript(() => { Object.defineProperty(window, 'IntersectionObserver', { value: undefined }); });
  const parent = 'b1000000-0000-4000-8000-000000000201';
  const solution = '60d8ba45-8b78-4593-838b-ffd15af69746';
  const pages: number[] = [];
  await page.route('**/atlas-public-api/**', route => {
    const url = new URL(route.request().url());
    if (url.pathname.endsWith('/catalog/node-types')) return route.fulfill({ json: [{ id: solution, name: 'Solution', pluralName: 'Solutions' }] });
    if (url.pathname.endsWith('/description') || url.pathname.endsWith('/reactions') || url.pathname.includes('/communities/')) return route.fulfill({ json: [] });
    if (url.pathname.endsWith('/comments')) return route.fulfill({ json: { items: [], next: null } });
    if (url.pathname.endsWith('/nodes/' + parent)) return route.fulfill({ json: { id: parent, requestedTypeIds: [solution] } });
    if (url.searchParams.has('ParentNodeId')) {
      expect(url.searchParams.get('ParentNodeId')).toBe(parent);
      expect(url.searchParams.get('NodeTypeId')).toBe(solution);
      const current = Number(url.searchParams.get('Page'));
      pages.push(current);
      const items = Array.from({ length: 3 }, (_, i) => {
        const item = entry(400 + (current - 1) * 3 + i + 1);
        return { ...item, card: { ...item.card, typeName: 'Solution' } };
      });
      return route.fulfill({ json: { items, totalCount: 15, page: current, pageSize: 3, hasNextPage: current < 5 } });
    }
    const item = entry(201);
    return route.fulfill({ json: { items: [{ ...item, nodeId: parent, card: { ...item.card, title: 'Orca recovery', childTypes: [{ typeId: solution, count: 15, label: '15 Solutions' }] } }], totalCount: 1, page: 1, pageSize: 3, hasNextPage: false } });
  });
  await page.goto('http://127.0.0.1:6006/iframe.html?id=compositions-node-page--playground&viewMode=story&args=nodeId:' + parent);
  await page.getByRole('tab', { name: '15 Solutions', exact: true }).click();
  const panel = page.getByRole('tabpanel');
  await expect(panel.locator('article')).toHaveCount(3);
  for (let current = 2; current <= 5; current++) {
    await panel.getByRole('button', { name: 'Load more nodes', exact: true }).click();
    await expect(panel.locator('article')).toHaveCount(current * 3);
  }
  await expect(panel.getByRole('status')).toContainText('caught up');
  await expect(panel.getByRole('button', { name: 'Load more nodes', exact: true })).toHaveCount(0);
  expect(pages).toEqual([1, 2, 3, 4, 5]);
  await page.getByRole('tab', { name: '0 Comments', exact: true }).click();
  await page.getByRole('tab', { name: '15 Solutions', exact: true }).click();
  await expect(panel.locator('article')).toHaveCount(3);
  expect(pages).toEqual([1, 2, 3, 4, 5, 1]);
});

import { expect, test, type Page } from '@playwright/test';
const feedStory = 'http://127.0.0.1:6006/iframe.html?id=compositions-feed--playground&viewMode=story';
const entry = (number: number) => ({ nodeId: `b1000000-0000-4000-8000-${String(number).padStart(12, '0')}`, averageVote: null, voteCount: 0, card: {
  title: `Feed question ${number}`, shortDescription: 'A concise introduction to this connected question.', typeName: 'Question', authorName: 'Reader', status: 'Active', createdAt: '2026-10-21T12:00:00Z', breadcrumb: '', subNodes: '2 Ideas', communities: 'Building Atlas', commentCount: 0, reactions: '', isHidden: false,
} });
async function mockFeed(page: Page, failSecond = false) {
  const requested: number[] = [];
  let fail = failSecond;
  await page.route('**/atlas-preview-api/**', route => {
    const url = new URL(route.request().url());
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
  await expect(page.getByRole('navigation', { name: 'Main navigation' })).toBeVisible();
  await expect(page.getByRole('link', { name: 'Discover', exact: true })).toHaveAttribute('aria-current', 'page');
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(320);
  await page.goto('http://127.0.0.1:6006/iframe.html?id=layout-footer--playground&viewMode=story');
  await expect(page.getByRole('navigation', { name: 'Footer navigation' })).toBeVisible();
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
  await page.route('**/atlas-preview-api/**', route => { requests++; return route.fulfill({ json: { items: [], totalCount: 0, page: 1, pageSize: 3, hasNextPage: false } }); });
  await page.goto(feedStory);
  await expect(page.getByRole('status')).toHaveText('No nodes found.');
  await expect(page.locator('article')).toHaveCount(0);
  await expect(page.getByRole('button', { name: 'Load more nodes' })).toHaveCount(0);
  expect(requests).toBe(1);
});

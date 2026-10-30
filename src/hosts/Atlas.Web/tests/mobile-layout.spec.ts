import { expect, test } from '@playwright/test';

const id = 'b1000000-0000-4000-8000-000000000201';
const communityId = 'a1000000-0000-4000-8000-000000000001';

test.beforeEach(async ({ page }) => {
  await page.setViewportSize({ width: 390, height: 844 });
  // Real UI behavior with fixed API projections: membership writes are recorded below.
  await page.route('**/atlas-public-api/**', route => {
    const path = new URL(route.request().url()).pathname;
    if (path.includes('/communities/node/')) return route.fulfill({ json: [
      { id: communityId, name: 'Orca recovery', status: 'Active' },
    ] });
    if (path.endsWith('/nodes')) return route.fulfill({ json: {
      items: [{ nodeId: id, averageVote: 7.5, voteCount: 12, card: {
        title: 'Restore salmon habitat', typeName: 'Solution', authorName: 'Demo author',
        createdAt: '2026-10-01T12:00:00Z', status: 'Active',
        shortDescription: 'A fuller introduction that should use the available mobile card width.',
        communities: 'Orca recovery', commentCount: 2, childTypes: [], isHidden: false,
      } }], totalCount: 1, page: 1, pageSize: 3, hasNextPage: false,
    } });
    return route.fulfill({ json: [] });
  });
  await page.route('**/atlas-session-api/**', route => {
    const path = new URL(route.request().url()).pathname;
    if (path.endsWith('/votes')) return route.fulfill({ json: { voteCount: 12, averageVote: 7.5, currentParticipantVote: null } });
    return route.fulfill({ status: 401, json: {} });
  });
});

test('mobile feed summary opens the existing dial without casting a vote', async ({ page }) => {
  const writes: string[] = [];
  page.on('request', request => { if (['PUT', 'DELETE', 'POST'].includes(request.method())) writes.push(request.url()); });
  await page.goto('http://127.0.0.1:4173/');
  const card = page.locator('article.node-card').first();
  const summary = card.getByRole('button', { name: /Rate node importance/ });
  await expect(summary).toBeEnabled();
  await expect(card.getByRole('spinbutton')).toHaveCount(0);
  await expect(summary).toContainText('7.5');
  await expect(summary).toContainText('12');
  await expect(summary).toContainText('Vote');
  const readMore = card.getByRole('button', { name: 'Read more', exact: true });
  const row = await summary.boundingBox();
  const read = await readMore.boundingBox();
  expect(Math.abs((row!.y + row!.height / 2) - (read!.y + read!.height / 2))).toBeLessThan(2);
  expect(row!.x + row!.width).toBeLessThanOrEqual(read!.x);
  await summary.click();
  const dial = card.getByRole('spinbutton');
  await expect(dial).toBeVisible();
  // The expanded dial belongs beside the introduction, rather than a row beneath it.
  await expect(card.locator('.dial-content')).toHaveCSS('float', 'left');
  const selector = await card.locator('.selector').boundingBox();
  const introduction = await card.locator('.description').boundingBox();
  expect(selector!.y).toBeLessThan(introduction!.y + introduction!.height);

  await dial.focus();
  await page.keyboard.press('End');
  await expect(page.getByRole('dialog', { name: 'Confirm your importance rating', exact: true })).not.toBeVisible();
  await page.keyboard.press('Enter');
  await expect(page.getByRole('dialog', { name: 'Confirm your importance rating', exact: true })).toBeVisible();
  await page.getByRole('button', { name: 'Cancel', exact: true }).click();
  await summary.click();
  await expect(dial).toHaveCount(0);
  expect(writes).toEqual([]);
});

test('bottom navigation hides while reading down and returns when scrolling up', async ({ page }) => {
  await page.goto('http://127.0.0.1:4173/');
  const nav = page.getByRole('navigation', { name: 'Bottom navigation', exact: true });
  await expect(nav).toBeVisible();
  await expect(nav.getByRole('link', { name: 'Discover' })).toHaveAttribute('href', '/');
  await expect(nav.getByRole('link', { name: 'Create' })).toHaveAttribute('href', '/new');
  await expect(nav.getByRole('link', { name: 'You' })).toHaveAttribute('href', '/login');
  // Make the document long without relying on pagination or font-specific card heights.
  await page.evaluate(() => { document.body.style.minHeight = '3000px'; window.scrollTo(0, 600); });
  await expect(nav).toBeHidden();
  await page.evaluate(() => window.scrollTo(0, 400));
  await expect(nav).toBeVisible();
});

test('joining from a card uses membership transport and updates other controls', async ({ page }) => {
  let joined = false;
  const writes: string[] = [];
  await page.route('**/atlas-session-api/**', route => {
    const path = new URL(route.request().url()).pathname;
    if (path.endsWith('/auth/session')) return route.fulfill({ json: { participantId: 'demo' } });
    if (path.endsWith('/auth/csrf')) return route.fulfill({ json: { token: 'fixture-token' } });
    if (path.endsWith('/communities/mine')) return route.fulfill({ json: joined ? [{ id: communityId, name: 'Orca recovery', isMember: true }] : [] });
    if (path.endsWith('/membership')) {
      writes.push(route.request().method());
      expect(route.request().headers()['x-csrf-token']).toBeTruthy();
      joined = true;
      return route.fulfill({ status: 204 });
    }
    return route.fallback();
  });
  await page.goto('http://127.0.0.1:4173/');
  const card = page.locator('article.node-card').first();
  await card.getByRole('button', { name: 'Join Orca recovery', exact: true }).click();
  await expect(card.getByText('Joined', { exact: true })).toBeVisible();
  expect(writes).toEqual(['PUT']);
});

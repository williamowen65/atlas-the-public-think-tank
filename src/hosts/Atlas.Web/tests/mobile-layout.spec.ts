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
  await expect(card.locator('.node-type-row .rating-summary')).toBeVisible();
  const row = await summary.boundingBox();
  const type = await card.locator('.mobile-type').boundingBox();
  expect(Math.abs((row!.y + row!.height / 2) - (type!.y + type!.height / 2))).toBeLessThan(2);
  expect(type!.x + type!.width).toBeLessThanOrEqual(row!.x);
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
  // Returning to desktop restores the original bottom-right float after the
  // mobile dial has been mounted and removed.
  await page.setViewportSize({ width: 1280, height: 900 });
  await expect(card.locator('.read-more')).toHaveCSS('float', 'right');
  await expect(card.locator('.read-more')).toHaveCSS('position', 'static');
  await expect(dial).toBeVisible();
  await expect.poll(() => card.evaluate(element => Math.abs(
    element.querySelector('.description')!.getBoundingClientRect().bottom -
    element.querySelector('.read-more button')!.getBoundingClientRect().bottom,
  ))).toBeLessThan(2);
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

for (const rating of [0, 6]) {
  test(`saved mobile rating ${rating} collapses the dial and remains available for editing`, async ({ page }) => {
    let saved: number | null = null;
    await page.route('**/atlas-session-api/**', route => {
      const path = new URL(route.request().url()).pathname;
      if (path.endsWith('/auth/csrf')) return route.fulfill({ json: { token: 'fixture-token' } });
      if (path.endsWith('/vote') && route.request().method() === 'PUT') {
        saved = route.request().postDataJSON().value;
        return route.fulfill({ status: 204 });
      }
      if (path.endsWith('/votes')) return route.fulfill({ json: {
        voteCount: saved === null ? 12 : 13, averageVote: 7.5, currentParticipantVote: saved,
      } });
      return route.fallback();
    });
    await page.goto('http://127.0.0.1:4173/');
    const card = page.locator('article.node-card').first();
    const summary = card.getByRole('button', { name: /Rate node importance/ });
    await summary.click();
    const dial = card.getByRole('spinbutton');
    await dial.focus();
    await page.keyboard.press('Home');
    for (let i = 0; i < rating; i++) await page.keyboard.press('ArrowUp');
    await page.keyboard.press('Enter');
    await page.getByRole('button', { name: 'Confirm rating', exact: true }).click();
    await expect(summary).toContainText(`Voted ${rating}/10`);
    await expect(summary).toHaveAttribute('aria-expanded', 'false');
    await expect(dial).toHaveCount(0);
    await expect(summary).toBeFocused();
    // The retained button reopens the confirmed value rather than a neutral preview.
    await summary.click();
    await expect(dial).toHaveAttribute('aria-valuenow', String(rating));
    expect(saved).toBe(rating);
  });
}

test('mobile header stays on one row and long community drawer scrolls above bottom navigation', async ({ page }) => {
  await page.route('**/atlas-session-api/**', route => {
    const path = new URL(route.request().url()).pathname;
    if (path.endsWith('/auth/session')) return route.fulfill({ json: { participantId: 'demo' } });
    if (path.endsWith('/communities/mine')) return route.fulfill({ json: Array.from({ length: 40 }, (_, i) => ({
      id: 'community-' + i, name: 'Community ' + i, isMember: true,
    })) });
    return route.fallback();
  });
  await page.goto('http://127.0.0.1:4173/');
  const search = page.getByRole('textbox', { name: 'Search nodes and filters' });
  // Native summary role varies across accessibility implementations; use its label.
  const summary = page.getByLabel('Open navigation menu', { exact: true });
  const menuBox = await summary.boundingBox();
  const searchBox = await search.boundingBox();
  expect(Math.abs(menuBox!.y + menuBox!.height / 2 - searchBox!.y - searchBox!.height / 2)).toBeLessThan(2);
  await search.click();
  await expect(page.getByRole('textbox', { name: 'Search nodes and filters' })).toHaveAttribute('placeholder', 'Atlas: The Public Think Tank');
  await page.keyboard.press('Escape');
  const bottom = page.getByRole('navigation', { name: 'Bottom navigation', exact: true });
  await bottom.getByRole('link', { name: 'Discover' }).hover();
  await expect(bottom.getByRole('link', { name: 'Discover' })).toHaveCSS('text-decoration-line', 'none');
  await summary.click();
  const drawer = page.getByRole('navigation', { name: 'Mobile navigation', exact: true });
  await expect(drawer.getByRole('link', { name: 'Community 39', exact: true })).toHaveCount(1);
  await expect(bottom).toBeHidden();
  await expect(drawer).toHaveCSS('flex-wrap', 'nowrap');
  expect(await drawer.evaluate(element => element.scrollWidth <= element.clientWidth)).toBe(true);
  await drawer.evaluate(element => { element.scrollTop = element.scrollHeight; });
  expect(await drawer.evaluate(element => element.scrollTop)).toBeGreaterThan(0);
  await expect(drawer.getByRole('link', { name: 'Community 39', exact: true })).toBeInViewport();
  await page.keyboard.press('Escape');
  await expect(drawer).toBeHidden();
  await expect(summary).toBeFocused();
});

test('drawer animates after opening and outside dismissal', async ({ page }) => {
  await page.goto('http://127.0.0.1:4173/');
  const summary = page.getByLabel('Open navigation menu', { exact: true });
  const details = page.locator('details.mobile-menu');
  const drawer = page.getByRole('navigation', { name: 'Mobile navigation', exact: true });
  await summary.click();
  await expect(drawer).toBeVisible();
  await expect(details).not.toHaveClass(/entering/);
  // The exposed right-hand strip is outside the drawer, not its Close button.
  await page.mouse.click(385, 500);
  await expect(details).toHaveClass(/closing/);
  await expect(drawer).toBeHidden();
  await summary.click();
  await expect(drawer).toBeVisible();
  await expect(details).not.toHaveClass(/entering/);
  await page.keyboard.press('Escape');
  await expect(drawer).toBeHidden();
});

test('node detail matches compact mobile feed metadata and voting disclosure', async ({ page }) => {
  await page.route('**/atlas-public-api/nodes/' + id, route => route.fulfill({
    json: { id, requestedTypeIds: [], status: 'Active', authorId: 'demo' },
  }));
  await page.goto('http://127.0.0.1:4173/nodes?id=' + id);
  const node = page.locator('article.node-page');
  await expect(node.getByRole('heading', { name: 'Restore salmon habitat', exact: true })).toBeVisible();
  await expect(node.locator('.mobile-type')).toHaveText('Solution');
  await expect(node.locator('.relative-date')).toBeVisible();
  await expect(node.getByRole('button', { name: 'Join Orca recovery', exact: true })).toBeVisible();
  const summary = node.getByRole('button', { name: /Rate node importance/ });
  await expect(summary).toBeEnabled();
  await expect(node.locator('.node-type-row .rating-summary')).toBeVisible();
  await expect(node.getByRole('spinbutton')).toHaveCount(0);
  await summary.click();
  await expect(node.getByRole('spinbutton')).toBeVisible();
  await expect(node.locator('.dial-content')).toHaveCSS('float', 'left');
  await summary.click();
  await expect(node.getByRole('spinbutton')).toHaveCount(0);
});

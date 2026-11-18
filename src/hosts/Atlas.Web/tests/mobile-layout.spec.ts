import { expect, test } from '@playwright/test';

const id = 'b1000000-0000-4000-8000-000000000201';
const communityId = 'a1000000-0000-4000-8000-000000000001';

test.beforeEach(async ({ page }) => {
  await page.setViewportSize({ width: 390, height: 844 });
  // Exercise the feed with fixed API responses.
  await page.route('**/atlas-public-api/**', route => {
    const path = new URL(route.request().url()).pathname;
    if (path.includes('/communities/node/')) return route.fulfill({ json: [
      { id: communityId, name: 'Orca recovery', status: 'Active' },
    ] });
    if (path.endsWith('/nodes')) return route.fulfill({ json: {
      items: [{ nodeId: id, averageVote: 7.5, voteCount: 12, card: {
        title: 'Restore salmon habitat', typeName: 'Solution', authorName: 'Demo author',
        createdAt: '2026-10-01T12:00:00Z', status: 'Active',
        description: 'A fuller introduction that should use the available mobile card width.',
        communities: 'Orca recovery', commentCount: 2, childTypes: [], isHidden: false,
      } }], totalCount: 1, page: 1, pageSize: 3, hasNextPage: false,
    } });
    return route.fulfill({ json: [] });
  });
  await page.route('**/atlas-session-api/**', route => {
    if (route.request().method() === 'GET' && new URL(route.request().url()).pathname.endsWith('/moderation/queue')) return route.fulfill({ status: 403 });
    if (route.request().method() === 'GET' && /^\/atlas-session-api\/nodes\/[^/]+$/.test(new URL(route.request().url()).pathname)) return route.fulfill({ json: { hidden: false, requestedTypeIds: [] } });
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
  const body = card.locator('.body');
  const before = await body.boundingBox();
  await summary.click();
  const picker = card.getByRole('dialog', { name: 'Rate node importance', exact: true });
  const dial = picker.getByRole('spinbutton');
  await expect(dial).toBeVisible();
  expect(await body.boundingBox()).toEqual(before);

  await dial.focus();
  await page.keyboard.press('End');
  await expect(page.getByRole('dialog', { name: 'Confirm your importance rating', exact: true })).not.toBeVisible();
  await page.keyboard.press('Enter');
  await expect(page.getByRole('dialog', { name: 'Confirm your importance rating', exact: true })).toBeVisible();
  await page.getByRole('button', { name: 'Cancel', exact: true }).click();
  await picker.getByRole('button', { name: 'Close vote dial' }).click();
  await expect(dial).toHaveCount(0);
  expect(writes).toEqual([]);
});

for (const width of [390, 1280]) {
  test(`feed card details are collapsed by default and can be toggled at ${width}px`, async ({ page }) => {
    await page.setViewportSize({ width, height: 900 });
    await page.goto('http://127.0.0.1:4173/');
    const card = page.locator('article.node-card').first();
    const toggle = card.getByRole('button', { name: /reactions, comments, and subnodes/ });
    const details = card.locator('.card-details');
    await expect(toggle).toHaveAttribute('aria-expanded', 'false');
    await expect(details).toBeHidden();
    await toggle.click();
    await expect(toggle).toHaveAttribute('aria-expanded', 'true');
    await expect(details).toBeVisible();
    await toggle.click();
    await expect(details).toBeHidden();
  });
}

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

for (const isMember of [false, true]) {
  test(`mobile feed links to the community without membership controls (member: ${isMember})`, async ({ page }) => {
    await page.route('**/atlas-session-api/auth/session', route => route.fulfill({ json: { participantId: 'demo' } }));
    await page.route('**/atlas-session-api/communities/mine', route => route.fulfill({
      json: isMember ? [{ id: communityId, name: 'Orca recovery', isMember: true }] : [],
    }));
    await page.goto('http://127.0.0.1:4173/');
    const card = page.locator('article.node-card').first();
    await expect(card.getByRole('link', { name: 'Orca recovery' })).toHaveAttribute('href', `/communities/${communityId}`);
    await expect(card.getByRole('button', { name: 'Join Orca recovery' })).toHaveCount(0);
    await expect(card.getByText('Joined', { exact: true })).toHaveCount(0);
  });
}

for (const rating of [0, 6]) {
  test(`saved mobile rating ${rating} collapses the dial and remains available for editing`, async ({ page }) => {
    let saved: number | null = null;
    await page.route('**/atlas-session-api/**', route => {
    if (route.request().method() === 'GET' && new URL(route.request().url()).pathname.endsWith('/moderation/queue')) return route.fulfill({ status: 403 });
    if (route.request().method() === 'GET' && /^\/atlas-session-api\/nodes\/[^/]+$/.test(new URL(route.request().url()).pathname)) return route.fulfill({ json: { hidden: false, requestedTypeIds: [] } });
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
    await expect(summary.locator('.summary-label')).toHaveText(new RegExp(`^Voted\n${rating}/10$`), { useInnerText: true });
    await expect(summary).toHaveAttribute('aria-expanded', 'false');
    await expect(dial).toHaveCount(0);
    await expect(summary).toBeFocused();
    // The retained button reopens the confirmed value rather than a neutral preview.
    await summary.click();
    await expect(dial).toHaveAttribute('aria-valuenow', String(rating));
    expect(saved).toBe(rating);
  });
}

test('mobile header keeps its wordmark and search button on one row and the community drawer scrolls', async ({ page }) => {
  await page.route('**/atlas-session-api/**', route => {
    if (route.request().method() === 'GET' && new URL(route.request().url()).pathname.endsWith('/moderation/queue')) return route.fulfill({ status: 403 });
    if (route.request().method() === 'GET' && /^\/atlas-session-api\/nodes\/[^/]+$/.test(new URL(route.request().url()).pathname)) return route.fulfill({ json: { hidden: false, requestedTypeIds: [] } });
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
  const toggle = page.getByRole('button', { name: 'Toggle search', exact: true });
  const menuBox = await summary.boundingBox();
  const searchBox = await toggle.boundingBox();
  expect(Math.abs(menuBox!.y + menuBox!.height / 2 - searchBox!.y - searchBox!.height / 2)).toBeLessThan(2);
  await toggle.click();
  await expect(search).toHaveAttribute('placeholder', 'Search Atlas');
  const wordmark = page.getByText('The Public Think Tank', { exact: true });
  await expect(wordmark).toBeHidden();
  await expect(summary).toBeHidden();
  await page.keyboard.press('Escape');
  await expect(wordmark).toBeVisible();
  await expect(summary).toBeVisible();
  const bottom = page.getByRole('navigation', { name: 'Bottom navigation', exact: true });
  await bottom.getByRole('link', { name: 'Discover' }).hover();
  await expect(bottom.getByRole('link', { name: 'Discover' })).toHaveCSS('text-decoration-line', 'none');
  await summary.click();
  const drawer = page.getByRole('navigation', { name: 'Explore navigation', exact: true });
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
  const details = page.locator('details.explore-sidebar');
  const drawer = page.getByRole('navigation', { name: 'Explore navigation', exact: true });
  await summary.click();
  await expect(drawer).toBeVisible();
  await expect(details).not.toHaveClass(/entering/);
  // Clicking the exposed backdrop dismisses the shared Explore drawer.
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
  await expect(node.locator('.node-type')).toHaveText('Solution');
  await expect(node.locator('.relative-date')).toBeVisible();
  await expect(node.getByRole('link', { name: 'Orca recovery', exact: true })).toHaveAttribute('href', `/communities/${communityId}`);
  await expect(node.getByRole('button', { name: 'Join Orca recovery', exact: true })).toHaveCount(0);
  const summary = node.getByRole('button', { name: /Rate node importance/ });
  await expect(summary).toBeEnabled();
  await expect(node.locator('.node-type-row .rating-summary')).toBeVisible();
  await expect(node.getByRole('spinbutton')).toHaveCount(0);
  await summary.click();
  await expect(node.getByRole('spinbutton')).toBeVisible();
  await expect(node.getByRole('dialog', { name: 'Rate node importance' })).toBeVisible();
  await node.getByRole('button', { name: 'Close vote dial' }).click();
  await expect(node.getByRole('spinbutton')).toHaveCount(0);
});

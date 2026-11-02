import { test, expect, type Page } from '@playwright/test';

// These are UI-contract tests: the backend's authorization and persistence have separate tests.
// All session API responses are intercepted so CI needs no SQL Server or signed-in account.
const cases = [
  { nodeId: '11111111-1111-4111-8111-111111111111', reportedTitle: 'Orca recovery', count: 5, reviewRequested: false },
  { nodeId: '22222222-2222-4222-8222-222222222222', reportedTitle: 'Harbor access', count: 2, reviewRequested: true },
  { nodeId: '33333333-3333-4333-8333-333333333333', reportedTitle: 'Public transit', count: 1, reviewRequested: false },
];
async function mockModeration(page: Page, onDecision?: (url: string, body: unknown) => void) {
  await page.route('**/atlas-session-api/**', async (route) => {
    const url = route.request().url();
    if (url.endsWith('/auth/session')) return route.fulfill({ json: { participantId: 'moderator-participant' } });
    if (url.endsWith('/auth/csrf')) return route.fulfill({ json: { token: 'test-csrf' } });
    if (url.endsWith('/moderation/queue')) return route.fulfill({ json: cases });
    if (url.endsWith('/history')) return route.fulfill({ json: [
      { id: 'report-1', reason: 'Spam', explanation: 'Unrelated advertising', createdAt: '2026-01-01T00:00:00Z', status: 'Pending' },
      { id: 'report-2', reason: 'Harassment', explanation: 'Hostile language', createdAt: '2026-01-02T00:00:00Z', status: 'Pending' },
    ] });
    if (url.endsWith('/decision')) {
      onDecision?.(url, route.request().postDataJSON());
      return route.fulfill({ status: 204 });
    }
    return route.fulfill({ status: 404 });
  });
}

test('queue searches titles without changing the underlying node count', async ({ page }) => {
  await mockModeration(page);
  await page.goto('http://127.0.0.1:4173/moderation');
  await expect(page.getByText('Showing 3 of 3 nodes')).toBeVisible();
  await page.getByRole('searchbox', { name: 'Search node titles' }).fill('orca');
  await expect(page.getByText('Showing 1 of 3 nodes')).toBeVisible();
  await expect(page.getByRole('button', { name: /Orca recovery/ })).toBeVisible();
  await expect(page.getByRole('button', { name: /Harbor access/ })).toHaveCount(0);
});

test('review-request filter intersects with search and can be cleared', async ({ page }) => {
  await mockModeration(page);
  await page.goto('http://127.0.0.1:4173/moderation');
  await page.getByRole('checkbox', { name: 'Author review requests only' }).check();
  await expect(page.getByText('Showing 1 of 3 nodes')).toBeVisible();
  await expect(page.getByRole('button', { name: /Harbor access/ })).toBeVisible();
  await page.getByRole('searchbox', { name: 'Search node titles' }).fill('orca');
  await expect(page.getByText('No nodes match these filters.')).toBeVisible();
  await page.getByRole('checkbox', { name: 'Author review requests only' }).uncheck();
  await expect(page.getByRole('button', { name: /Orca recovery/ })).toBeVisible();
});

test('sorting changes order but does not merge unrelated nodes', async ({ page }) => {
  await mockModeration(page);
  await page.goto('http://127.0.0.1:4173/moderation');
  const buttons = page.locator('section.moderation > ul > li > button');
  await expect(buttons.first()).toContainText('Orca recovery');
  await page.getByLabel('Sort cases').selectOption('title');
  await expect(buttons.first()).toContainText('Harbor access');
  await expect(buttons).toHaveCount(3);
});

test('decision targets only the selected node even when other cases are visible', async ({ page }) => {
  const decisions: { url: string; body: unknown }[] = [];
  await mockModeration(page, (url, body) => decisions.push({ url, body }));
  await page.goto('http://127.0.0.1:4173/moderation');
  await page.getByRole('button', { name: /Orca recovery/ }).click();
  await expect(page.getByText('Unrelated advertising')).toBeVisible();
  await expect(page.getByText('Hostile language')).toBeVisible();
  await page.getByLabel('Moderator rationale (required)').fill('Reviewed all five reports.');
  await page.getByRole('button', { name: 'Dismiss reports' }).click();
  await expect.poll(() => decisions.length).toBe(1);
  expect(decisions[0].url).toContain('/moderation/nodes/' + cases[0].nodeId + '/decision');
  expect(decisions[0].url).not.toContain(cases[1].nodeId);
  expect(decisions[0].body).toMatchObject({ decision: 0, rationale: 'Reviewed all five reports.' });
});

test('access denial does not expose the moderation queue', async ({ page }) => {
  await page.route('**/atlas-session-api/**', async route => {
    if (route.request().url().endsWith('/moderation/queue')) return route.fulfill({ status: 403 });
    return route.fulfill({ json: { participantId: 'regular-member' } });
  });
  await page.goto('http://127.0.0.1:4173/moderation');
  await expect(page.getByRole('alert')).toContainText('Global moderator access is required');
  await expect(page.getByRole('button', { name: /Orca recovery/ })).toHaveCount(0);
});

test('anonymous visitors are redirected to sign-in before loading the queue', async ({ page }) => {
  let queueRequests = 0;
  await page.route('**/atlas-session-api/**', async route => {
    if (route.request().url().endsWith('/auth/session'))
      return route.fulfill({ json: { participantId: null } });
    if (route.request().url().endsWith('/moderation/queue')) queueRequests++;
    return route.fulfill({ status: 403 });
  });
  await page.goto('http://127.0.0.1:4173/moderation');
  await expect(page).toHaveURL(/\\/login(?:[?#]|$)/);
  expect(queueRequests).toBe(0);
  await expect(page.getByRole('heading', { name: 'Moderation', exact: true })).toHaveCount(0);
});

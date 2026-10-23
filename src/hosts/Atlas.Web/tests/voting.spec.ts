import { expect, test, type Page } from '@playwright/test';
const node = 'b1000000-0000-4000-8000-000000000001';
const story = `http://127.0.0.1:6006/iframe.html?id=compositions-node-page--playground&viewMode=story&args=nodeId:${node}`;
async function setup(page: Page, anonymous = false) {
  let signedIn = !anonymous; let fail = false;
  let summary = { voteCount: 3, averageVote: 5, currentParticipantVote: null as number | null };
  const mutations: { method: string; value?: number }[] = [];
  await page.route('**/atlas-public-api/**', route => {
    const url = new URL(route.request().url());
    if (url.pathname.endsWith('/node-types') || url.pathname.endsWith('/description') || url.pathname.endsWith('/reactions')) return route.fulfill({ json: [] });
    if (url.pathname.endsWith('/comments')) return route.fulfill({ json: { items: [], next: null } });
    if (url.pathname.endsWith(`/nodes/${node}`)) return route.fulfill({ json: { id: node, requestedTypeIds: [] } });
    return route.fulfill({ json: { items: [{ nodeId: node, voteCount: 3, averageVote: 5, card: { title: 'Voting question', shortDescription: 'Introduction', typeName: 'Question', authorName: 'Sam', status: 'Active', createdAt: '2026-10-22T12:00:00Z', breadcrumb: '', communities: '', commentCount: 0, isHidden: false } }], totalCount: 1, page: 1, pageSize: 3, hasNextPage: false } });
  });
  await page.route('**/atlas-session-api/**', route => {
    const request = route.request(); const path = new URL(request.url()).pathname;
    if (path.endsWith('/votes')) return route.fulfill({ json: { ...summary, currentParticipantVote: signedIn ? summary.currentParticipantVote : null } });
    if (path.endsWith('/auth/session')) return signedIn ? route.fulfill({ json: { participantId: 'voter' } }) : route.fulfill({ status: 401, json: {} });
    if (path.endsWith('/auth/csrf')) return route.fulfill({ json: { token: 'csrf-fixture' } });
    if (request.method() === 'GET' && path.endsWith('/reactions')) return route.fulfill({ json: [] });
    expect(request.headers()['x-csrf-token']).toBe('csrf-fixture');
    if (path.endsWith('/auth/login')) { signedIn = true; return route.fulfill({ status: 204 }); }
    if (path.endsWith('/vote')) {
      if (!signedIn) return route.fulfill({ status: 401, json: {} });
      if (fail) return route.fulfill({ status: 403, json: {} });
      const value = request.method() === 'DELETE' ? undefined : request.postDataJSON().value;
      mutations.push({ method: request.method(), value });
      summary = value === undefined ? { voteCount: 3, averageVote: 5, currentParticipantVote: null } : { voteCount: 4, averageVote: value === 10 ? 6.25 : 3.75, currentParticipantVote: value };
      return route.fulfill({ status: 204 });
    }
    return route.fulfill({ status: 404, json: {} });
  });
  return { mutations, fail: (value: boolean) => fail = value };
}

test('node-page votes persist, use server totals, restore on reload and support keyboard undo', async ({ page }) => {
  const fixture = await setup(page); await page.goto(story);
  const dial = page.getByRole('spinbutton', { name: 'Node importance', exact: true });
  await expect(dial).toHaveAttribute('aria-disabled', 'false');
  await expect(page.locator('.average')).toHaveText('5.0');
  await dial.focus(); await page.keyboard.press('End');
  // Selecting a value must not send a write until the user confirms it.
  await expect(page.getByRole('dialog', { name: 'Confirm your importance rating', exact: true })).toBeVisible();
  expect(fixture.mutations).toEqual([]);
  await page.getByRole('button', { name: 'Confirm rating', exact: true }).click();
  await expect(page.locator('.average')).toHaveText('6.3');
  await expect(page.locator('.vote-count')).toHaveAccessibleName('4 votes');
  await expect(page.locator('.voting-dial')).toHaveClass(/voted/);
  await expect(dial).toHaveAttribute('aria-valuenow', '10');
  expect(fixture.mutations).toEqual([{ method: 'PUT', value: 10 }]);
  await page.reload();
  await expect(dial).toHaveAttribute('aria-valuenow', '10');
  await dial.focus(); await page.keyboard.press('Home');
  await page.getByRole('button', { name: 'Confirm rating', exact: true }).click();
  await expect(dial).toHaveAttribute('aria-disabled', 'false');
  await expect(page.locator('.average')).toHaveText('3.8');
  await expect(page.locator('.vote-count')).toHaveAccessibleName('4 votes');
  await expect(dial).toHaveAttribute('aria-valuenow', '0');
  const undo = page.getByRole('button', { name: 'Undo your node importance vote' });
  await undo.focus(); await page.keyboard.press('Enter');
  await page.getByRole('button', { name: 'Confirm undo', exact: true }).click();
  await expect(dial).toHaveAttribute('aria-valuetext', /Unrated/);
  await expect(page.locator('.average')).toHaveText('5.0');
  await expect(page.locator('.vote-count')).toHaveAccessibleName('3 votes');
  await expect(page.locator('.voting-dial')).not.toHaveClass(/voted/);
  await expect(undo).toHaveCount(0);
  expect(fixture.mutations.at(-1)?.method).toBe('DELETE');
});

test('anonymous rating rolls back and opens a dismissible sign-in prompt', async ({ page }) => {
  const fixture = await setup(page, true); await page.goto(story);
  const dial = page.getByRole('spinbutton');
  await expect(dial).toHaveAttribute('aria-disabled', 'false');
  await dial.focus(); await page.keyboard.press('End');
  await page.getByRole('button', { name: 'Confirm rating', exact: true }).click();
  const dialog = page.getByRole('dialog', { name: 'Sign in to vote' });
  await expect(dialog).toBeVisible();
  await expect(dial).toHaveAttribute('aria-valuetext', /Unrated/);
  expect(fixture.mutations).toEqual([]);
  await page.keyboard.press('Escape'); await expect(dialog).not.toBeVisible();
  await dial.focus(); await page.keyboard.press('End');
  await page.getByRole('button', { name: 'Confirm rating', exact: true }).click(); await expect(dialog).toBeVisible();
  await dialog.getByLabel('Email', { exact: true }).fill('voter@example.test');
  await dialog.getByLabel('Password', { exact: true }).fill('fixture-password');
  await dialog.getByRole('button', { name: 'Sign in', exact: true }).click();
  await expect(dialog).not.toBeVisible();
  await expect(dial).toHaveAttribute('aria-disabled', 'false');
  await dial.focus(); await page.keyboard.press('End');
  await page.getByRole('button', { name: 'Confirm rating', exact: true }).click();
  await expect(page.locator('.vote-count')).toHaveAccessibleName('4 votes');
});

test('failed voting returns the dial to its confirmed state without inventing totals', async ({ page }) => {
  const fixture = await setup(page); fixture.fail(true); await page.goto(story);
  const dial = page.getByRole('spinbutton');
  await expect(dial).toHaveAttribute('aria-disabled', 'false');
  await dial.focus(); await page.keyboard.press('End');
  await page.getByRole('button', { name: 'Confirm rating', exact: true }).click();
  await expect(page.getByRole('alert')).toContainText('not permitted');
  await expect(dial).toHaveAttribute('aria-valuetext', /Unrated/);
  await expect(page.locator('.average')).toHaveText('5.0');
  await expect(page.locator('.vote-count')).toHaveAccessibleName('3 votes');
  expect(fixture.mutations).toEqual([]);
});


test('cancelling a proposed rating leaves the confirmed vote and server totals unchanged', async ({ page }) => {
  const fixture = await setup(page);
  await page.goto(story);
  const dial = page.getByRole('spinbutton', { name: 'Node importance', exact: true });
  await expect(dial).toHaveAttribute('aria-disabled', 'false');
  await dial.focus();
  await page.keyboard.press('End');
  const confirmation = page.getByRole('dialog', { name: 'Confirm your importance rating', exact: true });
  await expect(confirmation).toBeVisible();
  expect(fixture.mutations).toEqual([]);
  await confirmation.getByRole('button', { name: 'Cancel', exact: true }).click();
  await expect(confirmation).not.toBeVisible();
  await expect(dial).toHaveAttribute('aria-valuetext', /Unrated/);
  await expect(page.locator('.average')).toHaveText('5.0');
  await expect(page.locator('.vote-count')).toHaveAccessibleName('3 votes');
  expect(fixture.mutations).toEqual([]);
});

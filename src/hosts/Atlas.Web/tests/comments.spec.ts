import { expect, test, type Page } from '@playwright/test';
const node = 'b1000000-0000-4000-8000-000000000001';
const mine = 'b2000000-0000-4000-8000-000000000001';
const other = 'b2000000-0000-4000-8000-000000000002';
const first = 'c1000000-0000-4000-8000-000000000001';
const second = 'c1000000-0000-4000-8000-000000000002';
const comment = (id: string, body: string | null, authorId = mine) => ({ id, body, authorId, createdAt: '2026-10-22T12:00:00Z', updatedAt: '2026-10-22T12:00:00Z', status: body === null ? 'RemovedByAuthor' : 'Active' });
const story = 'http://127.0.0.1:6006/iframe.html?id=compositions-node-page--playground&viewMode=story';
async function setup(page: Page) {
  let signedIn = false;
  const reads: URL[] = []; const writes: { path: string; method: string; body: unknown }[] = [];
  await page.route('**/atlas-public-api/**', route => {
    const url = new URL(route.request().url());
    if (url.pathname.endsWith('/comments')) {
      reads.push(url);
      if (url.searchParams.has('parent')) return route.fulfill({ json: { items: [comment(url.searchParams.has('afterId') ? 'reply-two' : 'reply', url.searchParams.has('afterId') ? 'A second reply' : 'A nested reply', other)], next: url.searchParams.has('afterId') ? null : { afterAt: '2026-10-22T12:00:00Z', afterId: 'reply' } } });
      if (url.searchParams.has('afterId')) return route.fulfill({ json: { items: [comment(second, 'Another perspective', other)], next: null } });
      return route.fulfill({ json: { items: [comment(first, 'An opening thought'), comment('removed', null, other)], next: { afterAt: '2026-10-22T12:00:00Z', afterId: first } } });
    }
    if (url.pathname.endsWith('/catalog/node-types')) return route.fulfill({ json: [] });
    if (url.pathname.endsWith('/reactions') || url.pathname.endsWith('/description')) return route.fulfill({ json: [] });
    if (url.pathname.endsWith(`/nodes/${node}`)) return route.fulfill({ json: { id: node, requestedTypeIds: [] } });
    return route.fulfill({ json: { items: [{ nodeId: node, averageVote: null, voteCount: 0, card: { title: 'Comment discussion', shortDescription: 'A place to discuss', typeName: 'Question', authorName: 'Sam', status: 'Active', createdAt: '2026-10-22T12:00:00Z', breadcrumb: '', subNodes: 'None', communities: '', commentCount: 3, isHidden: false } }], totalCount: 1, page: 1, pageSize: 3, hasNextPage: false } });
  });
  await page.route('**/atlas-session-api/**', async route => {
    if (new URL(route.request().url()).pathname.endsWith('/votes')) return route.fulfill({ json: { voteCount: 0, averageVote: null, currentParticipantVote: null } });
    const request = route.request(); const url = new URL(request.url());
    if (url.pathname.endsWith('/auth/session')) return signedIn ? route.fulfill({ json: { participantId: mine } }) : route.fulfill({ status: 401, json: {} });
    if (url.pathname.endsWith('/auth/csrf')) return route.fulfill({ json: { token: 'fixture-csrf' } });
    if (url.pathname.includes('/participants/')) return route.fulfill({ json: { displayName: url.pathname.endsWith(mine) ? 'Sam' : 'Taylor' } });
    if (request.method() === 'GET' && url.pathname.endsWith('/reactions')) return route.fulfill({ json: [] });
    expect(request.headers()['x-csrf-token']).toBe('fixture-csrf');
    const body = request.method() === 'DELETE' ? undefined : request.postDataJSON();
    writes.push({ path: url.pathname, method: request.method(), body });
    if (url.pathname.endsWith('/auth/login')) { signedIn = true; return route.fulfill({ status: 204 }); }
    if (url.pathname.endsWith('/auth/logout')) { signedIn = false; return route.fulfill({ status: 204 }); }
    if (request.method() === 'POST') return route.fulfill({ json: comment('posted', body.body) });
    return route.fulfill({ status: 204 });
  });
  return { reads, writes };
}
async function login(page: Page) {
  await page.getByText('Sign in to comment', { exact: true }).click();
  await page.getByLabel('Email', { exact: true }).fill('sam@example.test');
  await page.getByLabel('Password', { exact: true }).fill('fixture-password');
  await page.getByRole('button', { name: 'Sign in', exact: true }).click();
  await expect(page.getByRole('button', { name: 'Post comment', exact: true })).toBeVisible();
}

test('comments load a bounded root page and lazy, independently paged reply threads', async ({ page }) => {
  const { reads } = await setup(page);
  await page.goto(story);
  await expect(page.getByText('An opening thought', { exact: true })).toBeVisible();
  await expect(page.getByText('Comment removed.', { exact: true })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Post comment' })).toHaveCount(0);
  expect(reads).toHaveLength(1); expect(reads[0].searchParams.get('size')).toBe('10');
  const item = page.locator(`article[data-comment-id="${first}"]`);
  await item.getByRole('button', { name: 'View replies' }).click();
  await expect(page.getByText('A nested reply', { exact: true })).toBeVisible();
  expect(reads[1].searchParams.get('parent')).toBe(first);
  expect(reads[1].searchParams.has('afterId')).toBe(false);
  await page.getByRole('button', { name: 'Load more comments' }).click();
  await expect(page.getByText('Another perspective', { exact: true })).toBeVisible();
  expect(reads[2].searchParams.get('afterId')).toBe(first);
  expect(reads[2].searchParams.get('afterAt')).toBe('2026-10-22T12:00:00Z');
  await expect(page.getByRole('button', { name: 'Load more comments' })).toHaveCount(0);
  await page.getByRole('button', { name: 'Load more replies' }).click();
  await expect(page.getByText('A second reply', { exact: true })).toBeVisible();
  expect(reads[3].searchParams.get('parent')).toBe(first);
  expect(reads[3].searchParams.get('afterId')).toBe('reply');
  await expect(page.getByRole('button', { name: 'Load more replies' })).toHaveCount(0);
  await page.setViewportSize({ width: 320, height: 800 });
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(320);
});

test('sign-in enables posting, replies and author-only editing/removal with CSRF headers', async ({ page }) => {
  const { writes } = await setup(page); await page.goto(story); await login(page);
  await page.getByLabel('Add a comment', { exact: true }).fill('My new comment');
  await page.getByRole('button', { name: 'Post comment', exact: true }).click();
  await expect(page.getByText('My new comment', { exact: true })).toBeVisible();
  await expect(page.getByRole('tab', { name: '4 Comments', exact: true })).toBeVisible();
  const opening = page.locator(`article[data-comment-id="${first}"]`);
  await opening.getByRole('button', { name: 'Edit', exact: true }).click();
  await page.getByLabel('Edit comment', { exact: true }).fill('Edited opening thought');
  await page.getByRole('button', { name: 'Save changes' }).click();
  await expect(page.getByText('Edited opening thought', { exact: true })).toBeVisible();
  await opening.getByRole('button', { name: 'Reply', exact: true }).first().click();
  await page.getByLabel('Your reply', { exact: true }).fill('A reply from me');
  await page.getByRole('button', { name: 'Post reply', exact: true }).click();
  await expect(page.getByText('A reply from me', { exact: true })).toBeVisible();
  await expect(page.getByRole('tab', { name: '5 Comments', exact: true })).toBeVisible();
  await expect(page.getByRole('article', { name: 'Comment by Taylor' }).getByRole('button', { name: 'Edit', exact: true })).toHaveCount(0);
  const edited = page.locator('article.comment').filter({ has: page.getByText('Edited opening thought', { exact: true }) });
  await edited.getByRole('button', { name: 'Remove', exact: true }).first().click();
  await page.getByRole('button', { name: 'Remove comment', exact: true }).click();
  await expect(page.getByText('Edited opening thought', { exact: true })).toHaveCount(0);
  await expect(page.getByText('A reply from me', { exact: true })).toBeVisible();
  await expect(page.getByRole('tab', { name: '5 Comments', exact: true })).toBeVisible();
  expect(writes.some(write => write.method === 'PUT' && write.path.endsWith(`/comments/${first}`))).toBe(true);
  expect(writes.some(write => write.path.endsWith(`/comments/${first}/replies`))).toBe(true);
  expect(writes.some(write => write.method === 'DELETE')).toBe(true);
  // This composition hides SessionControl after sign-in; simulate session expiry.
  await page.route('**/atlas-session-api/auth/session', route => route.fulfill({ status: 401, json: {} }));
  await page.evaluate(() => window.dispatchEvent(new Event('atlas-session-changed')));
  await expect(page.getByRole('button', { name: 'Post comment', exact: true })).toHaveCount(0);
});

test('failed posting keeps the draft and failed pages retain already loaded comments', async ({ page }) => {
  await setup(page); await page.goto(story); await login(page);
  await page.route(`**/atlas-session-api/nodes/${node}/comments`, route => route.fulfill({ status: 403, json: {} }));
  await page.getByLabel('Add a comment', { exact: true }).fill('Keep this draft');
  await page.getByRole('button', { name: 'Post comment', exact: true }).click();
  await expect(page.getByRole('alert')).toContainText('not permitted');
  await expect(page.getByLabel('Add a comment', { exact: true })).toHaveValue('Keep this draft');
  await page.route('**/atlas-public-api/**/comments?**', route => route.fulfill({ status: 503, json: {} }));
  await page.getByRole('button', { name: 'Load more comments' }).click();
  await expect(page.getByText('An opening thought', { exact: true })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Retry comments' })).toBeVisible();
});

test('local session bridge rejects foreign origins and unrelated writes', async ({ request }) => {
  const foreign = await request.post(`http://127.0.0.1:6007/atlas-session-api/nodes/${node}/comments`, { headers: { Origin: 'https://example.test' }, data: { body: 'Rejected' } });
  expect(foreign.status()).toBe(403);
  const unrelated = await request.post('http://127.0.0.1:6007/atlas-session-api/auth/delete', { headers: { Origin: 'http://127.0.0.1:6007' }, data: {} });
  expect(unrelated.status()).toBe(405);
});


test('feed comment control opens and positions the Comments tab', async ({ page }) => {
  await setup(page);
  await page.goto('http://127.0.0.1:6006/iframe.html?id=compositions-feed--playground&viewMode=story');
  await page.getByRole('button', { name: '3 Comments', exact: true }).click();
  await expect(page.getByRole('tab', { name: '3 Comments', exact: true })).toHaveAttribute('aria-selected', 'true');
  await expect(page.getByText('An opening thought', { exact: true })).toBeVisible();
  const tabs = page.getByRole('tablist');
  await expect.poll(async () => Math.abs(((await tabs.boundingBox())?.y ?? Infinity) - 20)).toBeLessThanOrEqual(2);
});

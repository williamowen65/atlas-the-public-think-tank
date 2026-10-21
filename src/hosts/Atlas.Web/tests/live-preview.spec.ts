import { expect, test } from '@playwright/test';

const preview = 'http://127.0.0.1:6006/iframe.html?id=compositions-node-voting-preview--playground&viewMode=story&args=source:local-api';
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
  await page.goto(preview);
  await expect(page.getByRole('heading', { name: 'Database question', exact: true })).toBeVisible();
  await expect(page.locator('.average')).toHaveText('7.5');
  await expect(page.locator('.vote-count')).toHaveAccessibleName('12 votes');
  await expect(page.locator('.node-card')).toContainText('Local community');
  await expect(page.getByRole('option', { name: /Hidden private title/ })).toHaveCount(0);
  await page.getByLabel('Node on this page').selectOption('two');
  await expect(page.getByRole('heading', { name: 'Database second node' })).toBeVisible();
  await page.getByRole('spinbutton').focus();
  await page.keyboard.press('End');
  await expect(page.getByRole('status')).toHaveText('Local preview selection: 10 out of 10. Not submitted.');
  await expect(page.getByRole('status')).toHaveText('No local rating selected');
  await expect(page.getByRole('spinbutton')).toHaveAttribute('aria-valuetext', /Unrated/);
  await expect.poll(() => page.getByRole('spinbutton').evaluate(el => el.scrollTop)).toBe(220);
  await page.getByRole('button', { name: 'Next page' }).click();
  await expect(page.getByRole('heading', { name: 'A newly entered node' })).toBeVisible();
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

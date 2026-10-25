import { expect, test } from '@playwright/test';

test('search voting preference persists, reaches backend and resets pagination', async ({
  page,
}) => {
  const weights: string[] = [];
  const strictness: string[] = [];
  await page.route('**/atlas-session-api/**', (route) => route.fulfill({ status: 401, json: {} }));
  await page.route('**/atlas-public-api/**', (route) => {
    const url = new URL(route.request().url());
    if (url.pathname.endsWith('/catalog/node-types')) return route.fulfill({ json: [] });
    if (url.pathname.endsWith('/search'))
      strictness.push(url.searchParams.get('MatchStrictness') ?? 'missing');
    if (url.pathname.endsWith('/search'))
      weights.push(url.searchParams.get('VotingWeight') ?? 'missing');
    return route.fulfill({
      json: { items: [], totalCount: 0, page: 1, pageSize: 3, hasNextPage: false },
    });
  });
  await page.goto('http://127.0.0.1:4173/about?q=testing');
  await page.getByRole('button', { name: 'Toggle search', exact: true }).click();
  await expect.poll(() => weights.includes('40')).toBe(true);
  await page.getByRole('button', { name: 'Filters', exact: true }).click();
  await expect(page.getByRole('slider', { name: 'Match strictness', exact: true })).toHaveValue(
    '15',
  );
  await page.getByRole('slider', { name: 'Match strictness', exact: true }).press('End');
  await page.getByRole('slider', { name: 'Voting weight', exact: true }).press('Home');
  await expect(page.getByRole('slider', { name: 'Voting weight', exact: true })).toHaveValue('0');
  await page.getByRole('button', { name: 'Apply filters', exact: true }).click();
  await expect.poll(() => weights.at(-1)).toBe('0');
  await expect.poll(() => strictness.at(-1)).toBe('100');
  await page.reload();
  await page.getByRole('button', { name: 'Toggle search', exact: true }).click();
  await page.getByRole('button', { name: 'Filters', exact: true }).click();
  await expect(page.getByRole('slider', { name: 'Voting weight', exact: true })).toHaveValue('0');
  await expect(page.getByRole('slider', { name: 'Match strictness', exact: true })).toHaveValue(
    '100',
  );
  await page.getByRole('button', { name: 'Reset voting weight', exact: true }).click();
  await expect(page.getByRole('slider', { name: 'Voting weight', exact: true })).toHaveValue('40');
  await page.goto('http://127.0.0.1:4173/my-settings');
  await expect(page.getByRole('slider', { name: 'Voting weight', exact: true })).toHaveValue('40');
});

test('settings restores match strictness independently of voting weight', async ({ page }) => {
  await page.addInitScript(() =>
    localStorage.setItem(
      'atlas-search-preferences:anonymous',
      JSON.stringify({ votingWeight: 20, matchStrictness: 75 }),
    ),
  );
  await page.route('**/atlas-session-api/**', (route) => route.fulfill({ status: 401, json: {} }));
  await page.goto('http://127.0.0.1:4173/my-settings');
  await expect(page.getByRole('slider', { name: 'Match strictness', exact: true })).toHaveValue(
    '75',
  );
  await page.getByRole('button', { name: 'Reset match strictness' }).click();
  await expect(page.getByRole('slider', { name: 'Voting weight', exact: true })).toHaveValue('20');
  expect(
    await page.evaluate(() =>
      JSON.parse(localStorage.getItem('atlas-search-preferences:anonymous')!),
    ),
  ).toEqual({ votingWeight: 20, matchStrictness: 15 });
});

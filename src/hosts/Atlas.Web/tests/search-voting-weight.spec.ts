import { expect, test } from '@playwright/test';

test('search voting preference persists, reaches backend and resets pagination', async ({
  page,
}) => {
  const weights: string[] = [];
  await page.route('**/atlas-session-api/**', (route) => route.fulfill({ status: 401, json: {} }));
  await page.route('**/atlas-public-api/**', (route) => {
    const url = new URL(route.request().url());
    if (url.pathname.endsWith('/catalog/node-types')) return route.fulfill({ json: [] });
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
  await page.getByRole('slider', { name: 'Voting weight', exact: true }).press('Home');
  await expect(page.getByRole('slider', { name: 'Voting weight', exact: true })).toHaveValue('0');
  await page.getByRole('button', { name: 'Apply filters', exact: true }).click();
  await expect.poll(() => weights.at(-1)).toBe('0');
  await page.reload();
  await page.getByRole('button', { name: 'Toggle search', exact: true }).click();
  await page.getByRole('button', { name: 'Filters', exact: true }).click();
  await expect(page.getByRole('slider', { name: 'Voting weight', exact: true })).toHaveValue('0');
  await page.getByRole('button', { name: 'Reset voting weight', exact: true }).click();
  await expect(page.getByRole('slider', { name: 'Voting weight', exact: true })).toHaveValue('40');
  await page.goto('http://127.0.0.1:4173/my-settings');
  await expect(page.getByRole('slider', { name: 'Voting weight', exact: true })).toHaveValue('40');
});

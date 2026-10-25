import { expect, test } from '@playwright/test';
const story =
  'http://127.0.0.1:6007/iframe.html?id=compositions-search-ranking--workbench&viewMode=story';
function report() {
  return {
    policy: 'support-v2',
    page: { totalCount: 4, page: 1, hasNextPage: true },
    rows: [
      {
        position: 1,
        searchText: 'Actual local indexed text',
        item: { card: { title: 'Local node' }, voteCount: 1, averageVote: 10 },
        evidence: {
          id: 'node-id',
          nodeId: 'node-id',
          kind: 'node',
          textRank: 100,
          support: 5,
          multiplier: 1.0833333333,
          finalScore: 108.33333333,
          tieDate: '2026-01-01T12:00:00Z',
        },
      },
    ],
  };
}
test('searches local data, displays diagnostics and pages the same query', async ({ page }) => {
  const submitted: unknown[] = [];
  await page.route('**/atlas-ranking-lab/run', (route) => {
    submitted.push(route.request().postDataJSON());
    return route.fulfill({ json: report() });
  });
  await page.goto(story);
  await expect(page.getByLabel('Scenario', { exact: true })).toHaveCount(0);
  await page.getByLabel('Search terms').fill('testing');
  await page.getByRole('button', { name: 'Search', exact: true }).click();
  await expect(page.getByRole('table')).toContainText('108.3333');
  await expect(page.getByRole('table')).toContainText('Local node');
  expect(submitted[0]).toEqual({ query: { searchText: 'testing', page: 1 }, scope: 'all' });
  await page.getByRole('button', { name: 'Next page' }).click();
  await expect.poll(() => submitted.length).toBe(2);
  expect(submitted[1]).toEqual({ query: { searchText: 'testing', page: 2 }, scope: 'all' });
});
test('failed searches retain inputs and permit retry and export', async ({ page }) => {
  let calls = 0;
  await page.route('**/atlas-ranking-lab/run', (route) =>
    ++calls === 1
      ? route.fulfill({ status: 503, json: { message: 'SQL Server is unavailable.' } })
      : route.fulfill({ json: report() }),
  );
  await page.goto(story);
  await page.getByLabel('Search terms').fill('custom query');
  await page.getByRole('button', { name: 'Search', exact: true }).click();
  await expect(page.getByRole('alert')).toHaveText('SQL Server is unavailable.');
  await expect(page.getByLabel('Search terms')).toHaveValue('custom query');
  await page.getByRole('button', { name: 'Search', exact: true }).click();
  await page.getByText('Exportable diagnostics', { exact: true }).click();
  expect(
    JSON.parse(await page.getByLabel('Search inputs and ranking results').inputValue()).inputs.query
      .searchText,
  ).toBe('custom query');
});

test('development bridge rejects foreign origins and unsupported operations', async ({
  request,
}) => {
  const foreign = await request.post('http://127.0.0.1:6007/atlas-ranking-lab/run', {
    headers: { Origin: 'https://example.test' },
    data: {},
  });
  expect(foreign.status()).toBe(403);
  const unrelated = await request.delete('http://127.0.0.1:6007/atlas-ranking-lab/delete', {
    headers: { Origin: 'http://127.0.0.1:6007' },
  });
  expect(unrelated.status()).toBe(405);
});

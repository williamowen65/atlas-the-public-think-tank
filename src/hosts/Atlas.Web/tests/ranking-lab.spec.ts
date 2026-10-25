import { expect, test } from '@playwright/test';
const story =
  'http://127.0.0.1:6007/iframe.html?id=compositions-search-ranking--workbench&viewMode=story';
function report() {
  return {
    policy: 'support-v2',
    page: { totalCount: 4, page: 1, hasNextPage: false },
    rows: [
      {
        position: 1,
        searchText: 'Actual local indexed text',
        sources: [{ field: 'Title', text: 'Actual local indexed text' }],
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
    const result = report();
    const payload = route.request().postDataJSON();
    result.page.page = payload.query.page;
    result.page.hasNextPage = payload.query.page === 1;
    result.rows[0].evidence.id = `node-${payload.query.page}`;
    result.rows[0].item.card.title = `Local node ${payload.query.page}`;
    return route.fulfill({ json: result });
  });
  await page.goto(story);
  await expect(page.getByLabel('Scenario', { exact: true })).toHaveCount(0);
  await page.getByLabel('Search terms').fill('testing');
  await page.getByRole('button', { name: 'Search', exact: true }).click();
  await expect(page.getByRole('table').first()).toContainText('108.3333');
  await expect(page.getByRole('table').first()).toContainText('Local node');
  expect(submitted[0]).toEqual({ query: { searchText: 'testing', page: 1 }, scope: 'all' });
  await page.getByRole('heading', { name: 'Indexed text and tie-breaks' }).scrollIntoViewIfNeeded();
  await expect.poll(() => submitted.length).toBe(2);
  await expect(page.getByRole('table').first()).toContainText('Local node 1');
  await expect(page.getByRole('table').first()).toContainText('Local node 2');
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

test('field evidence and experimental scores are inspectable without another search', async ({
  page,
}) => {
  let calls = 0;
  await page.route('**/atlas-ranking-lab/run', (route) => {
    calls++;
    return route.fulfill({ json: report() });
  });
  await page.goto(story);
  await page.getByLabel('Search terms').fill('local');
  await page.getByRole('button', { name: 'Search', exact: true }).click();
  const comparison = page.getByRole('table', { name: 'Experimental ranking' });
  await expect(comparison).toContainText('0.5000');
  await page.getByText(/1\. Local node · node/).click();
  await expect(
    page.getByRole('heading', { name: 'Title · 4 words · 1 literal hits' }),
  ).toBeVisible();
  await expect(page.locator('mark').first()).toHaveText('local');
  await page.getByLabel('Voting weight (%)').press('Home');
  await page.getByText('Exportable diagnostics', { exact: true }).click();
  const exported = JSON.parse(
    await page.getByLabel('Search inputs and ranking results').inputValue(),
  );
  expect(exported.experiment.votingPercent).toBe(0);
  expect(exported.experiment.rows[0].voteContribution).toBe(0);
  expect(exported.report.rows[0].sources[0].field).toBe('Title');
  expect(calls).toBe(1);
});

import { expect, test } from '@playwright/test';
import fixtures from '../../../../tests/Fixtures/SearchRanking/scenarios.json';
const story = 'http://127.0.0.1:6007/iframe.html?id=compositions-search-ranking--workbench&viewMode=story';

// Transport stubs verify the UI only; actual scores/orders are asserted against SQL by the shared fixture tests.
function report() {
  return {
    policy: 'support-v2', corpus: 'Isolated fixture corpus.',
    phases: [{ name: 'Baseline', checks: [{ description: 'Expected order: rated → supported → neutral', passed: true }],
      rows: [{ position: 1, page: 1, key: 'rated', searchText: 'Silver otter meadow  ', votes: [10], average: 10, netScore: 0,
        evidence: { id: '20000000-0000-0000-0000-000000000001', nodeId: '20000000-0000-0000-0000-000000000001', kind: 'node', textRank: 100,
          support: 5, multiplier: 1.0833333333, finalScore: 108.33333333, tieDate: '2026-01-01T12:00:00Z' } }] }],
  };
}

test('shared fixtures can be edited, reset, run and inspected with SQL diagnostic output', async ({ page }) => {
  const submitted: unknown[] = [];
  await page.route('**/atlas-ranking-lab/run', route => {
    submitted.push(route.request().postDataJSON());
    return route.fulfill({ json: report() });
  });
  await page.goto(story);
  await expect(page.getByRole('heading', { name: 'See why one result outranks another' })).toBeVisible();
  const scenarios = page.getByLabel('Scenario', { exact: true });
  await expect(scenarios.locator('option')).toHaveCount(fixtures.length);
  await page.getByRole('button', { name: 'Run scenario', exact: true }).click();
  expect(submitted[0]).toMatchObject({ id: fixtures[0].id, query: fixtures[0].query, phases: fixtures[0].phases });
  await expect(page.getByRole('table')).toContainText('108.3333');
  await expect(page.getByRole('table')).toContainText('×1.0833');
  await expect(page.getByText('PASS', { exact: true })).toBeVisible();
  await page.getByRole('textbox', { name: 'Search terms', exact: true }).fill('testing');
  await expect(page.getByRole('status')).toContainText('Inputs changed');
  await page.getByRole('button', { name: 'Reset scenario', exact: true }).click();
  await expect(page.getByRole('textbox', { name: 'Search terms', exact: true })).toHaveValue(fixtures[0].query);
  await expect(page.getByRole('table')).toHaveCount(0);
  await scenarios.selectOption('test-testing');
  await expect(page.getByRole('textbox', { name: 'Search terms', exact: true })).toHaveValue('testing');
  await expect(page.getByText('Explore test versus testing', { exact: true })).toHaveCount(1);
});

test('failed runs retain inputs, permit retry and expose failed expectations', async ({ page }) => {
  let calls = 0;
  await page.route('**/atlas-ranking-lab/run', route => {
    if (++calls === 1) return route.fulfill({ status: 503, json: { message: 'SQL Server is unavailable.' } });
    const response = report(); response.phases[0].checks[0].passed = false;
    return route.fulfill({ json: response });
  });
  await page.goto(story);
  await page.getByRole('textbox', { name: 'Search terms', exact: true }).fill('custom query');
  await page.getByRole('button', { name: 'Run scenario', exact: true }).click();
  await expect(page.getByRole('alert')).toHaveText('SQL Server is unavailable.');
  await expect(page.getByRole('textbox', { name: 'Search terms', exact: true })).toHaveValue('custom query');
  await page.getByRole('button', { name: 'Run scenario', exact: true }).click();
  await expect(page.getByText('FAIL', { exact: true })).toBeVisible();
  await page.getByText('Exportable diagnostics', { exact: true }).click();
  const exported = page.getByRole('textbox', { name: 'Scenario inputs and ranking results' });
  expect(JSON.parse(await exported.inputValue()).inputs.query).toBe('custom query');
});

test('development bridge rejects foreign origins and unsupported operations', async ({ request }) => {
  const foreign = await request.post('http://127.0.0.1:6007/atlas-ranking-lab/run', { headers: { Origin: 'https://example.test' }, data: {} });
  expect(foreign.status()).toBe(403);
  const unrelated = await request.delete('http://127.0.0.1:6007/atlas-ranking-lab/delete', { headers: { Origin: 'http://127.0.0.1:6007' } });
  expect(unrelated.status()).toBe(405);
});

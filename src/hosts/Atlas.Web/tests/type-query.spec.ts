import { expect, test } from '@playwright/test';
import { parseDiscoveryQuery } from '../src/lib/state/parseDiscoveryQuery';

const types = [
  { id: 'issue-id', name: 'Issue', autoPluralize: true },
  { id: 'multi-id', name: 'Research Question', autoPluralize: true },
];

test('type queries resolve names case-insensitively, including quoted multiword names', () => {
  expect(parseDiscoveryQuery('oceans type:issue', types)).toEqual({
    text: 'oceans',
    filters: { typeId: 'issue-id' },
    errors: [],
  });
  expect(parseDiscoveryQuery('type:ISSUE', types).filters.typeId).toBe('issue-id');
  expect(parseDiscoveryQuery('type:"research question" votes:1..5', types)).toEqual({
    text: '',
    filters: { typeId: 'multi-id', minVotes: 1, maxVotes: 5 },
    errors: [],
  });
  expect(parseDiscoveryQuery('type:missing', types).errors).toHaveLength(1);
  // Existing saved ID queries remain compatible.
  expect(parseDiscoveryQuery('type:issue-id', types).filters.typeId).toBe('issue-id');
});

test('type toolbox emits a readable name and sends the corresponding API ID', async ({ page }) => {
  await page.route('**/atlas-session-api/**', (route) => route.fulfill({ status: 401, json: {} }));
  const lookups: URL[] = [];
  await page.route('**/atlas-public-api/**', (route) => {
    const url = new URL(route.request().url());
    if (url.pathname.endsWith('/catalog/node-types')) return route.fulfill({ json: types });
    lookups.push(url);
    return route.fulfill({
      json: { items: [], totalCount: 0, page: 1, pageSize: 3, hasNextPage: false },
    });
  });
  await page.goto('http://127.0.0.1:4173/about');
  await page.getByRole('button', { name: 'Toggle search', exact: true }).click();
  await page.getByRole('button', { name: 'Filters', exact: true }).click();
  await page.getByRole('combobox', { name: 'Node type', exact: true }).selectOption('multi-id');
  await expect(page.getByRole('textbox', { name: 'Search nodes and filters' })).toHaveValue(
    'type:"Research Question"',
  );
  await page.getByRole('button', { name: 'Apply filters', exact: true }).click();
  await expect
    .poll(() => lookups.some((url) => url.searchParams.get('NodeTypeId') === 'multi-id'))
    .toBe(true);
});

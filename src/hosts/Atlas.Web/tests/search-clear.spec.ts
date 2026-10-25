import { expect, test } from '@playwright/test';

test('input clear button clears applied filters, invalid draft text and saved query together', async ({
  page,
}) => {
  await page.route('**/atlas-session-api/**', (route) => route.fulfill({ status: 401, json: {} }));
  await page.route('**/atlas-public-api/**', (route) => route.fulfill({ json: [] }));
  await page.goto('http://127.0.0.1:4173/about?q=orcas%20votes%3A1..5');
  const toggle = page.getByRole('button', { name: 'Toggle search', exact: true });
  await toggle.click();
  const input = page.getByRole('textbox', { name: 'Search nodes and filters' });
  await input.fill('votes:invalid');
  await page.getByRole('button', { name: 'Clear search and filters', exact: true }).click();
  await expect(input).toHaveValue('');
  await expect(input).toBeFocused();
  await expect(
    page.getByRole('button', { name: 'Clear search and filters', exact: true }),
  ).toHaveCount(0);
  expect(await page.evaluate(() => localStorage.getItem('atlas.discovery.query.v1'))).toBeNull();
  await page.getByRole('button', { name: 'Filters', exact: true }).click();
  await expect(page.getByRole('spinbutton', { name: 'Minimum votes', exact: true })).toHaveValue(
    '',
  );
  await expect(page.getByRole('spinbutton', { name: 'Maximum votes', exact: true })).toHaveValue(
    '',
  );
});

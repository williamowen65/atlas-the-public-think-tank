import { expect, test } from '@playwright/test';
import { searchIndicators } from '../src/lib/state/searchIndicators';

test('search hints distinguish text, filters and inert query tokens', () => {
  expect(searchIndicators('')).toEqual({ hasText: false, hasFilters: false });
  expect(searchIndicators('orcas')).toEqual({ hasText: true, hasFilters: false });
  expect(searchIndicators('type:Idea')).toEqual({ hasText: false, hasFilters: true });
  expect(searchIndicators('"healthy oceans" votes:0..5')).toEqual({
    hasText: true,
    hasFilters: true,
  });
  expect(searchIndicators('archived:false votes:..')).toEqual({
    hasText: false,
    hasFilters: false,
  });
  expect(searchIndicators('archived:true')).toEqual({ hasText: false, hasFilters: true });
  expect(searchIndicators('unknown:word')).toEqual({ hasText: true, hasFilters: false });
});

test('header hints track applied queries while search is closed, excluding unsaved edits', async ({
  page,
}) => {
  await page.route('**/atlas-session-api/**', (route) => route.fulfill({ status: 401, json: {} }));
  await page.route('**/atlas-public-api/**', (route) => route.fulfill({ json: [] }));
  await page.goto('http://127.0.0.1:4173/about?q=orcas%20votes%3A1..5');
  const toggle = page.getByRole('button', { name: 'Toggle search', exact: true });
  await expect(toggle.locator('.text-indicator')).toBeVisible();
  await expect(toggle.locator('.filter-indicator')).toBeVisible();
  await expect(toggle).toHaveAccessibleDescription('Text search applied. Filters applied');
  await toggle.click();
  const input = page.getByRole('textbox', { name: 'Search nodes and filters' });
  await input.fill('');
  await expect(toggle.locator('.filter-indicator')).toBeVisible();
  await page.getByRole('button', { name: 'Search', exact: true }).click();
  await expect(toggle.locator('.search-indicator')).toHaveCount(0);
  await input.fill('salmon');
  await page.getByRole('button', { name: 'Search', exact: true }).click();
  await expect(toggle.locator('.text-indicator')).toBeVisible();
  await expect(toggle.locator('.filter-indicator')).toHaveCount(0);
  await toggle.click();
  await expect(input).toHaveCount(0);
  await expect(toggle.locator('.text-indicator')).toBeVisible();
});

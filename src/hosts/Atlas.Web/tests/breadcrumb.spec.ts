import AxeBuilder from '@axe-core/playwright';
import { expect, test } from '@playwright/test';

const ribbon = 'http://127.0.0.1:6006/iframe.html?id=components-breadcrumb--playground&viewMode=story';
const card = 'http://127.0.0.1:6006/iframe.html?id=compositions-node-voting-preview--playground&viewMode=story';

test('ribbon shows newest ancestor first, scrolls to older items and fits mobile', async ({ page }) => {
  await page.setViewportSize({ width: 320, height: 800 });
  await page.goto(ribbon);
  const nav = page.getByRole('navigation', { name: 'Node breadcrumb' });
  const viewport = nav.locator('.viewport');
  await expect(viewport).toBeVisible();
  await expect.poll(() => viewport.evaluate(el => el.scrollWidth - el.clientWidth - el.scrollLeft)).toBeLessThanOrEqual(1);
  expect(await viewport.evaluate(el => el.scrollWidth > el.clientWidth)).toBe(true);
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(320);
  await nav.getByRole('button', { name: 'Local restoration projects', exact: true }).click();
  await expect(page.getByRole('status')).toHaveText('Local restoration projects');
  // Reaching the left edge requests one bounded page, then keeps existing items anchored.
  await viewport.evaluate(el => { el.scrollLeft = 0; });
  await expect(nav.getByRole('button', { name: 'Healthy oceans', exact: true })).toHaveCount(1);
  await expect.poll(() => viewport.evaluate(el => el.scrollLeft)).toBeGreaterThan(8);
  await viewport.evaluate(el => { el.scrollLeft = 0; });
  await expect(nav.getByRole('button', { name: 'What future is worth building?', exact: true })).toHaveCount(1);
  await expect(nav.getByRole('button', { name: 'Load older ancestors', exact: true })).toHaveCount(0);
  // Keyboard focus reveals offscreen ancestors and retains native activation.
  await nav.getByRole('button').first().focus();
  await page.keyboard.press('Enter');
  await expect(page.getByRole('status')).toHaveText('What future is worth building?');
  await expect.poll(() => viewport.evaluate(el => el.scrollLeft)).toBeLessThan(10);
  await viewport.evaluate(el => { el.scrollLeft = el.scrollWidth; });
  await page.setViewportSize({ width: 420, height: 800 });
  await expect.poll(() => viewport.evaluate(el => el.scrollWidth - el.clientWidth - el.scrollLeft)).toBeLessThanOrEqual(1);
  const results = await new AxeBuilder({ page }).include('#storybook-root').analyze();
  expect(results.violations).toEqual([]);
});

test('card ribbon navigates ancestors; original presentation remains usable', async ({ page }) => {
  await page.goto(`${card}&args=selectedNode:issue`);
  const nav = page.getByRole('navigation', { name: 'Node breadcrumb' });
  await nav.getByRole('button', { name: 'What ideas could help make the world better?', exact: true }).click();
  await expect(page.getByRole('heading', { name: 'What ideas could help make the world better?', exact: true })).toBeVisible();
  await expect(nav).toHaveCount(0);
  await page.goto(`${card}&args=presentation:current;selectedNode:issue`);
  await expect(page.locator('article')).toHaveAttribute('data-presentation', 'current');
  await expect(page.locator('article .breadcrumb').first()).toContainText('What ideas could help make the world better?');
  await page.getByRole('button', { name: 'Read more' }).click();
  await expect(page.getByRole('button', { name: 'Show less' })).toBeVisible();
});

import AxeBuilder from '@axe-core/playwright';
import { expect, test } from '@playwright/test';

// The iframe URL opens just the rendered story, without the Storybook sidebar.
const story = (args = '') =>
  `http://127.0.0.1:6006/iframe.html?id=components-actionbutton--playground&viewMode=story&args=${args}`;

/** Given an enabled isolated story, keyboard activation invokes its callback. */
test('enabled story responds to keyboard activation', async ({ page }) => {
  await page.goto(story());
  const button = page.getByRole('button', { name: 'Try action' });
  await button.focus();
  await page.keyboard.press('Enter');
  await expect(page.getByRole('status')).toHaveText('Action completed');
});

/** Given a disabled story, even an attempted click must not invoke its callback. */
test('disabled story rejects activation', async ({ page }) => {
  await page.goto(story('disabled:true'));
  const button = page.getByRole('button', { name: 'Try action' });
  await expect(button).toBeDisabled();
  await button.click({ force: true });
  await expect(page.getByRole('status')).toHaveText('No action yet');
});

/** Long fixture text remains within a narrow viewport instead of overflowing. */
test('long label fits a narrow viewport', async ({ page }) => {
  await page.setViewportSize({ width: 320, height: 640 });
  await page.goto(story('label:Explore another way to understand this issue together'));
  await expect(page.getByRole('button')).toBeVisible();
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(320);
});

/** The integration page imports the same component and receives its callback. */
test('shared component works in the web composition', async ({ page }) => {
  await page.goto('http://127.0.0.1:4173/components');
  await page.getByRole('button', { name: 'Try action' }).click();
  await expect(page.getByRole('status')).toHaveText('Action completed');
});

/** Given the enabled fixture, automated accessibility rules find no violations. */
test('enabled story passes automated accessibility checks', async ({ page }) => {
  await page.goto(story());
  await expect(page.getByRole('button', { name: 'Try action' })).toBeVisible();
  const results = await new AxeBuilder({ page }).include('#storybook-root').analyze();
  expect(results.violations).toEqual([]);
});

/** Client-side links retain the document; Back and direct URLs resolve routes. */
test('SvelteKit routes support client navigation, Back and direct entry', async ({ page }) => {
  await page.goto('http://127.0.0.1:4173/components');
  await expect(page.getByRole('button', { name: 'Try action' })).toBeVisible();

  // A full document reload would erase this marker; client navigation keeps it.
  await page.evaluate(() => { document.documentElement.dataset.navigationProbe = 'retained'; });
  await page.getByRole('link', { name: 'About this foundation' }).click();
  await expect(page).toHaveURL('http://127.0.0.1:4173/about');
  await expect(page.getByRole('heading', { name: 'About this foundation' })).toBeVisible();
  await expect(page.locator('html')).toHaveAttribute('data-navigation-probe', 'retained');

  await page.goBack();
  await expect(page.getByRole('heading', { name: 'Atlas component integration' })).toBeVisible();
  await page.goto('http://127.0.0.1:4173/about');
  await expect(page.getByRole('heading', { name: 'About this foundation' })).toBeVisible();
});

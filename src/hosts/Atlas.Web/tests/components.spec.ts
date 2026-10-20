import AxeBuilder from '@axe-core/playwright';
import { expect, test } from '@playwright/test';

// The iframe URL opens just the rendered story, without the Storybook sidebar.
const story = (name: string) =>
  `http://127.0.0.1:6006/iframe.html?id=foundation-actionbutton--${name}&viewMode=story`;

/** Given an enabled isolated story, keyboard activation invokes its callback. */
test('enabled story responds to keyboard activation', async ({ page }) => {
  await page.goto(story('enabled'));
  const button = page.getByRole('button', { name: 'Try action' });
  await button.focus();
  await page.keyboard.press('Enter');
  await expect(page.getByRole('status')).toHaveText('Deliberately incorrect verification expectation');
});

/** Given a disabled story, even an attempted click must not invoke its callback. */
test('disabled story rejects activation', async ({ page }) => {
  await page.goto(story('disabled'));
  const button = page.getByRole('button', { name: 'Try action' });
  await expect(button).toBeDisabled();
  await button.click({ force: true });
  await expect(page.getByRole('status')).toHaveText('No action yet');
});

/** Long fixture text remains within a narrow viewport instead of overflowing. */
test('long label fits a narrow viewport', async ({ page }) => {
  await page.setViewportSize({ width: 320, height: 640 });
  await page.goto(story('long-label'));
  await expect(page.getByRole('button')).toBeVisible();
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(320);
});

/** The integration page imports the same component and receives its callback. */
test('shared component works in the web composition', async ({ page }) => {
  await page.goto('http://127.0.0.1:4173');
  await page.getByRole('button', { name: 'Try action' }).click();
  await expect(page.getByRole('status')).toHaveText('Action completed');
});

/** Given the enabled fixture, automated accessibility rules find no violations. */
test('enabled story passes automated accessibility checks', async ({ page }) => {
  await page.goto(story('enabled'));
  await expect(page.getByRole('button', { name: 'Try action' })).toBeVisible();
  const results = await new AxeBuilder({ page }).include('#storybook-root').analyze();
  expect(results.violations).toEqual([]);
});

import AxeBuilder from '@axe-core/playwright';
import { expect, test } from '@playwright/test';

const story = (id: string, globals = '') =>
  `http://127.0.0.1:6006/iframe.html?id=${id}&viewMode=story${globals ? `&globals=${globals}` : ''}`;

/** Given the standard fixture, keyboard changes stay in range and notify its parent. */
test('standard voting supports keyboard, Home and End', async ({ page }) => {
  await page.goto(story('components-votingdial--standard'));
  const dial = page.getByRole('spinbutton', { name: 'Node importance', exact: true });
  await dial.focus();
  await page.keyboard.press('ArrowUp');
  await expect(dial).toHaveAttribute('aria-valuenow', '6');
  await expect(page.getByRole('status')).toHaveText('Preview selection: 6 out of 10');
  await page.keyboard.press('End');
  await page.keyboard.press('ArrowUp');
  await expect(dial).toHaveAttribute('aria-valuenow', '10');
  await page.keyboard.press('Home');
  await page.keyboard.press('ArrowDown');
  await expect(dial).toHaveAttribute('aria-valuenow', '0');
});

/** Null is visibly unrated; explicitly selecting zero becomes a real selection. */
test('unrated is distinct from zero', async ({ page }) => {
  await page.goto(story('components-votingdial--unrated'));
  const dial = page.getByRole('spinbutton');
  await expect(dial).toHaveAttribute('aria-valuetext', /Unrated/);
  await expect(page.getByRole('status')).toHaveText('No rating selected');
  await dial.focus();
  await page.keyboard.press('Home');
  await expect(page.getByRole('status')).toHaveText('Preview selection: 0 out of 10');
});

/** Compact buttons retain the full control contract on a narrow card. */
test('compact voting works without horizontal overflow', async ({ page }) => {
  await page.setViewportSize({ width: 320, height: 700 });
  await page.goto(story('components-votingdial--compact'));
  await page.getByRole('button', { name: 'Increase Node importance' }).click();
  await expect(page.getByRole('status')).toHaveText('Preview selection: 6 out of 10');
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(320);
});

/** Disabled buttons and keyboard input must not change the displayed value. */
test('disabled voting remains unchanged', async ({ page }) => {
  await page.goto(story('components-votingdial--disabled'));
  const button = page.getByRole('button', { name: 'Increase Node importance' });
  await expect(button).toBeDisabled();
  await button.click({ force: true });
  await page.getByRole('spinbutton').dispatchEvent('keydown', { key: 'ArrowUp' });
  await expect(page.getByRole('status')).toHaveText('Preview selection: 5 out of 10');
});

/** Two mounted controls own independent values and contain no repeated style blocks. */
test('multiple dials are independent and share emitted styles', async ({ page }) => {
  await page.goto(story('compositions-voting-comparison--independent-selections'));
  await page.getByRole('button', { name: 'Increase First node importance' }).click();
  await expect(page.getByRole('spinbutton', { name: 'First node importance', exact: true })).toHaveAttribute('aria-valuenow', '5');
  await expect(page.getByRole('spinbutton', { name: 'Second node importance', exact: true })).toHaveAttribute('aria-valuenow', '0');
  await expect(page.locator('.voting-dial')).toHaveCount(2);
  await expect(page.locator('.voting-dial style')).toHaveCount(0);
});

/** Preview globals apply the same token definitions to layouts and components. */
test('global theme and density apply across story types', async ({ page }) => {
  for (const id of ['styles-and-layout-playground--layout', 'components-votingdial--standard']) {
    await page.goto(story(id, 'theme:dark;density:compact'));
    await expect(page.locator('html')).toHaveAttribute('data-theme', 'dark');
    await expect(page.locator('html')).toHaveAttribute('data-density', 'compact');
    expect(await page.evaluate(() => getComputedStyle(document.documentElement).getPropertyValue('--atlas-space').trim())).toBe('.5rem');
    const results = await new AxeBuilder({ page }).include('#storybook-root').analyze();
    expect(results.violations).toEqual([]);
  }
});

/** The card composition receives a dial selection without any live API. */
test('card composition observes voting callbacks', async ({ page }) => {
  await page.goto(story('compositions-node-voting-preview--standard'));
  await page.getByRole('button', { name: 'Increase Node importance' }).click();
  await expect(page.getByRole('status')).toHaveText('Preview selection: 5 out of 10');
});

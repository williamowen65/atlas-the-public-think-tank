import { expect, test } from '@playwright/test';

const overview = (globals = '') =>
  `http://127.0.0.1:6006/iframe.html?id=styles-and-layout-atlas-design-system--overview&viewMode=story${globals ? `&globals=${globals}` : ''}`;

test('design system overview uses real Atlas components without a second controls playground', async ({
  page,
}) => {
  await page.goto(overview());

  await expect(page.getByRole('heading', { name: 'Atlas design system' })).toBeVisible();
  await expect(page.locator('.swatch-card')).toHaveCount(8);
  await expect(page.getByRole('button', { name: 'Primary action' })).toBeEnabled();
  await expect(page.getByRole('button', { name: 'Disabled action' })).toBeDisabled();
  await expect(page.locator('article[data-presentation="ribbon-corner"]')).toHaveCount(1);

  await page.getByRole('button', { name: 'Primary action' }).click();
  await expect(page.getByRole('status').last()).toHaveText('Primary action activated (preview only).');
});

test('typography inventory reads the computed styles of the real node card', async ({ page }) => {
  await page.goto(overview());

  await expect(page.getByRole('heading', { name: 'Typography actually used in Atlas' })).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Question: Understanding a complex issue' })).toHaveCount(0);

  const styles = [
    ['Node title', '.node-card > header > h2'],
    ['Author / date', '.node-card > header > .byline'],
    ['Short description', '.node-card .description'],
    ['Contribution invitation', '.node-card .invitation'],
    ['Card details', '.node-card > .card-details'],
  ] as const;

  for (const width of [1100, 360]) {
    await page.setViewportSize({ width, height: 850 });
    for (const [name, selector] of styles) {
      const measured = await page.locator('.card-example').locator(selector).evaluate((element) => {
        const computed = getComputedStyle(element);
        return [computed.fontSize, computed.fontWeight, computed.lineHeight, computed.marginBottom];
      });
      const row = page.locator('.type-table tbody tr').filter({ has: page.getByRole('rowheader', { name: new RegExp(name.replace('/', '\\/')) }) });
      await expect(row.locator('td')).toHaveText(measured);
    }
  }
});

test('design reference displays CSS values from the selected Storybook theme', async ({ page }) => {
  for (const globals of ['theme:light;density:comfortable', 'theme:dark;density:compact']) {
    await page.goto(overview(globals));
    const [theme, density] = globals.split(';').map((entry) => entry.split(':')[1]);
    await expect(page.locator('html')).toHaveAttribute('data-theme', theme);
    await expect(page.locator('html')).toHaveAttribute('data-density', density);

    const accent = await page.locator('html').evaluate((root) =>
      getComputedStyle(root).getPropertyValue('--atlas-accent').trim(),
    );
    await expect(
      page.locator('.swatch-card').filter({ has: page.locator('code', { hasText: '--atlas-accent' }) }).locator('.value'),
    ).toHaveText(accent);

    const spacing = await page.locator('html').evaluate((root) =>
      getComputedStyle(root).getPropertyValue('--atlas-space').trim(),
    );
    await expect(
      page.locator('.token-row').filter({ has: page.locator('code', { hasText: '--atlas-space' }) }),
    ).toContainText(spacing);
  }
});

import { expect, test } from '@playwright/test';

for (const authenticated of [false, true]) {
  test(`header search pill stays separate from navigation across widths (${authenticated ? 'signed in' : 'signed out'})`, async ({
    page,
  }) => {
    await page.route('**/atlas-session-api/**', (route) =>
      route.fulfill({
        status: authenticated ? 200 : 401,
        json: { participantId: authenticated ? 'participant' : null },
      }),
    );
    await page.route('**/atlas-public-api/**', (route) =>
      route.fulfill({
        json: new URL(route.request().url()).pathname.includes('/participants/')
          ? { displayName: 'Header tester' }
          : [],
      }),
    );
    for (const width of [320, 600, 780, 980, 1180]) {
      await page.setViewportSize({ width, height: 850 });
      await page.goto('http://127.0.0.1:4173/about');
      const header = page.locator('.atlas-header');
      if (authenticated) await expect(header.locator('.account .avatar')).toHaveCount(1);
      const brand = header.getByRole('link', { name: 'Atlas home', exact: true });
      await expect(brand).toBeVisible();
      const search = header.getByRole('textbox', { name: 'Search nodes and filters' });
      await expect(search).toHaveCount(0);
      const toggle = header.getByRole('button', { name: 'Toggle search', exact: true });
      await expect(toggle).toHaveAttribute('aria-expanded', 'false');
      const brandBox = (await brand.boundingBox())!;
      const toggleBox = (await toggle.boundingBox())!;
      expect(brandBox.x + brandBox.width).toBeLessThanOrEqual(toggleBox.x + 1);
      await toggle.click();
      await expect(search).toBeVisible();
      await expect(search).toBeFocused();
      await expect(toggle).toHaveAttribute('aria-expanded', 'true');
      expect((await search.boundingBox())!.width).toBeGreaterThan(30);
      const pill = await header.locator('.search-line').boundingBox();
      const navigation = await header.locator('.right-group').boundingBox();
      expect(pill && navigation).toBeTruthy();
      const overlapX =
        Math.min(pill!.x + pill!.width, navigation!.x + navigation!.width) -
        Math.max(pill!.x, navigation!.x);
      const overlapY =
        Math.min(pill!.y + pill!.height, navigation!.y + navigation!.height) -
        Math.max(pill!.y, navigation!.y);
      expect(overlapX <= 1 || overlapY <= 1).toBe(true);
      expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(
        width,
      );
      await search.fill('orcas');
      await header.getByRole('button', { name: 'Search', exact: true }).click();
      expect(await page.evaluate(() => localStorage.getItem('atlas.discovery.query.v1'))).toBe(
        'orcas',
      );
      const filters = header.getByRole('button', { name: 'Filters', exact: true });
      await filters.focus();
      await page.keyboard.press('Enter');
      await expect(filters).toHaveAttribute('aria-expanded', 'true');
      await expect(header.getByRole('combobox', { name: /^Node type/ })).toBeVisible();
      await page.keyboard.press('Escape');
      await expect(filters).toHaveAttribute('aria-expanded', 'false');
      // Escape closes the outer disclosure after the toolbox has closed.
      await page.keyboard.press('Escape');
      await expect(search).toHaveCount(0);
      await expect(toggle).toHaveAttribute('aria-expanded', 'false');
      await expect(toggle).toBeFocused();
      await toggle.click();
      await expect(search).toHaveValue('orcas');
      await toggle.click();
      await expect(search).toHaveCount(0);
      if (width > 560)
        await expect(
          header.getByRole('navigation', { name: 'Main navigation', exact: true }),
        ).toBeVisible();
    }
  });
}

test('collapsed header still restores a URL search before its input is opened', async ({
  page,
}) => {
  await page.route('**/atlas-session-api/**', (route) => route.fulfill({ status: 401, json: {} }));
  await page.route('**/atlas-public-api/**', (route) => route.fulfill({ json: [] }));
  await page.goto('http://127.0.0.1:4173/about?q=salmon');
  const input = page.getByRole('textbox', { name: 'Search nodes and filters' });
  await expect(input).toHaveCount(0);
  await expect
    .poll(() => page.evaluate(() => localStorage.getItem('atlas.discovery.query.v1')))
    .toBe('salmon');
  await page.getByRole('button', { name: 'Toggle search', exact: true }).click();
  await expect(input).toHaveValue('salmon');
});

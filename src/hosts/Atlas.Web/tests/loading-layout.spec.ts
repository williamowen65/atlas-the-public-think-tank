import { expect, test } from '@playwright/test';

for (const view of ['discover', 'communities', 'node'] as const) {
  test(`${view} keeps a stable shell while awaiting fresh data and removes loading space afterward`, async ({
    page,
  }) => {
    let release!: () => void;
    const pending = new Promise<void>((resolve) => {
      release = resolve;
    });
    await page.setViewportSize({ width: 1100, height: 800 });
    await page.route('**/atlas-session-api/**', (route) =>
      route.fulfill({ status: 401, json: {} }),
    );
    await page.route('**/atlas-public-api/**', async (route) => {
      const url = new URL(route.request().url());
      if (url.pathname.endsWith('/catalog/node-types')) return route.fulfill({ json: [] });
      if (url.pathname.endsWith('/nodes') || url.pathname.endsWith('/communities')) {
        await pending;
        return route.fulfill({
          json: url.pathname.endsWith('/communities')
            ? []
            : { items: [], totalCount: 0, page: 1, pageSize: 3, hasNextPage: false },
        });
      }
      return route.fulfill({ json: [] });
    });
    await page.goto(
      `http://127.0.0.1:4173${view === 'communities' ? '/communities' : view === 'node' ? '/nodes?id=b1000000-0000-4000-8000-000000000001' : '/'}`,
    );
    const placeholder = page.locator('.loading-placeholder');
    await expect(placeholder).toBeVisible();
    await expect(placeholder).toHaveAttribute('aria-busy', 'true');
    const width = await page
      .locator('main')
      .evaluate((element) => element.getBoundingClientRect().width);
    expect(
      await page.locator('main').evaluate((element) => element.getBoundingClientRect().height),
    ).toBeGreaterThan(250);
    release();
    await expect(placeholder).toHaveCount(0);
    await expect(
      page.getByText(
        view === 'communities'
          ? 'No communities match your search.'
          : view === 'node'
            ? 'This node is unavailable or could not be loaded.'
            : 'No nodes found.',
        { exact: true },
      ),
    ).toBeVisible();
    expect(
      await page.locator('main').evaluate((element) => element.getBoundingClientRect().width),
    ).toBeCloseTo(width, 0);
  });
}

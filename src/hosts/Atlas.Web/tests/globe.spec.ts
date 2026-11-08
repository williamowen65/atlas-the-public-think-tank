import { expect, test, type Page } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';

const snapshot = {
  live: {
    visitors: 2,
    since: '2026-11-08T00:00:00Z',
    pins: [{ longitude: -122, latitude: 48, visitors: 1 }],
  },
  historical: {
    visitors: 12,
    since: '2026-11-01T00:00:00Z',
    pins: [{ longitude: -122, latitude: 48, visitors: 3 }],
  },
};
async function mockApi(page: Page, offline = false, hasLocation = false) {
  // Hold a deterministic listener open; actual SSE transport is covered by GlobeHttpTests.
  await page.addInitScript(
    ({ snapshot, offline }) => {
      class GlobeEvents {
        onmessage: ((event: MessageEvent) => void) | null = null;
        onerror: (() => void) | null = null;
        closed = false;
        constructor() {
          setTimeout(() => {
            if (this.closed) return;
            if (offline) this.onerror?.();
            else this.onmessage?.(new MessageEvent('message', { data: JSON.stringify(snapshot) }));
          }, 100);
        }
        close() {
          this.closed = true;
        }
      }
      Object.defineProperty(window, 'EventSource', { value: GlobeEvents });
    },
    { snapshot, offline },
  );
  await page.route('**/atlas-session-api/**', async (route) => {
    const url = new URL(route.request().url());
    if (url.pathname.endsWith('/auth/csrf'))
      return route.fulfill({ json: { token: 'test-token' } });
    if (url.pathname.endsWith('/globe/visit')) return route.fulfill({ json: { hasLocation } });
    if (url.pathname.endsWith('/globe/events'))
      return offline
        ? route.abort()
        : route.fulfill({
            contentType: 'text/event-stream',
            body: `data: ${JSON.stringify(snapshot)}\n\n`,
          });
    if (url.pathname.endsWith('/globe/location')) return route.fulfill({ status: 204 });
    return route.fulfill({ status: 401, json: {} });
  });
  await page.route('**/atlas-public-api/**', (route) =>
    route.fulfill({ json: { items: [], totalCount: 0 } }),
  );
}

test('desktop mini opens compact globe, toggles history, and restores focus on Escape', async ({
  page,
}) => {
  await mockApi(page);
  await page.goto('/');
  const launcher = page
    .getByRole('button', { name: 'Open Atlas visitor globe', exact: true })
    .last();
  await launcher.click();
  const dialog = page.getByRole('dialog', { name: 'Visitor globe' });
  await expect(dialog).toBeVisible();
  await expect(dialog.getByRole('button', { name: 'Live', exact: true })).toHaveAttribute(
    'aria-pressed',
    'true',
  );
  await dialog.getByRole('button', { name: 'Historical', exact: true }).click();
  await expect(dialog.getByRole('button', { name: 'Historical', exact: true })).toHaveAttribute(
    'aria-pressed',
    'true',
  );
  await expect(dialog.locator('.metric strong')).toHaveText('12');
  await page.screenshot({ path: test.info().outputPath('desktop-globe.png') });
  await dialog.getByRole('button', { name: 'About this globe', exact: true }).click();
  await dialog.getByText('Shared locations', { exact: false }).click();
  await expect(dialog.getByText('Near 48° latitude, -122° longitude: 3 visitors')).toBeVisible();
  expect((await new AxeBuilder({ page }).include('dialog').analyze()).violations).toEqual([]);
  await page.keyboard.press('Escape');
  await expect(dialog).not.toBeVisible();
  await expect(launcher).toBeFocused();
});

test('mobile bottom control opens without overflow or overlapping the dialog', async ({ page }) => {
  await page.setViewportSize({ width: 390, height: 844 });
  await mockApi(page);
  await page.goto('/');
  await page
    .getByRole('navigation', { name: 'Bottom navigation' })
    .getByRole('button', { name: 'Open Atlas visitor globe' })
    .click();
  const dialog = page.getByRole('dialog');
  await expect(dialog).toBeVisible();
  const fits = await dialog.evaluate((element) => element.scrollWidth <= element.clientWidth);
  expect(fits).toBe(true);
  const bounds = await dialog.boundingBox();
  const header = await page.locator('.atlas-header').boundingBox();
  const nav = await page.getByRole('navigation', { name: 'Bottom navigation' }).boundingBox();
  expect(bounds!.y).toBeGreaterThan(header!.y + header!.height);
  expect(bounds!.y + bounds!.height).toBeLessThan(nav!.y);
  expect(await dialog.evaluate((element) => element.scrollHeight <= element.clientHeight)).toBe(
    true,
  );
  await page.screenshot({ path: test.info().outputPath('mobile-globe.png') });
  await dialog.getByRole('button', { name: 'Close visitor globe' }).click();
  await expect(dialog).not.toBeVisible();
});

test('location permission waits for globe consent and only rounded coordinates leave browser', async ({
  page,
  context,
}) => {
  await context.grantPermissions(['geolocation']);
  await context.setGeolocation({ longitude: -122.4321, latitude: 47.6543 });
  await mockApi(page);
  await page.addInitScript(() => {
    const original = navigator.geolocation.getCurrentPosition.bind(navigator.geolocation);
    (window as unknown as { locationCalls: number }).locationCalls = 0;
    navigator.geolocation.getCurrentPosition = (...args) => {
      (window as unknown as { locationCalls: number }).locationCalls++;
      original(...args);
    };
  });
  await page.goto('/');
  await page.getByRole('button', { name: 'Open Atlas visitor globe', exact: true }).last().click();
  await expect
    .poll(() => page.evaluate(() => (window as unknown as { locationCalls: number }).locationCalls))
    .toBe(0);
  await page.getByRole('button', { name: 'Share my location on this globe', exact: true }).click();
  await expect(
    page.getByText('The next step opens your browser’s permission prompt.', { exact: false }),
  ).toBeVisible();
  expect(
    await page.evaluate(() => (window as unknown as { locationCalls: number }).locationCalls),
  ).toBe(0);
  const write = page.waitForRequest(
    (request) => request.url().endsWith('/globe/location') && request.method() === 'PUT',
  );
  await page.getByRole('button', { name: 'Continue: share on this globe' }).click();
  expect((await write).postDataJSON()).toEqual({ longitude: -122, latitude: 48 });
  await expect(
    page.getByText('Your approximate location is shared on the Atlas globe.', { exact: false }),
  ).toBeVisible();
  const remove = page.waitForRequest(
    (request) => request.url().endsWith('/globe/location') && request.method() === 'PUT',
  );
  await page.getByRole('button', { name: 'Remove my shared location' }).click();
  expect((await remove).postDataJSON()).toEqual({ longitude: null, latitude: null });
});

test('unavailable listener has recovery and does not show stale live counts', async ({ page }) => {
  await mockApi(page, true);
  await page.goto('/');
  await page.getByRole('button', { name: 'Open Atlas visitor globe', exact: true }).last().click();
  await expect(page.getByText('Reconnecting…', { exact: true })).toBeVisible();
  await expect(page.locator('.metric strong')).toHaveText('—');
  await expect(page.getByRole('button', { name: 'Retry now' })).toBeEnabled();
});

test('denied location stays optional and never submits coordinates', async ({ page }) => {
  await mockApi(page);
  let writes = 0;
  page.on('request', (request) => {
    if (request.url().endsWith('/globe/location')) writes++;
  });
  await page.addInitScript(() => {
    navigator.geolocation.getCurrentPosition = (_success, error) =>
      error?.({
        code: 1,
        message: 'Denied',
        PERMISSION_DENIED: 1,
        POSITION_UNAVAILABLE: 2,
        TIMEOUT: 3,
      });
  });
  await page.goto('/');
  await page.getByRole('button', { name: 'Open Atlas visitor globe', exact: true }).last().click();
  await page.getByRole('button', { name: 'Share my location on this globe', exact: true }).click();
  await page.getByRole('button', { name: 'Continue: share on this globe' }).click();
  await expect(page.getByText('Location was not shared.', { exact: false })).toBeVisible();
  expect(writes).toBe(0);
  await expect(page.getByRole('button', { name: 'Historical', exact: true })).toBeEnabled();
});

test('map failure leaves counts and text locations usable', async ({ page }) => {
  await mockApi(page);
  await page.route('**/globe/land.geojson', (route) => route.abort());
  await page.goto('/');
  await page.getByRole('button', { name: 'Open Atlas visitor globe', exact: true }).last().click();
  await expect(page.getByText('Map details could not load.', { exact: false })).toBeVisible();
  await expect(page.locator('.metric strong')).toHaveText('2');
  await expect(page.getByRole('button', { name: 'Retry map' })).toBeEnabled();
  await page.getByRole('button', { name: 'About this globe', exact: true }).click();
  await page.getByText('Shared locations', { exact: false }).click();
  await expect(page.getByText('Near 48° latitude, -122° longitude: 1 visitor')).toBeVisible();
});

test('miniature and popup share one rotation across opening and reopening', async ({ page }) => {
  await mockApi(page);
  await page.goto('/');
  const launcher = page
    .getByRole('button', { name: 'Open Atlas visitor globe', exact: true })
    .last();
  await expect(launcher.locator('strong, .mini-caption')).toHaveCount(0);
  const dimensions = await launcher.boundingBox();
  expect(dimensions!.width).toBe(dimensions!.height);
  await launcher.click();
  const angles = () =>
    page
      .locator('svg[data-angle]')
      .evaluateAll((elements) => elements.map((element) => element.getAttribute('data-angle')));
  const before = await angles();
  expect(before).toHaveLength(2);
  expect(before[0]).toBe(before[1]);
  await expect.poll(async () => (await angles())[0]).not.toBe(before[0]);
  const moving = await angles();
  expect(moving[0]).toBe(moving[1]);
  await page.keyboard.press('Escape');
  await launcher.click();
  const reopened = await angles();
  expect(reopened[0]).toBe(reopened[1]);
  expect(reopened[0]).not.toBe('-25');
  await expect(page.getByRole('heading', { name: 'Atlas, together' })).toHaveCount(0);
});

test('saved location shows sharing state instead of another share button', async ({ page }) => {
  await mockApi(page, false, true);
  await page.goto('/');
  await page.getByRole('button', { name: 'Open Atlas visitor globe', exact: true }).last().click();
  await expect(page.getByText('Location shared', { exact: true })).toBeVisible();
  await expect(
    page.getByRole('button', { name: 'Share my location on this globe', exact: true }),
  ).toHaveCount(0);
  await page.getByRole('button', { name: 'Remove my shared location' }).click();
  await expect(
    page.getByRole('button', { name: 'Share my location on this globe', exact: true }),
  ).toBeVisible();
});

test('small phone popup fits between site controls and follows the site theme', async ({
  page,
}) => {
  await page.setViewportSize({ width: 320, height: 568 });
  await mockApi(page);
  await page.goto('/');
  await page.evaluate(() => (document.documentElement.dataset.theme = 'dark'));
  await page
    .getByRole('navigation', { name: 'Bottom navigation' })
    .getByRole('button', { name: 'Open Atlas visitor globe' })
    .click();
  const dialog = page.getByRole('dialog', { name: 'Visitor globe' });
  const geometry = await page.evaluate(() => {
    const popup = document.querySelector('dialog.explorer')!;
    return {
      bounds: popup.getBoundingClientRect().toJSON(),
      header: document.querySelector('.atlas-header')!.getBoundingClientRect().toJSON(),
      nav: document.querySelector('.bottom-navigation')!.getBoundingClientRect().toJSON(),
      scrolls: popup.scrollHeight > popup.clientHeight,
    };
  });
  expect(geometry.bounds.y).toBeGreaterThan(geometry.header.bottom);
  expect(geometry.bounds.bottom).toBeLessThan(geometry.nav.y);
  expect(geometry.scrolls).toBe(false);
  await expect(dialog).toHaveCSS('background-color', 'rgb(29, 42, 56)');
});

test('reduced motion keeps both globe renderers still and synchronized', async ({ page }) => {
  await page.emulateMedia({ reducedMotion: 'reduce' });
  await mockApi(page);
  await page.goto('/');
  await page.getByRole('button', { name: 'Open Atlas visitor globe', exact: true }).last().click();
  await page.evaluate(
    () =>
      new Promise<void>((resolve) => {
        let frames = 0;
        function next() {
          if (++frames === 8) resolve();
          else requestAnimationFrame(next);
        }
        requestAnimationFrame(next);
      }),
  );
  const angles = await page
    .locator('svg[data-angle]')
    .evaluateAll((elements) => elements.map((element) => element.getAttribute('data-angle')));
  expect(angles).toEqual(['-25', '-25']);
});

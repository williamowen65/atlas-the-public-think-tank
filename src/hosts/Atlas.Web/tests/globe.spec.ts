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
async function mockApi(page: Page, offline = false) {
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
    if (url.pathname.endsWith('/globe/visit'))
      return route.fulfill({ json: { hasLocation: false } });
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

test('desktop mini opens full globe, toggles history, and restores focus on Escape', async ({
  page,
}) => {
  await mockApi(page);
  await page.goto('/');
  const launcher = page
    .getByRole('button', { name: 'Open Atlas visitor globe', exact: true })
    .last();
  await launcher.click();
  const dialog = page.getByRole('dialog', { name: 'Atlas, together' });
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
  await expect(page.getByText('Live connection unavailable.', { exact: false })).toBeVisible();
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
  await page.getByText('Shared locations', { exact: false }).click();
  await expect(page.getByText('Near 48° latitude, -122° longitude: 1 visitor')).toBeVisible();
});

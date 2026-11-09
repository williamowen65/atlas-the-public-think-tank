import { expect, test, type Page } from '@playwright/test';
test.use({
  launchOptions: {
    args: ['--use-angle=swiftshader', '--enable-unsafe-swiftshader'],
    executablePath: process.env.PLAYWRIGHT_CHROMIUM_EXECUTABLE,
  },
});

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
async function mockApi(page: Page, offline = false, hasLocation = false, canShare = true) {
  let saved = hasLocation;
  const view = () => ({
    ...snapshot,
    canShare,
    hasLocation: saved,
    capturedAt: '2026-11-08T12:00:00Z',
    onlineWindowMinutes: 5,
  });
  await page.route('**/atlas-session-api/**', async (route) => {
    const path = new URL(route.request().url()).pathname;
    if (path.endsWith('/auth/csrf')) return route.fulfill({ json: { token: 'test-token' } });
    if (path.endsWith('/globe/snapshot') || path.endsWith('/globe/activity'))
      return offline ? route.fulfill({ status: 503 }) : route.fulfill({ json: view() });
    if (path.endsWith('/globe/location')) {
      saved = route.request().postDataJSON().longitude !== null;
      return route.fulfill({ json: view() });
    }
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
  await expect(dialog.locator('canvas[data-renderer="webgl"]')).toHaveAttribute(
    'data-ready',
    'true',
  );
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
  await expect(dialog.getByText('Near 48° latitude, -122° longitude: 3 accounts')).toBeVisible();
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
  await page.getByLabel('Pin color', { exact: true }).fill('#2468ac');
  await expect(page.getByRole('img', { name: 'Your pin preview: #2468ac' })).toBeVisible();
  await page.screenshot({ path: test.info().outputPath('pin-color-choice.png') });
  const write = page.waitForRequest(
    (request) => request.url().endsWith('/globe/location') && request.method() === 'PUT',
  );
  await page.getByRole('button', { name: 'Continue: share on this globe' }).click();
  expect((await write).postDataJSON()).toEqual({ longitude: -122, latitude: 48, color: '#2468ac' });
  await expect(
    page.getByText('Your approximate location is shared on the Atlas globe.', { exact: false }),
  ).toBeVisible();
  const remove = page.waitForRequest(
    (request) => request.url().endsWith('/globe/location') && request.method() === 'PUT',
  );
  await page.getByRole('button', { name: 'Remove my shared location' }).click();
  expect((await remove).postDataJSON()).toEqual({ longitude: null, latitude: null });
});

test('unavailable snapshot has recovery and does not show stale live counts', async ({ page }) => {
  await mockApi(page, true);
  await page.goto('/');
  await page.getByRole('button', { name: 'Open Atlas visitor globe', exact: true }).last().click();
  await expect(page.getByText('Update unavailable', { exact: true })).toBeVisible();
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
  await expect(page.getByText('Near 48° latitude, -122° longitude: 1 account')).toBeVisible();
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
      .locator('canvas[data-angle]')
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

test('reduced motion keeps miniature and expanded globes still and synchronized', async ({
  page,
}) => {
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
    .locator('canvas[data-angle]')
    .evaluateAll((elements) => elements.map((element) => element.getAttribute('data-angle')));
  expect(angles).toEqual(['-25', '-25']);
});

test('outside clicks dismiss the globe without blocking header navigation', async ({ page }) => {
  await mockApi(page);
  await page.goto('/');
  const launcher = page
    .getByRole('button', { name: 'Open Atlas visitor globe', exact: true })
    .last();
  await launcher.click();
  const popup = page.getByRole('dialog', { name: 'Visitor globe' });
  await expect(popup).toHaveAttribute('aria-modal', 'false');
  await popup.getByRole('button', { name: 'Historical', exact: true }).click();
  await expect(popup).toBeVisible();
  await page.mouse.click(5, 300);
  await expect(popup).not.toBeVisible();
  await launcher.click();
  await page.locator('.atlas-header').getByRole('link', { name: 'Log in', exact: true }).click();
  await expect(page).toHaveURL(/\/login$/);
  await expect(popup).not.toBeVisible();
});

test('backdrop covers the full viewport after scrolling and resizing', async ({ page }) => {
  await mockApi(page);
  await page.goto('/');
  await page.evaluate(() => {
    document.body.style.minHeight = '2500px';
    window.scrollTo(0, 400);
  });
  await page.getByRole('button', { name: 'Open Atlas visitor globe', exact: true }).last().click();
  for (const size of [
    { width: 1280, height: 720 },
    { width: 1000, height: 900 },
    { width: 390, height: 844 },
  ]) {
    await page.setViewportSize(size);
    await expect
      .poll(() =>
        page.locator('.body-shade').evaluate((element) => {
          const r = element.getBoundingClientRect();
          return { x: r.x, y: r.y, right: r.right, bottom: r.bottom };
        }),
      )
      .toEqual({ x: 0, y: 0, right: size.width, bottom: size.height });
    await expect(page.getByRole('dialog', { name: 'Visitor globe' })).toBeVisible();
  }
  await page.mouse.click(2, 100);
  await expect(page.getByRole('dialog', { name: 'Visitor globe' })).not.toBeVisible();
});

test('the launcher toggles the open globe closed', async ({ page }) => {
  await mockApi(page);
  await page.goto('/');
  const launcher = page
    .getByRole('button', { name: 'Open Atlas visitor globe', exact: true })
    .last();
  await launcher.click();
  await expect(page.getByRole('dialog', { name: 'Visitor globe' })).toBeVisible();
  await launcher.click();
  await expect(page.getByRole('dialog', { name: 'Visitor globe' })).not.toBeVisible();
});

test('anonymous viewers only request snapshots and the sign-in link closes the globe', async ({
  page,
}) => {
  await mockApi(page, false, false, false);
  const requests: string[] = [];
  page.on('request', (request) => {
    if (request.url().includes('/atlas-session-api/globe/'))
      requests.push(request.method() + ' ' + new URL(request.url()).pathname);
  });
  await page.goto('/');
  await page.getByRole('button', { name: 'Open Atlas visitor globe', exact: true }).last().click();
  await expect(page.getByRole('link', { name: 'Sign in to place a pin' })).toBeVisible();
  await expect(
    page.getByRole('button', { name: 'Share my location on this globe', exact: true }),
  ).toHaveCount(0);
  expect(
    requests.every((request) => request.startsWith('GET /atlas-session-api/globe/snapshot')),
  ).toBe(true);
  expect(requests.length).toBeGreaterThan(0);
  await page.getByRole('link', { name: 'Sign in to place a pin' }).click();
  await expect(page).toHaveURL(/\/login$/);
  await expect(page.getByRole('dialog', { name: 'Visitor globe' })).toBeHidden();
});

test('visible pages poll using short requests once a minute without event streams', async ({
  page,
}) => {
  await page.clock.install();
  await mockApi(page, false, true, true);
  let activity = 0;
  const streams: string[] = [];
  page.on('request', (request) => {
    if (request.url().endsWith('/globe/activity')) activity++;
    if (/\/globe\/(events|visit)(?:\?|$)/.test(request.url())) streams.push(request.url());
  });
  await page.goto('/');
  await expect.poll(() => activity).toBe(1);
  await page.clock.fastForward(59000);
  expect(activity).toBe(1);
  await page.clock.fastForward(1000);
  await expect.poll(() => activity).toBe(2);
  expect(streams).toEqual([]);
});

test('hidden tabs pause polling and refresh on return', async ({ page }) => {
  await page.clock.install();
  await mockApi(page, false, true, true);
  let activity = 0;
  page.on('request', (request) => {
    if (request.url().endsWith('/globe/activity')) activity++;
  });
  await page.goto('/');
  await expect.poll(() => activity).toBe(1);
  await page.evaluate(() => {
    Object.defineProperty(document, 'hidden', { configurable: true, value: true });
    document.dispatchEvent(new Event('visibilitychange'));
  });
  await page.clock.fastForward(120000);
  expect(activity).toBe(1);
  await page.evaluate(() => {
    Object.defineProperty(document, 'hidden', { configurable: true, value: false });
    document.dispatchEvent(new Event('visibilitychange'));
  });
  await expect.poll(() => activity).toBe(2);
});

test('unsupported WebGL keeps the launcher and textual globe information usable', async ({
  page,
}) => {
  await page.addInitScript(() => {
    const original = HTMLCanvasElement.prototype.getContext;
    HTMLCanvasElement.prototype.getContext = function (
      this: HTMLCanvasElement,
      type: string,
      ...args: unknown[]
    ) {
      if (type === 'webgl2') return null;
      return Reflect.apply(original, this, [type, ...args]);
    } as typeof original;
  });
  await mockApi(page);
  await page.goto('/');
  await page.getByRole('button', { name: 'Open Atlas visitor globe', exact: true }).last().click();
  const dialog = page.getByRole('dialog', { name: 'Visitor globe' });
  await expect(
    dialog.getByRole('status').filter({ hasText: 'WebGL 2 is unavailable' }),
  ).toContainText('WebGL 2 is unavailable');
  await expect(dialog.locator('.metric strong')).toHaveText('2');
  await dialog.getByRole('button', { name: 'Historical', exact: true }).click();
  await expect(dialog.locator('.metric strong')).toHaveText('12');
  await dialog.getByRole('button', { name: 'About this globe', exact: true }).click();
  await dialog.locator('summary').filter({ hasText: 'Shared locations' }).click();
  await expect(dialog.getByText('Near 48° latitude, -122° longitude: 3 accounts')).toBeVisible();
});

test('closing the expanded globe releases its GPU programs', async ({ page }) => {
  await page.addInitScript(() => {
    (window as any).deletedGlobePrograms = 0;
    const original = WebGL2RenderingContext.prototype.deleteProgram;
    WebGL2RenderingContext.prototype.deleteProgram = function (program) {
      (window as any).deletedGlobePrograms++;
      return original.call(this, program);
    };
  });
  await mockApi(page);
  await page.goto('/');
  await page.getByRole('button', { name: 'Open Atlas visitor globe', exact: true }).last().click();
  await expect(page.getByRole('dialog').locator('canvas[data-renderer="webgl"]')).toHaveAttribute(
    'data-ready',
    'true',
  );
  const before = await page.evaluate(() => (window as any).deletedGlobePrograms);
  await page.keyboard.press('Escape');
  await expect
    .poll(() => page.evaluate(() => (window as any).deletedGlobePrograms))
    .toBe(before + 2);
});

test('choose a globe spot previews and saves without browser location access', async ({ page }) => {
  await mockApi(page);
  await page.addInitScript(() => {
    (window as any).manualLocationCalls = 0;
    navigator.geolocation.getCurrentPosition = () => {
      (window as any).manualLocationCalls++;
    };
  });
  const writes: unknown[] = [];
  page.on('request', (request) => {
    if (request.url().endsWith('/globe/location') && request.method() === 'PUT')
      writes.push(request.postDataJSON());
  });
  await page.goto('/');
  await page.getByRole('button', { name: 'Open Atlas visitor globe', exact: true }).last().click();
  await page.getByRole('button', { name: 'Share my location on this globe', exact: true }).click();
  await page.getByRole('button', { name: 'Choose a spot on the globe instead' }).click();
  const canvas = page.getByRole('dialog').locator('canvas[data-angle]');
  const angle = Number(await canvas.getAttribute('data-angle'));
  await page.getByRole('button', { name: 'Choose a location on the globe', exact: true }).click();
  await expect(page.getByLabel('Latitude', { exact: true })).toHaveValue('18');
  const pickedLongitude = Number(await page.getByLabel('Longitude', { exact: true }).inputValue());
  // Pointer clicks can land half a CSS pixel away from the geometric center.
  expect(Math.abs(pickedLongitude + angle)).toBeLessThanOrEqual(1);
  await page.getByLabel('Pin color', { exact: true }).fill('#2468ac');
  expect(writes).toHaveLength(0);
  await expect(canvas).toHaveAttribute('data-angle', String(angle));
  await page.screenshot({ path: test.info().outputPath('manual-pin-preview.png') });
  await page.getByRole('button', { name: 'Save this pin', exact: true }).click();
  await expect.poll(() => writes.length).toBe(1);
  expect(writes[0]).toEqual({ longitude: pickedLongitude, latitude: 18, color: '#2468ac' });
  expect(await page.evaluate(() => (window as any).manualLocationCalls)).toBe(0);
});

test('manual edit preserves saved color and cancel makes no location write', async ({ page }) => {
  await mockApi(page, false, true);
  await page.route('**/atlas-session-api/globe/activity', (route) =>
    route.fulfill({
      json: {
        ...snapshot,
        canShare: true,
        hasLocation: true,
        pinColor: '#123abc',
        capturedAt: '2026-11-08T12:00:00Z',
        onlineWindowMinutes: 5,
      },
    }),
  );
  const writes: unknown[] = [];
  page.on('request', (request) => {
    if (request.url().endsWith('/globe/location') && request.method() === 'PUT')
      writes.push(request.postDataJSON());
  });
  await page.goto('/');
  await page.getByRole('button', { name: 'Open Atlas visitor globe', exact: true }).last().click();
  await page.getByRole('button', { name: 'Edit my pin', exact: true }).click();
  await expect(page.getByLabel('Pin color', { exact: true })).toHaveValue('#123abc');
  await page.getByLabel('Latitude', { exact: true }).fill('-33');
  await page.getByLabel('Longitude', { exact: true }).fill('151');
  await page.getByRole('button', { name: 'Cancel', exact: true }).click();
  expect(writes).toHaveLength(0);
  await expect(page.getByText('Location shared', { exact: true })).toBeVisible();
});

test('mobile manual placement supports valid coordinates without leaving the viewport', async ({
  page,
}) => {
  await page.setViewportSize({ width: 390, height: 844 });
  await mockApi(page);
  await page.goto('/');
  await page.getByRole('button', { name: 'Open Atlas visitor globe', exact: true }).last().click();
  await page.getByRole('button', { name: 'Share my location on this globe', exact: true }).click();
  await page.getByRole('button', { name: 'Choose a spot on the globe instead' }).click();
  const save = page.getByRole('button', { name: 'Save this pin', exact: true });
  await expect(save).toBeDisabled();
  await page.getByLabel('Latitude', { exact: true }).fill('100');
  await page.getByLabel('Longitude', { exact: true }).fill('-122');
  await expect(save).toBeDisabled();
  await page.getByLabel('Latitude', { exact: true }).fill('47');
  await expect(save).toBeEnabled();
  const bounds = await page.getByRole('dialog').boundingBox();
  expect(bounds!.y).toBeGreaterThanOrEqual(0);
  expect(bounds!.y + bounds!.height).toBeLessThanOrEqual(844);
  await page.screenshot({ path: test.info().outputPath('manual-pin-mobile.png') });
  const request = page.waitForRequest(
    (request) => request.url().endsWith('/globe/location') && request.method() === 'PUT',
  );
  await save.click();
  expect((await request).postDataJSON()).toEqual({
    latitude: 47,
    longitude: -122,
    color: '#b73539',
  });
});

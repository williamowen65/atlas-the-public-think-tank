import { expect, test } from '@playwright/test';

// Exercise the real shaders on CI without depending on a host graphics card.
// Software rendering verifies correctness, not device performance.
test.use({
  launchOptions: {
    args: ['--use-angle=swiftshader', '--enable-unsafe-swiftshader'],
    executablePath: process.env.PLAYWRIGHT_CHROMIUM_EXECUTABLE,
  },
});

const story =
  'http://127.0.0.1:6006/iframe.html?id=components-visitorglobe--playground&viewMode=story';

test('WebGL draws instanced pins and rotates without uploading geometry or map each frame', async ({
  page,
}) => {
  await page.emulateMedia({ reducedMotion: 'no-preference' });
  await page.addInitScript(() => {
    const counters = { buffers: 0, maps: 0, instances: 0 };
    (window as unknown as { gpuCounters: typeof counters }).gpuCounters = counters;
    const prototype = WebGL2RenderingContext.prototype;
    for (const [method, key] of [
      ['bufferData', 'buffers'],
      ['texImage2D', 'maps'],
      ['drawArraysInstanced', 'instances'],
    ] as const) {
      const original = prototype[method];
      Object.defineProperty(prototype, method, {
        value: function (...args: unknown[]) {
          counters[key]++;
          return Reflect.apply(original, this, args);
        },
      });
    }
  });
  const errors: string[] = [];
  page.on('pageerror', (error) => errors.push(error.message));
  await page.goto(story + '&args=pinCount:10000;targetFps:30');
  const canvas = page.locator('canvas[data-renderer="webgl"]');
  await expect(canvas).toHaveAttribute('data-ready', 'true');
  await expect(page.locator('[data-demo-count]')).toHaveAttribute('data-demo-count', '10000');
  // Wait for the asynchronous land texture before checking steady-state uploads.
  await expect
    .poll(() => page.evaluate(() => (window as any).gpuCounters.maps))
    .toBeGreaterThanOrEqual(2);
  const before = await page.evaluate(() => ({ ...(window as any).gpuCounters }));
  const angle = await canvas.getAttribute('data-angle');
  await expect(canvas).not.toHaveAttribute('data-angle', angle!);
  await expect
    .poll(() => page.evaluate(() => (window as any).gpuCounters.instances))
    .toBeGreaterThan(before.instances + 4);
  const after = await page.evaluate(() => ({ ...(window as any).gpuCounters }));
  expect(after.buffers).toBe(before.buffers);
  expect(after.maps).toBe(before.maps);
  expect(
    await canvas.evaluate((element: HTMLCanvasElement) => element.getContext('webgl2')!.getError()),
  ).toBe(0);
  await page.screenshot({ path: test.info().outputPath('globe-webgl-stress.png') });
  expect(errors).toEqual([]);
});

test('WebGL preserves angle controls', async ({ page }) => {
  await page.goto(story + '&args=animate:!false;angle:40;pinCount:100');
  await expect(page.locator('canvas[data-renderer="webgl"]')).toHaveAttribute('data-ready', 'true');
  await expect(page.locator('canvas[data-renderer="webgl"]')).toHaveAttribute('data-angle', '40');
});

test('unavailable WebGL leaves an accessible explanation', async ({ page }) => {
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
  await page.goto(story + '&args=animate:!false');
  await expect(page.getByRole('status')).toContainText('WebGL 2 is unavailable');
});

test('WebGL recovers from context loss', async ({ page }) => {
  await page.goto(story + '&args=animate:!false');
  const canvas = page.locator('canvas[data-renderer="webgl"]');
  await expect(canvas).toHaveAttribute('data-ready', 'true');
  await canvas.evaluate((element: HTMLCanvasElement) => {
    const extension = element.getContext('webgl2')!.getExtension('WEBGL_lose_context')!;
    (window as any).restoreGlobeContext = () => extension.restoreContext();
    extension.loseContext();
  });
  await expect(page.getByRole('status')).toContainText('context was lost');
  await page.evaluate(() => (window as any).restoreGlobeContext());
  await expect(canvas).toHaveAttribute('data-ready', 'true');
  await expect(page.getByRole('status')).toHaveCount(0);
});

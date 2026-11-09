import { expect, test } from '@playwright/test';
test.use({
  launchOptions: {
    args: ['--use-angle=swiftshader', '--enable-unsafe-swiftshader'],
    executablePath: process.env.PLAYWRIGHT_CHROMIUM_EXECUTABLE,
  },
});
const story =
  'http://127.0.0.1:6006/iframe.html?id=components-visitorglobe--playground&viewMode=story';
for (const variant of [
  'playground',
  'paper',
  'night',
  'miniature',
  'miniature-sparse',
  'miniature-busy',
  'empty',
  'sparse-worldwide',
  'busy-worldwide',
  'dense-worldwide',
  'clustered',
  'custom-pins',
  'flat-heads',
  'stems-only',
  'equator-and-horizon',
  'stress',
  'uniform-stress',
]) {
  test(`WebGL ${variant} story renders without an API`, async ({ page }) => {
    const apiCalls: string[] = [];
    page.on('request', (request) => {
      if (/atlas-(public|session)-api/.test(request.url())) apiCalls.push(request.url());
    });
    await page.goto(story.replace('--playground', `--${variant}`) + '&args=animate:!false');
    await expect(page.locator('canvas[data-renderer="webgl"]')).toHaveAttribute(
      'data-ready',
      'true',
    );
    await page.screenshot({ path: test.info().outputPath(`globe-${variant}.png`) });
    expect(apiCalls).toEqual([]);
  });
}
test('pin count and update rate controls remain available', async ({ page }) => {
  await page.emulateMedia({ reducedMotion: 'no-preference' });
  await page.goto(story + '&args=pinCount:10000;targetFps:10');
  await expect(page.locator('[data-demo-count]')).toHaveAttribute('data-demo-count', '10000');
  await expect(page.locator('[data-performance]')).toContainText('target 10 FPS');
  await expect
    .poll(async () =>
      Number(
        (await page.locator('[data-performance]').innerText()).match(/(\d+) rotation updates/)?.[1],
      ),
    )
    .toBeGreaterThan(0);
});

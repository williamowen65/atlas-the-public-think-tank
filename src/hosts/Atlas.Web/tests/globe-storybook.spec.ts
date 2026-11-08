import { expect, test } from '@playwright/test';

const story =
  'http://127.0.0.1:6006/iframe.html?id=components-visitorglobe--playground&viewMode=story';

test('globe story controls change colors, grid and angle without calling an API', async ({
  page,
}) => {
  const apiCalls: string[] = [];
  page.on('request', (request) => {
    if (/atlas-(public|session)-api/.test(request.url())) apiCalls.push(request.url());
  });
  await page.goto(
    story +
      '&args=animate:!false;angle:40;oceanColor:!hex(abcdef);landColor:!hex(654321);showGrid:!false',
  );
  await expect(page.locator('svg[data-angle]')).toHaveAttribute('data-angle', '40');
  await expect(page.locator('.land')).toHaveAttribute('fill', '#654321');
  await expect(page.locator('radialGradient stop').first()).toHaveAttribute(
    'stop-color',
    '#abcdef',
  );
  await expect(page.locator('.graticule')).toHaveCount(0);
  await expect(page.locator('.pin-stem').first()).toBeVisible();
  await expect(page.locator('.map-pin circle')).toHaveCount(0);
  expect(apiCalls).toEqual([]);
});

for (const variant of [
  'playground',
  'paper',
  'night',
  'miniature',
  'sparse-worldwide',
  'busy-worldwide',
  'dense-worldwide',
  'clustered',
  'custom-pins',
]) {
  test(`globe ${variant} story loads the vendored map and pin stems`, async ({ page }) => {
    await page.goto(story.replace('--playground', `--${variant}`) + '&args=animate:!false');
    await expect(page.locator('.land')).toBeVisible();
    await expect(page.locator('.pin-stem').first()).toBeVisible();
    await page.screenshot({ path: test.info().outputPath(`globe-${variant}.png`) });
  });
}

test('demo count and distribution controls change the preview without an API', async ({ page }) => {
  await page.goto(story + '&args=animate:!false;pinCount:12;distribution:worldwide');
  await expect(page.locator('[data-demo-count]')).toHaveAttribute('data-demo-count', '12');
  const sparse = await page.locator('.pin-stem').count();
  await page.goto(story + '&args=animate:!false;pinCount:500;distribution:worldwide');
  await expect(page.locator('[data-demo-count]')).toHaveAttribute('data-demo-count', '500');
  expect(await page.locator('.pin-stem').count()).toBeGreaterThan(sparse);
  await page.goto(story.replace('--playground', '--clustered') + '&args=animate:!false');
  await expect(page.locator('[data-demo-count]')).toContainText('150 demo pins · clustered');
  await page.goto(story.replace('--playground', '--empty') + '&args=animate:!false');
  await expect(page.locator('[data-demo-count]')).toHaveAttribute('data-demo-count', '0');
  await expect(page.locator('.pin-stem')).toHaveCount(0);
});

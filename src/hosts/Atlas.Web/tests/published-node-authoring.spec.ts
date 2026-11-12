import { expect, test } from '@playwright/test';

const story = 'http://127.0.0.1:6006/iframe.html?id=content-node-authoring--published-card&viewMode=story';

test('the published-card story previews the current draft and keeps editing state', async ({ page }) => {
  await page.setViewportSize({ width: 390, height: 844 });
  await page.route('https://images.unsplash.com/**', (route) => route.fulfill({
    contentType: 'image/svg+xml',
    body: '<svg xmlns="http://www.w3.org/2000/svg" width="80" height="50"><rect width="80" height="50" fill="green"/></svg>',
  }));
  await page.goto(story);
  const canvas = page.getByRole('textbox', { name: 'Node content: editable rich-text document' });
  await expect(canvas).toBeVisible();
  const type = page.getByRole('combobox', { name: 'Node type' });
  const shortWidth = (await type.boundingBox())!.width;
  await type.selectOption('counterargument');
  expect((await type.boundingBox())!.width).toBeGreaterThan(shortWidth);
  await type.selectOption('idea');
  await expect(page.getByRole('checkbox', { name: 'Show parent breadcrumb' })).toHaveCount(0);
  await page.getByRole('textbox', { name: 'Title', exact: true }).fill('A revised public square');
  await page.getByRole('button', { name: 'Preview in feed' }).click();

  const card = page.getByRole('article', { name: /A revised public square/ });
  await expect(card).toHaveAttribute('data-presentation', 'ribbon-corner');
  const footer = card.locator('.navigation-details');
  await expect(footer.getByRole('button')).toHaveText(['0 Comments', '0 Questions', '0 Evidence']);
  await expect(card).not.toContainText('Help develop this node with');
  const feed = card.getByRole('region', { name: 'Feed preview of node content blocks' });
  await expect(feed).toContainText('1 of 2');
  await expect(feed.getByRole('img', { name: 'A city street lined with tall buildings' })).toBeVisible();
  await feed.getByRole('button', { name: 'Next node content block' }).click();
  await expect(feed).toContainText('2 of 2');
  await expect(feed).toContainText('Start with a small pilot');
  await feed.getByRole('button', { name: 'Read more' }).click();
  await expect(page.getByText('Full node preview', { exact: true })).toBeVisible();
  await expect(page.getByText('Start with a small pilot', { exact: false }).filter({ visible: true })).toBeVisible();

  await page.getByRole('button', { name: 'Edit', exact: true }).click();
  await expect(canvas).toBeVisible();
  await expect(page.getByRole('textbox', { name: 'Title', exact: true })).toHaveValue('A revised public square');
});

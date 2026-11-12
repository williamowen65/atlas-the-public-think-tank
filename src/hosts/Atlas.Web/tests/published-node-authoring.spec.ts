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
  const title = page.getByRole('textbox', { name: 'Title', exact: true });
  const shortWidth = (await type.boundingBox())!.width;
  expect((await type.boundingBox())!.height).toBeCloseTo((await title.boundingBox())!.height, 0);
  await expect(type).toHaveCSS('font-size', await title.evaluate((input) => getComputedStyle(input).fontSize));
  await type.selectOption('question');
  expect(await type.evaluate((select) => {
    const label = select.previousElementSibling!;
    const range = document.createRange();
    range.selectNodeContents(label);
    const style = getComputedStyle(select);
    return select.clientWidth - parseFloat(style.paddingLeft) - parseFloat(style.paddingRight)
      >= range.getBoundingClientRect().width - 1;
  })).toBe(true);
  await type.selectOption('counterargument');
  expect((await type.boundingBox())!.width).toBeGreaterThan(shortWidth);
  await type.selectOption('idea');
  await expect(page.getByRole('checkbox', { name: 'Show parent breadcrumb' })).toHaveCount(0);
  await page.getByRole('textbox', { name: 'Title', exact: true }).fill('A revised public square');
  const community = page.getByRole('combobox', { name: 'Communities (optional)' });
  await community.selectOption('public-spaces');
  await community.selectOption('urban-trees');
  await expect(page.getByRole('list', { name: 'Linked communities' })).toContainText('Urban trees');

  await canvas.getByRole('button', { name: 'Edit image in text' }).click();
  const imageSettings = page.getByRole('form', { name: 'Edit image in Markdown' });
  await expect(imageSettings.getByLabel('Inline image URL')).toHaveValue(/images.unsplash.com/);
  await imageSettings.getByLabel('Inline image alternative text').fill('Buildings around the square');
  await imageSettings.getByRole('button', { name: 'Apply image' }).click();
  await expect(canvas.getByRole('img', { name: 'Buildings around the square' })).toHaveCount(1);
  await page.getByRole('button', { name: 'Preview in feed' }).click();

  const card = page.getByRole('article', { name: /A revised public square/ });
  await expect(card).toHaveAttribute('data-presentation', 'ribbon-corner');
  await expect(card.locator('.byline')).toContainText('Public spaces, Urban trees');
  await expect(card.locator('.reactions')).toContainText('Useful 3');
  const footer = card.locator('.navigation-details');
  await expect(footer.getByRole('button')).toHaveText(['0 Comments', '0 Questions', '0 Evidence']);
  await expect(card).not.toContainText('Help develop this node with');
  const feed = card.getByRole('region', { name: 'Feed preview of node content blocks' });
  await expect(feed).toContainText('1 of 3');
  await expect(feed.getByRole('img', { name: 'Buildings around the square' })).toBeVisible();
  await feed.getByRole('button', { name: 'Next node content block' }).click();
  await expect(feed).toContainText('2 of 3');
  await expect(feed).toContainText('Start with a small pilot');
  await feed.getByRole('button', { name: 'Read more' }).click();
  await expect(page.getByText('Full node preview', { exact: true })).toBeVisible();
  await expect(page.getByText('Start with a small pilot', { exact: false }).filter({ visible: true })).toBeVisible();

  await page.getByRole('button', { name: 'Edit', exact: true }).click();
  await expect(canvas).toBeVisible();
  await expect(page.getByRole('textbox', { name: 'Title', exact: true })).toHaveValue('A revised public square');
});

test('the desktop feed carousel uses the voting dial height on short and long slides', async ({ page }) => {
  await page.setViewportSize({ width: 1180, height: 900 });
  await page.route('https://images.unsplash.com/**', (route) => route.abort());
  await page.goto(story);
  await page.getByRole('button', { name: 'Preview in feed' }).click();

  const feed = page.getByRole('region', { name: 'Feed preview of node content blocks' });
  const viewport = feed.getByRole('region', { name: 'Node content carousel, swipe or use arrows' });
  const readMore = feed.getByRole('button', { name: 'Read more' });
  const dial = page.getByRole('complementary', { name: 'Importance rating' }).locator('.voting-dial');
  await expect(dial).toBeVisible();
  const bottom = async () => readMore.evaluate((button) => button.getBoundingClientRect().bottom);
  const dialBottom = await dial.evaluate((element) => element.getBoundingClientRect().bottom);
  await expect.poll(async () => Math.abs((await bottom()) - dialBottom)).toBeLessThan(2);
  const longHeight = (await viewport.boundingBox())!.height;
  const arrowTop = (await feed.getByRole('button', { name: 'Next node content block' }).boundingBox())!.y;
  expect(await viewport.locator('.content-blocks').evaluate((el) => el.scrollHeight > el.clientHeight)).toBe(true);

  await feed.getByRole('button', { name: 'Next node content block' }).click();
  await expect(viewport).toContainText('Start with a small pilot');
  expect((await viewport.boundingBox())!.height).toBeCloseTo(longHeight, 0);
  expect((await feed.getByRole('button', { name: 'Next node content block' }).boundingBox())!.y).toBeCloseTo(arrowTop, 0);
  expect(await bottom()).toBeCloseTo(dialBottom, 0);
});


test('the feed and full node open the same gallery at the clicked image', async ({ page }) => {
  await page.setViewportSize({ width: 1180, height: 900 });
  await page.route('https://images.unsplash.com/**', (route) => route.fulfill({
    contentType: 'image/svg+xml',
    body: '<svg xmlns="http://www.w3.org/2000/svg" width="80" height="120"><rect width="80" height="120" fill="green"/></svg>',
  }));
  await page.goto(story);
  await page.getByRole('button', { name: 'Preview in feed' }).click();
  const feed = page.getByRole('region', { name: 'Feed preview of node content blocks' });
  const openingImage = feed.getByRole('button', { name: 'View A city street lined with tall buildings' });
  await openingImage.click();
  const gallery = page.getByRole('dialog', { name: 'Node media gallery' });
  await expect(gallery).toContainText('1 of 2');
  await expect(gallery.getByRole('img', { name: 'A city street lined with tall buildings' })).toBeVisible();
  await page.keyboard.press('ArrowRight');
  await expect(gallery).toContainText('2 of 2');
  await expect(gallery.getByRole('img', { name: 'A street lined with trees' })).toBeVisible();
  await page.keyboard.press('Escape');
  await expect(gallery).toBeHidden();
  await expect(openingImage).toBeFocused();
  await expect(feed).toContainText('1 of 3');

  await feed.getByRole('button', { name: 'Read more' }).click();
  await page.getByRole('button', { name: 'View A street lined with trees' }).click();
  await expect(gallery).toContainText('2 of 2');
  await gallery.getByRole('button', { name: 'Previous item' }).click();
  await expect(gallery).toContainText('1 of 2');
});

test('Image Layouts opens in TipTap and can switch back from Markdown', async ({ page }) => {
  await page.route('https://images.unsplash.com/**', (route) => route.abort());
  await page.goto(story.replace('--published-card', '--image-layouts'));
  const markdown = page.getByRole('checkbox', { name: 'Edit Markdown' });
  const canvas = page.getByRole('textbox', { name: 'Node content: editable rich-text document' });

  await expect(canvas).toBeVisible();
  await markdown.check();
  await expect(page.getByLabel('Text (Markdown)').first()).toBeVisible();
  await markdown.uncheck();
  await expect(canvas.locator('[data-atlas-inline-image][data-image-placement="right"]')).toHaveCount(1);
  await expect(canvas.locator('[data-atlas-inline-image][data-image-placement="center"]')).toHaveCount(1);
  await expect(markdown).not.toBeChecked();
});

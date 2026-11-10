import { expect, test } from '@playwright/test';

const storyUrl =
  'http://127.0.0.1:6006/iframe.html?id=content-editor-workbench-experimental--feed-block-carousel-concept&viewMode=story';

const carousel = (page: import('@playwright/test').Page) =>
  page.getByRole('region', { name: 'Feed preview of node content blocks' });

test('the feed has exactly one slide per authored Atlas block and keeps their order', async ({ page }) => {
  await page.goto(storyUrl);
  await expect(page.locator('.atlas-story-rich-document > [data-atlas-key]')).toHaveCount(4);
  await expect(page.locator('.model-inspector ol > li')).toHaveCount(4);
  await page.getByRole('button', { name: 'Feed', exact: true }).click();
  const feed = carousel(page);
  await expect(feed).toContainText('1 of 4 blocks');
  await expect(feed.locator('.feed-block-slide')).toHaveAttribute('data-block-id', 'opening');
  await expect(feed.locator('.feed-block-slide')).toHaveAttribute('data-block-kind', 'markdown');
  await feed.getByRole('button', { name: 'Next node content block' }).click();
  await expect(feed.locator('.feed-block-slide')).toHaveAttribute('data-block-id', 'first-photo');
  await expect(feed.locator('.feed-block-slide figure img')).toBeVisible();
  await feed.getByRole('button', { name: 'Next node content block' }).click();
  await expect(feed.locator('.feed-block-slide')).toHaveAttribute('data-block-id', 'long-section');
  await feed.getByRole('button', { name: 'Next node content block' }).click();
  await expect(feed.locator('.feed-block-slide')).toHaveAttribute('data-block-id', 'second-photo');
  await expect(feed).toContainText('4 of 4 blocks');
  await expect(page.locator('.model-inspector ol > li')).toHaveCount(4);
});

test('long text is bounded to one feed slide and Read more opens the entire normal node', async ({ page }) => {
  await page.goto(storyUrl);
  await page.getByRole('button', { name: 'Feed', exact: true }).click();
  const feed = carousel(page);
  await feed.getByRole('button', { name: 'Show node block 3' }).click();
  await expect(feed.locator('.feed-block-slide')).toHaveAttribute('data-block-id', 'long-section');
  const bounds = await feed.evaluate(el => {
    const view = el.querySelector('.viewport');
    const content = el.querySelector('.block-preview');
    const markdown = content?.querySelector('.content-blocks');
    if (!view || !content || !markdown) return null;
    return {
      viewHeight: view.getBoundingClientRect().height,
      contentHeight: content.getBoundingClientRect().height,
      fullHeight: markdown.scrollHeight,
    };
  });
  expect(bounds).not.toBeNull();
  expect(bounds!.viewHeight).toBeLessThanOrEqual(210);
  expect(bounds!.fullHeight).toBeGreaterThan(bounds!.contentHeight);
  await feed.getByRole('button', { name: 'Read more' }).click();
  await expect(page.getByRole('article', { name: 'Full node preview' })).toBeVisible();
  await expect(carousel(page)).toHaveCount(0);
  await expect(page.getByText('This final paragraph is intentionally farther down')).toBeVisible();
  await expect(page.locator('.node-content .content-blocks figure')).toHaveCount(2);
});

test('editing a text block changes its feed slide directly without copying data', async ({ page }) => {
  await page.goto(storyUrl);
  await page.getByRole('button', { name: 'Existing block forms' }).click();
  const textInputs = page.getByLabel('Text (Markdown)');
  await textInputs.first().fill('A NEW opening written using the existing Atlas block editor.');
  await page.getByRole('button', { name: 'Feed', exact: true }).click();
  const feed = carousel(page);
  await expect(feed).toContainText('A NEW opening written using the existing Atlas block editor.');
  await expect(feed.locator('.feed-block-slide')).toHaveAttribute('data-block-id', 'opening');
  await feed.getByRole('button', { name: 'Read more' }).click();
  await expect(page.getByRole('article', { name: 'Full node preview' }))
    .toContainText('A NEW opening written using the existing Atlas block editor.');
});

test('feed aspect-frame experiments do not change node content or appear in Full Node', async ({ page }) => {
  await page.goto(storyUrl);
  await page.getByRole('button', { name: 'Feed', exact: true }).click();
  const feed = carousel(page);
  const viewport = feed.getByRole('region', { name: 'Node content carousel, swipe or use arrows' });
  await expect(viewport).toHaveClass(/landscape/);
  const before = await viewport.boundingBox();
  await page.getByRole('button', { name: 'Portrait', exact: true }).click();
  await expect(viewport).toHaveClass(/portrait/);
  const after = await viewport.boundingBox();
  expect(before).not.toBeNull();
  expect(after).not.toBeNull();
  expect(after!.height).toBeGreaterThan(before!.height);
  await page.getByRole('button', { name: 'Full node', exact: true }).click();
  await expect(carousel(page)).toHaveCount(0);
  await expect(page.getByRole('group', { name: 'Feed slide framing' })).toHaveCount(0);
  await expect(page.locator('.node-content .content-blocks figure')).toHaveCount(2);
  await expect(page.locator('.model-inspector ol > li')).toHaveCount(4);
});

test('phone-width feed supports swipe, dots, arrows and keyboard without opening the node', async ({ page }) => {
  await page.goto(storyUrl);
  await page.getByRole('button', { name: 'Simulate phone width' }).click();
  await page.getByRole('button', { name: 'Feed', exact: true }).click();
  const feed = carousel(page);
  const viewport = feed.getByRole('region', { name: 'Node content carousel, swipe or use arrows' });
  await expect(feed).toContainText('1 of 4 blocks');
  await viewport.evaluate(element => {
    const first = new Touch({ identifier: 1, target: element, clientX: 230, clientY: 120 });
    const end = new Touch({ identifier: 1, target: element, clientX: 60, clientY: 120 });
    element.dispatchEvent(new TouchEvent('touchstart', {
      bubbles: true, touches: [first], changedTouches: [first], targetTouches: [first],
    }));
    element.dispatchEvent(new TouchEvent('touchend', {
      bubbles: true, touches: [], changedTouches: [end], targetTouches: [],
    }));
  });
  await expect(feed).toContainText('2 of 4 blocks');
  await feed.getByRole('button', { name: 'Show node block 4' }).click();
  await expect(feed.locator('.feed-block-slide')).toHaveAttribute('data-block-id', 'second-photo');
  await viewport.focus();
  await viewport.press('ArrowLeft');
  await expect(feed.locator('.feed-block-slide')).toHaveAttribute('data-block-id', 'long-section');
  await feed.getByRole('button', { name: 'Previous node content block' }).click();
  await expect(feed.locator('.feed-block-slide')).toHaveAttribute('data-block-id', 'first-photo');
});

test('standard editor stories also derive feed previews from their current flat blocks', async ({ page }) => {
  await page.goto('http://127.0.0.1:6006/iframe.html?id=content-editor-workbench-experimental--compare-editors&viewMode=story');
  await page.getByRole('button', { name: 'Feed', exact: true }).click();
  const feed = carousel(page);
  await expect(feed).toContainText('1 of 3 blocks');
  await expect(feed.locator('.feed-block-slide')).toHaveAttribute('data-block-id', 'intro');
  await feed.getByRole('button', { name: 'Show node block 2' }).click();
  await expect(feed.locator('.feed-block-slide')).toHaveAttribute('data-block-id', 'public-square');
  await feed.getByRole('button', { name: 'Read more' }).click();
  await expect(page.getByRole('article', { name: 'Full node preview' })).toBeVisible();
  await expect(carousel(page)).toHaveCount(0);
});

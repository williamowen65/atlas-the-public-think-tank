import { expect, test } from '@playwright/test';

const storyUrl =
  'http://127.0.0.1:6006/iframe.html?id=content-editor-workbench-experimental--media-gallery-concept&viewMode=story';

test('gallery uses the same uncropped portrait or landscape frame in Edit and Full Node', async ({ page }) => {
  await page.goto(storyUrl);
  const gallery = page.getByRole('region', { name: 'Experimental mixed-media gallery' });
  const viewport = gallery.getByRole('region', { name: 'Gallery media, swipe or use arrows' });
  await expect(viewport).toHaveClass(/portrait/);
  await expect(gallery.getByLabel('Gallery frame')).toHaveValue('portrait');
  const portraitEdit = await viewport.boundingBox();
  expect(portraitEdit).not.toBeNull();
  expect(portraitEdit!.height).toBeGreaterThan(portraitEdit!.width);

  await page.getByRole('button', { name: 'Full node', exact: true }).click();
  const fullPortrait = await viewport.boundingBox();
  expect(fullPortrait).not.toBeNull();
  expect(Math.abs(fullPortrait!.width - portraitEdit!.width)).toBeLessThan(3);
  expect(Math.abs(fullPortrait!.height - portraitEdit!.height)).toBeLessThan(3);
  await expect(viewport.locator('img')).toHaveCSS('object-fit', 'contain');

  await page.getByRole('button', { name: 'Edit', exact: true }).click();
  await gallery.getByLabel('Gallery frame').selectOption('landscape');
  const wideEdit = await viewport.boundingBox();
  expect(wideEdit).not.toBeNull();
  expect(wideEdit!.width).toBeGreaterThan(wideEdit!.height);
  await page.getByRole('button', { name: 'Full node', exact: true }).click();
  const wideFull = await viewport.boundingBox();
  expect(wideFull).not.toBeNull();
  expect(Math.abs(wideFull!.width - wideEdit!.width)).toBeLessThan(3);
  expect(Math.abs(wideFull!.height - wideEdit!.height)).toBeLessThan(3);
});

test('arrows, page dots and keyboard discover four typed child blocks including playable video', async ({ page }) => {
  await page.goto(storyUrl);
  const gallery = page.getByRole('region', { name: 'Experimental mixed-media gallery' });
  const viewport = gallery.getByRole('region', { name: 'Gallery media, swipe or use arrows' });
  await expect(gallery).toContainText('Swipe left or right');
  await expect(gallery).toContainText('1 / 4');
  await gallery.getByRole('button', { name: 'Next gallery item' }).click();
  await expect(gallery).toContainText('2 / 4');
  await gallery.getByRole('button', { name: 'Go to gallery item 3' }).click();
  await expect(gallery).toContainText('3 / 4');
  const video = viewport.locator('video');
  await expect(video).toBeVisible();
  await expect(video).toHaveAttribute('controls', '');
  await expect(video).toHaveAttribute('preload', 'metadata');
  await viewport.focus();
  await viewport.press('ArrowLeft');
  await expect(gallery).toContainText('2 / 4');
  await gallery.getByRole('button', { name: 'Previous gallery item' }).click();
  await expect(gallery).toContainText('1 / 4');
});

test('a new gallery item can be edited and remains in Full Node while gallery data stays separate from Atlas blocks', async ({ page }) => {
  await page.goto(storyUrl);
  const gallery = page.getByRole('region', { name: 'Experimental mixed-media gallery' });
  await expect(page.locator('.model-inspector')).toContainText('4 media items');
  const initialBlockCount = await page.locator('.model-inspector ol > li').count();
  await gallery.getByRole('button', { name: 'Add image to gallery' }).click();
  await expect(gallery).toContainText('5 / 5');
  await gallery.getByRole('textbox', { name: 'Image URL' }).fill('https://example.org/new-square.jpg');
  await gallery.getByRole('textbox', { name: 'Alternative text' }).fill('New city garden');
  await gallery.getByRole('textbox', { name: 'Caption' }).fill('A future public garden');
  await expect(gallery).toContainText('A future public garden');
  await page.getByRole('button', { name: 'Full node', exact: true }).click();
  await expect(gallery).toContainText('5 / 5');
  await expect(gallery).toContainText('A future public garden');
  await expect(page.locator('.model-inspector ol > li')).toHaveCount(initialBlockCount);
});

test('feed gallery shows a compact teaser with clear navigation rather than stretching a portrait', async ({ page }) => {
  await page.goto(storyUrl);
  await page.getByRole('button', { name: 'Feed', exact: true }).click();
  const gallery = page.getByRole('region', { name: 'Experimental mixed-media gallery' });
  const viewport = gallery.getByRole('region', { name: 'Gallery media, swipe or use arrows' });
  await expect(gallery).toHaveClass(/feed/);
  await expect(viewport).toHaveCSS('aspect-ratio', '16 / 9');
  await expect(viewport.locator('img')).toHaveCSS('object-fit', 'cover');
  await expect(page.getByRole('article', { name: 'Compact feed preview' }))
    .toContainText('Read more');
  await gallery.getByRole('button', { name: 'Next gallery item' }).click();
  await expect(gallery).toContainText('2 / 4');
  await expect(page.getByText('The feed uses a compact 16:9 teaser')).toBeVisible();
});

test('on simulated phone width gallery is swipeable and keeps visible navigation clues', async ({ page }) => {
  await page.goto(storyUrl);
  await page.getByRole('button', { name: 'Simulate phone width' }).click();
  const gallery = page.getByRole('region', { name: 'Experimental mixed-media gallery' });
  const viewport = gallery.getByRole('region', { name: 'Gallery media, swipe or use arrows' });
  await expect(gallery).toContainText('1 / 4');
  await viewport.evaluate(element => {
    const start = new Touch({ identifier: 1, target: element, clientX: 250, clientY: 180 });
    const end = new Touch({ identifier: 1, target: element, clientX: 95, clientY: 180 });
    element.dispatchEvent(new TouchEvent('touchstart', {
      bubbles: true, touches: [start], changedTouches: [start], targetTouches: [start],
    }));
    element.dispatchEvent(new TouchEvent('touchend', {
      bubbles: true, touches: [], changedTouches: [end], targetTouches: [],
    }));
  });
  await expect(gallery).toContainText('2 / 4');
  await expect(gallery.getByRole('button', { name: 'Next gallery item' })).toBeVisible();
});

test('single images have matching edit and full-node maximum heights in the gallery experiment', async ({ page }) => {
  await page.goto(storyUrl);
  const editorImage = page.locator('.atlas-story-figure img').first();
  await expect(editorImage).toBeVisible();
  const editHeight = await editorImage.evaluate(element => getComputedStyle(element).maxHeight);
  await page.getByRole('button', { name: 'Full node', exact: true }).click();
  const fullImage = page.locator('.node-content .content-blocks figure img').first();
  await expect(fullImage).toBeVisible();
  const fullHeight = await fullImage.evaluate(element => getComputedStyle(element).maxHeight);
  expect(fullHeight).toBe(editHeight);
});


test('the feed carousel reveals a mock poll child and permits local-only choice selection', async ({ page }) => {
  const writes: string[] = [];
  page.on('request', request => {
    if (/\/atlas-(?:session|public)-api\//.test(request.url()) ||
        (request.method() !== 'GET' && request.method() !== 'HEAD'))
      writes.push(request.url());
  });
  await page.goto(storyUrl);
  await page.getByRole('button', { name: 'Feed', exact: true }).click();
  const gallery = page.getByRole('region', { name: 'Experimental mixed-media gallery' });
  await gallery.getByRole('button', { name: 'Go to gallery item 4' }).click();
  const poll = gallery.getByLabel('Demo poll child block');
  await expect(poll).toBeVisible();
  await expect(poll).toContainText('What would make this public square more welcoming?');
  await expect(poll).toContainText('Poll block · demo only');
  await poll.getByRole('button', { name: 'Safer walking paths' }).click();
  await expect(poll.getByRole('button', { name: 'Safer walking paths' })).toHaveAttribute('aria-pressed', 'true');
  await expect(poll).toContainText('No vote was submitted');
  await page.getByRole('button', { name: 'Full node', exact: true }).click();
  await expect(gallery.getByLabel('Demo poll child block')).toContainText('No vote was submitted');
  expect(writes).toEqual([]);
});

test('a mock poll is an editable carousel child without creating a persisted Atlas block', async ({ page }) => {
  await page.goto(storyUrl);
  const gallery = page.getByRole('region', { name: 'Experimental mixed-media gallery' });
  const blocksBefore = await page.locator('.model-inspector ol > li').count();
  await gallery.getByRole('button', { name: 'Go to gallery item 4' }).click();
  await expect(gallery.getByLabel('Poll question')).toHaveValue('What would make this public square more welcoming?');
  await gallery.getByLabel('Poll question').fill('How should we improve the plaza?');
  await expect(gallery).toContainText('How should we improve the plaza?');
  await gallery.getByRole('button', { name: 'Move slide left' }).click();
  await expect(gallery).toContainText('3 / 4');
  await page.getByRole('button', { name: 'Feed', exact: true }).click();
  await expect(gallery.getByLabel('Demo poll child block')).toContainText('How should we improve the plaza?');
  await expect(page.locator('.model-inspector ol > li')).toHaveCount(blocksBefore);
});

test('the gallery navigation does not intercept arrow keys while interactive poll choices are focused', async ({ page }) => {
  await page.goto(storyUrl);
  const gallery = page.getByRole('region', { name: 'Experimental mixed-media gallery' });
  await gallery.getByRole('button', { name: 'Go to gallery item 4' }).click();
  const choice = gallery.getByRole('button', { name: 'More shade and trees' });
  await choice.focus();
  await choice.press('ArrowLeft');
  await expect(gallery).toContainText('4 / 4');
  await gallery.getByRole('region', { name: 'Gallery media, swipe or use arrows' }).focus();
  await page.keyboard.press('ArrowLeft');
  await expect(gallery).toContainText('3 / 4');
});

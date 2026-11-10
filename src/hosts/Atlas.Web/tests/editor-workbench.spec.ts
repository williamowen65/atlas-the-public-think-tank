import { expect, test } from '@playwright/test';

const storyUrl = 'http://127.0.0.1:6006/iframe.html?id=content-editor-workbench-experimental--compare-editors&viewMode=story';

test('Storybook workbench keeps both authoring approaches available', async ({ page }) => {
  await page.goto(storyUrl);
  await expect(page.getByRole('heading', { name: 'Node authoring workbench' })).toBeVisible();
  await expect(page.locator('.atlas-story-rich-document')).toBeVisible();
  await page.getByRole('button', { name: 'Existing block forms' }).click();
  await expect(page.getByLabel('Text (Markdown)').first()).toBeVisible();
  await page.getByRole('button', { name: 'Tiptap canvas' }).click();
  await expect(page.locator('.atlas-story-rich-document')).toBeVisible();
  await page.getByRole('button', { name: 'Editorial', exact: true }).click();
  await expect(page.locator('.tiptap-prototype.editorial')).toBeVisible();
});

test('Tiptap prototype text appears in both Storybook previews without API calls', async ({ page }) => {
  const requests: string[] = [];
  page.on('request', request => {
    if (/\/atlas-(?:session|public)-api\//.test(request.url())) requests.push(request.url());
  });
  await page.goto(storyUrl);
  const editor = page.locator('.atlas-story-rich-document');
  await expect(editor).toBeVisible();
  await editor.click();
  await editor.press('ControlOrMeta+End');
  await editor.press('Enter');
  await editor.pressSequentially('A new paragraph written in the Tiptap prototype.');
  await page.getByRole('button', { name: 'Full node', exact: true }).click();
  await expect(page.getByRole('article', { name: 'Full node preview' })).toContainText('A new paragraph written in the Tiptap prototype.');
  await page.getByRole('button', { name: 'Feed', exact: true }).click();
  await expect(page.getByRole('article', { name: 'Compact feed preview' })).toContainText('A better public square');
  expect(requests).toEqual([]);
});

test('phone-width toggle exposes a narrow canvas while preserving editor mode switch', async ({ page }) => {
  await page.goto(storyUrl);
  await page.getByRole('button', { name: 'Simulate phone width' }).click();
  await expect(page.locator('.workbench.phone-width')).toBeVisible();
  await page.getByRole('button', { name: 'Existing block forms' }).click();
  await expect(page.getByLabel('Text (Markdown)').first()).toBeVisible();
  await page.getByRole('button', { name: 'Tiptap canvas' }).click();
  await expect(page.locator('.atlas-story-rich-document')).toBeVisible();
});

test('narrow Tiptap editor can insert a captioned image with required alt text', async ({ page }) => {
  await page.goto(storyUrl);
  await page.getByRole('button', { name: 'Simulate phone width' }).click();
  const editor = page.locator('.atlas-story-rich-document');
  await editor.click();
  await editor.press('ControlOrMeta+End');
  await page.getByRole('button', { name: 'Add a content block' }).click();
  await page.getByRole('button', { name: 'Image', exact: true }).click();
  await page.getByLabel('Image URL', { exact: true }).fill('https://example.org/new-park.png');
  await page.getByLabel('Alternative text').fill('A walkway beside a tree');
  await page.getByLabel('Caption').fill('An accessible new park');
  await page.getByRole('button', { name: 'Apply image' }).click();
  await expect(page.locator('.atlas-story-figure').last()).toContainText('An accessible new park');
  await expect(page.locator('.atlas-story-figure').last().locator('img')).toHaveAttribute('alt', 'A walkway beside a tree');
});

test('Tiptap and block forms edit the same Markdown content in both directions', async ({ page }) => {
  await page.goto(storyUrl);
  await expect(page.locator('.atlas-story-text[data-atlas-key="intro"]')).toBeVisible();
  await page.locator('.atlas-story-text[data-atlas-key="intro"] p').first().click();
  await page.keyboard.press('End');
  await page.keyboard.type(' Updated using Tiptap.');
  await page.getByRole('button', { name: 'Existing block forms' }).click();
  await expect(page.getByLabel('Text (Markdown)').first()).toHaveValue(/Updated using Tiptap/);
  await page.getByLabel('Text (Markdown)').first().fill('Changed in the **form** editor.');
  await page.getByRole('button', { name: 'Tiptap canvas' }).click();
  await expect(page.locator('.atlas-story-text[data-atlas-key="intro"]')).toContainText('Changed in the form editor.');
  await page.getByRole('button', { name: 'Existing block forms' }).click();
  await expect(page.getByLabel('Text (Markdown)').first()).toHaveValue('Changed in the **form** editor.');
});

test('image movement in Tiptap updates the form order and keeps the image key', async ({ page }) => {
  await page.goto(storyUrl);
  const root = page.locator('.atlas-story-rich-document');
  await expect(root.locator(':scope > .atlas-story-text').first()).toHaveAttribute('data-atlas-key', 'intro');
  const image = root.locator(':scope > .atlas-story-figure').first();
  await image.click();
  await page.getByRole('button', { name: 'Move image up' }).click();
  await expect(root.locator(':scope > *').first()).toHaveAttribute('data-atlas-key', 'public-square');
  await page.getByRole('button', { name: 'Existing block forms' }).click();
  await expect(page.locator('.block-composer .content-block').first()).toContainText('Block 1 · Image');
  await expect(page.locator('.block-composer .content-block').first().locator('input[type="url"]'))
    .toHaveValue(/images\.unsplash\.com/);
  await page.getByRole('button', { name: 'Tiptap canvas' }).click();
  await expect(page.locator('.atlas-story-rich-document > *').first())
    .toHaveAttribute('data-atlas-key', 'public-square');
});

test('node card framing and draft metadata are consistent in Edit and Full node views', async ({ page }) => {
  await page.goto(storyUrl);
  await expect(page.getByRole('navigation', { name: 'Node ancestry' })).toContainText('Better streets');
  await page.getByRole('textbox', { name: 'Node title' }).fill('Redesign our green spaces');
  await page.getByLabel('Node type').selectOption('Question');
  await page.getByLabel('Community', { exact: true }).selectOption('Public Spaces');
  await page.getByRole('button', { name: 'Full node', exact: true }).click();
  const preview = page.getByRole('article', { name: 'Full node preview' });
  await expect(preview).toContainText('Question:');
  await expect(preview).toContainText('Redesign our green spaces');
  await expect(preview).toContainText('Public Spaces');
  await expect(preview.getByRole('navigation', { name: 'Node ancestry' })).toContainText('Better streets');
  await page.getByRole('button', { name: 'Edit', exact: true }).click();
  await expect(page.getByRole('textbox', { name: 'Node title' })).toHaveValue('Redesign our green spaces');
});

test('formatting controls stay visible and the plus stays completely left of writing', async ({ page }) => {
  await page.goto(storyUrl);
  const toolbar = page.getByRole('toolbar', { name: 'Text formatting' });
  const plus = page.getByRole('button', { name: 'Add a content block' });
  await expect(toolbar.getByRole('button', { name: 'Bold selected text' })).toBeVisible();
  await expect(toolbar.getByRole('button', { name: 'Add or edit link' })).toBeVisible();
  await expect(plus).toBeVisible();

  const first = page.locator('.atlas-story-text p').first();
  await first.click();
  await first.press('End');
  await expect(toolbar).toBeVisible();
  await plus.click();
  await expect(page.getByLabel('Insert a content block')).toBeVisible();
  const geometry = await page.evaluate(() => {
    const button = document.querySelector('.gutter-position .insert-trigger');
    const canvas = document.querySelector('.editor-canvas');
    if (!button || !canvas) return null;
    return { buttonRight: button.getBoundingClientRect().right, canvasLeft: canvas.getBoundingClientRect().left };
  });
  expect(geometry).not.toBeNull();
  expect(geometry!.buttonRight).toBeLessThanOrEqual(geometry!.canvasLeft);
  await expect(toolbar).toBeVisible();
});

test('link form creates a real linked text span and maps back to Atlas Markdown', async ({ page }) => {
  await page.goto(storyUrl);
  const p = page.locator('.atlas-story-text[data-atlas-key="intro"] p').first();
  await p.click();
  await p.press('Home');
  await p.press('ControlOrMeta+Shift+ArrowRight');
  await page.getByRole('button', { name: 'Add or edit link' }).click();
  const form = page.getByRole('form', { name: 'Insert or edit link' });
  await expect(form).toBeVisible();
  await form.getByLabel('Link URL').fill('https://example.org/walkability');
  await form.getByRole('button', { name: 'Apply link' }).click();
  await expect(page.locator('.atlas-story-rich-document a[href="https://example.org/walkability"]')).toBeVisible();
  await page.getByRole('button', { name: 'Existing block forms' }).click();
  await expect(page.getByLabel('Text (Markdown)').first()).toHaveValue(/\]\(https:\/\/example\.org\/walkability\)/);
});

test('Backspace in the final empty paragraph does not create phantom Atlas blocks', async ({ page }) => {
  await page.goto('http://127.0.0.1:6006/iframe.html?id=content-editor-workbench-experimental--blank-node&viewMode=story');
  const editor = page.locator('.atlas-story-rich-document');
  await expect(editor).toBeVisible();
  await editor.locator('.atlas-story-text p').last().click();
  await editor.press('Enter');
  await editor.press('Backspace');
  await editor.press('Backspace');
  await expect(page.locator('.atlas-story-rich-document > .atlas-story-text')).toHaveCount(1);
  await page.getByRole('button', { name: 'Existing block forms' }).click();
  await expect(page.locator('.block-composer .content-block')).toHaveCount(1);
});

test('an extra empty text block can be removed without deleting the final writing area', async ({ page }) => {
  await page.goto(storyUrl);
  await page.getByRole('button', { name: 'Add a content block' }).click();
  await page.getByRole('button', { name: 'Text', exact: true }).click();
  await expect(page.locator('.atlas-story-rich-document > .atlas-story-text')).toHaveCount(3);
  await page.locator('.atlas-story-rich-document > .atlas-story-text p').last().click();
  await expect(page.getByRole('button', { name: 'Remove empty text block' })).toBeVisible();
  await page.getByRole('button', { name: 'Remove empty text block' }).click();
  await expect(page.locator('.atlas-story-rich-document > .atlas-story-text')).toHaveCount(2);
});

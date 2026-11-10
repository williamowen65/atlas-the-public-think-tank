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
  await page.getByRole('button', { name: '+ Insert' }).click();
  await page.getByRole('button', { name: 'Image', exact: true }).last().click();
  await page.getByLabel('Image URL', { exact: true }).fill('https://example.org/new-park.png');
  await page.getByLabel('Alternative text').fill('A walkway beside a tree');
  await page.getByLabel('Caption').fill('An accessible new park');
  await page.getByRole('button', { name: 'Apply image' }).click();
  await expect(page.locator('.atlas-story-figure').last()).toContainText('An accessible new park');
  await expect(page.locator('.atlas-story-figure').last().locator('img')).toHaveAttribute('alt', 'A walkway beside a tree');
});

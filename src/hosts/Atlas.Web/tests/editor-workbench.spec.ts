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
  await page.getByRole('button', { name: 'Move current block up' }).click();
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
  await expect(plus).toHaveCount(0);

  const first = page.locator('.atlas-story-text p').first();
  await first.click();
  await expect(plus).toBeVisible();
  await first.press('End');
  await expect(toolbar).toBeVisible();
  await plus.click();
  await expect(page.getByLabel('Insert a content block')).toBeVisible();
  const geometry = await page.evaluate(() => {
    const button = document.querySelector('.gutter-position .insert-trigger');
    const active = document.querySelector('.atlas-story-rich-document > .atlas-story-active-block');
    const root = document.querySelector('.atlas-story-rich-document');
    const inactive = Array.from(root?.children ?? []).find(element => element !== active);
    if (!button || !active || !inactive || !root) return null;
    return {
      buttonRight: button.getBoundingClientRect().right,
      activeLeft: active.getBoundingClientRect().left,
      inactiveLeft: inactive.getBoundingClientRect().left,
      editorLeft: root.getBoundingClientRect().left,
    };
  });
  expect(geometry).not.toBeNull();
  expect(geometry!.buttonRight).toBeLessThanOrEqual(geometry!.activeLeft);
  expect(Math.abs(geometry!.inactiveLeft - geometry!.editorLeft)).toBeLessThanOrEqual(3);
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
  await page.locator('.atlas-story-rich-document > .atlas-story-text p').first().click();
  await page.getByRole('button', { name: 'Add a content block' }).click();
  await page.getByRole('button', { name: 'Text block' }).click();
  await expect(page.locator('.atlas-story-rich-document > .atlas-story-text')).toHaveCount(3);
  await page.locator('.atlas-story-rich-document > .atlas-story-text').nth(1).locator('p').click();
  await page.getByRole('button', { name: 'Delete current block' }).click();
  await expect(page.getByRole('alertdialog', { name: 'Confirm delete block' })).toContainText('Delete this text block?');
  await page.getByRole('button', { name: 'Delete block', exact: true }).click();
  await expect(page.locator('.atlas-story-rich-document > .atlas-story-text')).toHaveCount(2);
});


test('only the active block is indented, with move controls in its side rail', async ({ page }) => {
  await page.goto(storyUrl);
  const root = page.locator('.atlas-story-rich-document');
  const original = root.locator(':scope > .atlas-story-text[data-atlas-key="intro"]');
  const image = root.locator(':scope > .atlas-story-figure[data-atlas-key="public-square"]');

  await original.locator('p').first().click();
  await expect(original).toHaveClass(/atlas-story-active-block/);
  await expect(image).not.toHaveClass(/atlas-story-active-block/);
  await expect(page.getByRole('button', { name: 'Move current block up' })).toBeDisabled();
  await expect(page.getByRole('button', { name: 'Move current block down' })).toBeEnabled();

  const layout = await page.evaluate(() => {
    const root = document.querySelector('.atlas-story-rich-document');
    const first = root?.querySelector('.atlas-story-text[data-atlas-key="intro"]');
    const image = root?.querySelector('.atlas-story-figure[data-atlas-key="public-square"]');
    const rail = document.querySelector('.gutter-position');
    if (!root || !first || !image || !rail) return null;
    return {
      rootLeft: root.getBoundingClientRect().left,
      firstLeft: first.getBoundingClientRect().left,
      imageLeft: image.getBoundingClientRect().left,
      railRight: rail.getBoundingClientRect().right,
    };
  });
  expect(layout).not.toBeNull();
  expect(layout!.firstLeft).toBeGreaterThan(layout!.rootLeft + 25);
  expect(Math.abs(layout!.imageLeft - layout!.rootLeft)).toBeLessThanOrEqual(3);
  expect(layout!.railRight).toBeLessThanOrEqual(layout!.firstLeft);

  await page.getByRole('button', { name: 'Move current block down' }).click();
  await expect(root.locator(':scope > *').first()).toHaveAttribute('data-atlas-key', 'public-square');
  await expect(original).toHaveClass(/atlas-story-active-block/);
  await expect(image).not.toHaveClass(/atlas-story-active-block/);
  await page.getByRole('button', { name: 'Existing block forms' }).click();
  await expect(page.locator('.block-composer .content-block').first()).toContainText('Block 1 · Image');
});

test('only one section indents in narrow phone-sized workbench', async ({ page }) => {
  await page.goto(storyUrl);
  await page.getByRole('button', { name: 'Simulate phone width' }).click();
  const root = page.locator('.atlas-story-rich-document');
  const first = root.locator(':scope > .atlas-story-text[data-atlas-key="intro"]');
  const image = root.locator(':scope > .atlas-story-figure[data-atlas-key="public-square"]');
  await image.click();
  await expect(image).toHaveClass(/atlas-story-active-block/);
  await expect(first).not.toHaveClass(/atlas-story-active-block/);
  const layout = await page.evaluate(() => {
    const root = document.querySelector('.atlas-story-rich-document');
    const first = root?.querySelector('.atlas-story-text[data-atlas-key="intro"]');
    const image = root?.querySelector('.atlas-story-figure[data-atlas-key="public-square"]');
    const rail = document.querySelector('.gutter-position');
    if (!root || !first || !image || !rail) return null;
    return {
      rootLeft: root.getBoundingClientRect().left,
      firstLeft: first.getBoundingClientRect().left,
      imageLeft: image.getBoundingClientRect().left,
      railRight: rail.getBoundingClientRect().right,
    };
  });
  expect(layout).not.toBeNull();
  expect(Math.abs(layout!.firstLeft - layout!.rootLeft)).toBeLessThanOrEqual(3);
  expect(layout!.imageLeft).toBeGreaterThan(layout!.rootLeft + 25);
  expect(layout!.railRight).toBeLessThanOrEqual(layout!.imageLeft);
});


test('selected Markdown section indents even after typing while inactive blocks remain full width', async ({ page }) => {
  await page.goto(storyUrl);
  const root = page.locator('.atlas-story-rich-document');
  const first = root.locator(':scope > .atlas-story-text[data-atlas-key="intro"]');
  const second = root.locator(':scope > .atlas-story-text[data-atlas-key="details"]');
  await first.locator('p').click();
  await first.locator('p').press('End');
  await first.locator('p').pressSequentially(' more words');
  await expect(first).toHaveClass(/atlas-story-active-block/);
  await expect(second).not.toHaveClass(/atlas-story-active-block/);
  const geometry = await page.evaluate(() => {
    const card = document.querySelector('.node-frame');
    const root = document.querySelector('.atlas-story-rich-document');
    const active = root?.querySelector('.atlas-story-text[data-atlas-key="intro"]');
    const inactive = root?.querySelector('.atlas-story-text[data-atlas-key="details"]');
    const rail = document.querySelector('.gutter-position');
    if (!card || !root || !active || !inactive || !rail) return null;
    return {
      cardLeft: card.getBoundingClientRect().left,
      rootLeft: root.getBoundingClientRect().left,
      activeLeft: active.getBoundingClientRect().left,
      inactiveLeft: inactive.getBoundingClientRect().left,
      railLeft: rail.getBoundingClientRect().left,
      railRight: rail.getBoundingClientRect().right,
    };
  });
  expect(geometry).not.toBeNull();
  expect(geometry!.railLeft - geometry!.cardLeft).toBeGreaterThanOrEqual(0);
  expect(geometry!.railLeft - geometry!.cardLeft).toBeLessThan(10);
  expect(geometry!.railRight).toBeLessThanOrEqual(geometry!.activeLeft);
  expect(Math.abs(geometry!.inactiveLeft - geometry!.rootLeft)).toBeLessThanOrEqual(3);
});

test('deleting any selected block requires confirmation; cancelling preserves data', async ({ page }) => {
  await page.goto(storyUrl);
  const root = page.locator('.atlas-story-rich-document');
  const image = root.locator(':scope > .atlas-story-figure[data-atlas-key="public-square"]');
  await image.click();
  await page.getByRole('button', { name: 'Delete current block' }).click();
  const dialog = page.getByRole('alertdialog', { name: 'Confirm delete block' });
  await expect(dialog).toContainText('Delete this image block?');
  await page.getByRole('button', { name: 'Cancel', exact: true }).click();
  await expect(image).toBeVisible();

  await page.getByRole('button', { name: 'Delete current block' }).click();
  await page.getByRole('button', { name: 'Delete block', exact: true }).click();
  await expect(root.locator(':scope > .atlas-story-figure')).toHaveCount(0);
  await page.getByRole('button', { name: 'Existing block forms' }).click();
  await expect(page.locator('.block-composer .content-block')).toHaveCount(2);
});

test('deleting the final content block replaces it with one blank editable text block', async ({ page }) => {
  await page.goto('http://127.0.0.1:6006/iframe.html?id=content-editor-workbench-experimental--blank-node&viewMode=story');
  const root = page.locator('.atlas-story-rich-document');
  await root.locator(':scope > .atlas-story-text p').click();
  await page.getByRole('button', { name: 'Delete current block' }).click();
  await page.getByRole('button', { name: 'Delete block', exact: true }).click();
  await expect(root.locator(':scope > .atlas-story-text')).toHaveCount(1);
  await root.locator(':scope > .atlas-story-text p').click();
  await page.keyboard.type('Able to continue writing');
  await page.getByRole('button', { name: 'Existing block forms' }).click();
  await expect(page.getByLabel('Text (Markdown)').first()).toHaveValue('Able to continue writing');
});


test('plus menu opens beside active block and inserts before or after without a Heading option', async ({ page }) => {
  await page.goto(storyUrl);
  const root = page.locator('.atlas-story-rich-document');
  const image = root.locator(':scope > .atlas-story-figure[data-atlas-key="public-square"]');
  await image.click();
  await page.getByRole('button', { name: 'Add a content block' }).click();
  const menu = page.getByRole('group', { name: 'Insert a content block' });
  await expect(menu).toBeVisible();
  await expect(menu.getByRole('button', { name: 'Before' })).toHaveAttribute('aria-pressed', 'false');
  await expect(menu.getByRole('button', { name: 'After' })).toHaveAttribute('aria-pressed', 'true');
  await expect(menu.getByRole('button', { name: 'Heading' })).toHaveCount(0);
  await menu.getByRole('button', { name: 'Before' }).click();
  await menu.getByRole('button', { name: 'Text block' }).click();
  await expect(root.locator(':scope > *').nth(2)).toHaveAttribute('data-atlas-key', 'public-square');

  await image.click();
  await page.getByRole('button', { name: 'Add a content block' }).click();
  await menu.getByRole('button', { name: 'After' }).click();
  await menu.getByRole('button', { name: 'Section break' }).click();
  await expect(root.locator(':scope > *').nth(2)).toHaveAttribute('data-atlas-key', 'public-square');
  await expect(root.locator(':scope > *').nth(3).locator('hr')).toBeVisible();
});

test('selecting text shows a formatting bubble; clearing the selection hides it', async ({ page }) => {
  await page.goto(storyUrl);
  const p = page.locator('.atlas-story-text[data-atlas-key="intro"] p').first();
  await p.click();
  await p.press('Home');
  await p.press('Shift+ArrowRight');
  const pop = page.getByRole('toolbar', { name: 'Selected text formatting' });
  await expect(pop).toBeVisible();
  await expect(pop.getByRole('button', { name: 'Bold highlighted text' })).toBeVisible();
  await expect(pop.getByRole('button', { name: 'Italicize highlighted text' })).toBeVisible();
  await expect(pop.getByRole('button', { name: 'Link highlighted text' })).toBeVisible();
  await expect(pop.getByRole('button', { name: 'Heading for highlighted text' })).toBeVisible();
  await expect(pop.getByRole('button', { name: 'List for highlighted text' })).toBeVisible();
  await pop.getByRole('button', { name: 'Bold highlighted text' }).click();
  await expect(p.locator('strong').first()).toBeVisible();
  await p.press('ArrowRight');
  await expect(pop).toHaveCount(0);
  await expect(page.getByRole('toolbar', { name: 'Text formatting' })).toBeVisible();
});

test('clicking outside the editable document hides block controls and restores full width', async ({ page }) => {
  await page.goto(storyUrl);
  const root = page.locator('.atlas-story-rich-document');
  const p = root.locator('.atlas-story-text[data-atlas-key="intro"] p').first();
  await expect(page.getByRole('group', { name: 'Current block controls' })).toHaveCount(0);
  await expect(root.locator(':scope > .atlas-story-active-block')).toHaveCount(0);
  await p.click();
  await expect(page.getByRole('group', { name: 'Current block controls' })).toBeVisible();
  await expect(root.locator(':scope > .atlas-story-active-block')).toHaveCount(1);
  await page.getByRole('button', { name: 'Add a content block' }).click();
  await expect(page.getByRole('group', { name: 'Insert a content block' })).toBeVisible();
  await page.getByRole('textbox', { name: 'Node title' }).click();
  await expect(page.getByRole('group', { name: 'Current block controls' })).toHaveCount(0);
  await expect(page.getByRole('group', { name: 'Insert a content block' })).toHaveCount(0);
  await expect(root.locator(':scope > .atlas-story-active-block')).toHaveCount(0);

  await p.click();
  await expect(root.locator(':scope > .atlas-story-active-block')).toHaveCount(1);
  await page.locator('.workbench-header').click();
  await expect(page.getByRole('group', { name: 'Current block controls' })).toHaveCount(0);
  await expect(root.locator(':scope > .atlas-story-active-block')).toHaveCount(0);
});

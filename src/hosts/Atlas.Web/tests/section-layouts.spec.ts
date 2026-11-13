import { expect, test } from '@playwright/test';
import { applyLayout, blockAt, example, moveBlock } from '../storybook/SectionLayouts/sectionLayouts';

test('a smaller layout keeps overflow blocks in reading order', () => {
  const original = example('editorial');
  const first = original.sections[0];
  const next = applyLayout(original, first.id, 'full');

  expect(next.blocks).toEqual(original.blocks);
  expect(next.sections.map(({ layoutId }) => layoutId)).toEqual(['full', 'full', 'equal']);
  expect(blockAt(next, first.id, 'left')?.id).toBe('opening');
  expect(blockAt(next, next.sections[1].id, 'left')?.id).toBe('street-photo');
});

test('moving into an occupied slot swaps blocks and layout changes retain an empty slot', () => {
  const original = example('editorial');
  const first = original.sections[0].id;
  const swapped = moveBlock(original, 'opening', first, 'right');
  expect(blockAt(swapped, first, 'left')?.id).toBe('street-photo');
  expect(blockAt(swapped, first, 'right')?.id).toBe('opening');

  const emptyLeft = { ...swapped, assignments: swapped.assignments.filter(({ blockId }) => blockId !== 'street-photo') };
  const resized = applyLayout(emptyLeft, first, 'wide-right');
  expect(blockAt(resized, first, 'left')).toBeUndefined();
  expect(blockAt(resized, first, 'right')?.id).toBe('opening');
});

test('the workbench creates a reusable layout and shows the same draft in feed and node previews', async ({ page }) => {
  await page.setViewportSize({ width: 1180, height: 900 });
  await page.route('https://images.unsplash.com/**', (route) => route.abort());
  await page.goto('http://127.0.0.1:6006/iframe.html?id=content-section-layout-workbench--editorial&viewMode=story');

  const section = page.getByRole('region', { name: 'Section 1', exact: true });
  await section.getByRole('textbox', { name: /Node content/ }).click();
  await section.getByRole('button', { name: 'Change layout' }).click();
  await section.getByText('Customize layout').click();
  await section.getByLabel('Layout name').fill('Pilot layout');
  await section.getByRole('button', { name: 'Save as a new preset' }).click();
  await expect(section.getByRole('button', { name: 'Change layout' })).toHaveAttribute('aria-expanded', 'false');
  await page.getByRole('button', { name: 'Add section' }).click();
  await page.getByLabel('Layout', { exact: true }).selectOption({ label: 'Pilot layout' });
  await page.getByRole('button', { name: 'Add this layout' }).click();

  await page.getByText('Backend data preview').click();
  const data = JSON.parse(await page.locator('.inspector pre').innerText());
  const layout = data.layouts.find((entry: { name: string }) => entry.name === 'Pilot layout');
  expect(layout.slots).toHaveLength(2);
  expect(data.sections.filter((entry: { layoutId: string }) => entry.layoutId === layout.id)).toHaveLength(2);
  expect(data.assignments).toHaveLength(4);

  await page.getByRole('checkbox', { name: 'Phone width' }).check();
  await page.getByRole('button', { name: 'Feed preview' }).click();
  await page.getByRole('button', { name: 'Expand', exact: true }).click();
  await expect(page.getByRole('button', { name: 'Collapse', exact: true })).toBeVisible();
  await page.getByRole('button', { name: 'Read More', exact: true }).click();
  await expect(page.getByRole('heading', { name: 'A greener public square' })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Read More', exact: true })).toHaveCount(0);
});

test('the editor formats a text slot and swaps blocks using the destination on the page', async ({ page }) => {
  await page.route('https://images.unsplash.com/**', (route) => route.abort());
  await page.goto('http://127.0.0.1:6006/iframe.html?id=content-section-layout-workbench--editorial&viewMode=story');
  const section = page.getByRole('region', { name: 'Section 1', exact: true });
  const text = section.getByRole('textbox', { name: /Node content/ });
  await text.click();
  await text.evaluate((element) => {
    const range = document.createRange();
    const paragraph = element.querySelector('p')!;
    range.selectNodeContents(paragraph);
    const selection = window.getSelection()!;
    selection.removeAllRanges();
    selection.addRange(range);
  });
  await page.getByRole('button', { name: 'Bold', exact: true }).click();
  await expect(text.locator('strong')).toHaveCount(1);

  await expect(page.getByRole('button', { name: 'Move block', exact: true })).toHaveCount(1);
  await expect(page.getByRole('button', { name: 'Change layout' })).toHaveCount(1);
  await section.getByRole('button', { name: 'Move block', exact: true }).click();
  await expect(page.getByRole('button', { name: 'Move block', exact: true })).toHaveCount(0);
  await expect(page.getByRole('button', { name: 'Change layout' })).toHaveCount(0);
  await expect(page.getByRole('button', { name: /Swap here/ })).toHaveCount(3);
  await section.getByRole('button', { name: 'Swap here · Section 1 · Right' }).click();
  await expect(section.locator('.edit-slot').first().getByRole('img')).toHaveCount(1);
  await expect(section.locator('.edit-slot').last().locator('strong')).toHaveCount(1);
});

test('duplicates a section with independent blocks, photos and slot alignment', async () => {
  const { duplicateSection } = await import('../storybook/SectionLayouts/sectionLayouts');
  const draft = example('editorial');
  draft.assignments[0].vertical = 'center';
  const original = draft.sections[0];
  const result = duplicateSection(draft, original.id);
  const copy = result.sections[1];
  expect(copy.layoutId).toBe(original.layoutId);
  expect(copy.id).not.toBe(original.id);
  const first = blockAt(result, original.id, 'right');
  const second = blockAt(result, copy.id, 'right');
  expect(first?.id).not.toBe(second?.id);
  if (first?.kind !== 'image' || second?.kind !== 'image') throw new Error('Expected image blocks');
  expect(first.photos[0].id).not.toBe(second.photos[0].id);
  expect(result.assignments.find(({ sectionId, slotId }) => sectionId === copy.id && slotId === 'left')?.vertical).toBe('center');
});

test('confirms block removal and undoes block and empty section removal in order', async ({ page }) => {
  await page.route('https://images.unsplash.com/**', (route) => route.abort());
  await page.goto('http://127.0.0.1:6006/iframe.html?id=content-section-layout-workbench--editorial&viewMode=story');
  const sections = page.locator('.edit-section');
  const section = sections.first();
  const undo = page.getByRole('button', { name: 'Undo', exact: true });
  await expect(undo).toBeDisabled();
  await expect(section.getByRole('button', { name: 'Delete empty section' })).toHaveCount(0);
  const text = section.getByRole('textbox', { name: /Node content/ });
  await text.click();
  page.once('dialog', (dialog) => dialog.dismiss());
  await section.getByRole('button', { name: 'Remove block' }).click();
  await expect(text).toBeVisible();
  await expect(undo).toBeDisabled();

  page.once('dialog', (dialog) => dialog.accept());
  await section.getByRole('button', { name: 'Remove block' }).click();
  await expect(text).toHaveCount(0);
  await page.keyboard.press('Control+z');
  await expect(text).toBeVisible();

  await text.click();
  page.once('dialog', (dialog) => dialog.accept());
  await section.getByRole('button', { name: 'Remove block' }).click();
  await section.getByRole('button', { name: /Edit Buildings along/ }).click();
  page.once('dialog', (dialog) => dialog.accept());
  await section.getByRole('button', { name: 'Remove block' }).click();
  await section.getByRole('button', { name: 'Delete empty section' }).click();
  await expect(sections).toHaveCount(1);
  await undo.click();
  await expect(sections).toHaveCount(2);
  await section.getByRole('button', { name: /Add block/ }).first().focus();
  await expect(section.getByRole('button', { name: 'Delete empty section' })).toBeVisible();
  await undo.click();
  await expect(section.getByRole('img')).toBeVisible();
  await undo.click();
  await expect(text).toBeVisible();
  await expect(undo).toBeDisabled();
});

test('only authored section breaks appear in the editor and previews', async ({ page }) => {
  await page.route('https://images.unsplash.com/**', (route) => route.abort());
  await page.goto('http://127.0.0.1:6006/iframe.html?id=content-section-layout-workbench--editorial&viewMode=story');
  const rule = page.locator('hr.section-break');
  const second = page.getByRole('region', { name: 'Section 2', exact: true });
  await expect(rule).toHaveCount(0);
  await second.getByRole('textbox', { name: /Node content/ }).first().click();
  await second.getByRole('button', { name: 'Add section break' }).click();
  await expect(second.locator('hr.section-break')).toHaveCount(1);
  await page.getByRole('button', { name: 'Feed preview' }).click();
  await expect(rule).toHaveCount(1);
  await page.getByRole('button', { name: 'Node page' }).click();
  await expect(rule).toBeVisible();
  await page.getByRole('button', { name: 'Edit', exact: true }).click();
  await second.getByRole('textbox', { name: /Node content/ }).first().click();
  await second.getByRole('button', { name: 'Remove section break' }).click();
  await expect(rule).toHaveCount(0);
  await page.getByRole('button', { name: 'Undo', exact: true }).click();
  await expect(rule).toHaveCount(1);
});

test('clicking away dismisses block controls and H6 survives preview', async ({ page }) => {
  await page.route('https://images.unsplash.com/**', (route) => route.abort());
  await page.goto('http://127.0.0.1:6006/iframe.html?id=content-section-layout-workbench--editorial&viewMode=story');
  const section = page.getByRole('region', { name: 'Section 1', exact: true });
  await section.getByRole('button', { name: /Edit Buildings along/ }).click();
  await expect(section.getByLabel('Image URL')).toBeVisible();
  const editor = section.getByRole('textbox', { name: /Node content/ });
  await editor.locator('p').first().click();
  await expect(section.getByLabel('Image URL')).toHaveCount(0);
  const headings = page.getByRole('combobox', { name: 'Heading level' });
  await headings.selectOption('6');
  await expect(editor.locator('h6')).toHaveCount(1);
  await page.getByRole('textbox', { name: 'Node title' }).click();
  await expect(headings).toBeDisabled();
  await expect(page.getByRole('button', { name: 'Change layout' })).toHaveCount(0);
  await expect(page.getByRole('button', { name: 'Move block', exact: true })).toHaveCount(0);
  await expect(section.getByRole('button', { name: 'Remove block' })).toHaveCount(0);
  await page.getByRole('button', { name: 'Node page' }).click();
  await expect(page.locator('h6')).toHaveCount(1);
});

test('layout choices preview immediately and cancel or click-away resolve the change', async ({ page }) => {
  await page.route('https://images.unsplash.com/**', (route) => route.abort());
  await page.goto('http://127.0.0.1:6006/iframe.html?id=content-section-layout-workbench--editorial&viewMode=story');
  const section = page.getByRole('region', { name: 'Section 1', exact: true });
  const grid = section.locator('.grid');
  const title = page.getByRole('textbox', { name: 'Node title' });
  const change = async () => {
    await section.getByRole('textbox', { name: /Node content/ }).click();
    await section.getByRole('button', { name: 'Change layout' }).click();
    await section.getByLabel('Layout', { exact: true }).selectOption('full');
    await expect(grid.locator('.edit-slot')).toHaveCount(1);
  };
  await change();
  await section.getByRole('button', { name: 'Cancel', exact: true }).click();
  await expect(grid.locator('.edit-slot')).toHaveCount(2);
  await change();
  await title.click();
  await expect(grid.locator('.edit-slot')).toHaveCount(1);
  await page.getByRole('button', { name: 'Undo', exact: true }).click();
  await expect(grid.locator('.edit-slot')).toHaveCount(2);
});

test('image framing and centered captions carry from editing into the feed', async ({ page }) => {
  await page.route('https://images.unsplash.com/**', (route) => route.fulfill({
    contentType: 'image/svg+xml',
    body: '<svg xmlns="http://www.w3.org/2000/svg" width="240" height="120"><rect width="240" height="120" fill="green"/></svg>',
  }));
  await page.goto('http://127.0.0.1:6006/iframe.html?id=content-section-layout-workbench--editorial&viewMode=story');
  const caption = page.getByText('Make room to walk and gather.', { exact: true });
  await expect(caption).toHaveCSS('text-align', 'center');
  await page.getByRole('button', { name: /Edit Buildings along/ }).click();
  await page.getByLabel('Crop shape').selectOption('2 / 3');
  await page.getByLabel('Corners').selectOption('square');
  await page.getByRole('button', { name: 'Done', exact: true }).click();
  await page.getByRole('button', { name: 'Feed preview' }).click();
  await expect(caption).toHaveCSS('text-align', 'center');
  const frame = page.locator('.frame.crop');
  await expect(frame).toHaveCSS('aspect-ratio', '2 / 3');
  await expect(frame.locator('img')).toHaveCSS('object-fit', 'cover');
  await expect(frame).toHaveCSS('border-top-left-radius', '0px');
});

test('an image wraps at a selected paragraph in the editor and previews', async ({ page }) => {
  await page.setViewportSize({ width: 1180, height: 900 });
  await page.route('https://images.unsplash.com/**', (route) => route.fulfill({
    contentType: 'image/svg+xml',
    body: '<svg xmlns="http://www.w3.org/2000/svg" width="320" height="200"><rect width="320" height="200" fill="teal"/></svg>',
  }));
  await page.goto('http://127.0.0.1:6007/iframe.html?id=content-section-layout-workbench--editorial&viewMode=story');

  const section = page.getByRole('region', { name: 'Section 1', exact: true });
  await section.getByRole('button', { name: 'Edit Buildings along a city street' }).click();
  await section.getByLabel('Wrap text').selectOption('right');
  await section.getByRole('button', { name: 'Done', exact: true }).click();
  await section.getByText(/^Start with the places/).click();
  await page.getByRole('button', { name: 'Move image here' }).click();
  await expect(section.locator('.atlas-story-text > p + .ProseMirror-widget .section-wrapped-image')).toHaveCSS('float', 'right');

  const image = section.getByRole('button', { name: 'Edit Buildings along a city street' });
  await image.dragTo(section.getByText(/^A public square can be/));
  await expect(section.locator('.atlas-story-text > .ProseMirror-widget:first-child').getByRole('button', {
    name: 'Edit Buildings along a city street',
  })).toBeVisible();
  await image.dragTo(section.getByText(/^Start with the places/));
  await image.click();
  await section.getByRole('slider', { name: /Image width/ }).focus();
  await page.keyboard.press('Home');
  await section.getByRole('button', { name: 'Done', exact: true }).click();
  const float = section.locator('.section-wrapped-image');
  expect(await float.evaluate((element) => element.getBoundingClientRect().width)).toBeLessThan(160);
  expect(await image.evaluate((element) => element.getBoundingClientRect().width)).toBeCloseTo(
    await float.evaluate((element) => element.getBoundingClientRect().width),
  );

  await page.getByRole('button', { name: 'Feed preview' }).click();
  const wrapped = page.locator('.markdown > p + .section-wrapped-image');
  await expect(wrapped).toHaveCSS('float', 'right');
  await page.getByRole('button', { name: 'Read More', exact: true }).click();
  await expect(wrapped.getByRole('button', { name: 'View Buildings along a city street' })).toBeVisible();
  await page.getByRole('checkbox', { name: 'Phone width' }).check();
  await expect(wrapped).toHaveCSS('float', 'none');
});

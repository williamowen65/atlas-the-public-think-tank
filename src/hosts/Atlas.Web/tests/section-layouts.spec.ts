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
  await section.getByRole('button', { name: 'Change layout' }).click();
  await section.getByText('Customize and save a layout').click();
  await section.getByLabel('Layout name').fill('Pilot layout');
  await section.getByRole('button', { name: 'Save and apply layout' }).click();
  await expect(section.getByText('Pilot layout', { exact: true })).toBeVisible();
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

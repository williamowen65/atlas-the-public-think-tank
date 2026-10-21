import AxeBuilder from '@axe-core/playwright';
import { expect, test } from '@playwright/test';

const story = (id: string, globals = '') =>
  `http://127.0.0.1:6006/iframe.html?id=${id}&viewMode=story${globals ? `&globals=${globals}` : ''}`;

/** Given the standard fixture, keyboard changes stay in range and notify its parent. */
test('standard voting supports keyboard, Home and End', async ({ page }) => {
  await page.goto(story('components-votingdial--playground'));
  const dial = page.getByRole('spinbutton', { name: 'Node importance', exact: true });
  await dial.focus();
  await page.keyboard.press('ArrowUp');
  await expect(dial).toHaveAttribute('aria-valuenow', '6');
  await expect(page.getByRole('status')).toHaveText('Preview selection: 6 out of 10');
  await page.keyboard.press('End');
  await page.keyboard.press('ArrowUp');
  await expect(dial).toHaveAttribute('aria-valuenow', '10');
  await page.keyboard.press('Home');
  await page.keyboard.press('ArrowDown');
  await expect(dial).toHaveAttribute('aria-valuenow', '0');
});

/** Null stays distinct from an explicit zero selection. */
test('unrated is distinct from zero', async ({ page }) => {
  await page.goto(story('components-votingdial--playground&args=initialValue:!null;average:!null;totalVotes:0'));
  const dial = page.getByRole('spinbutton');
  await expect(dial).toHaveAttribute('aria-valuetext', /Unrated/);
  await expect(page.getByRole('status')).toHaveText('No rating selected');
  await dial.focus();
  await page.keyboard.press('Home');
  await expect(page.getByRole('status')).toHaveText('Preview selection: 0 out of 10');
});

/** Compact keyboard selection retains the full control contract on a narrow card. */
test('compact voting works without horizontal overflow', async ({ page }) => {
  await page.setViewportSize({ width: 320, height: 700 });
  await page.goto(story('components-votingdial--playground&args=compact:true'));
  await page.getByRole('spinbutton').focus();
  await page.keyboard.press('ArrowUp');
  await expect(page.getByRole('status')).toHaveText('Preview selection: 6 out of 10');
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(320);
});

/** Disabled pointer, wheel and keyboard input must not change the displayed value. */
test('disabled voting remains unchanged', async ({ page }) => {
  await page.goto(story('components-votingdial--playground&args=disabled:true'));
  const dial = page.getByRole('spinbutton');
  await expect(dial).toHaveAttribute('aria-disabled', 'true');
  await dial.dispatchEvent('wheel', { deltaY: 100 });
  await dial.dispatchEvent('pointerdown', { pointerType: 'mouse', button: 0, pointerId: 1, clientY: 100 });
  await dial.dispatchEvent('pointermove', { pointerType: 'mouse', pointerId: 1, clientY: 10 });
  await dial.dispatchEvent('pointerup', { pointerType: 'mouse', pointerId: 1, clientY: 10 });
  await page.getByRole('spinbutton').dispatchEvent('keydown', { key: 'ArrowUp' });
  await expect(page.getByRole('status')).toHaveText('Preview selection: 5 out of 10');
});

/** Two mounted controls own independent values and contain no repeated style blocks. */
test('multiple dials are independent and share emitted styles', async ({ page }) => {
  await page.goto(story('compositions-voting-comparison--playground'));
  await page.getByRole('spinbutton', { name: 'First node importance', exact: true }).focus();
  await page.keyboard.press('ArrowUp');
  await expect(page.getByRole('spinbutton', { name: 'First node importance', exact: true })).toHaveAttribute('aria-valuenow', '5');
  await expect(page.getByRole('spinbutton', { name: 'Second node importance', exact: true })).toHaveAttribute('aria-valuenow', '0');
  await expect(page.locator('.voting-dial')).toHaveCount(2);
  await expect(page.locator('.voting-dial style')).toHaveCount(0);
});

/** Preview globals apply the same token definitions to layouts and components. */
test('global theme and density apply across story types', async ({ page }) => {
  for (const id of ['styles-and-layout-typography--playground', 'components-votingdial--playground']) {
    await page.goto(story(id, 'theme:dark;density:compact'));
    await expect(page.locator('html')).toHaveAttribute('data-theme', 'dark');
    await expect(page.locator('html')).toHaveAttribute('data-density', 'compact');
    expect(await page.evaluate(() => getComputedStyle(document.documentElement).getPropertyValue('--atlas-space').trim())).toBe('.5rem');
    const results = await new AxeBuilder({ page }).include('#storybook-root').analyze();
    expect(results.violations).toEqual([]);
  }
});

/** The card composition receives a dial selection without any live API. */
test('card composition observes voting callbacks', async ({ page }) => {
  await page.goto(story('compositions-node-voting-preview--playground'));
  await page.getByRole('spinbutton').focus();
  await page.keyboard.press('ArrowUp');
  await expect(page.getByRole('status')).toHaveText('Preview selection: 5 out of 10');
});

/** Reset restores code-defined typography arguments rather than reloading edited args. */
test('typography reset restores code defaults', async ({ page }) => {
  await page.goto(story('styles-and-layout-typography--playground&args=fontSize:24;paragraphGap:40'));
  await expect(page.locator('.specimen')).toHaveCSS('font-size', '24px');
  await page.getByRole('button', { name: 'Reset typography to code defaults' }).click();
  await expect(page.locator('.specimen')).toHaveCSS('font-size', '16px');
  await expect(page.locator('.specimen p').first()).toHaveCSS('margin-bottom', '16px');
});

/** The interactive development renderer must support initial render, args and reset. */
test('typography development server renders and resets after argument updates', async ({ page }) => {
  await page.goto('http://127.0.0.1:6007/iframe.html?id=styles-and-layout-typography--playground&viewMode=story&args=fontSize:24');
  await expect(page.getByRole('heading', { name: 'Atlas typography' })).toBeVisible();
  await expect(page.locator('.specimen')).toHaveCSS('font-size', '24px');
  await page.getByRole('button', { name: 'Reset typography to code defaults' }).click();
  await expect(page.locator('.specimen')).toHaveCSS('font-size', '16px');
  await expect(page.getByText('Storybook preview hooks can only be called', { exact: false })).toHaveCount(0);
});

/** Wheel/drag work on both layouts, with no visible arrows or text labels. */
test('dial supports wheel and dragging in both layouts', async ({ page }) => {
  for (const compact of [false, true]) {
    await page.goto(story(`components-votingdial--playground&args=compact:${compact};totalVotes:20`));
    const dial = page.getByRole('spinbutton');
    await expect(dial).toHaveAttribute('aria-valuenow', '5');
    await expect(page.locator('.voting-dial button')).toHaveCount(0);
    await expect(page.locator('.vote-count')).toHaveAccessibleName('20 votes');
    await expect(page.locator('.count')).toHaveText('20');
    await expect(page.locator('.voting-dial')).not.toContainText(/Average|votes|Unrated|Your selection/);
    await dial.hover();
    await page.mouse.wheel(0, 88);
    await expect(dial).toHaveAttribute('aria-valuenow', '3');
    await expect.poll(() => dial.evaluate(el => el.scrollTop)).toBeCloseTo(7 * 44, 0);
    const box = (await dial.boundingBox())!;
    await page.mouse.move(box.x + box.width / 2, box.y + box.height / 2);
    await page.mouse.down();
    await page.mouse.move(box.x + box.width / 2, box.y + box.height / 2 - 44, { steps: 8 });
    if (compact) {
      await expect(page.locator('.selection-window')).toHaveCSS('transform', 'matrix(1, 0, 0, 1, 0, 0)');
    } else {
      // A 44px pull now shifts the standard lens by 13.2px (previously 7.92px).
      await expect.poll(() => page.locator('.selection-window').evaluate(el =>
        Math.abs(new DOMMatrix(getComputedStyle(el).transform).m42))).toBeGreaterThan(12);
    }
    await page.mouse.up();
    await expect(page.locator('.selection-window')).toHaveCSS('transform', 'matrix(1, 0, 0, 1, 0, 0)');
    await expect(dial).toHaveAttribute('aria-valuenow', '2');
    await expect(page.getByRole('status')).toHaveText('Preview selection: 2 out of 10');
    await dial.focus();
    await page.keyboard.press('End');
    await expect(dial).toHaveAttribute('aria-valuenow', '10');
  }
});

/** A real browser touch gesture scrolls and selects in standard and compact views. */
test('native touch scroll snaps and updates the controlled selection', async ({ browser }) => {
  const context = await browser.newContext({ hasTouch: true, viewport: { width: 390, height: 844 } });
  const page = await context.newPage();
  const session = await context.newCDPSession(page);
  for (const compact of [false, true]) {
    await page.goto(story(`components-votingdial--playground&args=compact:${compact}`));
    const dial = page.getByRole('spinbutton');
    await expect.poll(() => dial.evaluate(el => el.scrollTop)).toBe(220);
    const box = (await dial.boundingBox())!;
    const x = box.x + box.width / 2;
    const y = box.y + box.height / 2;
    await session.send('Input.dispatchTouchEvent', { type: 'touchStart', touchPoints: [{ x, y }] });
    for (let step = 1; step <= 8; step++) {
      await session.send('Input.dispatchTouchEvent', { type: 'touchMove', touchPoints: [{ x, y: y - step * 6 }] });
      await page.waitForTimeout(25);
    }
    await session.send('Input.dispatchTouchEvent', { type: 'touchEnd', touchPoints: [] });
    await expect.poll(async () => Number(await dial.getAttribute('aria-valuenow'))).toBeLessThan(5);
    await expect.poll(() => dial.evaluate(el => Math.abs(el.scrollTop / 44 - Math.round(el.scrollTop / 44)))).toBeLessThan(.02);
    const rating = await dial.getAttribute('aria-valuenow');
    await expect(page.getByRole('status')).toHaveText(`Preview selection: ${rating} out of 10`);
  }
  await context.close();
});

/** State colors change at the first selection, while zero remains a valid rating. */
test('unrated color changes when zero is selected', async ({ page }) => {
  await page.goto(story('components-votingdial--playground&args=initialValue:!null'));
  const window = page.locator('.selection-window');
  const before = await window.evaluate(el => getComputedStyle(el).backgroundColor);
  await page.getByRole('spinbutton').focus();
  await page.keyboard.press('Home');
  await expect(page.locator('.selector')).not.toHaveClass(/unrated/);
  expect(await window.evaluate(el => getComputedStyle(el).backgroundColor)).not.toBe(before);
  await expect(page.getByRole('spinbutton')).toHaveAttribute('aria-valuenow', '0');
});

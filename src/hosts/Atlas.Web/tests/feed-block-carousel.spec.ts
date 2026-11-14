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
  await expect(feed).toContainText('1 of 4');
  await expect(feed.locator('.feed-block-slide')).toHaveAttribute('data-block-id', 'opening');
  await expect(feed.locator('.feed-block-slide')).toHaveAttribute('data-block-kind', 'markdown');
  await feed.getByRole('button', { name: 'Next node content block' }).click();
  await expect(feed.locator('.feed-block-slide')).toHaveAttribute('data-block-id', 'first-photo');
  await expect(feed.locator('.feed-block-slide figure img')).toBeVisible();
  await feed.getByRole('button', { name: 'Next node content block' }).click();
  await expect(feed.locator('.feed-block-slide')).toHaveAttribute('data-block-id', 'long-section');
  await feed.getByRole('button', { name: 'Next node content block' }).click();
  await expect(feed.locator('.feed-block-slide')).toHaveAttribute('data-block-id', 'second-photo');
  await expect(feed).toContainText('4 of 4');
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
  await expect(feed).toContainText('1 of 4');
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
  await expect(feed).toContainText('2 of 4');
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
  await expect(feed).toContainText('1 of 3');
  await expect(feed.locator('.feed-block-slide')).toHaveAttribute('data-block-id', 'intro');
  await feed.getByRole('button', { name: 'Show node block 2' }).click();
  await expect(feed.locator('.feed-block-slide')).toHaveAttribute('data-block-id', 'public-square');
  await feed.getByRole('button', { name: 'Read more' }).click();
  await expect(page.getByRole('article', { name: 'Full node preview' })).toBeVisible();
  await expect(carousel(page)).toHaveCount(0);
});


test('Markdown intro includes an embedded image in the same feed slide as its text', async ({ page }) => {
  await page.goto(storyUrl);
  // This is one original Markdown block; the image must not create extra slides.
  await expect(page.locator('.model-inspector ol > li')).toHaveCount(4);
  const editorText = page.locator('.atlas-story-text[data-atlas-key="opening"]');
  await expect(editorText.getByRole('img', { name: 'Street framed by tall buildings' })).toBeVisible();
  await page.getByRole('button', { name: 'Feed', exact: true }).click();
  const feed = carousel(page);
  await expect(feed.locator('.feed-block-slide')).toHaveAttribute('data-block-id', 'opening');
  await expect(feed).toContainText('What if our public square made more room');
  await expect(feed.getByRole('img', { name: 'Street framed by tall buildings' })).toBeVisible();
  await expect(feed).toContainText('1 of 4');
  await expect(feed.locator('.feed-block-slide')).toHaveCount(1);
  await feed.getByRole('button', { name: 'Read more' }).click();
  await expect(page.getByRole('article', { name: 'Full node preview' }))
    .toContainText('A little greenery could change how this space feels');
  await expect(page.getByRole('article', { name: 'Full node preview' })
    .getByRole('img', { name: 'Street framed by tall buildings' })).toBeVisible();
  await expect(page.locator('.model-inspector ol > li')).toHaveCount(4);
});

test('Markdown image inserted in Tiptap survives switching to block forms, Feed and Full Node', async ({ page }) => {
  await page.goto(storyUrl);
  const intro = page.locator('.atlas-story-text[data-atlas-key="opening"]');
  await intro.locator('p').first().click();
  await page.getByRole('button', { name: 'Insert image within text' }).click();
  const form = page.getByRole('form', { name: 'Insert image into Markdown' });
  await expect(form).toBeVisible();
  await form.getByLabel('Inline image URL').fill('https://example.org/another-garden.jpg');
  await form.getByLabel('Inline image alternative text').fill('New garden at a downtown plaza');
  await form.getByRole('button', { name: 'Insert image', exact: true }).click();
  await expect(intro.getByRole('img', { name: 'New garden at a downtown plaza' })).toBeVisible();
  await page.getByRole('button', { name: 'Existing block forms' }).click();
  await expect(page.getByLabel('Text (Markdown)').first())
    .toHaveValue(/!\[New garden at a downtown plaza\]\(https:\/\/example\.org\/another-garden\.jpg\)/);
  await page.getByRole('button', { name: 'Feed', exact: true }).click();
  await expect(carousel(page).getByRole('img', { name: 'New garden at a downtown plaza' })).toBeVisible();
  await page.getByRole('button', { name: 'Full node', exact: true }).click();
  await expect(page.getByRole('article', { name: 'Full node preview' })
    .getByRole('img', { name: 'New garden at a downtown plaza' })).toBeVisible();
  await expect(page.locator('.model-inspector ol > li')).toHaveCount(4);
});

test('editing Markdown image syntax in block forms shows text and image together without adding a slide', async ({ page }) => {
  await page.goto(storyUrl);
  await page.getByRole('button', { name: 'Existing block forms' }).click();
  const markdown = page.getByLabel('Text (Markdown)').first();
  await markdown.fill('A tiny garden photo in Markdown.\n\n![A shady public square](https://example.org/square.jpg)\n\nMore detail below.');
  await page.getByRole('button', { name: 'Feed', exact: true }).click();
  const feed = carousel(page);
  await expect(feed).toContainText('A tiny garden photo in Markdown.');
  await expect(feed.getByRole('img', { name: 'A shady public square' })).toBeVisible();
  await expect(feed).toContainText('1 of 4');
  await feed.getByRole('button', { name: 'Read more' }).click();
  await expect(page.getByRole('article', { name: 'Full node preview' }))
    .toContainText('More detail below.');
  await expect(page.locator('.model-inspector ol > li')).toHaveCount(4);
});

test('feed slide visually belongs to node card without inner border or block-type labels', async ({ page }) => {
  await page.goto(storyUrl);
  await page.getByRole('button', { name: 'Feed', exact: true }).click();
  const feed = carousel(page);
  await expect(feed.getByText('Explore this node')).toHaveCount(0);
  await expect(feed.getByText('Text block')).toHaveCount(0);
  await expect(feed.getByText('Image block')).toHaveCount(0);
  const metrics = await feed.evaluate(root => {
    const viewport = root.querySelector('.viewport');
    const slide = root.querySelector('.feed-block-slide');
    const text = root.querySelector('.block-preview');
    const previous = root.querySelector('.nav.previous');
    const next = root.querySelector('.nav.next');
    if (!viewport || !slide || !text || !previous || !next) return null;
    const v = viewport.getBoundingClientRect();
    const s = slide.getBoundingClientRect();
    const t = text.getBoundingClientRect();
    const p = previous.getBoundingClientRect();
    const n = next.getBoundingClientRect();
    return {
      border: getComputedStyle(viewport).borderTopWidth,
      slideInset: t.left - s.left,
      previousLeft: p.left - v.left,
      nextRight: v.right - n.right,
      arrowWidth: p.width,
    };
  });
  expect(metrics).not.toBeNull();
  expect(metrics!.border).toBe('0px');
  expect(metrics!.slideInset).toBeLessThan(30);
  expect(metrics!.previousLeft).toBeLessThan(4);
  expect(metrics!.nextRight).toBeLessThan(4);
  expect(metrics!.arrowWidth).toBeLessThan(27);
});


test('research Markdown renderer refuses data URLs and raw HTML in feed content', async ({ page }) => {
  await page.goto(storyUrl);
  await page.getByRole('button', { name: 'Existing block forms' }).click();
  await page.getByLabel('Text (Markdown)').first().fill(
    'Safe text. ![Untrusted](data:image/png;base64,AAAA)\n\n<img src="https://example.org/injected.jpg" onerror="alert(1)">'
  );
  await page.getByRole('button', { name: 'Feed', exact: true }).click();
  const feed = carousel(page);
  await expect(feed).toContainText('Safe text.');
  await expect(feed.locator('.story-markdown img')).toHaveCount(0);
  await expect(feed.locator('.story-markdown')).toContainText('img src=');
  await expect(page.locator('.model-inspector ol > li')).toHaveCount(4);
});

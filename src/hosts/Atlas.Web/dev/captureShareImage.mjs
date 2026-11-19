import { mkdir, rename, rm, writeFile } from 'node:fs/promises';
import { randomUUID } from 'node:crypto';
import { parseArgs } from 'node:util';
import { chromium } from '@playwright/test';

const { values } = parseArgs({
  options: {
    origin: { type: 'string', default: 'http://127.0.0.1:5173' },
    node: { type: 'string' },
    layout: { type: 'string' },
  },
});

const origin = new URL(values.origin);
if (!['http:', 'https:'].includes(origin.protocol) ||
    !['localhost', '127.0.0.1', '[::1]'].includes(origin.hostname) ||
    origin.username || origin.password || origin.pathname !== '/' || origin.search || origin.hash) {
  throw new Error('Use a local Atlas origin, such as http://127.0.0.1:5173.');
}

const id = values.node?.toLowerCase();
if (id !== undefined && !/^[0-9a-f]{8}(-[0-9a-f]{4}){3}-[0-9a-f]{12}$/.test(id)) {
  throw new Error('Pass a published node UUID with --node.');
}
const isNode = id !== undefined;
const layout = values.layout ?? (isNode ? 'mobile' : 'wide');
if (layout !== 'mobile' && layout !== 'wide') throw new Error('Use --layout mobile or --layout wide.');

const width = layout === 'mobile' ? 480 : 1200;
const height = layout === 'mobile' ? 252 : 630;
const directory = new URL('../.share-captures/', import.meta.url);
const name = isNode ? `node-${id}` : 'discover';
const destination = new URL(`${name}-${layout}.png`, directory);
const temporary = new URL(`.${randomUUID()}.png`, directory);

const browser = await chromium.launch({ executablePath: process.env.ATLAS_LOCAL_CHROMIUM });
try {
  const page = await browser.newPage({
    viewport: { width, height: isNode ? 1000 : height },
    deviceScaleFactor: 1200 / width,
    ignoreHTTPSErrors: origin.protocol === 'https:',
    colorScheme: 'light',
  });
  const url = new URL(isNode ? '/nodes' : '/', origin);
  if (isNode) {
    url.searchParams.set('id', id);
    url.searchParams.set('share', 'feed');
  }
  const response = await page.goto(url.href, { waitUntil: 'domcontentloaded' });
  if (!response?.ok() || new URL(page.url()).origin !== origin.origin) {
    throw new Error(`Could not load the local Atlas page (${response?.status() ?? 'no response'}).`);
  }

  const target = page.locator(isNode ? '#main-content [data-share-node] article.node-card' : '#main-content article.node-card').first();
  await target.waitFor({ state: 'visible', timeout: 30_000 });
  await target.scrollIntoViewIfNeeded();
  await target.locator("[data-feed-preview-ready='true']").waitFor({ state: 'attached' });
  await page.evaluate(() => document.fonts.ready);
  if (isNode) {
    // Keep the published node's title and opening content in frame.
    await target.evaluate(element => element.scrollIntoView({ block: 'start', behavior: 'instant' }));
  } else {
    await page.evaluate(() => window.scrollTo({ top: 0, behavior: 'instant' }));
  }
  const top = isNode ? Math.max(0, (await target.boundingBox()).y) : 0;

  await mkdir(directory, { recursive: true });
  const image = await page.screenshot({
    clip: { x: 0, y: Math.ceil(top), width, height },
    animations: 'disabled',
    style: isNode
      ? '.app-shell > :not(main) { visibility: hidden; }'
      : '.app-shell > .mini-launcher { right: 4.67vw !important; }',
  });
  await writeFile(temporary, image);
  await rename(temporary, destination);
  console.log(`Saved ${destination.pathname}`);
} finally {
  await browser.close();
  await rm(temporary, { force: true });
}

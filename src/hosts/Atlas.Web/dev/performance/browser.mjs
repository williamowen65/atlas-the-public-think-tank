import { chromium } from '@playwright/test';

export async function measureBrowser(origin, localHttps = false) {
  const browser = await chromium.launch();
  try {
    const page = await browser.newPage({
      viewport: { width: 700, height: 600 },
      ignoreHTTPSErrors: localHttps,
    });
    const feed = page.getByRole('region', { name: 'Node feed', exact: true }).first();
    const started = performance.now();
    await page.goto(origin, { waitUntil: 'domcontentloaded', timeout: 15_000 });
    await feed.locator('[data-feed-page]').first().waitFor({ timeout: 15_000 });
    const firstCardMs = performance.now() - started;
    const pagination = await feed.evaluate(async section => {
      const samples = [];
      const countCards = () => section.querySelectorAll('[data-feed-page]').length;
      const frame = () => new Promise(resolve => requestAnimationFrame(resolve));
      for (let step = 0; step < 3; step++) {
        const tail = section.querySelector('.feed-tail');
        const button = tail?.querySelector('button');
        if (!button) break;
        const count = countCards();
        const boundary = tail.getBoundingClientRect().top + window.scrollY;
        const deadline = performance.now() + 10_000;
        let waitStarted;
        let previousFrame = performance.now();
        let result = { outcome: 'timeout' };
        while (performance.now() < deadline) {
          if (tail.querySelector('button')?.textContent.trim() === 'Retry loading nodes') {
            result = { outcome: 'error' };
            break;
          }
          const now = performance.now();
          const reachedBoundary = window.scrollY + window.innerHeight >= boundary;
          if (countCards() > count) {
            result = { outcome: 'complete', visibleWaitMs: waitStarted === undefined ? 0 : now - waitStarted };
            break;
          }
          if (reachedBoundary) waitStarted ??= now;
          else window.scrollBy(0, Math.ceil((now - previousFrame) * 1.8));
          previousFrame = now;
          await frame();
        }
        samples.push(result);
        if (result.outcome !== 'complete') break;
      }
      return samples;
    });
    return { firstCardMs, pagination };
  } finally {
    await browser.close();
  }
}

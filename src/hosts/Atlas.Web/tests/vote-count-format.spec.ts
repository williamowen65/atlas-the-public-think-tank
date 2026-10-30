import { expect, test } from '@playwright/test';
import { formatVoteCount } from '../src/lib/format/formatVoteCount';

// Boundary cases protect unit changes and avoid rounding a count into the next unit.
for (const [count, expected] of [
  [0, '0'], [1, '1'], [999, '999'], [1000, '1K'], [1099, '1K'],
  [1100, '1.1K'], [3000, '3K'], [4000, '4K'], [999999, '999.9K'],
  [1000000, '1M'], [1250000, '1.2M'], [1000000000, '1B'],
] as const) {
  test(`formats ${count} as ${expected}`, () => {
    expect(formatVoteCount(count)).toBe(expected);
  });
}

test('dial shows the compact count and retains the exact accessible total', async ({ page }) => {
  // Use the real dial story; no session or backend write is needed to render community totals.
  await page.goto('http://127.0.0.1:6006/iframe.html?id=components-votingdial--thousands-of-votes&viewMode=story');
  const count = page.getByRole('img', { name: '4000 votes', exact: true });
  await expect(count).toHaveText('4K');
  await expect(count).toHaveAttribute('title', '4,000');
});

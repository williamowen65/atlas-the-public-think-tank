import { expect, test } from '@playwright/test';

test('footer links reach the public help, feedback, and policy pages', async ({ page }) => {
  await page.route('**/atlas-session-api/**', route => route.fulfill({ status: 401, json: {} }));
  await page.route('**/atlas-public-api/**', route => route.fulfill({ json: [] }));
  await page.goto('/about');
  const footer = page.getByRole('contentinfo');
  for (const [link, path, heading] of [
    ['How to Write a Node', '/how-to-write-a-node', 'How to Write a Node'],
    ['Give feedback', '/feedback', 'Give feedback'],
    ['Support Atlas', '/support', 'Support Atlas'],
    ['Help & support', '/help', 'Help & Support'],
    ['Donate', '/support#donate', 'Support Atlas'],
    ['Privacy policy', '/privacy', 'Privacy policy'],
    ['Terms of use', '/terms', 'Terms of use'],
  ]) {
    await footer.getByRole('link', { name: link, exact: true }).click();
    await expect(page).toHaveURL(new URL(path, page.url()).href);
    await expect(page.getByRole('heading', { level: 1, name: heading })).toBeVisible();
    const fragment = new URL(page.url()).hash;
    if (fragment) await expect(page.locator(fragment)).toBeInViewport();
  }
  await expect(footer.getByRole('link', { name: 'Guides', exact: true })).toHaveCount(0);
  await expect(footer.getByRole('link', { name: 'Become a tester', exact: true })).toHaveCount(0);
});

test('feedback has a private bug channel and a public contribution path', async ({ page }) => {
  await page.route('**/atlas-session-api/**', route => route.fulfill({ status: 401, json: {} }));
  await page.route('**/atlas-public-api/**', route => route.fulfill({ json: [] }));
  await page.goto('/feedback');
  const bugs = page.getByRole('region', { name: 'Something broke' });
  await expect(bugs.getByRole('link', { name: 'atlas@thepublicthinktank.com' }))
    .toHaveAttribute('href', /^mailto:/);
  await expect(bugs.getByRole('link', { name: 'Submit a bug report on GitHub' }))
    .toHaveAttribute('href', /\/issues\/new\?template=bug_report\.yml$/);
  await expect(bugs).toContainText('How can we reproduce it?');
  const ideas = page.getByRole('region', { name: 'Help shape Atlas' });
  await expect(ideas.getByRole('link', { name: 'start a discussion' })).toHaveAttribute('href', '/new');
});

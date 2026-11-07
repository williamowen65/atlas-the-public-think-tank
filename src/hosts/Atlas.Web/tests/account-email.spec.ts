import { expect, test } from '@playwright/test';

// These tests exercise real route rendering and the shared CSRF adapter, with SMTP/SQL mocked at HTTP.
test('confirmation removes the token from the address bar and waits for user action', async ({ page }) => {
  const writes: unknown[] = [];
  await page.route('**/atlas-session-api/**', async route => {
    if (route.request().url().endsWith('/csrf')) return route.fulfill({ json: { token: 'csrf-test' } });
    if (route.request().url().endsWith('/confirm-email')) {
      writes.push(route.request().postDataJSON());
      expect(route.request().headers()['x-csrf-token']).toBe('csrf-test');
      return route.fulfill({ status: 204 });
    }
    return route.fulfill({ status: 401 });
  });
  await page.goto('http://127.0.0.1:4173/confirm-email#userId=00000000-0000-0000-0000-000000000001&token=a%2Bb%2Fc%3D');
  await expect(page.getByRole('button', { name: 'Confirm email', exact: true })).toBeEnabled();
  expect(new URL(page.url()).hash).toBe('');
  expect(writes).toHaveLength(0);
  await page.getByRole('button', { name: 'Confirm email', exact: true }).click();
  await expect(page.getByRole('status')).toContainText('Your email is confirmed');
  expect(writes).toEqual([{ userId: '00000000-0000-0000-0000-000000000001', token: 'a+b/c=' }]);
});

test('missing reset token offers recovery without submitting a password', async ({ page }) => {
  await page.goto('http://127.0.0.1:4173/reset-password');
  await expect(page.getByRole('alert')).toContainText('link is incomplete');
  await expect(page.getByRole('link', { name: 'Request another reset email' })).toHaveAttribute('href', '/forgot-password');
  await expect(page.getByLabel('New password')).toHaveCount(0);
});

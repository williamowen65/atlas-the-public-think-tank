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

test('registration shows live checks and name feedback; resend needs only email', async ({ page }) => {
  let resent: unknown;
  let finishResend!: () => void;
  const resendPending = new Promise<void>(resolve => { finishResend = resolve; });
  await page.route('**/atlas-session-api/**', async route => {
    const path = new URL(route.request().url()).pathname;
    if (path.endsWith('/csrf')) return route.fulfill({ json: { token: 'csrf-test' } });
    if (path.endsWith('/resend-confirmation')) {
      resent = route.request().postDataJSON();
      await resendPending;
      return route.fulfill({ status: 202 });
    }
    if (path.endsWith('/register')) return route.fulfill({ status: 400, json: {
      errors: { DisplayName: ['That display name is already in use. Please choose another.'] },
    } });
    return route.fulfill({ status: 401 });
  });
  await page.goto('http://127.0.0.1:4173/register');
  const resend = page.getByRole('button', { name: 'Resend confirmation instructions' });
  await expect(resend).toBeEnabled();
  await resend.click();
  await expect(page.getByRole('alert')).toContainText('Enter a valid email address above');
  await page.getByLabel('Email', { exact: true }).fill('test@example.com');
  await resend.click();
  await expect(page.getByRole('button', { name: 'Sending confirmation…' })).toHaveAttribute('aria-busy', 'true');
  await expect(page.getByRole('button', { name: 'Create account', exact: true })).toBeDisabled();
  await expect(page.getByRole('button', { name: 'Please wait…' })).toHaveCount(0);
  finishResend();
  await expect(page.getByRole('status')).toContainText('account awaiting confirmation');
  expect(resent).toEqual({ email: 'test@example.com' });
  await expect(page.locator('.password-checks input:checked')).toHaveCount(0);
  await page.getByLabel('Password', { exact: true }).fill('LocalTesting123!');
  await expect(page.locator('.password-checks input:checked')).toHaveCount(5);
  await page.getByRole('button', { name: 'Show password', exact: true }).click();
  await expect(page.getByLabel('Password', { exact: true })).toHaveAttribute('type', 'text');
  await page.getByRole('button', { name: 'Hide password', exact: true }).click();
  await expect(page.getByLabel('Password', { exact: true })).toHaveAttribute('type', 'password');
  await page.getByLabel('Display name', { exact: true }).fill('Taken name');
  await page.getByRole('button', { name: 'Create account', exact: true }).click();
  await expect(page.getByLabel('Display name', { exact: true })).toHaveAttribute('aria-invalid', 'true');
  await expect(page.getByText('That display name is already in use. Please choose another.')).toBeVisible();
  await expect(page.getByRole('link', { name: 'Recover your account' })).toHaveAttribute('href', '/forgot-password');
  await page.getByLabel('Display name', { exact: true }).fill('Another name');
  await expect(page.getByLabel('Display name', { exact: true })).not.toHaveAttribute('aria-invalid', 'true');
  await page.getByRole('button', { name: 'Already have an account? Sign in' }).click();
  await expect(resend).toHaveCount(0);
  await expect(page.getByRole('link', { name: 'Forgot your password?' })).toBeVisible();
});

test('recovery requests email, resets with matching passwords, and offers sign-in', async ({ page }) => {
  const writes: { path: string; body: unknown }[] = [];
  await page.route('**/atlas-session-api/**', async route => {
    const path = new URL(route.request().url()).pathname;
    if (path.endsWith('/csrf')) return route.fulfill({ json: { token: 'csrf-test' } });
    if (path.endsWith('/forgot-password') || path.endsWith('/reset-password')) {
      expect(route.request().headers()['x-csrf-token']).toBe('csrf-test');
      writes.push({ path, body: route.request().postDataJSON() });
      return route.fulfill({ status: path.endsWith('/forgot-password') ? 202 : 204 });
    }
    return route.fulfill({ status: 401 });
  });
  await page.goto('http://127.0.0.1:4173/forgot-password');
  await expect(page.getByRole('link', { name: 'Sign in or resend confirmation' })).toHaveCount(0);
  await page.getByLabel('Email', { exact: true }).fill('recovery@example.com');
  await page.getByRole('button', { name: 'Send reset email', exact: true }).click();
  await expect(page.getByRole('status')).toContainText('If this address has an eligible account');
  expect(writes[0].body).toEqual({ email: 'recovery@example.com' });
  // Simulate opening the provider's email link. Tokens must stay in POST bodies.
  await page.goto('http://127.0.0.1:4173/reset-password#email=recovery%40example.com&token=reset%2Btoken');
  await expect(page.getByLabel('Email', { exact: true })).toHaveValue('recovery@example.com');
  await page.getByLabel('New password', { exact: true }).fill('NewPassword123!');
  await page.getByLabel('Confirm new password', { exact: true }).fill('OtherPassword123!');
  await page.getByRole('button', { name: 'Reset password', exact: true }).click();
  await expect(page.getByRole('alert')).toContainText('passwords do not match');
  expect(writes).toHaveLength(1);
  await page.getByLabel('Confirm new password', { exact: true }).fill('NewPassword123!');
  await page.getByRole('button', { name: 'Reset password', exact: true }).click();
  await expect(page.getByRole('status')).toContainText('Your password has been reset');
  await expect(page.getByRole('link', { name: 'Sign in', exact: true })).toHaveAttribute('href', '/login');
  expect(writes[1].body).toEqual({ email: 'recovery@example.com', token: 'reset+token', newPassword: 'NewPassword123!' });
});

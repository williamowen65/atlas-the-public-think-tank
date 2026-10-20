import { defineConfig } from '@playwright/test';

// Serve the built artifacts locally: CI needs neither deployment nor Atlas.Api.
export default defineConfig({
  testDir: './tests',
  timeout: 30_000,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 1 : 0,
  reporter: [['list'], ['html', { open: 'never' }]],
  use: {
    browserName: 'chromium',
    trace: 'retain-on-failure',
  },
  webServer: [
    {
      command: 'npx vite preview --host 127.0.0.1 --port 4173 --strictPort',
      url: 'http://127.0.0.1:4173',
      reuseExistingServer: false,
    },
    {
      command: 'npx vite preview --config storybook-preview.config.ts --outDir storybook-static --host 127.0.0.1 --port 6006 --strictPort',
      url: 'http://127.0.0.1:6006',
      reuseExistingServer: false,
    },
  ],
});

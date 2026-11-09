import { defineConfig } from '@playwright/test';

// Exercise the production frontend build locally; API responses are mocked by the spec.
export default defineConfig({
  testDir: './tests',
  testMatch: 'globe.spec.ts',
  use: {
    baseURL: 'http://localhost:4175',
    browserName: 'chromium',
    launchOptions: process.env.PLAYWRIGHT_CHROMIUM_EXECUTABLE
      ? { executablePath: process.env.PLAYWRIGHT_CHROMIUM_EXECUTABLE }
      : {},
  },
  webServer: {
    command: 'npx vite preview --host localhost --port 4175 --strictPort',
    url: 'http://localhost:4175',
    reuseExistingServer: !process.env.CI,
  },
});

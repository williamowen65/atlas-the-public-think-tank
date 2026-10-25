import { defineConfig } from '@playwright/test';

// Dedicated workbench UI checks. SQL ranking assertions run in DiscoveryRankingScenarioTests.
export default defineConfig({
  testDir: './tests',
  testMatch: ['ranking-lab.spec.ts', 'ranking-evidence.spec.ts'],
  timeout: 30_000,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 1 : 0,
  use: { browserName: 'chromium', trace: 'retain-on-failure' },
  webServer: {
    command: 'npx storybook dev --ci --no-open --disable-telemetry --host 127.0.0.1 --port 6007',
    url: 'http://127.0.0.1:6007',
    timeout: 120_000,
    reuseExistingServer: false,
  },
});

import type { StorybookConfig } from '@storybook/sveltekit';
import { mergeConfig } from 'vite';
import { publicApiGuard, publicApiProxy, sessionApiGuard, sessionApiProxy } from '../dev/localApi';

const config: StorybookConfig = {
  // Storybook also uses Vite: share the app's guards and forwarding in dev only.
  // A static Storybook build needs its own hosting routes to reach the API.
  viteFinal: async (config, { configType }) => configType === 'DEVELOPMENT'
    ? mergeConfig(config, {
        // Each guard passes unrelated paths along with next().
        plugins: [publicApiGuard, sessionApiGuard],
        server: {
          proxy: {
            '/atlas-public-api': publicApiProxy(),
            '/atlas-session-api': sessionApiProxy(),
          },
        },
      })
    : config,
  stories: ['../storybook/**/*.stories.ts'],
  framework: '@storybook/sveltekit',
  addons: ['@storybook/addon-docs', '@storybook/addon-a11y'],
  core: { disableTelemetry: true },
};

export default config;

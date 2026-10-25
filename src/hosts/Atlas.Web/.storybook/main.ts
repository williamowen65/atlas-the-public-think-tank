import type { StorybookConfig } from '@storybook/sveltekit';
import { mergeConfig } from 'vite';
import { fileURLToPath } from 'node:url';
import { rankingLabGuard, rankingLabProxy } from '../dev/rankingLab';
import { publicApiGuard, publicApiProxy, sessionApiGuard, sessionApiProxy } from '../dev/localApi';

const config: StorybookConfig = {
  // Storybook also uses Vite: share the app's guards and forwarding in dev only.
  // A static Storybook build needs its own hosting routes to reach the API.
  viteFinal: async (config, { configType }) => configType === 'DEVELOPMENT'
    ? mergeConfig(config, {
        // Each guard passes unrelated paths along with next().
        plugins: [publicApiGuard, sessionApiGuard, rankingLabGuard],
        server: {
          // The versioned fixture JSON is shared with backend tests outside Atlas.Web.
          fs: { allow: [fileURLToPath(new URL('../../../../', import.meta.url))] },
          proxy: {
            '/atlas-public-api': publicApiProxy(),
            '/atlas-ranking-lab': rankingLabProxy(),
            '/atlas-session-api': sessionApiProxy(),
          },
        },
      })
    : config,
  // Serve the generated MSW worker for the isolated breadcrumb example only.
  // This Storybook-only folder is separate from the application's static assets.
  staticDirs: ['./public'],
  stories: ['../storybook/**/*.stories.ts'],
  framework: '@storybook/sveltekit',
  addons: ['@storybook/addon-docs', '@storybook/addon-a11y'],
  core: { disableTelemetry: true },
};

export default config;

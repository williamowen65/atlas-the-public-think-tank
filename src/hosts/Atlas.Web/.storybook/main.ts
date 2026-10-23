import type { StorybookConfig } from '@storybook/sveltekit';
import { mergeConfig } from 'vite';
import { publicApiGuard, publicApiProxy, sessionApiGuard, sessionApiProxy } from '../dev/localApi';

const config: StorybookConfig = {
  viteFinal: async (config, { configType }) => configType === 'DEVELOPMENT'
    ? mergeConfig(config, { plugins: [publicApiGuard, sessionApiGuard], server: { proxy: { '/atlas-public-api': publicApiProxy(), '/atlas-session-api': sessionApiProxy() } } })
    : config,
  stories: ['../storybook/**/*.stories.ts'],
  framework: '@storybook/sveltekit',
  addons: ['@storybook/addon-docs', '@storybook/addon-a11y'],
  core: { disableTelemetry: true },
};

export default config;

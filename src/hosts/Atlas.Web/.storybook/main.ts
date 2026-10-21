import type { StorybookConfig } from '@storybook/sveltekit';
import { mergeConfig } from 'vite';
import { anonymousPreview, localApiProxy } from './localApi';

const config: StorybookConfig = {
  viteFinal: async (config, { configType }) => configType === 'DEVELOPMENT'
    ? mergeConfig(config, { plugins: [anonymousPreview], server: { proxy: { '/atlas-preview-api': localApiProxy() } } })
    : config,
  stories: ['../storybook/**/*.stories.ts'],
  framework: '@storybook/sveltekit',
  addons: ['@storybook/addon-docs', '@storybook/addon-a11y'],
  core: { disableTelemetry: true },
};

export default config;

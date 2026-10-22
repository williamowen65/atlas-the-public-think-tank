import type { StorybookConfig } from '@storybook/sveltekit';
import { mergeConfig } from 'vite';
import { anonymousPreview, localApiProxy, commentsPreview, commentsApiProxy } from './localApi';

const config: StorybookConfig = {
  viteFinal: async (config, { configType }) => configType === 'DEVELOPMENT'
    ? mergeConfig(config, { plugins: [anonymousPreview, commentsPreview], server: { proxy: { '/atlas-preview-api': localApiProxy(), '/atlas-comments-api': commentsApiProxy() } } })
    : config,
  stories: ['../storybook/**/*.stories.ts'],
  framework: '@storybook/sveltekit',
  addons: ['@storybook/addon-docs', '@storybook/addon-a11y'],
  core: { disableTelemetry: true },
};

export default config;

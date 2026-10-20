import type { StorybookConfig } from '@storybook/html-vite';

// HTML is an interim renderer so the workbench does not select a SPA framework.
const config: StorybookConfig = {
  stories: ['../src/**/*.stories.ts'],
  framework: '@storybook/html-vite',
  addons: ['@storybook/addon-docs', '@storybook/addon-a11y'],
  core: { disableTelemetry: true },
};

export default config;

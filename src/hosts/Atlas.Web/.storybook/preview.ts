import type { Preview } from '@storybook/sveltekit';
import '../src/styles.css';

const preview: Preview = {
  globalTypes: {
    theme: {
      description: 'Shared color tokens',
      toolbar: { title: 'Theme', icon: 'paintbrush', items: ['light', 'dark'], dynamicTitle: true },
    },
    density: {
      description: 'Shared spacing tokens',
      toolbar: { title: 'Density', icon: 'circlehollow', items: ['comfortable', 'compact'], dynamicTitle: true },
    },
  },
  initialGlobals: { theme: 'light', density: 'comfortable' },
  decorators: [
    (story, context) => {
      // Configure global CSS tokens; the actual component markup stays in Svelte.
      document.documentElement.dataset.theme = context.globals.theme === 'dark' ? 'dark' : 'light';
      document.documentElement.dataset.density = context.globals.density === 'compact' ? 'compact' : 'comfortable';
      return story();
    },
  ],
  parameters: {
    a11y: { test: 'error' },
    options: { storySort: { order: ['Styles and Layout', 'Components', 'Compositions'] } },
  },
};

export default preview;

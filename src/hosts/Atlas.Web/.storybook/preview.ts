import type { Preview } from '@storybook/sveltekit';
import '../src/styles.scss';
import { applyDesign } from '../storybook/StylesAndLayout/designSettings';

const preview: Preview = {
  globalTypes: {
    design: { description: 'Shared style experiments from Styles and Layout' },
    theme: {
      description: 'Shared color tokens',
      toolbar: { title: 'Theme', icon: 'paintbrush', items: ['light', 'dark'], dynamicTitle: true },
    },
    density: {
      description: 'Shared spacing tokens',
      toolbar: { title: 'Density', icon: 'circlehollow', items: ['comfortable', 'compact'], dynamicTitle: true },
    },
  },
  initialGlobals: { theme: 'light', density: 'comfortable', design: {} },
  decorators: [
    (story, context) => {
      // Configure global CSS tokens; the actual component markup stays in Svelte.
      document.documentElement.dataset.theme = context.globals.theme === 'dark' ? 'dark' : 'light';
      document.documentElement.dataset.density = context.globals.density === 'compact' ? 'compact' : 'comfortable';
      applyDesign(context.globals.design ?? {});
      return story();
    },
  ],
  parameters: {
    a11y: { test: 'error' },
    options: { storySort: { order: ['Styles and Layout', 'Layout', 'Components', 'Compositions'] } },
  },
};

export default preview;

import type { Preview } from '@storybook/sveltekit';
import '../src/styles.css';

const preview: Preview = {
  parameters: { a11y: { test: 'error' } },
};

export default preview;

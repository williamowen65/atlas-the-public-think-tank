import type { Meta, StoryObj } from '@storybook/sveltekit';
import StylePlayground from './StylePlayground.svelte';

const meta = {
  title: 'Styles and Layout/Playground',
  component: StylePlayground,
  args: { columns: 2, maxWidth: 900 },
  argTypes: {
    columns: { control: { type: 'range', min: 1, max: 3, step: 1 } },
    maxWidth: { control: { type: 'range', min: 280, max: 1200, step: 20 } },
  },
} satisfies Meta<typeof StylePlayground>;
export default meta;
type Story = StoryObj<typeof meta>;
export const Layout: Story = {};
export const Narrow: Story = { args: { maxWidth: 320, columns: 1 } };

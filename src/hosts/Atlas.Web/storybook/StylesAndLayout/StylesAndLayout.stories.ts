import type { Meta, StoryObj } from '@storybook/sveltekit';
import { useArgs } from 'storybook/preview-api';
import StylePlayground from './StylePlayground.svelte';

const defaults = { fontSize: 16, lineHeight: 1.6, headingScale: 1.5, paragraphGap: 16, headingGap: 12 };
const meta = {
  title: 'Styles and Layout/Typography',
  component: StylePlayground,
  args: { ...defaults, onReset: () => {} },
  argTypes: {
    fontSize: { control: { type: 'range', min: 12, max: 24, step: 1 } },
    lineHeight: { control: { type: 'range', min: 1.2, max: 2, step: 0.1 } },
    headingScale: { control: { type: 'range', min: 1.1, max: 2, step: 0.1 } },
    paragraphGap: { control: { type: 'range', min: 0, max: 40, step: 2 } },
    headingGap: { control: { type: 'range', min: 0, max: 32, step: 2 } },
    onReset: { control: false, table: { disable: true } },
  },
  render: (args) => {
    const [, updateArgs] = useArgs();
    return { Component: StylePlayground, props: { ...args, onReset: () => updateArgs(defaults) } };
  },
} satisfies Meta<typeof StylePlayground>;
export default meta;
type Story = StoryObj<typeof meta>;
export const Playground: Story = {};

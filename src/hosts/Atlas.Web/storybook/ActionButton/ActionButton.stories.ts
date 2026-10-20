import type { Meta, StoryObj } from '@storybook/sveltekit';
import ActionButtonExample from './ActionButtonExample.svelte';

// One story group demonstrates the reusable button with observable callback results.
const meta = {
  title: 'Components/ActionButton',
  component: ActionButtonExample,
  argTypes: {
    label: { control: 'text' },
    disabled: { control: 'boolean' },
  },
  args: { label: 'Try action', disabled: false },
} satisfies Meta<typeof ActionButtonExample>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Playground: Story = {};

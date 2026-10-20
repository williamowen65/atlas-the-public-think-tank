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

/** Enabled activation should update the demonstration's status message. */
export const Enabled: Story = {};

/** A disabled button must leave the demonstration's status unchanged. */
export const Disabled: Story = { args: { disabled: true } };

/** Long labels wrap without overflowing a narrow viewport. */
export const LongLabel: Story = {
  args: { label: 'Explore another way to understand this issue together' },
};

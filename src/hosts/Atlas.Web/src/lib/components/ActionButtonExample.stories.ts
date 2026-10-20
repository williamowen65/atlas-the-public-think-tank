import type { Meta, StoryObj } from '@storybook/sveltekit';
import ActionButtonExample from './ActionButtonExample.svelte';

// A separate demo shows callback outcomes without mixing fixture HTML into Meta.
const meta = {
  title: 'Examples/ActionButton',
  component: ActionButtonExample,
  args: { label: 'Try action', disabled: false },
} satisfies Meta<typeof ActionButtonExample>;

export default meta;
type Story = StoryObj<typeof meta>;

/** Enabled activation should update the demonstration's status message. */
export const Enabled: Story = {};

/** A disabled button must leave the demonstration's status unchanged. */
export const Disabled: Story = { args: { disabled: true } };

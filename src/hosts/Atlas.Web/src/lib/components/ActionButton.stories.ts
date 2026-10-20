import type { Meta, StoryObj } from '@storybook/sveltekit';
import ActionButton from './ActionButton.svelte';

// Direct component stories: Storybook renders the production Svelte markup.
const meta = {
  title: 'Foundation/ActionButton',
  component: ActionButton,
  tags: ['autodocs'],
  argTypes: {
    label: { control: 'text' },
    disabled: { control: 'boolean' },
    onActivate: { control: false },
  },
  args: { label: 'Try action', disabled: false },
} satisfies Meta<typeof ActionButton>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Enabled: Story = {};
export const Disabled: Story = { args: { disabled: true } };
export const LongLabel: Story = {
  args: { label: 'Explore another way to understand this issue together' },
};

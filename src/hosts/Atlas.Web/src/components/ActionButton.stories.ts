import type { Meta, StoryObj } from '@storybook/html-vite';
import { createActionButton, type ActionButtonProps } from './ActionButton';

const meta = {
  title: 'Foundation/ActionButton',
  tags: ['autodocs'],
  argTypes: {
    label: { control: 'text' },
    disabled: { control: 'boolean' },
    onActivate: { control: false },
  },
  args: { label: 'Try action', disabled: false },
  render: (args) => {
    const example = document.createElement('section');
    const result = document.createElement('p');
    result.setAttribute('role', 'status');
    result.textContent = 'No action yet';

    // This fixture supplies a visible callback result without an API or account.
    example.append(createActionButton({
      ...args,
      onActivate: () => { result.textContent = 'Action completed'; },
    }), result);

    return example;
  },
} satisfies Meta<ActionButtonProps>;

export default meta;
type Story = StoryObj<ActionButtonProps>;

/** An enabled action should report completion after mouse or keyboard activation. */
export const Enabled: Story = {};

/** A disabled action must remain unavailable and leave the result unchanged. */
export const Disabled: Story = { args: { disabled: true } };

export const LongLabel: Story = {
  args: { label: 'Explore another way to understand this issue together' },
};

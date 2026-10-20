import type { Meta, StoryObj } from '@storybook/sveltekit';
import VotingDialExample from './VotingDialExample.svelte';

const meta = {
  title: 'Components/VotingDial',
  component: VotingDialExample,
  args: { initialValue: 5, average: 7.4, totalVotes: 21, compact: false, disabled: false, label: 'Node importance' },
  argTypes: {
    initialValue: { control: { type: 'number', min: 0, max: 10, step: 1 } },
    average: { control: { type: 'number', min: 0, max: 10 } },
    totalVotes: { control: { type: 'number', min: 0 } },
  },
} satisfies Meta<typeof VotingDialExample>;
export default meta;
type Story = StoryObj<typeof meta>;

export const Standard: Story = {};
export const Compact: Story = { args: { compact: true } };
export const Unrated: Story = { args: { initialValue: null, average: null, totalVotes: 0 } };
export const Zero: Story = { args: { initialValue: 0 } };
export const Disabled: Story = { args: { disabled: true } };
export const CommentExploration: Story = { args: { compact: true, label: 'Comment rating preview' } };

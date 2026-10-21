import type { Meta, StoryObj } from '@storybook/sveltekit';
import VotingDialExample from './VotingDialExample.svelte';

const meta = {
  title: 'Components/VotingDial',
  component: VotingDialExample,
  args: { voteOutcome: 'accepted', initialValue: 5, average: 7.4, totalVotes: 21, compact: false, disabled: false, label: 'Node importance' },
  argTypes: {
    voteOutcome: { type: 'string', control: 'select', options: ['accepted', 'anonymous', 'failed'], description: 'Simulated cast outcome. Uncast votes return to neutral; no API request is made.' },
    initialValue: { control: { type: 'number', min: 0, max: 10, step: 1 } },
    average: { control: { type: 'number', min: 0, max: 10 } },
    totalVotes: { control: { type: 'number', min: 0 } },
  },
} satisfies Meta<typeof VotingDialExample>;
export default meta;
type Story = StoryObj<typeof meta>;

export const Playground: Story = {};

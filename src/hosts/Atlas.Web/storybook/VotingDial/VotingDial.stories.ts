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

export const Playground: Story = {};

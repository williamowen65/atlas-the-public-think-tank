import type { Meta, StoryObj } from '@storybook/sveltekit';
import VotingComparison from './VotingComparison.svelte';

const meta = {
  title: 'Compositions/Voting comparison',
  component: VotingComparison,
} satisfies Meta<typeof VotingComparison>;
export default meta;
type Story = StoryObj<typeof meta>;
export const IndependentSelections: Story = {};

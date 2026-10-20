import type { Meta, StoryObj } from '@storybook/sveltekit';
import VotingCardExample from './VotingCardExample.svelte';

const meta = {
  title: 'Compositions/Node voting preview',
  component: VotingCardExample,
  args: { compact: false },
} satisfies Meta<typeof VotingCardExample>;
export default meta;
type Story = StoryObj<typeof meta>;
export const Standard: Story = {};
export const Compact: Story = { args: { compact: true } };

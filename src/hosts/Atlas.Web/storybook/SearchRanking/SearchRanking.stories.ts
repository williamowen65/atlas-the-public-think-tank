import type { Meta, StoryObj } from '@storybook/sveltekit';
import SearchRanking from './SearchRanking.svelte';
const meta = {
  title: 'Compositions/Search ranking',
  component: SearchRanking,
  parameters: { layout: 'fullscreen' },
} satisfies Meta<typeof SearchRanking>;
export default meta;
type Story = StoryObj<typeof meta>;
export const Workbench: Story = {};

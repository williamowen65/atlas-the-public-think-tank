import type { Meta, StoryObj } from '@storybook/sveltekit';
import LiveNodeFeed from '../../src/lib/components/LiveNodeFeed.svelte';
const meta = {
  title: 'Compositions/Feed', component: LiveNodeFeed,
  args: { presentation: 'ribbon-corner' },
  argTypes: { presentation: { control: 'select', options: ['context-below', 'ribbon', 'ribbon-responsive', 'ribbon-split', 'ribbon-corner'] } },
} satisfies Meta<typeof LiveNodeFeed>;
export default meta;
type Story = StoryObj<typeof meta>;
export const Playground: Story = {};

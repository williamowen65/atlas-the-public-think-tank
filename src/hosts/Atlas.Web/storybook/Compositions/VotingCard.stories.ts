import type { Meta, StoryObj } from '@storybook/sveltekit';
import LiveNodePreview from '../NodeCard/LiveNodePreview.svelte';

const meta = {
  title: 'Compositions/Node voting preview',
  component: LiveNodePreview,
  args: { presentation: 'ribbon' },
  argTypes: {
    presentation: { control: 'select', options: ['ribbon', 'current'], description: 'Compare both card presentations with live Atlas data.' },
  },
} satisfies Meta<typeof LiveNodePreview>;
export default meta;
type Story = StoryObj<typeof meta>;
export const Playground: Story = {};

import type { Meta, StoryObj } from '@storybook/sveltekit';
import LiveNodePreview from '../NodeCard/LiveNodePreview.svelte';

const meta = {
  title: 'Compositions/Atlas node',
  component: LiveNodePreview,
  args: { presentation: 'ribbon-corner' },
  argTypes: {
    presentation: { type: 'string', control: 'select', options: ['ribbon', 'context-below', 'ribbon-responsive', 'ribbon-split', 'ribbon-corner'], description: 'Compare standalone card versions using the same live Atlas data.' },
  },
} satisfies Meta<typeof LiveNodePreview>;
export default meta;
type Story = StoryObj<typeof meta>;
export const Playground: Story = {};

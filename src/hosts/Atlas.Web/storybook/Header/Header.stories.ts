import type { Meta, StoryObj } from '@storybook/sveltekit';
import AtlasHeader from '../../src/lib/components/AtlasHeader.svelte';

const meta = {
  title: 'Layout/Header',
  component: AtlasHeader,
  args: { active: 'discover', authenticated: false, displayName: 'Atlas User' },
  argTypes: {
    active: { control: 'select', options: ['discover', 'about', 'components', 'create'] },
    authenticated: { control: 'boolean' },
    displayName: { control: 'text' }
  }
} satisfies Meta<typeof AtlasHeader>;
export default meta;
type Story = StoryObj<typeof meta>;
export const Playground: Story = {};

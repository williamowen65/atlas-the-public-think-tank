import type { Meta, StoryObj } from '@storybook/sveltekit';
import AtlasHeader from '../../src/lib/components/AtlasHeader.svelte';
const meta = { title: 'Layout/Header', component: AtlasHeader, args: { active: 'discover' }, argTypes: { active: { control: 'select', options: ['discover', 'about', 'components'] } } } satisfies Meta<typeof AtlasHeader>;
export default meta;
type Story = StoryObj<typeof meta>;
export const Playground: Story = {};

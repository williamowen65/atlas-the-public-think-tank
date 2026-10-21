import type { Meta, StoryObj } from '@storybook/sveltekit';
import NodePageExample from './NodePageExample.svelte';
const meta = { title: 'Compositions/Node page', component: NodePageExample, args: { nodeId: '' }, argTypes: { nodeId: { control: 'text', description: 'A live node ID. Empty selects the first public Discovery result.' } } } satisfies Meta<typeof NodePageExample>;
export default meta;
type Story = StoryObj<typeof meta>;
export const Playground: Story = {};

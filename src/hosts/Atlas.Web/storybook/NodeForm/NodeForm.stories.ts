import type { Meta, StoryObj } from '@storybook/sveltekit';
import LiveNodeForm from './NodeFormExample.svelte';
const meta = { title: 'Components/Node form', component: LiveNodeForm } satisfies Meta<typeof LiveNodeForm>;
export default meta;
type Story = StoryObj<typeof meta>;
export const Playground: Story = {};

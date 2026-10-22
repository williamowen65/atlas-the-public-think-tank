import type { Meta, StoryObj } from '@storybook/sveltekit';
import SessionControl from '../../src/lib/components/SessionControl.svelte';
const meta = { title: 'Components/Sign in', component: SessionControl, args: { expanded: true } } satisfies Meta<typeof SessionControl>;
export default meta;
type Story = StoryObj<typeof meta>;
export const Playground: Story = {};

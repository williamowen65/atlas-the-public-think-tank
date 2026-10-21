import type { Meta, StoryObj } from '@storybook/sveltekit';
import AtlasFooter from '../../src/lib/components/AtlasFooter.svelte';
const meta = { title: 'Components/Footer', component: AtlasFooter, args: { tagline: 'A place to connect questions, evidence, and ideas.' } } satisfies Meta<typeof AtlasFooter>;
export default meta;
type Story = StoryObj<typeof meta>;
export const Playground: Story = {};

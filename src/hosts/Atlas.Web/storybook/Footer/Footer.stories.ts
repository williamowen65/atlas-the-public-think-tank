import type { Meta, StoryObj } from '@storybook/sveltekit';
import AtlasFooter from './FooterExample.svelte';
const meta = { title: 'Layout/Footer', component: AtlasFooter, args: { tagline: 'A place to connect questions, evidence, and ideas.' } } satisfies Meta<typeof AtlasFooter>;
export default meta;
type Story = StoryObj<typeof meta>;
export const Playground: Story = {};

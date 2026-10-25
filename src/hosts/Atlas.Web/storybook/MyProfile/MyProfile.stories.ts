import type { Meta, StoryObj } from '@storybook/sveltekit';
import MyProfileExample from './MyProfileExample.svelte';
const meta = { title: 'Compositions/My profile', component: MyProfileExample } satisfies Meta<typeof MyProfileExample>;
export default meta;
type Story = StoryObj<typeof meta>;
export const Playground: Story = {};

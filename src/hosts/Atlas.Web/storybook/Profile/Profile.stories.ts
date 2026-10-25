import type { Meta, StoryObj } from '@storybook/sveltekit';
import ProfileExample from './ProfileExample.svelte';
const meta = {
  title: 'Compositions/Profile page',
  component: ProfileExample,
  args: { participantId: '' },
  argTypes: { participantId: { control: 'text', description: 'Public participant ID. Empty selects the first participant.' } }
} satisfies Meta<typeof ProfileExample>;
export default meta;
type Story = StoryObj<typeof meta>;
export const Playground: Story = {};

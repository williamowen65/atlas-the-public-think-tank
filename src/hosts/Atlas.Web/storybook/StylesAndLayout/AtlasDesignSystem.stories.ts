import type { Meta, StoryObj } from '@storybook/sveltekit';
import AtlasDesignSystem from './AtlasDesignSystem.svelte';

// A visual reference, not another controls playground. Theme and Density remain Storybook globals.
const meta = {
  title: 'Styles and Layout/Atlas Design System',
  component: AtlasDesignSystem,
  parameters: { controls: { disable: true } },
} satisfies Meta<typeof AtlasDesignSystem>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Overview: Story = {};

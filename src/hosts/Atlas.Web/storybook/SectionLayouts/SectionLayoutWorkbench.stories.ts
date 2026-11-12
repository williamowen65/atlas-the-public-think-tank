import type { Meta, StoryObj } from '@storybook/sveltekit';
import SectionLayoutWorkbench from './SectionLayoutWorkbench.svelte';

const meta = {
  title: 'Content/Section layout workbench',
  component: SectionLayoutWorkbench,
  parameters: { layout: 'fullscreen' },
} satisfies Meta<typeof SectionLayoutWorkbench>;

export default meta;
type Story = StoryObj<typeof meta>;

export const BlankNode: Story = { args: { start: 'blank' } };
export const Editorial: Story = { args: { start: 'editorial' } };
export const PhotoEssay: Story = { args: { start: 'photoEssay' } };

import type { Meta, StoryObj } from '@storybook/sveltekit';
import BreadcrumbExample from './BreadcrumbExample.svelte';
import MultipleParentsExample from './MultipleParentsExample.svelte';

const meta = {
  title: 'Components/Breadcrumb',
  component: BreadcrumbExample,
  args: { depth: 8 },
  argTypes: { depth: { control: { type: 'range', min: 0, max: 8, step: 1 }, description: 'Oldest to newest. Changing the path reveals the newest ancestor again.' } },
} satisfies Meta<typeof BreadcrumbExample>;
export default meta;
type Story = StoryObj<typeof meta>;
export const Playground: Story = {};

/** Demonstrates the alternate-parent picker without requiring a live API. */
export const MultipleParents = { render: () => ({ Component: MultipleParentsExample }) };

import type { Meta, StoryObj } from '@storybook/sveltekit';
import { useArgs } from 'storybook/preview-api';
import VotingCardExample from './VotingCardExample.svelte';
import { fixtureOptions } from '../NodeCard/nodeFixtures';

const meta = {
  title: 'Compositions/Node voting preview',
  component: VotingCardExample,
  args: { source: 'fixtures', selectedNode: 'question', onSelect: () => {} },
  argTypes: {
    source: { type: 'string', control: 'select', options: ['fixtures', 'local-api'], description: 'Local API loads real public nodes; all voting stays local.' },
    selectedNode: { control: 'select', options: fixtureOptions, description: 'Select a fixture node; connected-node clicks update this Control too.' },
    onSelect: { control: false, table: { disable: true } },
  },
  decorators: [(story, context) => {
    const [, updateArgs] = useArgs();
    return story({ args: { ...context.args, onSelect: (selectedNode: string) => updateArgs({ selectedNode }) } });
  }],
} satisfies Meta<typeof VotingCardExample>;
export default meta;
type Story = StoryObj<typeof meta>;
export const Playground: Story = {};

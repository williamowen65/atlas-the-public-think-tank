import type { Meta, StoryObj } from '@storybook/sveltekit';
import NodeForm from '../../src/lib/components/NodeForm.svelte';

const typeId = 'idea';
const meta = {
  title: 'Content/Node authoring',
  component: NodeForm,
  args: {
    types: [{ id: typeId, name: 'Idea', pluralName: 'Ideas' }],
    onSubmit: () => {},
  },
} satisfies Meta<typeof NodeForm>;
export default meta;
type Story = StoryObj<typeof meta>;

const draft = {
  title: 'A greener public square',
  shortDescription: '',
  markdown: '',
  typeId,
  requestedTypeIds: [],
  blocks: [
    {
      key: 'introduction', kind: 'markdown', url: '', caption: '',
      text: 'What if the public square made more room to **walk** and meet?\n\n![A city street lined with tall buildings](https://images.unsplash.com/photo-1519501025264-65ba15a82390?w=600)',
    },
    {
      key: 'detail', kind: 'markdown', url: '', caption: '',
      text: 'Start with a small pilot and invite neighbors to help choose where to add trees and seating.',
    },
  ],
};

export const PublishedCard: Story = { args: { initialDraft: draft } };

export const ChildNode: Story = {
  args: {
    initialDraft: draft,
    parentId: 'public-spaces',
    parentTitle: 'How could we improve our public spaces?',
  },
};

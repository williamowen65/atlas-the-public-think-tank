import type { Meta, StoryObj } from '@storybook/sveltekit';
import NodeForm from '../../src/lib/components/NodeForm.svelte';

const typeId = 'idea';
const meta = {
  title: 'Content/Node authoring',
  component: NodeForm,
  args: {
    types: [
      { id: typeId, name: 'Idea', pluralName: 'Ideas' },
      { id: 'question', name: 'Question', pluralName: 'Questions' },
      { id: 'evidence', name: 'Evidence', pluralName: 'Evidence' },
      { id: 'counterargument', name: 'Counterargument', pluralName: 'Counterarguments' },
    ],
    communities: [
      { id: 'public-spaces', name: 'Public spaces' },
      { id: 'walkable-streets', name: 'Walkable streets' },
      { id: 'urban-trees', name: 'Urban trees' },
    ],
    previewReactions: [{ label: 'Useful', count: 3 }, { label: 'Inspiring', count: 2 }],
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
  requestedTypeIds: ['question', 'evidence'],
  blocks: [
    {
      key: 'introduction', kind: 'markdown', url: '', caption: '',
      text: '![A city street lined with tall buildings](https://images.unsplash.com/photo-1519501025264-65ba15a82390?w=600) What if the public square made more room to **walk** and meet? Today most people pass straight through. A few trees, seats in the shade, and a wider path could give neighbors a reason to linger, make it easier to cross the square, and leave space for a weekend market. We could start with movable benches and planters and ask people where they naturally stop.',
    },
    {
      key: 'detail', kind: 'markdown', url: '', caption: '',
      text: 'Start with a small pilot and invite neighbors to help choose where to add trees and seating.',
    },
    {
      key: 'street-photo', kind: 'image', text: 'A street lined with trees',
      url: 'https://images.unsplash.com/photo-1518005020951-eccb494ad742?w=600',
      caption: 'A reference for the pilot project',
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

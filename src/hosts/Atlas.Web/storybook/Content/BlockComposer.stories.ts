import type { Meta, StoryObj } from '@storybook/sveltekit';
import BlockComposerDemo from './BlockComposerDemo.svelte';

const text = (key: string, value: string) => ({
  key, kind: 'markdown', text: value, url: '', caption: '',
});
const image = (key: string) => ({
  key, kind: 'image', text: 'Green public square with trees and people walking',
  url: 'https://images.unsplash.com/photo-1519501025264-65ba15a82390?w=800',
  caption: 'A pedestrian-friendly city space',
});
const meta = {
  title: 'Content/Block composer',
  component: BlockComposerDemo,
  args: { title: 'A better public square', initialBlocks: [text('intro', 'Public spaces can bring people together.')] },
} satisfies Meta<typeof BlockComposerDemo>;
export default meta;
type Story = StoryObj<typeof meta>;

export const TextFirst: Story = {};
export const ImageFirst: Story = { args: { initialBlocks: [image('cover'), text('body', 'Could our neighborhood use more shared space?')] } };
export const TextAndImage: Story = { args: { initialBlocks: [text('intro', 'Walkable places make communities stronger. What would you change?'), image('photo'), text('details', 'Here are a few ideas to consider.')] } };
export const LongText: Story = { args: { initialBlocks: [text('long', 'The compact feed should not grow indefinitely. '.repeat(50)), image('photo')] } };
export const UnimplementedBlock: Story = { args: { initialBlocks: [text('intro', 'The next block is preserved but not editable yet.'), {
  key: 'poll', id: '00000000-0000-4000-8000-000000000001', kind: 'poll-reference', text: '', url: '', caption: '',
  original: { id: '00000000-0000-4000-8000-000000000001', kind: 'poll-reference', content: { pollId: 'some-reference' } },
}] } };

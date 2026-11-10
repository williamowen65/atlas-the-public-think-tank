import type { Meta, StoryObj } from '@storybook/sveltekit';
import EditorWorkbenchDemo from './EditorWorkbenchDemo.svelte';
import type { AuthorBlock } from '../../src/lib/api/contentAuthoring';

const text = (key: string, value: string): AuthorBlock => ({
  key, kind: 'markdown', text: value, url: '', caption: '',
});
const image = (key: string): AuthorBlock => ({
  key, kind: 'image', text: 'A pedestrian-friendly public square with trees',
  url: 'https://images.unsplash.com/photo-1519501025264-65ba15a82390?w=1000',
  caption: 'A greener public gathering space',
});
const textFirst = [
  text('intro', 'What if a part of our city center were transformed into a more inviting public space? We could create room to gather, **walk**, and enjoy more trees.'),
  image('public-square'),
  text('details', 'A small pilot could help neighbors test this idea and share what they learn.'),
];
const imageFirst = [
  image('cover'),
  text('opening', 'Could an unused parking lot become a welcoming place for people to meet?'),
  text('after', 'Share your thoughts on what should happen here.'),
];

const meta = {
  title: 'Content/Editor workbench (experimental)',
  component: EditorWorkbenchDemo,
  args: {
    title: 'A better public square',
    initialBlocks: textFirst,
    initialMode: 'tiptap',
    initialStyle: 'atlas',
    initialNarrow: false,
  },
  parameters: {
    layout: 'fullscreen',
    docs: {
      description: {
        component: 'Research-only comparison of the existing block forms with an actual Tiptap editing canvas. Switch editors, preview the compact feed and full node, and compare Atlas and editorial styles. No API calls, saving, image upload, or production migrations. The Media Gallery Concept story additionally prototypes a mixed photo/video gallery with responsive swiping and parity between edit/full views; its slides are not yet Atlas blocks. Both editors use one shared ordered Atlas block array; test text, images and identity by switching modes.',
      },
    },
  },
} satisfies Meta<typeof EditorWorkbenchDemo>;
export default meta;
type Story = StoryObj<typeof meta>;

// Keep the workbench focused: editor and typography styles already switch in-place.
// The separate Block composer stories remain available for isolated form testing.
export const CompareEditors: Story = {};
export const ImageFirst: Story = { args: { initialBlocks: imageFirst } };
export const PhoneWidth: Story = { args: { initialNarrow: true } };
export const BlankNode: Story = { args: { title: '', initialBreadcrumb: false, initialBlocks: [text('empty', '')] } };

export const MediaGalleryConcept: Story = {
  args: {
    title: 'A richer view of our city',
    galleryExperiment: true,
    initialBlocks: [
      text('opening', 'A single image can still be its own block, but what if a **gallery** could combine portrait and landscape photos with playable video?'),
      image('standalone-photo'),
      text('after-media', 'Try the gallery below, change its frame, then switch between Edit, Full node and Feed. These slides are experimental data, not persisted Atlas blocks.'),
    ],
  },
};

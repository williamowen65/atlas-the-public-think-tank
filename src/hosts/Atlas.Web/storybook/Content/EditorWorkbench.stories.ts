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
        component: 'Research-only comparison of the existing block forms with an actual Tiptap editing canvas. Switch editors, preview the compact feed and full node, and compare Atlas and editorial styles. No API calls, saving, image upload, or production migrations. The Feed Block Carousel Concept presents one slide for each existing Atlas node content block, only in the Feed view. Edit and Full Node show the same blocks normally, and Read more opens the continuous node preview. This is a Storybook-only view experiment requiring no gallery block or persistence changes. Both editors use one shared ordered Atlas block array; test text, images and identity by switching modes.',
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

export const FeedBlockCarouselConcept: Story = {
  args: {
    title: 'A richer view of our city',
    initialBlocks: [
      text('opening', 'What if a public square felt more welcoming? We could create a place to meet, **walk**, and enjoy more trees.'),
      image('first-photo'),
      text('long-section', [
        'The same neighborhood can tell a much bigger story when its details are included.',
        'A single Markdown section may hold several paragraphs. The feed should only show a bounded preview of it.',
        'The complete block remains available when the reader opens the full node.',
        'We could test benches, safer crosswalks, more shade, and community events.',
        'Residents could share what worked before the next set of improvements.',
        'This final paragraph is intentionally farther down: it should not stretch the feed card.',
      ].join('\n\n')),
      {
        ...image('second-photo'),
        url: 'https://images.unsplash.com/photo-1518005020951-eccb494ad742?w=1200',
        text: 'Urban architectural structures and open space',
        caption: 'Another angle on a shared public place',
      },
    ],
  },
};

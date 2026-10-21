import type { Meta, StoryObj } from '@storybook/sveltekit';
import { useArgs, useGlobals, useEffect } from 'storybook/preview-api';
import { designDefaults, type DesignSettings } from './designSettings';
import StylePlayground from './StylePlayground.svelte';

const defaults = designDefaults;
const meta = {
  title: 'Styles and Layout/Typography',
  component: StylePlayground,
  args: { ...defaults, onReset: () => {} },
  argTypes: {
    fontFamily: { control: 'select', options: ['system-ui, sans-serif', 'Georgia, serif', 'Arial, sans-serif', 'Verdana, sans-serif', 'ui-monospace, monospace'] },
    space: { description: '0 follows the Density toolbar; other values override shared spacing in pixels.', control: { type: 'range', min: 0, max: 32, step: 2 } },
    cardPadding: { control: { type: 'range', min: 8, max: 40, step: 2 } },
    cardPaddingNarrow: { control: { type: 'range', min: 4, max: 24, step: 2 } },
    dialTextGap: { control: { type: 'range', min: 4, max: 32, step: 2 } },
    radius: { control: { type: 'range', min: 0, max: 24, step: 2 } },
    background: { control: 'color' }, surface: { control: 'color' }, text: { control: 'color' }, muted: { control: 'color' },
    accent: { control: 'color' }, onAccent: { control: 'color' }, border: { control: 'color' }, focus: { control: 'color' },
    fontSize: { control: { type: 'range', min: 12, max: 24, step: 1 } },
    lineHeight: { control: { type: 'range', min: 1.2, max: 2, step: 0.1 } },
    headingScale: { control: { type: 'range', min: 1.1, max: 2, step: 0.1 } },
    paragraphGap: { control: { type: 'range', min: 0, max: 40, step: 2 } },
    headingGap: { control: { type: 'range', min: 0, max: 32, step: 2 } },
    onReset: { control: false, table: { disable: true } },
  },
  decorators: [
    (story, context) => {
      // Storybook owns this hook context in both development and static builds.
      const [, updateArgs] = useArgs();
      const [, updateGlobals] = useGlobals();
      // Only style args trigger synchronization; globals changes do not feed back into args.
      const settings = Object.fromEntries((Object.keys(defaults) as (keyof DesignSettings)[]).map(key => [key, context.args[key]]));
      useEffect(() => { updateGlobals({ design: settings }); }, Object.values(settings));
      return story({
        args: { ...context.args, onReset: () => updateArgs(defaults) },
      });
    },
  ],
} satisfies Meta<typeof StylePlayground>;
export default meta;
type Story = StoryObj<typeof meta>;
export const Playground: Story = {};

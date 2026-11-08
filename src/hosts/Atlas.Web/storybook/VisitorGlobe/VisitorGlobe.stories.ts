import type { Meta, StoryObj } from '@storybook/sveltekit';
import VisitorGlobeExample from './VisitorGlobeExample.svelte';

const meta = {
  title: 'Components/VisitorGlobe',
  component: VisitorGlobeExample,
  parameters: {
    layout: 'centered',
    docs: {
      description: {
        component:
          'Isolated globe appearance playground. Adjust pinCount and distribution for worldwide or clustered fictional demo pins; select custom to edit the pins array. All pins are fixtures; no API, geolocation permission, or database is used. Set oceanColor and oceanEdgeColor to the same color for a flat ocean. Rotation and angle controls are for Storybook exploration only.',
      },
    },
  },
  args: {
    targetFps: 60,
    oceanColor: '#e3eff3',
    oceanEdgeColor: '#bdd6df',
    landColor: '#668f87',
    gridColor: '#385e6b',
    pinColor: '#485453',
    pinHeadColor: '#b73539',
    variedHeads: true,
    pinStyle: 'round',
    pinLength: 23,
    pinHeadRadius: 3.5,
    pinLean: 0.2,
    outlineColor: '#547d89',
    showGrid: true,
    animate: true,
    speed: 3,
    angle: -25,
    size: 420,
    mini: false,
    pinCount: 100,
    distribution: 'worldwide',
    pins: [
      { longitude: 0, latitude: 30, visitors: 1 },
      { longitude: 40, latitude: 5, visitors: 3 },
      { longitude: -122, latitude: 48, visitors: 1 },
    ],
  },
  argTypes: {
    targetFps: {
      control: 'select',
      options: [10, 15, 24, 30, 60],
      description:
        'Animation update ceiling; observed updates/s is not a GPU paint-time measurement.',
    },
    oceanColor: { control: 'color' },
    oceanEdgeColor: { control: 'color' },
    landColor: { control: 'color' },
    gridColor: { control: 'color' },
    pinColor: { control: 'color', description: 'Shaft color (no outline).' },
    pinHeadColor: { control: 'color' },
    variedHeads: { control: 'boolean' },
    pinStyle: {
      control: 'select',
      options: ['round', 'flat', 'stem'],
      type: { name: 'enum', value: ['round', 'flat', 'stem'] },
    },
    pinLength: { control: { type: 'range', min: 8, max: 32, step: 1 } },
    pinHeadRadius: { control: { type: 'range', min: 1, max: 6, step: 0.5 } },
    pinLean: { control: { type: 'range', min: 0, max: 0.4, step: 0.02 } },
    outlineColor: { control: 'color' },
    showGrid: { control: 'boolean' },
    animate: { control: 'boolean' },
    mini: { control: 'boolean' },
    speed: { control: { type: 'range', min: 0, max: 15, step: 0.5 } },
    angle: { control: { type: 'range', min: -180, max: 180, step: 1 } },
    size: { control: { type: 'range', min: 60, max: 600, step: 10 } },
    pinCount: {
      control: { type: 'range', min: 0, max: 10000, step: 1 },
      description: 'Number of generated demo pins; ignored in custom mode.',
    },
    distribution: { control: 'select', options: ['worldwide', 'clustered', 'custom'] },
    pins: { control: 'object', description: 'Used when distribution is custom.' },
  },
} satisfies Meta<typeof VisitorGlobeExample>;
export default meta;
type Story = StoryObj<typeof meta>;
export const Playground: Story = {};
export const Paper: Story = {
  args: {
    oceanColor: '#f3f0e8',
    oceanEdgeColor: '#f3f0e8',
    landColor: '#718379',
    gridColor: '#5c716c',
    outlineColor: '#667870',
    pinColor: '#a34c24',
  },
};
export const Night: Story = {
  args: {
    oceanColor: '#244b62',
    oceanEdgeColor: '#193446',
    landColor: '#8cbeae',
    gridColor: '#b7d3df',
    outlineColor: '#5d8493',
    pinColor: '#ffd481',
  },
};
export const Miniature: Story = { args: { size: 68, mini: true } };

export const Empty: Story = { args: { pinCount: 0 } };
export const SparseWorldwide: Story = { args: { pinCount: 12 } };
export const BusyWorldwide: Story = { args: { pinCount: 100 } };
export const DenseWorldwide: Story = { args: { pinCount: 500 } };
export const Clustered: Story = { args: { pinCount: 150, distribution: 'clustered' } };
export const CustomPins: Story = { args: { distribution: 'custom' } };

export const FlatHeads: Story = { args: { pinStyle: 'flat' } };
export const StemsOnly: Story = { args: { pinStyle: 'stem' } };
export const EquatorAndHorizon: Story = {
  args: {
    distribution: 'custom',
    angle: 0,
    speed: 6,
    pins: [-105, -95, -90, -85, -10, 0, 10, 85, 90, 95, 105].map((longitude) => ({
      longitude,
      latitude: 0,
      visitors: 1,
    })),
  },
};

export const Stress: Story = { args: { pinCount: 10000 } };

<script lang="ts">
  import VisitorGlobe from '../../src/lib/components/VisitorGlobe.svelte';
  import { createGlobeRotation } from '../../src/lib/globe/rotation.svelte';
  import type { GlobePin } from '../../src/lib/globe/presence';
  let {
    oceanColor = '#e3eff3',
    oceanEdgeColor = '#bdd6df',
    landColor = '#668f87',
    gridColor = '#385e6b',
    pinColor = '#b34e10',
    outlineColor = '#547d89',
    showGrid = true,
    animate = true,
    speed = 3,
    angle = -25,
    size = 420,
    mini = false,
    pins = [
      { longitude: 0, latitude: 30, visitors: 1 },
      { longitude: 40, latitude: 5, visitors: 3 },
      { longitude: -122, latitude: 48, visitors: 1 },
    ],
  }: {
    oceanColor?: string;
    oceanEdgeColor?: string;
    landColor?: string;
    gridColor?: string;
    pinColor?: string;
    outlineColor?: string;
    showGrid?: boolean;
    animate?: boolean;
    speed?: number;
    angle?: number;
    size?: number;
    mini?: boolean;
    pins?: GlobePin[];
  } = $props();
  const rotation = createGlobeRotation(
    () => animate,
    () => speed,
  );
  $effect(() => {
    rotation.angle = angle;
  });
</script>

<div class="example" style:width={`${size}px`}>
  <VisitorGlobe
    angle={rotation.angle}
    {mini}
    {pins}
    {oceanColor}
    {oceanEdgeColor}
    {landColor}
    {gridColor}
    {pinColor}
    {outlineColor}
    {showGrid}
  />
</div>

<style>
  .example {
    max-width: 100%;
    margin: auto;
  }
</style>

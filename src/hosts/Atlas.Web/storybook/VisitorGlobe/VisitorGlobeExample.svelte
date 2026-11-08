<script lang="ts">
  import { onMount } from 'svelte';
  import type { GeoPermissibleObjects } from 'd3-geo';
  import VisitorGlobe from '../../src/lib/components/VisitorGlobe.svelte';
  import { createGlobeRotation } from '../../src/lib/globe/rotation.svelte';
  import { demoPins } from './demoPins';
  import type { GlobePin } from '../../src/lib/globe/presence';
  let {
    targetFps = 60,
    oceanColor = '#e3eff3',
    oceanEdgeColor = '#bdd6df',
    landColor = '#668f87',
    gridColor = '#385e6b',
    pinColor = '#485453',
    pinHeadColor = '#b73539',
    variedHeads = true,
    pinStyle = 'round',
    pinLength = 23,
    pinHeadRadius = 3.5,
    pinLean = 0.2,
    outlineColor = '#547d89',
    showGrid = true,
    animate = true,
    speed = 3,
    angle = -25,
    size = 420,
    mini = false,
    pinCount = 100,
    distribution = 'worldwide',
    pins = [
      { longitude: 0, latitude: 30, visitors: 1 },
      { longitude: 40, latitude: 5, visitors: 3 },
      { longitude: -122, latitude: 48, visitors: 1 },
    ],
  }: {
    targetFps?: number;
    oceanColor?: string;
    oceanEdgeColor?: string;
    landColor?: string;
    gridColor?: string;
    pinColor?: string;
    pinHeadColor?: string;
    variedHeads?: boolean;
    pinStyle?: 'round' | 'flat' | 'stem';
    pinLength?: number;
    pinHeadRadius?: number;
    pinLean?: number;
    outlineColor?: string;
    showGrid?: boolean;
    animate?: boolean;
    speed?: number;
    angle?: number;
    size?: number;
    mini?: boolean;
    pinCount?: number;
    distribution?: 'worldwide' | 'uniform' | 'clustered' | 'custom';
    pins?: GlobePin[];
  } = $props();
  let demoLand = $state.raw<GeoPermissibleObjects>();
  let demoFailed = $state(false);
  onMount(() => {
    let active = true;
    void fetch('/globe/land.geojson')
      .then((response) => {
        if (!response.ok) throw new Error('Map unavailable');
        return response.json();
      })
      .then((value) => {
        if (active) demoLand = value;
      })
      .catch(() => {
        if (active) demoFailed = true;
      });
    return () => {
      active = false;
    };
  });
  const displayedPins = $derived(
    distribution === 'custom' ? pins : demoPins(pinCount, distribution, demoLand),
  );
  const rotation = createGlobeRotation(
    () => animate,
    () => speed,
    () => targetFps,
  );
  $effect(() => {
    rotation.angle = angle;
  });
</script>

<div class="example" style:width={`${size}px`}>
  <VisitorGlobe
    angle={rotation.angle}
    {mini}
    pins={displayedPins}
    {oceanColor}
    {oceanEdgeColor}
    {landColor}
    {gridColor}
    {pinColor}
    {pinHeadColor}
    {variedHeads}
    {pinStyle}
    {pinLength}
    {pinHeadRadius}
    {pinLean}
    {outlineColor}
    {showGrid}
  />
</div>
<p class="demo-caption" data-performance>
  WebGL · target {targetFps} FPS · {rotation.updatesPerSecond} rotation updates/s
</p>
{#if demoFailed}<p role="status">Demo land data could not load.</p>{/if}
<p class="demo-caption" data-demo-count={displayedPins.length}>
  {displayedPins.length} demo pins · {distribution}
</p>
<p class="demo-note">
  Synthetic demo locations. Worldwide uses land-only, region-weighted positions; uniform includes
  oceans; clustered tests dense areas. These are illustrative, not population estimates.
</p>

<style>
  .demo-caption,
  .demo-note {
    text-align: center;
    color: var(--atlas-muted);
    font-size: 0.85rem;
  }
  .demo-note {
    font-size: 0.75rem;
    max-width: 28rem;
    margin-inline: auto;
  }
  .example {
    max-width: min(100%, calc(100vw - 2rem));
    margin: auto;
  }
</style>

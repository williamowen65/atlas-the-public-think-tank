<script lang="ts" module>
  import type { GeoPermissibleObjects } from 'd3-geo';
  let landRequest: Promise<GeoPermissibleObjects> | undefined;
  function loadLand() {
    return (landRequest ??= fetch('/globe/land.geojson')
      .then(async (response) => {
        if (!response.ok) throw new Error('Map unavailable');
        return (await response.json()) as GeoPermissibleObjects;
      })
      .catch((error) => {
        landRequest = undefined;
        throw error;
      }));
  }
</script>

<script lang="ts">
  import WebGLGlobe from './WebGLGlobe.svelte';
  import { onMount } from 'svelte';
  import { preparePin } from '$lib/globe/pinGeometry';
  import type { GlobePin } from '$lib/globe/presence';
  let {
    pins = [],
    mini = false,
    miniPinSize = 2,
    angle = -25,
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
  }: {
    pins?: GlobePin[];
    mini?: boolean;
    miniPinSize?: number;
    angle?: number;
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
  } = $props();
  let land = $state<GeoPermissibleObjects>();
  let failed = $state(false);
  const preparedPins = $derived(pins.map((pin) => preparePin(pin, pinLength, pinLean)));
  async function retry() {
    failed = false;
    try {
      land = await loadLand();
    } catch {
      failed = true;
    }
  }
  onMount(() => {
    let active = true;
    void loadLand()
      .then((value) => {
        if (active) land = value;
      })
      .catch(() => {
        if (active) failed = true;
      });
    return () => {
      active = false;
    };
  });
</script>

<div class="globe-art" class:mini>
  <WebGLGlobe
    {angle}
    options={{
      land,
      pins: preparedPins,
      oceanColor,
      oceanEdgeColor,
      landColor,
      gridColor,
      outlineColor,
      pinColor,
      pinHeadColor,
      variedHeads,
      pinStyle,
      pinHeadRadius,
      mini,
      miniPinSize,
      showGrid,
    }}
  />
  {#if !mini && failed}<div class="map-fallback">
      <p>Map details could not load. Visitor counts and locations are still listed below.</p>
      <button type="button" onclick={retry}>Retry map</button>
    </div>{/if}
</div>

<style>
  .globe-art {
    width: 100%;
    min-width: 0;
    position: relative;
  }
  .map-fallback {
    position: absolute;
    inset: auto 0 0;
    background: var(--atlas-surface);
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    padding: 0.75rem;
    text-align: center;
    font-size: 0.9rem;
  }
</style>

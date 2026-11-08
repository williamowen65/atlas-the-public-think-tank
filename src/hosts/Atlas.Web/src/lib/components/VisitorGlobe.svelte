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
  import CanvasGlobe from './CanvasGlobe.svelte';
  import { onMount } from 'svelte';
  import { geoOrthographic, geoPath, geoGraticule10 } from 'd3-geo';
  import { projectPin } from '$lib/globe/pinGeometry';
  import type { GlobePin } from '$lib/globe/presence';
  let {
    renderer = 'svg',
    pins = [],
    mini = false,
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
    renderer?: 'svg' | 'canvas';
    pins?: GlobePin[];
    mini?: boolean;
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
  const projection = $derived(
    geoOrthographic().translate([200, 200]).scale(176).rotate([angle, -18]).clipAngle(90),
  );
  const path = $derived(geoPath(projection));
  const id = $props.id();
  const projectedPins = $derived(
    pins
      .map((pin) => ({ ...projectPin(pin, angle, pinLength, pinLean), pin }))
      .sort((a, b) => a.head[2] - b.head[2]),
  );
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

{#snippet renderPins(front: boolean)}
  {#each projectedPins as item}
    <g class="map-pin" data-layer={front ? 'front' : 'back'}>
      <title
        >{item.pin.visitors}
        {item.pin.visitors === 1 ? 'account' : 'accounts'} near {item.pin.latitude}°, {item.pin
          .longitude}°</title
      >
      {#each item.segments.filter((segment) => segment.front === front) as segment}
        <line
          class="pin-stem"
          x1={segment.from[0]}
          y1={segment.from[1]}
          x2={segment.to[0]}
          y2={segment.to[1]}
          stroke={pinColor}
          stroke-width={mini ? 1.2 : 1.5}
          stroke-linecap="round"
        />
      {/each}
      {#if pinStyle !== 'stem' && item.head[2] >= 0 === front}
        <circle
          class="pin-head"
          cx={item.head[0]}
          cy={item.head[1]}
          r={pinHeadRadius}
          fill={variedHeads ? item.color : pinHeadColor}
        />
        {#if pinStyle === 'round'}
          <ellipse
            cx={item.head[0] - pinHeadRadius * 0.3}
            cy={item.head[1] - pinHeadRadius * 0.35}
            rx={pinHeadRadius * 0.4}
            ry={pinHeadRadius * 0.23}
            fill="white"
            opacity="0.3"
          />
        {/if}
      {/if}
    </g>
  {/each}
{/snippet}

<div class="globe-art" class:mini>
  {#if renderer === 'canvas'}
    <CanvasGlobe
      {angle}
      options={{
        projection,
        land,
        pins: projectedPins,
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
        showGrid,
      }}
    />
  {:else}
    <svg
      data-renderer="svg"
      data-angle={angle}
      viewBox="-20 -20 440 440"
      role="img"
      aria-labelledby={`${id}-title`}
    >
      <title id={`${id}-title`}
        >Atlas visitor globe. Pins show approximate locations voluntarily shared by account holders.</title
      >
      <defs>
        <radialGradient id={`${id}-ocean`} cx="50%" cy="50%" r="50%">
          <stop offset="0" stop-color={oceanColor} /><stop offset="1" stop-color={oceanEdgeColor} />
        </radialGradient>
      </defs>
      {@render renderPins(false)}
      <circle
        class="ocean"
        cx="200"
        cy="200"
        r="176"
        fill={`url(#${id}-ocean)`}
        stroke={outlineColor}
        stroke-width="1"
      />
      {#if showGrid}<path
          class="graticule"
          d={path(geoGraticule10()) ?? ''}
          fill="none"
          stroke={gridColor}
          stroke-opacity="0.14"
          stroke-width="0.6"
        />{/if}
      {#if land}<path
          class="land"
          d={path(land) ?? ''}
          fill={landColor}
          stroke={outlineColor}
          stroke-width="0.35"
        />{/if}
      {@render renderPins(true)}
    </svg>
  {/if}
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
  svg {
    width: 100%;
    height: 100%;
    display: block;
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

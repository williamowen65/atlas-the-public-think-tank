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
  import { onMount } from 'svelte';
  import { geoOrthographic, geoPath, geoGraticule10, geoDistance } from 'd3-geo';
  import type { GlobePin } from '$lib/globe/presence';
  let {
    pins = [],
    mini = false,
    angle = -25,
    oceanColor = '#e3eff3',
    oceanEdgeColor = '#bdd6df',
    landColor = '#668f87',
    gridColor = '#385e6b',
    pinColor = '#b34e10',
    outlineColor = '#547d89',
    showGrid = true,
  }: {
    pins?: GlobePin[];
    mini?: boolean;
    angle?: number;
    oceanColor?: string;
    oceanEdgeColor?: string;
    landColor?: string;
    gridColor?: string;
    pinColor?: string;
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
  const visiblePins = $derived(
    pins.filter((pin) => geoDistance([pin.longitude, pin.latitude], [-angle, 18]) < Math.PI / 2),
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

<div class="globe-art" class:mini>
  <svg data-angle={angle} viewBox="0 0 400 400" role="img" aria-labelledby={`${id}-title`}>
    <title id={`${id}-title`}
      >Atlas visitor globe. Pins show approximate locations voluntarily shared by account holders.</title
    >
    <defs>
      <radialGradient id={`${id}-ocean`} cx="50%" cy="50%" r="50%">
        <stop offset="0" stop-color={oceanColor} /><stop offset="1" stop-color={oceanEdgeColor} />
      </radialGradient>
    </defs>
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
    {#each visiblePins as pin}
      {@const point = projection([pin.longitude, pin.latitude])}
      {#if point}
        {@const dx = point[0] - 200}
        {@const dy = point[1] - 200}
        {@const length = Math.hypot(dx, dy)}
        {@const tipX = point[0] + (length > 25 ? (dx / length) * 23 : 3)}
        {@const tipY = point[1] + (length > 25 ? (dy / length) * 23 : -18)}
        <g class="map-pin">
          <title
            >{pin.visitors}
            {pin.visitors === 1 ? 'account' : 'accounts'} near {pin.latitude}°, {pin.longitude}°</title
          >
          <line
            x1={point[0]}
            y1={point[1]}
            x2={tipX}
            y2={tipY}
            stroke="#ffffff"
            stroke-opacity="0.85"
            stroke-width={mini ? 2.8 : 3.8}
            vector-effect="non-scaling-stroke"
            stroke-linecap="round"
          />
          <line
            class="pin-stem"
            x1={point[0]}
            y1={point[1]}
            x2={tipX}
            y2={tipY}
            stroke={pinColor}
            stroke-width={mini ? 1.5 : 2}
            vector-effect="non-scaling-stroke"
            stroke-linecap="round"
          />
        </g>
      {/if}
    {/each}
  </svg>
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

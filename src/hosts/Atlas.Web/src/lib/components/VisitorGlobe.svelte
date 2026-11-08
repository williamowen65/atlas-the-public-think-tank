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
    animate = true,
  }: { pins?: GlobePin[]; mini?: boolean; animate?: boolean } = $props();
  let land = $state<GeoPermissibleObjects>();
  let failed = $state(false);
  let angle = $state(-25);
  const projection = $derived(
    geoOrthographic().translate([200, 200]).scale(184).rotate([angle, -18]).clipAngle(90),
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
    const reducedMotion = matchMedia('(prefers-reduced-motion: reduce)');
    let frame = 0;
    let previous = 0;
    function rotate(now: number) {
      if (animate && !document.hidden && !reducedMotion.matches && now - previous >= 40) {
        angle += Math.min(now - previous, 100) * 0.003;
        previous = now;
      } else if (!animate || document.hidden || reducedMotion.matches) previous = now;
      frame = requestAnimationFrame(rotate);
    }
    frame = requestAnimationFrame(rotate);
    return () => {
      active = false;
      cancelAnimationFrame(frame);
    };
  });
</script>

<div class="globe-art" class:mini>
  <svg viewBox="0 0 400 400" role="img" aria-labelledby={`${id}-title`}>
    <title id={`${id}-title`}
      >Atlas visitor globe. Pins show approximate locations voluntarily shared by visitors.</title
    >
    <defs>
      <radialGradient id={`${id}-ocean`} cx="35%" cy="25%" r="80%">
        <stop offset="0" stop-color="#194d68" /><stop offset="1" stop-color="#061a2e" />
      </radialGradient>
      <radialGradient id={`${id}-light`} cx="27%" cy="25%" r="75%">
        <stop offset="0" stop-color="#fff" stop-opacity="0.10" /><stop
          offset="0.65"
          stop-color="#fff"
          stop-opacity="0"
        /><stop offset="1" stop-color="#020c18" stop-opacity="0.55" />
      </radialGradient>
    </defs>
    <circle
      cx="200"
      cy="200"
      r="184"
      fill={`url(#${id}-ocean)`}
      stroke="#528ba6"
      stroke-width="1.5"
    />
    <path
      d={path(geoGraticule10()) ?? ''}
      fill="none"
      stroke="#7cbbc9"
      stroke-opacity="0.17"
      stroke-width="0.6"
    />
    {#if land}<path d={path(land) ?? ''} fill="#69afa2" stroke="#a7d9bf" stroke-width="0.4" />{/if}
    <circle cx="200" cy="200" r="184" fill={`url(#${id}-light)`} />
    {#each visiblePins as pin}
      {@const point = projection([pin.longitude, pin.latitude])}
      {#if point}
        <circle
          cx={point[0]}
          cy={point[1]}
          r={mini ? 9 : Math.min(10, 4 + Math.sqrt(pin.visitors))}
          fill="#ffd481"
          stroke="#4a310f"
          stroke-width="1.2"
        >
          <title
            >{pin.visitors}
            {pin.visitors === 1 ? 'visitor' : 'visitors'} near {pin.latitude}°, {pin.longitude}°</title
          >
        </circle>
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
    filter: drop-shadow(0 0 24px #54b6b51a);
  }
  .map-fallback {
    padding: 0.75rem;
    text-align: center;
    font-size: 0.9rem;
  }
</style>

<script lang="ts">
  import { onMount } from 'svelte';
  import { createWebGLGlobe, type WebGLGlobeOptions } from '$lib/globe/webglRenderer';
  let { options, angle }: { options: WebGLGlobeOptions; angle: number } = $props();
  let canvas: HTMLCanvasElement;
  let engine = $state.raw<ReturnType<typeof createWebGLGlobe>>();
  let width = $state(0);
  let pixelRatio = $state(1);
  let unavailable = $state(false);
  let lost = $state(false);
  onMount(() => {
    const initialize = () => {
      try {
        engine = createWebGLGlobe(canvas);
        unavailable = false;
        lost = false;
      } catch (error) {
        console.warn('Globe WebGL initialization failed', error);
        unavailable = true;
      }
    };
    const resize = () => {
      width = canvas.getBoundingClientRect().width;
      pixelRatio = window.devicePixelRatio || 1;
    };
    const contextLost = (event: Event) => {
      event.preventDefault();
      lost = true;
      engine = undefined;
    };
    canvas.addEventListener('webglcontextlost', contextLost);
    canvas.addEventListener('webglcontextrestored', initialize);
    const observer = new ResizeObserver(resize);
    observer.observe(canvas);
    window.addEventListener('resize', resize);
    initialize();
    resize();
    return () => {
      observer.disconnect();
      window.removeEventListener('resize', resize);
      canvas.removeEventListener('webglcontextlost', contextLost);
      canvas.removeEventListener('webglcontextrestored', initialize);
      engine?.dispose();
    };
  });
  $effect(() => {
    engine?.setMap(options);
  });
  $effect(() => {
    engine?.setPins(options);
  });
  $effect(() => {
    engine?.setStyle(options);
  });
  $effect(() => {
    // Draw after map, geometry or style changes as well as camera/size changes.
    void options;
    if (!engine || !width) return;
    const pixels = Math.max(1, Math.round(width * pixelRatio));
    if (canvas.width !== pixels || canvas.height !== pixels) {
      canvas.width = pixels;
      canvas.height = pixels;
    }
    engine.draw(angle);
  });
</script>

<div
  role="img"
  aria-label="Atlas visitor globe. Pins show approximate locations voluntarily shared by account holders."
>
  <canvas
    bind:this={canvas}
    data-renderer="webgl"
    data-angle={angle}
    data-ready={!!engine}
    aria-hidden="true"
  >
    Atlas visitor globe showing approximate shared account locations.
  </canvas>
</div>
{#if unavailable || lost}
  <p role="status">
    {lost ? 'WebGL graphics context was lost.' : 'WebGL 2 is unavailable in this browser.'} Select SVG
    or Canvas to continue comparing.
  </p>
{/if}

<style>
  canvas {
    display: block;
    width: 100%;
    aspect-ratio: 1;
  }
</style>

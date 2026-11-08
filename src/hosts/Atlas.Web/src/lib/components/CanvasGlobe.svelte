<script lang="ts">
  import { onMount } from 'svelte';
  import { drawCanvasGlobe, type CanvasGlobeOptions } from '$lib/globe/canvasRenderer';
  let { options, angle }: { options: CanvasGlobeOptions; angle: number } = $props();
  let canvas: HTMLCanvasElement;
  let width = $state(0);
  let pixelRatio = $state(1);
  let unavailable = $state(false);
  onMount(() => {
    const resize = () => {
      width = canvas.getBoundingClientRect().width;
      pixelRatio = window.devicePixelRatio || 1;
    };
    const observer = new ResizeObserver(resize);
    observer.observe(canvas);
    window.addEventListener('resize', resize);
    resize();
    return () => {
      observer.disconnect();
      window.removeEventListener('resize', resize);
    };
  });
  $effect(() => {
    if (!canvas || !width) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) {
      unavailable = true;
      return;
    }
    const pixels = Math.max(1, Math.round(width * pixelRatio));
    if (canvas.width !== pixels || canvas.height !== pixels) {
      canvas.width = pixels;
      canvas.height = pixels;
    }
    ctx.setTransform(1, 0, 0, 1, 0, 0);
    ctx.clearRect(0, 0, pixels, pixels);
    const scale = pixels / 440;
    ctx.setTransform(scale, 0, 0, scale, 20 * scale, 20 * scale);
    drawCanvasGlobe(ctx, options);
  });
</script>

<div
  role="img"
  aria-label="Atlas visitor globe. Pins show approximate locations voluntarily shared by account holders."
>
  <canvas bind:this={canvas} data-angle={angle} data-renderer="canvas" aria-hidden="true">
    Atlas visitor globe showing approximate shared account locations.
  </canvas>
</div>
{#if unavailable}<p>Canvas is unavailable in this browser. Select the SVG renderer.</p>{/if}

<style>
  canvas {
    display: block;
    width: 100%;
    aspect-ratio: 1;
  }
</style>

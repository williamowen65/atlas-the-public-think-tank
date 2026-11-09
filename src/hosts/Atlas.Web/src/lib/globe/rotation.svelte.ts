import { onMount } from 'svelte';

/** One clock per globe experience, shared by its miniature and expanded renderers. */
export function createGlobeRotation(
  enabled = () => true,
  degreesPerSecond = () => 3,
  targetFps = () => 60,
) {
  const rotation = $state({ angle: -25, updatesPerSecond: 0 });
  onMount(() => {
    const reducedMotion = matchMedia('(prefers-reduced-motion: reduce)');
    let frame = 0;
    let previous = performance.now();
    let sampleStart = previous;
    let updates = 0;
    function rotate(now: number) {
      if (
        enabled() &&
        !document.hidden &&
        !reducedMotion.matches &&
        now - previous >= 1000 / Math.max(1, Math.min(60, targetFps())) - 0.5
      ) {
        rotation.angle =
          (rotation.angle + (Math.min(now - previous, 100) * degreesPerSecond()) / 1000) % 360;
        previous = now;
        updates++;
      } else if (!enabled() || document.hidden || reducedMotion.matches) previous = now;
      if (now - sampleStart >= 1000) {
        rotation.updatesPerSecond = Math.round((updates * 1000) / (now - sampleStart));
        sampleStart = now;
        updates = 0;
      }
      frame = requestAnimationFrame(rotate);
    }
    frame = requestAnimationFrame(rotate);
    return () => cancelAnimationFrame(frame);
  });
  return rotation;
}

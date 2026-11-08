import { onMount } from 'svelte';

/** One clock per globe experience, shared by its miniature and expanded renderers. */
export function createGlobeRotation(enabled = () => true, degreesPerSecond = () => 3) {
  const rotation = $state({ angle: -25 });
  onMount(() => {
    const reducedMotion = matchMedia('(prefers-reduced-motion: reduce)');
    let frame = 0;
    let previous = performance.now();
    function rotate(now: number) {
      if (enabled() && !document.hidden && !reducedMotion.matches && now - previous >= 40) {
        rotation.angle =
          (rotation.angle + (Math.min(now - previous, 100) * degreesPerSecond()) / 1000) % 360;
        previous = now;
      } else if (!enabled() || document.hidden || reducedMotion.matches) previous = now;
      frame = requestAnimationFrame(rotate);
    }
    frame = requestAnimationFrame(rotate);
    return () => cancelAnimationFrame(frame);
  });
  return rotation;
}

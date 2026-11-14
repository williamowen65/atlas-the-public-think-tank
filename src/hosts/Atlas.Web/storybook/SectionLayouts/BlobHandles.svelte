<script lang="ts">
  import { blobCurve, type BlobSide, type BlobTangent } from './blobCurve';
  import { blobRadii, type PhotoFrame } from './sectionLayouts';

  let { frame, onChange }: {
    frame: PhotoFrame;
    onChange: (tangents: BlobTangent[], first: boolean) => void;
  } = $props();

  const nodes = $derived(blobCurve(frame.blobTangents, blobRadii(frame)));
  const sides = ['incoming', 'outgoing'] as const;
  let overlay: HTMLDivElement;
  let drag: { pointer: number; changed: boolean } | undefined;

  function adjust(index: number, side: BlobSide, x: number, y: number, first: boolean) {
    const node = nodes[index];
    const sign = side === 'incoming' ? -1 : 1;
    const dx = (x - node.x) * sign;
    const dy = (y - node.y) * sign;
    const angle = (Math.atan2(dy, dx) * 180 / Math.PI - node.angle + 540) % 360 - 180;
    const boundedAngle = Math.abs(dx) + Math.abs(dy) < .001 ? node.value.angle : Math.max(-20, Math.min(20, angle));
    // Project onto the allowed tangent so pulling across the anchor shortens the handle.
    const radians = (node.angle + boundedAngle) * Math.PI / 180;
    const length = Math.max(.09, Math.min(.25, dx * Math.cos(radians) + dy * Math.sin(radians)));
    if (Math.abs(length - node.value[side]) < .0001 && Math.abs(boundedAngle - node.value.angle) < .01) return false;
    onChange(nodes.map(({ value }, i) => i === index ? { ...value, angle: boundedAngle, [side]: length } : value), first);
    return true;
  }

  function move(event: PointerEvent, index: number, side: BlobSide) {
    if (drag?.pointer !== event.pointerId) return;
    const bounds = overlay.getBoundingClientRect();
    if (!bounds.width || !bounds.height) return;
    if (adjust(index, side, (event.clientX - bounds.left) / bounds.width,
      (event.clientY - bounds.top) / bounds.height, !drag.changed)) drag.changed = true;
  }

  function keydown(event: KeyboardEvent, index: number, side: BlobSide) {
    const dx = event.key === 'ArrowLeft' ? -.012 : event.key === 'ArrowRight' ? .012 : 0;
    const dy = event.key === 'ArrowUp' ? -.012 : event.key === 'ArrowDown' ? .012 : 0;
    if (!dx && !dy) return;
    event.preventDefault();
    const point = nodes[index][side];
    adjust(index, side, point.x + dx, point.y + dy, true);
  }
</script>

<div class="blob-handles" bind:this={overlay} role="group" aria-label="Blob shape handles">
  <svg viewBox="0 0 1 1" aria-hidden="true">
    {#each nodes as node (node.name)}
      <line x1={node.incoming.x} y1={node.incoming.y} x2={node.outgoing.x} y2={node.outgoing.y} />
    {/each}
  </svg>
  {#each nodes as node, index (node.name)}
    <span class="anchor" aria-hidden="true" style:left={node.x * 100 + '%'} style:top={node.y * 100 + '%'}></span>
    {#each sides as side}
      <button type="button" aria-label={`Adjust ${node.name.toLowerCase()} ${side} blob handle`}
        style:left={node[side].x * 100 + '%'} style:top={node[side].y * 100 + '%'}
        onpointerdown={(event) => {
          if (event.button !== 0 || drag) return;
          event.currentTarget.setPointerCapture(event.pointerId);
          drag = { pointer: event.pointerId, changed: false };
        }}
        onpointermove={(event) => move(event, index, side)}
        onlostpointercapture={(event) => { if (drag?.pointer === event.pointerId) drag = undefined; }}
        onkeydown={(event) => keydown(event, index, side)}>
        <span></span>
      </button>
    {/each}
  {/each}
</div>

<style>
  .blob-handles { position: absolute; inset: 0 0 auto; margin-inline: auto; width: var(--photo-width, 100%); aspect-ratio: 1; pointer-events: none; }
  svg { position: absolute; inset: 0; width: 100%; height: 100%; overflow: visible; }
  line { stroke: #766aff; stroke-width: 2px; vector-effect: non-scaling-stroke; }
  .anchor, button { position: absolute; transform: translate(-50%, -50%); }
  .anchor { width: .5rem; height: .5rem; background: white; border: 1px solid #4434b0; }
  button {
    width: 2rem; height: 2rem; padding: 0; border: 0; background: transparent;
    display: grid; place-items: center; pointer-events: auto; touch-action: none; cursor: move;
  }
  button span { width: .65rem; height: .65rem; border: 2px solid #4434b0; border-radius: 50%; background: white; }
  button:focus-visible span { box-shadow: 0 0 0 2px white, 0 0 0 4px #4434b0; }
</style>

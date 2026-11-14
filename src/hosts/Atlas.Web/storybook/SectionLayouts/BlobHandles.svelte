<script lang="ts">
  import { blobCurve, blobLimits, bounded, type BlobSide, type BlobTangent } from './blobCurve';
  import { blobRadii, type PhotoFrame } from './sectionLayouts';

  let { frame, onChange }: {
    frame: PhotoFrame;
    onChange: (tangents: BlobTangent[], first: boolean) => void;
  } = $props();

  const nodes = $derived(blobCurve(frame.blobTangents, blobRadii(frame)));
  const sides = ['incoming', 'outgoing'] as const;
  let overlay: HTMLDivElement;
  type Control = BlobSide | 'anchor';
  let drag: { pointer: number; offsetX: number; offsetY: number; changed: boolean } | undefined;

  function controlPoint(index: number, control: Control) {
    return control === 'anchor' ? nodes[index] : nodes[index][control];
  }

  function emitChange(index: number, update: Partial<BlobTangent>, first: boolean) {
    onChange(nodes.map(({ value }, i) => i === index ? { ...value, ...update } : value), first);
  }

  function adjust(index: number, side: Control, x: number, y: number, first: boolean) {
    const node = nodes[index];
    if (side === 'anchor') {
      const travel = blobLimits.anchorTravel;
      const nextX = bounded(x, node.origin.x - travel, node.origin.x + travel, node.x);
      const nextY = bounded(y, node.origin.y - travel, node.origin.y + travel, node.y);
      if (Math.abs(nextX - node.x) < .0001 && Math.abs(nextY - node.y) < .0001) return false;
      emitChange(index, { x: nextX, y: nextY }, first);
      return true;
    }
    const sign = side === 'incoming' ? -1 : 1;
    const dx = (x - node.x) * sign;
    const dy = (y - node.y) * sign;
    const angle = (Math.atan2(dy, dx) * 180 / Math.PI - node.angle + 540) % 360 - 180;
    const boundedAngle = Math.abs(dx) + Math.abs(dy) < .001 ? node.value.angle : bounded(angle, -blobLimits.angle, blobLimits.angle, node.value.angle);
    // Project onto the allowed tangent so pulling across the anchor shortens the handle.
    const radians = (node.angle + boundedAngle) * Math.PI / 180;
    const length = bounded(dx * Math.cos(radians) + dy * Math.sin(radians), blobLimits.minLength, blobLimits.maxLength, node.value[side]);
    if (Math.abs(length - node.value[side]) < .0001 && Math.abs(boundedAngle - node.value.angle) < .01) return false;
    emitChange(index, { angle: boundedAngle, [side]: length }, first);
    return true;
  }

  function start(event: PointerEvent & { currentTarget: HTMLButtonElement }, index: number, control: Control) {
    if (event.button !== 0 || drag) return;
    const bounds = overlay.getBoundingClientRect();
    if (!bounds.width || !bounds.height) return;
    const point = controlPoint(index, control);
    event.currentTarget.setPointerCapture(event.pointerId);
    drag = {
      pointer: event.pointerId,
      offsetX: point.x - (event.clientX - bounds.left) / bounds.width,
      offsetY: point.y - (event.clientY - bounds.top) / bounds.height,
      changed: false,
    };
  }

  function move(event: PointerEvent, index: number, side: Control) {
    if (drag?.pointer !== event.pointerId) return;
    const bounds = overlay.getBoundingClientRect();
    if (!bounds.width || !bounds.height) return;
    if (adjust(index, side, (event.clientX - bounds.left) / bounds.width + drag.offsetX,
      (event.clientY - bounds.top) / bounds.height + drag.offsetY, !drag.changed)) drag.changed = true;
  }

  function keydown(event: KeyboardEvent, index: number, side: Control) {
    const dx = event.key === 'ArrowLeft' ? -.012 : event.key === 'ArrowRight' ? .012 : 0;
    const dy = event.key === 'ArrowUp' ? -.012 : event.key === 'ArrowDown' ? .012 : 0;
    if (!dx && !dy) return;
    event.preventDefault();
    const point = controlPoint(index, side);
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
    {@render control(index, 'anchor', `Move ${node.name.toLowerCase()} blob anchor`)}
    {#each sides as side}
      {@render control(index, side, `Adjust ${node.name.toLowerCase()} ${side} blob handle`)}
    {/each}
  {/each}
</div>

{#snippet control(index: number, kind: Control, label: string)}
  {@const point = controlPoint(index, kind)}
  <button type="button" class:anchor={kind === 'anchor'} aria-label={label}
    style:left={point.x * 100 + '%'} style:top={point.y * 100 + '%'}
    onpointerdown={(event) => start(event, index, kind)}
    onpointermove={(event) => move(event, index, kind)}
    onlostpointercapture={(event) => { if (drag?.pointer === event.pointerId) drag = undefined; }}
    onkeydown={(event) => keydown(event, index, kind)}>
    <span></span>
  </button>
{/snippet}

<style>
  .blob-handles { position: absolute; inset: 0 0 auto; margin-inline: auto; width: var(--photo-width, 100%); aspect-ratio: 1; pointer-events: none; }
  svg { position: absolute; inset: 0; width: 100%; height: 100%; overflow: visible; }
  line { stroke: #766aff; stroke-width: 2px; vector-effect: non-scaling-stroke; }
  button { position: absolute; transform: translate(-50%, -50%); }
  button.anchor { z-index: 1; }
  button.anchor span { width: .5rem; height: .5rem; border-radius: 0; }
  button {
    width: 2rem; height: 2rem; padding: 0; border: 0; background: transparent;
    display: grid; place-items: center; pointer-events: auto; touch-action: none; cursor: move;
  }
  button span { width: .65rem; height: .65rem; border: 2px solid #4434b0; border-radius: 50%; background: white; }
  button:focus-visible span { box-shadow: 0 0 0 2px white, 0 0 0 4px #4434b0; }
</style>

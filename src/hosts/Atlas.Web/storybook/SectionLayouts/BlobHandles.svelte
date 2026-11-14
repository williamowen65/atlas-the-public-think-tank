<script lang="ts">
  import { blobRadii, type BlobRadii, type PhotoFrame } from './sectionLayouts';

  let { frame, onChange }: {
    frame: PhotoFrame;
    onChange: (radii: BlobRadii, first: boolean) => void;
  } = $props();

  const names = ['Top left', 'Top right', 'Bottom right', 'Bottom left'];
  const radii = $derived(blobRadii(frame));
  const edgeOffset = 1 - Math.SQRT1_2;
  let overlay: HTMLDivElement;
  let drag: { pointer: number; x: number; y: number; radii: BlobRadii; changed: boolean } | undefined;

  function adjust(index: number, initial: BlobRadii, dx: number, dy: number, first: boolean) {
    const next = [...initial] as BlobRadii;
    const x = index * 2;
    const y = x + 1;
    const horizontalNeighbor = [2, 0, 6, 4][index];
    const verticalNeighbor = [7, 5, 3, 1][index];
    next[x] = Math.round(Math.max(30, Math.min(100 - initial[horizontalNeighbor], initial[x] + dx)));
    next[y] = Math.round(Math.max(30, Math.min(100 - initial[verticalNeighbor], initial[y] + dy)));
    if (next[x] === radii[x] && next[y] === radii[y]) return false;
    onChange(next, first);
    return true;
  }

  function start(event: PointerEvent & { currentTarget: HTMLButtonElement }) {
    if (event.button !== 0 || drag) return;
    event.currentTarget.setPointerCapture(event.pointerId);
    drag = { pointer: event.pointerId, x: event.clientX, y: event.clientY, radii: [...radii], changed: false };
  }

  function move(event: PointerEvent, index: number, left: boolean, top: boolean) {
    if (!drag || drag.pointer !== event.pointerId) return;
    const { width, height } = overlay.getBoundingClientRect();
    const dx = (event.clientX - drag.x) / width * 100 / edgeOffset * (left ? 1 : -1);
    const dy = (event.clientY - drag.y) / height * 100 / edgeOffset * (top ? 1 : -1);
    if (adjust(index, drag.radii, dx, dy, !drag.changed)) drag.changed = true;
  }

  function keydown(event: KeyboardEvent, index: number, left: boolean, top: boolean) {
    const dx = event.key === 'ArrowLeft' ? -2 : event.key === 'ArrowRight' ? 2 : 0;
    const dy = event.key === 'ArrowUp' ? -2 : event.key === 'ArrowDown' ? 2 : 0;
    if (!dx && !dy) return;
    event.preventDefault();
    adjust(index, radii, dx * (left ? 1 : -1), dy * (top ? 1 : -1), true);
  }
</script>

<div class="blob-handles" bind:this={overlay} role="group" aria-label="Blob shape handles">
  {#each names as name, index}
    {@const left = index === 0 || index === 3}
    {@const top = index < 2}
    {@const x = radii[index * 2] * edgeOffset}
    {@const y = radii[index * 2 + 1] * edgeOffset}
    <button type="button" aria-label={`Adjust ${name.toLowerCase()} blob corner`} title={name}
      style:left={(left ? x : 100 - x) + '%'} style:top={(top ? y : 100 - y) + '%'}
      onpointerdown={start} onpointermove={(event) => move(event, index, left, top)}
      onlostpointercapture={(event) => { if (drag?.pointer === event.pointerId) drag = undefined; }}
      onkeydown={(event) => keydown(event, index, left, top)}>
      <span></span>
    </button>
  {/each}
</div>

<style>
  .blob-handles { position: absolute; inset: 0 0 auto; margin-inline: auto; width: var(--photo-width, 100%); aspect-ratio: 1; pointer-events: none; }
  button {
    position: absolute; width: 2rem; height: 2rem; padding: 0; border: 0; background: transparent;
    display: grid; place-items: center; transform: translate(-50%, -50%);
    pointer-events: auto; touch-action: none; cursor: move;
  }
  span {
    width: .8rem; height: .8rem; border: 2px solid white; border-radius: 50%;
    background: #174e65; box-shadow: 0 0 0 1px #174e65;
  }
  button:focus-visible span { box-shadow: 0 0 0 3px #174e65, 0 0 0 5px white; }
</style>

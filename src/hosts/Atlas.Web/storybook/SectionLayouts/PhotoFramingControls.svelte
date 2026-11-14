<script lang="ts">
  import PhotoFrame from './PhotoFrame.svelte';
  import type { Photo, PhotoFrame as Frame } from './sectionLayouts';
  let { photo, src, gallery, fitTile = false, slideshowFit, onChange }: {
    photo: Photo; src?: string; gallery: boolean; fitTile?: boolean; slideshowFit?: 'contain' | 'cover'; onChange: (frame: Frame) => void;
  } = $props();
  const focalHintId = $props.id();
  const focalSteps: Record<string, readonly [number, number]> = {
    ArrowLeft: [-5, 0], ArrowRight: [5, 0], ArrowUp: [0, -5], ArrowDown: [0, 5],
  };
  const ratio = $derived(photo.frame?.ratio ?? (gallery ? '1 / 1' : 'natural'));
  function update(change: Frame) { onChange({ ...photo.frame, ...change }); }

  function moveFocalPoint(event: KeyboardEvent) {
    const step = focalSteps[event.key];
    if (!step || !Array.isArray(step)) return;
    event.preventDefault();
    const [dx, dy] = step;
    update({
      x: Math.min(100, Math.max(0, (photo.frame?.x ?? 50) + dx)),
      y: Math.min(100, Math.max(0, (photo.frame?.y ?? 50) + dy)),
    });
  }
</script>

<fieldset>
  <legend>Framing</legend>
  {#if slideshowFit}
    <small>The slideshow sets the frame size. Width and crop shape apply in other layouts.</small>
  {:else if fitTile}
    <small>Mosaic sets the tile size. Width and crop shape apply in other layouts.</small>
  {:else}
    <label><span class="range-label">Image width <span aria-hidden="true">{photo.frame?.width ?? 100}%</span></span>
      <input type="range" min="25" max="100" step="5" value={photo.frame?.width ?? 100}
        onchange={(event) => update({ width: event.currentTarget.valueAsNumber })} />
    </label>
    <label>Crop shape
      <select value={ratio} onchange={(event) => update({ ratio: event.currentTarget.value as Frame['ratio'] })}>
        <option value="natural">Original proportions</option>
        <option value="1 / 1">Square</option>
        <option value="4 / 5">Portrait</option>
        <option value="2 / 3">Tall portrait</option>
        <option value="4 / 3">Landscape</option>
      </select>
    </label>
  {/if}
  <label>Corners
    <select value={photo.frame?.corners ?? 'rounded'}
      onchange={(event) => update({ corners: event.currentTarget.value as Frame['corners'] })}>
      <option value="rounded">Rounded</option><option value="square">Square</option>
    </select>
  </label>
  {#if slideshowFit === 'cover' || (!slideshowFit && (fitTile || ratio !== 'natural'))}
    <label><span class="range-label">Crop zoom <span aria-hidden="true">{photo.frame?.zoom ?? 1}×</span></span>
      <input type="range" min="1" max="3" step=".1" value={photo.frame?.zoom ?? 1}
        onchange={(event) => update({ zoom: event.currentTarget.valueAsNumber })} />
    </label>
    {#if src}
      <div class="focal-label">Focal point
        <small id={focalHintId}>Click or tap the photo to set the focal point. Use the arrow keys when the photo is focused.</small>
      </div>
      <button class="focal-preview" type="button" aria-label="Set focal point on photo"
        aria-describedby={focalHintId} onkeydown={moveFocalPoint}
        onclick={(event) => {
          if (!event.detail) { update({ x: 50, y: 50 }); return; }
          const rect = event.currentTarget.getBoundingClientRect();
          update({
            x: Math.round(Math.min(100, Math.max(0, (event.clientX - rect.left) / rect.width * 100))),
            y: Math.round(Math.min(100, Math.max(0, (event.clientY - rect.top) / rect.height * 100))),
          });
        }}>
        <PhotoFrame photo={{ ...photo, caption: '', frame: { ratio: 'natural' } }} {src} />
        <span class="dot" aria-hidden="true" style:left={(photo.frame?.x ?? 50) + '%'} style:top={(photo.frame?.y ?? 50) + '%'}></span>
      </button>
    {/if}
  {/if}
</fieldset>

<style>
  fieldset {
    min-inline-size: 0; margin: .25rem 0 0; padding: .65rem;
    display: grid; gap: .55rem; border-radius: var(--atlas-radius, .45rem);
    border: 1px solid color-mix(in srgb, var(--atlas-border) 45%, transparent);
  }
  legend { padding-inline: .25rem; font-size: .85rem; font-weight: 650; }
  label { display: grid; gap: .25rem; }
  small { color: var(--atlas-muted); }
  .range-label { display: flex; justify-content: space-between; align-items: baseline; gap: .5rem; }
  .range-label span { font-weight: 400; font-variant-numeric: tabular-nums; color: var(--atlas-muted); }
  .focal-label { display: grid; gap: .25rem; font-size: .875rem; font-weight: 600; }
  .focal-label small { font-weight: 400; }
  .focal-preview { justify-self: center; position: relative; display: block; width: min(100%, 12rem); padding: 0; border: 0; background: transparent; cursor: crosshair; }
  .dot { position: absolute; transform: translate(-50%, -50%); width: .85rem; height: .85rem;
    border: 2px solid white; border-radius: 50%; background: #172f40; pointer-events: none; }
</style>

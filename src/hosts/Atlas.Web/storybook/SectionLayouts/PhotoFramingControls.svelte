<script lang="ts">
  import PhotoFrame from './PhotoFrame.svelte';
  import type { Photo, PhotoFrame as Frame } from './sectionLayouts';
  let { photo, src, gallery, onChange }: {
    photo: Photo; src?: string; gallery: boolean; onChange: (frame: Frame) => void;
  } = $props();
  const ratio = $derived(photo.frame?.ratio ?? (gallery ? '1 / 1' : 'natural'));
  function update(change: Frame) { onChange({ ...photo.frame, ...change }); }
</script>

<label>Image width · {photo.frame?.width ?? 100}% of slot
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
<label>Corners
  <select value={photo.frame?.corners ?? 'rounded'}
    onchange={(event) => update({ corners: event.currentTarget.value as Frame['corners'] })}>
    <option value="rounded">Rounded</option><option value="square">Square</option>
  </select>
</label>
{#if ratio !== 'natural'}
  <label>Crop zoom · {photo.frame?.zoom ?? 1}×
    <input type="range" min="1" max="3" step=".1" value={photo.frame?.zoom ?? 1}
      onchange={(event) => update({ zoom: event.currentTarget.valueAsNumber })} />
  </label>
  {#if src}
    <button class="focal-preview" type="button" aria-label="Set focal point on photo"
      onclick={(event) => {
        if (!event.detail) { update({ x: 50, y: 50 }); return; }
        const rect = event.currentTarget.getBoundingClientRect();
        update({
          x: Math.round(Math.min(100, Math.max(0, (event.clientX - rect.left) / rect.width * 100))),
          y: Math.round(Math.min(100, Math.max(0, (event.clientY - rect.top) / rect.height * 100))),
        });
      }}>
      <PhotoFrame photo={{ ...photo, frame: { ratio: 'natural' } }} {src} />
      <span class="dot" aria-hidden="true" style:left={(photo.frame?.x ?? 50) + '%'} style:top={(photo.frame?.y ?? 50) + '%'}></span>
    </button>
    <small>Click or tap the photo to set its focal point, or use the sliders.</small>
  {/if}
  <label>Focal point horizontal
    <input type="range" min="0" max="100" value={photo.frame?.x ?? 50}
      onchange={(event) => update({ x: event.currentTarget.valueAsNumber })} />
  </label>
  <label>Focal point vertical
    <input type="range" min="0" max="100" value={photo.frame?.y ?? 50}
      onchange={(event) => update({ y: event.currentTarget.valueAsNumber })} />
  </label>
{/if}

<style>
  .focal-preview { position: relative; display: block; width: min(100%, 12rem); padding: 0; border: 0; background: transparent; cursor: crosshair; }
  .dot { position: absolute; transform: translate(-50%, -50%); width: .85rem; height: .85rem;
    border: 2px solid white; border-radius: 50%; background: #172f40; pointer-events: none; }
</style>

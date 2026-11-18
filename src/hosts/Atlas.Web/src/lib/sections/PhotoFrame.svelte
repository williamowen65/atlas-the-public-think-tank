<script lang="ts">
  import { blobCurve, blobPath } from './blobCurve';
  import { blobRadii, photoCropPreset, type Photo } from './sectionLayouts';
  let { photo, src, gallery = false }: {
    photo: Photo; src: string; gallery?: boolean;
  } = $props();
  const crop = $derived(photoCropPreset(photo.frame?.ratio, gallery));
  const clipId = $props.id();
  const path = $derived(crop.id === 'blob' ? blobPath(blobCurve(photo.frame?.blobTangents, blobRadii(photo.frame))) : undefined);
  const x = $derived((photo.frame?.x ?? 50) + '%');
  const y = $derived((photo.frame?.y ?? 50) + '%');
</script>

<span class="frame" class:crop={!!crop.ratio} class:square={photo.frame?.corners === 'square'}
  class:extra-rounded={photo.frame?.corners === 'extra-rounded'}
  data-silhouette={'silhouette' in crop ? crop.silhouette : undefined}
  style:--blob-clip={path ? `url("#${clipId}")` : undefined}
  style:aspect-ratio={crop.ratio}
  style:--focus-x={x} style:--focus-y={y} style:--zoom={photo.frame?.zoom ?? 1}>
  {#if path}
    <svg class="clip-definition" width="0" height="0" aria-hidden="true">
      <defs><clipPath id={clipId} clipPathUnits="objectBoundingBox"><path d={path} /></clipPath></defs>
    </svg>
  {/if}
  <img data-photo-frame draggable="false" {src} alt={photo.alt} loading="lazy" referrerpolicy="no-referrer" />
  {#if photo.caption && photo.captionPlacement === 'overlay'}
    <span class="caption-overlay">{photo.caption}</span>
  {/if}
</span>

<style>
  .frame { position: relative; display: block; overflow: hidden; border-radius: var(--atlas-radius, .5rem); }
  .clip-definition { position: absolute; }
  .caption-overlay {
    position: absolute; inset: auto 0 0; padding: .2rem .4rem;
    background: rgb(0 0 0 / 65%); color: white; text-align: center;
    font-size: var(--atlas-node-meta-size); line-height: var(--atlas-node-meta-line-height);
    overflow-wrap: anywhere; pointer-events: none;
  }
  .square { border-radius: 0; }
  .extra-rounded { border-radius: 1.5rem; }
  .frame[data-silhouette="circle"],
  .frame[data-silhouette="pill"] { border-radius: 9999px; }
  .frame[data-silhouette="blob"] { border-radius: 0; clip-path: var(--blob-clip); }
  .frame[data-silhouette] .caption-overlay { inset: auto 16% 15%; border-radius: var(--atlas-radius, .5rem); }
  img { display: block; width: 100%; height: auto; }
  .crop img { height: 100%; object-fit: cover; object-position: var(--focus-x) var(--focus-y);
    transform: scale(var(--zoom)); transform-origin: var(--focus-x) var(--focus-y); }
</style>

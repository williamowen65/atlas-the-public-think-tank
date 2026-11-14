<script lang="ts">
  import { blobProfiles, photoCropPreset, type Photo } from './sectionLayouts';
  let { photo, src, gallery = false }: {
    photo: Photo; src: string; gallery?: boolean;
  } = $props();
  const crop = $derived(photoCropPreset(photo.frame?.ratio, gallery));
  const x = $derived((photo.frame?.x ?? 50) + '%');
  const y = $derived((photo.frame?.y ?? 50) + '%');
</script>

<span class="frame" class:crop={!!crop.ratio} class:square={photo.frame?.corners === 'square'}
  class:extra-rounded={photo.frame?.corners === 'extra-rounded'}
  data-silhouette={'silhouette' in crop ? crop.silhouette : undefined}
  data-blob-profile={crop.id === 'blob' ? blobProfiles.find(({ id }) => id === photo.frame?.blobProfile)?.id : undefined}
  class:blob-reversed={crop.id === 'blob' && photo.frame?.blobDirection === 'right'}
  style:aspect-ratio={crop.ratio}
  style:--focus-x={x} style:--focus-y={y} style:--zoom={photo.frame?.zoom ?? 1}>
  <img data-photo-frame draggable="false" {src} alt={photo.alt} loading="lazy" referrerpolicy="no-referrer" />
  {#if photo.caption && photo.captionPlacement === 'overlay'}
    <span class="caption-overlay">{photo.caption}</span>
  {/if}
</span>

<style>
  .frame { position: relative; display: block; overflow: hidden; border-radius: var(--atlas-radius, .5rem); }
  .caption-overlay {
    position: absolute; inset: auto 0 0; padding: .2rem .4rem;
    background: rgb(0 0 0 / 65%); color: white; text-align: center;
    font-size: .85rem; overflow-wrap: anywhere; pointer-events: none;
  }
  .square { border-radius: 0; }
  .extra-rounded { border-radius: 1.5rem; }
  .frame[data-silhouette="circle"],
  .frame[data-silhouette="pill"] { border-radius: 9999px; }
  .frame[data-silhouette="blob"] {
    --blob-tl: 59% 43%; --blob-tr: 41% 56%; --blob-br: 53% 44%; --blob-bl: 47% 57%;
    border-top-left-radius: var(--blob-tl);
    border-top-right-radius: var(--blob-tr);
    border-bottom-right-radius: var(--blob-br);
    border-bottom-left-radius: var(--blob-bl);
  }
  .frame[data-blob-profile="soft"] {
    --blob-tl: 53% 47%; --blob-tr: 47% 52%; --blob-br: 48% 48%; --blob-bl: 52% 53%;
  }
  .frame[data-blob-profile="swept"] {
    --blob-tl: 42% 58%; --blob-tr: 58% 36%; --blob-br: 39% 64%; --blob-bl: 61% 42%;
  }
  .frame[data-silhouette="blob"].blob-reversed {
    border-top-left-radius: var(--blob-tr);
    border-top-right-radius: var(--blob-tl);
    border-bottom-right-radius: var(--blob-bl);
    border-bottom-left-radius: var(--blob-br);
  }
  .frame[data-silhouette] .caption-overlay { inset: auto 16% 15%; border-radius: var(--atlas-radius, .5rem); }
  img { display: block; width: 100%; height: auto; }
  .crop img { height: 100%; object-fit: cover; object-position: var(--focus-x) var(--focus-y);
    transform: scale(var(--zoom)); transform-origin: var(--focus-x) var(--focus-y); }
</style>

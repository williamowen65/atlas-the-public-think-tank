<script lang="ts">
  import type { Photo } from './sectionLayouts';
  let { photo, src, gallery = false }: {
    photo: Photo; src: string; gallery?: boolean;
  } = $props();
  const ratio = $derived(photo.frame?.ratio ?? (gallery ? '1 / 1' : 'natural'));
  const x = $derived((photo.frame?.x ?? 50) + '%');
  const y = $derived((photo.frame?.y ?? 50) + '%');
</script>

<span class="frame" class:crop={ratio !== 'natural'} class:square={photo.frame?.corners === 'square'}
  style:aspect-ratio={ratio === 'natural' ? undefined : ratio}
  style:--focus-x={x} style:--focus-y={y} style:--zoom={photo.frame?.zoom ?? 1}>
  <img {src} alt={photo.alt} loading="lazy" referrerpolicy="no-referrer" />
</span>

<style>
  .frame { position: relative; display: block; overflow: hidden; border-radius: var(--atlas-radius, .5rem); }
  .square { border-radius: 0; }
  img { display: block; width: 100%; height: auto; }
  .crop img { height: 100%; object-fit: cover; object-position: var(--focus-x) var(--focus-y);
    transform: scale(var(--zoom)); transform-origin: var(--focus-x) var(--focus-y); }
</style>

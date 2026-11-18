<script lang="ts" module>
  export interface GalleryMedia {
    kind: 'image' | 'video';
    src: string;
    alt: string;
    caption: string;
  }
</script>

<script lang="ts">
  import { tick } from 'svelte';

  let { items }: { items: GalleryMedia[] } = $props();
  let dialog = $state<HTMLDialogElement>();
  let index = $state(0);
  let opened = $state(false);
  let trigger: HTMLButtonElement | undefined;
  const item = $derived(items[index]);

  export async function open(position: number, opener: HTMLButtonElement) {
    if (!items[position]) return;
    index = position;
    trigger = opener;
    opened = true;
    await tick();
    if (!dialog?.open) dialog?.showModal();
  }

  function move(direction: number) {
    index = (index + direction + items.length) % items.length;
  }
</script>

<dialog bind:this={dialog} aria-label="Node media gallery"
  onclose={() => {
    opened = false;
    trigger?.focus({ preventScroll: true });
  }}
  onclick={(event) => { if (event.target === dialog) dialog?.close(); }}
  onkeydown={(event) => {
    if (items.length < 2) return;
    if (event.key === 'ArrowLeft' || event.key === 'ArrowRight') {
      event.preventDefault();
      move(event.key === 'ArrowLeft' ? -1 : 1);
    }
  }}>
  <div class="gallery">
    <header>
      <span aria-live="polite">{index + 1} of {items.length}</span>
      <button type="button" aria-label="Close media gallery" onclick={() => dialog?.close()}>×</button>
    </header>
    {#if opened && item}
      <figure>
        {#if item.kind === 'image'}
          <img src={item.src} alt={item.alt} referrerpolicy="no-referrer" />
        {:else}
          {#if /\.(mp4|webm|ogv)$/i.test(new URL(item.src).pathname)}
            <!-- svelte-ignore a11y_media_has_caption (Node video blocks only supply a URL and description.) -->
            <video src={item.src} controls playsinline preload="none" aria-label={item.caption || 'Node video'}></video>
          {/if}
          <a href={item.src} target="_blank" rel="noopener noreferrer">Open video</a>
        {/if}
        {#if item.caption}<figcaption>{item.caption}</figcaption>{/if}
      </figure>
    {/if}
    {#if items.length > 1}
      <nav aria-label="Gallery slides">
        <button type="button" onclick={() => move(-1)}>Previous item</button>
        <button type="button" onclick={() => move(1)}>Next item</button>
      </nav>
    {/if}
  </div>
</dialog>

<style>
  dialog {
    width: min(56rem, 94vw);
    max-height: 94dvh;
    padding: 0;
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    background: var(--atlas-surface);
    color: var(--atlas-text);
  }
  dialog::backdrop { background: rgb(0 0 0 / 0.78); }
  .gallery { display: grid; gap: .65rem; padding: .8rem; }
  header, nav { display: flex; gap: .6rem; align-items: center; justify-content: space-between; }
  figure { margin: 0; text-align: center; }
  img, video { display: block; margin-inline: auto; max-width: 100%; max-height: 72dvh; object-fit: contain; }
  figcaption { margin-top: .45rem; }
  button {
    padding: .35rem .7rem;
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    background: var(--atlas-surface);
    color: var(--atlas-text);
    font: inherit;
    cursor: pointer;
  }
</style>

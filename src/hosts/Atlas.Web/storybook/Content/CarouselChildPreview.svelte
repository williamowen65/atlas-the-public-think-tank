<!-- One typed child-block renderer; carousel parent owns navigation/order/frame. -->
<script lang="ts">
  import { publicUrl } from '../../src/lib/api/nodePage';
  import type { CarouselChild } from './carouselPrototype';

  let {
    item,
    mode = 'edit',
    pollChoice = null,
    onPollChoice,
  }: {
    item: CarouselChild;
    mode?: 'edit' | 'full' | 'feed';
    pollChoice?: number | null;
    onPollChoice?: (key: string, index: number) => void;
  } = $props();
</script>

{#if item.type === 'image'}
  {#if publicUrl(item.url)}
    <img class="carousel-media" src={publicUrl(item.url)}
      alt={item.alt || 'Gallery image; alternative text needed'} loading="lazy"/>
  {:else}
    <div class="placeholder">
      <span>Image child block</span>
      <span>Enter an image URL in the editor below.</span>
    </div>
  {/if}
{:else if item.type === 'video'}
  {#if publicUrl(item.url)}
    <video class="carousel-media" controls playsinline preload="metadata"
      poster={publicUrl(item.poster)} aria-label={item.alt || 'Gallery video'}
      src={publicUrl(item.url)}>
      Your browser does not support this video.
    </video>
  {:else}
    <div class="placeholder">
      <span>Video child block</span>
      <span>Enter a video URL in the editor below.</span>
    </div>
  {/if}
{:else if item.type === 'poll'}
  <div class="poll-child" class:compact={mode === 'feed'} aria-label="Demo poll child block">
    <span class="poll-label">Poll block · demo only</span>
    <h3>{item.question || 'What do you think?'}</h3>
    <div class="poll-choices" role="group" aria-label="Demo poll choices">
      {#each item.options ?? [] as option, i}
        <button type="button" class:chosen={pollChoice === i}
          aria-pressed={pollChoice === i}
          onclick={() => onPollChoice?.(item.key, i)}>
          {option}
        </button>
      {/each}
    </div>
    <p class="poll-note" aria-live="polite">
      {pollChoice === null
        ? 'Choose an option to preview the interaction.'
        : 'Selected: ' + (item.options?.[pollChoice] || '') + '. No vote was submitted.'}
    </p>
  </div>
{/if}

<style>
  .carousel-media {
    display: block; width: 100%; height: 100%; object-fit: contain;
  }
  .placeholder {
    display: grid; gap: .4rem; align-content: center; justify-items: center;
    width: 100%; height: 100%; padding: .8rem; box-sizing: border-box;
    color: #fff; background: #263442; text-align: center; font-size: .8rem;
  }
  .poll-child {
    width: 100%; height: 100%; box-sizing: border-box; overflow: auto;
    display: flex; flex-direction: column; justify-content: center; gap: .65rem;
    padding: clamp(.65rem, 3vw, 1.5rem);
    background: var(--atlas-surface); color: var(--atlas-text);
    text-align: left; touch-action: pan-y;
  }
  .poll-child h3 { margin: 0; font-size: clamp(.95rem, 2.3vw, 1.22rem); line-height: 1.25; }
  .poll-label { color: var(--atlas-muted); font-size: .73rem; font-weight: 600; }
  .poll-choices { display: grid; gap: .42rem; }
  .poll-choices button {
    display: block; width: 100%; padding: .48rem .65rem; text-align: left;
    border: 1px solid var(--atlas-border); border-radius: .45rem;
    background: var(--atlas-surface); color: var(--atlas-text);
    font: inherit; font-size: .83rem; line-height: 1.3; cursor: pointer;
  }
  .poll-choices button.chosen {
    border-color: var(--atlas-accent);
    background: color-mix(in srgb, var(--atlas-accent) 8%, var(--atlas-surface));
  }
  .poll-note { margin: 0; color: var(--atlas-muted); font-size: .73rem; }
  .poll-child.compact { gap: .25rem; padding: .35rem 3.1rem; }
  .poll-child.compact h3 { font-size: .87rem; }
  .poll-child.compact .poll-choices { gap: .18rem; }
  .poll-child.compact .poll-choices button { padding: .16rem .4rem; font-size: .7rem; }
  .poll-child.compact .poll-label { font-size: .63rem; }
  .poll-child.compact .poll-note { font-size: .65rem; }
  button:focus-visible { outline: 2px solid var(--atlas-focus); outline-offset: 2px; }
</style>

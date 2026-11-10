<!--
  Storybook-only media gallery experiment. This is NOT a persisted Atlas
  content-block schema or a production media uploader/player.
-->
<script lang="ts">
  import { publicUrl } from '../../src/lib/api/nodePage';

  type GalleryItem = {
    key: string;
    type: 'image' | 'video';
    url: string;
    alt: string;
    caption: string;
    poster?: string;
  };
  type GalleryFrame = 'portrait' | 'landscape';

  let {
    items,
    frame,
    index,
    mode = 'edit',
    onChange,
    onFrameChange,
    onIndexChange,
  }: {
    items: GalleryItem[];
    frame: GalleryFrame;
    index: number;
    mode?: 'edit' | 'full' | 'feed';
    onChange?: (items: GalleryItem[]) => void;
    onFrameChange?: (frame: GalleryFrame) => void;
    onIndexChange?: (index: number) => void;
  } = $props();

  let touchStartX: number | null = null;
  let current = $derived(items[Math.max(0, Math.min(items.length - 1, index))]);

  function go(next: number) {
    if (!items.length) return;
    onIndexChange?.((next + items.length) % items.length);
  }

  function updateCurrent(changes: Partial<GalleryItem>) {
    if (!current) return;
    onChange?.(items.map(item => item.key === current.key ? { ...item, ...changes } : item));
  }

  function add(type: 'image' | 'video') {
    const key = crypto.randomUUID();
    onChange?.([...items, { key, type, url: '', alt: '', caption: '', poster: '' }]);
    onIndexChange?.(items.length);
  }

  function move(direction: -1 | 1) {
    const next = index + direction;
    if (next < 0 || next >= items.length) return;
    const reordered = [...items];
    [reordered[index], reordered[next]] = [reordered[next], reordered[index]];
    onChange?.(reordered);
    onIndexChange?.(next);
  }

  function removeCurrent() {
    if (!current) return;
    const remaining = items.filter(item => item.key !== current.key);
    onChange?.(remaining);
    onIndexChange?.(Math.max(0, Math.min(index, remaining.length - 1)));
  }

  function handleTouchEnd(event: TouchEvent) {
    if (touchStartX === null || !event.changedTouches.length) return;
    const delta = event.changedTouches[0].clientX - touchStartX;
    touchStartX = null;
    if (Math.abs(delta) >= 45) go(index + (delta < 0 ? 1 : -1));
  }
</script>

<section class="gallery-demo" class:feed={mode === 'feed'} aria-label="Experimental mixed-media gallery">
  <div class="gallery-heading">
    <div>
      <strong>Gallery block</strong>
      <span class="research-label">Storybook concept · not yet an Atlas block type</span>
    </div>
    {#if mode === 'edit'}
      <label class="frame-setting">
        Frame
        <select aria-label="Gallery frame" value={frame}
          onchange={(event) => onFrameChange?.(event.currentTarget.value as GalleryFrame)}>
          <option value="portrait">Portrait 4:5</option>
          <option value="landscape">Landscape 16:9</option>
        </select>
      </label>
    {/if}
  </div>

  <div class="gallery-viewport" class:portrait={frame === 'portrait'} class:landscape={frame === 'landscape'}
    role="region" aria-roledescription="carousel" aria-label="Gallery media, swipe or use arrows"
    tabindex="0"
    onkeydown={(event) => {
      if (event.key === 'ArrowLeft') { event.preventDefault(); go(index - 1); }
      if (event.key === 'ArrowRight') { event.preventDefault(); go(index + 1); }
    }}
    ontouchstart={(event) => { touchStartX = event.touches[0]?.clientX ?? null; }}
    ontouchend={handleTouchEnd}
    ontouchcancel={() => { touchStartX = null; }}>
    {#if current}
      {#key current.key}
        <div class="slide" role="group" aria-roledescription="slide"
          aria-label={'Slide ' + (index + 1) + ' of ' + items.length}>
          {#if current.type === 'image'}
            {#if publicUrl(current.url)}
              <img src={publicUrl(current.url)} alt={current.alt || 'Gallery image; alternative text needed'}
                loading={index === 0 ? 'eager' : 'lazy'} />
            {:else}
              <div class="empty-slide">
                <span>Image #{index + 1}</span>
                <span>Enter an image URL in the editor below.</span>
              </div>
            {/if}
          {:else if publicUrl(current.url)}
            <video controls playsinline preload="metadata" poster={publicUrl(current.poster)}
              aria-label={current.alt || 'Gallery video'} src={publicUrl(current.url)}>
              Your browser does not support this video.
            </video>
          {:else}
            <div class="empty-slide">
              <span>Video #{index + 1}</span>
              <span>Enter a video URL in the editor below.</span>
            </div>
          {/if}
        </div>
      {/key}
    {:else}
      <div class="empty-slide">No media yet. Add an image or video.</div>
    {/if}
    {#if items.length > 1}
      <button type="button" class="nav previous" aria-label="Previous gallery item"
        onclick={() => go(index - 1)}>‹</button>
      <button type="button" class="nav next" aria-label="Next gallery item"
        onclick={() => go(index + 1)}>›</button>
      <div class="count">{index + 1} / {items.length}</div>
    {/if}
  </div>

  <div class="gallery-footer">
    <div class="caption">
      {#if current?.caption}
        <span>{current.caption}</span>
      {:else if current}
        <span>{current.type === 'video' ? 'Video' : 'Image'} {index + 1}</span>
      {/if}
    </div>
    {#if items.length > 1}
      <div class="pagination" role="group" aria-label="Gallery pages">
        {#each items as item, i (item.key)}
          <button type="button" class:active={i === index}
            aria-label={'Go to gallery item ' + (i + 1)}
            aria-current={i === index ? 'true' : undefined}
            onclick={() => go(i)}></button>
        {/each}
      </div>
    {/if}
  </div>
  {#if items.length > 1}
    <p class="swipe-hint">Swipe left or right on touch screens, or use the arrows to browse all {items.length} items.</p>
  {/if}

  {#if mode === 'edit'}
    <div class="gallery-editor">
      <div class="editor-actions">
        <span>Media in this block: {items.length}</span>
        <button type="button" onclick={() => add('image')}>Add image to gallery</button>
        <button type="button" onclick={() => add('video')}>Add video to gallery</button>
      </div>
      {#if current}
        <div class="slide-editor">
          <h3>Edit {current.type} {index + 1}</h3>
          <div class="move-actions">
            <button type="button" disabled={index === 0} onclick={() => move(-1)}>Move slide left</button>
            <button type="button" disabled={index === items.length - 1} onclick={() => move(1)}>Move slide right</button>
            <button type="button" onclick={removeCurrent}>Remove slide</button>
          </div>
          <label>
            {current.type === 'image' ? 'Image URL' : 'Video URL'}
            <input type="url" value={current.url} placeholder="https://example.org/media"
              oninput={(event) => updateCurrent({ url: event.currentTarget.value })} />
          </label>
          <label>
            {current.type === 'image' ? 'Alternative text' : 'Video description'}
            <input value={current.alt} placeholder="Describe this media accessibly"
              oninput={(event) => updateCurrent({ alt: event.currentTarget.value })} />
          </label>
          {#if current.type === 'video'}
            <label>
              Poster image URL (optional)
              <input type="url" value={current.poster || ''} placeholder="https://example.org/poster.jpg"
                oninput={(event) => updateCurrent({ poster: event.currentTarget.value })} />
            </label>
          {/if}
          <label>
            Caption
            <input value={current.caption} placeholder="Optional description shown under the media"
              oninput={(event) => updateCurrent({ caption: event.currentTarget.value })} />
          </label>
        </div>
      {/if}
    </div>
  {/if}
</section>

<style>
  .gallery-demo { width: 100%; min-width: 0; margin: 1.25rem 0; color: var(--atlas-text); }
  .gallery-heading { display: flex; align-items: center; justify-content: space-between; gap: .75rem; flex-wrap: wrap; margin-bottom: .65rem; }
  .gallery-heading > div { display: grid; gap: .1rem; }
  .gallery-heading strong { font-size: .96rem; }
  .research-label { font-size: .72rem; color: var(--atlas-muted); }
  .frame-setting { display: flex; gap: .4rem; align-items: center; font-size: .8rem; }
  .frame-setting select { border: 1px solid var(--atlas-border); border-radius: .4rem; padding: .3rem; background: var(--atlas-surface); color: var(--atlas-text); }
  .gallery-viewport {
    position: relative; width: 100%; max-width: 100%; overflow: hidden;
    background: #17212c; border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius); box-sizing: border-box;
    touch-action: pan-y;
  }
  .gallery-viewport.portrait { aspect-ratio: 4 / 5; max-width: 24rem; }
  .gallery-viewport.landscape { aspect-ratio: 16 / 9; max-width: 100%; }
  .gallery-demo.feed .gallery-viewport { aspect-ratio: 16 / 9; max-width: 100%; max-height: 11rem; }
  .slide { width: 100%; height: 100%; display: flex; align-items: center; justify-content: center; }
  .slide img, .slide video { display: block; width: 100%; height: 100%; object-fit: contain; }
  /* Feed is intentionally an editorial crop; Edit and Full Node never crop. */
  .gallery-demo.feed .slide img, .gallery-demo.feed .slide video { object-fit: cover; }
  .empty-slide { width: 100%; height: 100%; display: grid; align-content: center; justify-items: center; gap: .4rem; padding: 1rem; color: white; background: #263442; text-align: center; box-sizing: border-box; }
  .nav {
    position: absolute; top: 50%; transform: translateY(-50%);
    display: grid; place-items: center; width: 2.1rem; height: 2.6rem;
    padding: 0; border: 0; border-radius: .5rem;
    background: #111c26dd; color: #fff; font: 2rem/1 system-ui, sans-serif;
    cursor: pointer;
  }
  .previous { left: .45rem; } .next { right: .45rem; }
  .count { position: absolute; right: .45rem; bottom: .45rem; padding: .2rem .45rem; border-radius: .3rem; background: #111c26d8; color: white; font-size: .74rem; }
  .gallery-footer { display: flex; align-items: center; justify-content: space-between; gap: 1rem; padding-top: .5rem; }
  .caption { font-size: .88rem; color: var(--atlas-muted); line-height: 1.45; }
  .pagination { display: flex; gap: .35rem; align-items: center; }
  .pagination button { width: .63rem; height: .63rem; padding: 0; border: 1px solid var(--atlas-muted); border-radius: 50%; background: transparent; cursor: pointer; }
  .pagination button.active { background: var(--atlas-accent); border-color: var(--atlas-accent); }
  .swipe-hint { margin: .45rem 0; font-size: .75rem; color: var(--atlas-muted); }
  .gallery-editor { display: grid; gap: .65rem; margin-top: .8rem; border-top: 1px dashed var(--atlas-border); padding-top: .8rem; }
  .editor-actions, .move-actions { display: flex; flex-wrap: wrap; align-items: center; gap: .4rem; }
  .editor-actions span { font-size: .8rem; color: var(--atlas-muted); margin-right: auto; }
  .gallery-editor button { border: 1px solid var(--atlas-border); background: var(--atlas-surface); color: var(--atlas-text); border-radius: .4rem; font: inherit; font-size: .82rem; padding: .35rem .55rem; cursor: pointer; }
  .gallery-editor button:disabled { opacity: .4; cursor: default; }
  .slide-editor { display: grid; gap: .5rem; padding: .75rem; border: 1px solid var(--atlas-border); border-radius: .6rem; }
  .slide-editor h3 { margin: 0; font-size: .88rem; }
  .slide-editor label { display: grid; gap: .2rem; font-size: .82rem; }
  .slide-editor input { min-width: 0; box-sizing: border-box; width: 100%; border: 1px solid var(--atlas-border); border-radius: .4rem; padding: .5rem; font: inherit; background: var(--atlas-surface); color: var(--atlas-text); }
  button:focus-visible, select:focus-visible, input:focus-visible, .gallery-viewport:focus-visible { outline: 2px solid var(--atlas-focus); outline-offset: 2px; }
</style>

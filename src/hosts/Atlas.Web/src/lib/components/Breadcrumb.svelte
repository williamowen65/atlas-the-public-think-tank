<script lang="ts" module>
  /** Oldest ancestor first; omit id when the source supplies display text only. */
  export interface BreadcrumbItem { id?: string; title: string }
  export interface BreadcrumbPage { items: BreadcrumbItem[]; hasMore: boolean }
  export type LoadOlderAncestors = (oldestId?: string) => Promise<BreadcrumbPage>;
</script>

<script lang="ts">
  let { items, onNavigate, hasMore = false, loadOlder }: {
    items: BreadcrumbItem[]; onNavigate?: (id: string) => void;
    hasMore?: boolean; loadOlder?: LoadOlderAncestors;
  } = $props();
  let older = $state<BreadcrumbItem[]>([]);
  let more = $state(false);
  let loading = $state(false);
  let error = $state('');
  let generation = 0;
  const allItems = $derived([...older, ...items]);
  $effect(() => { items; hasMore; loadOlder; generation++; older = []; more = hasMore; loading = false; error = ''; });

  async function requestOlder() {
    if (!more || loading || !loadOlder) return;
    const current = generation;
    loading = true;
    error = '';
    try {
      const page = await loadOlder(allItems[0]?.id);
      if (current !== generation) return;
      const seen = new Set(allItems.flatMap(item => item.id ? [item.id] : []));
      // A repeated ancestor means this route has cycled. Stop instead of looping requests.
      const fresh = page.items.filter(item => !item.id || !seen.has(item.id));
      older = [...fresh, ...older];
      more = page.hasMore && fresh.length === page.items.length && fresh.length > 0;
    } catch (cause) {
      if (current === generation) error = cause instanceof Error ? cause.message : 'Could not load older ancestors.';
    } finally {
      if (current === generation) loading = false;
    }
  }

  // Measure after insertion; a detached element has no usable scrollWidth.
  function revealNewest(viewport: HTMLElement, path: BreadcrumbItem[]) {
    let previous = path;
    let width = viewport.scrollWidth;
    let pinned = true;
    const scrollToEnd = () => { viewport.scrollLeft = viewport.scrollWidth; width = viewport.scrollWidth; };
    scrollToEnd();
    viewport.dataset.ready = 'true';
    const observer = new ResizeObserver(() => { if (pinned) scrollToEnd(); });
    observer.observe(viewport);
    if (viewport.firstElementChild) observer.observe(viewport.firstElementChild);
    const onScroll = () => {
      pinned = viewport.scrollWidth - viewport.clientWidth - viewport.scrollLeft <= 1;
      if (viewport.scrollLeft <= 8 && !pinned && !error) void requestOlder();
    };
    viewport.addEventListener('scroll', onScroll);
    return {
      update(next: BreadcrumbItem[]) {
        const prepended = next.length > previous.length && previous.every((item, i) =>
          JSON.stringify(item) === JSON.stringify(next[next.length - previous.length + i]));
        if (prepended) {
          pinned = false;
          viewport.scrollLeft += viewport.scrollWidth - width;
          width = viewport.scrollWidth;
        } else { pinned = true; scrollToEnd(); }
        previous = next;
      },
      destroy() { observer.disconnect(); viewport.removeEventListener('scroll', onScroll); },
    };
  }

</script>

{#if allItems.length || more}
  <nav aria-label="Node breadcrumb">
    <!-- svelte-ignore a11y_no_noninteractive_tabindex (Native scrolling needs keyboard focus for display-only ancestor labels.) -->
    <div class="viewport" role="group" aria-label="Scrollable ancestor path" tabindex="0" use:revealNewest={allItems}>
      <ol>
        {#if more && loadOlder}
          <li class="load-older"><button type="button" disabled={loading} onclick={requestOlder}>{loading ? 'Loading…' : 'Load older ancestors'}</button></li>
        {/if}
        {#each allItems as item, index}
          <li style:z-index={allItems.length - index}>
            {#if item.id && onNavigate}
              <button type="button" onclick={() => onNavigate?.(item.id!)}>{item.title}</button>
            {:else}
              <span>{item.title}</span>
            {/if}
          </li>
        {/each}
      </ol>
    </div>
    {#if error}<p role="alert">{error} <button type="button" onclick={requestOlder}>Retry</button></p>{/if}
  </nav>
{/if}

<style lang="scss">
  nav {
    min-width: 0;
    max-width: 100%;
  }

  .viewport {
    max-width: 100%;
    overflow-x: auto;
    overscroll-behavior-x: contain;
    visibility: hidden;
    scrollbar-width: none;
    &::-webkit-scrollbar { display: none; }
    &:global([data-ready='true']) {
      visibility: visible;
    }
  }

  ol {
    display: flex;
    width: max-content;
    list-style: none;
    margin: 0;
    padding: .3rem 1.15rem .5rem 0;
  }

  li {
    --crumb-color: #264653;
    position: relative;
    flex: none;
    background: var(--crumb-color);
    color: #fff;
    &:nth-child(4n + 2) { --crumb-color: #06584e; }
    &:nth-child(4n + 3) { --crumb-color: #2d3142; }
    &:nth-child(4n) { --crumb-color: #4f5d75; }
    &::after {
      content: '';
      position: absolute;
      top: 0;
      left: 100%;
      width: 1rem;
      height: 100%;
      background: var(--crumb-color);
      clip-path: polygon(0 0, 100% 50%, 0 100%);
      pointer-events: none;
    }
    &:first-child {
      border-radius: .2rem 0 0 .2rem;
      button, span { padding-inline-start: .75rem; }
    }
  }

  .load-older button {
    min-width: 13rem;
  }

  button, span {
    display: block;
    border: 0;
    padding: .55rem .7rem .55rem 1.65rem;
    font: inherit;
    font-size: .85rem;
    font-weight: 600;
    line-height: 1.4;
    white-space: nowrap;
    color: inherit;
    background: transparent;
  }

  button {
    cursor: pointer;
    &:hover { text-decoration: underline; }
    &:focus-visible {
      outline: 2px solid white;
      outline-offset: -4px;
      text-decoration: underline;
    }
  }
</style>

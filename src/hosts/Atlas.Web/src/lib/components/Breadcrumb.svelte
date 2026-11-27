<!--
  @component
  Breadcrumb: Scrollable ancestry ribbon with older-page loading and alternate-parent paths.
  Keeps scroll position while prepending ancestors or switching paths; async lookups do not own app routing.

  **Props**
  - `items` (`BreadcrumbItem[]`; required): Ancestor labels in oldest-to-newest order; IDs enable
    navigation when a handler is supplied.
  - `onNavigate` (`(id: string) => void`; optional): Callback: Receives the selected node ID when an
    ancestor is activated. The parent owns navigation.
  - `hasMore` (`boolean`; optional; default `false`): Whether older ancestors remain available to
    load.
  - `loadOlder` (`LoadOlderAncestors`; optional): Async ancestor loader; receives the oldest
    displayed ID and returns a BreadcrumbPage.
  - `originId` (`string`; optional): Current node ID used to look up alternative parents for the
    final breadcrumb segment.
  - `apiBase` (`string`; optional; default `'/atlas-public-api'`): Base URL for this component’s API
    reads. This prop alone does not configure a proxy.
-->
<script lang="ts" module>
  export type { BreadcrumbItem, BreadcrumbPage, LoadOlderAncestors } from './breadcrumb.types';
</script>

<script lang="ts">
  import { untrack } from 'svelte';
  import AncestorNodeCard from './AncestorNodeCard.svelte';
  import type { BreadcrumbItem, LoadOlderAncestors } from './breadcrumb.types';
  let {
    items,
    expandable = false,
    connectedCards = false,
    onNavigate,
    hasMore = false,
    loadOlder,
    originId,
    apiBase = '/atlas-public-api',
  }: {
    /**
     * Ancestor labels in oldest-to-newest order; IDs enable navigation when a handler is supplied.
     */
    items: BreadcrumbItem[];
    /** Show the immediate parent with an optional ancestry toggle. */
    expandable?: boolean;
    /** Standalone, collapsed-by-default ancestry of compact cards. */
    connectedCards?: boolean;
    /**
     * Callback: Receives the selected node ID when an ancestor is activated. The parent owns
     * navigation.
     */
    onNavigate?: (id: string) => void;
    /** Whether older ancestors remain available to load. */
    hasMore?: boolean;
    /** Async ancestor loader; receives the oldest displayed ID and returns a BreadcrumbPage. */
    loadOlder?: LoadOlderAncestors;
    /** Current node ID used to look up alternative parents for the final breadcrumb segment. */
    originId?: string;
    /** Base URL for this component’s API reads. This prop alone does not configure a proxy. */
    apiBase?: string;
  } = $props();
  /**
   * Older ancestor items prepended by the current path’s pager.
   */
  const panelId = $props.id();
  let expanded = $state(false);
  let older = $state<BreadcrumbItem[]>([]);
  /**
   * Whether the alternative-parent menu is visible.
   */
  let openRouteMenu = $state(false);
  /**
   * Viewport coordinates placing the floating menu beside its trigger.
   */
  let menuPosition = $state({ top: 0, left: 0 });
  let preserveScroll = false;
  let pendingAnchor: { id?: string; offset: number } | null = null;
  /**
   * Mounted scrolling container used for measurement and scroll positioning.
   */
  let breadcrumbViewport = $state<HTMLElement>();
  /**
   * Mounted ordered list used to measure ancestor positions.
   */
  let breadcrumbList = $state<HTMLOListElement>();
  /**
   * Cached alternative parent records keyed by child node ID.
   */
  let liveChoices = $state<Record<string, { id: string; title: string }[]>>({});
  /**
   * API-resolved alternative ancestry path, or null to use supplied/paged items.
   */
  let livePath = $state<BreadcrumbItem[] | null>(null);
  /**
   * Child node whose API-loaded alternative parents populate the menu, or null when none is selected.
   */
  let liveMenuChild = $state<string | null>(null);
  /**
   * Whether an alternative-parent lookup is underway; locks menu choices.
   */
  let liveBusy = $state(false);
  function toggleRouteMenu(event: MouseEvent) {
    if (openRouteMenu) {
      openRouteMenu = false;
      return;
    }
    const trigger = event.currentTarget instanceof HTMLElement ? event.currentTarget : null;
    const segment = trigger?.closest('li');
    const rect = segment?.getBoundingClientRect();
    if (rect) {
      // Align the menu's right edge with the chevron-side edge of the full breadcrumb segment.
      // The menu grows leftwards beneath the segment rather than projecting into the next crumb.
      const menuWidth = Math.min(320, window.innerWidth - 16);
      menuPosition = {
        top: rect.bottom + 2,
        left: Math.max(8, Math.min(rect.right - menuWidth, window.innerWidth - menuWidth - 8)),
      };
    }
    openRouteMenu = true;
  }
  // Remember the visible anchor before fetching a different parent path, so the ribbon does not jump.
  async function selectLiveParent(parentId: string) {
    if (!liveMenuChild || liveBusy || loading) return;
    const child = liveMenuChild;
    const current = generation;
    const currentIndex = allItems.findIndex((_, index) => liveChildAt(index) === child);
    if (currentIndex < 0) return;
    const suffix = allItems.slice(currentIndex + 1);
    liveBusy = true;
    error = '';
    try {
      const response = await fetch(
        `${apiBase}/nodes/${encodeURIComponent(child)}/ancestors?firstParent=${encodeURIComponent(parentId)}`,
        { credentials: apiBase === '/atlas-session-api' ? 'same-origin' : 'omit' },
      );
      if (!response.ok) throw new Error('Could not load the selected parent route.');
      const page = (await response.json()) as {
        items: { id: string; title: string }[];
        hasMore: boolean;
      };
      if (!Array.isArray(page.items)) throw new Error('Invalid ancestor response.');
      const newAncestors = [...page.items].reverse();
      if (current !== generation) return;
      const anchor = breadcrumbList?.children.item(currentIndex) as HTMLElement | null;
      pendingAnchor =
        anchor && breadcrumbViewport
          ? {
              id: parentId,
              offset:
                anchor.getBoundingClientRect().left -
                breadcrumbViewport.getBoundingClientRect().left,
            }
          : null;
      preserveScroll = !expandable;
      livePath = [...newAncestors, ...suffix];
      more = page.hasMore;
      openRouteMenu = false;
      liveMenuChild = null;
    } catch (cause) {
      if (current === generation)
        error = cause instanceof Error ? cause.message : 'Could not change parent path.';
    } finally {
      if (current === generation) liveBusy = false;
    }
  }

  /**
   * Whether the current ancestry path has older items remaining.
   */
  let more = $state(false);
  /**
   * Whether an older-ancestor page is loading.
   */
  let loading = $state(false);
  /**
   * User-facing load or request failure; empty text hides the error message.
   */
  let error = $state('');
  let generation = 0;
  /** Displayed path: an API-selected alternative, or older items followed by supplied items. */
  const allItems = $derived(livePath ?? [...older, ...items]);
  $effect(() => {
    originId;
    expanded = false;
  });
  const liveChildAt = (index: number) =>
    index + 1 < allItems.length ? allItems[index + 1].id : originId;
  // A new source path resets paging and API-selected alternate ancestry.
  $effect(() => {
    items;
    hasMore;
    loadOlder;
    originId;
    generation++;
    older = [];
    livePath = null;
    liveChoices = {};
    openRouteMenu = false;
    liveMenuChild = null;
    liveBusy = false;
    more = hasMore;
    loading = false;
    error = '';
  });
  $effect(() => {
    if (!originId || !allItems.length || (connectedCards && !expanded)) return;
    const controller = new AbortController();
    const ids =
      expandable && !expanded
        ? [originId]
        : [...allItems.map((item) => item.id), originId].filter((id): id is string => !!id);
    void Promise.all(
      ids.map(async (id) => {
        try {
          const response = await fetch(`${apiBase}/nodes/${encodeURIComponent(id)}/parents`, {
            signal: controller.signal,
            credentials: apiBase === '/atlas-session-api' ? 'same-origin' : 'omit',
          });
          if (!response.ok) return;
          const parents = (await response.json()) as { id: string; title: string }[];
          if (!controller.signal.aborted && Array.isArray(parents))
            liveChoices = { ...liveChoices, [id]: parents };
        } catch {
          /* An unavailable ancestor simply has no switcher. */
        }
      }),
    );
    return () => controller.abort();
  });

  // Prepend only unseen ancestors; generation checks discard results from an outdated path.
  async function requestOlder() {
    if (!more || loading || liveBusy || !loadOlder) return;
    const current = generation;
    loading = true;
    error = '';
    try {
      const page = await loadOlder(allItems[0]?.id);
      if (current !== generation) return;
      const seen = new Set(allItems.flatMap((item) => (item.id ? [item.id] : [])));
      // A repeated ancestor means this route has cycled. Stop instead of looping requests.
      const fresh = page.items.filter((item) => !item.id || !seen.has(item.id));
      if (livePath) livePath = [...fresh, ...livePath];
      else older = [...fresh, ...older];
      more = page.hasMore && fresh.length === page.items.length && fresh.length > 0;
    } catch (cause) {
      if (current === generation)
        error = cause instanceof Error ? cause.message : 'Could not load older ancestors.';
    } finally {
      if (current === generation) loading = false;
    }
  }

  $effect(() => {
    if (expandable && !connectedCards && expanded && allItems.length && more && !loading && !liveBusy && !error)
      untrack(() => void requestOlder());
  });

  // Measure after insertion; a detached element has no usable scrollWidth.
  function revealNewest(viewport: HTMLElement, path: BreadcrumbItem[]) {
    let previous = path;
    let width = viewport.scrollWidth;
    let pinned = true;
    const scrollToEnd = () => {
      viewport.scrollLeft = viewport.scrollWidth;
      width = viewport.scrollWidth;
    };
    scrollToEnd();
    viewport.dataset.ready = 'true';
    const observer = new ResizeObserver(() => {
      if (pinned) scrollToEnd();
    });
    observer.observe(viewport);
    if (viewport.firstElementChild) observer.observe(viewport.firstElementChild);
    const onScroll = () => {
      pinned = viewport.scrollWidth - viewport.clientWidth - viewport.scrollLeft <= 1;
      if (viewport.scrollLeft <= 8 && !pinned && !error) void requestOlder();
    };
    viewport.addEventListener('scroll', onScroll);
    return {
      update(next: BreadcrumbItem[]) {
        const prepended =
          next.length > previous.length &&
          previous.every(
            (item, i) =>
              JSON.stringify(item) === JSON.stringify(next[next.length - previous.length + i]),
          );
        if (prepended) {
          pinned = false;
          viewport.scrollLeft += viewport.scrollWidth - width;
          width = viewport.scrollWidth;
        } else if (preserveScroll) {
          preserveScroll = false;
          pinned = false;
          if (pendingAnchor) {
            const anchorIndex = next.findIndex((item) => item.id === pendingAnchor?.id);
            const anchor = breadcrumbList?.children.item(anchorIndex) as HTMLElement | null;
            if (anchor)
              viewport.scrollLeft +=
                anchor.getBoundingClientRect().left -
                viewport.getBoundingClientRect().left -
                pendingAnchor.offset;
            pendingAnchor = null;
          }
          width = viewport.scrollWidth;
        } else {
          pinned = true;
          scrollToEnd();
        }
        previous = next;
      },
      destroy() {
        observer.disconnect();
        viewport.removeEventListener('scroll', onScroll);
      },
    };
  }
</script>

<!--
  Layout: A horizontally scrollable ancestor ribbon sits inside navigation; alternative parent
  choices open in a separate floating menu.
-->

<svelte:window
  onclick={(event) => {
    if (
      openRouteMenu &&
      event.target instanceof Element &&
      !event.target.closest('.route-choice') &&
      !event.target.closest('.route-menu')
    )
      openRouteMenu = false;
  }}
  onkeydown={(event) => {
    if (event.key === 'Escape') openRouteMenu = false;
  }}
/>

{#if expandable && connectedCards}
  {#if allItems.length}
    <nav class="ancestry connected-ancestry" aria-label="Node ancestry">
      <div class="ancestry-disclosure">
        <button type="button" aria-expanded={expanded} aria-controls={panelId}
          onclick={() => { expanded = !expanded; if (!expanded) openRouteMenu = false; }}>
          {expanded ? 'Hide ancestry' : 'Show ancestry'}
        </button>
      </div>
      <div id={panelId} hidden={!expanded}>
        {#if expanded}
          {#if more && loadOlder}<div class="older-ancestry">{@render olderButton()}</div>{/if}
          {@render loadError()}
          <ol>{@render ancestorItems()}</ol>
        {/if}
      </div>
    </nav>
  {/if}
{:else if expandable}
  {#if allItems.length}
    <nav class="ancestry" id={panelId} aria-label="Node ancestry">
      {#if expanded && loading}<p role="status">Loading older ancestors…</p>{/if}
      <ol>{@render ancestorItems()}</ol>
      {@render loadError()}
    </nav>
  {/if}
{:else if allItems.length || more}
  <nav aria-label="Node breadcrumb">
    <!-- svelte-ignore a11y_no_noninteractive_tabindex (Native scrolling needs keyboard focus for display-only ancestor labels.) -->
    <div
      bind:this={breadcrumbViewport}
      class="viewport"
      role="group"
      aria-label="Scrollable ancestor path"
      tabindex="0"
      use:revealNewest={allItems}
      onfocusin={(event) => {
        const target = event.target;
        // Prepend anchoring settles first, then reveal a newly focused ancestor.
        if (target instanceof HTMLElement && target !== event.currentTarget) {
          requestAnimationFrame(() => {
            if (target.isConnected) target.scrollIntoView({ block: 'nearest', inline: 'nearest' });
          });
        }
      }}
    >
      <!-- The viewport clips/scrolls this list; its items supply the ribbon segments. -->
      <ol bind:this={breadcrumbList}>
        {#if more && loadOlder}<li class="load-older">{@render olderButton()}</li>{/if}
        {@render ancestorItems()}
      </ol>
    </div>
    <!-- Keep the floating menu outside the scroll viewport so its choices are not clipped. -->
    {#if openRouteMenu}
      <div
        class="route-menu"
        role="group"
        aria-label="Other parent paths"
        style:top={menuPosition.top + 'px'}
        style:left={menuPosition.left + 'px'}
      >
        {@render parentChoices()}
      </div>
    {/if}
    {@render loadError()}
  </nav>
{/if}

{#snippet ancestryToggle()}
  <button
    type="button"
    class="ancestry-toggle"
    aria-label={expanded ? 'Hide ancestry' : 'Show ancestry'}
    aria-expanded={expanded}
    aria-controls={panelId}
    onclick={() => {
      expanded = !expanded;
      if (!expanded) openRouteMenu = false;
    }}
  >
    <svg
      viewBox="0 0 16 16"
      width="16"
      height="16"
      fill="none"
      stroke="currentColor"
      stroke-width="2"
      stroke-linecap="round"
      stroke-linejoin="round"
      aria-hidden="true"
    >
      <path d="m3 10 5-5 5 5" />
    </svg>
  </button>
{/snippet}

{#snippet ancestorItems()}
  {#each allItems as item, index (item.id ?? index)}
    {#if !expandable || expanded || index === allItems.length - 1}
      <li style:z-index={expandable ? undefined : allItems.length - index}>
        {#if expandable && !connectedCards && expanded && index > 0}
          <svg
            class="ancestry-arrow"
            viewBox="0 0 24 24"
            aria-hidden="true"
            fill="none"
            stroke="currentColor"
            stroke-width="2"
            stroke-linecap="round"
            stroke-linejoin="round"
          >
            <path d="M12 1v22" />
          </svg>
        {/if}
        {#if connectedCards && item.id}
          <AncestorNodeCard id={item.id} title={item.title} {apiBase} {onNavigate} />
        {:else}
        <div
          class="ancestor-title"
          class:parent-title={expandable && index === allItems.length - 1}
        >
          {#if expandable && index === allItems.length - 1 && (allItems.length > 1 || (more && loadOlder))}
            {@render ancestryToggle()}
          {/if}
          {#if expandable && item.id}
            {@const id = item.id}
            <a
              href={`/nodes?id=${encodeURIComponent(id)}`}
              onclick={(event) => {
                if (!onNavigate || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey)
                  return;
                event.preventDefault();
                onNavigate(id);
              }}>{item.title}</a
            >
          {:else if item.id && onNavigate}
            <button type="button" onclick={() => onNavigate?.(item.id!)}>{item.title}</button>
          {:else}
            <span>{item.title}</span>
          {/if}
        </div>
        {/if}
        {#if originId && (liveChoices[liveChildAt(index) ?? '']?.length ?? 0) > 1}
          <div class="route-choice">
            <button
              type="button"
              class="route-toggle"
              aria-label="Choose another parent path"
              aria-expanded={openRouteMenu && liveMenuChild === liveChildAt(index)}
              onclick={(event) => {
                if (expandable) {
                  const child = liveChildAt(index) ?? null;
                  openRouteMenu = !openRouteMenu || liveMenuChild !== child;
                  liveMenuChild = child;
                } else {
                  liveMenuChild = liveChildAt(index) ?? null;
                  toggleRouteMenu(event);
                }
              }}
            >
              {#if expandable}Other parents{:else}
                <svg aria-hidden="true" viewBox="0 0 16 16" width="14" height="14"
                  ><path
                    d="m3 6 5 5 5-5"
                    fill="none"
                    stroke="currentColor"
                    stroke-width="2"
                    stroke-linecap="round"
                    stroke-linejoin="round"
                  /></svg
                >
              {/if}
            </button>
            {#if expandable && openRouteMenu && liveMenuChild === liveChildAt(index)}
              <div class="inline-routes route-menu" role="group" aria-label="Other parent paths">
                {@render parentChoices()}
              </div>
            {/if}
          </div>
        {/if}
      </li>
    {/if}
  {/each}
{/snippet}

{#snippet parentChoices()}
  {#if liveMenuChild}
    {#each (liveChoices[liveMenuChild] ?? []).filter((parent) => parent.id !== allItems.find((item, index) => liveChildAt(index) === liveMenuChild)?.id) as parent (parent.id)}
      <button type="button" disabled={liveBusy} onclick={() => void selectLiveParent(parent.id)}
        >{parent.title}</button
      >
    {/each}
  {/if}
{/snippet}

{#snippet olderButton()}
  <button type="button" disabled={loading || liveBusy} onclick={requestOlder}>
    {loading ? 'Loading…' : 'Load older ancestors'}
  </button>
{/snippet}

{#snippet loadError()}
  {#if error}
    <p role="alert">{error}</p>
    {#if more && loadOlder && !openRouteMenu}
      <button type="button" onclick={requestOlder}>Retry</button>
    {/if}
  {/if}
{/snippet}

<style lang="scss">
  .ancestor-title {
    display: contents;
  }

  .ancestry {
    --ancestry-gutter: 2.25rem;
    --ancestry-gap: 0.6rem;
    max-width: none;

    .ancestor-title {
      display: block;
      text-align: center;
    }

    .parent-title:has(> .ancestry-toggle) {
      --toggle-space: calc(var(--ancestry-gutter) + var(--ancestry-gap));
      display: flex;
      align-items: center;

      > a,
      > span {
        flex: 0 0 auto;
        max-inline-size: calc(100% - var(--toggle-space));
        margin-inline: auto;
        text-align: center;
      }

      // Balance the caret for short titles, then yield the space to longer ones.
      &::after {
        content: '';
        flex: 0 1 var(--toggle-space);
      }
    }

    color: var(--atlas-text);

    button:not(.ancestry-toggle),
    span {
      padding: 0.3rem 0.5rem;
      white-space: normal;
      overflow-wrap: anywhere;
      text-align: start;
    }

    button:focus-visible {
      outline-color: var(--atlas-accent);
    }

    background: var(--atlas-tinted-surface);
    border-radius: var(--atlas-radius) var(--atlas-radius) 0 0;
    border-bottom: 1px solid var(--atlas-border);
    margin-inline: calc(-1 * var(--card-padding));
    margin-bottom: 0.5rem;
    padding: 0.65rem var(--card-padding);

    a {
      color: var(--atlas-accent);
      font-size: 0.9rem;
      font-weight: 600;
      text-decoration: none;
      text-decoration-thickness: 1px;
      text-underline-offset: 0.18em;
      overflow-wrap: anywhere;

      &:focus-visible,
      &:active {
        text-decoration-line: underline;
      }

      @media (hover: hover) {
        &:hover {
          text-decoration-line: underline;
        }
      }
    }

    .ancestor-title:not(.parent-title) a {
      font-weight: 500;
    }

    .ancestor-title > span {
      text-align: center;
    }

    .ancestry-arrow {
      display: block;
      inline-size: 1.4rem;
      block-size: 1.4rem;
      margin-block: 0.15rem;
      margin-inline: auto;
      color: color-mix(in srgb, var(--atlas-text) 50%, var(--atlas-border));
    }

    ol {
      display: grid;
      width: auto;
      padding: 0;
      &::after {
        content: '';
        display: block;
        inline-size: 2px;
        block-size: 0.65rem;
        margin-inline: auto;
        margin-block-start: 0.2rem;
        background: color-mix(in srgb, var(--atlas-text) 50%, var(--atlas-border));
      }
    }

    li {
      display: block;
      background: transparent;
      color: inherit;

      // Only the first visible row shares the top corner with the edit control.
      &:first-child {
        padding-inline-end: max(
          0px,
          calc(var(--ancestry-end-padding, var(--card-padding)) - var(--card-padding))
        );
      }

      &::after {
        display: none;
      }
    }

    .route-choice {
      display: block;
      text-align: center;
    }
    .route-toggle {
      color: var(--atlas-accent);
      border: 0;
    }
    .inline-routes {
      position: static;
      width: auto;
      max-width: none;
      margin-block: 0.2rem;
      box-shadow: none;
    }
  }

  .ancestry.connected-ancestry {
    --connection-color: color-mix(in srgb, var(--atlas-text) 50%, var(--atlas-border));
    max-width: 48rem;
    margin: 0 auto;
    padding: 0;
    border: 0;
    background: transparent;
    border-radius: 0;
    .ancestry-disclosure, .older-ancestry {
      display: flex;
      justify-content: center;
      margin-block-end: 0.55rem;
    }
    .ancestry-disclosure button, .older-ancestry button {
      color: var(--atlas-accent);
      border: 1px solid var(--atlas-border);
      border-radius: var(--atlas-radius);
      background: var(--atlas-surface);
      padding: 0.4rem 0.85rem;
    }
    ol { display: block; }
    ol::after { display: none; }
    li, li:first-child { padding: 0; }
    li::after {
      display: block;
      position: static;
      width: 2px;
      height: 1rem;
      margin-inline: auto;
      background: var(--connection-color);
      clip-path: none;
    }
    .route-choice { text-align: end; }
  }

  .ancestry .ancestry-toggle {
    display: inline-grid;
    place-items: center;
    flex: 0 0 auto;
    margin-inline-end: var(--ancestry-gap);
    inline-size: var(--ancestry-gutter);
    block-size: var(--ancestry-gutter);
    padding: 0;
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    color: var(--atlas-accent);
    background: var(--atlas-surface);

    &[aria-expanded='true'] svg {
      transform: rotate(180deg);
    }
  }

  nav {
    display: block;
    margin: 0;
    min-width: 0;
    max-width: 100%;
  }

  .viewport {
    max-width: 100%;
    overflow-x: auto;
    overscroll-behavior-x: contain;
    visibility: hidden;
    scrollbar-width: none;
    &::-webkit-scrollbar {
      display: none;
    }
    &:global([data-ready='true']) {
      visibility: visible;
    }
  }

  ol {
    display: flex;
    width: max-content;
    list-style: none;
    margin: 0;
    padding: 0.3rem 1.15rem var(--atlas-breadcrumb-bottom-space, 0.5rem) 0;
  }

  li {
    --crumb-color: #264653;
    position: relative;
    flex: none;
    background: var(--crumb-color);
    color: #fff;
    &:nth-child(4n + 2) {
      --crumb-color: #06584e;
    }
    &:nth-child(4n + 3) {
      --crumb-color: #2d3142;
    }
    &:nth-child(4n) {
      --crumb-color: #4f5d75;
    }
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
      border-radius: 0.2rem 0 0 0.2rem;
    }
  }

  li {
    display: flex;
    align-items: center;
  }
  .viewport li:first-child {
    button,
    span {
      padding-inline-start: 0.75rem;
    }
  }

  .route-choice {
    display: flex;
    align-items: center;
    position: relative;
  }
  .route-toggle {
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 0.5rem 0.6rem;
    border-left: 1px solid #ffffff55;
  }
  .route-menu {
    position: fixed;
    z-index: 1000;
    width: max-content;
    max-width: min(20rem, 80vw);
    padding: 0.3rem;
    background: var(--atlas-surface);
    color: var(--atlas-text);
    border: 1px solid var(--atlas-border);
    border-radius: 0.35rem;
    box-shadow: 0 5px 18px #0003;
  }
  .route-menu button {
    display: block;
    width: 100%;
    padding: 0.55rem 0.7rem;
    color: inherit;
    text-align: left;
    white-space: normal;
  }
  .route-menu button:hover {
    background: var(--atlas-bg);
  }
  .load-older button {
    min-width: 13rem;
  }

  button,
  span {
    display: block;
    border: 0;
    padding: 0.55rem 0.7rem 0.55rem 1.65rem;
    font: inherit;
    font-size: 0.85rem;
    font-weight: 600;
    line-height: 1.4;
    white-space: nowrap;
    color: inherit;
    background: transparent;
  }

  button {
    cursor: pointer;
    &:hover {
      text-decoration: underline;
    }
    &:focus-visible {
      outline: 2px solid white;
      outline-offset: -4px;
      text-decoration: underline;
    }
  }
</style>

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
  - `routes` (`BreadcrumbRoute[]`; optional; default `[]`): Alternative ancestry paths for the route
    switcher.
  - `onRouteChange` (`(route: BreadcrumbRoute) => void`; optional): Callback: Called with the
    selected BreadcrumbRoute after switching a supplied path.
  - `originId` (`string`; optional): Current node ID used to look up alternative parents for the
    final breadcrumb segment.
  - `apiBase` (`string`; optional; default `'/atlas-public-api'`): Base URL for this component’s API
    reads. This prop alone does not configure a proxy.
-->
<script lang="ts" module>
  export type {
    BreadcrumbItem,
    BreadcrumbRoute,
    BreadcrumbPage,
    LoadOlderAncestors,
  } from './breadcrumb.types';
</script>

<script lang="ts">
  import type { BreadcrumbItem, BreadcrumbRoute, LoadOlderAncestors } from './breadcrumb.types';
  let {
    items,
    onNavigate,
    hasMore = false,
    loadOlder,
    routes = [],
    onRouteChange,
    originId,
    apiBase = '/atlas-public-api',
  }: {
    /**
     * Ancestor labels in oldest-to-newest order; IDs enable navigation when a handler is supplied.
     */
    items: BreadcrumbItem[];
    /**
     * Callback: Receives the selected node ID when an ancestor is activated. The parent owns
     * navigation.
     */
    onNavigate?: (id: string) => void;
    /** Whether older ancestors remain available to load. */
    hasMore?: boolean;
    /** Async ancestor loader; receives the oldest displayed ID and returns a BreadcrumbPage. */
    loadOlder?: LoadOlderAncestors;
    /** Alternative ancestry paths for the route switcher. */
    routes?: BreadcrumbRoute[];
    /** Callback: Called with the selected BreadcrumbRoute after switching a supplied path. */
    onRouteChange?: (route: BreadcrumbRoute) => void;
    /** Current node ID used to look up alternative parents for the final breadcrumb segment. */
    originId?: string;
    /** Base URL for this component’s API reads. This prop alone does not configure a proxy. */
    apiBase?: string;
  } = $props();
  /**
   * Older ancestor items prepended by the current path’s pager.
   */
  let older = $state<BreadcrumbItem[]>([]);
  /**
   * ID of the explicitly selected supplied route, or null for the default path.
   */
  let chosenRoute = $state<string | null>(null);
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
   * Child node whose alternative parents populate the menu, or null for supplied routes.
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
    if (!liveMenuChild || liveBusy) return;
    const child = liveMenuChild;
    const currentIndex = allItems.findIndex((item, index) => liveChildAt(index) === child);
    if (currentIndex < 0) return;
    const suffix = allItems.slice(currentIndex + 1);
    liveBusy = true;
    try {
      const response = await fetch(
        `${apiBase}/nodes/${encodeURIComponent(child)}/ancestors?firstParent=${encodeURIComponent(parentId)}`,
        { credentials: 'omit' },
      );
      if (!response.ok) throw new Error('Could not load the selected parent route.');
      const page = (await response.json()) as {
        items: { id: string; title: string }[];
        hasMore: boolean;
      };
      if (!Array.isArray(page.items)) throw new Error('Invalid ancestor response.');
      const newAncestors = [...page.items].reverse();
      const previous = allItems;
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
      preserveScroll = true;
      livePath = [...newAncestors, ...suffix];
      more = page.hasMore;
      openRouteMenu = false;
      liveMenuChild = null;
      // Refresh branch hints for the newly chosen path.
      for (const item of newAncestors) {
        if (liveChoices[item.id] || !item.id) continue;
        void fetch(`${apiBase}/nodes/${encodeURIComponent(item.id)}/parents`, {
          credentials: 'omit',
        })
          .then((res) => (res.ok ? res.json() : []))
          .then((parents) => {
            if (Array.isArray(parents)) liveChoices = { ...liveChoices, [item.id!]: parents };
          })
          .catch(() => {});
      }
    } catch (cause) {
      error = cause instanceof Error ? cause.message : 'Could not change parent path.';
    } finally {
      liveBusy = false;
    }
  }
  // Switch a caller-supplied route while retaining the user’s horizontal position.
  function selectRoute(route: BreadcrumbRoute) {
    preserveScroll = true;
    const selectedIndex = divergenceIndex;
    const anchor = breadcrumbList?.children.item(selectedIndex) as HTMLElement | null;
    pendingAnchor =
      anchor && breadcrumbViewport
        ? {
            id: allItems[selectedIndex]?.id,
            offset:
              anchor.getBoundingClientRect().left - breadcrumbViewport.getBoundingClientRect().left,
          }
        : null;
    chosenRoute = route.id;
    openRouteMenu = false;
    older = [];
    more = false;
    onRouteChange?.(route);
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
  /**
   * Supplied alternative route matching chosenRoute, if any.
   */
  const selectedRoute = $derived(routes.find((route) => route.id === chosenRoute));
  /**
   * Ribbon position showing the supplied routes’ branch chooser; -1 means none.
   */
  const divergenceIndex = $derived(
    routes.length > 1 ? (routes[0].divergenceIndex ?? Math.max(0, items.length - 1)) : -1,
  );
  /**
   * Displayed path: selected supplied route, live alternative, or older items followed by
   * supplied items.
   */
  const allItems = $derived(
    selectedRoute ? selectedRoute.items : (livePath ?? [...older, ...items]),
  );
  const liveChildAt = (index: number) =>
    index + 1 < allItems.length ? allItems[index + 1].id : originId;
  // A new source path resets paging and any previously selected alternate ancestry.
  $effect(() => {
    items;
    hasMore;
    loadOlder;
    routes;
    originId;
    generation++;
    older = [];
    livePath = null;
    liveChoices = {};
    more = hasMore;
    loading = false;
    error = '';
  });
  $effect(() => {
    if (!originId || routes.length) return;
    const controller = new AbortController();
    const ids = [...items.map((item) => item.id), originId].filter((id): id is string => !!id);
    void Promise.all(
      ids.map(async (id) => {
        try {
          const response = await fetch(`${apiBase}/nodes/${encodeURIComponent(id)}/parents`, {
            signal: controller.signal,
            credentials: 'omit',
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
    if (!more || loading || !loadOlder) return;
    const current = generation;
    loading = true;
    error = '';
    try {
      const page = await loadOlder(allItems[0]?.id);
      if (current !== generation) return;
      const seen = new Set(allItems.flatMap((item) => (item.id ? [item.id] : [])));
      // A repeated ancestor means this route has cycled. Stop instead of looping requests.
      const fresh = page.items.filter((item) => !item.id || !seen.has(item.id));
      older = [...fresh, ...older];
      more = page.hasMore && fresh.length === page.items.length && fresh.length > 0;
    } catch (cause) {
      if (current === generation)
        error = cause instanceof Error ? cause.message : 'Could not load older ancestors.';
    } finally {
      if (current === generation) loading = false;
    }
  }

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

{#if allItems.length || more}
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
        {#if more && loadOlder && !selectedRoute}
          <li class="load-older">
            <button type="button" disabled={loading} onclick={requestOlder}
              >{loading ? 'Loading…' : 'Load older ancestors'}</button
            >
          </li>
        {/if}
        {#each allItems as item, index (item.id ?? index)}
          <li style:z-index={allItems.length - index}>
            {#if item.id && onNavigate}
              <button type="button" onclick={() => onNavigate?.(item.id!)}>{item.title}</button>
            {:else}
              <span>{item.title}</span>
            {/if}
            {#if (index === divergenceIndex && routes.length > 1) || (originId && !routes.length && (liveChoices[liveChildAt(index) ?? '']?.length ?? 0) > 1)}
              <div class="route-choice">
                <button
                  type="button"
                  class="route-toggle"
                  aria-label="Choose another parent path"
                  aria-expanded={openRouteMenu}
                  onclick={(event) => {
                    liveMenuChild = routes.length ? null : (liveChildAt(index) ?? null);
                    toggleRouteMenu(event);
                  }}
                >
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
                </button>
              </div>
            {/if}
          </li>
        {/each}
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
        {#if liveMenuChild}
          {#each (liveChoices[liveMenuChild] ?? []).filter((parent) => parent.id !== allItems.find((item, index) => liveChildAt(index) === liveMenuChild)?.id) as parent (parent.id)}
            <button
              type="button"
              disabled={liveBusy}
              onclick={() => void selectLiveParent(parent.id)}>{parent.title}</button
            >
          {/each}
        {:else}
          {#each routes.filter((route) => route.id !== (chosenRoute ?? routes[0].id)) as route (route.id)}
            <button type="button" onclick={() => selectRoute(route)}>{route.label}</button>
          {/each}
        {/if}
      </div>
    {/if}
    {#if error}<p role="alert">
        {error} <button type="button" onclick={requestOlder}>Retry</button>
      </p>{/if}
  </nav>
{/if}

<style lang="scss">
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
      button,
      span {
        padding-inline-start: 0.75rem;
      }
    }
  }

  li {
    display: flex;
    align-items: center;
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

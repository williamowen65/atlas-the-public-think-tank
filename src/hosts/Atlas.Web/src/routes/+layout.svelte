<!--
  @component
  Shared application shell with header, route content and footer. Reads session/profile state
  for account navigation.

  **Props**
  - `children` (`Snippet`; required): Current route/nested-layout content rendered between the
    shared header and footer.
-->
<script lang="ts">
  import { onMount, type ComponentProps, type Snippet } from 'svelte';
  // SvelteKit's reactive route information, including the current URL; not Redux state.
  import { page } from '$app/state';
  import { afterNavigate, beforeNavigate } from '$app/navigation';
  import { readScrollPosition, saveScrollPosition, type ScrollAnchor } from '$lib/state/scrollPositions';
  import GlobeExplorer from '$lib/components/GlobeExplorer.svelte';
  import { apiRequest } from '$lib/api/apiRequest';
  import AtlasHeader from '$lib/components/AtlasHeader.svelte';
  import AtlasFooter from '$lib/components/AtlasFooter.svelte';
  import '../styles.scss';

  // The router supplies the current page (and any nested layouts) as this snippet.
  let {
    children,
  }: {
    /**
     * Current route/nested-layout content supplied by SvelteKit; rendered between the shared
     * header and footer.
     */
    children: Snippet;
  } = $props();

  // Reserve both bars even while scroll animations hide them, so feed cards stay stable.
  let headerHeight = $state(0);
  let bottomNavigationHeight = $state(0);

  // Local reactive state: changes update the props passed to AtlasHeader below.
  /**
   * Session-derived flag passed to the header; does not itself authorize API access.
   */
  let authenticated = $state(false);
  let canModerate = $state(false);
  let pendingModerationCount = $state(0);
  /**
   * Public profile name displayed by the header, with Atlas User as the fallback.
   */
  let displayName = $state('Atlas User');

  let restoreObserver: MutationObserver | undefined;
  let restoreResizeObserver: ResizeObserver | undefined;
  let restoreFrame = 0;
  let restoreTimeout: ReturnType<typeof setTimeout> | undefined;
  let restoringScroll = false;

  function visibleAnchor(): ScrollAnchor | undefined {
    const candidates = [
      ...document.querySelectorAll<HTMLElement>('[data-node-id], [data-comment-id]'),
    ].filter((element) => {
      const rect = element.getBoundingClientRect();
      return rect.bottom > 0 && rect.top < window.innerHeight;
    });
    const element = candidates.sort(
      (a, b) => Math.abs(a.getBoundingClientRect().top) - Math.abs(b.getBoundingClientRect().top),
    )[0];
    if (!element) return undefined;
    const id = element.dataset.nodeId ?? element.dataset.commentId;
    if (!id) return undefined;
    return {
      kind: element.dataset.nodeId ? 'node' : 'comment',
      id,
      offset: element.getBoundingClientRect().top,
      page: element.dataset.feedPage ? Number(element.dataset.feedPage) : undefined,
    };
  }

  function finishScrollRestore() {
    restoringScroll = false;
    document.documentElement.removeAttribute('data-atlas-scroll-restore');
    restoreObserver?.disconnect();
    restoreResizeObserver?.disconnect();
    restoreObserver = undefined;
    restoreResizeObserver = undefined;
    cancelAnimationFrame(restoreFrame);
    if (restoreTimeout) clearTimeout(restoreTimeout);
    restoreTimeout = undefined;
  }

  function restoreScrollPosition(position: NonNullable<ReturnType<typeof readScrollPosition>>) {
    finishScrollRestore();
    restoringScroll = true;
    document.documentElement.setAttribute('data-atlas-scroll-restore', 'pending');

    const findAnchor = () => {
      if (!position.anchor) return undefined;
      const attribute = position.anchor.kind === 'node' ? 'data-node-id' : 'data-comment-id';
      return [...document.querySelectorAll<HTMLElement>(`[${attribute}]`)].find(
        (element) => element.getAttribute(attribute) === position.anchor?.id,
      );
    };
    const attempt = () => {
      if (!restoringScroll) return;
      const anchor = findAnchor();
      if (anchor && position.anchor) {
        const top = anchor.getBoundingClientRect().top;
        window.scrollTo(0, Math.max(0, window.scrollY + top - position.anchor.offset));
        finishScrollRestore();
        return;
      }

      // Reaching the tail lets existing feed/comment pagination fetch pages until the saved
      // item is present. This keeps the data authoritative at the server instead of caching it.
      const root = document.scrollingElement ?? document.documentElement;
      window.scrollTo(0, Math.max(0, root.scrollHeight - window.innerHeight));
    };

    const scheduleAttempt = () => {
      cancelAnimationFrame(restoreFrame);
      restoreFrame = requestAnimationFrame(attempt);
    };

    restoreObserver = new MutationObserver(() => {
      scheduleAttempt();
    });
    restoreObserver.observe(document.querySelector('#main-content') ?? document.body, {
      childList: true,
      subtree: true,
    });
    restoreResizeObserver = new ResizeObserver(() => {
      scheduleAttempt();
    });
    restoreResizeObserver.observe(document.body);
    restoreTimeout = setTimeout(() => {
      if (!restoringScroll) return;
      window.scrollTo(0, position.y);
      finishScrollRestore();
    }, 15000);
    scheduleAttempt();
  }

  afterNavigate(({ from, to, type }) => {
    // A fresh HTTP response supplies share metadata for each directly loaded URL.
    // A client-side transition must not carry the previous page’s tags forward.
    if (from && to && (from.url.pathname !== to.url.pathname || from.url.search !== to.url.search)) {
      document.head.querySelectorAll('[data-atlas-server-seo]').forEach((element) => element.remove());
    }
    if (type !== 'popstate' || !to) return;
    const saved = readScrollPosition(to.url);
    // Feed routes resume the ranked page around this anchor; replaying earlier pages would
    // recreate the expensive load-up-to-the-old-offset behavior this state is meant to avoid.
    if (saved?.anchor?.kind === 'node' && Number.isInteger(saved.anchor.page) && saved.anchor.page! > 0)
      return;
    if (saved && Number.isFinite(saved.y)) restoreScrollPosition(saved);
  });

  // Navigation begins while the outgoing DOM is still available. Store only its scroll
  // coordinates and anchor, leaving URL and history updates entirely to the router.
  beforeNavigate(({ from }) => {
    if (from && !restoringScroll) {
      saveScrollPosition(from.url, { y: window.scrollY, anchor: visibleAnchor() });
    }
    finishScrollRestore();
  });

  // Once mounted in the browser, read the session and listen for sign-in/out changes.
  onMount(() => {
    // Prevent a late response from updating a layout that has been unmounted.
    let active = true;
    async function refresh() {
      try {
        const session = await apiRequest<{ participantId: string | null }>(
          '/atlas-session-api',
          'auth/session',
        );
        if (!active) return;
        authenticated = !!session.participantId;
        canModerate = false;
        pendingModerationCount = 0;
        if (session.participantId) {
          try {
            const queue = await apiRequest<{ items: Array<{ nodeId: string }>; totalCount: number }>('/atlas-session-api', 'moderation/queue');
            if (active) { canModerate = true; pendingModerationCount = queue.totalCount; }
          } catch { /* Moderation link is only shown when the server grants access. */ }
        }
        if (session.participantId) {
          try {
            const profile = await apiRequest<{ displayName: string }>(
              '/atlas-public-api',
              'participants/' + encodeURIComponent(session.participantId),
            );
            if (active) displayName = profile.displayName;
          } catch {
            /* Initials can fall back to generic name. */
          }
        }
      } catch {
        if (active) { authenticated = false; canModerate = false; }
      }
    }
    // This reads the existing session; it does not require the visitor to sign in.
    void refresh();
    window.addEventListener('atlas-session-changed', refresh);
    // Svelte runs this cleanup when the layout is removed.
    return () => {
      active = false;
      window.removeEventListener('atlas-session-changed', refresh);
      finishScrollRestore();
    };
  });

  // Recompute the highlighted navigation item when SvelteKit changes the URL.
  /**
   * Navigation destination derived from the current route for header highlighting.
   */
  const activePage = $derived.by((): ComponentProps<typeof AtlasHeader>['active'] => {
    const path = page.url.pathname;
    if (path === '/') return 'discover';
    if (path.startsWith('/communities')) return 'communities';
    if (path === '/new') return 'create';
    if (path === '/login') return 'login';
    if (path === '/register') return 'register';
    return null;
  });
</script>

<!--
  Layout: Skip link and shared header surround the route’s main content; the shared footer
  closes the application shell.
-->

<a class="skip-link" href="#main-content">Skip to content</a>
<div
  class="app-shell"
  style:--atlas-header-height={headerHeight ? `${headerHeight}px` : undefined}
>
  <GlobeExplorer>
    {#snippet navigation(globeLauncher)}
      <AtlasHeader bind:headerHeight bind:bottomNavigationHeight mobileExperiment {globeLauncher} {authenticated} {displayName} {canModerate} {pendingModerationCount} active={activePage} />
    {/snippet}
  </GlobeExplorer>
  <!-- The current route renders between the shared header and footer. -->
  <main id="main-content" style:--atlas-navigation-height={`${headerHeight + bottomNavigationHeight}px`}>
    {@render children()}
  </main>
  <AtlasFooter />
</div>

<style>
  .app-shell {
    max-width: 76rem;
    min-height: calc(100dvh - 2 * var(--atlas-space));
    margin-inline: auto;
    display: flex;
    flex-direction: column;
  }
  :where(.app-shell) :global(:where([id])) {
    scroll-margin-block-start: calc(var(--atlas-header-height, 5rem) + 0.5rem);
  }
  main {
    flex: 1;
    /* A baseline for brief routes/loading states; long routes retain their natural height. */
    min-block-size: min(18rem, 40svh);
    min-width: 0;
  }
  .skip-link {
    position: absolute;
    left: 1rem;
    top: -5rem;
    padding: 0.75rem;
    background: var(--atlas-surface);
    z-index: 10;
  }
  .skip-link:focus {
    top: 1rem;
  }
</style>

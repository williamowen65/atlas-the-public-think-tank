<script lang="ts">
  import { onMount, type ComponentProps, type Snippet } from 'svelte';
  // SvelteKit's reactive route information, including the current URL; not Redux state.
  import { page } from '$app/state';
  import { commentRequest } from '$lib/api/comments';
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

  // Local reactive state: changes update the props passed to AtlasHeader below.
  let authenticated = $state(false);
  let displayName = $state('Atlas User');
  // Once mounted in the browser, read the session and listen for sign-in/out changes.
  onMount(() => {
    // Prevent a late response from updating a layout that has been unmounted.
    let active = true;
    async function refresh() {
      try {
        const session = await commentRequest<{ participantId: string | null }>(
          '/atlas-session-api',
          'auth/session',
        );
        if (!active) return;
        authenticated = !!session.participantId;
        if (session.participantId) {
          try {
            const profile = await commentRequest<{ displayName: string }>(
              '/atlas-public-api',
              'participants/' + encodeURIComponent(session.participantId),
            );
            if (active) displayName = profile.displayName;
          } catch {
            /* Initials can fall back to generic name. */
          }
        }
      } catch {
        if (active) authenticated = false;
      }
    }
    // This reads the existing session; it does not require the visitor to sign in.
    void refresh();
    window.addEventListener('atlas-session-changed', refresh);
    // Svelte runs this cleanup when the layout is removed.
    return () => {
      active = false;
      window.removeEventListener('atlas-session-changed', refresh);
    };
  });

  // Recompute the highlighted navigation item when SvelteKit changes the URL.
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

<a class="skip-link" href="#main-content">Skip to content</a>
<div class="app-shell">
  <AtlasHeader {authenticated} {displayName} active={activePage} />
  <!-- The current route renders between the shared header and footer. -->
  <main id="main-content">
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
  main {
    flex: 1;
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

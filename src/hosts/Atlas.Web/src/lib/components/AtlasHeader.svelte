<!--
  @component
  AtlasHeader: Shared navigation, search, account menu and logout behavior.
  The parent supplies authentication and display-name props; this component performs the logout request.

  **Props**
  - `active` (`'discover' | 'communities' | 'create' | 'login' | 'register' | null`; optional;
    default `null`): Highlighted navigation item; null leaves all items unselected.
  - `authenticated` (`boolean`; optional; default `false`): Selects the signed-in header menu versus
    login/register links; does not authenticate by itself.
  - `displayName` (`string`; optional; default `'Atlas User'`): Name used to derive the
    account-avatar initial.
-->
<script lang="ts">
  import { myCommunities, type Community } from '../api/communities';
  import DiscoverySearch from './DiscoverySearch.svelte';
  import { restoreDiscoveryQuery } from '../state/discoveryQuery.svelte';
  import { onMount, tick } from 'svelte';
  import { apiRequest } from '../api/apiRequest';
  let {
    mobileExperiment = false,
    active = null,
    authenticated = false,
    displayName = 'Atlas User',
  }: {
    /** Highlighted navigation item; null leaves all items unselected. */
    /** Enable mobile search, community drawer and bottom navigation exploration. */
    mobileExperiment?: boolean;
    active?: 'discover' | 'communities' | 'create' | 'login' | 'register' | null;
    /**
     * Selects the signed-in header menu versus login/register links; does not authenticate by
     * itself.
     */
    authenticated?: boolean;
    /** Name used to derive the account-avatar initial. */
    displayName?: string;
  } = $props();
  /** Whether the header’s search disclosure is mounted below navigation. */
  let searchOpen = $state(false);
  /** Keep the real search input mounted on phones instead of a second lookalike. */
  let phone = $state(false);
  onMount(() => {
    const media = matchMedia('(max-width: 560px)');
    const update = () => { phone = media.matches; };
    update();
    media.addEventListener('change', update);
    return () => media.removeEventListener('change', update);
  });
  /** Mounted disclosure button, used to restore focus when Escape closes search. */
  let searchToggle = $state<HTMLButtonElement>();
  /** Mounted search region; its input receives focus after the disclosure opens. */
  let searchPanel = $state<HTMLDivElement>();
  /** Unique DOM ID connecting the disclosure button to its search region. */
  const searchPanelId = $props.id();

  /** Toggle the search region; wait for Svelte to mount it before focusing the input. */
  async function toggleSearch() {
    searchOpen = !searchOpen;
    if (!searchOpen) {
      filtersOpen = false;
      return;
    }
    headerVisible = true;
    await tick();
    searchPanel?.querySelector('input')?.focus({ preventScroll: true });
  }
  /**
   * Whether DiscoverySearch has its filter toolbox open; keeps the header visible.
   */
  let filtersOpen = $state(false);
  /**
   * Whether scroll behavior currently shows the header.
   */
  let headerVisible = $state(true);
  let lastScroll = 0;
  // Keep the header visible near the top or while filters are open; hide it during downward scrolling.
  onMount(() => {
    // Restore URL/device search even while the optional search input is unmounted.
    restoreDiscoveryQuery();
    lastScroll = window.scrollY;
    const onScroll = () => {
      const next = window.scrollY;
      if (searchOpen || filtersOpen || mobileElement?.open || document.activeElement?.closest('.bottom-navigation, .atlas-header') || next < 40 || next < lastScroll - 3) headerVisible = true;
      else if (next > lastScroll + 3 && next > 100) headerVisible = false;
      lastScroll = next;
    };
    window.addEventListener('scroll', onScroll, { passive: true });
    return () => window.removeEventListener('scroll', onScroll);
  });

  /**
   * Whether the desktop account navigation is open.
   */
  let menuOpen = $state(false);
  /** Membership list is loaded on disclosure, rather than once per feed card. */
  let joined = $state<Community[]>([]);
  let communityError = $state('');
  /** Refresh the switcher whenever its drawer opens; no membership data is invented. */
  async function loadJoined() {
    if (!mobileElement?.open || !mobileExperiment) return;
    joined = [];
    communityError = '';
    if (!authenticated) return;
    try { joined = (await myCommunities()).filter(c => c.isMember); }
    catch { communityError = 'Your communities could not be loaded.'; }
  }
  /**
   * Whether logout is underway; disables repeat logout actions.
   */
  let loggingOut = $state(false);
  /**
   * Logout failure shown in the header; empty text hides it.
   */
  let logoutError = $state('');
  // Send logout through the session API, then notify other session-aware components.
  async function logOut() {
    if (loggingOut) return;
    loggingOut = true;
    logoutError = '';
    menuOpen = false;
    try {
      await apiRequest('/atlas-session-api', 'auth/logout', 'POST');
      window.dispatchEvent(new Event('atlas-session-changed'));
      window.location.assign('/');
    } catch {
      logoutError = 'Could not log out. Please try again.';
      loggingOut = false;
    }
  }
  /**
   * Mounted account-menu container used to detect clicks outside it.
   */
  let accountElement = $state<HTMLDivElement>();
  let mobileElement: HTMLDetailsElement;
  /** Keep native details open until its exit transition finishes. */
  let drawerClosing = $state(false);
  let drawerEntering = $state(false);
  let drawerFrame = 0;
  /** Mount offscreen, then transition on the following paint for every opening. */
  async function openDrawer() {
    clearTimeout(drawerTimer);
    drawerClosing = false;
    drawerEntering = true;
    mobileElement.open = true;
    headerVisible = true;
    await tick();
    drawerFrame = requestAnimationFrame(() => {
      drawerFrame = requestAnimationFrame(() => { drawerEntering = false; });
    });
  }
  let drawerTimer: ReturnType<typeof setTimeout> | undefined;
  /** Close the drawer after its slide-out, optionally restoring menu-button focus. */
  function closeDrawer(restoreFocus = false) {
    if (!mobileElement?.open || drawerClosing) return;
    cancelAnimationFrame(drawerFrame);
    drawerEntering = false;
    const finish = () => {
      mobileElement.open = false;
      drawerClosing = false;
      if (restoreFocus) mobileElement.querySelector('summary')?.focus();
    };
    if (!mobileExperiment || matchMedia('(prefers-reduced-motion: reduce)').matches) finish();
    else {
      drawerClosing = true;
      drawerTimer = setTimeout(finish, 200);
    }
  }
  /** Cancel deferred close when the header unmounts. */
  onMount(() => () => { clearTimeout(drawerTimer); cancelAnimationFrame(drawerFrame); });
  // Dismiss menus without intercepting normal navigation or control activation.
  function closeOnOutside(event: PointerEvent) {
    // Closing the disclosure preserves the shared query for the next time it opens.
    if (
      searchOpen &&
      event.target instanceof Node &&
      !searchPanel?.contains(event.target) &&
      !searchToggle?.contains(event.target)
    ) {
      searchOpen = false;
      filtersOpen = false;
    }
    if (menuOpen && event.target instanceof Node && !accountElement?.contains(event.target))
      menuOpen = false;
    if (
      mobileElement?.open &&
      event.target instanceof Node &&
      !mobileElement.contains(event.target)
    )
      closeDrawer();
  }
  function closeOnEscape(event: KeyboardEvent) {
    if (event.key === 'Escape') {
      // The first Escape closes an open filter toolbox; a subsequent one closes search.
      if (searchOpen && !filtersOpen) {
        searchOpen = false;
        if (mobileExperiment && phone) searchPanel?.querySelector('input')?.blur();
        else searchToggle?.focus();
      }
      menuOpen = false;
      if (mobileElement?.open) {
        closeDrawer(true);
      }
    }
  }
  function closeOnNavigation() {
    menuOpen = false;
    if (mobileElement) closeDrawer();
  }
  function closeOnResize() {
    if (window.innerWidth > 560 && mobileElement?.open) closeDrawer();
  }
  /**
   * Uppercase first character of the display name for the avatar, with U as fallback.
   */
  const initial = $derived(displayName.trim().charAt(0).toUpperCase() || 'U');
</script>

<!--
  Layout: Brand, search toggle and navigation share the top row. The optional search region
  opens below; a separate disclosure supplies mobile navigation.
-->

<svelte:window
  onpointerdown={closeOnOutside}
  onkeydown={closeOnEscape}
  onpopstate={closeOnNavigation}
  onresize={closeOnResize}
/>
<header
  class="atlas-header"
  class:mobile-experiment={mobileExperiment}
  class:search-open={searchOpen}
  class:hidden={!headerVisible}
  class:filters-open={filtersOpen}
  id="page-top"
>
  <a class="brand" href="/" aria-label="Atlas home"
    ><strong>Atlas</strong><span>The Public Think Tank</span></a
  >
  <!-- Search is a disclosure control; its input occupies the full row below navigation. -->
  <button
    class="search-toggle"
    bind:this={searchToggle}
    type="button"
    aria-label="Toggle search"
    title="Search"
    aria-expanded={searchOpen}
    aria-controls={searchPanelId}
    onclick={() => void toggleSearch()}
  >
    <svg
      aria-hidden="true"
      viewBox="0 0 24 24"
      width="20"
      height="20"
      fill="none"
      stroke="currentColor"
      stroke-width="2"
      stroke-linecap="round"
    >
      <circle cx="10.5" cy="10.5" r="6.5" /><path d="m16 16 4.5 4.5" />
    </svg>
    {#if mobileExperiment}<span class="search-placeholder">Atlas: The Public Think Tank</span>{/if}
  </button>
  {#if searchOpen || (mobileExperiment && phone)}
    <div class="search-panel" id={searchPanelId} bind:this={searchPanel}>
      <DiscoverySearch
        active={searchOpen}
        onActivate={() => { searchOpen = true; headerVisible = true; }}
        placeholder={mobileExperiment ? 'Atlas: The Public Think Tank' : undefined}
        onResultSelected={() => {
          searchOpen = false;
          filtersOpen = false;
        }}
        onExpandedChange={(open) => {
          filtersOpen = open;
          if (open) headerVisible = true;
        }}
      />
    </div>
  {/if}
  {#if logoutError}<p class="logout-error" role="alert">{logoutError}</p>{/if}
  <!-- Groups navigation/account controls so responsive styles can position them together. -->
  <div class="right-group">
    <nav class="primary" aria-label="Main navigation">
      <a href="/" aria-current={active === 'discover' ? 'page' : undefined}>Discover</a>
      <a href="/communities" aria-current={active === 'communities' ? 'page' : undefined}
        >Communities</a
      >
      <a href="/new" aria-current={active === 'create' ? 'page' : undefined}>Create</a>
    </nav>
    {#if authenticated}
      <div class="account" bind:this={accountElement}>
        <button
          type="button"
          class="avatar"
          aria-label="Open account menu"
          aria-expanded={menuOpen}
          onclick={() => (menuOpen = !menuOpen)}>{initial}</button
        >
        {#if menuOpen}
          <nav class="account-menu" aria-label="Account navigation">
            <a href="/profiles/me" onclick={closeOnNavigation}>My profile</a>
            <a href="/my-drafts" onclick={closeOnNavigation}>My drafts</a>
            <a href="/my-settings" onclick={closeOnNavigation}>Settings</a>
            <hr />
            <a href="/about" onclick={closeOnNavigation}>About Atlas</a>
            <a href="/feedback" onclick={closeOnNavigation}>Give feedback</a>
            <hr />
            <button
              type="button"
              class="logout-action"
              disabled={loggingOut}
              onclick={() => {
                closeOnNavigation();
                void logOut();
              }}>{loggingOut ? 'Logging out…' : 'Log out'}</button
            >
          </nav>
        {/if}
      </div>
    {:else}
      <div class="auth">
        <a href="/login" aria-current={active === 'login' ? 'page' : undefined}>Log in</a><a
          class="signup"
          href="/register"
          aria-current={active === 'register' ? 'page' : undefined}>Sign up</a
        >
      </div>
    {/if}
    <!--
      Native disclosure provides the narrow-screen navigation independently of the desktop
      account menu.
    -->
    <details class="mobile-menu" class:closing={drawerClosing} class:entering={drawerEntering} bind:this={mobileElement} ontoggle={() => void loadJoined()}>
      <summary aria-label="Open navigation menu" onclick={(event) => {
        if (!mobileExperiment) return;
        event.preventDefault();
        if (mobileElement.open) closeDrawer(true);
        else void openDrawer();
      }}>☰</summary>
      <nav aria-label="Mobile navigation">
        {#if mobileExperiment}
          <button class="close-drawer" type="button" onclick={() => closeDrawer(true)}>Close menu</button>
          <h2>Explore Atlas</h2>
        {/if}
        <a href="/" onclick={closeOnNavigation}>Discover</a><a
          href="/communities"
          onclick={closeOnNavigation}>Communities</a
        ><a href="/new" onclick={closeOnNavigation}>Create</a>
        {#if authenticated}
          <hr />
          <a href="/profiles/me" onclick={closeOnNavigation}>My profile</a><a
            href="/my-drafts"
            onclick={closeOnNavigation}>My drafts</a
          ><a href="/my-settings" onclick={closeOnNavigation}>Settings</a>
          <button
            type="button"
            class="logout-action"
            onclick={() => {
              closeOnNavigation();
              void logOut();
            }}>Log out</button
          >
        {:else}
          <a href="/login" onclick={closeOnNavigation}>Log in</a><a
            href="/register"
            onclick={closeOnNavigation}>Sign up</a
          >
        {/if}
        {#if mobileExperiment && authenticated}
          <hr /><h2>Your communities</h2>
          {#each joined as community}<a href={'/communities/' + encodeURIComponent(community.id)} onclick={closeOnNavigation}>{community.name}</a>{/each}
          {#if communityError}<p role="status">{communityError}</p>{:else if !joined.length}<p>Find communities to join.</p>{/if}
        {/if}
        <hr />
        <a href="/about" onclick={closeOnNavigation}>About Atlas</a><a
          href="/feedback"
          onclick={closeOnNavigation}>Give feedback</a
        >
      </nav>
    </details>
  </div>
</header>
{#if mobileExperiment}
  <nav class="bottom-navigation" class:hidden={!headerVisible} aria-label="Bottom navigation">
    <a href="/" aria-current={active === 'discover' ? 'page' : undefined}><span aria-hidden="true">⌂</span>Discover</a>
    <a href="/new" aria-current={active === 'create' ? 'page' : undefined}><span aria-hidden="true">＋</span>Create</a>
    <a href={authenticated ? '/profiles/me' : '/login'}><span aria-hidden="true">♙</span>You</a>
  </nav>
{/if}

<style lang="scss">
  .search-placeholder, .bottom-navigation { display: none; }
  @media (max-width: 560px) {
    .mobile-experiment {
      padding-block: 0.5rem;
      grid-template-columns: minmax(0, 1fr) auto;
      .brand { grid-row: 1; grid-column: 1; }
      .brand strong { font-size: 1.35rem; }
      .brand span { font-size: 0.7rem; }
      .right-group { grid-column: 2; }
      .search-toggle { grid-row: 2; grid-column: 1 / -1; width: 100%; justify-content: flex-start; gap: 0.6rem; padding-inline: 1rem; color: var(--atlas-muted); }
      .search-placeholder { display: inline; }
      &.search-open .search-toggle { display: none; }
      .search-panel { grid-row: 2; grid-column: 1 / -1; }
      .mobile-menu nav { position: fixed; inset: 0 auto 0 calc(-1 * var(--atlas-space)); width: min(85vw, 22rem); max-height: 100dvh; overflow-y: auto; border-radius: 0; padding: 1rem; box-shadow: 0 0 0 100vmax #0008; }
      .mobile-menu h2 { font-size: 0.9rem; margin: 0.5rem; }
      .mobile-menu p { font-size: 0.8rem; margin: 0.5rem; }
    }
    .bottom-navigation {
      position: fixed; inset: auto 0 0; z-index: 35;
      display: flex; flex-wrap: nowrap; justify-content: space-around; margin: 0;
      padding: 0.35rem 0 max(0.35rem, env(safe-area-inset-bottom));
      border-top: 1px solid var(--atlas-border); background: var(--atlas-surface);
      transition: transform 180ms ease;
      a { display: flex; flex: 1; align-items: center; flex-direction: column; min-height: 2.75rem; font-size: 0.75rem; }
      span { font-size: 1.4rem; line-height: 1.3; }
      &.hidden { transform: translateY(110%); visibility: hidden; }
      &:focus-within { transform: none; visibility: visible; }
    }
  }
  @media (prefers-reduced-motion: reduce) { .bottom-navigation, .atlas-header { transition: none; } }

  .atlas-header {
    position: sticky;
    top: 0;
    z-index: 30;
    background: var(--atlas-bg);
    transition: transform 0.18s ease;
    display: grid;
    grid-template-columns: auto minmax(0, 1fr) auto;
    align-items: center;
    column-gap: clamp(1rem, 2vw, 2rem);
    padding: 1rem 0;
    border-bottom: 0;
  }
  .atlas-header.hidden {
    transform: translateY(calc(-100% - 1rem));
    pointer-events: none;
  }
  .search-toggle {
    position: relative;
    grid-column: 2;
    grid-row: 1;
    justify-self: end;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 2.75rem;
    height: 2.75rem;
    padding: 0;
    border: 1px solid var(--atlas-border);
    border-radius: 999px;
    color: var(--atlas-text);
    background: var(--atlas-surface);
    cursor: pointer;
  }
  .search-toggle:hover,
  .search-toggle[aria-expanded='true'] {
    color: var(--atlas-on-accent);
    background: var(--atlas-accent);
  }
  .search-panel {
    grid-column: 1/-1;
    grid-row: 2;
    min-width: 0;
  }
  .atlas-header.filters-open .right-group {
    grid-column: 3;
    grid-row: 1;
  }
  .atlas-header.filters-open .brand {
    grid-column: 1;
    grid-row: 1;
  }
  .atlas-header::after {
    content: '';
    position: absolute;
    bottom: 0;
    left: 50%;
    width: 100vw;
    transform: translateX(-50%);
    border-bottom: 1px solid var(--atlas-border);
    pointer-events: none;
  }
  .brand {
    display: flex;
    flex-direction: column;
    justify-content: center;
    color: var(--atlas-text);
    text-decoration: none;
    white-space: nowrap;
    align-items: center;
    justify-self: start;
    text-align: center;
    strong {
      font-size: 1.8rem;
    }
    span {
      font-size: 0.85rem;
      color: var(--atlas-muted);
    }
  }
  .right-group {
    grid-column: 3;
    grid-row: 1;
    display: flex;
    align-items: center;
    justify-content: flex-end;
    gap: 1rem;
    min-width: 0;
    align-self: center;
  }
  .primary,
  .account-menu,
  .mobile-menu nav {
    margin: 0;
  }
  .primary,
  .auth {
    display: flex;
    align-items: center;
    gap: 1rem;
    white-space: nowrap;
    font-family: inherit;
    font-size: 0.9rem;
    font-weight: inherit;
    line-height: 1.4;
  }
  .primary a,
  .auth a {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    min-height: 2.5rem;
    box-sizing: border-box;
    font: inherit;
  }
  a {
    color: var(--atlas-text);
    text-decoration: none;
  }
  a:hover {
    text-decoration: underline;
  }
  a[aria-current] {
    font-weight: 700;
    text-underline-offset: 0.3em;
  }
  .signup {
    border: 1px solid var(--atlas-border);
    border-radius: 0.5rem;
    padding: 0.45rem 0.7rem;
  }
  .account {
    position: relative;
  }
  .avatar {
    width: 2.25rem;
    height: 2.25rem;
    border: 1px solid var(--atlas-border);
    border-radius: 50%;
    background: var(--atlas-surface);
    color: var(--atlas-text);
    cursor: pointer;
    font-weight: 700;
  }
  .account-menu,
  .mobile-menu nav {
    position: absolute;
    z-index: 20;
    right: 0;
    top: calc(100% + 0.5rem);
    min-width: 12rem;
    display: flex;
    flex-direction: column;
    gap: 0.15rem;
    background: var(--atlas-surface);
    border: 1px solid var(--atlas-border);
    border-radius: 0.5rem;
    padding: 0.5rem;
    box-shadow: 0 8px 24px #0002;
    a {
      padding: 0.5rem;
      border-radius: 0.25rem;
    }
    hr {
      width: 100%;
      border: 0;
      border-top: 1px solid var(--atlas-border);
    }
  }
  .logout-action {
    font: inherit;
    color: var(--atlas-text);
    background: transparent;
    border: 0;
    text-align: left;
    cursor: pointer;
    padding: 0.5rem;
    border-radius: 0.25rem;
  }
  .logout-action:hover {
    text-decoration: underline;
  }
  .logout-action:disabled {
    opacity: 0.6;
    cursor: default;
  }
  .logout-error {
    grid-column: 1/-1;
    color: var(--atlas-text);
    margin: 0.25rem 0;
  }
  .mobile-menu {
    display: none;
  }
  @media (max-width: 850px) {
    .brand span {
      font-size: 0.75rem;
    }
    .right-group {
      gap: 0.65rem;
    }
    .primary,
    .auth {
      gap: 0.65rem;
    }
  }
  @media (min-width: 561px) and (max-width: 720px) {
    .brand span {
      display: none;
    }
  }
  @media (max-width: 560px) {
    .atlas-header {
      grid-template-columns: minmax(0, 1fr) auto auto;
      gap: 0.6rem;
    }
    .primary,
    .auth,
    .account {
      display: none;
    }
    .mobile-menu {
      display: block;
      position: relative;
      summary {
        cursor: pointer;
        list-style: none;
        font-size: 1.3rem;
      }
      nav {
        right: 0;
        top: calc(100% + 0.5rem);
        min-width: 12rem;
      }
    }
    .right-group {
      grid-column: 3;
      grid-row: 1;
    }
  }
  @media (max-width: 560px) {
    // A single header row: menu on the left, search occupying the remaining width.
    .atlas-header.mobile-experiment {
      grid-template-columns: auto minmax(0, 1fr);
      padding-block: 0.5rem;
      .brand { display: none; }
      .right-group { grid-column: 1; grid-row: 1; }
      .search-toggle { grid-column: 2; grid-row: 1; min-width: 0; padding-inline: 0.65rem; }
      .search-toggle svg { flex-shrink: 0; }
      .search-placeholder { overflow: hidden; text-overflow: ellipsis; white-space: nowrap; font-size: 0.8rem; }
      .search-panel { grid-column: 2; grid-row: 1; }
      .search-panel :global(.filter-bar) { padding: 0; border-top: 0; background: transparent; }
      .mobile-menu nav {
        // Global nav uses flex-wrap; a fixed-height column must not wrap
        // into horizontal columns when its community list grows.
        flex-flow: column nowrap;
        align-items: stretch;
        min-width: 0;
        min-height: 0;
        height: 100dvh;
        max-height: 100dvh;
        overflow-x: hidden;
        padding-bottom: max(1rem, env(safe-area-inset-bottom));
        overflow-y: auto;
        overscroll-behavior: contain;
        touch-action: pan-y;
      }
      .mobile-menu nav > * { flex-shrink: 0; min-width: 0; overflow-wrap: anywhere; }
      .close-drawer { align-self: flex-end; font: inherit; padding: 0.4rem 0.6rem; cursor: pointer; }
    }
    // The header establishes a stacking context. Raise that context, not merely
    // the drawer inside it, so fixed bottom navigation cannot cover the menu.
    .atlas-header.mobile-experiment:has(.mobile-menu[open]) {
      z-index: 50;
      transform: none;
      pointer-events: auto;
    }
    .atlas-header.mobile-experiment:has(.mobile-menu[open]) + .bottom-navigation {
      visibility: hidden;
      pointer-events: none;
    }
    .bottom-navigation a,
    .bottom-navigation a:hover,
    .bottom-navigation a:focus,
    .bottom-navigation a:active,
    .bottom-navigation a[aria-current] {
      text-decoration: none;
    }
  }
  @media (max-width: 560px) {
    .atlas-header.mobile-experiment {
      .mobile-menu nav {
        transform: translateX(0);
        transition: transform 200ms ease-out;
      }
      .mobile-menu.closing nav { transform: translateX(-110%); }
      .close-drawer {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        border: 1px solid var(--atlas-border);
        border-radius: 999px;
        background: var(--atlas-surface);
        color: var(--atlas-text);
        font-size: 0.85rem;
        padding: 0.35rem 0.75rem;
      }
      .close-drawer:hover { background: color-mix(in srgb, var(--atlas-accent) 10%, var(--atlas-surface)); }
    }
    @starting-style {
      .atlas-header.mobile-experiment .mobile-menu[open] nav { transform: translateX(-110%); }
    }
  }
  @media (prefers-reduced-motion: reduce) {
    .atlas-header.mobile-experiment .mobile-menu nav { transition: none; }
  }
  @media (max-width: 560px) {
    .atlas-header.mobile-experiment {
      .search-placeholder { font-size: 0.95rem; font-weight: 700; }
      &.search-open {
        grid-template-columns: minmax(0, 1fr);
        .right-group { display: none; }
        .search-panel { grid-column: 1 / -1; }
      }
    }
  }
  @media (max-width: 560px) {
    .atlas-header.mobile-experiment .search-toggle { display: none; }
    .atlas-header.mobile-experiment .mobile-menu.entering nav { transform: translateX(-110%); }
  }
</style>

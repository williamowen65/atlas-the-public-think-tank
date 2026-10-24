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
  import DiscoverySearch from './DiscoverySearch.svelte';
  import { onMount } from 'svelte';
  import { apiRequest } from '../api/apiRequest';
  let {
    active = null,
    authenticated = false,
    displayName = 'Atlas User',
  }: {
    /** Highlighted navigation item; null leaves all items unselected. */
    active?: 'discover' | 'communities' | 'create' | 'login' | 'register' | null;
    /**
     * Selects the signed-in header menu versus login/register links; does not authenticate by
     * itself.
     */
    authenticated?: boolean;
    /** Name used to derive the account-avatar initial. */
    displayName?: string;
  } = $props();
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
    lastScroll = window.scrollY;
    const onScroll = () => {
      const next = window.scrollY;
      if (filtersOpen || next < 40 || next < lastScroll - 3) headerVisible = true;
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
  // Dismiss menus without intercepting normal navigation or control activation.
  function closeOnOutside(event: PointerEvent) {
    if (menuOpen && event.target instanceof Node && !accountElement?.contains(event.target))
      menuOpen = false;
    if (
      mobileElement?.open &&
      event.target instanceof Node &&
      !mobileElement.contains(event.target)
    )
      mobileElement.open = false;
  }
  function closeOnEscape(event: KeyboardEvent) {
    if (event.key === 'Escape') {
      menuOpen = false;
      if (mobileElement?.open) {
        mobileElement.open = false;
        mobileElement.querySelector('summary')?.focus();
      }
    }
  }
  function closeOnNavigation() {
    menuOpen = false;
    if (mobileElement) mobileElement.open = false;
  }
  function closeOnResize() {
    if (window.innerWidth > 560 && mobileElement?.open) mobileElement.open = false;
  }
  /**
   * Uppercase first character of the display name for the avatar, with U as fallback.
   */
  const initial = $derived(displayName.trim().charAt(0).toUpperCase() || 'U');
</script>

<!--
  Layout: Brand and search share the header with navigation and account controls; a separate
  disclosure supplies mobile navigation.
-->

<svelte:window
  onpointerdown={closeOnOutside}
  onkeydown={closeOnEscape}
  onpopstate={closeOnNavigation}
  onresize={closeOnResize}
/>
<header
  class="atlas-header"
  class:hidden={!headerVisible}
  class:filters-open={filtersOpen}
  id="page-top"
>
  <a class="brand" href="/" aria-label="Atlas home"
    ><strong>Atlas</strong><span>The Public Think Tank</span></a
  >
  <!-- Keeps expandable search controls in their own header layout slot. -->
  <div class="search-slot">
    <DiscoverySearch
      onExpandedChange={(open) => {
        filtersOpen = open;
        if (open) headerVisible = true;
      }}
    />
  </div>
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
    <details class="mobile-menu" bind:this={mobileElement}>
      <summary aria-label="Open navigation menu">☰</summary>
      <nav aria-label="Mobile navigation">
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
        <hr />
        <a href="/about" onclick={closeOnNavigation}>About Atlas</a><a
          href="/feedback"
          onclick={closeOnNavigation}>Give feedback</a
        >
      </nav>
    </details>
  </div>
</header>

<style lang="scss">
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
  .search-slot {
    min-width: 0;
    align-self: center;
  }
  /* Flatten the search wrappers so the search row stays in the same grid cell
     while the expanded controls span the full second header row. */
  .search-slot {
    display: contents;
  }
  .atlas-header :global(.filter-bar) {
    display: contents;
  }
  .atlas-header :global(.search-line) {
    grid-column: 2;
    grid-row: 1;
    min-width: 0;
    flex-wrap: nowrap;
  }
  .atlas-header :global(.toolbox) {
    grid-column: 1/-1;
    grid-row: 2;
    padding: 0.9rem 0;
  }
  .atlas-header :global(.validation) {
    grid-column: 1/-1;
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
    strong {
      font-size: 1.8rem;
    }
    span {
      font-size: 0.85rem;
      color: var(--atlas-muted);
    }
  }
  .right-group {
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
  @media (max-width: 560px) {
    .atlas-header {
      grid-template-columns: minmax(0, 1fr) auto auto;
      gap: 0.6rem;
    }
    .brand {
      display: none;
    }
    .atlas-header.filters-open .brand {
      display: none;
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
    .atlas-header :global(.search-line) {
      grid-column: 1/3;
      grid-row: 1;
    }
    .atlas-header :global(.toolbox) {
      grid-column: 1/-1;
      grid-row: 2;
    }

    .right-group {
      grid-column: 3;
      grid-row: 1;
    }
  }
</style>

<script lang="ts">
  import DiscoverySearch from './DiscoverySearch.svelte';
  import { onMount } from 'svelte';
  import { commentRequest } from '../api/comments';
  let { active = null, authenticated = false, displayName = 'Atlas User', onNavigate }: {
    active?: 'discover' | 'communities' | 'create' | 'login' | 'register' | null;
    authenticated?: boolean;
    displayName?: string;
    onNavigate?: (destination: 'discover' | 'create' | 'login' | 'register' | 'profile') => void;
  } = $props();
  let searching = $state(false);
  let filtersOpen = $state(false);
  let headerVisible = $state(true);
  let lastScroll = 0;
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

  let menuOpen = $state(false);
  let loggingOut = $state(false);
  let logoutError = $state('');
  async function logOut() {
    if (loggingOut) return;
    loggingOut = true;
    logoutError = '';
    menuOpen = false;
    try {
      await commentRequest('/atlas-comments-api', 'auth/logout', 'POST');
      window.dispatchEvent(new Event('atlas-session-changed'));
      window.location.assign('/');
    } catch {
      logoutError = 'Could not log out. Please try again.';
      loggingOut = false;
    }
  }
  let accountElement: HTMLDivElement;
  function closeOnOutside(event: PointerEvent) {
    if (menuOpen && event.target instanceof Node && !accountElement?.contains(event.target)) menuOpen = false;
  }
  function closeOnEscape(event: KeyboardEvent) {
    if (event.key === 'Escape') menuOpen = false;
  }
  function closeOnNavigation() { menuOpen = false; }
  const initial = $derived(displayName.trim().charAt(0).toUpperCase() || 'U');
</script>

<svelte:window onpointerdown={closeOnOutside} onkeydown={closeOnEscape} onpopstate={closeOnNavigation} />
<header class="atlas-header" class:hidden={!headerVisible} class:filters-open={filtersOpen} id="page-top">
  <a class="brand" href="/" aria-label="Atlas home"><strong>Atlas</strong><span>The Public Think Tank</span></a>
  <div class="search-slot"><DiscoverySearch onExpandedChange={open => { filtersOpen = open; if (open) headerVisible = true; }} /></div>
  {#if logoutError}<p class="logout-error" role="alert">{logoutError}</p>{/if}
  <div class="right-group">
    <nav class="primary" aria-label="Main navigation">
      <a href="/" onclick={event => { if (onNavigate) { event.preventDefault(); onNavigate('discover'); } }} aria-current={active === 'discover' ? 'page' : undefined}>Discover</a>
      <a href="/communities" aria-current={active === 'communities' ? 'page' : undefined}>Communities</a>
      <a href="/new" onclick={event => { if (onNavigate) { event.preventDefault(); onNavigate('create'); } }} aria-current={active === 'create' ? 'page' : undefined}>Create</a>
    </nav>
    {#if authenticated}
      <div class="account" bind:this={accountElement}>
        <button type="button" class="avatar" aria-label="Open account menu" aria-expanded={menuOpen} onclick={() => menuOpen = !menuOpen}>{initial}</button>
        {#if menuOpen}
          <nav class="account-menu" aria-label="Account navigation" onclick={closeOnNavigation}>
            <a href="/profiles/me" onclick={event => { if (onNavigate) { event.preventDefault(); onNavigate("profile"); } }}>My profile</a>
            <a href="/me/drafts">My drafts</a>
            <a href="/me/settings">Settings</a>
            <hr />
            <a href="/about">About Atlas</a>
            <a href="/feedback">Give feedback</a>
            <hr />
            <button type="button" class="logout-action" disabled={loggingOut} onclick={() => void logOut()}>{loggingOut ? 'Logging out…' : 'Log out'}</button>
          </nav>
        {/if}
      </div>
    {:else}
      <div class="auth"><a href="/login" aria-current={active === 'login' ? 'page' : undefined} onclick={event => { if (onNavigate) { event.preventDefault(); onNavigate("login"); } }}>Log in</a><a class="signup" href="/register" aria-current={active === 'register' ? 'page' : undefined} onclick={event => { if (onNavigate) { event.preventDefault(); onNavigate("register"); } }}>Sign up</a></div>
    {/if}
    <details class="mobile-menu">
      <summary aria-label="Open navigation menu">☰</summary>
      <nav aria-label="Mobile navigation">
        <a href="/">Discover</a><a href="/communities">Communities</a><a href="/new">Create</a>
        {#if authenticated}
          <a href="/profiles/me">My profile</a><a href="/me/drafts">My drafts</a><a href="/me/settings">Settings</a>
          <button type="button" class="logout-action" onclick={() => void logOut()}>Log out</button>
        {:else}
          <a href="/login">Log in</a><a href="/register">Sign up</a>
        {/if}
        <a href="/about">About Atlas</a><a href="/feedback">Give feedback</a>
      </nav>
    </details>
  </div>
</header>
<style lang="scss">
  .atlas-header { position:sticky;top:0;z-index:30;background:var(--atlas-surface);transition:transform .18s ease;display:grid;grid-template-columns:auto minmax(0,1fr) auto;align-items:center;column-gap:clamp(1rem,2vw,2rem);padding:1rem 0;border-bottom:0; }
  .atlas-header.hidden{transform:translateY(calc(-100% - 1rem));pointer-events:none;}
  .search-slot{min-width:0;align-self:center;}
  /* Flatten the search wrappers so the search row stays in the same grid cell
     while the expanded controls span the full second header row. */
  .search-slot{display:contents;}
  .atlas-header :global(.filter-bar){display:contents;}
  .atlas-header :global(.search-line){grid-column:2;grid-row:1;min-width:0;flex-wrap:nowrap;}
  .atlas-header :global(.toolbox){grid-column:1/-1;grid-row:2;padding:.9rem 0;}
  .atlas-header :global(.validation){grid-column:1/-1;}
  .atlas-header.filters-open .right-group{grid-column:3;grid-row:1;}
  .atlas-header.filters-open .brand{grid-column:1;grid-row:1;}
  .atlas-header::after{content:'';position:absolute;bottom:0;left:50%;width:100vw;transform:translateX(-50%);border-bottom:1px solid var(--atlas-border);pointer-events:none;}
  .brand {display:flex;flex-direction:column;justify-content:center;color:var(--atlas-text);text-decoration:none;white-space:nowrap;strong{font-size:1.8rem;}span{font-size:.85rem;color:var(--atlas-muted);}}
  .search {display:flex;align-items:center;width:100%;min-width:0;border:1px solid var(--atlas-border);border-radius:.5rem;box-sizing:border-box;input{min-width:0;width:100%;flex:1;border:0;background:transparent;color:var(--atlas-text);padding:.55rem;}button{flex-shrink:0;border:0;background:transparent;color:var(--atlas-text);cursor:pointer;font-size:1.4rem;padding:0 .65rem;}.close-search{display:none;}}
  .sr-only{position:absolute;width:1px;height:1px;padding:0;margin:-1px;overflow:hidden;clip:rect(0,0,0,0);white-space:nowrap;border:0;}
  .right-group{display:flex;align-items:center;justify-content:flex-end;gap:1rem;min-width:0;align-self:center;}
  .primary,.account-menu,.mobile-menu nav{margin:0;}\n  .primary,.auth{display:flex;align-items:center;gap:1rem;white-space:nowrap;font-size:.9rem;line-height:1.4;}.primary a,.auth a{display:inline-flex;align-items:center;justify-content:center;min-height:2.5rem;box-sizing:border-box;}
  a{color:var(--atlas-text);text-decoration:none;}a:hover{text-decoration:underline;}a[aria-current]{font-weight:700;text-underline-offset:.3em;}
  .signup{border:1px solid var(--atlas-border);border-radius:.5rem;padding:.45rem .7rem;}
  .account{position:relative;}.avatar{width:2.25rem;height:2.25rem;border:1px solid var(--atlas-border);border-radius:50%;background:var(--atlas-surface);color:var(--atlas-text);cursor:pointer;font-weight:700;}
  .account-menu{position:absolute;z-index:20;right:0;top:calc(100% + .5rem);min-width:12rem;display:flex;flex-direction:column;gap:.15rem;background:var(--atlas-surface);border:1px solid var(--atlas-border);border-radius:.5rem;padding:.5rem;box-shadow:0 8px 24px #0002;a{padding:.5rem;border-radius:.25rem;}hr{width:100%;border:0;border-top:1px solid var(--atlas-border);}}
  .logout-action{font:inherit;color:var(--atlas-text);background:transparent;border:0;text-align:left;cursor:pointer;padding:.5rem;border-radius:.25rem;}.logout-action:hover{text-decoration:underline;}.logout-action:disabled{opacity:.6;cursor:default;}
  .logout-error{grid-column:1/-1;color:var(--atlas-text);margin:.25rem 0;}
  .mobile-menu{display:none;}
  @media(max-width:850px){.brand span{font-size:.75rem;}.right-group{gap:.65rem;}.primary,.auth{gap:.65rem;}}
  @media(max-width:560px){
    .atlas-header{grid-template-columns:minmax(0,1fr) auto auto;gap:.6rem;}
    .brand{display:none;}
    .atlas-header.filters-open .brand{display:none;}
    .primary,.auth,.account{display:none;}
    .mobile-menu{display:block;position:relative;summary{cursor:pointer;list-style:none;font-size:1.3rem;}nav{position:absolute;z-index:20;right:0;top:100%;min-width:11rem;display:flex;flex-direction:column;background:var(--atlas-surface);border:1px solid var(--atlas-border);border-radius:.5rem;padding:.75rem;gap:.75rem;}}
    .atlas-header :global(.search-line){grid-column:1/3;grid-row:1;}
    .atlas-header :global(.toolbox){grid-column:1/-1;grid-row:2;}





    .right-group{grid-column:3;grid-row:1;}
  }
</style>
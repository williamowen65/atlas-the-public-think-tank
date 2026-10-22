<script lang="ts">
  let { active = 'discover', authenticated = false, displayName = 'Atlas User' }: {
    active?: 'discover' | 'about' | 'components' | 'create';
    authenticated?: boolean;
    displayName?: string;
  } = $props();
  let searching = $state(false);
  let searchText = $state('');
  let menuOpen = $state(false);
  const initial = $derived(displayName.trim().charAt(0).toUpperCase() || 'U');
  function submitSearch(event: SubmitEvent) {
    event.preventDefault();
    const query = searchText.trim();
    if (query) window.location.assign('/?search=' + encodeURIComponent(query));
  }
</script>

<header class="atlas-header" id="page-top">
  <a class="brand" href="/" aria-label="Atlas home"><strong>Atlas</strong><span>The Public Think Tank</span></a>
  <form class:expanded={searching} class="search" role="search" onsubmit={submitSearch}>
    <label class="sr-only" for="atlas-search">Search Atlas</label>
    <input id="atlas-search" type="search" placeholder="Search Atlas" bind:value={searchText} />
    <button type="submit" aria-label="Submit search" onclick={(event) => { if (window.matchMedia("(max-width: 560px)").matches && !searching) { event.preventDefault(); searching = true; } }}>⌕</button>
    {#if searching}<button type="button" class="close-search" onclick={() => searching = false} aria-label="Close search">×</button>{/if}
  </form>
  <div class="right-group">
    <nav class="primary" aria-label="Main navigation">
      <a href="/" aria-current={active === 'discover' ? 'page' : undefined}>Discover</a>
      <a href="/new" aria-current={active === 'create' ? 'page' : undefined}>Create</a>
    </nav>
    {#if authenticated}
      <div class="account">
        <button type="button" class="avatar" aria-label="Open account menu" aria-expanded={menuOpen} onclick={() => menuOpen = !menuOpen}>{initial}</button>
        {#if menuOpen}
          <nav class="account-menu" aria-label="Account navigation">
            <a href="/me">My profile</a>
            <a href="/me/drafts">My drafts</a>
            <a href="/me/settings">Settings</a>
            <hr />
            <a href="/about">About Atlas</a>
            <a href="/feedback">Give feedback</a>
            <hr />
            <a href="/logout">Log out</a>
          </nav>
        {/if}
      </div>
    {:else}
      <div class="auth"><a href="/login">Log in</a><a class="signup" href="/register">Sign up</a></div>
    {/if}
    <details class="mobile-menu">
      <summary aria-label="Open navigation menu">☰</summary>
      <nav aria-label="Mobile navigation">
        <a href="/">Discover</a><a href="/new">Create</a>
        {#if authenticated}
          <a href="/me">My profile</a><a href="/me/drafts">My drafts</a><a href="/me/settings">Settings</a>
          <a href="/logout">Log out</a>
        {:else}
          <a href="/login">Log in</a><a href="/register">Sign up</a>
        {/if}
        <a href="/about">About Atlas</a><a href="/feedback">Give feedback</a>
      </nav>
    </details>
  </div>
</header>
<style lang="scss">
  .atlas-header { position:relative;display:grid;grid-template-columns:auto minmax(0,1fr) auto;align-items:center;column-gap:clamp(1rem,2vw,2rem);padding:1rem 0;border-bottom:1px solid var(--atlas-border); }
  .brand {display:flex;flex-direction:column;justify-content:center;color:var(--atlas-text);text-decoration:none;white-space:nowrap;strong{font-size:1.8rem;}span{font-size:.85rem;color:var(--atlas-muted);}}
  .search {display:flex;align-items:center;width:100%;min-width:0;border:1px solid var(--atlas-border);border-radius:.5rem;box-sizing:border-box;input{min-width:0;width:100%;flex:1;border:0;background:transparent;color:var(--atlas-text);padding:.55rem;}button{flex-shrink:0;border:0;background:transparent;color:var(--atlas-text);cursor:pointer;font-size:1.4rem;padding:0 .65rem;}.close-search{display:none;}}
  .sr-only{position:absolute;width:1px;height:1px;padding:0;margin:-1px;overflow:hidden;clip:rect(0,0,0,0);white-space:nowrap;border:0;}
  .right-group{display:flex;align-items:center;justify-content:flex-end;gap:1rem;min-width:0;align-self:center;}
  .primary,.account-menu,.mobile-menu nav{margin:0;}\n  .primary,.auth{display:flex;align-items:center;gap:1rem;white-space:nowrap;font-size:.9rem;line-height:1.4;}.primary a,.auth a{display:inline-flex;align-items:center;justify-content:center;min-height:2.5rem;box-sizing:border-box;}
  a{color:var(--atlas-text);text-decoration:none;}a:hover{text-decoration:underline;}a[aria-current]{font-weight:700;text-underline-offset:.3em;}
  .signup{border:1px solid var(--atlas-border);border-radius:.5rem;padding:.45rem .7rem;}
  .account{position:relative;}.avatar{width:2.25rem;height:2.25rem;border:1px solid var(--atlas-border);border-radius:50%;background:var(--atlas-surface);color:var(--atlas-text);cursor:pointer;font-weight:700;}
  .account-menu{position:absolute;z-index:20;right:0;top:calc(100% + .5rem);min-width:12rem;display:flex;flex-direction:column;gap:.15rem;background:var(--atlas-surface);border:1px solid var(--atlas-border);border-radius:.5rem;padding:.5rem;box-shadow:0 8px 24px #0002;a{padding:.5rem;border-radius:.25rem;}hr{width:100%;border:0;border-top:1px solid var(--atlas-border);}}
  .mobile-menu{display:none;}
  @media(max-width:850px){.brand span{font-size:.75rem;}.right-group{gap:.65rem;}.primary,.auth{gap:.65rem;}}
  @media(max-width:560px){
    .atlas-header{grid-template-columns:minmax(0,1fr) auto auto;gap:.6rem;}
    .brand span{display:none;}
    .primary,.auth,.account{display:none;}
    .mobile-menu{display:block;position:relative;summary{cursor:pointer;list-style:none;font-size:1.3rem;}nav{position:absolute;z-index:20;right:0;top:100%;min-width:11rem;display:flex;flex-direction:column;background:var(--atlas-surface);border:1px solid var(--atlas-border);border-radius:.5rem;padding:.75rem;gap:.75rem;}}
    .search{grid-column:2;grid-row:1;width:2.75rem;border:0;}
    .search input{display:none;}
    .search.expanded{grid-column:1/3;grid-row:1;width:100%;border:1px solid var(--atlas-border);}
    .search.expanded input{display:block;}
    .search.expanded .close-search{display:block;}
    .atlas-header:has(.search.expanded) .brand{display:none;}
    .right-group{grid-column:3;grid-row:1;}
  }
</style>
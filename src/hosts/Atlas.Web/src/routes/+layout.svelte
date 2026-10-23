<script lang="ts">
  import '../styles.scss';
  import { onMount } from 'svelte';
  import { commentRequest } from '$lib/api/comments';
  let authenticated = $state(false);
  let displayName = $state('Atlas User');
  onMount(() => {
    let active = true;
    async function refresh() {
      try {
        const session = await commentRequest<{ participantId: string | null }>('/atlas-comments-api', 'auth/session');
        if (!active) return;
        authenticated = !!session.participantId;
        if (session.participantId) {
          try { const profile = await commentRequest<{ displayName: string }>('/atlas-preview-api', 'participants/' + encodeURIComponent(session.participantId)); if (active) displayName = profile.displayName; } catch { /* Initials can fall back to generic name. */ }
        }
      } catch { if (active) authenticated = false; }
    }
    void refresh();
    window.addEventListener('atlas-session-changed', refresh);
    return () => { active = false; window.removeEventListener('atlas-session-changed', refresh); };
  });
  import { page } from '$app/state';
  import AtlasHeader from '$lib/components/AtlasHeader.svelte';
  import AtlasFooter from '$lib/components/AtlasFooter.svelte';
  let { children } = $props();
</script>
<a class="skip-link" href="#main-content">Skip to content</a>
<div class="app-shell">
  <AtlasHeader {authenticated} {displayName} active={page.url.pathname === '/' ? 'discover' : page.url.pathname.startsWith('/communities') ? 'communities' : page.url.pathname === '/new' ? 'create' : null} />
  <main id="main-content">{@render children()}</main>
  <AtlasFooter />
</div>
<style>
  .app-shell{max-width:76rem;min-height:calc(100dvh - 2 * var(--atlas-space));margin-inline:auto;display:flex;flex-direction:column;}main{flex:1;min-width:0;}
  .skip-link{position:absolute;left:1rem;top:-5rem;padding:.75rem;background:var(--atlas-surface);z-index:10;}.skip-link:focus{top:1rem;}
</style>

<script lang="ts">
  import '../styles.scss';
  import { page } from '$app/state';
  import AtlasHeader from '$lib/components/AtlasHeader.svelte';
  import AtlasFooter from '$lib/components/AtlasFooter.svelte';
  let { children } = $props();
</script>
<a class="skip-link" href="#main-content">Skip to content</a>
<div class="app-shell">
  <AtlasHeader active={page.url.pathname === '/new' ? 'create' : page.url.pathname === '/about' ? 'about' : page.url.pathname === '/components' ? 'components' : 'discover'} />
  <main id="main-content">{@render children()}</main>
  <AtlasFooter />
</div>
<style>
  .app-shell{max-width:76rem;min-height:calc(100dvh - 2 * var(--atlas-space));margin-inline:auto;display:flex;flex-direction:column;}main{flex:1;min-width:0;}
  .skip-link{position:absolute;left:1rem;top:-5rem;padding:.75rem;background:var(--atlas-surface);z-index:10;}.skip-link:focus{top:1rem;}
</style>

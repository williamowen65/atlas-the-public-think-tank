<script lang="ts">
  let { active = 'discover' }: { active?: 'discover' | 'about' | 'components' | 'create' } = $props();
</script>
<header class="atlas-header" id="page-top">
  <a class="brand" href="/" aria-label="Atlas home"><strong>Atlas</strong><span>The Public Think Tank</span></a>
  <nav aria-label="Main navigation">
    <a href="/" aria-current={active === 'discover' ? 'page' : undefined}>Discover</a>
    <a href="/new" aria-current={active === 'create' ? 'page' : undefined}>Create node</a>
    <a href="/about" aria-current={active === 'about' ? 'page' : undefined}>About this foundation</a>
    <a href="/components" aria-current={active === 'components' ? 'page' : undefined}>Component example</a>
  </nav>
</header>
<style lang="scss">
  .atlas-header { display:flex; flex-wrap:wrap; align-items:center; justify-content:space-between; gap:1rem 2rem; padding:1rem 0; border-bottom:1px solid var(--atlas-border); }
  .brand { display:flex; flex-direction:column; color:var(--atlas-text); text-decoration:none; strong {font-size:1.8rem;} span {font-size:.85rem;color:var(--atlas-muted);} }
  nav { margin:0; align-items:center; font-size:.9rem; a[aria-current] {font-weight:700;text-underline-offset:.3em;} }
</style>

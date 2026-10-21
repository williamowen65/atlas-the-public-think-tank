<script lang="ts">
  let { tagline = 'A place to connect questions, evidence, and ideas.' }: { tagline?: string } = $props();
</script>
<footer class="atlas-footer">
  <div><strong>Atlas · The Public Think Tank</strong><p>{tagline}</p></div>
  <nav aria-label="Footer navigation"><a href="/about">About Atlas</a><a href="#page-top">Back to top</a></nav>
</footer>
<style lang="scss">
  .atlas-footer {display:flex;flex-wrap:wrap;justify-content:space-between;align-items:baseline;gap:1rem 2rem;padding:1.5rem 0;margin-top:2rem;border-top:1px solid var(--atlas-border);font-size:.85rem;color:var(--atlas-muted);p{margin:.4rem 0;max-width:40rem;}nav{margin:0;}}
</style>

<script lang="ts">
  let { children } = $props();
</script>

<div class="information-page">
  {@render children()}
</div>

<style>
  .information-page {
    max-width: 48rem;
    margin-inline: auto;
    padding-block: 1rem 2rem;
    line-height: 1.55;
  }
  .information-page :global(section) {
    padding-block: 1rem;
    border-top: 1px solid var(--atlas-border);
  }
  .information-page :global(h1),
  .information-page :global(h2) { margin-block: 0 .5rem; }
  .information-page :global(p),
  .information-page :global(ul) { margin-block: 0 .75rem; }
</style>

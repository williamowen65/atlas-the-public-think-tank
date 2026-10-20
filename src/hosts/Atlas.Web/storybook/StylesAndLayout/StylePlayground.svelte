<script lang="ts">
  let { columns = 2, maxWidth = 900 }: { columns?: number; maxWidth?: number } = $props();
</script>

<section class="atlas-container" style:max-width={`${maxWidth}px`}>
  <h1>Atlas styles and layout</h1>
  <p>Use the Theme and Density toolbar menus to change global tokens across every story.</p>
  <h2>Typography and surfaces</h2>
  <p>Readable body text, <a href="#layout-samples">links</a>, spacing and focus styles share one token source.</p>
  <div id="layout-samples" class="samples" style:grid-template-columns={`repeat(${columns}, minmax(0, 1fr))`}>
    {#each ['Surface and text', 'Accent and contrast', 'Borders and spacing', 'Responsive wrapping'] as title}
      <article class="atlas-panel"><h3>{title}</h3><p>A sample card with enough text to inspect line length and wrapping.</p><button class="atlas-action" type="button">Focus sample</button></article>
    {/each}
  </div>
  <p class="fixture-note">Starter CSS tokens, not a final design or selected CSS framework.</p>
</section>

<style>
  .samples { display: grid; gap: var(--atlas-space); }
  @media (max-width: 480px) { .samples { grid-template-columns: 1fr !important; } }
</style>

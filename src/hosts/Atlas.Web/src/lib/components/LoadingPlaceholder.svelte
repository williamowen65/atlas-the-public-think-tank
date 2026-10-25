<!--
  @component
  Temporary content-shaped space while an initial API read is pending. Removed on success or error;
  it does not cache data, delay rendering or impose minimum height on loaded content.

  **Props**
  - `kind` (`'feed' | 'node' | 'communities'`; optional; default `'feed'`): Layout being loaded.
  - `label` (`string`; optional; default `'Loading nodes…'`): Accessible and visible loading message.
-->
<script lang="ts">
  let {
    kind = 'feed',
    label = 'Loading nodes…',
  }: {
    /** Content shape to approximate while waiting for the initial response. */
    kind?: 'feed' | 'node' | 'communities';
    /** Status message announced once; the decorative shapes are hidden from assistive technology. */
    label?: string;
  } = $props();
</script>

<!-- Layout: One loading status followed by static decorative cards approximating incoming content. -->
<div
  class="loading-placeholder"
  class:node={kind === 'node'}
  class:communities={kind === 'communities'}
  role="status"
  aria-busy="true"
>
  <p>{label}</p>
  <div class="shapes" aria-hidden="true">
    {#each Array.from({ length: kind === 'node' ? 1 : 3 }) as _}
      <div class="shape">
        <span class="heading"></span>
        <span></span>
        <span></span>
        <span class="short"></span>
      </div>
    {/each}
  </div>
</div>

<style>
  .loading-placeholder {
    max-width: 48rem;
    margin-inline: auto;
  }
  p {
    margin: 0 0 0.75rem;
    color: var(--atlas-muted);
  }
  .shapes {
    display: grid;
    gap: 1.25rem;
  }
  .shape {
    height: 14rem;
    padding: var(--atlas-card-padding, 1.25rem);
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    background: var(--atlas-surface);
  }
  .node .shape {
    height: 30rem;
  }
  .communities .shape {
    height: 7rem;
  }
  span {
    display: block;
    height: 0.75rem;
    margin-block: 0 1rem;
    background: var(--atlas-border);
    opacity: 0.18;
    border-radius: 0.25rem;
  }
  .heading {
    height: 1.3rem;
    width: 65%;
    margin-bottom: 1.5rem;
  }
  .short {
    width: 45%;
  }
</style>

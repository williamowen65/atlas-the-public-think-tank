<!--
  @component
  Temporary content-shaped space while an initial API read is pending. Removed on success or error;
  it does not cache data, delay rendering or impose minimum height on loaded content.

  **Props**
  - `kind` (`'feed' | 'node' | 'communities' | 'mini'`; optional; default `'feed'`): Layout being loaded.
  - `label` (`string`; optional; default `'Loading nodes…'`): Accessible and visible loading message.
-->
<script lang="ts">
  let {
    kind = 'feed',
    label = 'Loading nodes…',
  }: {
    /** Content shape to approximate while waiting for the initial response. */
    kind?: 'feed' | 'node' | 'communities' | 'mini';
    /** Status message announced once; the decorative shapes are hidden from assistive technology. */
    label?: string;
  } = $props();
</script>

<!-- Layout: One loading status followed by static decorative cards approximating incoming content. -->
<div
  class="loading-placeholder"
  class:node={kind === 'node'}
  class:communities={kind === 'communities'}
  class:mini={kind === 'mini'}
  role="status"
  aria-busy="true"
>
  <p>{label}</p>
  <div class="shapes" aria-hidden="true">
    {#each Array.from({ length: kind === 'node' || kind === 'mini' ? 1 : 3 }) as _}
      <div class="shape">
        {#if kind === 'mini'}
          <div class="mini-meta"><span></span><span></span></div>
          <span class="mini-title"></span>
        {:else}
          <span class="heading"></span>
          <span></span>
          <span></span>
          <span class="short"></span>
        {/if}
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
  .mini p {
    position: absolute;
    width: 1px;
    height: 1px;
    padding: 0;
    margin: -1px;
    overflow: hidden;
    clip-path: inset(50%);
    white-space: nowrap;
  }
  .mini .shape {
    box-sizing: border-box;
    height: 3.55rem;
    padding: .45rem .7rem .55rem;
    display: grid;
    align-content: center;
    gap: .35rem;
  }
  .mini .shape span { margin: 0; }
  .mini-meta { display: flex; justify-content: space-between; gap: .5rem; }
  .mini-meta span { width: 20%; height: .6rem; }
  .mini-meta span:last-child { width: 25%; }
  .mini-title { width: 70%; height: .85rem; }
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

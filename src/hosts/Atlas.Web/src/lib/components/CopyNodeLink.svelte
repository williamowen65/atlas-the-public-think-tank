<script lang="ts">
  let { nodeId }: { nodeId: string } = $props();
  let message = $state('');
  let manualLink = $state('');

  async function copyLink() {
    const url = new URL('/nodes', window.location.origin);
    url.searchParams.set('id', nodeId);
    try {
      await navigator.clipboard.writeText(url.href);
      manualLink = '';
      message = 'Link copied';
    } catch {
      manualLink = url.href;
      message = 'Select and copy the link';
    }
  }
</script>

<span class="share">
  <button type="button" aria-label="Copy node link" title="Copy node link" onclick={copyLink}>
    <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
      <path d="M10 13a5 5 0 0 0 7 .5l3-3a5 5 0 0 0-7-7l-2 2m3 5.5a5 5 0 0 0-7-.5l-3 3a5 5 0 0 0 7 7l2-2" />
    </svg>
  </button>
  <span role="status">{message}</span>
  {#if manualLink}
    <input aria-label="Node link" readonly value={manualLink} onfocus={(event) => event.currentTarget.select()} />
  {/if}
</span>

<style>
  .share { display: inline-flex; align-items: center; gap: .25rem; vertical-align: middle; }
  button {
    display: grid;
    place-items: center;
    width: 2rem;
    height: 2rem;
    padding: 0;
    border: 0;
    border-radius: .35rem;
    background: transparent;
    color: inherit;
    cursor: pointer;
  }
  button:hover { background: color-mix(in srgb, currentColor 10%, transparent); }
  input { max-width: 12rem; }
</style>

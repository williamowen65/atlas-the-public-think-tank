<script lang="ts">
  import { publicUrl } from '../../src/lib/api/nodePage';
  import { linkPreviewSamples } from './sectionLayouts';

  let { url, editable = false }: { url: string; editable?: boolean } = $props();
  const href = $derived(publicUrl(url.trim()));
  const metadata = $derived(href ? linkPreviewSamples.find((sample) => sample.url === href) : undefined);
  const domain = $derived(href ? new URL(href).hostname : '');
</script>

{#snippet content()}
  {#if metadata?.image}
    <img src={metadata.image} alt="" loading="lazy" referrerpolicy="no-referrer" />
  {/if}
  <div class="summary">
    <small>{domain}</small>
    {#if metadata}
      <strong>{metadata.title}</strong>
      <p>{metadata.description}</p>
    {:else}
      <span>{href}</span>
    {/if}
  </div>
{/snippet}

{#if href}
  {#if editable}
    <div class="card">{@render content()}</div>
  {:else}
    <a class="card" href={href} target="_blank" rel="noopener noreferrer">{@render content()}</a>
  {/if}
{/if}

<style>
  .card {
    display: block; overflow: hidden; color: inherit; text-decoration: none;
    border: 1px solid var(--atlas-border); border-radius: var(--atlas-radius, .5rem);
    background: var(--atlas-surface);
  }
  a:hover strong, a:hover span { text-decoration: underline; }
  img { display: block; width: 100%; aspect-ratio: 2; object-fit: cover; max-height: 12rem; }
  .summary { display: grid; gap: .25rem; padding: .75rem; overflow-wrap: anywhere; }
  small { color: var(--atlas-muted); }
  p { margin: 0; }
</style>

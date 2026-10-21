<script lang="ts">
  import { onMount } from 'svelte';
  import NodeForm, { type NodeDraft } from './NodeForm.svelte';
  import MarkdownContent from './MarkdownContent.svelte';
  import { readTypes, type NodeType } from '../api/nodePage';
  let { parentId, parentTitle, initialTypeId, apiBase = '/atlas-preview-api' }: { parentId?: string; parentTitle?: string; initialTypeId?: string; apiBase?: string } = $props();
  let types = $state<NodeType[]>([]); let error = $state(''); let draft = $state<NodeDraft | null>(null); let loading = $state(true);
  let controller: AbortController;
  async function load() { loading = true; error = ''; controller?.abort(); controller = new AbortController(); try { types = await readTypes(apiBase, AbortSignal.any([controller.signal, AbortSignal.timeout(10000)])); } catch { if (!controller.signal.aborted) error = 'Could not load node types.'; } finally { loading = false; } }
  onMount(() => { void load(); return () => controller?.abort(); });
</script>
{#if loading}<p role="status">Loading node types…</p>{:else if error}<p role="alert">{error}</p><button onclick={() => void load()}>Retry node types</button>{:else}
  <NodeForm {types} {parentId} {parentTitle} {initialTypeId} onPreview={value => draft = value} />
  {#if draft}<section class="atlas-panel draft" aria-label="Draft preview"><h2>{draft.title}</h2><p>{draft.shortDescription}</p><p>Type: {draft.newType?.name ?? types.find(type => type.id === draft?.typeId)?.name}</p><MarkdownContent text={draft.markdown} /><p role="status">Draft preview only. Nothing has been saved.</p></section>{/if}
{/if}
<style>.draft{max-width:48rem;margin:1rem auto;}</style>

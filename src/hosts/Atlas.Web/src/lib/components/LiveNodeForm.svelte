<script lang="ts">
  import { onMount } from 'svelte';
  import NodeForm from './NodeForm.svelte';
  import type { BreadcrumbItem } from './Breadcrumb.svelte';
  import { readTypes, type NodeType } from '../api/nodePage';
  let { parentId, parentTitle, initialTypeId, breadcrumb = [], focusOnMount = false, apiBase = '/atlas-preview-api' }: { parentId?: string; parentTitle?: string; initialTypeId?: string; breadcrumb?: BreadcrumbItem[]; focusOnMount?: boolean; apiBase?: string } = $props();
  let types = $state<NodeType[]>([]); let error = $state(''); let loading = $state(true);
  let controller: AbortController;
  async function load() { loading = true; error = ''; controller?.abort(); controller = new AbortController(); try { types = await readTypes(apiBase, AbortSignal.any([controller.signal, AbortSignal.timeout(10000)])); } catch { if (!controller.signal.aborted) error = 'Could not load node types.'; } finally { loading = false; } }
  onMount(() => { void load(); return () => controller?.abort(); });
</script>
{#if loading}<p role="status">Loading node types…</p>{:else if error}<p role="alert">{error}</p><button onclick={() => void load()}>Retry node types</button>{:else}
  <NodeForm {types} {parentId} {parentTitle} {initialTypeId} {breadcrumb} {focusOnMount} />
{/if}

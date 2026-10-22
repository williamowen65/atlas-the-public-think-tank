<script lang="ts">
  import NodePage from './NodePage.svelte';
  import LiveNodeFeed from './LiveNodeFeed.svelte';
  import LiveComments from './LiveComments.svelte';
  import LiveNodeForm from './LiveNodeForm.svelte';
  import { readDiscovery, discoveryCard, readReactions, readOlderAncestors } from '../api/discovery';
  import { readJson, readTypes, type NodeType, type ContentBlock, type NodeDetail } from '../api/nodePage';
  import type { NodeCardData } from './NodeCard.svelte';
  let { nodeId = '', apiBase = '/atlas-preview-api', onOpenNode }: { nodeId?: string; apiBase?: string; onOpenNode?: (id: string) => void } = $props();
  let node = $state<NodeCardData | null>(null); let blocks = $state<ContentBlock[]>([]); let types = $state<NodeType[]>([]);
  let active = $state('comments'); let loading = $state(false); let error = $state(''); let retry = $state(0); let createType = $state<string | null>(null);
  let hasMoreAncestors = $state(false);
  let average = $state<number | null>(null); let totalVotes = $state(0);
  const openNode = (id: string) => onOpenNode ? onOpenNode(id) : window.location.assign(`/nodes?id=${encodeURIComponent(id)}`);
  $effect(() => {
    const id = nodeId; retry;
    const controller = new AbortController();
    const signal = AbortSignal.any([controller.signal, AbortSignal.timeout(10000)]);
    loading = true; error = ''; node = null; createType = null; active = 'comments';
    void (async () => {
      try {
        const page = await readDiscovery(apiBase, 1, '', signal, id || undefined);
        const entry = page.items.find(item => discoveryCard(item));
        const display = entry && discoveryCard(entry);
        if (!display) throw new Error('Unavailable');
        const [detail, content, catalog, reactions] = await Promise.all([
          readJson<NodeDetail>(apiBase, `nodes/${encodeURIComponent(display.id)}`, signal),
          readJson<ContentBlock[]>(apiBase, `nodes/${encodeURIComponent(display.id)}/description`, signal), readTypes(apiBase, signal), readReactions(display.id, signal, apiBase).catch(() => []),
        ]);
        if (detail.hidden) throw new Error('Unavailable');
        if (controller.signal.aborted) return;
        node = { ...display, reactions }; hasMoreAncestors = !!entry!.card?.breadcrumbSegment?.continuationNodeId; blocks = content; average = entry!.averageVote; totalVotes = entry!.voteCount;
        types = [...catalog].sort((a, b) => Number(detail.requestedTypeIds.includes(b.id)) - Number(detail.requestedTypeIds.includes(a.id)));
      } catch { if (!controller.signal.aborted) error = 'This node is unavailable or could not be loaded.'; }
      finally { if (!controller.signal.aborted) loading = false; }
    })();
    return () => controller.abort();
  });
</script>
{#if loading}<p role="status">Loading node…</p>{:else if error}<p role="alert">{error}</p><button onclick={() => retry++}>Retry node</button>{:else if node}
  {#key node.id}
    <NodePage {hasMoreAncestors} loadOlderAncestors={id => id ? readOlderAncestors(id, AbortSignal.timeout(10000), apiBase) : Promise.resolve({ items: [], hasMore: false })} {node} {blocks} {types} {active} {average} {totalVotes} onNavigate={openNode} onTabChange={id => active = id} onAddChild={id => { createType = id ?? types[0]?.id ?? ''; if (createType) active = createType; }}>
      {#if createType !== null}<div class="child-form"><button onclick={() => createType = null}>Close child form</button>{#key createType}<LiveNodeForm parentId={node.id} parentTitle={node.title} initialTypeId={createType} breadcrumb={node.breadcrumb} focusOnMount {apiBase} />{/key}</div>{/if}
      {#key active}{#if active === 'comments'}<LiveComments nodeId={node.id} {apiBase} />{:else}<LiveNodeFeed parentId={node.id} typeId={active} {apiBase} onOpenNode={openNode} />{/if}{/key}
    </NodePage>
  {/key}
{/if}
<style>.child-form{max-width:48rem;margin:1rem auto;}.child-form>button{margin-bottom:1rem;}</style>

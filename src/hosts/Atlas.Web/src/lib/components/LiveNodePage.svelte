<script lang="ts">
  import LiveVotingDial from './LiveVotingDial.svelte';
  import LiveReactions from './LiveReactions.svelte';
  import NodePage from './NodePage.svelte';
  import LiveNodeFeed from './LiveNodeFeed.svelte';
  import CommentsPanel from './CommentsPanel.svelte';
  import LiveNodeForm from './LiveNodeForm.svelte';
  import type { NodeDraft } from './NodeForm.svelte';
  import { readDiscovery, discoveryCard, readReactions, readOlderAncestors } from '../api/discovery';
  import { readJson, readTypes, type NodeType, type ContentBlock, type NodeDetail } from '../api/nodePage';
  import type { NodeCardData } from './NodeCard.svelte';
  let { nodeId = '', initialTypeId = '', apiBase = '/atlas-public-api', onOpenNode, onOpenProfile }: { nodeId?: string; initialTypeId?: string; apiBase?: string; onOpenNode?: (id: string, typeId?: string) => void; onOpenProfile?: (id: string) => void } = $props();
  let node = $state<NodeCardData | null>(null); let blocks = $state<ContentBlock[]>([]); let types = $state<NodeType[]>([]);
  let active = $state('comments'); let loading = $state(false); let error = $state(''); let retry = $state(0); let createType = $state<string | null>(null);
  let hasMoreAncestors = $state(false);
  let editingDraft = $state<NodeDraft | null>(null);
  let saveChildDraft: (() => Promise<boolean>) | null = null;
  let switching = $state(false);
  const draftChanged = (draft: NodeDraft) => !!(draft.title || draft.shortDescription || draft.markdown || draft.newType?.name || draft.newType?.description || draft.requestedTypeIds.length);
  async function closeChildForm(): Promise<boolean> {
    if (createType === null) return true;
    if (editingDraft && draftChanged(editingDraft)) {
      if (editingDraft.title.trim()) {
        if (!saveChildDraft || !await saveChildDraft()) { window.alert('Your draft could not be saved. The form will remain open.'); return false; }
      } else if (!window.confirm('This child node has unsaved changes but no title. A title is required to save a private draft. Discard these changes and close the form?')) return false;
    }
    createType = null; editingDraft = null; saveChildDraft = null;
    return true;
  }
  async function changeTab(id: string) {
    if (switching) return;
    switching = true;
    try { if (await closeChildForm()) active = id; } finally { switching = false; }
  }
  function selectDraftType(draft: NodeDraft | null) {
    editingDraft = draft;
    if (createType === null || !draft) return;
    const selected = draft.newType ? 'draft-new-type' : draft.typeId;
    if (selected) active = selected;
  }
  let average = $state<number | null>(null); let totalVotes = $state(0);
  const openNode = (id: string, typeId?: string) => onOpenNode ? onOpenNode(id, typeId) : window.location.assign(`/nodes?id=${encodeURIComponent(id)}${typeId ? `&type=${encodeURIComponent(typeId)}#contributions` : ''}`);
  $effect(() => {
    const id = nodeId; const selectedType = initialTypeId; retry;
    const controller = new AbortController();
    const signal = AbortSignal.any([controller.signal, AbortSignal.timeout(10000)]);
    loading = true; error = ''; node = null; createType = null; editingDraft = null; saveChildDraft = null; active = 'comments';
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
        node = { ...display, authorId: detail.authorId, reactions }; hasMoreAncestors = !!entry!.card?.breadcrumbSegment?.continuationNodeId; blocks = content; average = entry!.averageVote; totalVotes = entry!.voteCount;
        active = display.childTypes?.some(type => type.typeId === selectedType) ? selectedType : 'comments';
        types = [...catalog].sort((a, b) => Number(detail.requestedTypeIds.includes(b.id)) - Number(detail.requestedTypeIds.includes(a.id)));
      } catch { if (!controller.signal.aborted) error = 'This node is unavailable or could not be loaded.'; }
      finally { if (!controller.signal.aborted) loading = false; }
    })();
    return () => controller.abort();
  });
</script>
{#if loading}<p role="status">Loading node…</p>{:else if error}<p role="alert">{error}</p><button onclick={() => retry++}>Retry node</button>{:else if node}
  {#key node.id}
    <NodePage {apiBase} {onOpenProfile} revealChildren={!!initialTypeId && active === initialTypeId} {hasMoreAncestors} loadOlderAncestors={id => id ? readOlderAncestors(id, AbortSignal.timeout(10000), apiBase) : Promise.resolve({ items: [], hasMore: false })} {node} {blocks} {types} {active} draftTab={createType !== null && editingDraft ? { id: editingDraft.newType ? 'draft-new-type' : editingDraft.typeId ?? createType, name: editingDraft.newType ? (editingDraft.newType.name || 'New node type') + ' (draft)' : (types.find(type => type.id === editingDraft?.typeId)?.name ?? 'Child') + ' (draft)' } : undefined} {average} {totalVotes} onNavigate={openNode} onTabChange={id => void changeTab(id)} onAddChild={id => { if (createType !== null) return; createType = id ?? types[0]?.id ?? ''; active = createType; }}>
      {#snippet voting()}{#if node}<LiveVotingDial nodeId={node.id} initialAverage={average} initialTotalVotes={totalVotes} archived={node.status === 'Archived'} />{/if}{/snippet}
      {#snippet reactions()}{#if node}<LiveReactions nodeId={node.id} archived={node.status === 'Archived'} />{/if}{/snippet}
      {#if createType !== null}<div class="child-form"><button onclick={() => void closeChildForm()}>Close child form</button>{#key createType}<LiveNodeForm parentId={node.id} parentTitle={node.title} initialTypeId={createType} onCreated={openNode} onDraftChange={selectDraftType} onRegisterClose={close => saveChildDraft = close} breadcrumb={node.breadcrumb} focusOnMount {apiBase} />{/key}</div>{/if}
      {#key active}{#if active === 'comments'}<CommentsPanel nodeId={node.id} {apiBase} onAdded={() => { if (node) node = { ...node, commentCount: node.commentCount + 1 }; }} />{:else if active === 'draft-new-type' || (createType !== null && active === editingDraft?.typeId && !node.childTypes?.some(type => type.typeId === active))}<p>The child node draft is open above.</p>{:else}<LiveNodeFeed {onOpenProfile} parentId={node.id} typeId={active} {apiBase} onOpenNode={openNode} emptyTypeName={types.find(type => type.id === active)?.name ?? node.childTypes?.find(type => type.typeId === active)?.label.replace(/^\s*\d+\s+/, '').replace(/\s*\(\d+\)\s*$/, '').replace(/ies$/i, 'y').replace(/s$/i, '')} onCreateFirst={() => { if (createType === null) { createType = active; } }} />{/if}{/key}
    </NodePage>
  {/key}
{/if}
<style>.child-form{max-width:48rem;margin:1rem auto;}.child-form>button{margin-bottom:1rem;}</style>
